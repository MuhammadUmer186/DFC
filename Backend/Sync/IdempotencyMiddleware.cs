using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantSystem.Data;
using RestaurantSystem.Models;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// Phase 6. For any mutating request (POST/PUT/PATCH/DELETE) carrying an
    /// <c>Idempotency-Key</c> header:
    /// <list type="bullet">
    /// <item>first time — run the pipeline, store status + (capped) body keyed by the UUID;</item>
    /// <item>retry, same body hash — replay the stored response, don't touch the DB;</item>
    /// <item>retry, different body hash — <c>409 idempotency-key-reuse</c>;</item>
    /// <item>still in-progress — <c>409 request-in-progress</c> + <c>Retry-After</c>.</item>
    /// </list>
    /// Requests without the header are completely unaffected (backward compatible).
    /// </summary>
    public sealed class IdempotencyMiddleware
    {
        public const string HeaderName = "Idempotency-Key";

        private readonly RequestDelegate _next;
        private readonly IdempotencyOptions _opts;
        private readonly ILogger<IdempotencyMiddleware> _log;

        public IdempotencyMiddleware(RequestDelegate next, IdempotencyOptions opts, ILogger<IdempotencyMiddleware> log)
        {
            _next = next;
            _opts = opts;
            _log = log;
        }

        public async Task Invoke(HttpContext ctx, ApplicationDbContext db, ICommandContext commandCtx, INodeContext node)
        {
            if (!_opts.Enabled || !IsMutating(ctx.Request.Method) ||
                !ctx.Request.Headers.TryGetValue(HeaderName, out var raw) || string.IsNullOrWhiteSpace(raw))
            {
                await _next(ctx);
                return;
            }

            if (!Guid.TryParse(raw.ToString().Trim(), out var commandId) || commandId == Guid.Empty)
            {
                await WriteProblem(ctx, StatusCodes.Status400BadRequest, "invalid-idempotency-key",
                    "The Idempotency-Key header must be a non-empty UUID.");
                return;
            }

            commandCtx.Set(commandId);

            var route = $"{ctx.Request.Method} {ctx.Request.Path}";
            var requestHash = await HashRequestAsync(ctx.Request);

            // 1) Fast path — already recorded?
            var existing = await db.Set<ProcessedCommand>().AsNoTracking()
                .FirstOrDefaultAsync(p => p.CommandId == commandId);

            if (existing is not null)
            {
                if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
                {
                    await WriteProblem(ctx, StatusCodes.Status409Conflict, "idempotency-key-reuse",
                        "This Idempotency-Key was already used for a different request.");
                    return;
                }
                if (existing.State == "in-progress")
                {
                    ctx.Response.Headers["Retry-After"] = _opts.InProgressRetryAfterSeconds.ToString();
                    await WriteProblem(ctx, StatusCodes.Status409Conflict, "request-in-progress",
                        "A request with this Idempotency-Key is still being processed.");
                    return;
                }
                await ReplayAsync(ctx, existing);
                return;
            }

            // 2) Claim the key. Unique index on CommandId turns a race into a DbUpdateException.
            var record = new ProcessedCommand
            {
                CommandId = commandId,
                NodeId = node.NodeId,
                Route = route.Length > 400 ? route[..400] : route,
                RequestHash = requestHash,
                State = "in-progress",
                StartedAtUtc = DateTime.UtcNow
            };
            try
            {
                db.Set<ProcessedCommand>().Add(record);
                using (SyncStampingInterceptor.Suppress())
                    await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                db.Entry(record).State = EntityState.Detached;
                ctx.Response.Headers["Retry-After"] = _opts.InProgressRetryAfterSeconds.ToString();
                await WriteProblem(ctx, StatusCodes.Status409Conflict, "request-in-progress",
                    "A concurrent request with this Idempotency-Key is being processed.");
                return;
            }

            // 3) Run the pipeline, capturing the response.
            var originalBody = ctx.Response.Body;
            await using var buffer = new MemoryStream();
            ctx.Response.Body = buffer;

            try
            {
                await _next(ctx);
            }
            catch
            {
                ctx.Response.Body = originalBody;
                await MarkFailedAsync(db, commandId);
                throw;
            }

            ctx.Response.Body = originalBody;
            buffer.Position = 0;
            var bodyBytes = buffer.ToArray();

            var status = ctx.Response.StatusCode;
            var isServerError = status >= 500;

            if (isServerError)
            {
                // Let a genuine retry re-attempt: drop the claim.
                await MarkFailedAsync(db, commandId);
            }
            else
            {
                await CompleteAsync(db, commandId, status, ctx.Response.ContentType, bodyBytes);
            }

            await originalBody.WriteAsync(bodyBytes);
        }

        private static bool IsMutating(string method) =>
            HttpMethods.IsPost(method) || HttpMethods.IsPut(method) ||
            HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);

        private static async Task<string> HashRequestAsync(HttpRequest request)
        {
            request.EnableBuffering();
            string body = "";
            if (request.ContentLength is > 0 || request.Body.CanRead)
            {
                request.Body.Position = 0;
                using var reader = new StreamReader(request.Body, Encoding.UTF8, false, 4096, leaveOpen: true);
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }
            var payload = $"{request.Method}\n{request.Path}{request.QueryString}\n{body}";
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
        }

        private async Task ReplayAsync(HttpContext ctx, ProcessedCommand rec)
        {
            ctx.Response.StatusCode = rec.StatusCode ?? StatusCodes.Status200OK;
            ctx.Response.Headers["Idempotency-Replayed"] = "true";
            if (!string.IsNullOrEmpty(rec.ResponseContentType))
                ctx.Response.ContentType = rec.ResponseContentType;
            if (!string.IsNullOrEmpty(rec.ResponseBody))
                await ctx.Response.WriteAsync(rec.ResponseBody);
            _log.LogInformation("Idempotency: replayed {CommandId} ({Status}).", rec.CommandId, rec.StatusCode);
        }

        private async Task CompleteAsync(ApplicationDbContext db, Guid commandId, int status, string? contentType, byte[] body)
        {
            var rec = await db.Set<ProcessedCommand>().FirstOrDefaultAsync(p => p.CommandId == commandId);
            if (rec is null) return;

            var truncated = body.Length > _opts.MaxStoredBodyBytes;
            var stored = truncated ? body[.._opts.MaxStoredBodyBytes] : body;

            rec.State = "completed";
            rec.StatusCode = status;
            rec.ResponseContentType = contentType;
            rec.ResponseBody = truncated ? null : Encoding.UTF8.GetString(stored);
            rec.ResponseTruncated = truncated;
            rec.ResultGlobalId = TryExtractGlobalId(body);
            rec.CompletedAtUtc = DateTime.UtcNow;

            using (SyncStampingInterceptor.Suppress())
                await db.SaveChangesAsync();
        }

        private static async Task MarkFailedAsync(ApplicationDbContext db, Guid commandId)
        {
            var rec = await db.Set<ProcessedCommand>().FirstOrDefaultAsync(p => p.CommandId == commandId);
            if (rec is null) return;
            rec.State = "failed";
            rec.CompletedAtUtc = DateTime.UtcNow;
            using (SyncStampingInterceptor.Suppress())
                await db.SaveChangesAsync();
        }

        private static Guid? TryExtractGlobalId(byte[] body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;
                foreach (var name in new[] { "globalId", "GlobalId", "globalID" })
                    if (doc.RootElement.TryGetProperty(name, out var v) &&
                        v.ValueKind == JsonValueKind.String &&
                        Guid.TryParse(v.GetString(), out var g))
                        return g;
            }
            catch { /* not JSON / no field */ }
            return null;
        }

        private static Task WriteProblem(HttpContext ctx, int status, string code, string detail)
        {
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = code, detail }));
        }
    }
}
