using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using RestaurantSystem.Data;
using RestaurantSystem.Models;

namespace RestaurantSystem.Sync
{
    /// <summary>
    /// Serialises an aggregate root (+ owned <see cref="ISyncableChild"/>
    /// collections) to a portable JSON snapshot and applies one back. Integer FKs
    /// travel as the principal's <c>GlobalId</c> and are re-resolved to local ints
    /// on the receiver. Phase 5.
    /// </summary>
    public sealed class AggregateSnapshotService
    {
        private static readonly JsonSerializerOptions Json = new() { WriteIndented = false };

        private readonly ApplicationDbContext _db;
        public AggregateSnapshotService(ApplicationDbContext db) => _db = db;

        // ============================================================ serialize

        private bool _trackerOnlyRefs;

        /// <param name="trackerOnlyRefs">
        /// When true (the SaveChanges-interceptor path) FK GlobalIds are resolved
        /// only from the change tracker — never a DB query — to stay re-entrancy safe.
        /// </param>
        public string Serialize(object entity, bool trackerOnlyRefs = false)
        {
            _trackerOnlyRefs = trackerOnlyRefs;
            var et = _db.Model.FindEntityType(entity.GetType())
                     ?? throw new InvalidOperationException($"Unknown entity {entity.GetType().Name}");
            return SerializeNode(et, entity).ToJsonString(Json);
        }

        private object? FindPrincipal(Type clr, object fkValue)
        {
            foreach (var e in _db.ChangeTracker.Entries())
            {
                if (!clr.IsInstanceOfType(e.Entity)) continue;
                var idProp = e.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
                if (idProp is null) continue;
                var idVal = idProp.PropertyInfo?.GetValue(e.Entity);
                if (idVal is not null && idVal.Equals(fkValue)) return e.Entity;
            }
            return _trackerOnlyRefs ? null : _db.Find(clr, fkValue);
        }

        private JsonObject SerializeNode(IEntityType et, object entity)
        {
            var node = new JsonObject();
            var scalars = new JsonObject();
            var refs = new JsonObject();
            var fkByProp = et.GetForeignKeys().ToDictionary(fk => fk.Properties[0].Name, fk => fk);

            foreach (var p in et.GetProperties())
            {
                if (p.IsShadowProperty() || p.Name == "Id" || p.Name == nameof(ISyncableAggregate.RowVersion)) continue;
                var val = p.PropertyInfo?.GetValue(entity);

                if (fkByProp.TryGetValue(p.Name, out var fk) && val is not null &&
                    typeof(ISyncableAggregate).IsAssignableFrom(fk.PrincipalEntityType.ClrType))
                {
                    var principal = FindPrincipal(fk.PrincipalEntityType.ClrType, val);
                    var gid = principal is ISyncableAggregate sa ? sa.GlobalId : Guid.Empty;
                    // Fall back to the raw int when the GlobalId can't be resolved here;
                    // the worker re-serializes from a fresh context before dispatch.
                    if (gid == Guid.Empty) scalars[p.Name] = JsonValue.Create(ToJsonScalar(val));
                    else refs[p.Name] = gid.ToString();
                    continue;
                }
                scalars[p.Name] = JsonValue.Create(ToJsonScalar(val));
            }

            node["scalars"] = scalars;
            node["refs"] = refs;

            var children = new JsonObject();
            foreach (var nav in et.GetNavigations().Where(n => n.IsCollection &&
                         typeof(ISyncableChild).IsAssignableFrom(n.TargetEntityType.ClrType)))
            {
                if (nav.PropertyInfo?.GetValue(entity) is not IEnumerable items) continue;
                var arr = new JsonArray();
                foreach (var child in items) arr.Add(SerializeNode(nav.TargetEntityType, child));
                children[nav.Name] = arr;
            }
            if (children.Count > 0) node["children"] = children;
            return node;
        }

        private static string? ToJsonScalar(object? v) => v switch
        {
            null => null,
            DateTime dt => dt.ToUniversalTime().ToString("O"),
            DateOnly d => d.ToString("O"),
            TimeSpan ts => ts.ToString("c"),
            byte[] b => Convert.ToBase64String(b),
            bool bl => bl ? "true" : "false",
            IFormattable f => f.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => v.ToString()
        };

