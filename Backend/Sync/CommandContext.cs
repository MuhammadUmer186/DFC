using System;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// Per-request carrier of the current <c>Idempotency-Key</c>, populated by
    /// <c>IdempotencyMiddleware</c>. Services (e.g. order creation) read
    /// <see cref="CommandId"/> to derive a deterministic aggregate
    /// <c>GlobalId</c> so a cross-node retry merges into the same aggregate
    /// instead of creating a duplicate. Phase 6.
    /// </summary>
    public interface ICommandContext
    {
        bool HasKey { get; }
        Guid CommandId { get; }
        void Set(Guid commandId);

        /// <summary>
        /// Deterministic GlobalId for <paramref name="aggregateType"/> derived
        /// from the current command id. Same key + same aggregate type ⇒ same
        /// GlobalId on every node. Returns <c>null</c> when no key is present.
        /// </summary>
        Guid? DeriveGlobalId(string aggregateType);
    }

    public sealed class CommandContext : ICommandContext
    {
        private Guid _commandId;

        public bool HasKey => _commandId != Guid.Empty;
        public Guid CommandId => _commandId;
        public void Set(Guid commandId) => _commandId = commandId;

        public Guid? DeriveGlobalId(string aggregateType)
        {
            if (_commandId == Guid.Empty) return null;
            return DeriveGlobalId(_commandId, aggregateType);
        }

        /// <summary>Stable v8-style name-based GUID: SHA-256(commandId + "|" + type)[0..16].</summary>
        public static Guid DeriveGlobalId(Guid commandId, string aggregateType)
        {
            Span<byte> input = stackalloc byte[16 + 64];
            commandId.TryWriteBytes(input);
            var typeBytes = Encoding.UTF8.GetBytes("|" + aggregateType);
            var len = 16 + Math.Min(typeBytes.Length, 64);
            typeBytes.AsSpan(0, len - 16).CopyTo(input.Slice(16));

            Span<byte> hash = stackalloc byte[32];
            SHA256.HashData(input.Slice(0, len), hash);
            return new Guid(hash.Slice(0, 16));
        }
    }
}
