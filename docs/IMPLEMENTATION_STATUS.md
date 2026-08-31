# Offline-First / Cloud-Sync — Implementation Status

> Living checklist. Updated after every completed phase.
> **A phase is "Done" only when code + migration + configuration + verification all exist.**

- **Branch:** `feature/offline-first-edge-sync` (off `main` @ `a5d25b7`)
- **Started:** 2026-08-31
- **Last updated:** 2026-08-31 — Phases 0, 1, 2, 6, 14 complete (code + migration + config + verification)

---

## Legend

| Mark | Meaning |
|------|---------|
| ✅ | Done — code, migration, config, verification all present |
| 🟡 | In progress / partially landed |
| ⛔ | Not started |
| 🧊 | Deferred (needs decision or out-of-session infra) |

---

## Phase checklist

| # | Phase | State | Notes |
|---|-------|-------|-------|
| 0 | Repo inspection + gap analysis + docs | ✅ | This document + 4 companion docs |
| 1 | Node & branch identity (`Branch`, `SystemNode`, `NodeHeartbeat`, `Deployment` config) | ✅ | Entities + config + idempotent self-registration + additive migration + compose wiring. Verified: build, `ef database update`, two backend starts (create then idempotent refresh). |
| 2 | Sync-safe identity (`GlobalId`, `AggregateVersion`, timestamps, tombstones, backfill) | ✅ | `ISyncableAggregate`/`ISyncableChild` on 22 entities, `SyncStampingInterceptor`, `SyncTombstone`, `SyncBackfillService`, additive migration (`AddColumn`×126 + `CreateIndex`×54 + `SyncTombstones`; `Up()` has no drop/alter). Verified: build, migrate on dev DB, backfill stamped 101 existing rows across 11 root tables + idempotent on restart, live order create stamps `GlobalId`/`OriginNodeId`/`BranchId`/`AggregateVersion`. |
| 3 | Safe order numbering (`OrderNumberSequence` per Branch/Source/BusinessDay) | ⛔ | Depends on 1, 2 |
| 4 | Inventory ledger (`StockMovement`, reconciliation service + report) | ⛔ | Depends on 2 |
| 5 | Transactional sync (Outbox/Inbox/Checkpoint/Conflict/DeadLetter, `/api/sync/*`, HMAC) | ⛔ | Depends on 1, 2 |
| 6 | Idempotent commands (`ProcessedCommand`, `Idempotency-Key`) | ✅ | `IdempotencyMiddleware` (any mutating request with the header), `ProcessedCommand` (unique `CommandId`), `ICommandContext` + deterministic `DeriveGlobalId`. Order create now derives `GlobalId` from the key. Verified: double identical POST ⇒ 1 order, 2nd replayed (`Idempotency-Replayed: true`); same key + different body ⇒ 409 `idempotency-key-reuse`; order `GlobalId` is the derived value. Angular header wiring = Phase 9. |
| 7 | Conflict & ownership rules (orders/payments/inventory/master-data) | ⛔ | Depends on 5, 6 |
| 8 | Offline auth (DB SuperAdmin, asymmetric JWT per node, user sync) | ⛔ | Depends on 2, 5 |
| 9 | Angular PWA + runtime config + endpoint failover + status widget | ⛔ | Depends on 5, 8 |
| 10 | SignalR on selected node + import re-emit without duplicates | ⛔ | Depends on 5, 9 |
| 11 | Public online orders — edge-offline behavior flags | ⛔ | Depends on 5 |
| 12 | Uploaded-media metadata (`UploadedFile`, hash dedupe) | ⛔ | Depends on 2, 5 |
| 13 | Local printing (`IPrintDispatcher`, `PrintJob`, dedupe, reprint audit) | ⛔ | Depends on 6 |
| 14 | Controlled production migrations (dedicated migrator, expand/contract) | ✅ | `--migrate` one-shot (`DatabaseMigrator`): waits for SQL, `sp_getapplock` exclusive, optional `BACKUP DATABASE` checkpoint, `MigrateAsync` once, `SchemaMigrationHistory` row, exit 0/1. API start no longer auto-migrates outside Development; `Migrator:RequireUpToDate` fails fast on pending. `migrator` compose service gates `backend` via `service_completed_successfully`. Verified: applied `AddSchemaMigrationHistory` + exit 0, idempotent re-run "up to date" + exit 0, history row written, dev auto-migrate path intact. |
| 15 | Docker edge deployment (`docker-compose.edge.yml`, `.env.edge.example`) | ⛔ | Depends on 1, 5, 8, 13, 14 |
| 16 | Backup & recovery (scripts + docs, edition warning) | ⛔ | Partly doc-only; independent |
| 17 | Health & admin (`/health/*`, node-status, sync admin page) | ⛔ | Depends on 1, 5 |
| T | Test suites (unit / integration / e2e) + manual failure matrix | ⛔ | Grows with every phase |

