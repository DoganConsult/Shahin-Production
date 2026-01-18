# Production Deployment Steps

**Date**: January 15, 2026  
**Status**: Ready for Production Deployment

---

## 🚀 Step 1: Set Production Environment Variables

### Required Variables

```bash
# JWT Authentication (REQUIRED)
export JWT_SECRET="YOUR_64_CHARACTER_SECRET_HERE"  # Generate: openssl rand -base64 64

# Database Connections (REQUIRED)
export ConnectionStrings__DefaultConnection="Host=your-db-host;Database=GrcMvcDb;Username=your-user;Password=your-password;Port=5432"
export ConnectionStrings__GrcAuthDb="Host=your-db-host;Database=GrcAuthDb;Username=your-user;Password=your-password;Port=5432"

# Application Environment
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_URLS="http://0.0.0.0:8080"
```

### Windows (PowerShell)
```powershell
$env:JWT_SECRET="YOUR_64_CHARACTER_SECRET_HERE"
$env:ConnectionStrings__DefaultConnection="Host=your-db-host;Database=GrcMvcDb;Username=your-user;Password=your-password;Port=5432"
$env:ConnectionStrings__GrcAuthDb="Host=your-db-host;Database=GrcAuthDb;Username=your-user;Password=your-password;Port=5432"
$env:ASPNETCORE_ENVIRONMENT="Production"
```

### Docker/Kubernetes
```yaml
env:
  - name: JWT_SECRET
    valueFrom:
      secretKeyRef:
        name: jwt-secret
        key: JWT_SECRET
  - name: ConnectionStrings__DefaultConnection
    valueFrom:
      secretKeyRef:
        name: db-secret
        key: CONNECTION_STRING
  - name: ConnectionStrings__GrcAuthDb
    valueFrom:
      secretKeyRef:
        name: db-secret
        key: AUTH_CONNECTION_STRING
```

---

## 🚀 Step 2: Deploy Application

### Option A: Direct Deployment

```bash
cd bin/Release/net8.0
dotnet GrcMvc.dll
```

### Option B: Docker Deployment

```bash
docker build -t shahin-grc:latest -f Dockerfile.production .
docker run -d \
  --name grc-production \
  --restart unless-stopped \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e JWT_SECRET="${JWT_SECRET}" \
  -e "ConnectionStrings__DefaultConnection=${DB_CONNECTION}" \
  -e "ConnectionStrings__GrcAuthDb=${AUTH_DB_CONNECTION}" \
  shahin-grc:latest
```

### Option C: Systemd Service (Linux)

Create `/etc/systemd/system/grc-portal.service`:

```ini
[Unit]
Description=Shahin GRC Portal
After=network.target postgresql.service

[Service]
Type=notify
User=www-data
WorkingDirectory=/opt/shahin/grc/bin/Release/net8.0
ExecStart=/usr/bin/dotnet /opt/shahin/grc/bin/Release/net8.0/GrcMvc.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=JWT_SECRET=YOUR_SECRET_HERE
Environment=ConnectionStrings__DefaultConnection=YOUR_CONNECTION_STRING
Environment=ConnectionStrings__GrcAuthDb=YOUR_AUTH_CONNECTION_STRING

[Install]
WantedBy=multi-user.target
```

Then:
```bash
sudo systemctl daemon-reload
sudo systemctl enable grc-portal
sudo systemctl start grc-portal
sudo systemctl status grc-portal
```

---

## ✅ Step 3: Monitor Startup Logs

Watch for these critical messages:

```
🔄 Creating database schema...
✅ Database schema created
🔄 Applying Auth database migrations...
✅ Auth database migrations applied
```

**If you see errors:**
- ❌ "JWT_SECRET environment variable is required" → Set `JWT_SECRET`
- ❌ "Database connection failed" → Check connection strings
- ❌ "Migration failed" → Check database permissions

---

## 🔍 Step 4: Verify Database Schema

### Connect to GrcAuthDb Database

```bash
psql -h your-db-host -U your-user -d GrcAuthDb
```

### Run Verification Queries

```sql
-- 1. Check if AspNetUsers table exists
SELECT EXISTS (
    SELECT FROM information_schema.tables 
    WHERE table_name = 'AspNetUsers'
);
-- Expected: true

-- 2. List all ApplicationUser custom columns
SELECT 
    column_name, 
    data_type, 
    is_nullable, 
    column_default
FROM information_schema.columns
WHERE table_name = 'AspNetUsers'
AND column_name IN (
    'FirstName', 'LastName', 'Department', 'JobTitle',
    'RoleProfileId', 'KsaCompetencyLevel',
    'KnowledgeAreas', 'Skills', 'Abilities', 'AssignedScope',
    'IsActive', 'CreatedDate', 'LastLoginDate',
    'RefreshToken', 'RefreshTokenExpiry',
    'MustChangePassword', 'LastPasswordChangedAt'
)
ORDER BY column_name;

-- Expected: 17 rows

-- 3. Verify indexes
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'AspNetUsers'
AND indexname IN (
    'IX_AspNetUsers_Email',
    'IX_AspNetUsers_IsActive',
    'IX_AspNetUsers_RoleProfileId'
);

-- Expected: 3 indexes

-- 4. Check migration history
SELECT * FROM "__EFMigrationsHistory"
WHERE "ContextKey" LIKE '%GrcAuthDbContext%'
ORDER BY "MigrationId" DESC;

-- Expected: Should see "20260115064458_AddApplicationUserCustomColumns"
```

