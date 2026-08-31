# Backup & Recovery (Phase 16)

**Synchronization is not a backup.** Sync converges two live nodes; it does not
protect against a bad migration, a `DELETE`, ransomware, or a dead disk. Both the
Cloud and the Edge take their own scheduled SQL backups.

## What is backed up

| Item | Where | How |
|------|-------|-----|
| Edge SQL database | `edge-backups` volume + external drive | `backup` sidecar → `backup-loop.sh` (this dir) |
| Cloud SQL database | Dokploy volume / provider snapshot + `backup-loop.sh` if added to the cloud compose | same script, `SQL_HOST=sqlserver` |
| Uploaded files | `edge-uploads` / `backend-uploads` volume | volume snapshot + they also re-sync by hash (Phase 12) |
| Configuration | `.env` / `.env.edge` | store in your password manager / secrets store — **never in git** |
| JWT / DataProtection keys | `edge-keys` / `backend-keys` volume | volume snapshot; losing them logs everyone out (recoverable) |

## Schedule & retention

- `backup-loop.sh` runs a full `BACKUP DATABASE ... WITH INIT, CHECKSUM` every
  `BACKUP_INTERVAL_MINUTES` (default 30), then `RESTORE VERIFYONLY` on the file,
  writes a `.sha256`, and (if `BACKUP_EXTERNAL_DIR` is set) copies it to the
  external drive — **encrypted** when `BACKUP_GPG_RECIPIENT` is set.
- Retention: files older than `BACKUP_RETENTION_DAYS` (validated positive
  integer, default 14) are pruned **only under `BACKUP_DIR`**. Nothing is ever
  auto-deleted from the external drive.

## Restore test (do this monthly)

```
docker compose -f docker-compose.edge.yml exec backup \
  env SQL_HOST=sqlserver MSSQL_SA_PASSWORD=... \
  /scripts/restore.sh /backups/RestaurantDB_YYYYMMDD_HHMMSSZ.bak RestaurantDB_restore_test
```

Then point a throwaway backend at `RestaurantDB_restore_test` and sanity-check.

## Edge hardware failure

1. Stations fail over to the Cloud automatically (Phase 9).
2. New edge box → `docs/EDGE_DEPLOYMENT.md` §First install.
3. `restore.sh` the **latest edge `.bak` from the external drive** (the last
   unsynced minutes live only there).
4. Run the migrator (`--migrate`) if the backup predates the current schema.
5. Before resuming local writes: check `/api/sync/status` and
   `/api/stock/reconciliation`; clear every `SyncConflict` / `SyncDeadLetter`.

## SQL Server edition

The compose files pin `MSSQL_PID: Express` (free, licensed; 10 GB DB cap).
`backup-loop.sh` logs the running edition on start and **warns loudly if it is
Developer Edition**, which is not licensed for production. Never change a
licensed edition silently.
