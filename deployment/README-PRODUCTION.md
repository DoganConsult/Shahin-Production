# Production Deployment - Quick Reference

## Server Information
- **IP**: 212.147.229.36
- **UUID**: 007d5050-ade4-432d-b2bb-abc85c74a3b2
- **OS**: Ubuntu Server 24.04 LTS (Noble Numbat)
- **SSH User**: root

## 5 Containers to Deploy

| # | Container | Image | Port | Purpose |
|---|-----------|-------|------|---------|
| 1 | `landing` | Next.js | 3000 | Frontend Landing Page |
| 2 | `portal` | ASP.NET Core | 5000 | Backend API |
| 3 | `postgres` | PostgreSQL 16 | 5432 | Database |
| 4 | `redis` | Redis 7 | 6379 | Cache |
| 5 | `nginx` | Nginx | 80, 443 | Reverse Proxy |

## Quick Deploy

```bash
cd deployment
cp .env.production.template .env.production
# Edit .env.production with your values
./deploy-to-production.sh rebuild
```

## Files Created

1. `docker-compose.production-server.yml` - Docker Compose config for all 5 containers
2. `deploy-to-production.sh` - Automated deployment script
3. `nginx/production-212.147.229.36.conf` - Nginx configuration
4. `.env.production.template` - Environment variables template

## Next Steps

1. Copy `.env.production.template` to `.env.production`
2. Update passwords and API keys in `.env.production`
3. Set your SSH key: `export SSH_KEY=~/.ssh/id_ed25519`
4. Run: `./deploy-to-production.sh rebuild`

See `DEPLOYMENT-INSTRUCTIONS.md` for detailed instructions.
