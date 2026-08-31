# Edge Deployment

The edge is a small always-on machine on the restaurant LAN running the full
stack plus a `SyncWorker`. It is authoritative for the shop; the cloud is a
synced replica that also hosts the public site.

> Status: **planned (Phase 15)**. This document is the target design and runbook.
> `docker-compose.edge.yml` and `.env.edge.example` are delivered in Phase 15.

## 1. Hardware / OS

| Item | Recommendation | Why |
|------|----------------|-----|
| Machine | Mini-PC, 4+ cores, 16 GB RAM, 256 GB SSD, wired Ethernet, on a UPS | SQL Server + 4 containers; UPS so power blips don't corrupt the DB |
| OS | **Windows 11 Pro / Windows Server** if the ESC/POS printers use Windows spooler drivers (current `PrintService` P/Invokes `winspool.drv`); otherwise Ubuntu LTS + a LAN print agent | Printing path decides this — see Phase 13 |
| Container runtime | Docker Desktop (Windows) or Docker Engine (Linux), start on boot | `restart: unless-stopped` needs the daemon auto-starting |
| Static IP | Reserve a DHCP lease, e.g. `192.168.1.10` | Stations must find the edge at a fixed address |
| LAN DNS | `edge.dfc.lan` → the static IP (router host entry or Pi-hole/dnsmasq) | Friendly, cert-able hostname |
| HTTPS cert | Internal CA cert for `edge.dfc.lan` installed on every station, **or** a real cert for a subdomain that resolves to the LAN IP internally | Service worker / PWA and SignalR want HTTPS; browsers reject untrusted certs for WSS |

## 2. Compose services (`docker-compose.edge.yml`)

