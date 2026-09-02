# Applying the offline-first schema migrations

`offline-first.idempotent.sql` is an **idempotent** EF Core migration script:
it checks `__EFMigrationsHistory` and applies only the migrations that are
missing, so it is safe to run against a database in any state (it skips the
~20 pre-offline-first migrations and applies the 10 new ones).

Migrations it will add if missing:

```
20260831101207_AddNodeAndBranchIdentity
20260831102526_AddSyncIdentityColumns      <- adds GlobalId / AggregateVersion / ... to every synced table
20260831103145_AddSchemaMigrationHistory
20260831103654_AddProcessedCommands
20260831104157_AddOrderNumberSequences
20260831105110_AddStockMovementLedger
20260831110632_AddSyncEngine
20260831111606_AddUploadedFiles
20260831112320_AddOfflineAuth
20260831112655_AddPrintJobs
```

> **Take a database backup first.** These are additive / data-preserving and
> were verified on a restored copy, but `AddSyncIdentityColumns` alters every
> major table.

## How to run it

Pick whichever fits your access. **Run it against the deployed `RestaurantDB`.**

### 1. Into the SQL Server container (no host Docker CLI needed elsewhere)

```bash
# copy the script to the host, then:
docker cp deploy/migrations/offline-first.idempotent.sql <sqlserver-container>:/tmp/mig.sql
docker exec -i <sqlserver-container> /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P '<SA_PASSWORD>' -C -d RestaurantDB -b -i /tmp/mig.sql
```

### 2. Any SQL client

Open `offline-first.idempotent.sql` in DBeaver / Azure Data Studio / SSMS /
the Dokploy database console, connect to `RestaurantDB`, and execute the whole
script. It uses `GO` batch separators, so use a client that honours them
(sqlcmd, SSMS, ADS do).

### 3. Let the API apply them once (Dokploy env change)

On the `backend` service set:

```
Migrator__AutoMigrate=true
```

redeploy / restart the service once (it runs `Database.Migrate()` on boot),
then set it back to `false`. The dedicated `migrator` compose service is the
intended long-term path.

### 4. The migrator container

```bash
docker compose run --rm migrator          # from the repo root, on the Docker host
```

## Regenerating this script

```bash
cd Backend
dotnet ef migrations script --idempotent -o ../deploy/migrations/offline-first.idempotent.sql
```
