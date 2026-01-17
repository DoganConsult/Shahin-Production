# WINDOWS TUNNEL SETUP GUIDE FOR PRODUCTION
## Complete Configuration with 4 Tunnels

Generated: 2026-01-16
Platform: Windows with Docker Desktop

---

## TUNNEL ARCHITECTURE OVERVIEW

You're providing **4 tunnels** from your Windows server to production cloud.

### TUNNEL ALLOCATION:
1. **Tunnel 1:** Port 80 (HTTP) → Nginx/Traefik
2. **Tunnel 2:** Port 443 (HTTPS) → Nginx/Traefik
3. **Tunnel 3:** Port 3000 → Landing Page
4. **Tunnel 4:** Port 5000 → GRC Portal

---

## OPTION 1: WINDOWS PORT FORWARDING (NATIVE)

### Using Windows `netsh` for tunneling:

```powershell
# Run as Administrator in PowerShell

# Tunnel 1: HTTP (80)
netsh interface portproxy add v4tov4 listenport=80 listenaddress=0.0.0.0 connectport=80 connectaddress=YOUR_CLOUD_IP

# Tunnel 2: HTTPS (443)
netsh interface portproxy add v4tov4 listenport=443 listenaddress=0.0.0.0 connectport=443 connectaddress=YOUR_CLOUD_IP

# Tunnel 3: Landing Page (3000)
netsh interface portproxy add v4tov4 listenport=3000 listenaddress=0.0.0.0 connectport=3000 connectaddress=YOUR_CLOUD_IP

# Tunnel 4: Portal (5000)
netsh interface portproxy add v4tov4 listenport=5000 listenaddress=0.0.0.0 connectport=5000 connectaddress=YOUR_CLOUD_IP

# Verify tunnels
netsh interface portproxy show all

# Windows Firewall Rules
netsh advfirewall firewall add rule name="Shahin HTTP" dir=in action=allow protocol=TCP localport=80
netsh advfirewall firewall add rule name="Shahin HTTPS" dir=in action=allow protocol=TCP localport=443
netsh advfirewall firewall add rule name="Shahin Landing" dir=in action=allow protocol=TCP localport=3000
netsh advfirewall firewall add rule name="Shahin Portal" dir=in action=allow protocol=TCP localport=5000
```

---

## OPTION 2: DOCKER DESKTOP TUNNELS

### Docker Compose with Port Mapping:

```yaml
# docker-compose.tunnels.yml
version: '3.8'

services:
  # Tunnel 1 & 2: Nginx Reverse Proxy
  nginx-tunnel:
    image: nginx:alpine
    container_name: shahin-nginx-tunnel
    ports:
      - "80:80"      # Tunnel 1
      - "443:443"    # Tunnel 2
    volumes:
      - ./nginx-tunnel.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
    restart: always

  # Tunnel 3: Landing Page
  landing-tunnel:
    image: alpine/socat
    container_name: shahin-landing-tunnel
    command: TCP-LISTEN:3000,fork TCP:YOUR_CLOUD_IP:3000
    ports:
      - "3000:3000"  # Tunnel 3
    restart: always

  # Tunnel 4: Portal
  portal-tunnel:
    image: alpine/socat
    container_name: shahin-portal-tunnel
    command: TCP-LISTEN:5000,fork TCP:YOUR_CLOUD_IP:5000
    ports:
      - "5000:5000"  # Tunnel 4
    restart: always
```

### Start Docker tunnels:
```powershell
cd C:\Shahin-ai
docker-compose -f docker-compose.tunnels.yml up -d
```

---

## OPTION 3: SSH TUNNELS (MOST SECURE)

### Create SSH tunnels from Windows to Cloud:

```powershell
# Install OpenSSH Client (Windows 10/11)
Add-WindowsCapability -Online -Name OpenSSH.Client

# Create all 4 tunnels via SSH
# Run each in separate PowerShell window or use screen/tmux

# Tunnel 1: HTTP
ssh -L 80:localhost:80 user@YOUR_CLOUD_IP -N

# Tunnel 2: HTTPS
ssh -L 443:localhost:443 user@YOUR_CLOUD_IP -N

# Tunnel 3: Landing
ssh -L 3000:localhost:3000 user@YOUR_CLOUD_IP -N

# Tunnel 4: Portal
ssh -L 5000:localhost:5000 user@YOUR_CLOUD_IP -N
```

