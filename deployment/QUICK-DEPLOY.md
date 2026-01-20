# Quick Deploy - Complete Pipeline

## One Command Deploy Everything

```bash
cd deployment
./deploy-all.sh
```

This will:
1. ✅ Push code to GitHub
2. ✅ Build and push images to Docker Hub
3. ✅ Deploy all 8 containers to production server

## Manual Steps

### 1. Push to GitHub
```bash
./push-to-github.sh
```

### 2. Push to Docker Hub
```bash
# Set your Docker Hub username
export DOCKERHUB_USER=your-username

# Login to Docker Hub
docker login

# Push images
./push-to-dockerhub.sh
```

### 3. Deploy to Server
```bash
# Set SSH key
export SSH_KEY=~/.ssh/id_ed25519

# Deploy
./deploy-to-production.sh rebuild
```

### 4. Setup LLM Models (After deployment)
```bash
./setup-llm-models.sh
```

## All 8 Containers

### Core (5):
1. `landing` - Frontend
2. `portal` - Backend
3. `postgres` - Database
4. `redis` - Cache
5. `nginx` - Reverse Proxy

### LLM Models (3):
6. `ollama` - LLM Server
7. `llm-model-1` - Llama 3:8b
8. `llm-model-2` - Mistral:7b
9. `llm-model-3` - Phi-3:mini

## Environment Setup

```bash
# Copy template
cp .env.production.template .env.production

# Edit with your values
nano .env.production
```

## Access URLs

- Landing: http://212.147.229.36:3000
- Portal: http://212.147.229.36:5000
- Ollama API: http://212.147.229.36:11434