        /// <summary>Cheap read of the local aggregate's version (null = not present).</summary>
        public async Task<(long version, DateTime? deletedAtUtc)?> GetLocalStateAsync(string aggregateType, Guid gid, CancellationToken ct = default)
        {
            var et = _db.Model.GetEntityTypes().FirstOrDefault(e =>
                        e.ClrType.Name == aggregateType && typeof(ISyncableAggregate).IsAssignableFrom(e.ClrType));
            if (et is null) return null;
            var table = et.GetSchemaQualifiedTableName();

            var conn = _db.Database.GetDbConnection();
            var opened = conn.State != System.Data.ConnectionState.Open;
            if (opened) await conn.OpenAsync(ct);
            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT [AggregateVersion], [DeletedAtUtc] FROM {table} WHERE [GlobalId] = @g";
                var p = cmd.CreateParameter(); p.ParameterName = "@g"; p.Value = gid; cmd.Parameters.Add(p);
                cmd.Transaction = _db.Database.CurrentTransaction?.GetDbTransaction();
                await using var r = await cmd.ExecuteReaderAsync(ct);
                if (!await r.ReadAsync(ct)) return null;
                var ver = r.GetInt64(0);
                DateTime? del = await r.IsDBNullAsync(1, ct) ? null : r.GetDateTime(1);
                return (ver, del);
            }
            finally { if (opened) await conn.CloseAsync(); }
        }

        // =============================================================== apply

        public async Task ApplyAsync(string aggregateType, Guid globalId, long aggregateVersion,
            Guid branchId, Guid originNodeId, string payloadJson, bool isDelete, CancellationToken ct = default)
        {
            var et = _db.Model.GetEntityTypes().FirstOrDefault(e =>
                        e.ClrType.Name == aggregateType && typeof(ISyncableAggregate).IsAssignableFrom(e.ClrType))
                     ?? throw new InvalidOperationException($"Unknown synced aggregate '{aggregateType}'");

            var local = await LoadRootAsync(et, globalId, ct);

            using var _ = SyncStampingInterceptor.Suppress();

            if (isDelete)
            {
                if (local is null) return;
                var sad = (ISyncableAggregate)local;
                sad.DeletedAtUtc ??= DateTime.UtcNow;
                sad.UpdatedAtUtc = DateTime.UtcNow;
                sad.AggregateVersion = aggregateVersion;
                await _db.SaveChangesAsync(ct);
                return;
            }

            var root = JsonNode.Parse(payloadJson)!.AsObject();
            bool isNew = local is null;
            var entity = local ?? Activator.CreateInstance(et.ClrType)!;
            if (isNew) ((ISyncableAggregate)entity).GlobalId = globalId;

            ApplyNode(et, entity, root, isNew);

            var sa = (ISyncableAggregate)entity;
            sa.GlobalId = globalId;
            sa.AggregateVersion = aggregateVersion;
            if (branchId != Guid.Empty) sa.BranchId = branchId;
            if (originNodeId != Guid.Empty) sa.OriginNodeId = originNodeId;
            sa.UpdatedAtUtc = DateTime.UtcNow;
            if (sa.CreatedAtUtc == default) sa.CreatedAtUtc = DateTime.UtcNow;

            if (isNew) _db.Add(entity);
            await _db.SaveChangesAsync(ct);
        }

        private void ApplyNode(IEntityType et, object entity, JsonObject node, bool isNew)
        {
            var entry = _db.Entry(entity);
            var scalars = node["scalars"]?.AsObject();
            if (scalars is not null)
            {
                var dict = new Dictionary<string, object?>();
                foreach (var p in et.GetProperties())
                {
                    if (p.IsShadowProperty() || p.Name == "Id" || p.Name == nameof(ISyncableAggregate.RowVersion)) continue;
                    if (!scalars.TryGetPropertyValue(p.Name, out var jv)) continue;
                    dict[p.Name] = jv is null ? null : Coerce(jv, p.ClrType);
                }
                foreach (var kv in dict)
                    entry.Property(kv.Key).CurrentValue = kv.Value;
            }

            var refs = node["refs"]?.AsObject();
            if (refs is not null)
            {
                var fkByProp = et.GetForeignKeys().ToDictionary(fk => fk.Properties[0].Name, fk => fk);
                foreach (var kv in refs)
                {
                    if (kv.Value is null || !fkByProp.TryGetValue(kv.Key, out var fk)) continue;
                    var parentGid = Guid.Parse(kv.Value!.GetValue<string>());
                    var localId = ResolveLocalId(fk.PrincipalEntityType, parentGid)
                                  ?? throw new SnapshotResolutionException(fk.PrincipalEntityType.ClrType.Name, parentGid);
                    entry.Property(kv.Key).CurrentValue = Coerce(JsonValue.Create(localId)!, et.GetProperty(kv.Key).ClrType);
                }
            }

            var childrenNode = node["children"]?.AsObject();
            if (childrenNode is null) return;

            foreach (var nav in et.GetNavigations().Where(n => n.IsCollection &&
                         typeof(ISyncableChild).IsAssignableFrom(n.TargetEntityType.ClrType)))
            {
                if (!childrenNode.TryGetPropertyValue(nav.Name, out var an) || an is not JsonArray arr) continue;
                if (!isNew) entry.Collection(nav.Name).Load();
                var coll = (IList)nav.PropertyInfo!.GetValue(entity)!;

                var incoming = new Dictionary<Guid, JsonObject>();
                foreach (var cn in arr)
                {
                    var co = cn!.AsObject();
                    incoming[Guid.Parse(co["scalars"]!["GlobalId"]!.GetValue<string>())] = co;
                }
                for (int i = coll.Count - 1; i >= 0; i--)
                {
                    var child = (ISyncableChild)coll[i]!;
                    if (incoming.Remove(child.GlobalId, out var co)) ApplyNode(nav.TargetEntityType, child, co, false);
                    else coll.RemoveAt(i);
                }
                foreach (var co in incoming.Values)
                {
                    var child = Activator.CreateInstance(nav.TargetEntityType.ClrType)!;
                    coll.Add(child);
                    ApplyNode(nav.TargetEntityType, child, co, true);
                }
            }
        }

