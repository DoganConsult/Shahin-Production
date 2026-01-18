# 🚀 Shahin AI - Production Deployment Action Plan

**Date:** January 16, 2026  
**Status:** Ready for Deployment  
**Environment:** Windows 11 → Linux Production Server

---

## 📋 Current Status

✅ **Deployment Scripts Ready**
- `deploy-production.sh` - Full automation script (300+ lines)
- `docker-compose.production.yml` - Production configuration
- `.env.production` - Environment file exists

✅ **Documentation Complete**
- `PRODUCTION_DEPLOYMENT_COMPLETE.md` - Complete guide
- `DEPLOYMENT_SUMMARY.md` - Quick reference
- `CLOUDFLARE_TUNNEL_SETUP.md` - Cloudflare configuration

✅ **Infrastructure Files**
- Nginx configuration ready
- Database initialization scripts
- Health check endpoints

---

## 🎯 Deployment Steps Overview

### Phase 1: Pre-Deployment Preparation (Windows - Current Machine)
### Phase 2: Server Setup (Linux Production Server)
### Phase 3: Application Deployment
### Phase 4: Cloudflare Tunnel Configuration
### Phase 5: DNS Configuration
### Phase 6: Testing & Verification
### Phase 7: Monitoring & Maintenance

---

## 📝 PHASE 1: Pre-Deployment Preparation (Windows)

### Step 1.1: Verify Environment Configuration

**Action Required:** Check your `.env.production` file has these critical values set:

```bash
# Navigate to project directory
cd C:\Shahin-ai\Shahin-Jan-2026

# Check if .env.production exists
dir .env.production
```

**Required Environment Variables (DO NOT share these values):**
- ✅ `DB_PASSWORD` - Strong database password (16+ characters)
- ✅ `JWT_SECRET` - 256-bit random string (64 hex characters)
- ✅ `SMTP_PASSWORD` - Email service password
- ✅ `CLOUDFLARE_TUNNEL_TOKEN` - Cloudflare tunnel token (get from dashboard)

**Optional but Recommended:**
- `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET` - For Azure AD SSO
- `CLAUDE_API_KEY` - For AI features

### Step 1.2: Generate Secure Secrets (If Not Done)

**On Windows PowerShell:**

```powershell
# Generate JWT Secret (256-bit)
$bytes = New-Object byte[] 32
[Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($bytes)
$jwt_secret = [System.BitConverter]::ToString($bytes).Replace('-','').ToLower()
Write-Host "JWT_SECRET=$jwt_secret"

# Generate Database Password
$db_password = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 24 | ForEach-Object {[char]$_})
Write-Host "DB_PASSWORD=$db_password"
```

**Save these values securely!**

### Step 1.3: Package Project for Transfer

```powershell
# Create deployment package (excluding node_modules, bin, obj)
cd C:\Shahin-ai
Compress-Archive -Path "Shahin-Jan-2026\*" -DestinationPath "shahin-deployment-$(Get-Date -Format 'yyyyMMdd').zip" -Force

# Verify package created
dir shahin-deployment-*.zip
```

---

## 🖥️ PHASE 2: Server Setup (Linux Production Server)

### Step 2.1: Access Your Production Server

**You'll need:**
- Server IP address
- SSH credentials (username/password or SSH key)
- Root or sudo access

**Connect via SSH:**

```bash
# From Windows (using PowerShell or Windows Terminal)
ssh username@your-server-ip

# Or use PuTTY if you prefer GUI
```

### Step 2.2: Install Required Dependencies

**On the Linux server, run:**

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com | sh
sudo systemctl enable docker
sudo systemctl start docker
sudo usermod -aG docker $USER

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Install other tools
sudo apt install -y nginx git curl wget unzip

# Verify installations
docker --version
docker-compose --version
nginx -v
```

### Step 2.3: Create Deployment Directory

```bash
# Create directory structure
sudo mkdir -p /opt/shahin-ai
sudo chown $USER:$USER /opt/shahin-ai
cd /opt/shahin-ai

