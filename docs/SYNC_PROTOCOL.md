# Synchronization Protocol

Version: `sync/v1` · Schema version negotiated per event via `SchemaVersion`.

## 1. Roles

- **Edge worker** is the active party. It pushes local changes to the cloud and
  pulls cloud changes on a timer (`Sync:IntervalSeconds`, default 10, backoff on
  failure).
- **Cloud** is passive: it serves `push`/`pull`/`ack`/`heartbeat`/`status` and its
  own `SyncWorker` only handles conflict/dead-letter housekeeping and heartbeat
  liveness.

## 2. Event envelope

```jsonc
{
  "eventId":            "uuid",          // globally unique; idempotency key for inbox
  "eventType":          "OrderUpserted", // <Aggregate><Verb>
  "schemaVersion":      3,               // integer; receiver rejects unknown → dead-letter
  "aggregateType":      "Order",
  "aggregateGlobalId":  "uuid",
  "aggregateVersion":   42,              // monotonic per aggregate at the origin
  "branchId":           "uuid",
  "originNodeId":        "uuid",
  "occurredAtUtc":      "2026-08-31T09:14:03.187Z",
  "correlationId":      "uuid",          // business flow (e.g. the order lifecycle)
  "causationId":        "uuid|null",     // the event/command that caused this one
  "payloadJson":        "{ ...aggregate snapshot or delta, parent GlobalIds only... }"
}
```

- Payloads carry **parent `GlobalId`s**, never integer FKs.
- Payload is a full aggregate-root snapshot (root + owned children: Order + items +
  deals) for transactional/reference aggregates; ledger events are single
  immutable rows.
- `payloadJson` is canonicalized (sorted keys, no insignificant whitespace) before
  hashing/signing.

## 3. Transport endpoints

| Method | Route | Purpose |
|--------|-------|---------|
| `POST` | `/api/sync/push` | sender → receiver: `{ batchId, events: [envelope...] }`, bounded by `Sync:BatchSize` (default 200) |
| `GET`  | `/api/sync/pull?since=<checkpoint>&max=<n>&aggregateTypes=csv` | receiver returns ordered events after the checkpoint |
| `POST` | `/api/sync/ack` | `{ batchId, results: [{ eventId, status, conflictId? }] }` |
| `POST` | `/api/sync/heartbeat` | `{ nodeId, nodeRole, appVersion, schemaVersion, sentAtUtc, pendingOutbox, lastPullCheckpoint }` → `{ receivedAtUtc, cloudSchemaVersion }` |
| `GET`  | `/api/sync/status` | safe operational snapshot (see `FAILURE_RECOVERY.md` §Health) |
| `GET`  | `/api/sync/conflicts` | unresolved `SyncConflict` list (paged) |
| `POST` | `/api/sync/conflicts/{id}/resolve` | `{ resolution: "keepLocal"|"keepRemote"|"manual", patchJson? }` — MainAdmin/SuperAdmin only |

All endpoints require a valid RMS JWT **and** the HMAC headers below. None are
anonymous.

## 4. Per-node request authentication (HMAC-SHA256)

Headers on every `/api/sync/*` request:

```
X-Sync-Node:      <originNodeId>
X-Sync-Timestamp: <RFC3339 UTC, e.g. 2026-08-31T09:14:03Z>
X-Sync-Nonce:     <=128-bit random, base64url>
X-Sync-BodyHash:  <base64(SHA-256(raw request body))>   // empty-body → hash of ""
X-Sync-Signature: <base64(HMAC_SHA256(secret, signingString))>
```

`signingString = UPPER(method) + "\n" + path + "\n" + timestamp + "\n" + nonce + "\n" + bodyHash`

The shared `secret` is a per-pair value from `SYNC_HMAC_SECRET` (env / secret
mount), one per `(edgeNodeId, cloudNodeId)` pair, never in source.

Receiver rejects with `401`:
- unknown `X-Sync-Node` (not a registered `SystemNode`),
- signature mismatch,
- `X-Sync-Nonce` already seen within the retention window (`SyncNonce` table, TTL
  `Sync:NonceWindowMinutes`, default 10),
- `X-Sync-Timestamp` outside `±Sync:ClockSkewMinutes` (default 5).

TLS is still required; HMAC defends against a terminated-TLS proxy and replay.

## 5. Receiver apply algorithm

