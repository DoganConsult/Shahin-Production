# Production Architecture

**Generated:** 2026-01-16  
**Target:** Single Production VPS with Docker Compose + Nginx Reverse Proxy

---

## Recommended Architecture

**Approach:** Docker Compose with Nginx reverse proxy

**Justification:**
- Repository already contains `docker-compose.production.yml` and `nginx/nginx.conf`
- Single VPS target aligns with Docker Compose (no Kubernetes complexity)
- Nginx provides TLS termination, rate limiting, and security headers
- Existing health checks and volume mounts are Docker-native

---

## Service Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Internet (Port 80/443)                    │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              Nginx Reverse Proxy (Container)                  │
│  - TLS Termination (Let's Encrypt)                           │
│  - Rate Limiting                                             │
│  - Security Headers                                          │
│  - Static File Serving                                       │
└───────────────┬─────────────────────────────────────────────┘
                │
        ┌───────┴────────┬──────────────┬──────────────┐
        │                │              │              │
        ▼                ▼              ▼              ▼
┌──────────────┐ ┌──────────────┐ ┌──────────┐ ┌──────────┐
│   Backend    │ │   Frontend   │ │  Database │ │  Redis   │
│  (ASP.NET)   │ │   (Next.js)  │ │(PostgreSQL)│ │  Cache   │
│              │ │              │ │            │ │          │
│ Port: 8080   │ │ Port: 3000   │ │ Port: 5432│ │ Port:6379│
│ Health: /health│ │ Health: /   │ │ Internal │ │ Internal │
└──────────────┘ └──────────────┘ └──────────┘ └──────────┘
```

---

## Service Details

### 1. Nginx Reverse Proxy
- **Type:** Container (or host-installed)
- **Public Ports:** `80` (HTTP), `443` (HTTPS)
- **Internal Ports:** None (communicates with services via Docker network)
- **Domains:**
  - `app.shahin-ai.com` → Backend API + Frontend
  - `portal.shahin-ai.com` → Backend API + Frontend
  - `shahin-ai.com`, `www.shahin-ai.com` → Landing page (static)
- **SSL/TLS:** Let's Encrypt (certbot) or Traefik ACME
- **Features:**
  - HTTP → HTTPS redirect
  - Rate limiting (login: 10/min, API: 100/min, general: 50/min)
  - Security headers (X-Frame-Options, CSP, HSTS)
  - Gzip compression
  - Static file serving
  - WebSocket support (SignalR `/hubs/`)

### 2. Backend API (ASP.NET Core)
- **Service Name:** `grcmvc-prod` (or `grc-api` for clarity)
- **Internal Port:** `8080` (from `Dockerfile.production` / `ASPNETCORE_URLS`)
- **Public Port:** None (accessed via Nginx)
- **Health Endpoints:**
  - `/health` - Full health check
  - `/health/ready` - Readiness (database checks)
  - `/health/live` - Liveness (API checks)
- **Dependencies:**
  - PostgreSQL (main + auth databases)
  - Redis (caching)
  - RabbitMQ (optional, if MassTransit is required)
- **Volumes:**
  - `/app/backups` - Backup storage
  - `/app/keys` - Data protection keys
  - `/var/www/shahin-ai/storage` - File uploads
  - `/var/log/grc` - Application logs

### 3. Frontend (Next.js)
- **Service Name:** `grc-web` (or `grc-frontend`)
- **Internal Port:** `3000` (Next.js default)
- **Public Port:** None (accessed via Nginx)
- **Build Mode:** `standalone` (from `next.config.js`)
- **Health Check:** Root path `/` (200 OK expected)
- **Dependencies:**
  - Backend API (via internal network)
- **Volumes:**
  - `.next/standalone` - Build output (if needed for debugging)

### 4. Database (PostgreSQL)
- **Service Name:** `db-prod` (or `grc-db`)
- **Internal Port:** `5432`
- **Public Port:** None (internal only, not exposed to host)
- **External Port (if needed for admin):** `5433` (host mapping, firewall-restricted)
- **Databases:**
  - `GrcMvcDb` (main application)
  - `GrcMvcDb_auth` (authentication)
- **Volumes:**
  - `grc_prod_db_data:/var/lib/postgresql/data` - Persistent data
- **Initialization:** `scripts/init-db.sql` (extensions, schemas)

### 5. Redis (Cache)
- **Service Name:** `redis-prod` (or `grc-redis`)
- **Internal Port:** `6379`
- **Public Port:** None (internal only)
- **External Port (if needed for admin):** `6380` (host mapping, firewall-restricted)
- **Volumes:**
  - `grc_prod_redis_data:/data` - Persistent cache data

---

## Network Architecture

### Docker Network
- **Name:** `grc-prod-network` (or `grc-network`)
- **Type:** Bridge (default)
- **Services:** All containers communicate via service names
- **DNS:** Docker's built-in DNS resolution

### Port Mapping Strategy

#### Public Ports (Host)
- `80` → Nginx (HTTP)
- `443` → Nginx (HTTPS)
- `22` → SSH (management)

#### Internal Ports (Docker Network)
- Backend: `8080` (container internal)
- Frontend: `3000` (container internal)
- Database: `5432` (container internal)
- Redis: `6379` (container internal)

#### Optional Admin Ports (Host, Firewall-Restricted)
- `5433` → PostgreSQL (only if external admin tools needed)
- `6380` → Redis (only if external admin tools needed)

---

## Domain Plan

### Primary Domains
- **`app.shahin-ai.com`** → Main application (API + Frontend)
- **`portal.shahin-ai.com`** → Portal/alternative entry (API + Frontend)

### Landing/Marketing
- **`shahin-ai.com`** → Landing page (static files from `/var/www/landing`)
- **`www.shahin-ai.com`** → Landing page (redirects to `shahin-ai.com`)

### Routing Rules (Nginx)
- `/api/*` → Backend API (`grc-api:80`)
- `/health*` → Backend health checks (`grc-api:80`)
- `/auth/*`, `/login` → Backend authentication (`grc-api:80`)
- `/hubs/*` → Backend SignalR WebSocket (`grc-api:80`)
- `/storage/*` → Backend file uploads (`grc-api:80`)
- `/` → Frontend (`grc-web:3000`) or static files

---

## Health Checks

### Backend API
- **Endpoint:** `http://grc-api:80/health`
- **Type:** HTTP GET
- **Expected:** 200 OK with JSON response
- **Interval:** 30 seconds
- **Timeout:** 5 seconds
- **Retries:** 3
- **Start Period:** 60 seconds (allows startup time)

### Frontend
- **Endpoint:** `http://grc-web:3000/`
- **Type:** HTTP GET
- **Expected:** 200 OK
- **Interval:** 30 seconds
- **Timeout:** 5 seconds
- **Retries:** 3
- **Start Period:** 60 seconds

### Database
- **Command:** `pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}`
- **Type:** Exec
- **Interval:** 10 seconds
- **Timeout:** 5 seconds
- **Retries:** 3
- **Start Period:** 30 seconds

### Redis
- **Command:** `redis-cli ping`
- **Type:** Exec
- **Expected:** `PONG`
- **Interval:** 10 seconds
- **Timeout:** 5 seconds
- **Retries:** 3
- **Start Period:** 10 seconds

---

## Data Persistence

### Volumes

#### Database
- **Name:** `grc_prod_db_data`
- **Mount:** `/var/lib/postgresql/data`
- **Purpose:** PostgreSQL data files (persistent)
- **Backup Strategy:** Daily `pg_dump` to `/app/backups` (see Data Plan)

#### Redis
- **Name:** `grc_prod_redis_data`
- **Mount:** `/data`
- **Purpose:** Redis persistence (AOF/RDB)
- **Backup Strategy:** Optional (cache can be rebuilt)

#### Backend Storage
- **Name:** `grc_prod_backups` (or host-mounted)
- **Mount:** `/app/backups`
- **Purpose:** Database backups, file uploads backup
- **Retention:** 30 days (configurable)

#### Backend Keys
- **Name:** `grc_prod_keys` (or host-mounted)
- **Mount:** `/app/keys`
- **Purpose:** Data Protection keys (ASP.NET Core)
- **Security:** Must be backed up securely (encrypted)

#### Backend File Storage
- **Name:** Host-mounted (recommended) or named volume
- **Mount:** `/var/www/shahin-ai/storage`
- **Purpose:** User-uploaded files, evidence documents
- **Backup Strategy:** Included in daily backup

#### Backend Logs
- **Name:** Host-mounted (recommended)
- **Mount:** `/var/log/grc`
- **Purpose:** Application logs (Serilog file sink)
- **Retention:** 30 days (configurable via Serilog)

---

## Alternative Architecture (Systemd/No Docker)

**Status:** Documented as alternative, but **NOT RECOMMENDED** for this deployment.

### Rationale for Docker Compose
- Repository already contains Docker Compose configuration
- Easier dependency management (PostgreSQL, Redis versions)
- Isolated environments (no host package conflicts)
- Simplified rollback (container image tags)
- Health checks and restart policies built-in

### Systemd Alternative (If Required)
- **Backend:** systemd service running `dotnet GrcMvc.dll`
- **Frontend:** systemd service running `node .next/standalone/server.js`
- **Database:** Host-installed PostgreSQL 15
- **Redis:** Host-installed Redis 7
- **Reverse Proxy:** Host-installed Nginx
- **Management:** `systemctl` commands
- **Logs:** `journalctl`

**Note:** This approach requires manual installation of .NET 8.0 runtime, Node.js 18+, PostgreSQL, Redis, and Nginx on the host OS.

---

## Service Dependencies

```
Nginx → Backend API (depends on: Database, Redis)
Nginx → Frontend (depends on: Backend API)
Backend API → Database (required)
Backend API → Redis (required, but can start without)
Backend API → RabbitMQ (optional, if MassTransit is used)
```

### Startup Order
1. Database (PostgreSQL)
2. Redis
3. Backend API (waits for database healthy)
4. Frontend (waits for backend healthy)
5. Nginx (waits for backend and frontend healthy)

---

## Security Boundaries

### Network Isolation
- **Public:** Only Nginx (ports 80/443)
- **Internal:** All services communicate via Docker network
- **Admin Access:** SSH (port 22), optional database/Redis ports (firewall-restricted)

### Container Isolation
- **Non-root users:** All containers run as non-root (`shahin`, `appuser`, or similar)
- **Read-only filesystems:** Where possible (Next.js standalone supports this)
- **Secrets:** Environment variables (never hardcoded)

---

## Scalability Considerations

### Current Design (Single VPS)
- **Limitation:** Single instance of each service
- **Capacity:** Depends on VPS specs (see Environment Spec)

### Future Scaling (If Needed)
- **Horizontal:** Add more backend/frontend containers behind load balancer
- **Database:** Consider managed PostgreSQL (RDS, Azure Database) for high availability
- **Redis:** Consider Redis Cluster or managed Redis
- **Storage:** Consider object storage (S3, Azure Blob) for file uploads

---

## Next Steps

1. Resolve port and service name discrepancies (see Discovery doc)
2. Create unified `docker-compose.prod.yml` with all services
3. Update Nginx config to match service names and ports
4. Implement health checks in compose file
5. Document volume backup and restore procedures

---

**Next Step:** Proceed to STEP 3 - Environment Specification.
