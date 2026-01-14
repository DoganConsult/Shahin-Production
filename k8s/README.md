# Shahin GRC Platform - Kubernetes Deployment

High-availability Kubernetes deployment for the Shahin AI GRC Platform with multi-environment support.

## Quick Start

```bash
# 1. Install prerequisites (run once)
./setup-prerequisites.sh

# 2. Update secrets for your environment
# Edit: k8s/overlays/<env>/secrets.yaml

# 3. Deploy
./deploy.sh dev apply      # Development
./deploy.sh staging apply  # Staging
./deploy.sh production apply  # Production
```

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Cloudflare (or Internet)                      │
└─────────────────────────────┬───────────────────────────────────┘
                              │
                    ┌─────────▼─────────┐
                    │ Cloudflare Tunnel │  (Optional: for on-prem)
                    │   or LoadBalancer │
                    └─────────┬─────────┘
                              │
┌─────────────────────────────▼───────────────────────────────────┐
│                    Traefik Ingress                               │
│              (TLS termination, rate limiting)                    │
└─────────────────────────────┬───────────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────────┐
│                     GRC Portal                                   │
│               (ASP.NET Core 8.0 MVC)                            │
│                   2-6 replicas (HPA)                            │
└────────┬─────────────────┬──────────────────┬───────────────────┘
         │                 │                  │
    ┌────▼────┐      ┌─────▼─────┐      ┌─────▼─────┐
    │ Redis   │      │ PgBouncer │      │   Kafka   │
    │Sentinel │      │           │      │ 3 brokers │
    │3 nodes  │      └─────┬─────┘      └───────────┘
    └────┬────┘            │
         │           ┌─────▼─────┐
    ┌────▼────┐      │  HAProxy  │
    │  Redis  │      │write/read │
    │ Master  │      └─────┬─────┘
    │+Replicas│            │
    └─────────┘      ┌─────▼─────┐
                     │PostgreSQL │
                     │ 3 nodes   │
                     │ (Patroni) │
                     └─────┬─────┘
                           │
                     ┌─────▼─────┐
                     │   etcd    │
                     │ 3 nodes   │
                     └───────────┘
```

## Environments

| Environment | Namespace | Replicas | Resources | Use Case |
|-------------|-----------|----------|-----------|----------|
| **dev** | shahin-grc-dev | 1 each | Minimal | Local development |
| **staging** | shahin-grc-staging | 2 each | Moderate | Testing/QA |
| **production** | shahin-grc | 3 each | Full HA | Live system |

## Prerequisites

### Required Components

Run the automated setup script:

```bash
./setup-prerequisites.sh
```

This installs:
- **Longhorn** - Distributed block storage
- **Cert-Manager** - TLS certificate management
- **Traefik** - Ingress controller
- **Metrics Server** - For HPA autoscaling

### Infrastructure Requirements

| Environment | Nodes | CPU/Node | RAM/Node | Storage |
|-------------|-------|----------|----------|---------|
| Dev | 1 | 2 vCPU | 4 GB | 50 GB |
| Staging | 2-3 | 4 vCPU | 8 GB | 100 GB |
| Production | 3+ | 4 vCPU | 16 GB | 200 GB |

## Cloudflare Tunnel (Recommended for On-Premises)

For exposing services without a public IP:

### Option 1: Deploy to Kubernetes

```bash
kubectl apply -f k8s/cloudflare/cloudflared-deployment.yaml
```

### Option 2: Run Standalone

```bash
docker run -d --restart=always --name cloudflared \
  cloudflare/cloudflared:latest tunnel --no-autoupdate run \
  --token YOUR_TUNNEL_TOKEN
```

Configure routes in Cloudflare Dashboard:
- `portal.shahin-ai.com` → `http://traefik.traefik.svc:80`
- `staging.shahin-ai.com` → `http://traefik.traefik.svc:80`

## DNS Configuration

### With Cloudflare Tunnel (No Public IP needed)
DNS is automatically configured through Cloudflare.

### With LoadBalancer

```bash
# Get LoadBalancer IP
kubectl get svc traefik -n traefik -o jsonpath='{.status.loadBalancer.ingress[0].ip}'
```

Configure A records:
| Domain | Target |
|--------|--------|
| dev.shahin-ai.com | LoadBalancer IP |
| staging.shahin-ai.com | LoadBalancer IP |
| portal.shahin-ai.com | LoadBalancer IP |

### Local Development (Port Forward)

```bash
kubectl port-forward svc/dev-grc-portal 8080:8080 -n shahin-grc-dev
# Access at: http://localhost:8080
```

## Secrets Configuration

### Required Secrets

Each environment needs secrets configured:

```bash
# Development - edit and apply
vi k8s/overlays/dev/secrets.yaml
kubectl apply -f k8s/overlays/dev/secrets.yaml

# Staging
vi k8s/overlays/staging/secrets.yaml
kubectl apply -f k8s/overlays/staging/secrets.yaml

# Production (use strong passwords!)
vi k8s/overlays/production/secrets.yaml
kubectl apply -f k8s/overlays/production/secrets.yaml
```

### Generate Secure Passwords

```bash
# For staging/production passwords
openssl rand -base64 32

# For JWT secret (64 chars for production)
openssl rand -base64 64
```

## Deployment Commands

```bash
# Preview changes
./deploy.sh <env> diff

# Deploy/Update
./deploy.sh <env> apply

# Check status
./deploy.sh <env> status

# Health check
./deploy.sh <env> health

# Delete environment
./deploy.sh <env> delete
```

## Verification

### Cluster Health

```bash
# PostgreSQL Patroni cluster
kubectl exec -n shahin-grc postgres-0 -- patronictl list

# Redis Sentinel
kubectl exec -n shahin-grc redis-sentinel-0 -- redis-cli -p 26379 SENTINEL masters

# Application health
curl https://portal.shahin-ai.com/health
```

### Failover Testing

```bash
# Test PostgreSQL failover
kubectl delete pod postgres-0 -n shahin-grc
watch kubectl exec -n shahin-grc postgres-1 -- patronictl list

# Test Redis failover
kubectl delete pod redis-master-0 -n shahin-grc
kubectl exec -n shahin-grc redis-sentinel-0 -- redis-cli -p 26379 SENTINEL get-master-addr-by-name shahin-master
```

## Directory Structure

```
k8s/
├── base/                       # Kustomize base
├── overlays/                   # Environment configs
│   ├── dev/
│   ├── staging/
│   └── production/
├── etcd/                       # Consensus cluster
├── database/                   # PostgreSQL HA
├── redis/                      # Redis + Sentinel
├── kafka/                      # Kafka cluster
├── applications/               # GRC Portal
├── cloudflare/                 # Cloudflare Tunnel
├── ingress/                    # Traefik config
├── network-policies/           # Security
├── monitoring/                 # Prometheus
├── backup/                     # Backup jobs
├── deploy.sh                   # Deploy script
├── setup-prerequisites.sh      # Setup script
└── README.md
```

## Troubleshooting

### Pods Not Starting

```bash
kubectl get events -n <namespace> --sort-by='.lastTimestamp'
kubectl describe pod <pod-name> -n <namespace>
kubectl logs <pod-name> -n <namespace>
```

### Database Issues

```bash
kubectl exec -n shahin-grc postgres-0 -- pg_isready
kubectl logs -n shahin-grc -l app.kubernetes.io/name=pgbouncer
```

### Certificate Issues

```bash
kubectl get certificates -A
kubectl describe certificate <name> -n <namespace>
```

## Support

- Issues: https://github.com/doganlap/shahin-grc/issues
