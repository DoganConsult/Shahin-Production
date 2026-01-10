#!/bin/bash
# ============================================================================
# GRC Application - Hetzner Complete Deployment Script
# Run this on a fresh Ubuntu 22.04 Hetzner server
# Usage: sudo bash hetzner-setup.sh
# ============================================================================

set -e

echo "=========================================="
echo "GRC Application - Hetzner Deployment"
echo "=========================================="

# Variables - CHANGE THESE
APP_DOMAIN="${APP_DOMAIN:-your-domain.com}"
DB_PASSWORD="${DB_PASSWORD:-GrcSecure2026!}"
GITHUB_REPO="https://github.com/doganlap/Shahin-Jan-2026.git"
APP_USER="grcadmin"
APP_DIR="/home/${APP_USER}/grc-app"
PUBLISH_DIR="/home/${APP_USER}/grc-published"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# ============================================================================
# STEP 0: Server Hardening & Security Setup
# ============================================================================
log_info "Step 0: Server hardening and security setup..."

# Create non-root application user
if ! id "${APP_USER}" &>/dev/null; then
    useradd -m -s /bin/bash ${APP_USER}
    log_info "Created user: ${APP_USER}"
else
    log_info "User ${APP_USER} already exists"
fi

# Set timezone
timedatectl set-timezone UTC

# Disable root password login (SSH key only)
sed -i 's/^PermitRootLogin yes/PermitRootLogin prohibit-password/' /etc/ssh/sshd_config
sed -i 's/^#PasswordAuthentication yes/PasswordAuthentication no/' /etc/ssh/sshd_config
systemctl restart sshd || true

# Install fail2ban for brute force protection
apt install -y fail2ban
systemctl enable fail2ban
systemctl start fail2ban

log_info "Server hardening complete"

# ============================================================================
# STEP 1: System Update
# ============================================================================
log_info "Step 1: Updating system packages..."
apt update && apt upgrade -y
apt install -y curl wget git unzip software-properties-common

# ============================================================================
# STEP 2: Install .NET 8 SDK
# ============================================================================
log_info "Step 2: Installing .NET 8 SDK..."
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
apt update
apt install -y dotnet-sdk-8.0 aspnetcore-runtime-8.0

dotnet --version
log_info ".NET installed successfully"

# ============================================================================
# STEP 3: Install PostgreSQL
# ============================================================================
log_info "Step 3: Installing PostgreSQL..."
apt install -y postgresql postgresql-contrib
systemctl enable postgresql
systemctl start postgresql

# Create database and user
sudo -u postgres psql <<EOF
CREATE USER grcuser WITH PASSWORD '${DB_PASSWORD}';
CREATE DATABASE grcdb OWNER grcuser;
GRANT ALL PRIVILEGES ON DATABASE grcdb TO grcuser;
\c grcdb
GRANT ALL ON SCHEMA public TO grcuser;
EOF

log_info "PostgreSQL configured with database 'grcdb'"

# ============================================================================
# STEP 4: Install Nginx
# ============================================================================
log_info "Step 4: Installing Nginx..."
apt install -y nginx
systemctl enable nginx

# ============================================================================
# STEP 5: Install Certbot for SSL (optional)
# ============================================================================
log_info "Step 5: Installing Certbot for SSL..."
apt install -y certbot python3-certbot-nginx

# ============================================================================
# STEP 6: Clone and Build Application
# ============================================================================
log_info "Step 6: Cloning repository..."
mkdir -p ${APP_DIR}
chown -R ${APP_USER}:${APP_USER} /home/${APP_USER}

cd /home/${APP_USER}

if [ -d "grc-app" ]; then
    log_warn "Directory exists, pulling latest..."
    cd grc-app
    git pull origin main
else
    git clone ${GITHUB_REPO} grc-app
    cd grc-app
fi

log_info "Building application..."
cd src/GrcMvc
dotnet restore
dotnet publish -c Release -o ${PUBLISH_DIR}

