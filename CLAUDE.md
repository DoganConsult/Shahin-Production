# CLAUDE.md - Project Instructions for Claude AI Assistant

## Project Overview

**GRC System** (Governance, Risk, and Compliance) - Enterprise platform built with ABP Framework 8.3.0 and Blazor Server featuring 9 specialized Claude AI agents for compliance, risk assessment, audit, policy management, and analytics.

## Technology Stack

- **Framework**: ABP Framework 8.3.0
- **UI**: Blazor Server (84 .razor files, 78 routes)
- **Backend**: .NET 8.0 / ASP.NET Core
- **Database**: SQL Server 2022 (Docker containerized)
- **ORM**: Entity Framework Core 8.0.8
- **Authentication**: OpenIddict (OAuth 2.0 / OIDC)
- **AI**: Anthropic Claude SDK 4.3.0 (9 specialized agents)
- **Caching**: Redis (optional)
- **Policy Engine**: YAML-based rules (YamlDotNet 15.1.4)
- **Deployment**: Docker Compose, CI/CD via GitHub Actions

## Environment Setup

### .NET CLI Path Configuration

**CRITICAL**: Before running any \`dotnet\` or \`abp\` commands, ensure PATH is set:

\`\`\`bash
export PATH="\$PATH:/usr/share/dotnet:\$HOME/.dotnet/tools"
\`\`\`

**Locations**:
- .NET 8.0 SDK: \`/usr/share/dotnet\`
- ABP CLI: \`\$HOME/.dotnet/tools\`

Add to \`~/.bashrc\` or \`~/.zshrc\` for persistence:
\`\`\`bash
echo 'export PATH="\$PATH:/usr/share/dotnet:\$HOME/.dotnet/tools"' >> ~/.bashrc
source ~/.bashrc
\`\`\`

**Verify installation**:
\`\`\`bash
dotnet --version   # Should show 8.0.x
abp --version      # Should show ABP CLI version
\`\`\`

## Project Structure

\`\`\`
src/
├── Grc.Blazor/               # Blazor Server UI (84 .razor files)
├── Grc.HttpApi.Host/         # API Backend (REST endpoints)
├── Grc.Application/          # Application services (CRUD logic)
├── Grc.Application.Contracts/# DTOs and interfaces
├── Grc.Domain/               # Domain entities and business logic
├── Grc.Domain.Shared/        # Shared constants, enums, localization
├── Grc.EntityFrameworkCore/  # EF Core DbContext and migrations
├── Grc.Agents/               # 9 Claude AI agents module
└── Grc.DbMigrator/           # Database migration console app

test/
├── Grc.Application.Tests/    # Unit & integration tests
└── Grc.TestBase/             # Test infrastructure

scripts/
├── test-connectivity.sh      # Quick connectivity test (30s)
├── test-frontend-api-db.sh   # Full integration test
└── deploy-*.sh               # Deployment scripts
\`\`\`

## Key Conventions

### Naming Conventions
- **Entities**: PascalCase, singular (e.g., \`Risk\`, \`Control\`, \`Assessment\`)
- **DTOs**: \`{Entity}Dto\`, \`Create{Entity}Dto\`, \`Update{Entity}Dto\`
- **AppServices**: \`{Entity}AppService\` implementing \`I{Entity}AppService\`
- **Repositories**: \`I{Entity}Repository\`

### Code Style
- Use \`async/await\` for all I/O operations
- Follow ABP's \`ApplicationService\` base class patterns
- Use \`IRepository<T>\` or custom repositories
- Implement \`ITransientDependency\` or \`ISingletonDependency\` for DI

### Database
- **SQL Server 2022** with EF Core 8.0.8
- Migrations in \`Grc.EntityFrameworkCore/Migrations\` (3 migration files)
- Use \`GrcDbContext\` for data access
- Multi-tenancy: Hybrid database style (enabled)

### Localization
- Resources in \`Grc.Domain.Shared/Localization/Grc\`
- Support for English (en) and Arabic (ar)
- Arabic menu navigation in \`GrcMenuContributor.cs\`

### AI Agents Module (Grc.Agents)
9 specialized Claude agents:
1. **ComplianceAgent** - Regulatory compliance analysis
2. **RiskAssessmentAgent** - Risk scoring and assessment
3. **AuditAgent** - Audit trail analysis
4. **PolicyAgent** - Policy enforcement and recommendations
5. **WorkflowAgent** - Workflow automation
6. **AnalyticsAgent** - Data analytics and insights
7. **IntegrationAgent** - External system integration
8. **SecurityAgent** - Security monitoring
9. **ReportingAgent** - Report generation

**Required**: \`ClaudeAgents:ApiKey\` in configuration

## Critical Configuration (BEFORE FIRST RUN)

### 1. Create .env File
\`\`\`bash
cp .env.production.template .env
\`\`\`

### 2. Required Environment Variables
Edit \`.env\` and set:

\`\`\`bash
# Database (REQUIRED)
ConnectionStrings__Default="Server=localhost;Database=GrcDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"

# Authentication (REQUIRED)
AuthServer__Authority="http://localhost:5010"

# Application URLs (REQUIRED)
App__SelfUrl="http://localhost:5010"
App__ClientUrl="http://localhost:8080"

# CORS (REQUIRED)
Cors__AllowedOrigins__0="http://localhost:8080"
Cors__AllowedOrigins__1="http://localhost:5010"

# Claude AI (REQUIRED for AI features)
ClaudeAgents__ApiKey="sk-ant-api03-xxxxx"  # Get from https://console.anthropic.com/
\`\`\`

**See**: \`REQUIRED_CONFIGURATION_CHECKLIST.md\` for complete setup

## Common Tasks

### First-Time Setup
\`\`\`bash
# 1. Ensure PATH is set
export PATH="\$PATH:/usr/share/dotnet:\$HOME/.dotnet/tools"

# 2. Create .env file
cp .env.production.template .env
# Edit .env with your configuration

# 3. Start database
docker-compose up -d db redis

# 4. Run migrations
cd src/Grc.DbMigrator
dotnet run

# 5. Start API
cd ../Grc.HttpApi.Host
dotnet run

# 6. Start Blazor (in new terminal)
cd ../Grc.Blazor
dotnet run
\`\`\`

### Running the Application
\`\`\`bash
# Set PATH first
export PATH="\$PATH:/usr/share/dotnet:\$HOME/.dotnet/tools"

# Option 1: Docker Compose (Recommended)
docker-compose up

# Option 2: Manual
# Terminal 1 - API
cd src/Grc.HttpApi.Host
dotnet run  # Runs on http://localhost:5010

# Terminal 2 - Blazor
cd src/Grc.Blazor
dotnet run  # Runs on http://localhost:8080
\`\`\`

### Testing
\`\`\`bash
# Quick connectivity test (30 seconds)
./scripts/test-connectivity.sh

# Full integration test
./scripts/test-frontend-api-db.sh

# Unit & integration tests
dotnet test

# Specific test project
dotnet test test/Grc.Application.Tests
\`\`\`

### Database Operations
\`\`\`bash
# Add migration
cd src/Grc.EntityFrameworkCore
dotnet ef migrations add YourMigrationName -s ../Grc.HttpApi.Host

# Update database
cd src/Grc.DbMigrator
dotnet run

# View migrations
dotnet ef migrations list -s ../Grc.HttpApi.Host
\`\`\`

### Code Quality
\`\`\`bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run analyzers (StyleCop, Security Code Scan, etc.)
dotnet build /p:TreatWarningsAsErrors=true
\`\`\`

### Adding a New Entity
1. Create entity in \`Grc.Domain/Entities\`
2. Add DbSet to \`GrcDbContext\` (\`Grc.EntityFrameworkCore/Data/GrcDbContext.cs\`)
3. Create DTOs in \`Grc.Application.Contracts/{Module}/Dtos\`
4. Create AppService in \`Grc.Application/{Module}/{Entity}AppService.cs\`
5. Add migration: 
   \`\`\`bash
   cd src/Grc.EntityFrameworkCore
   dotnet ef migrations add Add{Entity} -s ../Grc.HttpApi.Host
   \`\`\`
6. Run migrator:
   \`\`\`bash
   cd ../Grc.DbMigrator
   dotnet run
   \`\`\`

## Important Files

### Configuration
- \`.env\` - **Environment variables (CREATE THIS FIRST)**
- \`src/Grc.HttpApi.Host/appsettings.json\` - API configuration
- \`src/Grc.Blazor/appsettings.json\` - Blazor configuration
- \`etc/policies/grc-baseline.yml\` - Policy engine rules
- \`docker-compose.yml\` - Docker orchestration

### Core Modules
- \`src/Grc.HttpApi.Host/GrcHttpApiHostModule.cs\` - API module
- \`src/Grc.Blazor/GrcBlazorModule.cs\` - Blazor module
- \`src/Grc.EntityFrameworkCore/Data/GrcDbContext.cs\` - Database context
- \`src/Grc.Agents/GrcAgentsModule.cs\` - Claude AI agents module

### Database
- \`src/Grc.EntityFrameworkCore/Migrations/\` - EF Core migrations (3 files)
- \`src/Grc.DbMigrator/GrcDbMigratorModule.cs\` - Migration runner
- \`src/Grc.Domain/Data/GrcDataSeedContributor.cs\` - Seed data

### Documentation
- \`COMPREHENSIVE_DEPENDENCY_AUDIT.md\` - Full dependency analysis
- \`REQUIRED_CONFIGURATION_CHECKLIST.md\` - Setup guide
- \`DEPENDENCY_STATUS_MATRIX.md\` - Quick reference
- \`TEST_GUIDE.md\` - Testing documentation
- \`BLAZOR_PAGES_ROUTES_SUMMARY.md\` - All routes (78 routes)

## Security & Authentication

### OpenIddict Configuration
- **OAuth 2.0 / OpenID Connect** server built-in
- Auto-generated development certificates
- Production requires .pfx certificates (see \`REQUIRED_CONFIGURATION_CHECKLIST.md\`)
- Discovery endpoint: \`http://localhost:5010/.well-known/openid-configuration\`

### Roles (15 predefined roles seeded)
- Admin, Compliance Officer, Risk Manager, Auditor, Legal Counsel
- IT Security, Data Protection Officer, Business Analyst, Executive
- Operational Manager, HR Manager, Finance Manager, Quality Assurance
- Vendor Manager, Board Member

### Permissions
- Role-based access control via ABP permissions
- Policy engine enforcement (YAML rules)
- Data classification: Public, Internal, Confidential, Restricted

### Audit & Logging
- Audit logging enabled for all entities (ABP built-in)
- Serilog configured (Console + File rolling)
- Application Insights support (optional)

## Default Credentials

**Admin User** (seeded by DbMigrator):
- Username: \`admin\`
- Password: Check \`GrcDataSeedContributor.cs\` or set in seed data

## Troubleshooting

### Common Issues

**1. "Claude API key is not configured" error**
\`\`\`bash
# Add to .env file:
ClaudeAgents__ApiKey="sk-ant-api03-xxxxx"

# Or disable agents module in Grc.HttpApi.Host.csproj:
# Comment out: <ProjectReference Include="..\Grc.Agents\Grc.Agents.csproj" />
\`\`\`

**2. "Cannot connect to database"**
\`\`\`bash
# Check connection string in .env
# Start SQL Server:
docker-compose up -d db

# Test connection:
docker exec grc-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourPassword -C -Q "SELECT 1"
\`\`\`

**3. "dotnet: command not found"**
\`\`\`bash
# Set PATH:
export PATH="\$PATH:/usr/share/dotnet:\$HOME/.dotnet/tools"

# Verify:
dotnet --version
\`\`\`

**4. CORS errors in browser**
\`\`\`bash
# Ensure CORS origins are set in .env:
Cors__AllowedOrigins__0="http://localhost:8080"
Cors__AllowedOrigins__1="http://localhost:5010"
\`\`\`

**5. Migration errors**
\`\`\`bash
# Check if database exists:
docker exec grc-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourPassword -C -Q "SELECT name FROM sys.databases"

# Run migrator:
cd src/Grc.DbMigrator
dotnet run
\`\`\`

### Health Checks
\`\`\`bash
# API health
curl http://localhost:5010/health

# API ready
curl http://localhost:5010/health/ready

# Blazor health
curl http://localhost:8080/health
\`\`\`

## When Making Changes

### Code Quality Checklist
1. ✅ **Set PATH** before running dotnet commands
2. ✅ Follow existing ABP patterns and conventions
3. ✅ Add error handling with try/catch
4. ✅ Use ABP's localization for user-facing strings
5. ✅ Update related unit tests in \`test/Grc.Application.Tests\`
6. ✅ Run tests: \`dotnet test\`
7. ✅ Check for breaking changes in APIs
8. ✅ Add XML documentation comments
9. ❌ Don't commit \`.env\` file or secrets
10. ❌ Don't bypass authentication/authorization
11. ❌ Don't hardcode configuration values

### Before Committing
\`\`\`bash
# Restore and build
dotnet restore
dotnet build

# Run tests
dotnet test

# Check analyzers (StyleCop, Security)
dotnet build /p:TreatWarningsAsErrors=true

# Run connectivity tests
./scripts/test-connectivity.sh
\`\`\`

## Useful Resources

- **ABP Documentation**: https://docs.abp.io/en/abp/latest
- **Anthropic Claude API**: https://docs.anthropic.com/claude/reference/getting-started-with-the-api
- **Entity Framework Core**: https://learn.microsoft.com/en-us/ef/core/
- **Blazor**: https://learn.microsoft.com/en-us/aspnet/core/blazor/

## Quick Reference

### Default Ports
- API: \`http://localhost:5010\`
- Blazor: \`http://localhost:8080\`
- SQL Server: \`localhost:1433\`
- Redis: \`localhost:6379\`

### Key Commands
\`\`\`bash
# Set PATH (ALWAYS RUN FIRST)
export PATH="\$PATH:/usr/share/dotnet:\$HOME/.dotnet/tools"

# Quick test
./scripts/test-connectivity.sh

# Start everything
docker-compose up

# View logs
docker-compose logs -f

# Stop everything
docker-compose down

# Rebuild and start
docker-compose up --build
\`\`\`