---

## 🧪 Step 5: Test Application

### 5.1 Health Check

```bash
curl http://localhost:8080/health/ready
```

**Expected Response:**
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy"
  }
}
```

### 5.2 Test User Creation

1. Navigate to: `http://your-domain/Account/Register` or `/Users/Create`
2. Fill in all fields including:
   - First Name
   - Last Name
   - Department
   - Job Title
   - Abilities (JSON array)
   - Assigned Scope
   - KSA Competency Level
3. Submit the form
4. Verify the user was created successfully

### 5.3 Test User Editing

1. Navigate to: `/Users/Edit/{userId}`
2. Verify all fields load correctly:
   - ✅ FirstName, LastName
   - ✅ Department, JobTitle
   - ✅ Abilities, AssignedScope
   - ✅ All other ApplicationUser properties
3. Modify some fields
4. Save and verify changes persist

### 5.4 Verify Database Data

```sql
-- Check created user has all custom fields
SELECT 
    "Id",
    "Email",
    "FirstName",
    "LastName",
    "Department",
    "JobTitle",
    "Abilities",
    "AssignedScope",
    "KsaCompetencyLevel",
    "IsActive",
    "CreatedDate"
FROM "AspNetUsers"
ORDER BY "CreatedDate" DESC
LIMIT 5;
```

---

## 📋 Production Readiness Checklist

### Pre-Deployment
- [ ] Release build completed successfully
- [ ] All environment variables set
- [ ] Database connection strings configured
- [ ] JWT_SECRET generated (64+ characters)
- [ ] Database backups configured

### Deployment
- [ ] Application deployed
- [ ] Application starts without errors
- [ ] Migrations applied successfully (check logs)
- [ ] Health check endpoint responds

### Post-Deployment Verification
- [ ] `AspNetUsers` table exists
- [ ] All 17 custom columns present
- [ ] All 3 indexes created
- [ ] Migration history shows `AddApplicationUserCustomColumns`
- [ ] User creation form works
- [ ] User editing form works
- [ ] All ApplicationUser properties save/load correctly

### Security
- [ ] HTTPS enabled (if applicable)
- [ ] JWT_SECRET is secure and unique
- [ ] Database credentials are secure
- [ ] Firewall rules configured
- [ ] Logging configured

---

## 🔧 Troubleshooting

### Issue: "JWT_SECRET environment variable is required"

**Solution:**
```bash
export JWT_SECRET="$(openssl rand -base64 64)"
# Or set in your deployment configuration
```

### Issue: "Migration failed" or "Table already exists"

**Solution:**
```bash
# Check migration status
dotnet ef migrations list --context GrcAuthDbContext

# If migration is pending, apply it
dotnet ef database update --context GrcAuthDbContext
```

### Issue: Missing columns in AspNetUsers

**Solution:**
1. Verify migration was applied:
   ```sql
   SELECT * FROM "__EFMigrationsHistory" 
   WHERE "MigrationId" = '20260115064458_AddApplicationUserCustomColumns';
   ```

2. If migration not applied, run:
   ```bash
   dotnet ef database update --context GrcAuthDbContext
   ```

3. If migration applied but columns missing, check `Program.cs` uses `Migrate()` not `EnsureCreated()`

### Issue: Application won't start

**Check:**
1. Environment variables are set correctly
2. Database is accessible
3. Port 8080 (or configured port) is available
4. Check application logs for specific error

---

## 📊 Monitoring

### Key Metrics to Monitor

1. **Application Health**
   - Health check endpoint: `/health/ready`
   - Response time
   - Error rate

2. **Database**
   - Connection pool usage
   - Query performance
   - Migration status

3. **Authentication**
   - Login success rate
   - JWT token generation
   - User creation success

### Log Locations

- **Application Logs**: Check stdout/stderr or configured log file
- **Database Logs**: PostgreSQL logs
- **System Logs**: `/var/log/syslog` (Linux) or Event Viewer (Windows)

---

## ✅ Success Criteria

Your deployment is successful when:

1. ✅ Application starts without errors
2. ✅ Migrations applied (check logs)
3. ✅ `AspNetUsers` table has all 17 custom columns
4. ✅ User creation form works
5. ✅ User editing form works
6. ✅ All ApplicationUser properties save/load correctly
7. ✅ Health check returns "Healthy"

---

## 🎉 Production Ready!

Once all checks pass, your application is ready for production use. The Identity schema is properly configured with all `ApplicationUser` properties, and migrations will automatically apply on future deployments.