```
BEGIN TRANSACTION
  if EXISTS(SyncInbox where EventId = e.eventId):
      return { eventId, status: "duplicate" }              # idempotent no-op
  load local aggregate by GlobalId (incl. tombstone)
  switch:
    local missing:
        if e is tombstone: insert tombstone row, status "applied"
        else: insert aggregate (new int PK, map parent GlobalIds → local ints), status "applied"
    local.AggregateVersion  > e.aggregateVersion:
        status "stale"                                     # older event, ignore body
    local.AggregateVersion == e.aggregateVersion:
        if payload-hash equal: status "duplicate"
        else: create SyncConflict(kind="same-version-divergent"), status "conflict"
    local.AggregateVersion  < e.aggregateVersion:
        if e.aggregateVersion  > local.AggregateVersion + 1
           and gap not covered by this batch:
             create SyncConflict(kind="version-gap"), status "conflict"   # still apply latest snapshot
        apply domain rules (see §6); on violation → SyncConflict, status "conflict"
        upsert aggregate, set local.AggregateVersion = e.aggregateVersion
        status "applied"
  insert SyncInbox(EventId, EventType, OriginNodeId, AppliedAtUtc, Status)
COMMIT
```

`ack` statuses: `applied` · `duplicate` · `stale` · `conflict` (+ `conflictId`) ·
`deadletter` (unknown schema / unrecoverable).

## 6. Domain rules applied on import

- **Order status:** never regress `DeliveryStatus` (`Delivered`→`Preparing`
  rejected). `OrderStatus` may only move forward or to `Cancelled`.
- **Payments:** insert-only. A `(OrderGlobalId, PaymentCommandId)` unique index
  blocks double application. Reversals are separate rows with
  `ReferenceGlobalId` → original.
- **Inventory:** `StockMovement` rows are inserted, never updated/deleted. A unique
  `(ReferenceType, ReferenceGlobalId, MovementType, RawItemGlobalId)` prevents an
  order/kitchen-out/waste from consuming twice. `StoreStock` is recomputed from the
  ledger after a batch; negative results raise a reconciliation warning, not a
  hard failure.
- **Master data:** `AggregateVersion` wins. An older menu/price event can never
  overwrite a newer one. Divergent same-version edits ⇒ `SyncConflict` for
  MainAdmin/SuperAdmin.
- **Side effects:** imported events set `IsImported=true` on the unit of work.
  Notification senders and `IPrintDispatcher` skip imported events (dedupe by
  `EventId` / `(OrderGlobalId, SlipType)`).

## 7. Ordering & checkpoints

- The sender streams events ordered by `(aggregateType, aggregateVersion, occurredAtUtc)`.
- `SyncCheckpoint(PeerNodeId, Direction, AggregateType, LastAckedVersion, UpdatedAtUtc)`.
- `pull` returns everything after `LastAckedVersion` for the requested types, up to
  `max`. The receiver advances its checkpoint only for events it `ack`ed as
  `applied`/`duplicate`/`stale`.
- A `conflict` or `deadletter` does **not** block the checkpoint; it is recorded
  and the stream continues (at-least-once, converge-later).

## 8. Batching, retry, backoff

- Batch size `Sync:BatchSize` (default 200), hard cap 1000.
- On transport failure: retry with exponential backoff
  `min(BaseDelay * 2^attempt, MaxDelay)` + full jitter; `BaseDelay=2s`,
  `MaxDelay=5m`.
- `CancellationToken` threaded through worker → HTTP client → DB calls.
- Structured logs: `nodeId`, `direction`, `batchId`, `eventCount`, `applied`,
  `conflicts`, `deadletters`, `durationMs`.

## 9. Schema-version handling (expand/contract)

- Every envelope carries `schemaVersion`.
- Receiver supports a range `[MinSupported, Current]`. Below `MinSupported` or
  above `Current` ⇒ `SyncDeadLetter(kind="schema")`, `ack` status `deadletter`,
  worker continues.
- Migrations are expand-and-contract so edge and cloud can run adjacent versions
  during a rollout.

## 10. Bootstrap (first edge sync)

1. Edge registers its `SystemNode` (Phase 1) and exchanges public keys / HMAC
   secret out of band.
2. Edge `pull`s from checkpoint `0` for reference + master aggregates first
   (menu, categories, deals, recipes, areas, settings, users, riders), then
   transactional history within `Sync:BootstrapHistoryDays` (default 90).
3. Edge downloads referenced uploaded files by hash (Phase 12).
4. Steady state: incremental `push` then `pull` every `Sync:IntervalSeconds`.