---

## Phase 1 — delivered (2026-08-31)

**Files added**
- `Backend/Models/Branch.cs`, `Backend/Models/SystemNode.cs`, `Backend/Models/NodeHeartbeat.cs`
- `Backend/Sync/DeploymentOptions.cs`, `Backend/Sync/NodeRegistrationService.cs`
- `Backend/Migrations/20260831101207_AddNodeAndBranchIdentity.cs` (+ `.Designer.cs`)

**Files changed**
- `Backend/Models/Enum.cs` — add `NodeRole { Cloud, Edge }`
- `Backend/Data/ApplicationDbContext.cs` — 3 `DbSet`s + `ConfigureSyncNodeIdentity` (unique `Branch.BranchId`, unique `SystemNode.NodeId`, index `SystemNode.BranchId`, index `NodeHeartbeat(NodeId,ReceivedAtUtc)`; `Role` persisted as string)
- `Backend/Migrations/ApplicationDbContextModelSnapshot.cs` — additive only, `ProductVersion` unchanged (`8.0.22`)
- `Backend/Program.cs` — bind `DeploymentOptions` (singleton), register `NodeRegistrationService` (scoped), call `EnsureRegisteredAsync()` once in the existing post-`Migrate()` startup scope, guarded so failure never blocks the API
- `Backend/appsettings.json` — empty `Deployment` placeholder section
- `docker-compose.yml` — `Deployment__*` env passthrough on `backend`, new `backend-keys` volume mounted at `/app/keys`
- `.env.example` — `DEPLOYMENT_*` safe placeholders
- `.gitignore` — `Backend/keys/`, `**/keys/node-id.txt`, `.env.edge`

**Behavior**
- `NodeId` resolution precedence: `Deployment:NodeId` config → `keys/node-id.txt` → generated once + persisted (+ loud warning) → reuse existing DB row for the role → ephemeral (error log).
- On every startup: ensure one `Branch` (from `Deployment:BranchId`, else first existing, else a new default GUID) and upsert this `SystemNode` (role, branch, base URL, app version, last-applied migration as schema version), then insert a `self` `NodeHeartbeat`.
- Fully idempotent — restart logs `present; refreshed last-seen`, creates nothing.

**Verification run**
- `dotnet build -c Debug` → Build succeeded, 0 errors (pre-existing analyzer warnings only).
- `dotnet ef migrations add AddNodeAndBranchIdentity` → additive `CreateTable`×3 + `CreateIndex`×4; `Down` drops only the new tables.
- `dotnet ef database update` (local dev DB `.\TEW_SQLEXPRESS/RestaurantDB`) → `Applying migration '20260831101207_AddNodeAndBranchIdentity'. Done.` No existing table altered; no data change.
- Backend start #1 → `registered NEW Edge node …` + `created default Branch …` + `Now listening / Application started`.
- Backend start #2 → `Edge node … present; refreshed last-seen` (idempotent, no duplicate rows).
- `docker compose config -q` → VALID.

