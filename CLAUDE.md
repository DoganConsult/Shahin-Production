# CLAUDE.md - Project Instructions for Claude AI Assistant

## Project Overview

**Shahin AI GRC System** (Governance, Risk, and Compliance) - Enterprise SaaS platform built with ASP.NET Core 8.0 MVC and PostgreSQL, featuring 12 specialized AI agents for compliance, risk assessment, audit, policy management, and analytics.

**Multi-tenant Architecture**: Full tenant isolation with workspace-based collaboration, supporting KSA regulatory compliance (NCA, SAMA, PDPL, CITC, etc.) and international frameworks (ISO 27001, SOC 2, NIST, GDPR).

## Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **UI**: Razor Views (337 .cshtml files)
- **Backend**: .NET 8.0 / C# 12
- **Database**: PostgreSQL 15 (Docker containerized)
- **ORM**: Entity Framework Core 8.0.8 (Npgsql provider)
- **Authentication**: ASP.NET Core Identity + JWT Bearer
- **AI**: Anthropic Claude Sonnet 4.5 (12 specialized agents)
- **Caching**: Redis 7 (optional)
- **Message Queue**: Kafka + RabbitMQ (MassTransit)
- **Workflow Engine**: Camunda BPM 7.20 + Custom workflow system
- **Background Jobs**: Hangfire with PostgreSQL storage
- **Policy Engine**: YAML-based rules (YamlDotNet 15.1.4)
- **Localization**: English + Arabic (RTL support)
- **Deployment**: Docker Compose, CI/CD via GitHub Actions

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    Shahin AI GRC Platform                    │
├─────────────────────────────────────────────────────────────┤
│  Web Layer (ASP.NET Core MVC)                               │
│  ├── 100+ Controllers (MVC + API)                           │
│  ├── 337 Razor Views (.cshtml)                              │
│  └── Localization (2,495 EN + 2,399 AR strings)             │
├─────────────────────────────────────────────────────────────┤
│  Service Layer                                               │
│  ├── 12 AI Agents (Claude Sonnet 4.5)                       │
│  ├── Policy Enforcement Engine                              │
│  ├── Workflow Services (Custom + Camunda)                   │
│  ├── RBAC & Permission System                               │
│  └── Integration Services (Graph, Email, SMTP)              │
├─────────────────────────────────────────────────────────────┤
│  Data Layer                                                  │
│  ├── 200+ Entity Models                                     │
│  ├── 60 EF Core Migrations                                  │
│  ├── Multi-tenant Query Filters                             │
│  └── Audit Logging & Soft Delete                            │
├─────────────────────────────────────────────────────────────┤
│  Infrastructure                                              │
│  ├── PostgreSQL 15 (Main DB + Auth DB)                      │
│  ├── Redis 7 (Caching)                                      │
│  ├── Kafka + Zookeeper (Event streaming)                    │
│  ├── Camunda BPM (Workflow orchestration)                   │
│  ├── ClickHouse (Analytics OLAP)                            │
│  ├── Hangfire (Background jobs)                             │
│  └── Grafana + Superset + Metabase (BI/Analytics)           │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