### Automated SSH Tunnel Script:
```powershell
# save as: start-tunnels.ps1
$cloudIP = "YOUR_CLOUD_IP"
$sshUser = "YOUR_SSH_USER"
$sshKey = "C:\Users\YOUR_USER\.ssh\id_rsa"

# Start all tunnels
Start-Process ssh -ArgumentList "-i $sshKey -L 80:localhost:80 $sshUser@$cloudIP -N"
Start-Process ssh -ArgumentList "-i $sshKey -L 443:localhost:443 $sshUser@$cloudIP -N"
Start-Process ssh -ArgumentList "-i $sshKey -L 3000:localhost:3000 $sshUser@$cloudIP -N"
Start-Process ssh -ArgumentList "-i $sshKey -L 5000:localhost:5000 $sshUser@$cloudIP -N"

Write-Host "All 4 tunnels started!"
```

---

## ALL REQUIRED KEYS & CONFIGURATIONS

### 1. ENVIRONMENT VARIABLES (.env.production)
```env
# DATABASE
DB_HOST=localhost
DB_PORT=5432
DB_NAME=shahin_grc
DB_USER=shahin_admin
DB_PASSWORD=GENERATE_STRONG_PASSWORD_HERE

# JWT AUTHENTICATION
JWT_SECRET=GENERATE_256_BIT_KEY_HERE
JWT_ISSUER=GrcSystem
JWT_AUDIENCE=GrcSystemUsers
JWT_EXPIRY_DAYS=30

# CLAUDE AI (CRITICAL - GET FROM https://console.anthropic.com/)
CLAUDE_API_KEY=sk-ant-api03-YOUR_ACTUAL_KEY_HERE
CLAUDE_MODEL=claude-3-5-sonnet-20241022
CLAUDE_MAX_TOKENS=4096
CLAUDE_TEMPERATURE=0.7
CLAUDE_API_VERSION=2023-06-01

# SMTP EMAIL
SMTP_HOST=smtp.office365.com
SMTP_PORT=587
SMTP_USERNAME=your-email@company.com
SMTP_PASSWORD=YOUR_APP_PASSWORD_HERE
SMTP_FROM_EMAIL=noreply@shahin-ai.com
SMTP_FROM_NAME=Shahin AI Platform
SMTP_USE_SSL=true

# REDIS CACHE
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=GENERATE_REDIS_PASSWORD_HERE

# AZURE (IF USING SSO)
AZURE_TENANT_ID=YOUR_TENANT_ID
AZURE_CLIENT_ID=YOUR_CLIENT_ID
AZURE_CLIENT_SECRET=YOUR_CLIENT_SECRET
AZURE_REDIRECT_URI=https://portal.shahin-ai.com/signin-oidc
AZURE_SCOPE=openid profile email
AZURE_AUTHORITY=https://login.microsoftonline.com/

# APPLICATION
APP_BASE_URL=https://portal.shahin-ai.com
ALLOWED_HOSTS=portal.shahin-ai.com,www.shahin-ai.com
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000;https://+:5001

# FILE STORAGE
STORAGE_PATH=/app/storage
MAX_FILE_SIZE=104857600
ALLOWED_FILE_TYPES=.pdf,.docx,.xlsx,.txt,.jpg,.png

# RATE LIMITING
RATE_LIMIT_ENABLED=true
RATE_LIMIT_REQUESTS_PER_MINUTE=60
RATE_LIMIT_REQUESTS_PER_HOUR=1000
RATE_LIMIT_IP_WHITELIST=127.0.0.1,::1
RATE_LIMIT_API_KEYS=key1,key2

# LOGGING
LOG_LEVEL=Information
LOG_PATH=/app/logs
LOG_RETENTION_DAYS=30
SERILOG_MIN_LEVEL=Information

# BACKUP
BACKUP_ENABLED=true
BACKUP_SCHEDULE=0 2 * * *
BACKUP_RETENTION_DAYS=30
BACKUP_PATH=/backups
```

