# Repository Discovery Report

**Generated:** 2026-01-16  
**Purpose:** Comprehensive inventory of backend, frontend, database, and infrastructure components for production deployment planning.

---

## 1. Backend (.NET)

### Solution Structure
- **Solution File:** `GrcMvc.sln`
- **Project:** `src/GrcMvc/GrcMvc.csproj`
- **Target Framework:** `.NET 8.0` (`net8.0`)
- **Hosting Model:** ASP.NET Core Web API (Kestrel)

### Configuration Files

#### `appsettings.json` (Base Configuration)
- **Base URL:** `https://portal.shahin-ai.com`
- **Connection Strings:**
  - `DefaultConnection` (main GRC database)
  - `GrcAuthDb` (authentication database)
  - `Redis` (caching)
  - `HangfireConnection` (background jobs)
- **JWT Settings:**
  - `Secret`: `${JWT_SECRET}` (environment variable)
  - `Issuer`: `GrcSystem`
  - `Audience`: `GrcSystemUsers`
- **SMTP Settings:**
  - `Host`: `smtp.office365.com`
  - `Port`: `587`
  - `EnableSsl`: `true`
  - `FromEmail`: `info@shahin-ai.com`
  - `Username`: `info@shahin-ai.com`
  - `Password`: (empty, expects env var)
- **CORS Allowed Origins:**
  - `https://portal.shahin-ai.com`
  - `https://www.shahin-ai.com`
  - `http://localhost:3000` (development)
- **Logging:**
  - Default: `Information`
  - `Microsoft.AspNetCore`: `Warning`
  - Serilog: Console + File (`logs/grc-system-.log`)

#### `appsettings.Production.json` (Production Overrides)
- **Connection Strings:** All use environment variable placeholders:
  - `"${ConnectionStrings__DefaultConnection}"`
  - `"${ConnectionStrings__GrcAuthDb}"`
  - `"${ConnectionStrings__Redis}"`
  - `"${ConnectionStrings__HangfireConnection}"`
- **Kestrel Endpoints:**
  - `Http.Url`: `http://0.0.0.0:80`
- **JWT Settings:**
  - `Secret`: `"${JWT_SECRET}"`
  - `Issuer`: `ShahinAI`
  - `Audience`: `ShahinAIUsers`
- **Logging:**
  - Default: `Warning`
  - `Microsoft.AspNetCore`: `Warning`
  - `Microsoft.EntityFrameworkCore`: `Warning`
- **CORS Allowed Origins:**
  - `https://shahin-ai.com`
  - `https://www.shahin-ai.com`
  - `https://portal.shahin-ai.com`
- **File Storage:**
  - `Provider`: `LocalFileSystem`
  - `BasePath`: `/var/www/shahin-ai/storage`
- **Third-Party Services:** All configured via environment variables:
  - SMTP, Microsoft Graph, Claude AI, Azure AD, Kafka, Camunda

### Kestrel Configuration
- **Production Port:** `80` (internal, behind reverse proxy)
- **Development Port:** `8080` (from `Dockerfile.production`)
- **URL Binding:** `http://0.0.0.0:80` (production), `http://+:8080` (Dockerfile)
- **HTTPS:** Handled by reverse proxy (Nginx)

### Authentication
- **Primary:** JWT Bearer Authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Identity Framework:** ASP.NET Core Identity (`Microsoft.AspNetCore.Identity.EntityFrameworkCore`)
- **Identity Database:** Separate `GrcAuthDbContext` (isolated from main app data)
- **User Model:** `ApplicationUser` (extends `IdentityUser` with custom properties)
- **Token Provider:** OpenIddict (`Volo.Abp.OpenIddict.Domain`, `Volo.Abp.OpenIddict.EntityFrameworkCore`)
- **External Providers:** Microsoft Graph (Azure AD) configured

### Entity Framework Core

