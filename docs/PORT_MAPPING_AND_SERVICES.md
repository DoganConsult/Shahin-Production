# Shahin AI GRC Platform - Port Mapping & Services Reference

| **Version** | 1.0 |
|-------------|-----|
| **Owner** | DevOps Team |
| **Last Updated** | 2026-01-19 |
| **Status** | Active |

---

## Table of Contents

1. [Environments Overview](#environments-overview)
2. [Port Mapping Tables](#port-mapping-tables)
3. [Quick Reference](#quick-reference)
4. [Port Reservation Policy](#port-reservation-policy)
5. [Access Credentials](#access-credentials)
6. [HTTPS & Certificates](#https--certificates)
7. [Common Tasks](#common-tasks)
8. [Troubleshooting](#troubleshooting)
9. [Kafka Connect Plugins](#kafka-connect-plugins)

---

## Environments Overview

| Environment | Purpose | Compose File | Network |
|-------------|---------|--------------|---------|
| **Development** | Full stack with all services | `docker-compose.yml` | `grc-network` |
| **Staging** | Pre-production testing | `docker-compose.staging.yml` | `grc-staging-network` |
| **Production** | Live deployment | `docker-compose.production.yml` | `grc-prod-network` |

### Environment Isolation

- **Development**: Uses default ports (8888, 5432, 6379)
- **Staging**: Uses offset ports to avoid conflicts (8080, 5434, 6381)
- **Production**: Cloud-hosted, internal networking only

---

## Port Mapping Tables

### Staging Environment

| Service | Container | Host Port | Protocol | URL |
|---------|-----------|-----------|----------|-----|
| GrcMvc App | 80 | **8080** | HTTP | http://localhost:8080 |
| PostgreSQL | 5432 | **5434** | TCP | localhost:5434 |
| Redis | 6379 | **6381** | TCP | localhost:6381 |

### Development Environment (Full Stack)

#### Core Application

| Service | Container | Host Port | Protocol | URL |
|---------|-----------|-----------|----------|-----|
| GrcMvc App | 80 | **8888** | HTTP | http://localhost:8888 |
| GrcMvc App (HTTPS) | 443 | **8443** | HTTPS | https://localhost:8443 |

#### Databases

| Service | Container | Host Port | Protocol | URL |
|---------|-----------|-----------|----------|-----|
| PostgreSQL | 5432 | **5432** | TCP | localhost:5432 |
| ClickHouse HTTP | 8123 | **8123** | HTTP | http://localhost:8123 |
| ClickHouse Native | 9000 | **9000** | TCP | localhost:9000 |
| Redis | 6379 | **6379** | TCP | localhost:6379 |

#### Message Queue (Kafka)

| Service | Container | Host Port | Protocol | URL |
|---------|-----------|-----------|----------|-----|
| Kafka External | 9092 | **9092** | TCP | localhost:9092 |
| Kafka Internal | 29092 | **29092** | TCP | localhost:29092 |
| Kafka Connect | 8083 | **8083** | HTTP | http://localhost:8083 |
| Zookeeper | 2181 | **2181** | TCP | localhost:2181 |

#### UI & Monitoring (9xxx and 3xxx range)

| Service | Container | Host Port | Protocol | URL |
|---------|-----------|-----------|----------|-----|
| Kafka UI | 8080 | **9080** | HTTP | http://localhost:9080 |
| Grafana | 3000 | **3030** | HTTP | http://localhost:3030 |
| Metabase | 3000 | **3033** | HTTP | http://localhost:3033 |
| Superset | 8088 | **8088** | HTTP | http://localhost:8088 |

#### Workflow & Automation

| Service | Container | Host Port | Protocol | URL |
|---------|-----------|-----------|----------|-----|
| Camunda BPM | 8080 | **8085** | HTTP | http://localhost:8085 |
| n8n | 5678 | **5678** | HTTP | http://localhost:5678 |

#### Optional Tooling Services (profile: tooling)

| Service | Container | Host Port | Protocol | URL |
|---------|-----------|-----------|----------|-----|
| pgAdmin | 80 | **5050** | HTTP | http://localhost:5050 |
| RedisInsight | 8001 | **8001** | HTTP | http://localhost:8001 |
| Mailpit | 8025 | **8025** | HTTP | http://localhost:8025 |
| Mailpit SMTP | 1025 | **1025** | SMTP | localhost:1025 |
| Adminer | 8080 | **8081** | HTTP | http://localhost:8081 |

#### Optional Observability Services (profile: observability)

| Service | Container | Host Port | Protocol | URL |
|---------|-----------|-----------|----------|-----|
| Prometheus | 9090 | **9090** | HTTP | http://localhost:9090 |
| Loki | 3100 | **3100** | HTTP | http://localhost:3100 |
| Tempo | 3200 | **3200** | HTTP | http://localhost:3200 |
| Tempo OTLP | 4317 | **4317** | gRPC | localhost:4317 |

#### Optional Kafka Extras (profile: kafka-extras)

| Service | Container | Host Port | Protocol | URL |
|---------|-----------|-----------|----------|-----|
| Schema Registry | 8081 | **8082** | HTTP | http://localhost:8082 |
| Kafdrop | 9000 | **9001** | HTTP | http://localhost:9001 |

> **Note:** Schema Registry uses host port 8082 to avoid collision with Adminer (8081).

---

## Quick Reference

```
STAGING (lean, for testing):
  App:        http://localhost:8080
  Database:   localhost:5434
  Redis:      localhost:6381

DEVELOPMENT (full stack):
  App:        http://localhost:8888
  App HTTPS:  https://localhost:8443
  Database:   localhost:5432
  Redis:      localhost:6379

MONITORING & UIs:
  Kafka UI:   http://localhost:9080
  Grafana:    http://localhost:3030
  Superset:   http://localhost:8088
  Metabase:   http://localhost:3033

WORKFLOW:
  Camunda:    http://localhost:8085
  n8n:        http://localhost:5678

TOOLING (optional):
  pgAdmin:    http://localhost:5050
  RedisInsight: http://localhost:8001
  Mailpit:    http://localhost:8025
```

---

## Port Reservation Policy

### Reserved Port Ranges

| Range | Purpose | Examples |
|-------|---------|----------|
| **8080-8099** | Application HTTP | 8080 (staging), 8083 (Kafka Connect), 8085 (Camunda), 8088 (Superset) |
| **8443** | Application HTTPS | 8443 (GrcMvc HTTPS) |
| **8888** | Development App | 8888 (GrcMvc Dev) |
| **3000-3099** | BI & Dashboards | 3030 (Grafana), 3033 (Metabase) |
| **5xxx** | Databases & Admin | 5432 (PostgreSQL), 5050 (pgAdmin), 5678 (n8n) |
| **6xxx** | Cache | 6379 (Redis), 6381 (Redis Staging) |
| **9xxx** | Kafka & Monitoring | 9080 (Kafka UI), 9090 (Prometheus), 9092 (Kafka) |

### Rules

1. **Do not reuse reserved ports** for local development tools
2. **All UIs** should be in 3xxx or 9xxx range for consistency
3. **Staging offsets**: Add +2 to database ports (5432→5434, 6379→6381)
4. **Document any new port** in this file before adding to compose

---

## Access Credentials

### Location

Credentials are stored in environment files. **Never commit secrets to git.**

| Environment | File | Template |
|-------------|------|----------|
| Development | `.env` | `.env.example` |
| Staging | `.env.staging` | `.env.staging.example` |
| Production | Vault / 1Password | N/A |

### Default Development Credentials

See `.env.example` for template. Key variables:

```bash
# Database
DB_USER=postgres
DB_PASSWORD=<see .env>
DB_NAME=GrcMvcDb

# Grafana
GRAFANA_ADMIN_USER=admin
GRAFANA_ADMIN_PASSWORD=<see .env>

# Superset
SUPERSET_ADMIN_USER=admin
SUPERSET_ADMIN_PASSWORD=<see .env>

# Camunda
# Default: demo/demo

# n8n
N8N_USER=<see .env>
N8N_PASSWORD=<see .env>
```

---

## HTTPS & Certificates

### Port 8443 - Self-Signed Certificate

The development HTTPS endpoint (https://localhost:8443) uses a **self-signed certificate**.

#### Trust the Certificate

**Windows:**
1. Navigate to https://localhost:8443 in Chrome
2. Click "Not Secure" → Certificate → Details → Export
3. Run `certmgr.msc` → Trusted Root Certification Authorities → Import

**macOS:**
```bash
# Export cert from browser, then:
sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain localhost.crt
```

**Linux (Ubuntu/Debian):**
```bash
sudo cp localhost.crt /usr/local/share/ca-certificates/
sudo update-ca-certificates
```

#### For Production

Production uses Cloudflare Tunnel with automatic TLS - no self-signed certs.

---

## Common Tasks

### Start/Stop Commands

```bash
# Development - Full stack
docker compose up -d
docker compose down

# Development - With tooling
docker compose --profile tooling up -d

# Development - With observability
docker compose --profile observability up -d

# Staging
docker compose -f docker-compose.staging.yml up -d
docker compose -f docker-compose.staging.yml down

# Production
docker compose -f docker-compose.production.yml up -d
```

### Reset Database

```bash
# Stop services
docker compose down

# Remove volume (WARNING: destroys all data)
docker volume rm shahin-jan-2026_grc_db_data

# Restart (will recreate DB)
docker compose up -d
```

### View Logs

```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f grcmvc
docker compose logs -f db
docker compose logs -f kafka

# Last 100 lines
docker compose logs --tail=100 grcmvc
```

### Rebuild After Code Changes

```bash
# Rebuild and restart app only
docker compose up -d --build grcmvc

# Force rebuild without cache
docker compose build --no-cache grcmvc
docker compose up -d
```

---

## Troubleshooting

### "Port already in use"

**Symptoms:** `bind: address already in use`

**Solution:**
```bash
# Find process using port (example: 8080)
# Windows PowerShell:
netstat -ano | findstr :8080
taskkill /PID <PID> /F

# Linux/macOS:
lsof -i :8080
kill -9 <PID>

# Or change port in .env:
APP_PORT=8889
```

### "Kafka UI shows no brokers"

**Symptoms:** Kafka UI loads but shows "No brokers available"

**Causes & Solutions:**

1. **Kafka not ready yet** - Wait 60s after `docker compose up`
2. **Zookeeper unhealthy** - Check: `docker compose logs zookeeper`
3. **Network issue** - Verify both are on same network:
   ```bash
   docker network inspect shahin-jan-2026_grc-network
   ```

### "App can't reach DB/Redis"

**Symptoms:** Connection refused errors in app logs

**Checklist:**

1. **Check service health:**
   ```bash
   docker compose ps
   # All services should show "healthy" or "running"
   ```

2. **Verify network:**
   ```bash
   # From app container, can you reach db?
   docker compose exec grcmvc ping db
   ```

3. **Check connection string:**
   ```bash
   # Ensure using service name, not localhost
   # CORRECT: Host=db;Port=5432
   # WRONG:   Host=localhost;Port=5432
   ```

4. **DB not initialized:**
   ```bash
   docker compose logs db | grep "ready to accept connections"
   ```

### "ClickHouse connection failed"

**Symptoms:** Analytics queries fail

**Solution:**
```bash
# Check ClickHouse is running
docker compose logs clickhouse

# Test connection
curl http://localhost:8123/ping
# Should return "Ok."
```

### "Camunda workflow not starting"

**Symptoms:** BPM processes stuck

**Solution:**
```bash
# Camunda needs ~2 minutes to start
docker compose logs camunda

# Access admin at http://localhost:8085/camunda
# Default login: demo/demo
```

---

## Kafka Connect Plugins

### Installed Connectors

| Connector | Version | Purpose |
|-----------|---------|---------|
| Debezium PostgreSQL | 2.5 | CDC from PostgreSQL |
| Debezium MySQL | 2.5 | CDC from MySQL (if needed) |

### Plugin Location

Connectors are mounted from: `./etc/debezium-connectors/`

### List Active Connectors

```bash
curl http://localhost:8083/connectors
```

### Known Connector Configurations

| Name | Source | Destination | Status |
|------|--------|-------------|--------|
| `grc-postgres-source` | PostgreSQL | Kafka topics | Active |

### Adding New Connectors

1. Download connector JAR to `./etc/debezium-connectors/`
2. Restart Kafka Connect: `docker compose restart kafka-connect`
3. Register via REST API:
   ```bash
   curl -X POST http://localhost:8083/connectors \
     -H "Content-Type: application/json" \
     -d @connector-config.json
   ```

---

## Cloudflare Tunnel Configuration

### Windows Service Config Location

When running `cloudflared` as a Windows service:

```
C:\Windows\System32\config\systemprofile\.cloudflared\config.yml
```

For interactive/user mode:
```
C:\Users\<username>\.cloudflared\config.yml
```

### Public Hostname Mapping

| Public Domain | Internal Service | Access Policy |
|---------------|------------------|---------------|
| portal.shahin-ai.com | http://localhost:8080 | Public (app auth) |
| grafana.shahin-ai.com | http://localhost:3030 | **Require CF Access** |
| analytics.shahin-ai.com | http://localhost:8088 | **Require CF Access** |
| metabase.shahin-ai.com | http://localhost:3033 | **Require CF Access** |
| workflow.shahin-ai.com | http://localhost:8085 | **Require CF Access** |
| automation.shahin-ai.com | http://localhost:5678 | **Require CF Access** |
| kafka.shahin-ai.com | http://localhost:9080 | **Require CF Access** |

### Security: Cloudflare Access Policies

**All admin tools MUST be protected with Cloudflare Access.**

#### Setup in Zero Trust Dashboard

1. Go to **Access → Applications**
2. Click **Add an application** → **Self-hosted**
3. Configure:
   - Application name: e.g., "Grafana"
   - Session duration: 24 hours
   - Application domain: grafana.shahin-ai.com

4. Create Access Policy:
   ```
   Policy name: Team Access
   Action: Allow
   Include:
     - Emails ending in: @yourdomain.com
   Require:
     - One-time PIN (or your Identity Provider)
   ```

5. Repeat for each admin hostname.

#### Recommended Access Levels

| Service | Who Should Access | MFA Required |
|---------|-------------------|--------------|
| Portal | All users | App-level |
| Grafana | DevOps, SRE | Yes |
| Superset/Metabase | Analysts, DevOps | Yes |
| Camunda | Developers, Admins | Yes |
| n8n | Developers, Admins | Yes |
| Kafka UI | DevOps only | Yes |

### Go-Live Checklist

```bash
# 1. Install service with token (Admin CMD)
cloudflared.exe service install <YOUR_TOKEN>

# 2. Copy config to system profile
copy config.yml C:\Windows\System32\config\systemprofile\.cloudflared\config.yml

# 3. Verify service is running
sc query cloudflared

# 4. Check tunnel status
cloudflared tunnel list

# 5. Test each hostname
curl -I https://portal.shahin-ai.com
```

### Troubleshooting 404 Errors

If you get 404 after tunnel setup:

1. **Check ingress order** - Most specific hostnames first
2. **Verify hostname match** - Exact match required (no wildcards by default)
3. **Confirm DNS** - In Cloudflare DNS, CNAME should point to `<tunnel-id>.cfargotunnel.com`
4. **Check Zero Trust** - Public hostname must exist in tunnel config

---

## Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-01-19 | DevOps | Initial documentation |