```
d:\Shahin-Jan-2026/
├── src/
│   └── GrcMvc/                        # Main ASP.NET Core MVC project
│       ├── Controllers/               # 100+ MVC & API controllers
│       ├── Views/                     # 337 Razor views
│       ├── Models/                    # 200+ entity models
│       ├── Services/                  # Business logic services
│       │   ├── Implementations/       # Service implementations
│       │   └── Interfaces/            # Service contracts
│       ├── Data/                      # EF Core DbContext & repositories
│       │   ├── GrcDbContext.cs        # Main application database
│       │   ├── GrcAuthDbContext.cs    # Separate auth database (security)
│       │   ├── Seeds/                 # 20+ seed data files
│       │   └── Repositories/          # Custom repositories
│       ├── Migrations/                # 60 EF Core migration files
│       ├── Agents/                    # AI agent configurations
│       ├── Application/               # Application layer
│       │   ├── Permissions/           # RBAC permission system
│       │   └── Policy/                # Policy engine
│       ├── Authorization/             # Custom authorization handlers
│       ├── BackgroundJobs/            # Hangfire jobs
│       ├── Configuration/             # Settings classes
│       ├── Middleware/                # Custom middleware
│       ├── Messaging/                 # Kafka/RabbitMQ consumers
│       ├── Resources/                 # Localization resources
│       ├── Security/                  # Security utilities
│       ├── Validators/                # FluentValidation validators
│       ├── wwwroot/                   # Static assets (CSS, JS, images)
│       ├── Program.cs                 # Application entry point (1,400+ lines)
│       ├── appsettings.json           # Configuration
│       ├── appsettings.Production.json
│       └── GrcMvc.csproj              # Project file
│
├── tests/
│   └── GrcMvc.Tests/                  # Test project
│       ├── Unit/                      # Unit tests
│       ├── Integration/               # Integration tests
│       ├── E2E/                       # End-to-end tests
│       ├── Security/                  # Security tests
│       └── Performance/               # Performance tests
│
├── scripts/                           # 33 automation scripts
│   ├── deploy-*.sh                    # Deployment scripts
│   ├── backup-*.sh                    # Database backup scripts
│   ├── start-*.sh                     # Startup scripts
│   ├── validate-*.sh                  # Validation scripts
│   └── init-db.sql                    # DB initialization (currently empty)
│
├── .github/workflows/                 # CI/CD pipelines
│   ├── ci-cd-pipeline.yml             # Main CI/CD pipeline
│   └── quality-check.yml              # Quality gate (5 phases)
│
├── docker-compose.yml                 # Main orchestration (11 services)
├── docker-compose.analytics.yml       # Analytics stack
├── docker-compose.quality.yml         # Quality monitoring
├── .env                               # Environment variables (DO NOT COMMIT)
├── .env.production.template           # Production template
└── CLAUDE.md                          # This file
```

## Key Conventions

### Naming Conventions
- **Entities**: PascalCase, singular (e.g., `Risk`, `Control`, `Assessment`)
- **Controllers**: `{Entity}Controller` (MVC), `{Entity}ApiController` (API)
- **Services**: `{Entity}Service` implementing `I{Entity}Service`
- **Repositories**: `{Entity}Repository` implementing `I{Entity}Repository`
- **DTOs**: `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto`
- **Views**: `{Controller}/{Action}.cshtml`

### Code Style
- Use `async/await` for all I/O operations
- Dependency Injection via constructor injection
- Use `ILogger<T>` for logging (Serilog configured)
- FluentValidation for input validation
- AutoMapper for object mapping

### Database (PostgreSQL 15)
- **Two databases**:
  - `GrcMvcDb` (main application data via `GrcDbContext`)
  - `GrcAuthDb` (authentication data via `GrcAuthDbContext`)
- **60 migrations** in `src/GrcMvc/Migrations/`
- **200+ entities** with full audit fields
- **Multi-tenancy**: Row-level isolation via `TenantId` query filters
- **Soft delete**: `IsDeleted` flag on all entities (base class: `BaseEntity`)
- **Concurrency**: Optimistic concurrency with `RowVersion` (timestamp)

### Localization
- Resources in `src/GrcMvc/Resources/`
- Full bilingual support: English (2,495 strings) + Arabic (2,399 strings)
- RTL (Right-to-Left) support for Arabic via `rtl.css`
- Usage: `@L["KeyName"]` in Razor views, `IStringLocalizer` in services

### Multi-Tenancy Architecture
- **Tenant Isolation**: Every entity has `TenantId` (Guid?)
- **Workspace Scoping**: Additional `WorkspaceId` for team collaboration
- **Query Filters**: Automatic filtering in `GrcDbContext.OnModelCreating()`
- **Tenant Context**: `ITenantContextService` tracks current tenant
- **Workspace Context**: `IWorkspaceContextService` tracks current workspace

### AI Agents Module

**12 Specialized AI Agents** (not 9!):

