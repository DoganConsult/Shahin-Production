# Shahin AI - Complete Deployment Guide

## Domain Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                        SHAHIN AI DOMAINS                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│   www.shahin-ai.com     →  Landing Page (React)     → Port 3000    │
│   shahin-ai.com         →  Landing Page (React)     → Port 3000    │
│   portal.shahin-ai.com  →  GRC Portal (ASP.NET)     → Port 5000    │
│   staging.shahin-ai.com →  Staging Environment      → Port 5001    │
│   dev.shahin-ai.com     →  Development              → Port 5002    │
│   demo.shahin-ai.com    →  Demo Environment         → Port 5002    │
│   test.shahin-ai.com    →  Test Environment         → Port 5002    │
│                                                                     │
│   *.shahin-ai.com       →  Tenant Subdomains        → Port 5000    │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

## Quick Start

### 1. Local Development

```bash
# Landing Page (React)
cd shahin-landing-temp/landing-page
npm install
npm run dev
# Opens at http://localhost:4000

# GRC Portal (ASP.NET)
cd Shahin-Jan-2026/src/GrcMvc
dotnet run
# Opens at https://localhost:5001
```

### 2. Build for Production

```bash
# Landing Page
cd shahin-landing-temp/landing-page
npm run build
# Output: dist/

# GRC Portal
cd Shahin-Jan-2026/src/GrcMvc
dotnet publish -c Release -o ./publish
# Output: publish/
```

### 3. Deploy with Docker

```bash
# Deploy Landing Page
cd shahin-landing-temp/landing-page
docker build -t shahin-landing -f docker/Dockerfile .
docker run -d --name shahin-landing -p 3000:80 shahin-landing

# Deploy GRC Portal
cd Shahin-Jan-2026/src/GrcMvc
docker build -t shahin-grc -f Dockerfile.production .
docker run -d --name grc-production -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="..." \
  shahin-grc
```

## Server Deployment Steps

### Step 1: Prepare Server

```bash
# SSH to server
ssh user@your-server

# Install Docker
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# Install nginx (optional, for reverse proxy)
sudo apt update && sudo apt install -y nginx

# Create directories
sudo mkdir -p /var/www/landing
sudo mkdir -p /var/www/portal
sudo mkdir -p /var/www/backups
```

### Step 2: Deploy Landing Page

```bash
# Clone and build
git clone https://github.com/doganlap/shahin-landing.git /opt/shahin-landing
cd /opt/shahin-landing/landing-page

# Create production env
cp .env.production .env

# Build and run Docker
docker build -t shahin-landing -f docker/Dockerfile .
docker run -d \
  --name shahin-landing \
  --restart unless-stopped \
  -p 3000:80 \
  --network shahin-network \
  shahin-landing
```

### Step 3: Deploy GRC Portal

```bash
# Pull from GitHub Container Registry
docker pull ghcr.io/doganlap/shahin-grc:latest

# Run production container
docker run -d \
  --name grc-production \
  --restart unless-stopped \
  -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e "ConnectionStrings__DefaultConnection=Host=db;Database=shahin_grc;..." \
  -e "JwtSettings__Secret=your-secret-key" \
  --network shahin-network \
  ghcr.io/doganlap/shahin-grc:latest
```

### Step 4: Configure Cloudflare Tunnel

```bash
# Install cloudflared
curl -L https://github.com/cloudflare/cloudflared/releases/latest/download/cloudflared-linux-amd64.deb -o cloudflared.deb
sudo dpkg -i cloudflared.deb

# Login to Cloudflare
cloudflared tunnel login

# Create tunnel
cloudflared tunnel create shahin-grc

# Copy config
sudo cp cloudflare-tunnel-config.yml ~/.cloudflared/config.yml

# Run as service
sudo cloudflared service install
sudo systemctl start cloudflared
sudo systemctl enable cloudflared
```

### Step 5: Configure nginx (Optional)