#### DbContexts
1. **`GrcDbContext`** (`src/GrcMvc/Data/GrcDbContext.cs`)
   - Inherits: `AbpDbContext<GrcDbContext>`
   - Purpose: Main application database (tenants, onboarding, compliance, GRC entities)
   - Multi-tenancy: Global query filters using `GetCurrentTenantId()`
   - Multi-workspace: Global query filters using `GetCurrentWorkspaceId()`
   - Connection String: `DefaultConnection`

2. **`GrcAuthDbContext`** (`src/GrcMvc/Data/GrcAuthDbContext.cs`)
   - Inherits: `IdentityDbContext<ApplicationUser>`
   - Purpose: Authentication and identity data (users, roles, tokens, audit logs)
   - Connection String: `GrcAuthDb`
   - Tables: `AspNetUsers`, `AspNetRoles`, `PasswordHistory`, `RefreshTokens`, `LoginAttempts`, `AuthenticationAuditLogs`

#### Migrations
- **Location:** `src/GrcMvc/Migrations/`
- **Migration Command:** `dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --context <DbContextName>`
- **Contexts:** Separate migrations for `GrcDbContext` and `GrcAuthDbContext`
- **Auto-Migration:** `Program.cs` applies migrations on startup in production using `Migrate()`
- **Critical Note:** `GrcAuthDbContext` MUST use `Migrate()`, not `EnsureCreated()` (see `docs/IDENTITY_SCHEMA_SAFEGUARDS.md`)

#### Seeding
- **Script:** `scripts/deploy-and-seed.sh`
- **Process:** Build → Migrate → Seed PlatformAdmin user
- **Hosted Service:** `UserSeedingHostedService` (`src/GrcMvc/Data/UserSeedingHostedService.cs`)

### Background Services

#### Hangfire
- **Package:** `Hangfire.Core` (1.8.14), `Hangfire.PostgreSql` (1.20.9)
- **Storage:** PostgreSQL (`HangfireConnection` connection string)
- **Schema:** `hangfire` (created by `init-db.sql`)
- **Configuration:** `Program.cs` configures `PostgreSqlStorage`

#### MassTransit (Message Queue)
- **Package:** `MassTransit` (8.1.3), `MassTransit.RabbitMQ` (8.1.3)
- **Broker:** RabbitMQ
- **Configuration:** `Program.cs` configures RabbitMQ connection

#### Kafka
- **Package:** `Confluent.Kafka` (2.3.0)
- **Consumer Service:** `KafkaConsumerService` (`src/GrcMvc/Services/Kafka/KafkaConsumerService.cs`)
- **Type:** `BackgroundService`

#### Other Hosted Services
- `UserSeedingHostedService` (IHostedService)
- `SecurityAuditService` (IHostedService)
- `OnboardingServicesStartupValidator` (IHostedService)
- `ConfigurationValidator` (IHostedService)
- `PolicyStore` (IHostedService)

### Health Checks
- **Package:** `AspNetCore.HealthChecks.NpgSql`, `AspNetCore.HealthChecks.Hangfire`, `AspNetCore.HealthChecks.Redis`
- **Endpoints:**
  - `/health` - Full health check (all services)
  - `/health/ready` - Readiness probe (database-dependent checks)
  - `/health/live` - Liveness probe (API-only checks)
- **Response Format:** JSON with status, timestamp, version, and individual check results

### Logging
- **Framework:** Serilog (`Serilog.AspNetCore`)
- **Sinks:**
  - Console
  - File (`logs/grc-system-.log` with rolling)
- **Application Insights:** `Microsoft.ApplicationInsights.AspNetCore` (2.22.0) - APM integration

### Key Dependencies
- **ABP Framework:** `Volo.Abp.*` (8.2.2) - Multi-tenancy, audit logging, feature management
- **AutoMapper:** (13.0.1) - Object mapping
- **Polly:** Resilience and retry policies
- **Rate Limiting:** `Microsoft.AspNetCore.RateLimiting`
- **Data Protection:** `Microsoft.AspNetCore.DataProtection` (keys persisted to file system)
- **Redis:** `Microsoft.Extensions.Caching.StackExchangeRedis` (10.0.1)

