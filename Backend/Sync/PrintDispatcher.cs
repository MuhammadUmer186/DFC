using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantSystem.Data;
using RestaurantSystem.DTOs;
using RestaurantSystem.Models;
using PrintSvc = Printing.Services.PrintService;
using PrinterKind = Printing.Services.PrinterType;

namespace RestaurantSystem.Sync
{
    public sealed record PrintRequest(
        string JobType, string Copy, PrintReceiptDto Dto, PrinterKind Printer,
        int? OrderId = null, Guid? OrderGlobalId = null,
        bool IsReprint = false, string? ReprintReason = null, string? RequestedByUserName = null);

    public sealed record PrintOutcome(Guid PrintJobId, string Status, bool Printed, string? Error);

    /// <summary>
    /// Phase 13. Wraps the ESC/POS <see cref="PrintSvc"/> so every job gets a
    /// <c>PrintJobId</c> + a persisted status, a delivery slip is never printed
    /// twice for the same <c>(OrderGlobalId, Copy)</c> (unless it's an authorized
    /// reprint), and a printer-offline condition is a job status — never an
    /// exception that fails the order transaction.
    /// </summary>
    public interface IPrintDispatcher
    {
        Task<PrintOutcome> DispatchAsync(PrintRequest req, CancellationToken ct = default);
    }

    public sealed class LocalPrintDispatcher : IPrintDispatcher
    {
        private readonly ApplicationDbContext _db;
        private readonly PrintSvc _print;
        private readonly ILogger<LocalPrintDispatcher> _log;

        public LocalPrintDispatcher(ApplicationDbContext db, PrintSvc print, ILogger<LocalPrintDispatcher> log)
        {
            _db = db;
            _print = print;
            _log = log;
        }

        public async Task<PrintOutcome> DispatchAsync(PrintRequest req, CancellationToken ct = default)
        {
            // Dedupe: a completed non-reprint slip for this order+copy already exists?
            if (!req.IsReprint && req.OrderGlobalId is { } gid && gid != Guid.Empty)
            {
                var already = await _db.Set<PrintJob>().AnyAsync(j =>
                    j.OrderGlobalId == gid && j.JobType == req.JobType && j.Copy == req.Copy &&
                    j.Status == "printed" && !j.IsReprint, ct);
                if (already)
                {
                    _log.LogInformation("Print: {Type}/{Copy} for order {Gid} already printed — skipped.", req.JobType, req.Copy, gid);
                    return new PrintOutcome(Guid.Empty, "skipped", false, null);
                }
            }

            var job = new PrintJob
            {
                PrintJobId = Guid.NewGuid(),
                JobType = req.JobType,
                Copy = req.Copy,
                OrderId = req.OrderId,
                OrderGlobalId = req.OrderGlobalId,
                PayloadJson = SafeSerialize(req.Dto),
                Status = "queued",
                Attempts = 0,
                IsReprint = req.IsReprint,
                ReprintReason = req.ReprintReason,
                RequestedByUserName = req.RequestedByUserName,
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.Set<PrintJob>().Add(job);
            using (SyncStampingInterceptor.Suppress())
                await _db.SaveChangesAsync(ct);

            var (ok, err) = TryPrint(req.Dto, req.Printer);
            job.Attempts++;
            job.Status = ok ? "printed" : "failed";
            job.Error = err;
            job.CompletedAtUtc = ok ? DateTime.UtcNow : null;
            using (SyncStampingInterceptor.Suppress())
                await _db.SaveChangesAsync(ct);

            if (!ok) _log.LogWarning("Print: {Type}/{Copy} FAILED (job {Id}): {Err}", req.JobType, req.Copy, job.PrintJobId, err);
            return new PrintOutcome(job.PrintJobId, job.Status, ok, err);
        }

        private (bool ok, string? err) TryPrint(PrintReceiptDto dto, PrinterKind printer)
        {
            try { return (_print.PrintReceipt(dto, printer), null); }
            catch (Exception ex) { return (false, ex.Message); }
        }

        private static string SafeSerialize(object o)
        {
            try { return JsonSerializer.Serialize(o); } catch { return "{}"; }
        }
    }
}
