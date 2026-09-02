# Test coverage for the offline-first work

`dotnet test Backend.Tests` — 13 DB-free unit tests (idempotency-key derivation,
HMAC sign/verify + tamper, nonce, schema window, role parsing). **All pass.**

The 24 scenarios from the spec, and where each is covered:

| # | Scenario | Status | Where |
|---|----------|--------|-------|
| 1 | Local POS order while cloud unavailable | ✅ verified | Edge node has no peer configured — order create works, outbox queues (Phase 5 notes) |
| 2 | Local order syncs exactly once after recovery | ✅ verified (single-node) | pull→clone→push ⇒ `applied`; re-push same EventId ⇒ `duplicate` (Phase 5) |
| 3 | Duplicate event delivery | ✅ verified | same as #2 — `SyncInbox` dedupe |
| 4 | Duplicate order POST, same Idempotency-Key | ✅ verified | 2 identical POSTs ⇒ 1 order, 2nd replayed (Phase 6) |
| 5 | Same key, different body | ✅ verified | ⇒ 409 `idempotency-key-reuse` (Phase 6) |
| 6 | Separate local/cloud order numbers | ✅ verified | POS-000001 vs WEB-000001 independent sequences; CLD on Cloud role (Phase 3) |
| 7 | Online order cloud→edge sync | 🔬 integration | mechanism proven by #2; needs both nodes |
| 8 | Approval edge→cloud sync | 🔬 integration | same |
| 9 | Payment duplicate prevention | ⛔ pending | needs `Payment` entity (Phase 7 remaining) |
| 10 | Rider payment confirmation | 🔬 integration | endpoint exists; idempotency via Phase 6 |
| 11 | Order cancellation + inventory reversal | ✅ verified | `CancelOrderAsync` emits compensating `Return` movements; originals kept (Phase 4) |
| 12 | Stock movement merging | ✅ verified | ledger append-only + `UX_StockMovements_Reference`; PO +20, reconciliation balanced (Phase 4) |
| 13 | Stale aggregate-version rejection | 🔬 integration | `SyncApplyService` returns `stale` when localVersion > incoming (unit-testable next) |
| 14 | Version-gap conflict creation | 🔬 integration | `SyncApplyService` records `version-gap` `SyncConflict` |
| 15 | Menu-price concurrent edit conflict | 🔬 integration | `same-version-divergent` path in `SyncApplyService` |
| 16 | Offline login | ✅ verified | DB SuperAdmin seeded from config; users sync; login works with no internet (Phase 8) |
| 17 | Disabled user after sync | ✅ verified | disabled user ⇒ 400 + audit `disabled` (Phase 8) |
| 18 | SignalR reconnect after endpoint change | 🔬 manual | `onreconnected` re-invokes `JoinQueue`; hub URL from `EndpointService` (Phase 10) |
| 19 | No duplicate notifications | ✅ by design | imported Order events emit `OrderCreated` once per EventId (inbox-deduped) |
| 20 | No duplicate printing | ✅ verified | `LocalPrintDispatcher` dedupe by `(OrderGlobalId, JobType, Copy)`; reprint bypasses + audits (Phase 13) |
| 21 | Upload hash dedupe | ✅ verified | identical re-upload ⇒ same URL, no new row (Phase 12) |
| 22 | Migration from existing prod schema | ✅ verified (dev copy) | every migration `Up()` additive (one benign `OrderNumber` length cap); applied to the dev DB which carries real data |
| 23 | Existing data remains readable | ✅ verified | backfill stamped 101 rows; existing users keep `IsActive=true`; legacy order numbers untouched |
| 24 | Restore from backup | 🔬 manual | `deploy/backup/restore.sh` + monthly test in `deploy/backup/README.md` |

**Legend** — ✅ verified: exercised end-to-end this session (see IMPLEMENTATION_STATUS.md
per-phase "Verified" notes). 🔬 integration/manual: mechanism implemented; needs
two running nodes or a device. ⛔ pending: blocked on remaining Phase 7 work.

## Manual failure test matrix

See `docs/FAILURE_RECOVERY.md` — the Edge×Internet×Cloud table. Execute it after
a real two-node deployment and record results back in IMPLEMENTATION_STATUS.md.
