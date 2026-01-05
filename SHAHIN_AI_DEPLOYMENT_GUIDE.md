# Shahin AI Production Deployment Guide

## 🌐 Domain Configuration

### DNS Records (Cloudflare)
All domains point to: **157.180.105.48**

| Type | Name | Content | Proxy Status |
|------|------|---------|--------------|
| A | shahin-ai.com | 157.180.105.48 | Proxied |
| A | www | 157.180.105.48 | Proxied |
| A | portal | 157.180.105.48 | Proxied |
| A | app | 157.180.105.48 | Proxied |
| A | login | 157.180.105.48 | Proxied |

### Routing Configuration

| Domain | Purpose | Backend | Port |
|--------|---------|---------|------|
| **shahin-ai.com** / **www.shahin-ai.com** | Landing Page (Marketing) | Next.js | 3000 |
| **portal.shahin-ai.com** / **app.shahin-ai.com** | GRC Application | Blazor (.NET) | 8080 |
| **login.shahin-ai.com** | Login Redirect | → portal.shahin-ai.com/login | Redirect |

---

## 🚀 Deployment Steps

### Prerequisites

1. **Server**: 157.180.105.48
2. **Node.js**: v18+ (for Next.js)
3. **.NET 8.0 SDK**: (for Blazor)
4. **PostgreSQL**: Running on port 5432/5433
5. **Nginx**: Installed and configured
6. **SSL Certificates**: Let's Encrypt for all domains

### Step 1: Prepare Next.js Landing Page

```bash
cd /home/dogan/grc-system

# Create Next.js project (if not exists)
mkdir -p shahin-ai-website
cd shahin-ai-website

# Initialize Next.js (one-time setup)
# Follow SHAHIN_AI_NEXTJS_COMPLETE_STRUCTURE.md for complete structure

# Install dependencies
npm install

# Build for production
npm run build

# Start Next.js (will run on port 3000)
npm start
```

### Step 2: Prepare Blazor Application

```bash
cd /home/dogan/grc-system/src/GrcMvc

# Set environment variables
export ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432"
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://localhost:8080"

# Build
dotnet build -c Release

# Run (will run on port 8080)
dotnet run --no-build
```

### Step 3: Configure Nginx

```bash
# Copy nginx configuration
sudo cp /home/dogan/grc-system/nginx-shahin-ai-production.conf /etc/nginx/sites-available/shahin-ai.com

# Enable site
sudo ln -s /etc/nginx/sites-available/shahin-ai.com /etc/nginx/sites-enabled/shahin-ai.com

# Test configuration
sudo nginx -t

# Reload nginx
sudo systemctl reload nginx
```

### Step 4: SSL Certificates

```bash
# Install certbot if not installed
sudo apt-get install certbot python3-certbot-nginx

# Obtain certificates for all domains
sudo certbot --nginx -d shahin-ai.com -d www.shahin-ai.com
sudo certbot --nginx -d portal.shahin-ai.com -d app.shahin-ai.com
sudo certbot --nginx -d login.shahin-ai.com

# Auto-renewal (already configured by certbot)
sudo certbot renew --dry-run
```

### Step 5: Run Deployment Script

```bash
cd /home/dogan/grc-system
./scripts/deploy-shahin-ai-production.sh
```

---

## 📋 Quick Deployment Checklist

### Before Deployment

- [ ] DNS records configured (all pointing to 157.180.105.48)
- [ ] SSL certificates obtained for all domains
- [ ] Next.js landing page built and ready
- [ ] Blazor application built and tested
- [ ] Database migrations applied
- [ ] Environment variables configured
- [ ] Firewall rules allow ports 80, 443, 3000, 8080

### During Deployment

- [ ] Next.js starts on port 3000
- [ ] Blazor starts on port 8080
- [ ] Nginx configuration valid
- [ ] Nginx reloaded successfully
- [ ] Health checks pass

### After Deployment

- [ ] https://shahin-ai.com loads landing page
- [ ] https://portal.shahin-ai.com loads application
- [ ] https://login.shahin-ai.com redirects to portal login
- [ ] Login functionality works
- [ ] API endpoints respond correctly
- [ ] Static assets load correctly
- [ ] SSL certificates valid (no browser warnings)

---

## 🔧 Service Management

### Start Services

```bash
# Start Next.js (landing page)
cd /home/dogan/grc-system/shahin-ai-website
npm start > /tmp/nextjs.log 2>&1 &

# Start Blazor (application)
cd /home/dogan/grc-system/src/GrcMvc
export ConnectionStrings__DefaultConnection="Host=localhost;Database=GrcMvcDb;Username=postgres;Password=postgres;Port=5432"
export ASPNETCORE_URLS="http://localhost:8080"
dotnet run > /tmp/grcmvc.log 2>&1 &

# Restart Nginx
sudo systemctl restart nginx
```

### Stop Services

