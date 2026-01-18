# Cloudflare Tunnel Routes Guide (sanitized)

**Last Updated:** 2026-01-17

This guide intentionally contains **no secret values**. Use `CLOUDFLARE_TUNNEL_TOKEN` from your `.env.production`.

## Recommended hostname → origin mapping (Docker production)

| Hostname | Origin |
|---|---|
| `portal.shahin-ai.com` | `http://127.0.0.1:5000` |
| `app.shahin-ai.com` | `http://127.0.0.1:5000` |
| `login.shahin-ai.com` (if used) | `http://127.0.0.1:5000` |
| `*.shahin-ai.com` | `http://127.0.0.1:5000` |
| `www.shahin-ai.com` | `http://127.0.0.1:3000` |
| `shahin-ai.com` | `http://127.0.0.1:3000` |

## Verification

Local (on the VPS):

```bash
curl -f http://127.0.0.1:5000/health
curl -f http://127.0.0.1:3000/
```

Public (after Cloudflare routes are active):

```bash
curl -I https://portal.shahin-ai.com
curl -I https://www.shahin-ai.com
```

## References

- `docs/production/10-cloudflare-tunnel.md`

