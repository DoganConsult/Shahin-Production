# Production Documentation Index

**Project:** Shahin AI GRC Platform  
**Last Updated:** 2026-01-17  
**Target:** Single VPS with Docker Compose + Cloudflare Tunnel (optional direct reverse proxy)

---

## Documentation Status

| Document | Status | Description |
|----------|--------|-------------|
| [00-repo-discovery.md](./00-repo-discovery.md) | ✅ READY | Backend, frontend, database, and infra inventory |
| [01-architecture.md](./01-architecture.md) | ✅ READY | Production architecture with Docker Compose |
| [02-environment-spec.md](./02-environment-spec.md) | ✅ READY | OS, resources, packages, firewall requirements |
| [03-config-and-secrets.md](./03-config-and-secrets.md) | ✅ READY | Environment variables and secrets inventory |
| [04-data-plan.md](./04-data-plan.md) | ✅ READY | Database migrations, backups, restore procedures |
| [05-deployment.md](./05-deployment.md) | ✅ READY | Docker Compose files and deployment commands |
| [06-observability.md](./06-observability.md) | ✅ READY | Logging, metrics, monitoring, alerting |
| [07-security-baseline.md](./07-security-baseline.md) | ✅ READY | SSH, firewall, TLS, secrets handling |
| [08-release-runbook.md](./08-release-runbook.md) | ✅ READY | Deploy, verify, and rollback procedures |
| [09-production-readiness-report.json](./09-production-readiness-report.json) | ⚠️ NOT_YET_READY | Machine-readable production readiness status (per policy gates) |
| [10-cloudflare-tunnel.md](./10-cloudflare-tunnel.md) | ✅ READY | Tunnel-aligned port map + production tunnel setup notes |

---

## Quick Reference

### Stack Summary

| Component | Technology | Version |
|-----------|------------|---------|
| **Backend** | ASP.NET Core | 8.0 |
| **Frontend** | Next.js | 14.2.x |
| **Database** | PostgreSQL | 15 |
| **Cache** | Redis | 7 |
| **Reverse Proxy** | Nginx | 1.25 |
| **Container Runtime** | Docker | 24.x |

### Key URLs

| Environment | URL |
|-------------|-----|
| **Portal** | https://portal.shahin-ai.com |
| **App** | https://app.shahin-ai.com |
| **Landing** | https://www.shahin-ai.com |
| **Health Check** | https://portal.shahin-ai.com/health |

### Key Ports (Local origins for Tunnel)

| Service | Port | Notes |
|---------|------|-------|
| Portal (GrcMvc) | 5000 | **Loopback-only** origin for Cloudflare Tunnel |
| Marketing (Next.js) | 3000 | **Loopback-only** origin for Cloudflare Tunnel |
| PostgreSQL (optional) | 5433 | Optional loopback-only admin access |
| Redis (optional) | 6380 | Optional loopback-only admin access |

---

## Deployment Quick Start

```bash
# 1. Clone and configure
cd /opt
git clone <repository-url> shahin-ai
cd shahin-ai/Shahin-Jan-2026
cp .env.example .env.production
nano .env.production  # Fill in secrets (names only in docs)

# 2. Deploy
docker compose -f docker-compose.production.yml --env-file .env.production up -d --build

# 3. Verify local origins (Tunnel targets)
curl -f http://127.0.0.1:5000/health
curl -f http://127.0.0.1:3000/
```

---

## Required Secrets

See [03-config-and-secrets.md](./03-config-and-secrets.md) for full list.

**Critical secrets (must be set):**
- `DB_PASSWORD` - PostgreSQL password
- `JWT_SECRET` - JWT signing key (64+ chars)
- `AZURE_TENANT_ID` - Azure AD tenant
- `MSGRAPH_CLIENT_SECRET` - Microsoft Graph API

---

## Maintenance Commands

### View Logs
```bash
docker compose -f docker-compose.production.yml logs -f grcmvc-prod
```

### Restart Services
```bash
docker compose -f docker-compose.production.yml restart
```

### Create Backup
```bash
docker compose -f docker-compose.production.yml exec db-prod \
  pg_dump -U "$DB_USER" -d "$DB_NAME" -F c -f /tmp/backup.dump
```

### Health Check
```bash
curl -sf http://127.0.0.1:5000/health
```

---

## Emergency Contacts

| Role | Contact |
|------|---------|
| On-Call | oncall@shahin-ai.com |
| Security | security@shahin-ai.com |
| Database | dba@shahin-ai.com |

---

## Next Actions

### Immediate (Before Go-Live)
- [ ] Configure VPS with Ubuntu 22.04/24.04
- [ ] Install Docker and Docker Compose
- [ ] Configure UFW firewall (22, 80, 443)
- [ ] Set up SSL certificates with Certbot
- [ ] Create `.env.production` with all secrets
- [ ] Deploy and verify health checks

### Post-Launch
- [ ] Set up automated backups (daily)
- [ ] Configure monitoring alerts
- [ ] Schedule regular security updates
- [ ] Document incident response contacts

---

## Related Documentation

- [DEPLOYMENT_GUIDE.md](../../DEPLOYMENT_GUIDE.md) - General deployment guide
- [PRODUCTION_DEPLOYMENT_GUIDE.md](../../PRODUCTION_DEPLOYMENT_GUIDE.md) - Detailed production steps
- [nginx-production.conf](../../nginx-production.conf) - Nginx configuration

---

## Changelog

| Date | Author | Changes |
|------|--------|---------|
| 2026-01-17 | System | Added 05-deployment.md, 06-observability.md, 07-security-baseline.md, 08-release-runbook.md |
| 2026-01-16 | System | Initial documentation (00-04) |
