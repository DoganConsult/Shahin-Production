#!/bin/bash
# GRC System Deployment Script
# This script deploys the GRC MVC application to the server

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}=====================================${NC}"
echo -e "${GREEN}GRC MVC System Deployment Script${NC}"
echo -e "${GREEN}=====================================${NC}"

# Configuration
APP_NAME="GrcMvc"
APP_PORT=5137
APP_USER="grcapp"
APP_GROUP="grcapp"
DEPLOY_DIR="/opt/grc-system"
DOTNET_VERSION="8.0"

# Step 1: Check prerequisites
echo -e "\n${YELLOW}Step 1: Checking prerequisites...${NC}"
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET Core is not installed${NC}"
    exit 1
fi
echo -e "${GREEN}✅ .NET $(dotnet --version) is installed${NC}"

# Step 2: Build the application
echo -e "\n${YELLOW}Step 2: Building application...${NC}"
cd /home/dogan/grc-system
dotnet build -c Release -o ./publish
echo -e "${GREEN}✅ Build completed${NC}"

# Step 3: Stop the service if running
echo -e "\n${YELLOW}Step 3: Stopping existing service...${NC}"
systemctl stop grc-mvc || true
echo -e "${GREEN}✅ Service stopped${NC}"

# Step 4: Deploy application
echo -e "\n${YELLOW}Step 4: Deploying application...${NC}"
mkdir -p $DEPLOY_DIR
cp -r ./publish/* $DEPLOY_DIR/
chown -R $APP_USER:$APP_GROUP $DEPLOY_DIR
echo -e "${GREEN}✅ Application deployed to ${DEPLOY_DIR}${NC}"

# Step 5: Create systemd service
echo -e "\n${YELLOW}Step 5: Creating systemd service...${NC}"
cat > /etc/systemd/system/grc-mvc.service << EOF
[Unit]
Description=GRC MVC Application
After=network.target
StartLimitIntervalSec=0

[Service]
Type=notify
User=$APP_USER
Group=$APP_GROUP
WorkingDirectory=$DEPLOY_DIR
ExecStart=/usr/bin/dotnet $DEPLOY_DIR/$APP_NAME.dll
Restart=always
RestartSec=10
TimeoutStopSec=30
SyslogIdentifier=grc-mvc
Environment="ASPNETCORE_ENVIRONMENT=Production"
Environment="ASPNETCORE_URLS=http://0.0.0.0:$APP_PORT"
Environment="DOTNET_PRINT_ALL_SETTINGS=false"

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
echo -e "${GREEN}✅ Systemd service created${NC}"

# Step 6: Enable and start service
echo -e "\n${YELLOW}Step 6: Starting service...${NC}"
systemctl enable grc-mvc
systemctl start grc-mvc
sleep 3

if systemctl is-active --quiet grc-mvc; then
    echo -e "${GREEN}✅ Service is running${NC}"
else
    echo -e "${RED}❌ Service failed to start${NC}"
    systemctl status grc-mvc
    exit 1
fi

# Step 7: Verify deployment
echo -e "\n${YELLOW}Step 7: Verifying deployment...${NC}"
if curl -f http://localhost:$APP_PORT/health >/dev/null 2>&1; then
    echo -e "${GREEN}✅ Application is responding to requests${NC}"
else
    echo -e "${YELLOW}⚠️  Health check endpoint not found (normal for initial setup)${NC}"
fi

# Step 8: Display status
echo -e "\n${GREEN}=====================================${NC}"
echo -e "${GREEN}Deployment completed successfully!${NC}"
echo -e "${GREEN}=====================================${NC}"
echo -e "\nApplication Details:"
echo -e "  Name: ${APP_NAME}"
echo -e "  URL: http://localhost:${APP_PORT}"
echo -e "  Deploy Directory: ${DEPLOY_DIR}"
echo -e "  Service: grc-mvc"
echo -e "\nUseful Commands:"
echo -e "  View logs: ${YELLOW}journalctl -u grc-mvc -f${NC}"
echo -e "  Check status: ${YELLOW}systemctl status grc-mvc${NC}"
echo -e "  Stop service: ${YELLOW}systemctl stop grc-mvc${NC}"
echo -e "  Start service: ${YELLOW}systemctl start grc-mvc${NC}"
echo -e "  Restart service: ${YELLOW}systemctl restart grc-mvc${NC}"

echo -e "\n${GREEN}✅ Deployment complete!${NC}"
