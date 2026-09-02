#!/usr/bin/env bash
# Phase 16 — restore a .bak into a target SQL Server. Use for restore testing
# and for edge-hardware recovery (see docs/FAILURE_RECOVERY.md).
#
#   ./restore.sh <path-to.bak> [TARGET_DB_NAME]
#
# Verifies the checksum + RESTORE VERIFYONLY first, then restores WITH REPLACE.
set -euo pipefail

BAK="${1:?usage: restore.sh <file.bak> [db-name]}"
DB="${2:-RestaurantDB}"
: "${SQL_HOST:?set SQL_HOST}" "${MSSQL_SA_PASSWORD:?set MSSQL_SA_PASSWORD}"

SQLCMD="/opt/mssql-tools/bin/sqlcmd"; [ -x "$SQLCMD" ] || SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
log() { echo "[$(date -u +%FT%TZ)] $*"; }

[ -f "$BAK" ] || { log "no such file: $BAK"; exit 1; }
if [ -f "$BAK.sha256" ]; then
  log "checksum…"; ( cd "$(dirname "$BAK")" && sha256sum -c "$(basename "$BAK").sha256" )
fi

log "RESTORE VERIFYONLY…"
"$SQLCMD" -S "$SQL_HOST" -U sa -P "$MSSQL_SA_PASSWORD" -C -b -Q \
  "RESTORE VERIFYONLY FROM DISK = N'$BAK' WITH CHECKSUM;"

log "RESTORE DATABASE [$DB] … WITH REPLACE"
"$SQLCMD" -S "$SQL_HOST" -U sa -P "$MSSQL_SA_PASSWORD" -C -b -Q \
  "ALTER DATABASE [$DB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
   RESTORE DATABASE [$DB] FROM DISK = N'$BAK' WITH REPLACE, CHECKSUM;
   ALTER DATABASE [$DB] SET MULTI_USER;" || \
"$SQLCMD" -S "$SQL_HOST" -U sa -P "$MSSQL_SA_PASSWORD" -C -b -Q \
  "RESTORE DATABASE [$DB] FROM DISK = N'$BAK' WITH REPLACE, CHECKSUM;"

log "done. Run the migrator (--migrate) if the backup predates the current schema."
