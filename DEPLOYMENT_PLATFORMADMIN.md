# 🚀 Deploy and Create PlatformAdmin Account

## Quick Deployment

### Option 1: Automated Script
```bash
./scripts/deploy-and-seed.sh
```

### Option 2: Manual Steps

#### 1. Clean and Build
```bash
cd /home/dogan/grc-system
dotnet clean src/GrcMvc/GrcMvc.csproj
dotnet restore src/GrcMvc/GrcMvc.csproj
dotnet build src/GrcMvc/GrcMvc.csproj -c Release
```

#### 2. Apply Database Migrations
```bash
# Main database
dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --context GrcDbContext

# Auth database
dotnet ef database update --project src/GrcMvc/GrcMvc.csproj --context GrcAuthDbContext
```

#### 3. Start Application
```bash
cd src/GrcMvc
dotnet run
```

## ✅ PlatformAdmin Creation

The PlatformAdmin account is **automatically created** when the application starts via `ApplicationInitializer`.

### Default Credentials
```
Email:    Dooganlap@gmail.com
Password: Platform@2026!
Role:     PlatformAdmin
Level:    Owner (Full Access)
```

### What Gets Created

1. **Identity User** - ASP.NET Core Identity user account
2. **PlatformAdmin Record** - Full owner-level permissions
3. **Role Assignment** - Added to "PlatformAdmin" role
4. **RBAC Permissions** - All permissions granted

### Verification

After application starts, check logs for:
```
✅ Platform Admin created successfully: Dooganlap@gmail.com with Owner level access
```

## 🔐 PlatformAdmin Permissions

- ✅ Create/Manage/Delete Tenants
- ✅ Manage Billing
- ✅ Access Tenant Data
- ✅ Manage Catalogs
- ✅ Manage Platform Admins
- ✅ View Analytics
- ✅ Manage Configuration
- ✅ Impersonate Users
- ✅ Reset Passwords

## 🌐 Access

After login, PlatformAdmin will be automatically redirected to:
- **URL**: `/platform/dashboard`
- **Route**: `http://localhost:8080/platform/dashboard`

## ⚠️ Important Notes

1. **Change Password**: Default password must be changed on first login
2. **Database Connection**: Ensure PostgreSQL is running and connection string is correct
3. **Migrations**: All migrations must be applied before PlatformAdmin can be created
4. **RBAC System**: RBAC roles and permissions are seeded before PlatformAdmin creation

## 🔧 Troubleshooting

### Database Connection Error
```bash
# Check PostgreSQL is running
sudo systemctl status postgresql

# Verify connection string in appsettings.json
# Default: Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5433
```

### PlatformAdmin Not Created
- Check application logs for errors
- Verify `ApplicationInitializer` ran successfully
- Check if user already exists (won't recreate if exists)

### Role Not Assigned
- Verify RBAC system was seeded (`RbacSeeds.SeedRbacSystemAsync`)
- Check Identity roles table for "PlatformAdmin" role

## 📝 Files Involved

- `src/GrcMvc/Data/Seeds/PlatformAdminSeeds.cs` - PlatformAdmin seeding logic
- `src/GrcMvc/Data/ApplicationInitializer.cs` - Calls seeding on startup
- `src/GrcMvc/Program.cs` - Registers and runs ApplicationInitializer
