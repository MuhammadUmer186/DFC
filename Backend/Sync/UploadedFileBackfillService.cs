using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestaurantSystem.Data;
using RestaurantSystem.Models;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// Phase 12 backfill. Registers an <see cref="UploadedFile"/> row (with
    /// SHA-256) for every file already under <c>wwwroot/uploads</c> that has no
    /// metadata yet, so existing images/PDFs participate in sync. Idempotent;
    /// bounded per run so a huge folder doesn't stall startup.
    /// </summary>
    public sealed class UploadedFileBackfillService
    {
        private const int MaxPerRun = 500;

        private readonly ApplicationDbContext _db;
        private readonly string _root;
        private readonly ILogger<UploadedFileBackfillService> _log;

        public UploadedFileBackfillService(ApplicationDbContext db, IHostEnvironment env, ILogger<UploadedFileBackfillService> log)
        {
            _db = db;
            _root = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
            _log = log;
        }

        public async Task RunAsync(CancellationToken ct = default)
        {
            if (!Directory.Exists(_root)) return;

            var known = (await _db.Set<UploadedFile>().Select(f => f.StorageKey).ToListAsync(ct)).ToHashSet();
            var added = 0;

            foreach (var path in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories))
            {
                if (ct.IsCancellationRequested || added >= MaxPerRun) break;
                var rel = Path.GetRelativePath(_root, path).Replace(Path.DirectorySeparatorChar, '/');
                if (known.Contains(rel)) continue;

                try
                {
                    var bytes = await File.ReadAllBytesAsync(path, ct);
                    var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                    var ext = Path.GetExtension(path).ToLowerInvariant();
                    _db.Set<UploadedFile>().Add(new UploadedFile
                    {
                        StorageKey = rel,
                        OriginalFileName = Path.GetFileName(path),
                        ContentType = ext switch
                        {
                            ".jpg" or ".jpeg" => "image/jpeg", ".png" => "image/png",
                            ".webp" => "image/webp", ".gif" => "image/gif", ".pdf" => "application/pdf",
                            _ => "application/octet-stream"
                        },
                        Size = bytes.LongLength,
                        Sha256Hash = hash,
                        Category = rel.Contains('/') ? rel[..rel.IndexOf('/')] : "misc",
                        SyncState = "available"
                    });
                    added++;
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Sync/Phase12: could not index upload {Path}", rel);
                }
            }

            if (added > 0)
            {
                using (SyncStampingInterceptor.Suppress())
                    await _db.SaveChangesAsync(ct);
                _log.LogWarning("Sync/Phase12: indexed {Count} existing upload(s) into UploadedFiles.", added);
            }
        }
    }
}
