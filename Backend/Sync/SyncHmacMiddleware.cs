using System;
using System.IO;
using System.Linq;
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
    /// Gate for <c>/api/sync/*</c>. Every request must carry a valid per-node
    /// HMAC-SHA256 signature (Phase 5). Rejects unknown nodes, bad signatures,
    /// reused nonces and stale timestamps with <c>401</c>. This IS the
    /// authentication for the sync channel — the endpoints are otherwise
    /// unauthenticated to users but never anonymous.
    /// </summary>
    public sealed class SyncHmacMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SyncOptions _opts;
        private readonly ILogger<SyncHmacMiddleware> _log;

        public SyncHmacMiddleware(RequestDelegate next, SyncOptions opts, ILogger<SyncHmacMiddleware> log)
        {
            _next = next;
            _opts = opts;
            _log = log;
        }

        public async Task Invoke(HttpContext ctx, ApplicationDbContext db, INodeContext node)
        {
            if (!ctx.Request.Path.StartsWithSegments("/api/sync", StringComparison.OrdinalIgnoreCase))
            {
                await _next(ctx);
                return;
            }

            if (string.IsNullOrWhiteSpace(_opts.HmacSecret))
            {
                await Deny(ctx, "sync-not-configured", "Sync HMAC secret is not configured on this node.");
                return;
            }

            var h = ctx.Request.Headers;
            var nodeIdRaw = h[SyncHmac.HNode].ToString();
            var timestamp = h[SyncHmac.HTimestamp].ToString();
            var nonce = h[SyncHmac.HNonce].ToString();
            var bodyHashHdr = h[SyncHmac.HBodyHash].ToString();
            var signature = h[SyncHmac.HSignature].ToString();

            if (string.IsNullOrEmpty(nodeIdRaw) || string.IsNullOrEmpty(timestamp) ||
                string.IsNullOrEmpty(nonce) || string.IsNullOrEmpty(signature) ||
                !Guid.TryParse(nodeIdRaw, out var callerNodeId))
            {
                await Deny(ctx, "missing-signature", "Sync request is missing required X-Sync-* headers.");
                return;
            }

            // known node?
            var known = await db.SystemNodes.AsNoTracking().AnyAsync(n => n.NodeId == callerNodeId && n.IsActive);
            if (!known && callerNodeId != node.NodeId)
            {
                await Deny(ctx, "unknown-node", "Calling node is not registered.");
                return;
            }

            // timestamp window
            if (!DateTimeOffset.TryParse(timestamp, null, System.Globalization.DateTimeStyles.RoundtripKind, out var ts) ||
                Math.Abs((DateTimeOffset.UtcNow - ts).TotalMinutes) > _opts.ClockSkewMinutes)
            {
                await Deny(ctx, "stale-timestamp", "X-Sync-Timestamp is outside the allowed window.");
                return;
            }

            // body hash
            ctx.Request.EnableBuffering();
            byte[] body;
            using (var ms = new MemoryStream())
            {
                await ctx.Request.Body.CopyToAsync(ms);
                body = ms.ToArray();
                ctx.Request.Body.Position = 0;
            }
            var computedBodyHash = SyncHmac.BodyHash(body);
            if (!string.IsNullOrEmpty(bodyHashHdr) && !SyncHmac.FixedTimeEquals(bodyHashHdr, computedBodyHash))
            {
                await Deny(ctx, "body-hash-mismatch", "X-Sync-BodyHash does not match the request body.");
                return;
            }

            var pathAndQuery = ctx.Request.Path + ctx.Request.QueryString;
            var signing = SyncHmac.SigningString(ctx.Request.Method, pathAndQuery, timestamp, nonce, computedBodyHash);
            var expected = SyncHmac.Sign(_opts.HmacSecret, signing);
            if (!SyncHmac.FixedTimeEquals(expected, signature))
            {
                await Deny(ctx, "bad-signature", "Sync request signature is invalid.");
                return;
            }

            // replay: nonce must be unseen within the window
            var cutoff = DateTime.UtcNow.AddMinutes(-_opts.NonceWindowMinutes);
            var reused = await db.SyncNonces.AsNoTracking()
                .AnyAsync(n => n.NodeId == callerNodeId && n.Nonce == nonce && n.SeenAtUtc >= cutoff);
            if (reused)
            {
                await Deny(ctx, "nonce-reused", "This nonce has already been used.");
                return;
            }
            db.SyncNonces.Add(new SyncNonce { NodeId = callerNodeId, Nonce = nonce, SeenAtUtc = DateTime.UtcNow });
            // opportunistic prune
            var stale = await db.SyncNonces.Where(n => n.SeenAtUtc < cutoff.AddMinutes(-_opts.NonceWindowMinutes)).Take(200).ToListAsync();
            if (stale.Count > 0) db.SyncNonces.RemoveRange(stale);
            using (SyncStampingInterceptor.Suppress())
                await db.SaveChangesAsync();

            ctx.Items["SyncCallerNodeId"] = callerNodeId;
            await _next(ctx);
        }

        private static Task Deny(HttpContext ctx, string code, string detail)
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = code, detail }));
        }
    }
}