### 2. GENERATE SECURE KEYS SCRIPT
```powershell
# save as: generate-keys.ps1

# Generate JWT Secret (256-bit)
$jwtSecret = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | % {[char]$_})
Write-Host "JWT_SECRET=$jwtSecret"

# Generate DB Password
$dbPassword = -join ((33..126) | Get-Random -Count 32 | % {[char]$_})
Write-Host "DB_PASSWORD=$dbPassword"

# Generate Redis Password
$redisPassword = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | % {[char]$_})
Write-Host "REDIS_PASSWORD=$redisPassword"

# Generate Azure Client Secret (if needed)
$azureSecret = [System.Guid]::NewGuid().ToString("N") + [System.Guid]::NewGuid().ToString("N")
Write-Host "AZURE_CLIENT_SECRET=$azureSecret"
```

### 3. SSL CERTIFICATES
```powershell
# Option A: Self-signed for testing
New-SelfSignedCertificate -DnsName "portal.shahin-ai.com", "www.shahin-ai.com" `
  -CertStoreLocation "cert:\LocalMachine\My" `
  -NotAfter (Get-Date).AddYears(2)

# Option B: Let's Encrypt (production)
# Install Certbot
choco install certbot

# Generate certificates
certbot certonly --standalone -d portal.shahin-ai.com -d www.shahin-ai.com
```

### 4. NGINX CONFIGURATION (nginx-tunnel.conf)
```nginx
events {
    worker_connections 1024;
}

http {
    upstream landing {
        server YOUR_CLOUD_IP:3000;
    }

    upstream portal {
        server YOUR_CLOUD_IP:5000;
    }

    server {
        listen 80;
        server_name www.shahin-ai.com shahin-ai.com;

        location / {
            proxy_pass http://landing;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }

    server {
        listen 443 ssl http2;
        server_name www.shahin-ai.com shahin-ai.com;

        ssl_certificate /etc/nginx/ssl/cert.pem;
        ssl_certificate_key /etc/nginx/ssl/key.pem;

        location / {
            proxy_pass http://landing;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
        }
    }

    server {
        listen 80;
        server_name portal.shahin-ai.com;
        return 301 https://$server_name$request_uri;
    }

    server {
        listen 443 ssl http2;
        server_name portal.shahin-ai.com;

        ssl_certificate /etc/nginx/ssl/cert.pem;
        ssl_certificate_key /etc/nginx/ssl/key.pem;

        location / {
            proxy_pass http://portal;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto https;
        }
    }
}
```

---

## DOCKER COMPOSE FOR WINDOWS

### Complete docker-compose.windows.yml:
```yaml
version: '3.8'

services:
  # Portal Application
  portal:
    image: shahin-grc:latest
    container_name: shahin-portal-win
    ports:
      - "5000:5000"
    env_file:
      - .env.production
    volumes:
      - C:\Shahin-ai\data:/app/data
      - C:\Shahin-ai\logs:/app/logs
    networks:
      - shahin-net
    restart: always

  # Landing Page
  landing:
    image: shahin-landing:latest
    container_name: shahin-landing-win
    ports:
      - "3000:3000"
    environment:
      - NODE_ENV=production
    networks:
      - shahin-net
    restart: always

  # PostgreSQL
  postgres:
    image: postgres:15-alpine
    container_name: shahin-db-win
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_USER=${DB_USER}
      - POSTGRES_PASSWORD=${DB_PASSWORD}
      - POSTGRES_DB=${DB_NAME}
    volumes:
      - C:\Shahin-ai\postgres-data:/var/lib/postgresql/data
    networks:
      - shahin-net
    restart: always

  # Redis
  redis:
    image: redis:7-alpine
    container_name: shahin-redis-win
    ports:
      - "6379:6379"
    command: redis-server --requirepass ${REDIS_PASSWORD}
    volumes:
      - C:\Shahin-ai\redis-data:/data
    networks:
      - shahin-net
    restart: always

  # Nginx (handles tunnels 1 & 2)
  nginx:
    image: nginx:alpine
    container_name: shahin-nginx-win
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx-tunnel.conf:/etc/nginx/nginx.conf:ro
      - C:\Shahin-ai\ssl:/etc/nginx/ssl:ro
    networks:
      - shahin-net
    restart: always

networks:
  shahin-net:
    driver: bridge
```

