using System;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// Per-node HMAC-SHA256 request authentication for <c>/api/sync/*</c>
    /// (docs/SYNC_PROTOCOL.md §4). Signs / verifies:
    /// <code>UPPER(method) \n path \n timestamp \n nonce \n base64(SHA-256(body))</code>
    /// </summary>
    public static class SyncHmac
    {
        public const string HNode = "X-Sync-Node";
        public const string HTimestamp = "X-Sync-Timestamp";
        public const string HNonce = "X-Sync-Nonce";
        public const string HBodyHash = "X-Sync-BodyHash";
        public const string HSignature = "X-Sync-Signature";

        public static string BodyHash(ReadOnlySpan<byte> body)
            => Convert.ToBase64String(SHA256.HashData(body));

        public static string SigningString(string method, string path, string timestamp, string nonce, string bodyHash)
            => $"{method.ToUpperInvariant()}\n{path}\n{timestamp}\n{nonce}\n{bodyHash}";

        public static string Sign(string secret, string signingString)
        {
            using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            return Convert.ToBase64String(h.ComputeHash(Encoding.UTF8.GetBytes(signingString)));
        }

        public static bool FixedTimeEquals(string a, string b)
        {
            var ba = Encoding.UTF8.GetBytes(a ?? "");
            var bb = Encoding.UTF8.GetBytes(b ?? "");
            return ba.Length == bb.Length && CryptographicOperations.FixedTimeEquals(ba, bb);
        }

        public static string NewNonce()
        {
            Span<byte> b = stackalloc byte[16];
            RandomNumberGenerator.Fill(b);
            return Convert.ToBase64String(b).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}