```bash
# Copy nginx config
sudo cp deployment/nginx/shahin-all-domains.conf /etc/nginx/sites-available/shahin
sudo ln -s /etc/nginx/sites-available/shahin /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

## Feature Test Checklist

### Landing Page Features

| Feature | URL | Test Steps | Expected |
|---------|-----|------------|----------|
| **Hero Section** | www.shahin-ai.com | Load page | Arabic-first hero with animations |
| **FloatingAIAgent** | www.shahin-ai.com | Click chat bubble | AI agent opens with Arabic greeting |
| **Voice Recording** | AI Agent → Mic | Click mic, speak | Voice recorded and processed |
| **Camera Capture** | AI Agent → Camera | Allow camera, capture | Image analyzed by AI |
| **File Upload** | AI Agent → Upload | Upload PDF/image | Document analyzed |
| **Demo Booking** | /demo-booking | Fill form, submit | Multi-step form works |
| **Time Slots** | Demo form step 3 | Select date | Available slots shown |
| **Key Features** | /#features | Scroll down | 12 feature cards visible |
| **Target Sectors** | /#target-sectors | Scroll down | 8 Saudi sectors with framework tags |
| **FAQ** | /#faq | Click questions | Accordions expand |
| **Dark Mode** | Header toggle | Click moon icon | Theme switches |
| **Mobile View** | Resize to mobile | Check responsive | All elements adapt |

### GRC Portal Features

| Feature | URL | Test Steps | Expected |
|---------|-----|------------|----------|
| **Login** | portal.shahin-ai.com/Account/Login | Enter credentials | Dashboard loads |
| **Dashboard** | /Dashboard | After login | Widgets and charts display |
| **Frameworks** | /Frameworks | Click menu | Framework list loads |
| **Assessments** | /Assessments | Click menu | Assessment grid loads |
| **Controls** | /Controls | Click menu | Controls table loads |
| **Onboarding** | /OnboardingWizard | New user | Wizard steps work |
| **Health Check** | /health | Direct access | "Healthy" response |
| **API Docs** | /api-docs | Direct access | Swagger UI loads |

### Integration Tests

| Test | Steps | Expected |
|------|-------|----------|
| **Landing → Portal** | Click "Login" on landing | Redirects to portal login |
| **Demo Booking API** | Submit demo form | API creates booking |
| **AI Agent API** | Send chat message | AI responds from backend |

## Monitoring Commands

```bash
# Check container status
docker ps -a

# View landing logs
docker logs shahin-landing -f

# View portal logs
docker logs grc-production -f

# Check health endpoints
curl http://localhost:3000/health  # Landing
curl http://localhost:5000/health  # Portal

# Check Cloudflare tunnel
cloudflared tunnel info shahin-grc

# Monitor nginx
sudo tail -f /var/log/nginx/access.log
```

## Rollback Procedure

```bash
# If deployment fails, rollback:

# 1. Stop new containers
docker stop shahin-landing grc-production

# 2. Start previous version
docker start shahin-landing-backup grc-production-backup

# Or restore from backup
cd /var/www/backups/landing
tar -xzf landing_backup_TIMESTAMP.tar.gz -C /var/www/landing
```

## Environment Variables Reference

### Landing Page (.env.production)
```
VITE_API_URL=https://portal.shahin-ai.com/api
VITE_FRONTEND_URL=https://www.shahin-ai.com
VITE_AUTH_URL=https://portal.shahin-ai.com/Account/Login
VITE_ENABLE_AI_AGENT=true
```

### GRC Portal (appsettings.json)
```json
{
  "App": {
    "BaseUrl": "https://portal.shahin-ai.com",
    "LandingUrl": "https://www.shahin-ai.com"
  }
}
```

## Support

- Documentation: This file
- Issues: https://github.com/doganlap/shahin-grc/issues
- Email: support@shahin-ai.com
