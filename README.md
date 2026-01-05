# GRC MVC - Governance, Risk, and Compliance System

A clean, secure, single MVC application for enterprise GRC management built with ASP.NET Core 8.0 and Entity Framework Core.

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- SQL Server 2019+ (or Docker)
- Visual Studio 2022 / VS Code / Rider

### Development Setup

1. **Clone the repository**
```bash
git clone <repository-url>
cd grc-system
```

2. **Copy environment configuration**
```bash
cp .env.example .env
# Edit .env with your settings
```

3. **Run with Docker Compose** (Recommended)
```bash
docker-compose up -d
```

Or **Run locally**:
```bash
cd src/GrcMvc
dotnet run
```

4. **Access the application**
- URL: http://localhost:8080
- Default Admin: admin@grcmvc.com / Admin@123456

## 📁 Project Structure

```
grc-system/
├── src/
│   └── GrcMvc/                    # Single MVC Application
│       ├── Areas/                 # Feature modules
│       │   ├── Admin/
│       │   ├── Assessment/
│       │   ├── Audit/
│       │   ├── Risk/
│       │   └── Workflow/
│       ├── Configuration/         # App configuration
│       ├── Controllers/           # MVC controllers
│       ├── Data/                  # Entity Framework
│       │   ├── GrcDbContext.cs
│       │   └── Repositories/
│       ├── Models/
│       │   ├── DTOs/             # Data Transfer Objects
│       │   ├── Entities/         # Domain entities
│       │   └── ViewModels/       # View models
│       ├── Services/              # Business logic
│       ├── Views/                 # Razor views
│       ├── wwwroot/              # Static files
│       ├── appsettings.json      # Base configuration
│       ├── Dockerfile            # Container definition
│       └── Program.cs            # Application entry
├── docker-compose.yml            # Docker orchestration
├── GrcMvc.sln                   # Solution file
└── README.md                    # This file
```

## 🔒 Security Features

- **No hardcoded secrets** - All sensitive data via environment variables
- **JWT authentication** with minimum 32-character keys
- **Secure file uploads** with multi-layer validation
- **SQL injection protection** via Entity Framework parameterized queries
- **XSS protection** built into Razor views
- **Host header validation** to prevent spoofing
- **HTTPS enforcement** in production
- **Password complexity** requirements
- **Account lockout** protection

## 🛠️ Core Features

### Domain Entities (11)
- **Risk Management** - Risk assessment and tracking
- **Control Management** - Internal controls and testing
- **Audit Management** - Audit planning and findings
- **Policy Management** - Policy documentation and violations
- **Workflow Engine** - Business process automation
- **Evidence Collection** - Secure document management
- **Assessment Tracking** - Compliance assessments

### Technical Features
- ASP.NET Core 8.0 MVC
- Entity Framework Core 8.0.8
- SQL Server database
- ASP.NET Core Identity authentication
- JWT bearer tokens for API
- Generic repository pattern
- Area-based modular architecture
- Soft-delete support
- Audit trail (CreatedBy, ModifiedBy)

## 🚢 Production Deployment

### Environment Variables Required

```bash
# Database (REQUIRED)
ConnectionStrings__DefaultConnection="Server=prod-server;Database=GrcProdDb;..."

# JWT Security (REQUIRED)
JwtSettings__Secret="[Generate with: openssl rand -base64 32]"
JwtSettings__Issuer="https://your-domain.com"
JwtSettings__Audience="https://your-domain.com"

# Host Security (REQUIRED)
AllowedHosts="your-domain.com;www.your-domain.com"
```

### Deployment Options

1. **Docker**
```bash
docker build -t grcmvc:latest -f src/GrcMvc/Dockerfile .
docker run -d -p 443:443 --env-file .env grcmvc:latest
```

2. **IIS** - See `src/GrcMvc/PRODUCTION_DEPLOYMENT_GUIDE.md`

3. **Azure App Service** - Deploy via GitHub Actions or Azure CLI

## 📊 Database

### Run Migrations
```bash
cd src/GrcMvc
dotnet ef database update
```

### Create New Migration
```bash
dotnet ef migrations add YourMigrationName
```

## 🧪 Development

### Build
```bash
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Watch Mode
```bash
dotnet watch run
```

## 📝 Documentation

- [Production Deployment Guide](src/GrcMvc/PRODUCTION_DEPLOYMENT_GUIDE.md)
- [Security Implementation](SECURE_MVC_IMPLEMENTATION_SUMMARY.md)
- [Migration Plan](SINGLE_APP_MIGRATION_PLAN.md)

## 🔐 Security Notes

⚠️ **Before Production:**
1. Change default admin password
2. Generate new JWT secret
3. Use dedicated SQL user (not sa)
4. Enable HTTPS only
5. Configure firewall rules
6. Review file upload settings

## 📜 License

[Your License]

## 🤝 Support

For issues or questions, please contact support@your-domain.com

---

**Version:** 1.0.0
**Framework:** ASP.NET Core 8.0
**Status:** Production Ready