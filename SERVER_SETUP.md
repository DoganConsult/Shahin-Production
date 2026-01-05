# GRC System - Production Server Setup Guide

## Server Information
- **IP Address**: 157.180.105.48
- **Domain**: portal.shahin-ai.com
- **Application Port**: 8080 (Docker)
- **Public Port**: 443 (HTTPS via Nginx)

## Prerequisites on Server

### 1. Install Docker and Docker Compose
```bash
# Update system
apt update && apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# Install Docker Compose
apt install docker-compose-plugin -y

# Verify installation
docker --version
docker compose version
```

### 2. Install Nginx
```bash
apt install nginx -y
systemctl enable nginx
systemctl start nginx
```

### 3. Install Certbot (Let's Encrypt)
```bash
apt install certbot python3-certbot-nginx -y
```

## Deployment Steps

### Step 1: Deploy Application
```bash
# On your local machine, run:
chmod +x deploy-to-server.sh
./deploy-to-server.sh
```

This will:
- Package the application
- Upload to server
- Extract and deploy using Docker Compose
- Start all containers

### Step 2: Configure Nginx Reverse Proxy
```bash
# On the server (157.180.105.48):
# Copy the Nginx configuration
scp nginx-config.conf root@157.180.105.48:/etc/nginx/sites-available/portal.shahin-ai.com

# Create symlink
ssh root@157.180.105.48
ln -s /etc/nginx/sites-available/portal.shahin-ai.com /etc/nginx/sites-enabled/

# Test Nginx configuration
nginx -t

# Reload Nginx
systemctl reload nginx
```

### Step 3: Set Up SSL Certificate
```bash
# On the server:
# Obtain SSL certificate from Let's Encrypt
certbot --nginx -d portal.shahin-ai.com

# Follow the prompts:
# - Enter email address
# - Agree to terms
# - Choose to redirect HTTP to HTTPS

# Verify auto-renewal
certbot renew --dry-run
```

### Step 4: Configure Firewall
```bash
# Allow HTTP and HTTPS
ufw allow 80/tcp
ufw allow 443/tcp
ufw allow 22/tcp  # SSH
ufw enable

# Verify
ufw status
```

## Verification

### 1. Check Docker Containers
```bash
cd /opt/grc-system
docker compose ps

# Should show:
# - grc-system-grcmvc-1 (Up)
# - grc-db (Up)
```

### 2. Check Application Logs
```bash
docker compose logs -f grc-system-grcmvc-1
```

### 3. Test Local Access
```bash
curl http://localhost:8080
# Should return HTML
```

### 4. Test Public Access
```bash
# From any machine:
curl https://portal.shahin-ai.com
# Should return HTML without redirect loop
```

### 5. Browser Test
Open: https://portal.shahin-ai.com

**Login Credentials:**
- User: `Info@doganconsult.com`
- Password: `AhmEma$123456`

## Troubleshooting

### Application won't start
```bash
# Check logs
docker compose logs

# Check if port 8080 is available
ss -tuln | grep 8080

# Restart containers
docker compose restart
```

### Nginx errors
```bash
# Check Nginx logs
tail -f /var/log/nginx/error.log
tail -f /var/log/nginx/portal.shahin-ai.com.error.log

# Test configuration
nginx -t

# Restart Nginx
systemctl restart nginx
```

### SSL certificate issues
```bash
# Check certificate status
certbot certificates

# Renew manually
certbot renew --force-renewal
```

### Database connection issues
```bash
# Check database container
docker compose logs grc-db

# Verify .env file exists with correct connection string
cat /opt/grc-system/.env
```

## Maintenance

### Update Application
```bash
# Run deployment script again
./deploy-to-server.sh
```

### Backup Database
```bash
# On server:
docker exec grc-db pg_dump -U postgres GrcMvcDb > backup_$(date +%Y%m%d).sql
```

### View Logs
```bash
# Application logs
docker compose logs -f

# Nginx logs
tail -f /var/log/nginx/portal.shahin-ai.com.access.log
```

### Restart Services
```bash
# Restart application
docker compose restart

# Restart Nginx
systemctl restart nginx
```

## Security Checklist
- [x] SSL certificate installed (HTTPS)
- [x] Firewall configured (UFW)
- [x] Strong passwords set
- [x] HTTP redirects to HTTPS
- [x] Security headers configured
- [ ] Regular backups scheduled
- [ ] Monitoring configured
- [ ] Log rotation configured

## Support
For issues, check:
1. Application logs: `docker compose logs`
2. Nginx logs: `/var/log/nginx/`
3. System logs: `journalctl -xe`