---

## 2. Frontend (Next.js)

### Project Structure
- **Location:** `shahin-ai-website/`
- **Package Manager:** npm (inferred from `package.json` structure)
- **Version:** Next.js `^14.0.4`
- **React:** `^18.2.0`

### Build Configuration

#### `package.json`
- **Scripts:**
  - `dev`: Development server
  - `build`: Production build
  - `start`: Production server
  - `lint`: Linting
- **Dependencies:**
  - `next`: `^14.0.4`
  - `react`: `^18.2.0`
  - `react-dom`: `^18.2.0`
- **Dev Dependencies:**
  - TypeScript types (`@types/node`, `@types/react`, `@types/react-dom`)
  - Tailwind CSS (`tailwindcss`, `postcss`, `autoprefixer`)

#### `next.config.js`
- **Output Mode:** `standalone` (production-optimized build)
- **Image Domains:**
  - `portal.shahin-ai.com`
  - `app.shahin-ai.com`
- **Redirects:**
  - `/login` → `https://app.shahin-ai.com/Account/Login` (permanent 308)
- **i18n:**
  - Locales: `['en', 'ar']`
  - Default: `ar` (Arabic)
  - Locale Detection: Enabled
- **React Strict Mode:** Enabled

### Environment Variables
- **Public Variables:** `NEXT_PUBLIC_*` (exposed to browser)
- **Server Variables:** Variables without `NEXT_PUBLIC_` prefix (server-only)
- **Status:** `.env.example` files exist but are filtered by `.gitignore` (cannot inspect)

### API Integration
- **Base URL:** Not explicitly defined in `next.config.js`
- **Authentication:** Redirects to backend (`app.shahin-ai.com/Account/Login`)
- **Inference:** Frontend appears to be a marketing/landing site that redirects to the ASP.NET Core backend for authentication and main application

### Build Artifacts
- **Standalone Output:** `.next/standalone/` (contains minimal runtime)
- **Static Assets:** `.next/static/` (JS, CSS, images)

---

## 3. Database (PostgreSQL)

### Version
- **Docker Image:** `postgres:15-alpine` (from `docker-compose.production.yml`)

### Connection Strings

#### Main Database (`GrcDbContext`)
- **Key:** `DefaultConnection` or `ConnectionStrings__DefaultConnection`
- **Format:** `Host={host};Port={port};Database={dbname};Username={user};Password={password};Include Error Detail=true`
- **Example Pattern:** `Host=db-prod;Port=5432;Database=GrcMvcDb;Username=postgres;Password=${DB_PASSWORD}`

#### Auth Database (`GrcAuthDbContext`)
- **Key:** `GrcAuthDb` or `ConnectionStrings__GrcAuthDb`
- **Format:** Same as above, but database name typically `{main_db_name}_auth`
- **Example Pattern:** `Host=db-prod;Port=5432;Database=GrcMvcDb_auth;Username=postgres;Password=${DB_PASSWORD}`

### Initialization

#### `scripts/init-db.sql`
- **Purpose:** Initialize database with extensions and schemas
- **Extensions:**
  - `uuid-ossp` (UUID generation)
  - `pg_trgm` (text search)
  - `pgcrypto` (encryption)
- **Schemas:**
  - `public` (default)
  - `audit` (audit logs)
  - `analytics` (analytics data)
  - `integration` (integration data)
  - `hangfire` (Hangfire background jobs)
- **Auth Database:** Attempts to create `{main_db}_auth` database (may require manual creation if `dblink` unavailable)
- **Mount Point:** `/docker-entrypoint-initdb.d/01-init.sql` (runs on first container start)

### Migrations
- **Tool:** EF Core Migrations
- **Command Pattern:**
  ```bash
  dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --context GrcDbContext
  dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --context GrcAuthDbContext
  ```