| Agent Code | Name | Type | Implementation Status |
|------------|------|------|----------------------|
| SHAHIN_AI | Shahin AI Assistant | Orchestrator | ✅ Implemented |
| COMPLIANCE_AGENT | Compliance Analysis Agent | Analysis | ✅ ClaudeAgentService |
| RISK_AGENT | Risk Assessment Agent | Analysis | ✅ ClaudeAgentService |
| AUDIT_AGENT | Audit Analysis Agent | Analysis | ✅ ClaudeAgentService |
| POLICY_AGENT | Policy Management Agent | Analysis | ✅ ClaudeAgentService |
| ANALYTICS_AGENT | Analytics & Insights Agent | Analytics | ✅ ClaudeAgentService |
| REPORT_AGENT | Report Generation Agent | Reporting | ✅ ClaudeAgentService |
| DIAGNOSTIC_AGENT | System Diagnostic Agent | Monitoring | ✅ DiagnosticAgentService |
| SUPPORT_AGENT | Customer Support Agent | Support | ✅ SupportAgentService |
| WORKFLOW_AGENT | Workflow Optimization Agent | Automation | ✅ ClaudeAgentService |
| EVIDENCE_AGENT | Evidence Collection Agent | Collection | ⚠️ Defined, not implemented |
| EMAIL_AGENT | Email Classification Agent | Communication | ⚠️ Separate EmailAiService |

**Architecture Pattern**: Unified `ClaudeAgentService` handles 7+ agent types (not separate services per agent).

**Agent Governance** (Enterprise-grade):
- `AgentApprovalGate` - Human-in-the-loop approval workflows
- `AgentSoDRule` - Segregation of Duties enforcement
- `AgentConfidenceScore` - Trust scoring
- `HumanRetainedResponsibility` - Critical decision retention

**API**: 20+ endpoints in `Controllers/Api/AgentController.cs`

**Configuration**:
```json
{
  "ClaudeAgents": {
    "Enabled": true,
    "ApiKey": "",  // ⚠️ EMPTY by default - set in .env
    "Model": "claude-sonnet-4-5-20250514",
    "MaxTokens": 4096,
    "Temperature": 0.7
  }
}
```

**⚠️ IMPORTANT**: Claude API key is **NOT configured by default**. AI features will return "AI service not configured" error until set.

## Environment Setup

