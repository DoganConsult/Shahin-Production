# GRC Application - Hetzner Deployment Guide

## Quick Start (One Command)

SSH into your Hetzner server and run:

```bash
curl -sSL https://raw.githubusercontent.com/doganlap/Shahin-Jan-2026/main/deploy/hetzner-setup.sh | sudo bash
```

Or manually:

```bash
wget https://raw.githubusercontent.com/doganlap/Shahin-Jan-2026/main/deploy/hetzner-setup.sh
sudo bash hetzner-setup.sh
```

## Deployment Options

### Option 1: Direct Installation (Recommended)
Uses systemd service with Nginx reverse proxy.

```bash
sudo bash hetzner-setup.sh
```

### Option 2: Docker Compose
Uses containers for app, database, and nginx.

```bash
cd /var/www/grc-app/deploy
docker-compose up -d
```

## Server Requirements

| Resource | Minimum | Recommended |
|----------|---------|-------------|
| CPU | 2 vCPU | 4 vCPU |
| RAM | 4 GB | 8 GB |
| Storage | 40 GB SSD | 80 GB SSD |
| OS | Ubuntu 22.04 | Ubuntu 22.04 |

**Hetzner Recommendations:**
- Development: CX21 (€5.39/month)
- Production: CX31 (€10.59/month)

## What Gets Installed

- .NET 8 SDK & Runtime
- PostgreSQL 15
- Nginx (reverse proxy)
- Certbot (SSL certificates)
- UFW Firewall

## Configuration

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| APP_DOMAIN | your-domain.com | Your domain name |
| DB_PASSWORD | GrcSecure2026! | PostgreSQL password |

### Database Connection

```
Host=localhost;Database=grcdb;Username=grcuser;Password=GrcSecure2026!
```

## Post-Deployment

### Enable HTTPS (SSL)

```bash
sudo certbot --nginx -d your-domain.com
```

### View Logs

```bash
# Application logs
journalctl -u grc -f

# Nginx logs
tail -f /var/log/nginx/error.log
```

### Update Application

```bash
sudo bash /var/www/grc-app/deploy/hetzner-update.sh
```

### Restart Services

```bash
sudo systemctl restart grc
sudo systemctl restart nginx
```

## Troubleshooting

### App won't start
```bash
journalctl -u grc -n 100
```

### Database connection issues
```bash
sudo -u postgres psql -c "\l"
```

### Port conflicts
```bash
sudo netstat -tlnp | grep -E '80|443|5000'
```

## File Structure

```
/var/www/
├── grc-app/              # Source code (git repo)
│   └── src/GrcMvc/
└── grc-published/        # Published application
    ├── GrcMvc.dll
    ├── appsettings.Production.json
    └── wwwroot/
```

## Support

- GitHub: https://github.com/doganlap/Shahin-Jan-2026
- Issues: https://github.com/doganlap/Shahin-Jan-2026/issues