---

## STARTUP SEQUENCE

### 1. Prepare Environment
```powershell
# Create directories
New-Item -ItemType Directory -Path C:\Shahin-ai\data
New-Item -ItemType Directory -Path C:\Shahin-ai\logs
New-Item -ItemType Directory -Path C:\Shahin-ai\postgres-data
New-Item -ItemType Directory -Path C:\Shahin-ai\redis-data
New-Item -ItemType Directory -Path C:\Shahin-ai\ssl

# Generate keys
.\generate-keys.ps1

# Copy template to production
Copy-Item .env.production.template .env.production

# Edit .env.production with generated keys
notepad .env.production
```

### 2. Start Services
```powershell
# Build images
docker-compose -f docker-compose.windows.yml build

# Start all services
docker-compose -f docker-compose.windows.yml up -d

# Check status
docker-compose -f docker-compose.windows.yml ps

# View logs
docker-compose -f docker-compose.windows.yml logs -f
```

### 3. Configure Tunnels
```powershell
# Option A: Windows native
.\start-tunnels.ps1

# Option B: Docker tunnels
docker-compose -f docker-compose.tunnels.yml up -d

# Option C: SSH tunnels
ssh -L 80:localhost:80 -L 443:localhost:443 -L 3000:localhost:3000 -L 5000:localhost:5000 user@CLOUD_IP
```

### 4. Verify
```powershell
# Test each tunnel
curl http://localhost:80          # Should reach Nginx
curl https://localhost:443        # Should reach Nginx (SSL)
curl http://localhost:3000        # Should reach Landing
curl http://localhost:5000        # Should reach Portal

# Check Docker logs
docker logs shahin-portal-win
docker logs shahin-landing-win
docker logs shahin-nginx-win
```

---

## MONITORING & TROUBLESHOOTING

### Check Tunnel Status:
```powershell
# Windows native tunnels
netsh interface portproxy show all

# Docker port mappings
docker port shahin-nginx-win
docker port shahin-portal-win
docker port shahin-landing-win

# Network connections
netstat -an | findstr "80 443 3000 5000"
```

### Common Issues:

1. **Port already in use:**
```powershell
# Find process using port
netstat -aon | findstr :80
# Kill process
taskkill /F /PID <PID>
```

2. **Docker networking issues:**
```powershell
# Reset Docker network
docker network prune
docker-compose down
docker-compose up -d
```

3. **Firewall blocking:**
```powershell
# Temporarily disable for testing
netsh advfirewall set allprofiles state off
# Re-enable after testing
netsh advfirewall set allprofiles state on
```

---

## SECURITY CHECKLIST

- [ ] Generated strong passwords (32+ characters)
- [ ] JWT secret is 256-bit minimum
- [ ] Claude API key obtained and tested
- [ ] SSL certificates installed
- [ ] Firewall rules configured
- [ ] Database not exposed externally
- [ ] Redis password set
- [ ] Backup configured
- [ ] Monitoring enabled
- [ ] Rate limiting active

---

## QUICK REFERENCE

### Essential Commands:
```powershell
# Start everything
docker-compose -f docker-compose.windows.yml up -d

# Stop everything
docker-compose -f docker-compose.windows.yml down

# View logs
docker-compose logs -f portal

# Restart service
docker-compose restart portal

# Database migrations
docker exec shahin-portal-win dotnet ef database update

# Backup database
docker exec shahin-db-win pg_dump -U shahin_admin shahin_grc > backup.sql
```

### URLs After Setup:
- Landing: http://localhost:3000
- Portal: http://localhost:5000
- Portal SSL: https://localhost:443
- Database: localhost:5432 (internal only)
- Redis: localhost:6379 (internal only)

---

## SUPPORT

For issues:
1. Check logs: `docker-compose logs`
2. Verify .env.production has all keys
3. Ensure Docker Desktop is running
4. Check Windows Firewall settings
5. Verify tunnel connections with netstat