**Not done in Phase 1 (by design)** — no `/api/system/node-status` endpoint yet (Phase 17), no HMAC/sync transport (Phase 5), no GUID/version columns on business aggregates (Phase 2).

---

## Phase 2 — delivered (2026-08-31)

**Model** — `Backend/Sync/ISyncable.cs`: `ISyncableAggregate` (roots: `GlobalId`,
`BranchId`, `OriginNodeId`, `AggregateVersion`, `CreatedAtUtc`, `UpdatedAtUtc`,
`DeletedAtUtc?`, `RowVersion`) and `ISyncableChild` (owned: `GlobalId`,
`CreatedAtUtc`, `UpdatedAtUtc`). Applied via additive partial-class fragments in
`Backend/Models/Sync/SyncableAggregates.Partials.cs` — original entity files
untouched; `Area`/`Customer`/`ServiceTimeSetting`/`SiteSetting` gained the
`partial` keyword only.

- **Roots (15):** Order, Customer, Area, MenuItem, Category, Deal, RawItem,
  Vendor, PurchaseOrder, KitchenOut, WasteRecord, User, SiteSetting,
  ServiceTimeSetting, Rider.
- **Children (7):** OrderItem, OrderDeal, DealItem, MenuRecipe,
  PurchaseOrderItem, KitchenOutItem, WasteItem — sync inside their root's
  snapshot; no independent version/tombstone.
- Payment is intentionally deferred to Phase 7 (no `Payment` entity exists yet).

**Plumbing**
- `Backend/Sync/NodeContext.cs` — `INodeContext` singleton, populated from
  `NodeRegistrationService` output at startup.
- `Backend/Sync/SyncStampingInterceptor.cs` — `SaveChanges` interceptor:
  Added ⇒ `GlobalId`/timestamps/`AggregateVersion=1`/origin/branch; Modified ⇒
  `UpdatedAtUtc` + `AggregateVersion++`; Deleted (root) ⇒ writes a
  `SyncTombstone` in the same unit of work. `AsyncLocal` suppression flag for
  inbound-sync apply (used from Phase 5). Registered on the `DbContext` via
  `AddInterceptors`.
- `Backend/Models/SyncTombstone.cs` + `SyncTombstones` table — propagates hard
  deletes without global query filters (existing queries unchanged).
- `Backend/Sync/SyncBackfillService.cs` — startup, idempotent: stamps
  `OriginNodeId`/`BranchId` on pre-sync rows of every root table.
- `ApplicationDbContext.ApplySyncConventions()` — loops the model: unique
  `GlobalId` index everywhere; roots also get `RowVersion` `IsRowVersion()`,
  `(BranchId, UpdatedAtUtc)` + `DeletedAtUtc` indexes, and SQL defaults
  (`NEWID()` / `SYSUTCDATETIME()` / all-zero GUID / `1`) so column-add
  backfills existing rows.

**Migration** `20260831102526_AddSyncIdentityColumns` — `Up()`: `AddColumn`×126,
`CreateIndex`×54, `CreateTable` `SyncTombstones`, `UpdateData`×5 (seed rows'
`DeletedAtUtc = null`). **No `DropColumn` / `AlterColumn` / `DropIndex` in
`Up()`.** Integer PKs and every existing FK unchanged.

**Verification run**
- `dotnet build` → 0 errors.
- `dotnet ef database update` (dev DB) → `Applying migration
  '20260831102526_AddSyncIdentityColumns'. Done.`
- Backend start → `Sync/Phase2: origin/branch backfill stamped 101 pre-existing
  row(s)` across Areas/Categories/Deals/KitchenOuts/MenuItems/Orders/Riders/
  ServiceTimeSettings/SiteSettings/Users/Vendors.
- `sqlcmd` checks: Orders 25/25 have distinct non-zero `GlobalId`, 25/25
  `OriginNodeId` stamped, `AggregateVersion` = 1; OrderItems 26/26 have
  `GlobalId`; MenuItems 24/24 stamped.