        // ---- typed EF access -------------------------------------------------

        private async Task<object?> LoadRootAsync(IEntityType et, Guid globalId, CancellationToken ct)
        {
            var q = SetAsQueryable(et.ClrType);
            q = WhereGlobalId(q, et.ClrType, globalId);
            var list = await ToListAsync(et.ClrType, q, ct);
            return list.Cast<object?>().FirstOrDefault();
        }

        private int? ResolveLocalId(IEntityType principal, Guid globalId)
        {
            var q = WhereGlobalId(SetAsQueryable(principal.ClrType), principal.ClrType, globalId);
            var list = ToListAsync(principal.ClrType, q, default).GetAwaiter().GetResult();
            var first = list.Cast<object?>().FirstOrDefault();
            return first is null ? null : Convert.ToInt32(principal.ClrType.GetProperty("Id")!.GetValue(first));
        }

        private IQueryable SetAsQueryable(Type clr)
        {
            var m = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!.MakeGenericMethod(clr);
            return (IQueryable)m.Invoke(_db, null)!;
        }

        private static IQueryable WhereGlobalId(IQueryable source, Type clr, Guid gid)
        {
            var param = Expression.Parameter(clr, "e");
            var body = Expression.Equal(
                Expression.Property(param, nameof(ISyncableAggregate.GlobalId)),
                Expression.Constant(gid));
            var lambda = Expression.Lambda(body, param);
            var where = typeof(Queryable).GetMethods()
                .First(x => x.Name == nameof(Queryable.Where) && x.GetParameters().Length == 2 &&
                            x.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2)
                .MakeGenericMethod(clr);
            return (IQueryable)where.Invoke(null, new object[] { source, lambda })!;
        }

        private static async Task<IEnumerable<object>> ToListAsync(Type clr, IQueryable query, CancellationToken ct)
        {
            var m = typeof(EntityFrameworkQueryableExtensions).GetMethods()
                .First(x => x.Name == nameof(EntityFrameworkQueryableExtensions.ToListAsync) && x.GetParameters().Length == 2)
                .MakeGenericMethod(clr);
            var task = (Task)m.Invoke(null, new object[] { query, ct })!;
            await task.ConfigureAwait(false);
            var result = task.GetType().GetProperty("Result")!.GetValue(task)!;
            return ((IEnumerable)result).Cast<object>();
        }

        private static object? Coerce(JsonNode jv, Type target)
        {
            var t = Nullable.GetUnderlyingType(target) ?? target;
            string? s = jv is JsonValue val && val.TryGetValue<string>(out var str) ? str : jv.ToJsonString().Trim('"');
            if (s is null) return null;
            if (s.Length == 0 && Nullable.GetUnderlyingType(target) != null) return null;

            if (t == typeof(string)) return s;
            if (t == typeof(Guid)) return Guid.Parse(s);
            if (t == typeof(DateTime)) return DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind);
            if (t == typeof(DateOnly)) return DateOnly.Parse(s);
            if (t == typeof(TimeSpan)) return TimeSpan.Parse(s);
            if (t == typeof(bool)) return bool.Parse(s);
            if (t == typeof(int)) return int.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            if (t == typeof(long)) return long.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            if (t == typeof(short)) return short.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            if (t == typeof(decimal)) return decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            if (t == typeof(double)) return double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            if (t == typeof(float)) return float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            if (t == typeof(byte[])) return Convert.FromBase64String(s);
            if (t.IsEnum) return Enum.Parse(t, s);
            return Convert.ChangeType(s, t, System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public sealed class SnapshotResolutionException : Exception
    {
        public string PrincipalType { get; }
        public Guid MissingGlobalId { get; }
        public SnapshotResolutionException(string principalType, Guid missing)
            : base($"Referenced {principalType} {missing} is not present locally yet.")
        { PrincipalType = principalType; MissingGlobalId = missing; }
    }
}
