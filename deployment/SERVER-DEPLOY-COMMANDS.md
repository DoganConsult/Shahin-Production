# Shahin AI - Server Deployment Commands

## Quick Deploy (Copy & Paste)

### Step 1: SSH to Server
```bash
ssh root@your-server-ip
# or
ssh dogan@65.108.37.204
```

### Step 2: Create Deployment Directory
```bash
mkdir -p /opt/shahin && cd /opt/shahin
```

### Step 3: Clone Repositories
```bash
# Clone main application
git clone https://github.com/doganlap/Shahin-Jan-2026.git app

# Clone landing page
git clone https://github.com/doganlap/shahin-landing.git landing
```

### Step 4: Configure Environment
```bash
cd /opt/shahin/app/deployment

# Copy and edit environment file
cp .env.production.template .env
nano .env
```

**Required Variables to Set:**
```env
DB_PASSWORD=YourSecurePassword123!
JWT_SECRET=AtLeast32CharactersLongSecretKey!
SMTP_PASSWORD=YourEmailPassword
CLAUDE_API_KEY=sk-ant-your-key
```

### Step 5: Create Docker Network
```bash
docker network create shahin-network
```

### Step 6: Deploy Full Stack
```bash
chmod +x deploy-full-stack.sh
./deploy-full-stack.sh deploy
```

---

## Individual Service Commands

### Landing Page Only
```bash
# Build and deploy landing
cd /opt/shahin/landing/landing-page
docker build -t shahin-landing -f docker/Dockerfile .
docker run -d --name shahin-landing --restart unless-stopped -p 3000:80 --network shahin-network shahin-landing
```

### Portal Only
```bash
# Build and deploy portal
cd /opt/shahin/app/src/GrcMvc
docker build -t shahin-grc -f Dockerfile.production .
docker run -d --name grc-production --restart unless-stopped \
  -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e "ConnectionStrings__DefaultConnection=Host=localhost;Database=shahin_grc;Username=shahin_admin;Password=YOUR_PASSWORD;Port=5432" \
  -e "JwtSettings__Secret=YOUR_JWT_SECRET" \
  --network shahin-network \
  shahin-grc
```

### PostgreSQL
```bash
docker run -d --name shahin-postgres --restart unless-stopped \
  -p 5432:5432 \
  -e POSTGRES_USER=shahin_admin \
  -e POSTGRES_PASSWORD=YOUR_PASSWORD \
  -e POSTGRES_DB=shahin_grc \
  -v shahin-postgres-data:/var/lib/postgresql/data \
  --network shahin-network \
  postgres:16-alpine
```

### Redis
```bash
docker run -d --name shahin-redis --restart unless-stopped \
  -p 6379:6379 \
  -v shahin-redis-data:/data \
  --network shahin-network \
  redis:7-alpine redis-server --appendonly yes
```

---

## Cloudflare Tunnel Setup

### Install cloudflared
```bash
curl -L https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb -o cloudflared.deb
sudo dpkg -i cloudflared.deb
```

### Login and Create Tunnel
```bash
cloudflared tunnel login
cloudflared tunnel create shahin-grc
```

### Copy Configuration
```bash
# Copy config file
cp /opt/shahin/app/cloudflare-tunnel-config.yml ~/.cloudflared/config.yml

# Edit if needed
nano ~/.cloudflared/config.yml
```

### Run as Service
```bash
sudo cloudflared service install
sudo systemctl start cloudflared
sudo systemctl enable cloudflared

# Check status
sudo systemctl status cloudflared
cloudflared tunnel info shahin-grc
```

---

## Health Checks

```bash
# Landing page
curl http://localhost:3000/health

# Portal
curl http://localhost:5000/health

# PostgreSQL
docker exec shahin-postgres pg_isready -U shahin_admin

# Redis
docker exec shahin-redis redis-cli ping
```

---

## View Logs

```bash
# All services
docker-compose -f /opt/shahin/app/deployment/docker-compose.production.yml logs -f

# Specific service
docker logs -f shahin-landing
docker logs -f grc-production
docker logs -f shahin-postgres
```

---

## Backup Database

```bash
# Create backup
docker exec shahin-postgres pg_dump -U shahin_admin shahin_grc | gzip > /var/backups/shahin_$(date +%Y%m%d).sql.gz

# Restore backup
gunzip -c /var/backups/shahin_20260113.sql.gz | docker exec -i shahin-postgres psql -U shahin_admin shahin_grc
```

---

## Update Deployment

```bash
cd /opt/shahin/app
git pull origin main

cd /opt/shahin/landing
git pull origin main

cd /opt/shahin/app/deployment
./deploy-full-stack.sh deploy
```

---

## Troubleshooting

### Container won't start
```bash
docker logs shahin-landing
docker logs grc-production
```

### Database connection issues
```bash
# Check if postgres is running
docker ps | grep postgres

# Test connection
docker exec -it shahin-postgres psql -U shahin_admin -d shahin_grc -c "SELECT 1"
```

### Port conflicts
```bash
# Check what's using a port
sudo lsof -i :3000
sudo lsof -i :5000

# Kill process
sudo kill -9 <PID>
```

### Reset everything
```bash
cd /opt/shahin/app/deployment
./deploy-full-stack.sh stop
docker system prune -a
./deploy-full-stack.sh deploy
```

---

## Expected URLs After Deployment

| URL | Service |
|-----|---------|
| https://www.shahin-ai.com | Landing Page |
| https://shahin-ai.com | Landing Page (redirect) |
| https://portal.shahin-ai.com | GRC Portal |
| https://portal.shahin-ai.com/health | Health Check |
| https://portal.shahin-ai.com/api-docs | API Documentation |
| https://staging.shahin-ai.com | Staging Environment |