- `POST /api/public/order` → new Order Id 32: fresh `GlobalId`, this node's
  `OriginNodeId`/`BranchId`, `AggregateVersion` 2 (insert + online-fields
  update = two persisted changes), child OrderItem `GlobalId` stamped.
- Restart → backfill logs nothing (idempotent). Snapshot `ProductVersion`
  unchanged (`8.0.22`).

**Deferred to later phases (by design)** — no global soft-delete query filters
(documented risk in Phase 7); `SiteSetting.OrderSerial*` counter columns still
sync as part of the row (Phase 3 excludes them); tombstone → outbox dispatch is
Phase 5.

---

## Phase 14 — delivered (2026-08-31)

- `Backend/Sync/DatabaseMigrator.cs` + `MigratorOptions.cs` +
  `Backend/Models/SchemaMigrationHistory.cs`.
- `Program.cs`: `--migrate` / `RUN_MIGRATOR=true` → run `DatabaseMigrator.RunAsync()`
  and `return` its exit code without starting Kestrel. Normal start:
  `AutoMigrate ?? env.IsDevelopment()` — apply in dev, otherwise verify only and
  throw on pending migrations when `RequireUpToDate ?? !IsDevelopment()`.
- `DatabaseMigrator`: wait-for-SQL (`CanConnectAsync` w/ backoff) → `sp_getapplock`
  `@Resource=RMS_SchemaMigration` `Exclusive` `Session` → `BACKUP DATABASE …
  WITH INIT, CHECKSUM` (no `COMPRESSION` — Express) when `BackupPath` set, else
  a logged warning → `MigrateAsync()` → `SchemaMigrationHistory` row (suppressing
  the sync interceptor) → `sp_releaseapplock` → exit 0; exit 1 + best-effort
  history on failure.
- Migration `20260831103145_AddSchemaMigrationHistory` — `CreateTable` +
  `CreateIndex` only.
- `docker-compose.yml`: new `migrator` service (`command: ["--migrate"]`,
  `restart: "no"`, `depends_on: sqlserver: service_healthy`); `backend`
  `depends_on: migrator: service_completed_successfully` + `Migrator__AutoMigrate=false`
  + `Migrator__RequireUpToDate=true`; `mssql-backups` volume on `sqlserver`.
- `.env.example`: `MIGRATOR_BACKUP_*`.

**Verification**: `dotnet run -- --migrate` applied the pending migration and
exited 0; re-run reported "already up to date" and exited 0; `SchemaMigrationHistories`
has `From=AddSyncIdentityColumns To=AddSchemaMigrationHistory Outcome=success`;
plain `dotnet run` (Development) still auto-migrates and serves; `docker compose
config -q` VALID with 5 services.

---

## Current-state gap analysis (2026-08-31)

### Repository facts established by inspection

