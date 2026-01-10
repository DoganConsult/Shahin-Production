#!/bin/bash
# ============================================================================
# GRC Application - Update/Redeploy Script
# Run this to pull latest changes and redeploy
# Usage: sudo bash hetzner-update.sh
# ============================================================================

set -e

echo "=========================================="
echo "GRC Application - Update Deployment"
echo "=========================================="

cd /var/www/grc-app

echo "[1/5] Pulling latest code from GitHub..."
git pull origin main

echo "[2/5] Building application..."
cd src/GrcMvc
dotnet publish -c Release -o /var/www/grc-published

echo "[3/5] Setting permissions..."
chown -R www-data:www-data /var/www/grc-published

echo "[4/5] Running migrations..."
export ConnectionStrings__DefaultConnection="Host=localhost;Database=grcdb;Username=grcuser;Password=GrcSecure2026!"
dotnet ef database update || echo "Migrations skipped or already applied"

echo "[5/5] Restarting services..."
systemctl restart grc
systemctl restart nginx

sleep 3

if systemctl is-active --quiet grc; then
    echo "=========================================="
    echo "UPDATE COMPLETE - Application is running!"
    echo "=========================================="
else
    echo "ERROR: Application failed to start"
    echo "Check logs: journalctl -u grc -f"
fi
