using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// Phase 8. Supplies JWT signing material. If an RS256 private key is
    /// configured (<c>Auth:PrivateKeyPath</c>, PEM) this node <b>signs</b> with
    /// RS256 under its own issuer (<c>Auth:Issuer</c>); otherwise it falls back to
    /// the legacy HS256 shared secret (fully backward compatible).
    /// For <b>validation</b> every API trusts: the HS256 secret (legacy) +
    /// every configured trusted public key (<c>Auth:TrustedPublicKeys:*</c>) so a
    /// token minted by either the cloud or the edge is accepted on both.
    /// </summary>
    public sealed class AuthKeyProvider
    {
        public bool UseRs256 { get; }
        public string Issuer { get; }
        public string[] ValidIssuers { get; }
        public SigningCredentials SigningCredentials { get; }
        public IReadOnlyList<SecurityKey> ValidationKeys { get; }

        private readonly SymmetricSecurityKey _hs256;

        public AuthKeyProvider(IConfiguration cfg, ILogger<AuthKeyProvider> log)
        {
            var legacySecret = cfg["AppSettings:Token"] ?? "";
            _hs256 = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(legacySecret));

            var legacyIssuer = cfg["AppSettings:Issuer"] ?? "DFC-RMS";
            Issuer = cfg["Auth:Issuer"] ?? legacyIssuer;

            var keys = new List<SecurityKey>();
            var issuers = new HashSet<string>(StringComparer.Ordinal) { legacyIssuer, Issuer };

            // this node's private key -> RS256 signing
            var pkPath = cfg["Auth:PrivateKeyPath"];
            if (!string.IsNullOrWhiteSpace(pkPath) && File.Exists(pkPath))
            {
                var rsa = RSA.Create();
                rsa.ImportFromPem(File.ReadAllText(pkPath));
                var rsaKey = new RsaSecurityKey(rsa) { KeyId = "this-node" };
                SigningCredentials = new SigningCredentials(rsaKey, SecurityAlgorithms.RsaSha256);
                keys.Add(rsaKey);           // trust our own tokens
                UseRs256 = true;
                log.LogInformation("Auth(Phase8): RS256 signing enabled, issuer '{Issuer}'.", Issuer);
            }
            else
            {
                SigningCredentials = new SigningCredentials(_hs256, SecurityAlgorithms.HmacSha256);
                UseRs256 = false;
                log.LogInformation("Auth(Phase8): HS256 signing (no Auth:PrivateKeyPath) — legacy compatible.");
            }

            // always trust the legacy HS256 secret during migration
            keys.Add(_hs256);

            // trusted peers' public keys
            foreach (var kv in cfg.GetSection("Auth:TrustedPublicKeys").GetChildren())
            {
                var path = kv.Value;
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) continue;
                try
                {
                    var rsa = RSA.Create();
                    rsa.ImportFromPem(File.ReadAllText(path));
                    keys.Add(new RsaSecurityKey(rsa) { KeyId = kv.Key });
                    if (!string.IsNullOrWhiteSpace(kv.Key)) issuers.Add($"DFC-RMS-{kv.Key}");
                    log.LogInformation("Auth(Phase8): trusting public key '{Name}'.", kv.Key);
                }
                catch (Exception ex)
                {
                    log.LogWarning(ex, "Auth(Phase8): failed to load trusted public key '{Name}' ({Path}).", kv.Key, path);
                }
            }

            // explicit extra issuers
            foreach (var i in (cfg["Auth:ValidIssuers"] ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                issuers.Add(i);

            ValidationKeys = keys;
            ValidIssuers = issuers.ToArray();
        }
    }
}