| Area | Finding |
|------|---------|
| Solution | **No `.sln`** — single project `Backend/RestaurantSystem.csproj` (`net8.0`). Stale nested copy at `Backend/RMS-DFC-PROJECT-angular-frontend/` is excluded from compile — leave it. |
| EF Core | **8.0.22** runtime. Local tool manifest `.config/dotnet-tools.json` pins `dotnet-ef` 10.0.2; global `dotnet ef` is 10.0.9. 45 migrations. Latest: `20260816145148_AddGoogleMapsUrlToSiteSettings`. |
| Migrations at startup | `Program.cs` calls `db.Database.Migrate()` on **every** API boot (inside a DI scope). Uncontrolled — Phase 14 target. |
| PKs | Every entity uses `int` identity. Only `Customer.UpdatedAt` and `ServiceTimeSetting.UpdatedAt` exist. **No `rowversion`, no GUID keys, no soft-delete** anywhere. |
| DateTime | Global EF conversion stamps every `DateTime` as `Kind=Utc` (`Data/UtcDateTimeConverters.cs`). `IRestaurantClock`/`RestaurantClock` converts to restaurant-local via `SiteSetting.TimeZoneId` for business-day logic. |
| Order number | `OrderService.GenerateOrderNumberAsync()` does one atomic `UPDATE SiteSettings ... OUTPUT INSERTED.OrderSerialCurrentNumber WHERE Id=1`. Prefix + daily reset from the **singleton `SiteSetting` (Id = 1)**. Single-writer safe only. |
| Inventory | `StoreStock.Quantity` is a mutable running total keyed by `(RawItemId, VendorId)`. Mutated in place by `KitchenOutService.ConsumeAsync` (order + kitchen-out), purchases, waste. **No ledger.** `MenuRecipe` expands `MenuItem` → raw items. |
| Order flow | `OrdersController` POS create (`[Authorize(Roles=Waiter,Cashier,Admin,MainAdmin,SuperAdmin)]`); `PublicController` anonymous `POST /api/public/order` → `CreateOnlineOrderAsync` (fake `Admin` claims, `Paid=false`, `skipStockCheck=true`, `Status=PendingApproval`). Approve/reject/assign-rider/delivery-status/confirm-payment endpoints exist. `OrderStatus {PendingApproval,Queued,Paid,Cancelled}`, `DeliveryStatus {Approved,Preparing,Enroute,Delivered,Rejected}`. |
| Payments | **No `Payment` entity.** Payment state lives on `Order` (`Paid`, `PaidAt`, `PaymentMethod`, `CashierId`, `CashierUserName`). Phase 7 needs an append-only payment record. |
| Auth | JWT HS256, **symmetric** shared secret `AppSettings:Token`, issuer/audience from config, 1-day expiry. SuperAdmin is **config-only** (`SuperAdmin:UserName/Password`) — no DB row. `POST /api/Auth/bootstrap-mainadmin` seeds first `MainAdmin`. Users table: `PasswordHasher<User>`. Roles: SuperAdmin, MainAdmin, Admin, Cashier, Waiter, StoreKeeper, Rider. |
| Angular auth | **No HTTP interceptor.** Every service builds `Authorization: Bearer` by hand from `localStorage`. `AuthGuard` reads `route.data.roles`. |
| Angular version | **v21** (spec said v20). RMS uses PrimeNG 21, `@microsoft/signalr` 10, `jwt-decode`, `qrcode`, `jspdf`. Customer site uses Leaflet. Both: `ng build` / `ng test` (Karma/Jasmine), **no lint script configured**, **no e2e**. |
| Environments | Compile-time only. RMS/Customer prod env = relative `/api`, `/hubs`; dev = `http://localhost:7122`. No runtime config file. |
| SignalR | `OrderHub` at `/hubs/orders`, single group `"OrderQueue"` (client calls `JoinQueue` which adds to `"OrderQueue"`). Server emits `OrderQueued`, `OrderCreated`, `OrderPaid`, `OrderCancelled`, `NewOnlineOrder`, plus `OrderCreated`/`NewOnlineOrder` from `OrderService`. No reconnect/rejoin logic beyond client defaults. |
| Printing | `Printing.Services.PrintService` (singleton). **Windows-only**: `RawPrinterHelper` P/Invokes `winspool.drv`; `System.Drawing.Common`; hard-coded printer names (`POS80Printer`, `SPEEDX`), Ethernet IP `192.168.0.100:9100`, COM10, and `C:\Logo\Logo DFC.png`. Cannot run in the Linux Hostinger container — printing today implies a Windows-hosted backend on the LAN. No job IDs, no dedupe, no audit. |
| Uploads | Saved to `Backend/wwwroot/uploads/` by `CategoryController`, `DealsController`, `MenuController`, `SiteSettingsController` (GUID filenames). Served via `app.UseStaticFiles()` and proxied `/uploads/`. `.gitignore` + `.dockerignore` exclude the folder; compose mounts `backend-uploads` volume. **No DB metadata, no hash.** |
| Deployment | Root `docker-compose.yml`: `sqlserver` (mssql/server:2022-latest, `MSSQL_PID: Express`), `backend` (build `./Backend`, `ASPNETCORE_ENVIRONMENT=Production`, port 8080), `rms-frontend` (4200:80), `customer-frontend` (4300:80). Each frontend nginx proxies `/api`, `/uploads`, `/hubs` to `backend:8080`. `.env` holds real secrets (gitignored); `.env.example` is safe. Prod is this stack on Hostinger via Dokploy. |
| CORS | Named policy `AllowLocalAndFrontends` — hard-coded origin list incl. several `192.168.*` LAN IPs and Firebase hosts. `AllowCredentials()`. |
| Git | On `main`, clean, up to date with `origin/main`. Only `main` exists. |
| Tests | **Zero .NET tests.** Angular has default `*.spec.ts` stubs only. |