- **Timing:** Applied automatically on startup in production (`Program.cs`)

### Backup Strategy
- **Current Status:** Not explicitly implemented in discovery
- **Tool:** `pg_dump` (standard PostgreSQL utility)
- **MISSING:** Automated backup script, retention policy, restore procedure

---

## 4. Infrastructure

### Docker Configuration

#### `docker-compose.production.yml`
- **Services:**
  - `grcmvc-prod` (backend API)
  - `db-prod` (PostgreSQL)
  - `redis-prod` (Redis cache)
  - `marketing-prod` (Next.js marketing)
- **Backend Service:**
  - **Dockerfile Reference:** `src/GrcMvc/Dockerfile.production`
  - **Ports:** `127.0.0.1:5000:8080` (loopback host → container)
  - **Environment File:** `.env.production`
  - **Health Check:** `curl -f http://localhost:8080/health` (container internal)
  - **Volumes:**
    - `grc_prod_backups:/app/backups`
    - `grc_prod_keys:/app/keys`
  - **Depends On:** `db-prod` (healthy), `redis-prod` (started)
- **Database Service:**
  - **Image:** `postgres:15-alpine`
  - **Ports:** `5433:5432` (external:internal)
  - **Volumes:**
    - `grc_prod_db_data:/var/lib/postgresql/data`
    - `./scripts/init-db.sql:/docker-entrypoint-initdb.d/01-init.sql:ro`
  - **Health Check:** `pg_isready -U ${DB_USER:-grc_admin} -d ${DB_NAME:-GrcMvcDb}`
- **Redis Service:**
  - **Image:** `redis:7-alpine`
  - **Ports:** `6380:6379` (external:internal)
  - **Volumes:** `grc_prod_redis_data:/data`
- **Network:** `grc-prod-network`

#### `Dockerfile.production` (Backend)
- **Build Stage:** `mcr.microsoft.com/dotnet/sdk:8.0`
- **Runtime Stage:** `mcr.microsoft.com/dotnet/aspnet:8.0`
- **User:** `shahin` (non-root)
- **Exposed Port:** `8080`
- **Environment:** `ASPNETCORE_URLS=http://+:8080`, `ASPNETCORE_ENVIRONMENT=Production`
- **Health Check:** `curl -f http://localhost:8080/health`
- **Directories:**
  - `/app/certificates`
  - `/var/www/shahin-ai/storage`
  - `/var/log/grc`

#### `Dockerfile` (Backend - Development)
- **Build Stage:** `mcr.microsoft.com/dotnet/sdk:8.0`
- **Runtime Stage:** `mcr.microsoft.com/dotnet/aspnet:8.0`
- **User:** `appuser` (non-root)
- **Directories:** `/app/keys`, `/app/backups`, `/app/data`, `/app/logs`
- **Note:** Includes "MANDATORY QUALITY CHECK" steps

#### Frontend Dockerfile
- **Status:** ⚠️ **MISSING** - No Dockerfile found for Next.js frontend

### Reverse Proxy

#### `nginx/nginx.conf`
- **Type:** Nginx reverse proxy
- **Upstream:** `grc_backend` → `grc-app:5001` ⚠️ **DISCREPANCY:** Port 5001 not found in compose files
- **Domains:**
  - `app.shahin-ai.com` (main application)
  - `portal.shahin-ai.com` (portal)
  - `shahin-ai.com`, `www.shahin-ai.com` (landing page)
- **SSL:**
  - Certificates: `/etc/nginx/ssl/fullchain.pem`, `/etc/nginx/ssl/privkey.pem`
  - Protocols: TLSv1.2, TLSv1.3
  - Modern cipher suites
  - OCSP stapling, HSTS
- **Rate Limiting:**
  - `login_limit` (10 req/min)
  - `api_limit` (100 req/min)
  - `general_limit` (50 req/min)