# Create backup and log directories
sudo mkdir -p /var/backups/shahin
sudo mkdir -p /var/log/shahin-ai
sudo chown $USER:$USER /var/backups/shahin
sudo chown $USER:$USER /var/log/shahin-ai
```

### Step 2.4: Transfer Project Files

**Option A: Using SCP (from Windows):**

```powershell
# From Windows PowerShell
scp C:\Shahin-ai\shahin-deployment-*.zip username@your-server-ip:/opt/shahin-ai/
```

**Option B: Using Git (if repository is accessible):**

```bash
# On Linux server
cd /opt/shahin-ai
git clone https://github.com/your-org/shahin-ai.git .
cd Shahin-Jan-2026
```

**Option C: Direct Upload via SFTP/FTP client (FileZilla, WinSCP)**

### Step 2.5: Extract and Prepare Files

```bash
# If using zip file
cd /opt/shahin-ai
unzip shahin-deployment-*.zip
cd Shahin-Jan-2026

# Make scripts executable
chmod +x deploy-production.sh
chmod +x scripts/*.sh
```

---

## 🔧 PHASE 3: Application Deployment

### Step 3.1: Configure Environment File

```bash
# On Linux server
cd /opt/shahin-ai/Shahin-Jan-2026

# Edit .env.production with your values
nano .env.production
```

**Critical values to set:**

```bash
# Database
DB_USER=grc_admin
DB_PASSWORD=your_secure_password_here
DB_NAME=GrcMvcDb

# JWT Authentication
JWT_SECRET=your_256_bit_secret_here
JWT_ISSUER=shahin-grc
JWT_AUDIENCE=shahin-grc-api

# SMTP (for email verification)
SMTP_HOST=smtp.office365.com
SMTP_PORT=587
SMTP_USERNAME=info@shahin-ai.com
SMTP_PASSWORD=your_smtp_password_here
SMTP_FROM=info@shahin-ai.com

# Cloudflare (will configure later)
CLOUDFLARE_TUNNEL_TOKEN=your_tunnel_token_here

# Optional - Azure AD
AZURE_TENANT_ID=
AZURE_CLIENT_ID=
AZURE_CLIENT_SECRET=

# Optional - Claude AI
CLAUDE_ENABLED=false
CLAUDE_API_KEY=

# Allowed Hosts
ALLOWED_HOSTS=shahin-ai.com,www.shahin-ai.com,portal.shahin-ai.com
```

**Save and exit:** `Ctrl+X`, then `Y`, then `Enter`

### Step 3.2: Deploy Database First

```bash
# Start database only
docker-compose -f docker-compose.production.yml up -d db-prod redis-prod

# Wait for database to be ready (30 seconds)
sleep 30

# Check database health
docker exec shahin-grc-db-prod pg_isready -U grc_admin -d GrcMvcDb

# Expected output: "accepting connections"
```

### Step 3.3: Deploy Application Services

```bash
# Build and start all services
docker-compose -f docker-compose.production.yml up -d --build

# This will:
# - Build the GRC portal Docker image
# - Build the marketing site Docker image
# - Start all containers
# - Run database migrations automatically

# Monitor the deployment
docker-compose -f docker-compose.production.yml logs -f
```

**Wait for these messages:**
- ✅ "Application started. Press Ctrl+C to shut down."
- ✅ "Database migration completed successfully"
- ✅ "Now listening on: http://[::]:80"

**Press `Ctrl+C` to stop following logs (containers keep running)**

### Step 3.4: Verify Services Are Running

```bash
# Check all containers
docker ps

# Expected output: 4 containers running
# - shahin-grc-production (port 5000, 8080)
# - shahin-grc-db-prod (port 5433)
# - shahin-grc-redis-prod (port 6380)
# - shahin-marketing-production (port 3000)

# Test health endpoints
curl http://localhost:5000/health
curl http://localhost:3000/

# Expected: HTTP 200 OK responses
```

---

## ☁️ PHASE 4: Cloudflare Tunnel Configuration

### Step 4.1: Install Cloudflared

```bash
# On Linux server
curl -L --output cloudflared.deb https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb
sudo dpkg -i cloudflared.deb

# Verify installation
cloudflared --version
```

### Step 4.2: Login to Cloudflare

```bash
# This will open a browser for authentication
cloudflared tunnel login

# Follow the browser prompts to authorize
# A certificate will be saved to ~/.cloudflared/cert.pem
```

### Step 4.3: Create Tunnel

```bash
# Create a tunnel named "shahin-production"
cloudflared tunnel create shahin-production

# Note the Tunnel ID from output (format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
# Save this ID - you'll need it for DNS configuration
```

### Step 4.4: Configure Tunnel Routes

```bash
# Create tunnel configuration
sudo mkdir -p /etc/cloudflared
sudo nano /etc/cloudflared/config.yml
```

**Add this configuration:**

```yaml
tunnel: shahin-production
credentials-file: /root/.cloudflared/shahin-production.json

ingress:
  # Marketing website
  - hostname: www.shahin-ai.com
    service: http://localhost:3000
  - hostname: shahin-ai.com
    service: http://localhost:3000
  
  # GRC Portal
  - hostname: portal.shahin-ai.com
    service: http://localhost:5000
  
  # Catch-all rule (required)
  - service: http_status:404
```

**Save and exit**

### Step 4.5: Start Tunnel as Service

```bash
# Install as system service
sudo cloudflared service install

# Start the service
sudo systemctl start cloudflared
sudo systemctl enable cloudflared

# Check status
sudo systemctl status cloudflared

# Expected: "active (running)"
```

### Step 4.6: Get Tunnel Token for DNS

```bash
# Get tunnel info
cloudflared tunnel info shahin-production

# Note the Tunnel ID - you'll use this for DNS configuration
```

---

## 🌐 PHASE 5: DNS Configuration

### Step 5.1: Configure Cloudflare DNS

**Go to Cloudflare Dashboard:**
1. Navigate to https://dash.cloudflare.com
2. Select your domain: `shahin-ai.com`
3. Go to **DNS** → **Records**

### Step 5.2: Create DNS Records

**Add these CNAME records:**

| Type | Name | Target | Proxy Status |
|------|------|--------|--------------|
| CNAME | www | `<tunnel-id>.cfargotunnel.com` | Proxied (Orange) |
| CNAME | @ | `<tunnel-id>.cfargotunnel.com` | Proxied (Orange) |
| CNAME | portal | `<tunnel-id>.cfargotunnel.com` | Proxied (Orange) |

**Replace `<tunnel-id>` with your actual tunnel ID from Step 4.3**

### Step 5.3: Configure Tunnel Routes in Cloudflare

1. Go to **Zero Trust** → **Access** → **Tunnels**
2. Find your tunnel: `shahin-production`
3. Click **Configure**
4. Add **Public Hostnames**:

**Route 1: Marketing Site**
- Public hostname: `www.shahin-ai.com`
- Service: `http://localhost:3000`

**Route 2: Marketing Site (root)**
- Public hostname: `shahin-ai.com`
- Service: `http://localhost:3000`

**Route 3: GRC Portal**
- Public hostname: `portal.shahin-ai.com`
- Service: `http://localhost:5000`

### Step 5.4: Enable SSL/TLS

1. Go to **SSL/TLS** → **Overview**
2. Set encryption mode to: **Full (strict)**
3. Go to **Edge Certificates**
4. Enable:
   - ✅ Always Use HTTPS
   - ✅ Automatic HTTPS Rewrites
   - ✅ Minimum TLS Version: 1.2

### Step 5.5: Verify DNS Propagation

```bash
# On Linux server or Windows
nslookup www.shahin-ai.com
nslookup portal.shahin-ai.com

# Should resolve to Cloudflare IPs
```

**DNS propagation can take 5-60 minutes**

---

## ✅ PHASE 6: Testing & Verification

### Step 6.1: Test External Access

**From your browser (after DNS propagates):**

```
https://www.shahin-ai.com
https://shahin-ai.com
https://portal.shahin-ai.com
```

**Expected Results:**
- ✅ Green padlock (SSL certificate valid)
- ✅ Marketing site loads
- ✅ Portal login page loads
- ✅ No certificate warnings

### Step 6.2: Test Application Functionality

**Test Registration:**
1. Go to `https://portal.shahin-ai.com/Account/Register`
2. Create a test account
3. Check email for verification link
4. Verify account activation works

**Test Login:**
1. Go to `https://portal.shahin-ai.com/Account/Login`
2. Login with test account
3. Verify dashboard loads
4. Check all menu items accessible

**Test API Endpoints:**

```bash
# Health check
curl https://portal.shahin-ai.com/health

# API documentation
curl https://portal.shahin-ai.com/api-docs
```

### Step 6.3: Monitor Logs

```bash
# On Linux server
cd /opt/shahin-ai/Shahin-Jan-2026

# View all logs
docker-compose -f docker-compose.production.yml logs -f

# View specific service logs
docker logs shahin-grc-production -f
docker logs shahin-marketing-production -f
docker logs shahin-grc-db-prod -f
```

**Look for:**
- ✅ No error messages
- ✅ Successful database connections
- ✅ Successful HTTP requests
- ✅ No authentication failures

### Step 6.4: Performance Testing

```bash
# Test response times
curl -w "@-" -o /dev/null -s https://portal.shahin-ai.com/health << 'EOF'
    time_namelookup:  %{time_namelookup}\n
       time_connect:  %{time_connect}\n
    time_appconnect:  %{time_appconnect}\n
      time_redirect:  %{time_redirect}\n
   time_starttransfer:  %{time_starttransfer}\n
                     ----------\n
         time_total:  %{time_total}\n
EOF
```

**Expected:** Total time < 2 seconds

---

## 📊 PHASE 7: Monitoring & Maintenance

### Step 7.1: Setup Automated Backups

```bash
# Create backup script
sudo nano /usr/local/bin/backup-shahin.sh
```

**Add this content:**

```bash
#!/bin/bash
BACKUP_DIR="/var/backups/shahin"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/shahin_backup_$TIMESTAMP.sql.gz"

# Create backup
docker exec shahin-grc-db-prod pg_dump -U grc_admin GrcMvcDb | gzip > "$BACKUP_FILE"

# Keep only last 30 backups
ls -t "$BACKUP_DIR"/*.sql.gz | tail -n +31 | xargs -r rm

echo "Backup completed: $BACKUP_FILE"
```

**Make executable and schedule:**

```bash
sudo chmod +x /usr/local/bin/backup-shahin.sh

# Add to crontab (daily at 2 AM)
sudo crontab -e

# Add this line:
0 2 * * * /usr/local/bin/backup-shahin.sh >> /var/log/shahin-ai/backup.log 2>&1
```

### Step 7.2: Setup Log Rotation

```bash
sudo nano /etc/logrotate.d/shahin-ai
```

**Add:**

```
/var/log/shahin-ai/*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 644 root root
}
```

### Step 7.3: Configure Firewall

```bash
# Enable UFW firewall
sudo ufw --force enable

# Allow required ports
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# Reload firewall
sudo ufw reload

# Check status
sudo ufw status
```

### Step 7.4: Setup Monitoring Dashboard

**Create monitoring script:**

```bash
nano ~/monitor-shahin.sh
```

**Add:**

```bash
#!/bin/bash
echo "=== Shahin AI Production Status ==="
echo ""
echo "Docker Containers:"
docker ps --filter "name=shahin" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo ""
echo "Service Health:"
curl -s http://localhost:5000/health && echo " - Portal: OK" || echo " - Portal: FAILED"
curl -s http://localhost:3000/ && echo " - Marketing: OK" || echo " - Marketing: FAILED"
docker exec shahin-grc-db-prod pg_isready -U grc_admin -d GrcMvcDb && echo " - Database: OK" || echo " - Database: FAILED"
echo ""
echo "System Resources:"
echo "CPU: $(uptime | awk '{print $NF}')"
echo "Memory: $(free -h | awk 'NR==2{printf "%.1f%% used", $3*100/$2}')"
echo "Disk: $(df -h / | awk 'NR==2{print $5 " used"}')"
```

```bash
chmod +x ~/monitor-shahin.sh

# Run anytime to check status
~/monitor-shahin.sh
```

---

## 🚨 Troubleshooting Guide

### Issue: Services Won't Start

```bash
# Check logs
docker-compose -f docker-compose.production.yml logs

# Check Docker status
sudo systemctl status docker

# Restart Docker
sudo systemctl restart docker

# Rebuild and restart
docker-compose -f docker-compose.production.yml down
docker-compose -f docker-compose.production.yml up -d --build
```

### Issue: Database Connection Failed

```bash
# Check database is running
docker ps | grep db-prod

# Test connection
docker exec shahin-grc-db-prod psql -U grc_admin -d GrcMvcDb -c "SELECT 1;"

# Check database logs
docker logs shahin-grc-db-prod

# Restart database
docker restart shahin-grc-db-prod
```

### Issue: Cloudflare Tunnel Not Working

```bash
# Check tunnel status
sudo systemctl status cloudflared

# View tunnel logs
sudo journalctl -u cloudflared -f

# Restart tunnel
sudo systemctl restart cloudflared

# Test tunnel connectivity
cloudflared tunnel info shahin-production
```

### Issue: SSL Certificate Errors

**Check Cloudflare SSL settings:**
1. Go to Cloudflare Dashboard
2. SSL/TLS → Overview
3. Ensure mode is "Full (strict)"
4. Check Edge Certificates are active

### Issue: Application Errors

```bash
# View application logs
docker logs shahin-grc-production -f

# Check environment variables
docker exec shahin-grc-production env | grep -E "(DB|JWT|SMTP)"

# Restart application
docker restart shahin-grc-production
```

---

## 📞 Support & Resources

### Quick Commands Reference

```bash
# Start all services
docker-compose -f docker-compose.production.yml up -d

# Stop all services
docker-compose -f docker-compose.production.yml down

# View logs
docker-compose -f docker-compose.production.yml logs -f

# Restart specific service
docker restart shahin-grc-production

# Create backup
docker exec shahin-grc-db-prod pg_dump -U grc_admin GrcMvcDb | gzip > backup_$(date +%Y%m%d).sql.gz

# Check status
docker ps
~/monitor-shahin.sh
```

### Important URLs

- **Marketing Site:** https://www.shahin-ai.com
- **GRC Portal:** https://portal.shahin-ai.com
- **API Docs:** https://portal.shahin-ai.com/api-docs
- **Health Check:** https://portal.shahin-ai.com/health
- **Cloudflare Dashboard:** https://dash.cloudflare.com

### Documentation Files

- `PRODUCTION_DEPLOYMENT_COMPLETE.md` - Complete deployment guide
- `DEPLOYMENT_SUMMARY.md` - Quick reference
- `CLOUDFLARE_TUNNEL_SETUP.md` - Cloudflare configuration
- `DATABASE_HEALTH_CHECK_REPORT.md` - Database diagnostics

---

## ✅ Deployment Checklist

### Pre-Deployment
- [ ] `.env.production` configured with all required values
- [ ] Secure passwords generated (DB, JWT, SMTP)
- [ ] Project files packaged or accessible via Git
- [ ] Linux server provisioned (4GB RAM, 50GB storage)
- [ ] SSH access to server configured

### Server Setup
- [ ] Docker installed and running
- [ ] Docker Compose installed
- [ ] Nginx installed
- [ ] Project files transferred to `/opt/shahin-ai`
- [ ] Scripts made executable

### Application Deployment
- [ ] Database container started and healthy
- [ ] Redis container started
- [ ] Application containers built and running
- [ ] Database migrations completed
- [ ] Health endpoints responding

### Cloudflare Configuration
- [ ] Cloudflared installed
- [ ] Tunnel created and configured
- [ ] Tunnel service running
- [ ] DNS records created (www, @, portal)
- [ ] Public hostnames configured in Cloudflare
- [ ] SSL/TLS set to "Full (strict)"

### Testing
- [ ] Marketing site accessible via HTTPS
- [ ] Portal accessible via HTTPS
- [ ] SSL certificates valid (green padlock)
- [ ] User registration working
- [ ] Email verification working
- [ ] Login working
- [ ] Dashboard accessible
- [ ] API endpoints responding

### Monitoring & Maintenance
- [ ] Automated backups configured
- [ ] Log rotation configured
- [ ] Firewall configured
- [ ] Monitoring script created
- [ ] Documentation reviewed

---

## 🎉 Deployment Complete!

Your Shahin AI GRC platform is now running in production with:

✅ **Secure Infrastructure**
- SSL/TLS encryption via Cloudflare
- Firewall protection
- Isolated Docker containers

✅ **High Availability**
- Docker container orchestration
- Automatic restart on failure
- Health monitoring

✅ **Scalable Architecture**
- Separate database and application layers
- Redis caching
- Load balancing ready

✅ **Professional Setup**
- Automated backups
- Log management
- Monitoring tools

**Your application is live at:**
- 🌐 **Marketing:** https://www.shahin-ai.com
- 🔐 **Portal:** https://portal.shahin-ai.com
- 📚 **API Docs:** https://portal.shahin-ai.com/api-docs

---

**Need Help?**
- Review the troubleshooting section above
- Check Docker logs: `docker-compose logs -f`
- Monitor system: `~/monitor-shahin.sh`
- Contact support: support@shahin-ai.com

**Last Updated:** January 16, 2026  
**Version:** Production v1.0  
**Status:** ✅ Ready for Deployment
