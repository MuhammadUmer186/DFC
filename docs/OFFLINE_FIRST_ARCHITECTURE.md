# Offline-First Architecture

## Goal

The restaurant keeps operating — POS orders, payments, kitchen, delivery, printing,
staff login — when the internet is down. When connectivity returns, the local and
cloud databases reconcile automatically, exactly once, with explicit conflict
handling. No feature, route, role, API path, print layout, uploaded-file URL, or
existing row is lost.

## Topology

```
                         ┌──────────────────────────────────────────┐
                         │  CLOUD  (Hostinger VPS / Dokploy)         │
                         │  NodeRole = Cloud                         │
                         │                                          │
   Internet customers ──►│  customer-frontend  (dfc.aiotstudio…)    │
   Owner / off-site  ───►│  rms-frontend       (dfcfront.aiot…)     │
                         │  backend  :8080   ── SignalR OrderHub     │
                         │  SQL Server (cloud DB)                    │
                         │  SyncWorker (Cloud)                       │
                         └───────────────▲──────────────────────────┘
                                         │  HTTPS + per-node HMAC
                                         │  /api/sync/push|pull|ack|heartbeat
                                         │
                         ┌───────────────▼──────────────────────────┐
                         │  EDGE  (restaurant LAN, mini-PC/server)   │
                         │  NodeRole = Edge                          │
                         │                                          │
   POS stations  ───────►│  edge gateway (nginx)  https://edge.lan   │
   Kitchen / rider  ────►│  rms-frontend (PWA)                       │
   (optional) local site │  backend  :8080   ── SignalR OrderHub     │
                         │  SQL Server (edge DB)                     │
                         │  SyncWorker (Edge)  ── drives push/pull   │
                         │  local ESC/POS printers / LAN print agent │
                         │  backup sidecar → external drive          │
                         └──────────────────────────────────────────┘
```

Both DBs are **temporarily independent writable nodes**. This is not replication,
volume copying, or last-write-wins. It is application-level, event-sourced
synchronization over an idempotent protocol.

## Operating modes (RMS client)

| Mode | When | Client points at | SignalR |
|------|------|------------------|---------|
| `LOCAL` | Edge health probe OK | `edgeApiUrl` | `edgeHubUrl` |
| `CLOUD` | Edge unreachable **and** internet OK, after N consecutive failed probes | `cloudApiUrl` | `cloudHubUrl` |
| `OFFLINE` | Neither edge nor cloud reachable | last selected | disconnected, queued UI only where safe |

Rules:
- Probe edge first (short timeout). Prefer `LOCAL`.
- Switch to `CLOUD` only after `Failover:ConsecutiveFailures` failed probes.
- **Never** switch endpoints during an unfinished order/payment transaction.
- **Never** auto-replay an uncertain `POST` against the other node except with its
  original `Idempotency-Key`.
- Probe for edge recovery in the background; switch back only when no critical
  transaction is active.

## Data classification

| Class | Aggregates | Sync direction | Conflict strategy |
|-------|-----------|----------------|-------------------|
| Transactional | Order, OrderItem, OrderDeal, Payment | two-way | creation-owner; append-only payments; forbidden backward status transitions |
| Ledger | StockMovement | two-way | immutable merge; never overwrite balances; compensating entries only |
| Reference / master | MenuItem, Category, Deal, DealItem, MenuRecipe, RawItem, Vendor, Area, ServiceTimeSetting, SiteSetting, Rider, Customer, User | two-way | `AggregateVersion` compare; stale loses; unresolved ⇒ `SyncConflict` |
| Cloud-authoritative (read-mostly at edge) | PurchaseOrder, WasteRecord (headers), reports data, AI\* | two-way but rarely edited at edge | `AggregateVersion` compare |
| Local-only (never synced) | `RowVersion` values, `SyncInbox`/`Outbox` bookkeeping, `ProcessedCommand`, `PrintJob`, per-node `OrderNumberSequence` counters, per-node JWT keys | — | — |

## Identity model

- `int` identity PKs stay as **internal** keys, unique only within one DB.
- Every synced aggregate gets `GlobalId uniqueidentifier` (app-generated, unique
  index) — the cross-node identity. Sync payloads reference **parent `GlobalId`**,
  never int FKs.
