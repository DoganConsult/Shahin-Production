# Platform Admin Access Setup

## 🔐 Super Admin Credentials

### Production Access
```
URL: https://yourdomain.com/admin
Username: admin@platform.local  
Password: Admin@Platform2026!
```

### Local Development Access
```
URL: http://localhost:5000/admin
Username: admin@platform.local
Password: Admin@Platform2026!
```

## Setup Instructions

### Method 1: Using DBeaver or pgAdmin
1. Connect to your Railway PostgreSQL database:
   - Host: caboose.proxy.rlwy.net
   - Port: 11527
   - Database: GrcMvcDb
   - Username: postgres
   - Password: QNcTvViWopMfCunsyIkkXwuDpufzhkLs
   - SSL Mode: Require

2. Execute the SQL script: `Scripts/CreatePlatformSuperAdmin.sql`

### Method 2: Using dotnet-ef
```powershell
cd C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc
dotnet ef database execute --sql-file Scripts/CreatePlatformSuperAdmin.sql
```

### Method 3: Direct SQL Execution
Run the following SQL commands directly in your database:

```sql
-- Create platform admin tenant
INSERT INTO "Tenants" (
    "Id", "TenantSlug", "OrganizationName", "TenantCode", 
    "Status", "IsActive", "SubscriptionTier", "CreatedDate"
) VALUES (
    '00000000-0000-0000-0000-000000000000',
    'admin',
    'Platform Administration',
    'ADMIN',
    'Active',
    true,
    'Enterprise',
    NOW()
) ON CONFLICT ("Id") DO NOTHING;

-- Create admin user (password: Admin@Platform2026!)
INSERT INTO "AspNetUsers" (
    "Id", "UserName", "Email", "EmailConfirmed",
    "PasswordHash", "TenantId", "FirstName", "LastName", "IsActive"
) VALUES (
    'superadmin-0000-0000-0000-000000000000',
    'admin@platform.local',
    'admin@platform.local',
    true,
    'AQAAAAEAACcQAAAAEGqK8wqYx5JhV6+L9yH3xQzVKQH7zYlHkPQj2B+hGU4nDp3gV7rKfQ2VQHZxJKTkXg==',
    '00000000-0000-0000-0000-000000000000',
    'Platform',
    'Administrator',
    true
) ON CONFLICT ("Id") DO NOTHING;
```

## Platform Admin Capabilities

Once logged in as platform admin, you can:

1. **Tenant Management**
   - View all tenants
   - Create new tenants
   - Activate/Deactivate tenants
   - Manage subscriptions

2. **User Management**
   - Create admin users for any tenant
   - Reset passwords
   - Manage roles and permissions

3. **System Configuration**
   - Configure GRC policies
   - Manage system settings
   - View audit logs

4. **Monitoring**
   - View system health
   - Monitor tenant usage
   - Review security events

## Security Notes

⚠️ **IMPORTANT**: 
- Change the default password immediately after first login
- Enable 2FA for the super admin account
- Limit access to this account to authorized personnel only
- Regularly audit super admin actions

## Access Routes

The platform admin has access to special routes:
- `/admin` - Admin dashboard
- `/admin/tenants` - Tenant management
- `/admin/users` - User management
- `/admin/settings` - System settings
- `/admin/logs` - Audit logs
