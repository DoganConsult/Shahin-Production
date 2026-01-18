# Cloudflare Tunnel Setup (token-based, sanitized)

**Last Updated:** 2026-01-17  

This document intentionally contains **no secret values**.

## Secrets (names only)

- `CLOUDFLARE_TUNNEL_TOKEN`

## Production origin ports (Docker)

When running `docker-compose.production.yml`:

- Portal origin: `http://127.0.0.1:5000`
- Marketing origin: `http://127.0.0.1:3000`

## Cloudflare Zero Trust routing

In Cloudflare Zero Trust → Tunnels → your tunnel → **Public Hostnames**, configure:

- `portal.shahin-ai.com` → `http://127.0.0.1:5000`
- `app.shahin-ai.com` → `http://127.0.0.1:5000`
- `login.shahin-ai.com` (if used) → `http://127.0.0.1:5000`
- `*.shahin-ai.com` (tenant subdomains) → `http://127.0.0.1:5000`
- `www.shahin-ai.com` → `http://127.0.0.1:3000`
- `shahin-ai.com` → `http://127.0.0.1:3000`

## References

- `docs/production/05-deployment.md`
- `docs/production/10-cloudflare-tunnel.md`

