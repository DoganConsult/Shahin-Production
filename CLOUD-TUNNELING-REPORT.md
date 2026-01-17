# CLOUD TUNNELING REQUIREMENTS REPORT
## Shahin AI GRC Platform - Production Environment

Generated: 2026-01-16
Environment: Production Cloud Deployment

---

## EXECUTIVE SUMMARY

**Total Tunnels Required: 7-9 tunnels** (depending on architecture choice)

The Shahin AI GRC platform requires multiple network tunnels for cloud production deployment to expose services securely while maintaining internal service communication.

---

## TUNNEL REQUIREMENTS BY ARCHITECTURE

### OPTION 1: BASIC PRODUCTION (7 Tunnels)
Based on `docker-compose.production.yml`:

| Service | Port | Tunnel Type | Purpose | External Access |
|---------|------|------------|---------|-----------------|
| Landing Page | 3000 | HTTP/HTTPS | React SPA - www.shahin-ai.com | YES - Public |
| GRC Portal | 5000 | HTTP/HTTPS | ASP.NET Core - portal.shahin-ai.com | YES - Public |
| PostgreSQL | 5432 | TCP | Database server | NO - Internal only |
| Redis | 6379 | TCP | Cache server | NO - Internal only |
| Nginx | 80 | HTTP | Reverse proxy | YES - Public |
| Nginx | 443 | HTTPS | SSL termination | YES - Public |
| Staging | 5001 | HTTP/HTTPS | staging.shahin-ai.com (optional) | YES - Restricted |

**Minimum External Tunnels: 4** (Ports: 80, 443, 3000, 5000)
**Optional: +1** for staging (5001)

---

### OPTION 2: HIGH AVAILABILITY WITH CLOUDFLARE (5 Tunnels)
Based on `docker-compose.ha-cloudflare.yml`:

| Service | Port | Tunnel Type | Purpose | External Access |
|---------|------|------------|---------|-----------------|
| Cloudflare Tunnel | - | Cloudflare | Secure tunnel to edge | MANAGED |
| Traefik | 80 | HTTP | Load balancer | YES - Via Cloudflare |
| Traefik | 443 | HTTPS | SSL termination | YES - Via Cloudflare |
| Traefik Dashboard | 8080 | HTTP | Monitoring UI | YES - Restricted |
| Internal Services | Various | - | All internal via Traefik | NO |

**With Cloudflare: 1 tunnel** (Cloudflare manages all ingress)
**Without Cloudflare: 3 tunnels** (80, 443, 8080)

---

### OPTION 3: FULL STACK ALL ENVIRONMENTS (9 Tunnels)
Based on `docker-compose.all.yml`:

| Environment | Service | Port | Purpose |
|------------|---------|------|---------|
| **Development** | App | 8888 | Dev portal |
| | PostgreSQL | 5432 | Dev database |
| | Redis | 6379 | Dev cache |
| **Staging** | App | 8080 | Staging portal |
| | PostgreSQL | 5434 | Staging database |
| | Redis | 6381 | Staging cache |
| **Production** | App | 80 | Prod portal HTTP |
| | App | 443 | Prod portal HTTPS |
| | PostgreSQL | 5433 | Prod database |
| | Redis | 6380 | Prod cache |

**Total External Tunnels: 9** (if all environments exposed)
**Recommended: 2** (Only expose 80 and 443 for production)

---

## RECOMMENDED ARCHITECTURE

### CLOUDFLARE TUNNEL (Most Secure)
- **1 Cloudflare Tunnel** handles all ingress
- **0 direct port exposure** to internet
- **Benefits:**
  - DDoS protection
  - Zero Trust Network Access
  - Automatic SSL/TLS
  - Geographic load balancing
  - WAF protection included

### Implementation:
```yaml
# Only expose through Cloudflare
cloudflared:
  Routes:
    - www.shahin-ai.com → Landing:3000
    - portal.shahin-ai.com → Portal:5000
    - staging.shahin-ai.com → Staging:5001
```

---

## PORT MAPPING SUMMARY

### External Ports (Need Tunnels):
- **80** - HTTP traffic (redirect to HTTPS)
- **443** - HTTPS traffic (main ingress)
- **3000** - Landing page (if direct access)
- **5000** - Portal (if direct access)
- **5001** - Staging (optional)
- **8080** - Traefik dashboard (admin only)

### Internal Ports (No Tunnels):
- **5432/5433/5434** - PostgreSQL instances
- **6379/6380/6381** - Redis instances
- **Internal Docker network** - Service-to-service

---

## SECURITY RECOMMENDATIONS

1. **Use Cloudflare Tunnel** - Reduces attack surface to 1 managed tunnel
2. **Never expose database ports** (5432-5434) to internet
3. **Never expose Redis ports** (6379-6381) to internet
4. **Use internal Docker networks** for service communication
5. **Implement rate limiting** at edge (Cloudflare/Nginx)
6. **Enable WAF rules** for application protection
7. **Use SSL/TLS everywhere** (managed by Cloudflare)

---

## CURRENT STATUS

### What's Configured:
- Docker Compose files ready
- Service definitions complete
- Network isolation configured
- Health checks defined

### What's Missing:
- **Cloudflare tunnel token** (need to register)
- **SSL certificates** (if not using Cloudflare)
- **Firewall rules** on cloud provider
- **Environment variables** (API keys, passwords)

---

## QUICK START COMMANDS

### Option 1 - Direct Exposure (NOT RECOMMENDED):
```bash
# Opens 7 ports to internet
docker-compose -f deployment/docker-compose.production.yml up -d
# Required tunnels: 80, 443, 3000, 5000, 5001
```

### Option 2 - Cloudflare Tunnel (RECOMMENDED):
```bash
# Install Cloudflare tunnel
cloudflared tunnel create shahin-ai
cloudflared tunnel route dns shahin-ai portal.shahin-ai.com
cloudflared tunnel route dns shahin-ai www.shahin-ai.com

# Start with HA setup
docker-compose -f docker-compose.ha-cloudflare.yml up -d
# Required tunnels: 1 (managed by Cloudflare)
```

### Option 3 - Basic with Nginx:
```bash
# Traditional reverse proxy
docker-compose -f docker-compose.production.yml --profile with-nginx up -d
# Required tunnels: 80, 443 only
```

---

## CONCLUSION

For production deployment in cloud:
- **Minimum tunnels: 2** (80, 443 with Nginx reverse proxy)
- **Typical setup: 4-5** (includes direct service access)
- **Recommended: 1** (Cloudflare Tunnel managing all traffic)

The Cloudflare Tunnel approach provides the best security, performance, and simplicity with only 1 managed tunnel instead of exposing multiple ports directly to the internet.