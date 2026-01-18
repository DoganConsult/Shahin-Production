# Status Report Template (sanitized)

**Last Updated:** 2026-01-17

This file intentionally contains **no secret values**.

## Docker production status

- Compose: `docker-compose.production.yml`
- Portal origin: `http://127.0.0.1:5000`
- Marketing origin: `http://127.0.0.1:3000`

### Commands

```bash
docker compose -f docker-compose.production.yml ps
docker compose -f docker-compose.production.yml logs -f --tail=200
curl -f http://127.0.0.1:5000/health
curl -f http://127.0.0.1:3000/
```

## Cloudflare Tunnel status

- Secret name: `CLOUDFLARE_TUNNEL_TOKEN`
- Verify tunnel in Cloudflare Zero Trust dashboard (Tunnels → Status)

