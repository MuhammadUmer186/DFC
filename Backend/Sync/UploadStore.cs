using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestaurantSystem.Data;
using RestaurantSystem.Models;

namespace RestaurantSystem.Sync
{
    public sealed class UploadOptions
    {
        public const string SectionName = "Uploads";
        public long MaxBytes { get; set; } = 15 * 1024 * 1024; // 15 MB (matches nginx client_max_body_size)
        public string[] AllowedExtensions { get; set; } =
            { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".pdf" };
        public Dictionary<string, string> ContentTypeByExt { get; } = new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".png"] = "image/png",
            [".webp"] = "image/webp", [".gif"] = "image/gif", [".pdf"] = "application/pdf"
        };
    }

    public sealed record StoredUpload(string Url, string StorageKey, string Sha256Hash, bool Deduplicated);

    /// <summary>
    /// Validates, de-duplicates (by SHA-256) and records uploads, and serves the
    /// bytes for cross-node fetch. Phase 12.
    /// </summary>
    public sealed class UploadStore
    {
        private readonly ApplicationDbContext _db;
        private readonly INodeContext _node;
        private readonly UploadOptions _opts;
        private readonly string _uploadsRoot;
        private readonly ILogger<UploadStore> _log;

        public UploadStore(ApplicationDbContext db, INodeContext node,
            Microsoft.Extensions.Options.IOptions<UploadOptions> opts, IHostEnvironment env, ILogger<UploadStore> log)
        {
            _db = db;
            _node = node;
            _opts = opts.Value;
            _uploadsRoot = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
            _log = log;
        }

        public async Task<StoredUpload> SaveAsync(IFormFile file, string category, string? namePrefix = null, CancellationToken ct = default)
        {
            if (file is null || file.Length == 0) throw new ArgumentException("No file uploaded");
            if (file.Length > _opts.MaxBytes) throw new ArgumentException($"File exceeds {_opts.MaxBytes} bytes");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_opts.AllowedExtensions.Contains(ext))
                throw new ArgumentException($"Unsupported file type '{ext}'.");

            category = SanitizeSegment(category, "misc");

            // hash the bytes
            await using var src = file.OpenReadStream();
            using var sha = SHA256.Create();
            var hashBytes = await sha.ComputeHashAsync(src, ct);
            var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
            src.Position = 0;

            // dedupe
            var existing = await _db.Set<UploadedFile>()
                .FirstOrDefaultAsync(f => f.Sha256Hash == hash && f.DeletedAtUtc == null, ct);
            if (existing is not null && File.Exists(Path.Combine(_uploadsRoot, existing.StorageKey.Replace('/', Path.DirectorySeparatorChar))))
                return new StoredUpload($"/uploads/{existing.StorageKey}", existing.StorageKey, hash, Deduplicated: true);

            var folder = Path.Combine(_uploadsRoot, category);
            Directory.CreateDirectory(folder);
            var fileName = $"{SanitizeSegment(namePrefix ?? "file", "file")}_{Guid.NewGuid():N}{ext}";
            var storageKey = $"{category}/{fileName}";
            var fullPath = Path.Combine(folder, fileName);

            await using (var dst = new FileStream(fullPath, FileMode.CreateNew))
                await src.CopyToAsync(dst, ct);

            var contentType = _opts.ContentTypeByExt.TryGetValue(ext, out var ctpe) ? ctpe : "application/octet-stream";

            if (existing is null)
            {
                _db.Set<UploadedFile>().Add(new UploadedFile
                {
                    StorageKey = storageKey,
                    OriginalFileName = Path.GetFileName(file.FileName),
                    ContentType = contentType,
                    Size = file.Length,
                    Sha256Hash = hash,
                    Category = category,
                    SyncState = "available"
                });
                await _db.SaveChangesAsync(ct);
            }

            return new StoredUpload($"/uploads/{storageKey}", storageKey, hash, Deduplicated: false);
        }

        public async Task<(byte[] bytes, string contentType, string fileName)?> ReadByHashAsync(string hash, CancellationToken ct = default)
        {
            var meta = await _db.Set<UploadedFile>().AsNoTracking()
                .FirstOrDefaultAsync(f => f.Sha256Hash == hash.ToLowerInvariant(), ct);
            if (meta is null) return null;
            var path = Path.Combine(_uploadsRoot, meta.StorageKey.Replace('/', Path.DirectorySeparatorChar));
            if (!path.StartsWith(_uploadsRoot, StringComparison.Ordinal) || !File.Exists(path)) return null;
            var bytes = await File.ReadAllBytesAsync(path, ct);
            return (bytes, meta.ContentType, Path.GetFileName(meta.StorageKey));
        }

        /// <summary>Write bytes fetched from a peer to the local uploads folder for a known metadata row.</summary>
        public async Task<bool> WriteFetchedAsync(string hash, byte[] bytes, CancellationToken ct = default)
        {
            var actual = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            if (!string.Equals(actual, hash.ToLowerInvariant(), StringComparison.Ordinal)) return false;

            var meta = await _db.Set<UploadedFile>().FirstOrDefaultAsync(f => f.Sha256Hash == actual, ct);
            if (meta is null) return false;

            var path = Path.Combine(_uploadsRoot, meta.StorageKey.Replace('/', Path.DirectorySeparatorChar));
            if (!path.StartsWith(_uploadsRoot, StringComparison.Ordinal)) return false;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, bytes, ct);
            meta.SyncState = "available";
            using (SyncStampingInterceptor.Suppress())
                await _db.SaveChangesAsync(ct);
            return true;
        }

        private static string SanitizeSegment(string? s, string fallback)
        {
            if (string.IsNullOrWhiteSpace(s)) return fallback;
            var clean = new string(s.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_').ToArray());
            return string.IsNullOrEmpty(clean) ? fallback : clean.ToLowerInvariant();
        }
    }
}
