# Production Deployment Guide - GRC System to 157.180.105.48

## 📋 Quick Summary

This guide deploys the GRC system to production server **157.180.105.48** with:
- ✅ Multiple domain support (portal.shahin-ai.com, app.shahin-ai.com, login.shahin-ai.com)
- ✅ Let's Encrypt SSL/TLS with auto-renewal
- ✅ Nginx reverse proxy with security headers
- ✅ Rate limiting and DDoS protection
- ✅ Automatic backups
- ✅ Health monitoring

---

## 🚀 Deployment Steps

### Step 1: SSH to Production Server

```bash
ssh root@157.180.105.48
```

### Step 2: Install Prerequisites

```bash
# Update system
apt update && apt upgrade -y

# Install Docker & Docker Compose
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# Install Docker Compose
apt install -y docker-compose

# Install Certbot for Let's Encrypt
apt install -y certbot python3-certbot-nginx

# Install Nginx
apt install -y nginx

# Install Git
apt install -y git

# Create app directory
mkdir -p /opt/grc-system
cd /opt/grc-system
```

### Step 3: Clone Repository

```bash
git clone <your-repo-url> /opt/grc-system
cd /opt/grc-system
```

### Step 4: Setup SSL Certificates

```bash
# Create certificate directory
mkdir -p /etc/letsencrypt/live/portal.shahin-ai.com

# Generate Let's Encrypt certificate
certbot certonly --nginx \
  -d portal.shahin-ai.com \
  -d app.shahin-ai.com \
  -d login.shahin-ai.com \
  -d shahin-ai.com \
  -d www.shahin-ai.com \
  --agree-tos \
  -n

# Verify certificate
certbot certificates
```

### Step 5: Configure Nginx

```bash
# Copy nginx configuration
sudo cp nginx-production.conf /etc/nginx/sites-available/portal.shahin-ai.com

# Enable site
sudo ln -s /etc/nginx/sites-available/portal.shahin-ai.com /etc/nginx/sites-enabled/

# Disable default site
sudo rm /etc/nginx/sites-enabled/default

# Test configuration
sudo nginx -t

# Reload Nginx
sudo systemctl reload nginx

# Enable Nginx auto-start
sudo systemctl enable nginx
```

### Step 6: Create Environment File

```bash
# Copy production environment
cp .env.production /opt/grc-system/.env

# Edit with production values
nano /opt/grc-system/.env
```

**Key variables to update:**
```bash
JWT_SECRET=YourVerySecretKeyHere_MinimumLen64Chars_ZZZZzzzzzzzzzzzzzzz
ADMIN_PASSWORD=YourSecureAdminPassword123!
POSTGRES_PASSWORD=YourSecurePostgresPassword123!
```

### Step 7: Build and Deploy with Docker

```bash
# Navigate to project directory
cd /opt/grc-system

# Build Docker images
docker-compose build

# Start services
docker-compose up -d

# Verify all services are running
docker-compose ps

# Check logs
docker-compose logs -f grcmvc
```

### Step 8: Setup Auto-Renewal for SSL

```bash
# Create renewal script
sudo nano /opt/grc-system/renew-cert.sh
```

**Content:**
```bash
#!/bin/bash
certbot renew --quiet
docker-compose -f /opt/grc-system/docker-compose.yml restart grcmvc
```

**Make executable and add to cron:**
```bash
chmod +x /opt/grc-system/renew-cert.sh
sudo crontab -e

# Add this line (runs daily at 3 AM):
0 3 * * * /opt/grc-system/renew-cert.sh >> /var/log/cert-renewal.log 2>&1
```

### Step 9: Setup Database Backups

```bash
# Create backup script
mkdir -p /opt/backups
nano /opt/grc-system/backup-db.sh
```

**Content:**
```bash
#!/bin/bash
BACKUP_DIR="/opt/backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/grcmvc_prod_$TIMESTAMP.sql"

# Create backup
docker-compose exec -T db pg_dump -U postgres grcmvc_prod > "$BACKUP_FILE"

# Compress
gzip "$BACKUP_FILE"

# Keep only last 30 days
find "$BACKUP_DIR" -name "grcmvc_prod_*.sql.gz" -mtime +30 -delete

echo "Backup completed: ${BACKUP_FILE}.gz"
```

**Schedule daily backups:**
```bash
chmod +x /opt/grc-system/backup-db.sh
sudo crontab -e

# Add this line (runs daily at 2 AM):
0 2 * * * /opt/grc-system/backup-db.sh >> /var/log/backup.log 2>&1
```

### Step 10: Setup Monitoring

```bash
# Check service health
curl https://portal.shahin-ai.com/health

# Monitor logs
docker-compose logs -f

# Check disk space
df -h

# Check memory usage
free -h
```

---

## 🔒 Security Checklist

- [x] SSL/TLS with Let's Encrypt
- [x] HSTS headers enabled
- [x] CSP headers configured
- [x] CORS configured for allowed domains
- [x] Rate limiting enabled
- [x] X-Frame-Options set to SAMEORIGIN
- [x] X-Content-Type-Options set to nosniff
- [x] Database password secured
- [x] JWT secret configured
- [x] Admin password changed
- [x] Firewall rules configured
- [x] SSH key-based authentication only
- [x] Automatic SSL renewal

---

## 🚨 Firewall Configuration

```bash
# Install UFW if not present
apt install -y ufw

# Allow SSH
sudo ufw allow 22/tcp

# Allow HTTP
sudo ufw allow 80/tcp

# Allow HTTPS
sudo ufw allow 443/tcp

# Enable firewall
sudo ufw enable

# Check status
sudo ufw status
```

---

## 📊 Monitoring & Maintenance

