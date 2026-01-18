# Deployment Guide (Docker + Cloudflare Tunnel aligned)

**Last Updated:** 2026-01-17  
**Target:** Production VPS with Docker Compose (optionally fronted by Cloudflare Tunnel)

---

## Deployment choice (recommended)

**Deploy with Docker Compose** using the repo’s production compose file:

- **Compose**: `Shahin-Jan-2026/docker-compose.production.yml`
- **Tunnel**: Cloudflare Tunnel routes to **local loopback ports** (see `10-cloudflare-tunnel.md`)

This keeps **databases/cache private** and only exposes the **portal** and **marketing** locally for the tunnel.

---

## Port map (tunnel-aligned)

These are the ports the repo now standardizes on for production compose:

| Service | Host bind | Container port | Purpose |
|--------|-----------|----------------|---------|
| **Portal (GrcMvc)** | `127.0.0.1:5000` | `8080` | Cloudflare Tunnel origin for `portal/app/*.shahin-ai.com` |
| **Marketing (Next.js)** | `127.0.0.1:3000` | `3000` | Cloudflare Tunnel origin for `www` + root domain |
| **PostgreSQL (optional host admin)** | `127.0.0.1:5433` | `5432` | Optional local psql/admin access |
| **Redis (optional host admin)** | `127.0.0.1:6380` | `6379` | Optional local redis-cli/admin access |

---

## Prerequisites

- **Docker Engine + Docker Compose**
- **Production env file**: `.env.production` (names only; no secrets in git)

---

## Deploy (Linux VPS)

This repo includes a Linux deployment script:

- `Shahin-Jan-2026/deploy-production.sh`

If you deploy manually, use:

```bash
cd /opt/shahin-ai/Shahin-Jan-2026

# Start / update production stack
docker compose -f docker-compose.production.yml --env-file .env.production up -d --build

# Verify local health endpoints (tunnel origins)
curl -f http://127.0.0.1:5000/health
curl -f http://127.0.0.1:3000/
```

---

## Deploy (Windows)

This repo includes a PowerShell deployment script:

- `Shahin-Jan-2026/scripts/deploy-production.ps1`

```powershell
cd C:\Shahin-ai\Shahin-Jan-2026

.\scripts\deploy-production.ps1 setup
.\scripts\deploy-production.ps1 rebuild-full
.\scripts\deploy-production.ps1 deploy

# Verify
Invoke-WebRequest -Uri "http://127.0.0.1:5000/health" -UseBasicParsing
Invoke-WebRequest -Uri "http://127.0.0.1:3000/" -UseBasicParsing
```

---

## Cloudflare Tunnel

Use the tunnel to expose the local origins above (recommended).  
See:

- `Shahin-Jan-2026/docs/production/10-cloudflare-tunnel.md`

---

## Operational commands

```bash
cd /opt/shahin-ai/Shahin-Jan-2026

# Status
docker compose -f docker-compose.production.yml ps

# Logs
docker compose -f docker-compose.production.yml logs -f --tail=200

# Restart
docker compose -f docker-compose.production.yml restart
```

