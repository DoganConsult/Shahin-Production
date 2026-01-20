#!/bin/bash
# ══════════════════════════════════════════════════════════════
# Setup LLM Models on Production Server
# Downloads and configures 3 default LLM models
# ══════════════════════════════════════════════════════════════

set -e

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

SERVER_IP="212.147.229.36"
SERVER_USER="root"
SSH_KEY="${SSH_KEY:-~/.ssh/id_ed25519}"

echo -e "${BLUE}Setting up LLM models on production server...${NC}"
echo ""

# Wait for Ollama to be ready
echo -e "${YELLOW}Waiting for Ollama service to start...${NC}"
sleep 30

# Pull LLM models via Ollama API
echo -e "${YELLOW}Downloading LLM Model 1: Llama 3 (8B)...${NC}"
ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << 'ENDSSH'
    docker exec shahin-ollama-prod ollama pull llama3:8b
ENDSSH
echo -e "${GREEN}✅ Llama 3:8b downloaded${NC}"
echo ""

echo -e "${YELLOW}Downloading LLM Model 2: Mistral (7B)...${NC}"
ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << 'ENDSSH'
    docker exec shahin-ollama-prod ollama pull mistral:7b
ENDSSH
echo -e "${GREEN}✅ Mistral:7b downloaded${NC}"
echo ""

echo -e "${YELLOW}Downloading LLM Model 3: Phi-3 (Mini)...${NC}"
ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << 'ENDSSH'
    docker exec shahin-ollama-prod ollama pull phi3:mini
ENDSSH
echo -e "${GREEN}✅ Phi-3:mini downloaded${NC}"
echo ""

# Verify models
echo -e "${YELLOW}Verifying installed models...${NC}"
ssh -i "$SSH_KEY" "$SERVER_USER@$SERVER_IP" << 'ENDSSH'
    docker exec shahin-ollama-prod ollama list
ENDSSH

echo ""
echo -e "${GREEN}══════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}  All 3 LLM models installed successfully${NC}"
echo -e "${GREEN}══════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "Models available:"
echo -e "  1. llama3:8b   - Meta's Llama 3 (8B parameters)"
echo -e "  2. mistral:7b   - Mistral AI (7B parameters)"
echo -e "  3. phi3:mini   - Microsoft Phi-3 (3.8B parameters)"
echo ""
echo -e "Test API: curl http://212.147.229.36:11434/api/generate"
echo ""
