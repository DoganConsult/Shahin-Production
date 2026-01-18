# Cloudflare Tunnel “Degraded” Fix (sanitized)

**Last Updated:** 2026-01-17

This file intentionally contains **no secret values**.

## Checklist

1) Confirm Docker origins are healthy:

```bash
curl -f http://127.0.0.1:5000/health
curl -f http://127.0.0.1:3000/
```

2) Restart tunnel service (Linux systemd example):

```bash
sudo systemctl restart cloudflared
sudo systemctl status cloudflared --no-pager
```

3) Check tunnel logs:

```bash
sudo journalctl -u cloudflared --since "15 minutes ago" --no-pager
```

## Notes

- Ensure `CLOUDFLARE_TUNNEL_TOKEN` is set in `.env.production` (do not commit values).
- Ensure Cloudflare “Public Hostnames” routes match `docs/production/10-cloudflare-tunnel.md`.

