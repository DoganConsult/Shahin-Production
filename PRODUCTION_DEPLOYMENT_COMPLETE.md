# 🚀 Shahin AI - Complete Production Deployment Guide

## Overview

This guide provides a complete production deployment solution for the Shahin AI GRC platform, including:

- **Landing Page** (React) - `www.shahin-ai.com`
- **GRC Portal** (ASP.NET Core) - `portal.shahin-ai.com`
- **PostgreSQL Database** - Multi-tenant GRC data
- **Redis Cache** - Session and caching
- **Nginx Reverse Proxy** - Domain routing
- **Cloudflare Tunnel** - Secure external access
- **SSL/TLS** - Automatic certificates
- **Monitoring** - Health checks and logging

---

## 📋 Prerequisites

### Server Requirements
- **Ubuntu 20.04+** or **CentOS 7+**
- **4GB RAM minimum** (8GB recommended)
- **2 CPU cores minimum** (4 recommended)
- **50GB storage** (SSD preferred)
- **Root or sudo access**

### Domain Setup
- `www.shahin-ai.com` → Landing page
- `portal.shahin-ai.com` → GRC portal
- `staging.shahin-ai.com` → Staging environment (optional)
- `dev.shahin-ai.com` → Development (optional)

### Cloudflare Setup
1. **Add domains** to Cloudflare
2. **Install cloudflared** on server
3. **Create tunnel** for each domain
4. **Configure DNS** records

---

## ⚡ Quick Production Deployment

### Step 1: Server Preparation

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install required packages
sudo apt install -y curl wget git unzip software-properties-common apt-transport-https ca-certificates gnupg lsb-release

# Install Docker
curl -fsSL https://get.docker.com | sh
sudo systemctl enable docker
sudo systemctl start docker

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Install nginx
sudo apt install -y nginx

# Create deployment directory
sudo mkdir -p /opt/shahin-ai
sudo chown $USER:$USER /opt/shahin-ai
cd /opt/shahin-ai
```

### Step 2: Clone Repository

```bash
# Clone the repository
git clone https://github.com/your-org/shahin-ai.git .
cd Shahin-Jan-2026

