# FoodSync

**Restaurant Management System** — an Orbionix Technologies product.

© 2026 Orbionix Technologies. All rights reserved. Proprietary and
confidential — see [LICENSE](LICENSE).

---

## Components

| Path | Stack | Purpose |
|------|-------|---------|
| `Backend/` | ASP.NET Core 8 · EF Core 8 · SQL Server | REST API, SignalR (`OrderHub`), ESC/POS printing, AI features, **offline-first sync engine** |
| `Frontend/RMS/` | Angular 21 · PrimeNG | Shop-management app (POS, kitchen, inventory, reports, admin) |
| `Frontend/CustomerOrderingWeb/` | Angular 21 | Public customer ordering site |
| `Backend.Tests/` | xUnit | Unit tests (`dotnet test`) |
| `deploy/` , `docker-compose*.yml` | Docker | Cloud + edge deployments, backup |
| `docs/` | — | Architecture, sync protocol, edge deployment, failure recovery, implementation status |

## Run locally

```
start-app.bat            # backend :7122 + RMS :4200 + customer site :4300
```

or see **`How to run.txt`** for the manual three-terminal steps.

## Offline-first / cloud sync

An in-restaurant **edge** node runs the full stack and keeps operating with no
internet, syncing to the **cloud** node when connectivity returns. Design and
status:

- `docs/OFFLINE_FIRST_ARCHITECTURE.md`
- `docs/SYNC_PROTOCOL.md`
- `docs/EDGE_DEPLOYMENT.md`
- `docs/FAILURE_RECOVERY.md`
- `docs/IMPLEMENTATION_STATUS.md` — living checklist / verification log

## Deployment

- **Cloud:** `docker-compose.yml` (Dokploy / Hostinger). Domains:
  `dfcfront.aiotstudio.online` (RMS), `dfc.aiotstudio.online` (customer site).
- **Edge:** `docker-compose.edge.yml` + `.env.edge.example`.
- **Backups:** `deploy/backup/`.

Never commit real secrets — only `*.example` env files.
