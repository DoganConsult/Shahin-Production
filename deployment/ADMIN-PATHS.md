# Admin Portal Paths Configuration

## Two Admin Paths

### 1. `/admin/*` - AdminPortalController
- **Route**: `/admin/*`
- **Controller**: `AdminPortalController`
- **Access**: Platform Admins only (ActivePlatformAdmin policy)
- **Main Routes**:
  - `/admin/login` - Platform Admin Login
  - `/admin/dashboard` - Admin Dashboard
  - `/admin/tenants` - Tenant Management
  - `/admin/users` - User Management

### 2. `/platform-admin/*` - PlatformAdminMvcController
- **Route**: `/platform-admin/*`
- **Controller**: `PlatformAdminMvcController`
- **Access**: Platform Admins only (ActivePlatformAdmin policy)
- **Main Routes**:
  - `/platform-admin` - Platform Admin Dashboard
  - `/platform-admin/tenants` - Tenant Management
  - `/platform-admin/users` - User Management
  - `/platform-admin/catalogs` - Catalog Management

## DNS Configuration

**admin.shahin-ai.com** → Routes to both:
- `http://admin.shahin-ai.com/admin/*`
- `http://admin.shahin-ai.com/platform-admin/*`

## Nginx Configuration

Both paths are configured in `nginx/production-212.147.229.36.conf`:

```nginx
# Admin routes: /admin/*
location /admin {
    proxy_pass http://portal_upstream;
    ...
}

# Platform Admin routes: /platform-admin/*
location /platform-admin {
    proxy_pass http://portal_upstream;
    ...
}
```

## Testing

After deployment, test both paths:

```bash
# Test admin path
curl http://admin.shahin-ai.com/admin/login

# Test platform-admin path
curl http://admin.shahin-ai.com/platform-admin

# Test health
curl http://admin.shahin-ai.com/health
```

## Access URLs

- **Admin Login**: http://admin.shahin-ai.com/admin/login
- **Platform Admin**: http://admin.shahin-ai.com/platform-admin
- **Health Check**: http://admin.shahin-ai.com/health