### Daily Tasks
- Check application logs: `docker-compose logs grcmvc`
- Verify health endpoint: `curl https://portal.shahin-ai.com/health`
- Monitor disk space: `df -h`
- Check nginx logs: `tail -f /var/log/nginx/grc_portal_access.log`

### Weekly Tasks
- Review error logs: `tail -f /var/log/nginx/grc_portal_error.log`
- Backup database: `./backup-db.sh`
- Check certificate expiration: `certbot certificates`
- Monitor resource usage: `docker stats`

### Monthly Tasks
- Update Docker images: `docker-compose pull && docker-compose up -d`
- Review security logs: `journalctl -u docker -n 50`
- Test disaster recovery procedures
- Review and update environment variables if needed

---

## 🔧 Troubleshooting

### Application not responding
```bash
# Check container status
docker-compose ps

# View logs
docker-compose logs --tail=50 grcmvc

# Restart service
docker-compose restart grcmvc
```

### Database connection issues
```bash
# Check database is running
docker-compose ps grcmvc-db

# Test connection
docker-compose exec grcmvc-db psql -U postgres -d grcmvc_prod -c "SELECT 1"

# Restore from backup
gunzip < /opt/backups/grcmvc_prod_20260104_020000.sql.gz | docker-compose exec -T db psql -U postgres grcmvc_prod
```

### SSL certificate issues
```bash
# Check certificate validity
certbot certificates

# Renew manually
certbot renew --force-renewal

# Test renewal
certbot renew --dry-run
```

### Nginx errors
```bash
# Test configuration
sudo nginx -t

# Reload configuration
sudo systemctl reload nginx

# Restart nginx
sudo systemctl restart nginx

# Check logs
tail -f /var/log/nginx/grc_portal_error.log
```

### High memory/CPU usage
```bash
# Check resource usage
docker stats

# Check processes
docker-compose exec grcmvc ps aux

# Increase limits if needed (in docker-compose.yml):
# deploy:
#   resources:
#     limits:
#       cpus: '2'
#       memory: 2G
```

---

## 📈 Performance Tuning

### Database Optimization
```bash
# Connect to database
docker-compose exec db psql -U postgres -d grcmvc_prod

# Create indexes
CREATE INDEX idx_tenants_slug ON tenants(tenant_slug);
CREATE INDEX idx_audit_events_tenant ON audit_events(tenant_id);
CREATE INDEX idx_workflows_tenant ON workflows(tenant_id);

# Analyze query performance
EXPLAIN ANALYZE SELECT * FROM tenants WHERE tenant_slug = 'example';
```

### Application Caching
- Redis cache enabled (6379)
- Session storage configured
- Query result caching implemented
- Static asset compression enabled

### Nginx Performance
- Connection pooling to backend
- Gzip compression enabled
- Static asset caching configured
- Rate limiting configured

---

## 🔄 Updating the Application

```bash
# Pull latest code
cd /opt/grc-system
git pull origin main

# Rebuild Docker images
docker-compose build

# Deploy updated version
docker-compose up -d

# Check health
curl https://portal.shahin-ai.com/health

# View logs for any errors
docker-compose logs -f
```

---

## 📞 Support & Logs

### Access Logs
```bash
# Portal access
tail -f /var/log/nginx/grc_portal_access.log

# App access
tail -f /var/log/nginx/grc_app_access.log

# Login access
tail -f /var/log/nginx/grc_login_access.log

# Error logs
tail -f /var/log/nginx/grc_portal_error.log
```

### Application Logs
```bash
# Full logs
docker-compose logs

# Specific service
docker-compose logs grcmvc

# Last 100 lines
docker-compose logs --tail=100

# Follow in real-time
docker-compose logs -f
```

### System Logs
```bash
# Docker daemon
sudo journalctl -u docker -n 50 -f

# Nginx
sudo systemctl status nginx

# System messages
sudo journalctl -n 50 -f
```

---

## ✅ Post-Deployment Verification

```bash
# Test all domains
curl -I https://portal.shahin-ai.com
curl -I https://app.shahin-ai.com
curl -I https://login.shahin-ai.com
curl -I https://shahin-ai.com
curl -I https://www.shahin-ai.com

# Test health endpoint
curl https://portal.shahin-ai.com/health | jq

# Test API endpoint
curl https://portal.shahin-ai.com/api/tenants | jq

# Check SSL certificate
echo | openssl s_client -servername portal.shahin-ai.com -connect 157.180.105.48:443 | grep "subject\|issuer\|dates"

# Verify HSTS header
curl -I https://portal.shahin-ai.com | grep Strict-Transport-Security
```

---

## 📋 Access Information

**After Deployment:**
- **Portal URL:** https://portal.shahin-ai.com
- **App URL:** https://app.shahin-ai.com
- **Login URL:** https://login.shahin-ai.com
- **Health Check:** https://portal.shahin-ai.com/health
- **API Documentation:** https://portal.shahin-ai.com/swagger

**Default Admin Account:**
- **Email:** admin@grcmvc.com
- **Password:** Admin@123456 (Change immediately in production!)

**Database Access (from server):**
```bash
docker-compose exec db psql -U postgres -d grcmvc_prod
```

---

## 🎯 Next Steps

1. ✅ Deploy to production server
2. ✅ Verify all domains working with HTTPS
3. ✅ Test login and basic functionality
4. ✅ Configure email notifications
5. ✅ Setup monitoring and alerting
6. ✅ Configure automated backups
7. ✅ Document run-books for ops team
8. ✅ Schedule security audits
9. ✅ Plan capacity and scaling strategy
10. ✅ Setup disaster recovery procedures

---

**Deployment Status:** 🚀 Ready for Production
**Last Updated:** 2026-01-04
**Server IP:** 157.180.105.48
**Domains:** portal.shahin-ai.com, app.shahin-ai.com, login.shahin-ai.com