chown -R ${APP_USER}:${APP_USER} /home/${APP_USER}
log_info "Application built successfully"

# ============================================================================
# STEP 7: Create appsettings.Production.json
# ============================================================================
log_info "Step 7: Creating production configuration..."
cat > ${PUBLISH_DIR}/appsettings.Production.json <<EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=grcdb;Username=grcuser;Password=${DB_PASSWORD}"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  },
  "JwtSettings": {
    "SecretKey": "$(openssl rand -base64 64 | tr -d '\n')",
    "Issuer": "GrcMvc",
    "Audience": "GrcMvc",
    "ExpirationInMinutes": 60
  }
}
EOF

# ============================================================================
# STEP 8: Create Systemd Service
# ============================================================================
log_info "Step 8: Creating systemd service..."
cat > /etc/systemd/system/grc.service <<EOF
[Unit]
Description=GRC MVC Application
After=network.target postgresql.service

[Service]
WorkingDirectory=${PUBLISH_DIR}
ExecStart=/usr/bin/dotnet ${PUBLISH_DIR}/GrcMvc.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=grc-app
User=${APP_USER}
Group=${APP_USER}
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
EOF

# Set permissions
chown -R ${APP_USER}:${APP_USER} ${PUBLISH_DIR}
chmod -R 755 ${PUBLISH_DIR}

systemctl daemon-reload
systemctl enable grc

# ============================================================================
# STEP 9: Configure Nginx
# ============================================================================
log_info "Step 9: Configuring Nginx..."
cat > /etc/nginx/sites-available/grc <<EOF
server {
    listen 80;
    server_name ${APP_DOMAIN} _;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
        proxy_buffering off;
        proxy_read_timeout 100s;
        client_max_body_size 50M;
    }

    location /hangfire {
        proxy_pass http://localhost:5000/hangfire;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
    }
}
EOF

# Enable site
rm -f /etc/nginx/sites-enabled/default
ln -sf /etc/nginx/sites-available/grc /etc/nginx/sites-enabled/

nginx -t
systemctl restart nginx

# ============================================================================
# STEP 10: Run Database Migrations
# ============================================================================
log_info "Step 10: Running database migrations..."
cd /home/${APP_USER}/grc-app/src/GrcMvc
export ConnectionStrings__DefaultConnection="Host=localhost;Database=grcdb;Username=grcuser;Password=${DB_PASSWORD}"
dotnet ef database update || log_warn "Migrations may need manual review"

# ============================================================================
# STEP 11: Start Application
# ============================================================================
log_info "Step 11: Starting application..."
systemctl start grc
sleep 5

# Check if running
if systemctl is-active --quiet grc; then
    log_info "Application started successfully!"
else
    log_error "Application failed to start. Check: journalctl -u grc -f"
fi

# ============================================================================
# STEP 12: Configure Firewall
# ============================================================================
log_info "Step 12: Configuring firewall..."
ufw allow 22/tcp
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable

# ============================================================================
# DEPLOYMENT COMPLETE
# ============================================================================
echo ""
echo "=========================================="
echo -e "${GREEN}DEPLOYMENT COMPLETE!${NC}"
echo "=========================================="
echo ""
echo "Application URL: http://${APP_DOMAIN}"
echo "Server IP: $(curl -s ifconfig.me)"
echo ""
echo "Useful commands:"
echo "  - View logs:     journalctl -u grc -f"
echo "  - Restart app:   systemctl restart grc"
echo "  - App status:    systemctl status grc"
echo "  - Nginx status:  systemctl status nginx"
echo ""
echo "For SSL (HTTPS), run:"
echo "  certbot --nginx -d ${APP_DOMAIN}"
echo ""
echo "App User:   ${APP_USER}"
echo "App Dir:    ${PUBLISH_DIR}"
echo "Source Dir: /home/${APP_USER}/grc-app"
echo ""
echo "Database: grcdb"
echo "DB User:  grcuser"
echo "DB Pass:  ${DB_PASSWORD}"
echo "=========================================="