# Make scripts executable
chmod +x deployment/deploy-full-stack.sh
chmod +x scripts/*.sh
```

### Step 3: Configure Environment

```bash
# Copy environment template
cp .env.production.template .env.production

# Edit with your settings
nano .env.production
```

**Required Environment Variables:**

```bash
# Database
DB_USER=shahin_admin
DB_PASSWORD=your_secure_db_password
DB_NAME=GrcMvcDb

# JWT Authentication
JWT_SECRET=your_256_bit_jwt_secret_key_here

# SMTP (for email verification)
SMTP_USERNAME=info@shahin-ai.com
SMTP_PASSWORD=your_smtp_password

# Azure AD (optional)
AZURE_TENANT_ID=your_tenant_id
AZURE_CLIENT_ID=your_client_id
AZURE_CLIENT_SECRET=your_client_secret

# Claude AI (optional)
CLAUDE_API_KEY=your_claude_api_key

# Cloudflare
CLOUDFLARE_TUNNEL_TOKEN=your_tunnel_token
```

### Step 4: Generate Secure Secrets

```bash
# Generate JWT secret (256-bit)
openssl rand -hex 32

# Generate database password
openssl rand -base64 24

# Generate other secrets as needed
```

### Step 5: Deploy Full Stack

```bash
# Run full deployment
./deployment/deploy-full-stack.sh deploy

# Check status
./deployment/deploy-full-stack.sh status
```

### Step 6: Configure Nginx

```bash
# Copy nginx configuration
sudo cp deployment/nginx/shahin-all-domains.conf /etc/nginx/sites-available/shahin-ai

# Enable site
sudo ln -s /etc/nginx/sites-available/shahin-ai /etc/nginx/sites-enabled/

# Test configuration
sudo nginx -t

# Reload nginx
sudo systemctl reload nginx
```

### Step 7: Setup Cloudflare Tunnel

```bash
# Install cloudflared
curl -L --output cloudflared.deb https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb
sudo dpkg -i cloudflared.deb

# Login to Cloudflare
cloudflared tunnel login

# Create tunnels for each domain
cloudflared tunnel create shahin-landing
cloudflared tunnel create shahin-portal

# Create tunnel configuration
sudo nano /etc/cloudflared/config.yml
```

**Cloudflare Config (`/etc/cloudflared/config.yml`):**

```yaml
tunnel: shahin-landing
credentials-file: /root/.cloudflared/shahin-landing.json

ingress:
  - hostname: www.shahin-ai.com
    service: http://localhost:3000
  - hostname: shahin-ai.com
    service: http://localhost:3000
  - service: http_status:404

---
tunnel: shahin-portal
credentials-file: /root/.cloudflared/shahin-portal.json

ingress:
  - hostname: portal.shahin-ai.com
    service: http://localhost:5000
  - service: http_status:404
```

### Step 8: Setup SSL Certificates

```bash
# Install certbot
sudo apt install -y certbot python3-certbot-nginx

# Get SSL certificates (Cloudflare handles this automatically)
# No action needed - Cloudflare provides SSL
```

### Step 9: Configure Firewall

```bash
# Allow SSH, HTTP, HTTPS
sudo ufw allow ssh
sudo ufw allow 80
sudo ufw allow 443
sudo ufw --force enable

# Allow internal Docker communication
sudo ufw allow from 172.17.0.0/16 to any port 5432 proto tcp  # PostgreSQL
sudo ufw allow from 172.17.0.0/16 to any port 6379 proto tcp  # Redis
```

### Step 10: Setup Monitoring

```bash
# Install monitoring tools
sudo apt install -y htop iotop ncdu

# Setup log rotation
sudo nano /etc/logrotate.d/shahin-ai
```

**Log Rotation Config (`/etc/logrotate.d/shahin-ai`):**

```
/var/log/shahin-ai/*.log {
    daily
    missingok
    rotate 52
    compress
    delaycompress
    notifempty
    create 644 www-data www-data
    postrotate
        systemctl reload nginx
    endscript
}
```

---

## 🔧 Service Management

### Start/Stop Services

```bash
# Full stack
./deployment/deploy-full-stack.sh deploy
./deployment/deploy-full-stack.sh stop

# Individual services
./deployment/deploy-full-stack.sh landing
./deployment/deploy-full-stack.sh portal
```

### View Logs

```bash
# All logs
./deployment/deploy-full-stack.sh logs

# Specific service logs
./deployment/deploy-full-stack.sh logs landing
./deployment/deploy-full-stack.sh logs portal
```

### Check Status

```bash
# Service status
./deployment/deploy-full-stack.sh status

# Docker containers
docker ps

# Health checks
curl http://localhost:3000/health
curl http://localhost:5000/health
```

---

## 🔄 Database Management

### Backup Database

```bash
# Automated backup
./deployment/deploy-full-stack.sh backup

# Manual backup
docker exec shahin-postgres pg_dump -U shahin_admin GrcMvcDb | gzip > backup_$(date +%Y%m%d_%H%M%S).sql.gz
```

### Restore Database

```bash
# Stop portal before restore
./deployment/deploy-full-stack.sh stop

# Restore from backup
gunzip < backup_file.sql.gz | docker exec -i shahin-postgres psql -U shahin_admin -d GrcMvcDb

# Start services
./deployment/deploy-full-stack.sh deploy
```

### Database Migration

```bash
# Run migrations
docker exec shahin-portal dotnet ef database update --context GrcDbContext
docker exec shahin-portal dotnet ef database update --context GrcAuthDbContext
```

---

## 🔒 Security Configuration

### SSL/TLS Setup

```bash
# Cloudflare handles SSL automatically
# For direct SSL (if not using Cloudflare):

# Install certbot
sudo apt install -y certbot python3-certbot-nginx

# Get certificates
sudo certbot --nginx -d www.shahin-ai.com -d portal.shahin-ai.com

# Auto-renewal
sudo crontab -e
# Add: 0 12 * * * /usr/bin/certbot renew --quiet
```

### Firewall Rules

```bash
# Allow only necessary ports
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow ssh
sudo ufw allow 80
sudo ufw allow 443

# Allow Cloudflare IPs (recommended)
# https://www.cloudflare.com/ips/
```

### Database Security

```bash
# Change default passwords
# Use strong passwords for all services
# Enable PostgreSQL SSL
# Restrict database access to application only
```

---

## 📊 Monitoring & Maintenance

### Health Checks

```bash
# Application health
curl https://portal.shahin-ai.com/health
curl https://www.shahin-ai.com/health

# Database health
docker exec shahin-postgres pg_isready -U shahin_admin

# Redis health
docker exec shahin-redis redis-cli ping
```

### Log Monitoring

```bash
# Application logs
docker logs shahin-portal -f
docker logs shahin-landing -f

# Nginx logs
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log

# System logs
sudo journalctl -u docker -f
```

### Performance Monitoring

```bash
# System resources
htop
df -h
free -h

# Docker stats
docker stats

# Database performance
docker exec shahin-postgres psql -U shahin_admin -c "SELECT * FROM pg_stat_activity;"
```

---

## 🚨 Troubleshooting

### Common Issues

#### 1. Services Won't Start

```bash
# Check Docker status
sudo systemctl status docker

# Check logs
./deployment/deploy-full-stack.sh logs

# Check resource usage
docker system df
```

#### 2. Database Connection Issues

```bash
# Test database connection
docker exec shahin-postgres psql -U shahin_admin -d GrcMvcDb -c "SELECT 1;"

# Check database logs
docker logs shahin-postgres
```

#### 3. Application Errors

```bash
# Check application logs
docker logs shahin-portal

# Test health endpoint
curl http://localhost:5000/health

# Check environment variables
docker exec shahin-portal env | grep -E "(DB|JWT|SMTP)"
```

#### 4. Nginx Issues

```bash
# Test configuration
sudo nginx -t

# Check nginx status
sudo systemctl status nginx

# View nginx logs
sudo tail -f /var/log/nginx/error.log
```

---

## 📈 Scaling & Optimization

### Horizontal Scaling

```bash
# Add more portal instances
docker-compose up -d --scale portal=3

# Load balancer configuration needed
```

### Database Optimization

```bash
# PostgreSQL tuning
docker exec shahin-postgres psql -U shahin_admin -c "ALTER SYSTEM SET shared_buffers = '256MB';"
docker exec shahin-postgres psql -U shahin_admin -c "ALTER SYSTEM SET work_mem = '4MB';"

# Create indexes
docker exec shahin-postgres psql -U shahin_admin -d GrcMvcDb -c "CREATE INDEX CONCURRENTLY idx_audit_events_timestamp ON \"AuditEvents\" (\"Timestamp\");"
```

### Caching Optimization

```bash
# Redis configuration
docker exec shahin-redis redis-cli CONFIG SET maxmemory 256mb
docker exec shahin-redis redis-cli CONFIG SET maxmemory-policy allkeys-lru
```

---

## 🔄 Updates & Rollbacks

### Update Deployment

```bash
# Pull latest changes
git pull origin main

# Rebuild and deploy
./deployment/deploy-full-stack.sh deploy

# Check status
./deployment/deploy-full-stack.sh status
```

### Rollback Procedure

```bash
# Stop current deployment
./deployment/deploy-full-stack.sh stop

# Restore from backup
# [Restore database and files from backup]

# Start previous version
docker run -d --name shahin-portal-backup [previous-image]

# Verify functionality
curl https://portal.shahin-ai.com/health
```

---

## 📞 Support & Documentation

### Documentation Links
- [API Documentation](https://portal.shahin-ai.com/api-docs)
- [User Guide](https://portal.shahin-ai.com/help)
- [Developer Guide](https://github.com/your-org/shahin-grc/wiki)

### Support Contacts
- **Technical Support:** support@shahin-ai.com
- **Emergency:** +966-XX-XXX-XXXX
- **GitHub Issues:** https://github.com/your-org/shahin-grc/issues

### Monitoring Dashboard
- **Application Metrics:** https://portal.shahin-ai.com/admin/metrics
- **System Health:** https://portal.shahin-ai.com/health
- **Database Stats:** https://portal.shahin-ai.com/admin/db-stats

---

## ✅ Deployment Checklist

### Pre-Deployment
- [ ] Server provisioned with required specs
- [ ] Domain DNS configured
- [ ] SSL certificates obtained
- [ ] Firewall configured
- [ ] Backup strategy in place

### Deployment Steps
- [ ] Repository cloned
- [ ] Environment configured
- [ ] Secrets generated
- [ ] Docker services deployed
- [ ] Nginx configured
- [ ] Cloudflare tunnel setup
- [ ] SSL configured

### Post-Deployment
- [ ] Health checks passing
- [ ] Login/registration working
- [ ] Email verification functional
- [ ] Monitoring configured
- [ ] Backup tested
- [ ] Documentation updated

### Security Verification
- [ ] SSL/TLS enabled
- [ ] Firewall rules applied
- [ ] Database access restricted
- [ ] Secrets properly configured
- [ ] Audit logging enabled

---

**Deployment Complete! 🎉**

Your Shahin AI GRC platform is now running in production with:
- **High availability** through Docker containers
- **Secure access** via Cloudflare Tunnel
- **Automatic SSL** certificates
- **Comprehensive monitoring** and logging
- **Scalable architecture** for future growth

**Access your application:**
- **Landing Page:** https://www.shahin-ai.com
- **GRC Portal:** https://portal.shahin-ai.com
- **API Documentation:** https://portal.shahin-ai.com/api-docs