### Gap analysis per phase

**Phase 1 — Node/branch identity.** Nothing exists. No concept of branch, node, or node role. `NodeId` undefined. → Add `Branch`, `SystemNode`, `NodeHeartbeat` entities; `Deployment` config section; startup self-registration keyed by `Deployment:NodeId`; additive migration (new tables only, zero change to existing tables). Backfill: seed one `Branch` ("Default") and register the running node as `Cloud` on first boot of the current prod, `Edge` on the edge box.

**Phase 2 — Sync-safe identity.** No `GlobalId`/version/tombstone columns anywhere. → Add `GlobalId uniqueidentifier` (+ unique index), `BranchId`, `OriginNodeId`, `AggregateVersion bigint`, `CreatedAtUtc`, `UpdatedAtUtc`, `DeletedAtUtc?`, `RowVersion rowversion` to the ~20 prioritized aggregates. App-side `GlobalId` generation via a `SaveChanges` interceptor. Backfill migration: `GlobalId = NEWID()` per row, timestamps from best available existing column (`CreatedAt`, `IssuedAt`, `PaidAt`, else `SYSUTCDATETIME()`), `AggregateVersion = 1`. Keep int FKs; carry parent `GlobalId` in sync payloads. Highest-risk migration — expand-only, batched, tested against a restored prod copy.

**Phase 3 — Order numbering.** Current single-writer serial in `SiteSetting`. → New `OrderNumberSequence (BranchId, SourceCode, BusinessDate, LastValue)` with atomic allocation (`UPDATE ... OUTPUT` or `MERGE`), source codes `POS`/`WEB`/`CLD`, format `{Prefix}-{SOURCE}-{value:D6}`. Unique constraint on `Order.OrderNumber`. Existing numbers untouched; new logic only applies going forward. Concurrency + integration tests.

**Phase 4 — Inventory ledger.** `StoreStock.Quantity` unsyncable. → Immutable `StockMovement` ledger; backfill each `StoreStock` row as one `OpeningBalance` movement; wire purchase/consumption/kitchen-out/waste to emit movements; compensating movements for reversals; unique `(ReferenceType, ReferenceGlobalId, MovementType, RawItemId)` guard against double consumption; `StoreStock` becomes a rebuildable projection; `IStockReconciliationService` + SuperAdmin report. Migrate reports to ledger gradually behind a feature flag.

**Phase 5 — Transactional sync.** Nothing exists. → `SyncOutbox`, `SyncInbox`, `SyncCheckpoint`, `SyncConflict`, `SyncDeadLetter`; event envelope; `SaveChanges` interceptor writes outbox rows in the same transaction as the business change; `SyncController` (`push`/`pull`/`ack`/`heartbeat`/`status`/`conflicts`/`conflicts/{id}/resolve`), all `[Authorize]` + per-node HMAC-SHA256 (NodeId, UTC ts, nonce, method+path, body SHA-256, signature) with nonce store + time window; bounded batches, exp backoff + jitter, cancellation, structured logs. Prefer a **separate .NET worker project** (`SyncWorker`) sharing the domain/data assembly; single-instance guarded by a DB app-lock (`sp_getapplock`).