- **Location Blocks:**
  - `/health` → Backend health endpoint
  - `^/(auth|login)` → Authentication routes
  - `/api/` → API routes
  - `/hubs/` → SignalR WebSocket support
  - `/storage/` → File uploads
  - Static files → Served directly
- **Security Headers:**
  - X-Frame-Options, X-Content-Type-Options, X-XSS-Protection
  - Referrer-Policy, Content-Security-Policy

### CI/CD Pipelines

#### `.github/workflows/`
- **Files Found:**
  - `ci-cd-pipeline.yml`
  - `auto-promote.yml`
  - `k8s-deploy.yml`
  - `quality-check.yml`
- **Status:** ⚠️ **NOT REVIEWED** - Kubernetes deployment suggests container orchestration, but VPS deployment target is Docker Compose

### Deployment Scripts

#### `scripts/deploy-and-seed.sh`
- **Purpose:** Local deployment and database seeding
- **Steps:**
  1. Clean, restore, build (`dotnet clean/restore/build`)
  2. Apply migrations for both contexts
  3. Seed PlatformAdmin user
- **Note:** Designed for local/development use

### Health Endpoints
- **Backend:**
  - `/health` - Full health check
  - `/health/ready` - Readiness (database checks)
  - `/health/live` - Liveness (API checks)
- **Frontend:** ⚠️ **NOT IDENTIFIED** - No health endpoint found for Next.js app

### Logging Approach
- **Backend:**
  - Serilog → Console + File (`logs/grc-system-.log`)
  - Application Insights (APM)
  - Docker logs (when containerized)
- **Frontend:** ⚠️ **NOT IDENTIFIED** - No logging configuration found

---

## 5. Missing Items & Discrepancies

### Critical Discrepancies

1. **Dockerfile Mismatch:**
   - `docker-compose.production.yml` references `src/GrcMvc/Dockerfile`
   - `src/GrcMvc/Dockerfile.production` exists but is not referenced
   - **Action Required:** Align compose file to use `Dockerfile.production` or rename/merge files

2. **Port Inconsistencies:**
   - `docker-compose.production.yml`: Backend exposes `80` internally
   - `Dockerfile.production`: Exposes `8080`
   - `nginx.conf`: Upstream points to `grc-app:5001`
   - **Action Required:** Standardize internal port (recommend 80 or 8080) and update nginx upstream

3. **Frontend Dockerfile:**
   - **MISSING:** No Dockerfile for Next.js frontend
   - **Action Required:** Create multi-stage Dockerfile for Next.js standalone build

4. **Service Name Mismatch:**
   - `nginx.conf` references `grc-app:5001`
   - `docker-compose.production.yml` service is `grcmvc-prod`
   - **Action Required:** Align service names or update nginx config

### Missing Components

1. **Frontend Deployment:**
   - No Dockerfile for Next.js
   - No production build script in compose
   - No health check endpoint

2. **Backup Automation:**
   - No automated backup script
   - No retention policy
   - No restore procedure documented

3. **Environment Variables:**
   - `.env.production.example` not found (filtered by gitignore)
   - **Action Required:** Create comprehensive `.env.production.example` with all required variables

4. **Monitoring:**
   - No metrics collection configured
   - No alerting thresholds defined
   - No log aggregation setup

5. **SSL/TLS Automation:**
   - Nginx config references certificate paths but no certbot/Let's Encrypt automation found
   - **Action Required:** Document certbot setup or use Traefik with ACME

---

## 6. Recommendations for Production

### Immediate Actions
1. Resolve Dockerfile and port discrepancies
2. Create Next.js Dockerfile
3. Generate `.env.production.example`
4. Standardize service names across all configs
5. Implement automated database backups

### Architecture Decision
- **Recommended:** Docker Compose with Nginx reverse proxy (already partially implemented)
- **Rationale:** Repository shows Docker Compose preference, existing nginx config, single VPS target

---

**Next Step:** Proceed to STEP 2 - Production Architecture design.
