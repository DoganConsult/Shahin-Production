# Cloudflare Tunnel (Production) — Docker port alignment

**Last Updated:** 2026-01-17  
**Goal:** Publish production services via **Cloudflare Tunnel** while keeping ports **private** on the VPS.

---

## What the tunnel should point to

With `docker-compose.production.yml`, the services are bound to **loopback**:

| Public hostname | Local origin service |
|---|---|
| `portal.shahin-ai.com` | `http://127.0.0.1:5000` |
| `app.shahin-ai.com` | `http://127.0.0.1:5000` |
| `login.shahin-ai.com` (if used) | `http://127.0.0.1:5000` |
| `*.shahin-ai.com` (tenant subdomains) | `http://127.0.0.1:5000` |
| `www.shahin-ai.com` | `http://127.0.0.1:3000` |
| `shahin-ai.com` (root) | `http://127.0.0.1:3000` |

---

## Secrets (names only)

- `CLOUDFLARE_TUNNEL_TOKEN` (required to run the tunnel)

Do **not** commit token values to git.

---

## Deploy order (recommended)

1) Start Docker services:

```bash
docker compose -f docker-compose.production.yml --env-file .env.production up -d --build
```

2) Verify local origins are up:

```bash
curl -f http://127.0.0.1:5000/health
curl -f http://127.0.0.1:3000/
```

3) Configure Cloudflare “Public Hostnames” (Zero Trust UI)

Map the hostnames to the local origins above.

---

## Notes about ports + firewall

- With Cloudflare Tunnel, you can run **without exposing** application ports publicly.
- If you also run direct nginx/https on the VPS, keep that as an explicit, separate choice.

