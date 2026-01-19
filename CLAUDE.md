# CLAUDE.md - Shahin AI GRC Platform

## Quick Start for Claude Code

This is the **Shahin AI GRC Platform** - an enterprise Governance, Risk, and Compliance SaaS built with ASP.NET Core 8.0 MVC and PostgreSQL.

### Repository Structure

```
Shahin-ai/
├── Shahin-Jan-2026/           # Main application code
│   ├── src/GrcMvc/            # ASP.NET Core MVC project
│   ├── tests/                 # Test projects
│   ├── scripts/               # Automation scripts
│   └── CLAUDE.md              # Detailed project documentation
├── grc-frontend/              # React public website
├── grc-app/                   # React internal application
└── docs/                      # Documentation
```

### Primary Working Directory

**Main codebase**: `Shahin-Jan-2026/src/GrcMvc/`

For detailed project instructions, see: [Shahin-Jan-2026/CLAUDE.md](Shahin-Jan-2026/CLAUDE.md)

### Quick Commands

```bash
# Start infrastructure (PostgreSQL + Redis)
cd Shahin-Jan-2026
docker-compose up -d db redis

# Build and run
cd Shahin-Jan-2026/src/GrcMvc
dotnet build
dotnet run

# Run tests
dotnet test

# Application URL
http://localhost:5010
```

### Key Technologies

- **Backend**: ASP.NET Core 8.0 MVC, EF Core 8, PostgreSQL 15
- **Frontend**: React (grc-frontend, grc-app)
- **AI**: 12 Claude-powered agents for GRC automation
- **Infrastructure**: Docker Compose, Redis, Kafka, Camunda BPM

### GitHub Repository

**Production**: https://github.com/DoganConsult/Shahin-Production.git

### Claude Code Integration

Claude Code CLI reads this file for project context. Common tasks:

```bash
# Debug authentication issues
claude "fix the JWT token generation in AuthenticationService"

# Run Golden Flow tests
claude "run the PowerShell Golden Flow tests"

# Fix entity type mismatches
claude "resolve the UserId string vs Guid mismatch in [Entity].cs"

# Database operations
claude "create a migration for the new [Entity]"
```

### Current Status

- **Build**: Successful (0 errors, 5 warnings)
- **Database**: PostgreSQL 15 via Docker
- **Authentication**: ASP.NET Core Identity + JWT

---

**Last Updated**: 2026-01-19