| Service | Image / build | Notes |
|---------|---------------|-------|
| `edge-gateway` | nginx | The only externally reachable port on the LAN (`443`). Proxies `/api`, `/uploads`, `/hubs` to `backend:8080`; serves the RMS SPA; optional `/order` → customer SPA. TLS terminates here. |
| `rms-frontend` | build `./Frontend/RMS` | PWA build; `runtime-config.json` mounted with edge + cloud URLs |
| `customer-frontend` | build `./Frontend/CustomerOrderingWeb` | optional — only if the shop wants an on-LAN ordering page / QR |
| `backend` | build `./Backend` | `Deployment__NodeRole=Edge`, `Deployment__NodeId` from secret, `ASPNETCORE_ENVIRONMENT=Production`, JWT **private** key mounted, both issuers' **public** keys mounted. **No published port** — only reachable via `edge-gateway` on the internal network. |
| `migrator` | build `./Backend` (entrypoint = migrator) | one-shot; waits for SQL health, takes `sp_getapplock`, backup checkpoint, `ef database update`, records schema version, exits 0. `backend` `depends_on: migrator: condition: service_completed_successfully`. |
| `sync-worker` | build `./SyncWorker` | one instance; `Deployment__NodeRole=Edge`; `Sync__CloudBaseUrl=https://dfcfront.aiotstudio.online`; `SYNC_HMAC_SECRET` mounted. Guarded by DB app-lock so a second copy can't run. |
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest`, `MSSQL_PID=Express` | **not published** outside the compose network. Named volume `edge-mssql-data`. |
| `backup` | small alpine + `sqlcmd`/`sqlpackage` + cron | scheduled full backups → `edge-backups` volume and a validated host path on the external drive (Phase 16). |

## 3. Volumes (persistent)

```
edge-mssql-data     → SQL Server data/log
edge-uploads        → /app/wwwroot/uploads   (shared with backend; same URL scheme as cloud)
edge-dpkeys         → /app/keys              (ASP.NET Data Protection + JWT private key material)
edge-backups        → SQL + file + config backups (also copied to external drive by `backup`)
```

Use **named volumes** or explicit host paths that are validated to exist and be
writable at container start. Never bind-mount a path that might not be mounted
(external drive absent → fail loudly, don't write to the container layer).

## 4. Networking

```
stations / kitchen / rider phones
        │  https://edge.dfc.lan
        ▼
   edge-gateway (nginx, :443)  ──────────────► internet ──► cloud /api/sync/*
        │ internal docker network only
        ▼
   backend:8080  ──►  sqlserver:1433 (internal only)
        ▲
   sync-worker ──────────────────────────────► cloud (outbound HTTPS only)
```

- Only `edge-gateway:443` is exposed on the LAN.
- `sqlserver` has **no** host port mapping.
- Outbound HTTPS to the cloud is the only internet dependency; losing it drops the
  node to full local operation, no functional loss inside the shop.

## 5. First install

1. Provision OS, static IP, LAN DNS `edge.dfc.lan`, install Docker, enable
   start-on-boot, attach UPS + external backup drive.
2. Install the internal CA / TLS cert for `edge.dfc.lan` on the edge and every
   station.
3. `git clone` the repo (this branch) to the edge; `cp .env.edge.example .env.edge`
   and fill real values (SQL password, `Deployment__NodeId` = a fresh GUID,
   `Deployment__BranchId`, `SYNC_HMAC_SECRET`, JWT key paths).
4. Generate the edge RS256 key pair; place the private key in `edge-dpkeys`, share
   the **public** key with the cloud config; put the cloud public key on the edge.
5. Register the edge node with the cloud (SuperAdmin action, Phase 17) so its
   `SystemNode` row + HMAC secret exist on both sides.
6. `docker compose -f docker-compose.edge.yml --env-file .env.edge up -d`.
   `migrator` runs once; `backend` starts after it succeeds.
7. Watch `GET https://edge.dfc.lan/api/sync/status` until the bootstrap pull
   completes (menu, users, recent history, uploaded files present).
8. Point one station at `https://edge.dfc.lan`, log in, place a test POS order,
   verify it appears in the cloud RMS within one sync interval, then void it.

## 6. Station configuration

- Each POS/kitchen station opens `https://edge.dfc.lan` and installs the PWA.
- `runtime-config.json` served by the edge gateway:
  ```json
  {
    "edgeApiUrl":  "https://edge.dfc.lan/api",
    "cloudApiUrl": "https://dfcfront.aiotstudio.online/api",
    "edgeHubUrl":  "https://edge.dfc.lan/hubs/orders",
    "cloudHubUrl": "https://dfcfront.aiotstudio.online/hubs/orders"
  }
  ```
- If the edge machine itself dies, the PWA fails over to `cloudApiUrl` (internet
  permitting) — see `FAILURE_RECOVERY.md`.

## 7. Upgrade

1. Cloud first (Dokploy) — deploy the new expand-phase image; verify `schemaVersion`.
2. `git pull` on the edge.
3. `docker compose -f docker-compose.edge.yml build`.
4. `docker compose -f docker-compose.edge.yml up -d migrator` → wait exit 0.
5. `docker compose -f docker-compose.edge.yml up -d` (rolling: gateway last).
6. Verify `/api/sync/status`, place a test order, confirm two-way sync.
7. Run the contract migration (drop obsolete columns) only after **all** nodes are
   on the new version.

## 8. Rollback

- **App only:** re-tag the previous image, `up -d`. Safe because migrations are
  expand-only — the old code ignores new columns.
- **Schema:** do **not** auto-down-migrate. If a migration is bad, restore the
  pre-migration backup checkpoint the `migrator` created, then redeploy the prior
  image. The edge keeps operating on its local DB throughout; queued outbox
  events replay on recovery.
- Never `docker compose down -v` on the edge — `-v` destroys `edge-mssql-data`.

## 9. Edge hardware failure

See `FAILURE_RECOVERY.md` §"Edge hardware failure". Summary: stations fail over to
cloud; provision a replacement edge; restore the latest edge SQL backup; let the
worker reconcile the gap from the cloud; verify with the reconciliation report
before resuming local writes.
