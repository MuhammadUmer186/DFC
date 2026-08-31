#!/usr/bin/env bash
# Phase 16 — scheduled SQL backups for the edge (and reusable on the cloud).
# Runs forever inside the `backup` container: full BACKUP DATABASE every
# INTERVAL_MINUTES, verifies each, prunes by RETENTION_DAYS, and (optionally)
# copies encrypted off-machine to an external drive.
#
# It NEVER deletes anything outside BACKUP_DIR and only prunes files older than
# a validated RETENTION_DAYS (must be a positive integer).
set -euo pipefail

: "${SQL_HOST:?}" "${DB_NAME:?}" "${MSSQL_SA_PASSWORD:?}" "${BACKUP_DIR:?}"
INTERVAL_MINUTES="${INTERVAL_MINUTES:-30}"
RETENTION_DAYS="${RETENTION_DAYS:-14}"
EXTERNAL_COPY_DIR="${EXTERNAL_COPY_DIR:-}"
GPG_RECIPIENT="${GPG_RECIPIENT:-}"

SQLCMD="/opt/mssql-tools/bin/sqlcmd"
[ -x "$SQLCMD" ] || SQLCMD="/opt/mssql-tools18/bin/sqlcmd"

log() { echo "[$(date -u +%FT%TZ)] $*"; }

case "$RETENTION_DAYS" in
  ''|*[!0-9]*) log "FATAL: RETENTION_DAYS must be a positive integer, got '$RETENTION_DAYS'"; exit 1;;
esac
[ "$RETENTION_DAYS" -ge 1 ] || { log "FATAL: RETENTION_DAYS must be >= 1"; exit 1; }

mkdir -p "$BACKUP_DIR"

edition_warning() {
  local v
  v=$("$SQLCMD" -S "$SQL_HOST" -U sa -P "$MSSQL_SA_PASSWORD" -C -h -1 -W \
        -Q "SET NOCOUNT ON; SELECT CAST(SERVERPROPERTY('Edition') AS varchar(128));" 2>/dev/null || echo "?")
  log "SQL Server edition: $v"
  case "$v" in
    *Developer*) log "WARNING: Developer Edition detected — NOT licensed for production use.";;
  esac
}

backup_once() {
  local ts file
  ts="$(date -u +%Y%m%d_%H%M%S)Z"
  file="$BACKUP_DIR/${DB_NAME}_${ts}.bak"

  log "BACKUP -> $file"
  "$SQLCMD" -S "$SQL_HOST" -U sa -P "$MSSQL_SA_PASSWORD" -C -b -Q \
    "BACKUP DATABASE [$DB_NAME] TO DISK = N'$file' WITH INIT, CHECKSUM, NAME = N'scheduled edge backup';"

  log "VERIFY $file"
  "$SQLCMD" -S "$SQL_HOST" -U sa -P "$MSSQL_SA_PASSWORD" -C -b -Q \
    "RESTORE VERIFYONLY FROM DISK = N'$file' WITH CHECKSUM;"

  sha256sum "$file" > "$file.sha256"

  if [ -n "$EXTERNAL_COPY_DIR" ] && [ -d "$(dirname "$EXTERNAL_COPY_DIR")" ]; then
    mkdir -p "$EXTERNAL_COPY_DIR"
    if [ -n "$GPG_RECIPIENT" ] && command -v gpg >/dev/null 2>&1; then
      gpg --yes --batch --trust-model always -r "$GPG_RECIPIENT" -o "$EXTERNAL_COPY_DIR/$(basename "$file").gpg" -e "$file"
      cp "$file.sha256" "$EXTERNAL_COPY_DIR/"
      log "off-machine (encrypted) -> $EXTERNAL_COPY_DIR/$(basename "$file").gpg"
    else
      cp "$file" "$file.sha256" "$EXTERNAL_COPY_DIR/"
      log "off-machine (plain) -> $EXTERNAL_COPY_DIR/  (set GPG_RECIPIENT to encrypt)"
    fi
  fi
}

prune() {
  log "prune: removing *.bak older than ${RETENTION_DAYS}d under $BACKUP_DIR"
  find "$BACKUP_DIR" -maxdepth 1 -type f \( -name '*.bak' -o -name '*.bak.sha256' \) -mtime "+$RETENTION_DAYS" -print -delete || true
}

edition_warning
while true; do
  if backup_once; then prune; else log "backup FAILED — keeping all existing backups, will retry"; fi
  log "sleep ${INTERVAL_MINUTES}m"
  sleep "$((INTERVAL_MINUTES * 60))"
done
