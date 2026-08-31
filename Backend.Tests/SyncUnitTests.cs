using System;
using System.Text;
using RestaurantSystem.Sync;
using Xunit;

namespace Backend.Tests;

/// <summary>
/// DB-free unit tests for the offline-first primitives. The DB-bound scenarios
/// (exactly-once sync, order-number sequences, ledger merge, stale-version
/// rejection, migration-from-prod, restore) require a real SQL Server and are
/// listed in TESTS.md as integration/manual — several are already exercised
/// end-to-end in IMPLEMENTATION_STATUS.md's per-phase "Verified" notes.
/// </summary>
public class SyncUnitTests
{
    // ---- Phase 6: deterministic idempotency-key -> GlobalId --------------------

    [Fact]
    public void DeriveGlobalId_is_deterministic_for_same_key_and_type()
    {
        var key = Guid.NewGuid();
        var a = CommandContext.DeriveGlobalId(key, "Order");
        var b = CommandContext.DeriveGlobalId(key, "Order");
        Assert.Equal(a, b);
        Assert.NotEqual(Guid.Empty, a);
    }

    [Fact]
    public void DeriveGlobalId_differs_by_key_and_by_type()
    {
        var k1 = Guid.NewGuid();
        var k2 = Guid.NewGuid();
        Assert.NotEqual(CommandContext.DeriveGlobalId(k1, "Order"), CommandContext.DeriveGlobalId(k2, "Order"));
        Assert.NotEqual(CommandContext.DeriveGlobalId(k1, "Order"), CommandContext.DeriveGlobalId(k1, "Payment"));
    }

    // ---- Phase 5: HMAC request signing --------------------------------------

    [Fact]
    public void Hmac_sign_then_verify_roundtrips()
    {
        const string secret = "unit-test-secret";
        var body = Encoding.UTF8.GetBytes("""{"batchId":"x","events":[]}""");
        var bodyHash = SyncHmac.BodyHash(body);
        var signing = SyncHmac.SigningString("POST", "/api/sync/push?x=1", "2026-08-31T00:00:00Z", "nonce123", bodyHash);
        var sig = SyncHmac.Sign(secret, signing);

        var recomputed = SyncHmac.Sign(secret, signing);
        Assert.True(SyncHmac.FixedTimeEquals(sig, recomputed));
    }

    [Fact]
    public void Hmac_rejects_tampered_body_or_path_or_secret()
    {
        const string secret = "unit-test-secret";
        var bodyHash = SyncHmac.BodyHash(Encoding.UTF8.GetBytes("original"));
        var signing = SyncHmac.SigningString("POST", "/api/sync/push", "2026-08-31T00:00:00Z", "n1", bodyHash);
        var sig = SyncHmac.Sign(secret, signing);

        var tamperedBody = SyncHmac.Sign(secret,
            SyncHmac.SigningString("POST", "/api/sync/push", "2026-08-31T00:00:00Z", "n1",
                SyncHmac.BodyHash(Encoding.UTF8.GetBytes("changed"))));
        var tamperedPath = SyncHmac.Sign(secret,
            SyncHmac.SigningString("POST", "/api/sync/pull", "2026-08-31T00:00:00Z", "n1", bodyHash));
        var wrongSecret = SyncHmac.Sign("other-secret", signing);

        Assert.False(SyncHmac.FixedTimeEquals(sig, tamperedBody));
        Assert.False(SyncHmac.FixedTimeEquals(sig, tamperedPath));
        Assert.False(SyncHmac.FixedTimeEquals(sig, wrongSecret));
    }

    [Fact]
    public void Nonce_is_url_safe_and_unique()
    {
        var a = SyncHmac.NewNonce();
        var b = SyncHmac.NewNonce();
        Assert.NotEqual(a, b);
        Assert.DoesNotContain('+', a);
        Assert.DoesNotContain('/', a);
        Assert.DoesNotContain('=', a);
    }

    // ---- Phase 5: schema-version gate -------------------------------------

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(999, false)]
    public void SchemaVersion_support_window(int v, bool supported)
        => Assert.Equal(supported, SyncSchema.IsSupported(v));

    // ---- Phase 1: deployment role parsing --------------------------------

    [Theory]
    [InlineData("Cloud", RestaurantSystem.Models.NodeRole.Cloud)]
    [InlineData("cloud", RestaurantSystem.Models.NodeRole.Cloud)]
    [InlineData("Edge", RestaurantSystem.Models.NodeRole.Edge)]
    [InlineData("", RestaurantSystem.Models.NodeRole.Edge)]
    [InlineData("garbage", RestaurantSystem.Models.NodeRole.Edge)]
    public void DeploymentOptions_parses_role(string raw, RestaurantSystem.Models.NodeRole expected)
        => Assert.Equal(expected, new DeploymentOptions { NodeRole = raw }.ParsedRole);
}