- `AggregateVersion bigint` increments on every change to an aggregate root and
  orders events across nodes.
- `RowVersion` (SQL `rowversion`) guards concurrency **inside one DB only** and is
  never compared across nodes.
- `OriginNodeId` + `BranchId` stamp where a row was born.
- `CreatedAtUtc` / `UpdatedAtUtc` / `DeletedAtUtc` in UTC for sync ordering;
  `RestaurantClock` still owns all business-day / local-time logic.
- Deletes of synced data are **tombstones** (`DeletedAtUtc` set) so removal
  propagates.

## Order numbering

Per-writer sequences replace the singleton counter:

```
OrderNumberSequence(BranchId, SourceCode ∈ {POS,WEB,CLD}, BusinessDate) → LastValue
Display: {SiteSetting.OrderSerialPrefix}-{SourceCode}-{value:D6}   e.g. DFC-POS-000123
```

Atomic allocation, unique constraint on `Order.OrderNumber`, existing numbers
untouched.

## Synchronization engine

- **Transactional Outbox:** the `SaveChanges` interceptor writes one `SyncOutbox`
  row per changed aggregate in the *same* SQL transaction as the business write.
- **Idempotent Inbox:** receiver checks `EventId` against `SyncInbox`; unseen
  events apply transactionally with the inbox row written in the same transaction.
- **Checkpoints:** `SyncCheckpoint` tracks the last acked `AggregateVersion` per
  `(peerNodeId, aggregateType)` in each direction.
- **Conflicts:** stale `AggregateVersion`, version gaps, or domain-rule violations
  produce a `SyncConflict` (never a silent overwrite).
- **Dead letters:** unknown `SchemaVersion` or repeated apply failure ⇒
  `SyncDeadLetter`; the worker keeps running.
- **Transport:** `POST /api/sync/push`, `GET /api/sync/pull`, `POST /api/sync/ack`,
  `POST /api/sync/heartbeat`, `GET /api/sync/status`, `GET /api/sync/conflicts`,
  `POST /api/sync/conflicts/{id}/resolve`. All authenticated; all additionally
  signed per-node with HMAC-SHA256 (see `SYNC_PROTOCOL.md`).
- **Worker:** dedicated `SyncWorker` .NET project, one instance per node, guarded
  by `sp_getapplock`. Bounded batches, exponential backoff + jitter, cancellation
  tokens, structured logs.

## Idempotency

Critical writes accept `Idempotency-Key: <uuid>`. `ProcessedCommand` stores the
key, route, request hash, result status, result `GlobalId`, and (where reasonable)
the response body. Same key + same body ⇒ original response replayed. Same key +
different body ⇒ `409 Conflict`. Order `GlobalId` is derived from the command ID so
a cross-node retry merges into the same order.

## Authentication

- SuperAdmin becomes a real `Users` row (config path kept only for one-time
  bootstrap, then disabled once a DB SuperAdmin exists).
- Each node signs JWTs with its **own RS256 private key** (mounted secret). Every
  API trusts the configured **public keys of both** issuers (`cloud`, `edge`).
- Active users, password hashes, roles, enabled/disabled state sync (hashes only,
  never plaintext).
- Revocation while the edge is offline is **delayed** until the next sync — see
  `FAILURE_RECOVERY.md`.

## Printing

`IPrintDispatcher` abstracts the transport. `LocalPrintDispatcher` talks to the
edge-attached printers (current ESC/POS builders unchanged); `QueuedPrintDispatcher`
persists `PrintJob`s and retries. Every job has a `PrintJobId`; a slip is never
printed twice for the same `(OrderGlobalId, SlipType)`. Printer-offline is a job
status, never an order-transaction failure. Authorized manual reprints are audited.

## What explicitly does not change

RMS routes, customer routes, roles, public API paths, phone-based order tracking,
discount math, deal pricing, delivery-fee snapshots, business-day logic, menu
recipes, reports, existing SignalR event names, ESC/POS layouts, uploaded-file
URLs, and all existing production rows.