```bash
# Stop Next.js
pkill -f "next start"

# Stop Blazor
pkill -f "dotnet.*GrcMvc"

# Stop Nginx
sudo systemctl stop nginx
```

### View Logs

```bash
# Next.js logs
tail -f /tmp/nextjs.log

# Blazor logs
tail -f /tmp/grcmvc.log

# Nginx access logs
tail -f /var/log/nginx/shahin_landing_access.log
tail -f /var/log/nginx/grc_portal_access.log

# Nginx error logs
tail -f /var/log/nginx/shahin_landing_error.log
tail -f /var/log/nginx/grc_portal_error.log
```

---

## 🔍 Troubleshooting

### Next.js Not Starting

```bash
# Check if port 3000 is in use
sudo lsof -i :3000

# Check Node.js version
node --version  # Should be v18+

# Check Next.js logs
tail -f /tmp/nextjs.log

# Restart Next.js
pkill -f "next start"
cd /home/dogan/grc-system/shahin-ai-website
npm start
```

### Blazor Not Starting

```bash
# Check if port 8080 is in use
sudo lsof -i :8080

# Check .NET version
dotnet --version  # Should be 8.0.x

# Check database connection
psql -h localhost -U postgres -d GrcMvcDb -c "SELECT 1;"

# Check Blazor logs
tail -f /tmp/grcmvc.log

# Restart Blazor
pkill -f "dotnet.*GrcMvc"
cd /home/dogan/grc-system/src/GrcMvc
dotnet run
```

### Nginx Issues

```bash
# Test configuration
sudo nginx -t

# Check nginx status
sudo systemctl status nginx

# View nginx error log
sudo tail -f /var/log/nginx/error.log

# Reload nginx
sudo systemctl reload nginx
```

### SSL Certificate Issues

```bash
# Check certificate expiration
sudo certbot certificates

# Renew certificates
sudo certbot renew

# Test renewal
sudo certbot renew --dry-run
```

---

## 🔐 Security Checklist

- [ ] SSL certificates valid and auto-renewing
- [ ] HTTPS enforced (HTTP → HTTPS redirect)
- [ ] Security headers configured (HSTS, CSP, etc.)
- [ ] Rate limiting enabled (API, login endpoints)
- [ ] Firewall configured (only necessary ports open)
- [ ] Database credentials secure (not in code)
- [ ] Environment variables set securely
- [ ] Logs don't contain sensitive information

---

## 📊 Monitoring

### Health Checks

```bash
# Landing page health
curl https://shahin-ai.com/health

# Portal health
curl https://portal.shahin-ai.com/health

# API health
curl https://portal.shahin-ai.com/api/health
```

### Performance Monitoring

- Monitor Next.js process: `ps aux | grep next`
- Monitor Blazor process: `ps aux | grep dotnet`
- Monitor Nginx: `sudo systemctl status nginx`
- Monitor database: `docker ps | grep grc-db`

---

## 🎯 Post-Deployment Verification

### Landing Page (shahin-ai.com)

- [ ] Homepage loads correctly
- [ ] Arabic/English switching works
- [ ] RTL layout works for Arabic
- [ ] All images and assets load
- [ ] Login link redirects to portal
- [ ] Contact form works (if implemented)

### Portal (portal.shahin-ai.com)

- [ ] Login page loads
- [ ] User can log in
- [ ] Dashboard displays correctly
- [ ] Menu items visible (RBAC-based)
- [ ] API endpoints respond
- [ ] Workflows accessible
- [ ] Reports generate

### Login Redirect (login.shahin-ai.com)

- [ ] Redirects to portal.shahin-ai.com/Account/Login
- [ ] SSL certificate valid
- [ ] No redirect loops

---

## 📝 Deployment Script Usage

```bash
# Full deployment
./scripts/deploy-shahin-ai-production.sh

# Manual step-by-step
# 1. Deploy Next.js
cd shahin-ai-website && npm start &

# 2. Deploy Blazor
cd src/GrcMvc && dotnet run &

# 3. Configure Nginx
sudo cp nginx-shahin-ai-production.conf /etc/nginx/sites-available/shahin-ai.com
sudo ln -s /etc/nginx/sites-available/shahin-ai.com /etc/nginx/sites-enabled/
sudo nginx -t && sudo systemctl reload nginx
```

---

## ⚠️ Important Notes

1. **Next.js Landing Page**: Must be created first using `SHAHIN_AI_NEXTJS_COMPLETE_STRUCTURE.md`
2. **SSL Certificates**: Must be obtained before nginx configuration works
3. **Ports**: Ensure ports 3000 and 8080 are not blocked by firewall
4. **Database**: Ensure PostgreSQL is running and accessible
5. **Environment Variables**: Set connection strings and secrets securely

---

**Status**: ⏳ **PENDING DEPLOYMENT**
**Last Updated**: 2026-01-22
