#!/bin/bash

# Start GRC Application Script
# Ensures the application starts and stays running

set -e

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

APP_DIR="/home/dogan/grc-system/src/GrcMvc"
LOG_FILE="/tmp/grcmvc-app.log"
PID_FILE="/tmp/grcmvc.pid"

echo -e "${GREEN}Starting GRC Application...${NC}"

# Stop any existing instances
if [ -f "$PID_FILE" ]; then
    OLD_PID=$(cat "$PID_FILE")
    if ps -p "$OLD_PID" > /dev/null 2>&1; then
        echo -e "${YELLOW}Stopping existing instance (PID: $OLD_PID)...${NC}"
        kill "$OLD_PID" 2>/dev/null || true
        sleep 2
    fi
    rm -f "$PID_FILE"
fi

# Kill any other dotnet GrcMvc processes
pkill -f "dotnet.*GrcMvc" 2>/dev/null || true
sleep 2

# Change to application directory
cd "$APP_DIR" || exit 1

# Start the application
echo -e "${GREEN}Starting application on ports 5000 (HTTP) and 5001 (HTTPS)...${NC}"
nohup dotnet run --project GrcMvc.csproj --urls "http://0.0.0.0:5000;https://0.0.0.0:5001" > "$LOG_FILE" 2>&1 &

# Get the process ID
APP_PID=$!
echo "$APP_PID" > "$PID_FILE"

echo -e "${GREEN}Application started with PID: $APP_PID${NC}"
echo -e "${YELLOW}Waiting for application to initialize (30 seconds)...${NC}"

# Wait for application to start
sleep 30

# Check if process is still running
if ! ps -p "$APP_PID" > /dev/null 2>&1; then
    echo -e "${RED}Application process died. Check logs: $LOG_FILE${NC}"
    tail -50 "$LOG_FILE"
    exit 1
fi

# Check if ports are listening
if ss -tlnp 2>/dev/null | grep -q ":5000\|:5001"; then
    echo -e "${GREEN}✅ Application is listening on ports 5000 and 5001${NC}"
else
    echo -e "${YELLOW}⚠ Ports not yet listening, application may still be starting...${NC}"
fi

# Test health endpoint
echo -e "${YELLOW}Testing health endpoint...${NC}"
sleep 5

if curl -k -s https://localhost:5001/health > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Health check passed${NC}"
    curl -k -s https://localhost:5001/health
    echo ""
else
    echo -e "${YELLOW}⚠ Health check not yet responding (may need more time)${NC}"
fi

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}Application Status${NC}"
echo -e "${GREEN}========================================${NC}"
echo -e "Process ID: $APP_PID"
echo -e "Log File: $LOG_FILE"
echo -e "HTTP URL: http://localhost:5000"
echo -e "HTTPS URL: https://localhost:5001"
echo -e ""
echo -e "${GREEN}✅ Application startup initiated${NC}"
echo -e "${YELLOW}Note: Seed data initialization runs in background and may take 1-2 minutes${NC}"