### Prerequisites
- .NET 8.0 SDK ([download](https://dotnet.microsoft.com/download/dotnet/8.0))
- Docker Desktop (for PostgreSQL, Kafka, etc.)
- Node.js 18+ (optional, for frontend build tools)
- Git

### .NET CLI Path Configuration (Linux/macOS)

If running on Linux/macOS, ensure .NET is in PATH:

```bash
export PATH="$PATH:/usr/share/dotnet:$HOME/.dotnet/tools"
```

Add to `~/.bashrc` or `~/.zshrc` for persistence:
```bash
echo 'export PATH="$PATH:/usr/share/dotnet:$HOME/.dotnet/tools"' >> ~/.bashrc
source ~/.bashrc
```

**Verify installation**:
```bash
dotnet --version   # Should show 8.0.x
```

**Note**: No ABP CLI required (this is NOT an ABP Framework project).

## Critical Configuration (BEFORE FIRST RUN)

### 1. Create .env File
```bash
cp .env.production.template .env
```

### 2. Required Environment Variables

Edit `.env` and configure:

```bash
# ════════════════════════════════════════════════════════════
# Database (PostgreSQL) - REQUIRED
# ════════════════════════════════════════════════════════════
DB_HOST=localhost
DB_PORT=5432
DB_NAME=GrcMvcDb
DB_USER=postgres
DB_PASSWORD=your_secure_password_here

# Or use connection string format:
CONNECTION_STRING="Host=localhost;Database=GrcMvcDb;Username=postgres;Password=your_password;Port=5432"

# ════════════════════════════════════════════════════════════
# JWT Authentication - REQUIRED
# ════════════════════════════════════════════════════════════
JWT_SECRET=your_jwt_secret_at_least_32_characters_long_change_in_production
JWT_ISSUER=https://portal.shahin-ai.com
JWT_AUDIENCE=https://portal.shahin-ai.com

# ════════════════════════════════════════════════════════════
# Application URLs - REQUIRED
# ════════════════════════════════════════════════════════════
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://0.0.0.0:5010
ALLOWED_HOSTS=localhost;portal.shahin-ai.com;157.180.105.48

# ════════════════════════════════════════════════════════════
# Claude AI - REQUIRED for AI features
# ════════════════════════════════════════════════════════════
CLAUDE_ENABLED=true
CLAUDE_API_KEY=sk-ant-api03-xxxxx  # Get from https://console.anthropic.com/
CLAUDE_MODEL=claude-sonnet-4-5-20250514

# ════════════════════════════════════════════════════════════
# Email (Microsoft 365 OAuth2) - Optional
# ════════════════════════════════════════════════════════════
SMTP_HOST=smtp.office365.com
SMTP_FROM_EMAIL=info@doganconsult.com
SMTP_FROM_NAME=Shahin AI GRC System
SMTP_TENANT_ID=your_azure_tenant_id
SMTP_CLIENT_ID=your_azure_client_id
SMTP_CLIENT_SECRET=your_azure_client_secret

# ════════════════════════════════════════════════════════════
# Microsoft Graph API - Optional
# ════════════════════════════════════════════════════════════
GRAPH_TENANT_ID=your_azure_tenant_id
GRAPH_CLIENT_ID=your_graph_client_id
GRAPH_CLIENT_SECRET=your_graph_client_secret

# ════════════════════════════════════════════════════════════
# Kafka - Optional (for event streaming)
# ════════════════════════════════════════════════════════════
KAFKA_ENABLED=true
KAFKA_BOOTSTRAP_SERVERS=localhost:9092
KAFKA_CLIENT_ID=grc-mvc
KAFKA_GROUP_ID=grc-consumer-group

# ════════════════════════════════════════════════════════════
# Camunda BPM - Optional (for workflow engine)
# ════════════════════════════════════════════════════════════
CAMUNDA_ENABLED=true
CAMUNDA_BASE_URL=http://localhost:8085/camunda
CAMUNDA_USERNAME=demo
CAMUNDA_PASSWORD=demo
```

### 3. Obtain Claude API Key

1. Go to [Anthropic Console](https://console.anthropic.com/)
2. Sign up / Log in
3. Navigate to API Keys
4. Create a new API key
5. Copy the key (starts with `sk-ant-api03-`)
6. Add to `.env`: `CLAUDE_API_KEY=sk-ant-api03-xxxxx`

**⚠️ Without this key, all AI agent features will be disabled!**

## Common Tasks

### First-Time Setup

```bash
# 1. Clone repository (if not already done)
git clone <repository-url>
cd Shahin-Jan-2026

# 2. Create .env file
cp .env.production.template .env
# Edit .env with your configuration (especially CLAUDE_API_KEY)

# 3. Start PostgreSQL and other infrastructure
docker-compose up -d db redis

# Wait for database to be ready (check health)
docker-compose ps

# 4. Run database migrations
cd src/GrcMvc
dotnet ef database update

# Or use the built-in migration runner:
dotnet run --seed-data

# 5. Start the application
dotnet run
# Application runs on http://localhost:5010

# 6. Access the application
# Open browser: http://localhost:5010
# Default admin credentials are seeded (check seed data files)
```

### Running the Application

#### Option 1: Docker Compose (Recommended for Full Stack)
```bash
# Start all services (app + infrastructure)
docker-compose up

# Start in background
docker-compose up -d

# View logs
docker-compose logs -f grcmvc

# Stop all services
docker-compose down
```

#### Option 2: Local Development (App only)
```bash
# Terminal 1 - Start infrastructure
docker-compose up -d db redis kafka

# Terminal 2 - Run application
cd src/GrcMvc
dotnet run
# Runs on http://localhost:5010
```

#### Option 3: Visual Studio / VS Code
- Open `src/GrcMvc/GrcMvc.csproj` in IDE
- Press F5 to debug
- Ensure Docker services are running first

### Database Operations

#### Run Migrations
```bash
cd src/GrcMvc
dotnet ef database update
```

#### Add New Migration
```bash
cd src/GrcMvc
dotnet ef migrations add YourMigrationName
```

#### Rollback Migration
```bash
dotnet ef database update PreviousMigrationName
```

#### View Migration List
```bash
dotnet ef migrations list
```

#### Seed Data
```bash
# Seed data runs automatically on first startup
dotnet run --seed-data

# Or manually execute seed data
# (Seeds are in src/GrcMvc/Data/Seeds/)
```

#### Backup Database
```bash
./scripts/backup-db.sh
# Or backup all tenant databases:
./scripts/backup-all-tenants.sh
```

### Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/GrcMvc.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=Security"
```

### Code Quality

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Build with warnings as errors
dotnet build /p:TreatWarningsAsErrors=true

# Run quality check script
./scripts/quality-check.sh

# Run full validation
./scripts/validate-all.sh
```

### Deployment

```bash
# Deploy to staging (manual)
./scripts/deploy-staging.sh

# Deploy to production (manual)
./scripts/deploy-production.sh

# Safe deployment (with backups)
./scripts/deploy-safe.sh

# CI/CD deploys automatically on push to main/develop
# See .github/workflows/ci-cd-pipeline.yml
```

## Default Credentials

**Platform Admin** (seeded automatically):
- Email: Check `src/GrcMvc/Data/Seeds/PlatformAdminSeeds.cs`
- Password: Check seed data or set during first run

**POC Tenant** (demo data):
- Tenant: Shahin AI Technologies (`shahin-ai`)
- Complete onboarding wizard data (12 steps)
- Sample assessments, risks, controls, evidence

**Roles** (15 predefined):
- Admin, Compliance Officer, Risk Manager, Auditor, Legal Counsel
- IT Security, Data Protection Officer, Business Analyst, Executive
- Operational Manager, HR Manager, Finance Manager, Quality Assurance
- Vendor Manager, Board Member

## Docker Services (11 Total)

| Service | Port | Purpose | Required |
|---------|------|---------|----------|
| grcmvc | 8888 | Main application | ✅ Yes |
| db (PostgreSQL) | 5432 | Primary database | ✅ Yes |
| redis | 6379 | Caching | ⚠️ Optional |
| kafka | 9092 | Event streaming | ⚠️ Optional |
| zookeeper | 2181 | Kafka coordinator | ⚠️ Optional |
| kafka-connect | 8083 | Debezium CDC | ⚠️ Optional |
| camunda | 8085 | Workflow engine | ⚠️ Optional |
| clickhouse | 8123/9000 | Analytics OLAP | ⚠️ Optional |
| grafana | 3030 | Monitoring dashboards | ⚠️ Optional |
| superset | 8088 | BI platform | ⚠️ Optional |
| metabase | 3033 | BI platform | ⚠️ Optional |

**Minimal Setup**: Only `grcmvc` + `db` required for core functionality.

## Health Checks

```bash
# Application health
curl http://localhost:5010/health

# Application ready
curl http://localhost:5010/health/ready

# Database connectivity
curl http://localhost:5010/health/db

# All subsystems
curl http://localhost:5010/health/detailed
```

## API Documentation

- **Swagger UI**: http://localhost:5010/swagger (Development only)
- **API Endpoints**: 100+ controllers with 200+ endpoints
- **Authentication**: Bearer token (JWT)
- **Rate Limiting**: Configured per endpoint

## Security & Authentication

### Authentication Flow
1. User logs in via `/Account/Login`
2. JWT token issued (includes TenantId, UserId, Roles)
3. Token stored in cookie or Authorization header
4. All API requests validated via `[Authorize]` attributes

### Authorization
- **Permission-based**: 214 `[Authorize(GrcPermissions.xxx)]` attributes
- **Role-based**: 15 predefined roles
- **Tenant-isolated**: All queries automatically filtered by TenantId
- **Workspace-scoped**: Team collaboration via WorkspaceId

### Security Features
- Separate auth database (`GrcAuthDbContext`)
- Password hashing (ASP.NET Core Identity)
- JWT token expiration (configurable)
- CSRF protection on all forms
- Rate limiting (configured in Program.cs)
- Audit logging (all entity changes tracked)

## Troubleshooting

### 1. "Claude API key is not configured" error
```bash
# Add to .env file:
CLAUDE_API_KEY=sk-ant-api03-xxxxx

# Or set environment variable:
export CLAUDE_API_KEY=sk-ant-api03-xxxxx

# Restart application
```

### 2. "Cannot connect to database"
```bash
# Check PostgreSQL is running:
docker-compose ps db

# Start PostgreSQL:
docker-compose up -d db

# Test connection:
docker exec -it grc-db psql -U postgres -d GrcMvcDb -c "SELECT 1"

# Check connection string in .env
```

### 3. "Pending migrations" error
```bash
# Run migrations:
cd src/GrcMvc
dotnet ef database update

# Or use seed-data flag:
dotnet run --seed-data
```

### 4. Port already in use
```bash
# Check what's using port 5010:
netstat -ano | findstr :5010  # Windows
lsof -i :5010                  # Linux/macOS

# Change port in .env:
ASPNETCORE_URLS=http://0.0.0.0:5011
```

### 5. "dotnet: command not found" (Linux/macOS)
```bash
# Add to PATH:
export PATH="$PATH:/usr/share/dotnet"

# Verify:
dotnet --version
```

### 6. Docker services failing to start
```bash
# Check Docker is running:
docker --version

# View service logs:
docker-compose logs db
docker-compose logs kafka

# Restart services:
docker-compose restart db
```

### 7. Migration conflicts
```bash
# Remove last migration (if not applied):
dotnet ef migrations remove

# Rollback database to previous migration:
dotnet ef database update PreviousMigrationName

# Reapply migrations:
dotnet ef database update
```

## When Making Changes

### Code Quality Checklist
1. ✅ Follow existing ASP.NET Core MVC patterns
2. ✅ Add error handling with try/catch
3. ✅ Use localization for user-facing strings: `@L["KeyName"]`
4. ✅ Update related unit tests in `tests/GrcMvc.Tests`
5. ✅ Run tests: `dotnet test`
6. ✅ Check for breaking changes in APIs
7. ✅ Add XML documentation comments
8. ✅ Run quality checks: `./scripts/quality-check.sh`
9. ❌ Don't commit `.env` file or secrets
10. ❌ Don't bypass authentication/authorization
11. ❌ Don't hardcode configuration values

### Before Committing
```bash
# Restore and build
dotnet restore
dotnet build

# Run tests
dotnet test

# Run quality checks
./scripts/quality-check.sh

# Check for secrets (Git Guardian)
git diff
```

### Adding a New Entity

1. **Create Entity Model** in `src/GrcMvc/Models/Entities/`:
```csharp
public class YourEntity : BaseEntity
{
    public string Name { get; set; }
    // ... other properties
}
```

2. **Add DbSet to `GrcDbContext`**:
```csharp
public DbSet<YourEntity> YourEntities { get; set; }
```

3. **Configure in `OnModelCreating`** (if needed):
```csharp
modelBuilder.Entity<YourEntity>(entity =>
{
    entity.HasIndex(e => e.Name);
    entity.HasIndex(e => e.TenantId);
});
```

4. **Create Migration**:
```bash
cd src/GrcMvc
dotnet ef migrations add Add_YourEntity
```

5. **Create DTO** in `Models/DTOs/`:
```csharp
public class YourEntityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    // ... map properties
}
```

6. **Create Service Interface & Implementation**:
- `Services/Interfaces/IYourEntityService.cs`
- `Services/Implementations/YourEntityService.cs`

7. **Create Controller**:
- `Controllers/YourEntityController.cs` (MVC views)
- `Controllers/Api/YourEntityApiController.cs` (REST API)

8. **Create Views** in `Views/YourEntity/`:
- `Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `Details.cshtml`, `Delete.cshtml`

9. **Register Service** in `Program.cs`:
```csharp
builder.Services.AddScoped<IYourEntityService, YourEntityService>();
```

10. **Run Migration**:
```bash
dotnet ef database update
```

## Important Files

### Configuration
- `.env` - **Environment variables (DO NOT COMMIT)**
- `.env.production.template` - Production template
- `src/GrcMvc/appsettings.json` - Application configuration
- `src/GrcMvc/appsettings.Production.json` - Production overrides
- `etc/policies/grc-baseline.yml` - Policy engine rules

### Core Application
- `src/GrcMvc/Program.cs` - **Application entry point (1,400+ lines)**
- `src/GrcMvc/Data/GrcDbContext.cs` - Main database context
- `src/GrcMvc/Data/GrcAuthDbContext.cs` - Auth database context
- `src/GrcMvc/Views/Shared/_Layout.cshtml` - Main layout

### Database
- `src/GrcMvc/Migrations/` - **60 EF Core migrations**
- `src/GrcMvc/Data/Seeds/` - **20+ seed data files**
- `scripts/init-db.sql` - Manual DB init (currently empty)

### CI/CD
- `.github/workflows/ci-cd-pipeline.yml` - Main pipeline
- `.github/workflows/quality-check.yml` - Quality gate (5 phases)

### Documentation
- `BUILD_AND_RUN_GUIDE.md` - Quick start guide
- `DEPLOYMENT_GUIDE.md` - Deployment instructions
- `API_TESTING_GUIDE.md` - API testing documentation
- `COMPREHENSIVE_TESTING_GUIDE.md` - Test strategy
- `SECURITY_GUIDE.md` - Security best practices
- `00_DELIVERABLES_CHECKLIST.md` - Project deliverables

## Useful Resources

- **ASP.NET Core**: https://learn.microsoft.com/aspnet/core/
- **Entity Framework Core**: https://learn.microsoft.com/ef/core/
- **PostgreSQL**: https://www.postgresql.org/docs/
- **Anthropic Claude API**: https://docs.anthropic.com/claude/reference/getting-started-with-the-api
- **Docker**: https://docs.docker.com/
- **Hangfire**: https://docs.hangfire.io/
- **Camunda BPM**: https://docs.camunda.org/

## Quick Reference

### Default Ports
- Application: `http://localhost:5010`
- PostgreSQL: `localhost:5432`
- Redis: `localhost:6379`
- Kafka: `localhost:9092`
- Camunda: `http://localhost:8085`
- ClickHouse: `localhost:8123` (HTTP), `localhost:9000` (native)
- Grafana: `http://localhost:3030`
- Superset: `http://localhost:8088`
- Metabase: `http://localhost:3033`

### Key Commands
```bash
# Start everything
docker-compose up

# Start infrastructure only
docker-compose up -d db redis

# Run application
cd src/GrcMvc && dotnet run

# Run tests
dotnet test

# Run migrations
cd src/GrcMvc && dotnet ef database update

# View logs
docker-compose logs -f

# Stop everything
docker-compose down

# Rebuild and start
docker-compose up --build
```

### Common Paths
- **Application**: `src/GrcMvc/`
- **Tests**: `tests/GrcMvc.Tests/`
- **Scripts**: `scripts/`
- **Migrations**: `src/GrcMvc/Migrations/`
- **Views**: `src/GrcMvc/Views/`
- **Controllers**: `src/GrcMvc/Controllers/`
- **Services**: `src/GrcMvc/Services/`

## Known Issues & Limitations

1. **Claude API Key Required**: AI features disabled without valid API key in `.env`
2. **60 Migrations**: Large number of migrations (consider squashing for production)
3. **Missing Agents**: SecurityAgent and IntegrationAgent defined but not fully implemented
4. **Deployment Scripts**: CI/CD pipeline has placeholder deployment commands (need implementation)
5. **Some TODOs**: 4 TODO items in views (Plans, Subscription, Help/Contact)
6. **Testing Coverage**: Agent services and API integration tests need expansion

See full audit report in `.claude/plans/` for detailed list of missing items.

## Contributing

1. Create feature branch from `develop`
2. Make changes following code quality checklist
3. Run tests: `dotnet test`
4. Run quality checks: `./scripts/quality-check.sh`
5. Submit pull request to `develop`
6. CI/CD pipeline will run quality gate (must pass)
7. Merge requires approval + passing quality gate

## Support

- **Issues**: GitHub Issues
- **Documentation**: See `/docs` folder and `*.md` files
- **Internal Support**: Contact GRC team

---

**Last Updated**: 2026-01-10
**Project Version**: 1.0.x
**Framework Version**: .NET 8.0 / ASP.NET Core 8.0
**Database Version**: PostgreSQL 15
**AI Model**: Claude Sonnet 4.5 (claude-sonnet-4-5-20250514)