**Phase 6 — Idempotent commands.** No idempotency anywhere. → `ProcessedCommand (CommandId, Route, RequestHash, ResultStatus, ResultGlobalId, ResponseJson?, ProcessedAtUtc)`; `Idempotency-Key` header filter/middleware on the listed write endpoints; same key + same hash ⇒ replay stored result; same key + different hash ⇒ `409`. Order `GlobalId` derived from the command ID so cross-node retries converge. Angular generates a UUID per command.

**Phase 7 — Conflict/ownership rules.** None. → Creation-owner vs fulfilment-owner for orders; forbidden backward `DeliveryStatus` transitions; post-payment cancel ⇒ explicit reversal workflow; imported events never re-notify/re-print; payments append-only with per-command uniqueness; inventory merge-only; master data via `AggregateVersion` with conflict records for MainAdmin/SuperAdmin; stale/older events cannot overwrite newer `AggregateVersion`.

**Phase 8 — Offline auth.** SuperAdmin config-only; symmetric JWT; no user sync. → Move SuperAdmin into `Users` (bootstrap-once then disable config path); asymmetric RS256 key pair per node; each API trusts **both** issuers' public keys; private keys = mounted secrets; user/role/enabled-state sync (hashes only); documented revocation lag; issuer/node in auth audit log; preserve current `[Authorize(Roles=…)]` semantics.

**Phase 9 — Angular PWA + failover.** Compile-time env only, no service worker. → `@angular/pwa`/service worker (app-shell + static caching, no blanket API caching); `runtime-config.json` fetched at bootstrap (`edgeApiUrl`/`cloudApiUrl`/`edgeHubUrl`/`cloudHubUrl`); `EndpointSelectionService` (probe edge → fallback after N failures → background recovery probe → never switch mid-transaction / never blind-replay POST); `OperatingStatusComponent` (`LOCAL`/`CLOUD`/`OFFLINE`, last sync, pending, conflicts, edge health, cloud health) — additive, non-disruptive.

**Phase 10 — SignalR.** Single static endpoint, no rejoin. → Hub URL from `EndpointSelectionService`; `withAutomaticReconnect`; re-`JoinQueue` on `onreconnected`; server re-emits events for newly imported synced records **once** (dedupe by `EventId`); no duplicate slip prints on import/reconnect.

**Phase 11 — Public online orders.** Always writes to cloud DB; no edge awareness. → `DisableCheckoutWhenEdgeOffline`, `AllowDelayedOnlineOrders`, `EdgeOfflineThresholdSeconds`; cloud reads edge heartbeat; disabled ⇒ polite unavailable message; delayed ⇒ store `PendingApproval`, no inventory, sync to edge on reconnect, tracking stays cloud-served, edge status changes flow back. Phone-based tracking unchanged.

**Phase 12 — Uploaded media.** Path-only. → `UploadedFile (FileGlobalId, StorageKey, OriginalFileName, ContentType, Size, Sha256Hash, OriginNodeId, CreatedAtUtc, SyncStatus)`; existing URLs preserved; volume persistence kept; sync by hash (skip if hash present); edge pulls logos/menu images; MIME + filename validation, no path traversal, configurable max size; document future S3 move.

**Phase 13 — Local printing.** Windows P/Invoke, no dedupe. → `IPrintDispatcher` + `LocalPrintDispatcher` + `QueuedPrintDispatcher`; `PrintJob (PrintJobId, Type, Payload, Status, Attempts, CreatedByUserGlobalId, ...)`; dedupe by `(PrintJobId)` and by `(OrderGlobalId, SlipType)`; authorized manual reprint writes an audit row; keep ESC/POS builders as-is; printer-offline surfaces as a job status, never fails the order transaction; optional LAN print-agent contract documented.

