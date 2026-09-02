# Failure & Recovery

## Failure test matrix (target behavior)

| Edge | Internet | Cloud | Expected result |
|------|----------|-------|-----------------|
| Up | Up | Up | Stations use **LOCAL**. Worker pushes/pulls every interval. Cloud RMS + public site live within one interval of edge writes. |
| Up | Down | Unreachable | Stations stay **LOCAL**. Full restaurant operation: POS, payments, kitchen, delivery status, printing, offline login. Outbox grows; nothing lost. |
| Down | Up | Up | Stations fail over to **CLOUD** after `Failover:ConsecutiveFailures` probes. POS runs against cloud DB with `CLD` order numbers. Printing degrades to whatever the station can reach (LAN agent if present) — orders still save. |
| Up | Up | Down | Stations use **LOCAL**. Worker retries with backoff; outbox queues. Public site checkout follows `DisableCheckoutWhenEdgeOffline` / `AllowDelayedOnlineOrders`. |
| Down | Down | Unreachable | **OFFLINE**. No service unless a second local node exists. RMS shows the `OFFLINE` banner; no data is invented. |
| Edge recovers | Up | Up | Worker resumes: push local backlog, pull cloud backlog, apply idempotently. Duplicate `EventId`s are no-ops; idempotency keys merge retried orders; ledger movements merge; conflicts recorded for review. No duplicate slips or notifications. |

Execute this matrix manually after Phase 15 and record results in
`IMPLEMENTATION_STATUS.md`.

## Duplicate / exactly-once guarantees

| Risk | Guard |
|------|-------|
| Same local order synced twice | `SyncInbox` keyed by `EventId`; apply is a no-op on duplicate |
| Same POS order POSTed twice (client retry) | `Idempotency-Key` → `ProcessedCommand`; same key+body replays the original response |
| Same key, different body | `409 Conflict` |
| Cross-node order retry after failover | Order `GlobalId` derived from the command ID → second attempt upserts the same aggregate |
| Payment applied twice | `Payment` append-only + unique `(OrderGlobalId, PaymentCommandId)` |
| Stock consumed twice for one order | unique `(ReferenceType, ReferenceGlobalId, MovementType, RawItemGlobalId)` on `StockMovement` |
| Duplicate print after reconnect/import | `PrintJob` dedupe by `PrintJobId` and `(OrderGlobalId, SlipType)`; imported events skip `IPrintDispatcher` |
| Duplicate SignalR notification | emit keyed by `EventId`; imported-event re-emit is de-duplicated client- and server-side |

## Conflict handling

`SyncConflict` kinds and default resolution:

| Kind | Meaning | Default | Who resolves |
|------|---------|---------|--------------|
| `stale` | incoming `AggregateVersion` ≤ local | ignore incoming body; log only | auto |
| `same-version-divergent` | equal version, different payload hash | keep local, record remote | MainAdmin / SuperAdmin |
| `version-gap` | incoming version > local + 1, gap not in batch | apply latest snapshot, flag for audit | auto + review |
| `domain-rule` | e.g. backward `Delivered`→`Preparing`, post-payment silent cancel | reject change, keep local | MainAdmin / SuperAdmin |
| `negative-stock` | ledger merge drives a balance < 0 | keep movements, raise reconciliation warning | StoreKeeper / SuperAdmin |
| `schema` | unknown `SchemaVersion` | `SyncDeadLetter`; worker continues | after upgrade, replay |

Resolution API: `POST /api/sync/conflicts/{id}/resolve`
`{ "resolution": "keepLocal" | "keepRemote" | "manual", "patchJson": "..." }`
(MainAdmin/SuperAdmin only). Resolving emits a normal outbox event so the decision
propagates.

## Offline authentication & revocation

- The edge holds synced `Users` (username, **hash**, role, enabled flag). Staff log
  in against the edge with no internet.
- Each node issues JWTs signed by its **own** RS256 private key; both APIs trust
  both public keys, so a token minted at the edge is accepted at the cloud and vice
  versa.
- **Revocation lag:** disabling a user in the cloud RMS while the edge is offline
  does not reach the edge until the next sync. Until then that user can still log in
  at the edge. Mitigations: short JWT lifetime (unchanged 1 day — consider
  shortening for edge), a synced `SecurityStamp` that invalidates tokens on change,
  and a documented manual "disable at the edge directly" procedure for
  emergencies. Every auth event logs issuer + node id.

## Edge hardware failure

1. Stations auto-fail-over to **CLOUD** (internet required). Confirm the header
   status widget shows `CLOUD`.
2. If the edge SSD is intact: move it / restore `edge-mssql-data` and
   `edge-uploads` volumes to replacement hardware, `docker compose -f
   docker-compose.edge.yml up -d`, let the worker reconcile.
3. If the SSD is lost: provision a fresh edge (`EDGE_DEPLOYMENT.md` §First install),
   restore the **latest edge SQL backup** (not just a cloud pull — the last
   unsynced minutes live only in that backup), then let the worker pull the cloud
   delta.
4. Before resuming local writes, run **`/api/sync/status`** and the **stock
   reconciliation report**; clear or accept every `SyncConflict` and
   `SyncDeadLetter`.
5. Any events that existed only on the dead SSD and were never backed up are lost —
   this is why "sync is not a backup" and why edge backups run every few minutes to
   the external drive.

## Cloud (Hostinger) failure

- Edge is unaffected functionally. Worker retries with backoff; outbox queues
  (bounded only by disk).
- Public site is down for internet customers until Hostinger/Dokploy recovers.
- On recovery the worker drains the outbox; cloud applies idempotently.

## Migration failure

- The dedicated `migrator` takes a backup checkpoint **before** applying. On
  failure it exits non-zero and the API is **not** started (compose
  `depends_on: service_completed_successfully`).
- Fix forward (new migration) or restore the checkpoint and redeploy the prior
  image. Expand/contract means the previous app runs fine against the
  partially-migrated (expanded) schema.
- Sync keeps working across adjacent schema versions; events with an unsupported
  `SchemaVersion` dead-letter and replay after the lagging node upgrades.

## Health snapshot (`GET /api/sync/status`, `GET /api/system/node-status`)

Safe fields only — no connection strings, keys, or stack traces:

```
nodeId, nodeRole, branchId, appVersion, schemaVersion,
databaseConnected (bool),
lastSuccessfulPushUtc, lastSuccessfulPullUtc,
pendingOutboxCount, deadLetterCount, conflictCount,
lastEdgeHeartbeatUtc, cloudReachable (bool), edgeReachable (bool),
operatingMode (LOCAL | CLOUD | OFFLINE)
```

`/health/live` = process up. `/health/ready` = DB reachable **and** (edge) worker
lock acquired / (cloud) migrations at expected version.