**Phase 14 — Controlled migrations.** `Database.Migrate()` on every boot. → Dedicated migrator (console project or one-shot compose service): wait for SQL health → `sp_getapplock` → backup checkpoint → `dotnet ef database update` once → record schema version → exit 0 → API starts. Dev-only opt-in auto-migrate behind `Deployment:AutoMigrate=true`. All migrations expand/contract. Envelope carries `SchemaVersion`; unknown ⇒ dead-letter, never crash.

**Phase 15 — Edge deployment.** Only the cloud compose exists. → `docker-compose.edge.yml` (edge nginx gateway, RMS, optional customer, backend `NodeRole=Edge`, SQL Server, `SyncWorker`, persistent uploads + DataProtection/JWT keys, backup sidecar, healthchecks, `restart: unless-stopped`, resource limits, log rotation). SQL not published outside the internal network; API only via gateway; named volumes/validated host paths; static IP + LAN DNS + HTTPS cert docs; first-install, upgrade, rollback runbooks. No real secrets in compose.

**Phase 16 — Backup/recovery.** Nothing. → Scripted scheduled full SQL backups (cloud + edge), retention, integrity verify (`RESTORE VERIFYONLY` / checksum), encrypted off-machine copy, external-drive copy, restore test, edge-hardware-failure recovery runbook. No auto-deletion without validated retention. **Warn: prod compose uses `MSSQL_PID: Express`** — acceptable licensing but 10 GB DB cap; flag if any node is switched to `Developer` in production.

**Phase 17 — Health/admin.** No health endpoints. → `/health/live`, `/health/ready`, `/api/system/node-status`, `/api/sync/status` (safe fields only); SuperAdmin/MainAdmin sync admin page in RMS (node health, mode, pending, last sync, conflicts, dead letters, retry, resolve, reconciliation status). No secrets/exceptions leaked.

### Cross-cutting risks

1. **Phase 2 migration on production data** is the single highest risk — adding a non-null `GlobalId` + unique index to every large table on a live SQL Server 2014-compat Express DB. Must be expand-only, batched backfill, run by the Phase 14 migrator against a restored copy first. **Stop-and-confirm before prod.**
2. **Printing is Windows-only.** Any "backend runs on Linux edge" assumption breaks current printing. Phase 13 must define the print path (Windows edge host, or LAN agent) before Phase 15 finalizes the edge image.
3. **SQL Server Express 10 GB cap** per node — relevant to backup/retention and long-term edge operation.
4. **HS256 → RS256 JWT** change (Phase 8) invalidates all existing tokens on cutover — schedule a re-login window.
5. **No test infrastructure** — a `.NET` test project and Angular test wiring must be created before most phases can be "Done" per the spec's own bar.
6. **EF tools 10 vs EF runtime 8** — migrations add works; keep an eye on snapshot/designer output.

### Dependency-ordered build plan

```
0 ✅ inspection + docs
1 → node/branch identity            (independent, additive tables)
2 → sync-safe identity              (needs 1 for OriginNodeId/BranchId defaults)
14 → controlled migrator            (independent; needed operationally before 2 hits prod)
6 → idempotent commands             (needs 2)
3 → order numbering                 (needs 1,2)
4 → inventory ledger                (needs 2)
5 → transactional sync engine       (needs 1,2; worker project)
8 → offline auth                    (needs 2,5)
7 → conflict/ownership rules        (needs 5,6)
12 → uploaded media                 (needs 2,5)
13 → local printing                 (needs 6)
17 → health/admin + sync page       (needs 1,5)
10 → SignalR on selected node       (needs 5)
9 → Angular PWA + failover          (needs 5,8,17)
11 → public online-order flags      (needs 5)
15 → edge compose                   (needs 1,5,8,13,14)
16 → backup/recovery                (mostly independent; finalize with 15)
T  → tests grow alongside every phase; failure matrix executed after 15
```
