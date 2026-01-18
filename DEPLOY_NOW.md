# 🚀 QUICK DEPLOYMENT GUIDE
## Deploy Shahin GRC Platform to Production

---

## Prerequisites Checklist

- [ ] Docker and Docker Compose installed
- [ ] PostgreSQL client tools (for backups) - optional
- [ ] 16+ GB RAM available
- [ ] 50+ GB disk space

---

## Step 1: Create Environment File (2 minutes)

```bash
cd Shahin-Jan-2026

# Copy template
cp src/GrcMvc/env.production.template .env.production

# Generate secure secrets
echo "JWT_SECRET=$(openssl rand -base64 64 | tr -d '\n')" >> .env.production
echo "DB_PASSWORD=$(openssl rand -base64 32 | tr -d '\n')" >> .env.production
```

**Edit `.env.production`** and set:
- `JWT_SECRET` (if not generated above)
- `DB_PASSWORD` (if not generated above)
- `CLAUDE_API_KEY` (optional - for AI features)

---

## Step 2: Build and Deploy (5 minutes)

```bash
# Build the Docker image
docker-compose -f docker-compose.production.yml build

# Start all services
docker-compose -f docker-compose.production.yml up -d

# Watch logs
docker-compose -f docker-compose.production.yml logs -f grcmvc-prod
```

---

## Step 3: Verify Deployment (2 minutes)

```bash
# Check health endpoint (note: app runs on port 8080)
curl http://localhost:8080/health
# Expected: {"status":"Healthy",...}

# Check all containers are running
docker-compose -f docker-compose.production.yml ps
# Expected: All containers "Up"

# Check Hangfire dashboard (background jobs)
# Open: http://localhost:8080/hangfire
```

---

## Step 4: Initial Setup (5 minutes)

1. **Open the application**: http://localhost:8080

2. **Create Platform Admin**:
   - Click "Register" 
   - Use email: `admin@yourdomain.com`
   - Complete 12-step onboarding wizard

3. **Verify Background Jobs**:
   - Go to http://localhost:8080/hangfire
   - Check "Recurring Jobs" tab
   - Should see:
     - `database-backup-daily` (2 AM)
     - `trial-nurture-hourly`
     - `sync-scheduler` (every 5 min)
     - etc.

---

## What Works After Deployment

| Feature | Status |
|---------|--------|
| Core GRC (Risk, Control, Audit) | ✅ Full |
| 12-Step Onboarding Wizard | ✅ Full |
| Workflow Engine | ✅ Full |
| Multi-Tenant (Database Isolation) | ✅ Full |
| Arabic Language + RTL | ✅ 90% |
| AI Agents (Claude) | ✅ Full (if API key set) |
| Background Jobs (Hangfire) | ✅ Full |
| Database Backups | ✅ Full (daily @ 2 AM) |
| Email Notifications | ⚠️ Requires SMTP config |
| SSO (Azure/Google/Okta) | ⚠️ Requires Azure config |

---

## Troubleshooting

### "JWT_SECRET is required" error
```bash
# Generate and add to .env.production
echo "JWT_SECRET=$(openssl rand -base64 64 | tr -d '\n')" >> .env.production
```

### Database connection failed
```bash
# Check db-prod container
docker logs shahin-grc-db-prod

# Verify password matches
grep DB_PASSWORD .env.production

# Wait for database to be ready (takes ~10-15 seconds)
docker-compose -f docker-compose.production.yml exec db-prod pg_isready -U grc_admin
```

### Database tables missing / migration errors
```bash
# The app applies migrations automatically on startup
# Check app logs for migration status:
docker logs shahin-grc-production | grep -i "migration\|schema"

# If migrations fail, you can manually apply them:
docker-compose -f docker-compose.production.yml exec grcmvc-prod \
  dotnet ef database update --project /app/GrcMvc.dll
```

### Container keeps restarting
```bash
# Check application logs
docker logs shahin-grc-production --tail 100

# Check health
docker exec shahin-grc-production curl -s http://localhost/health
```

### Claude AI not working
1. Verify `CLAUDE_API_KEY` is set in `.env.production`
2. Verify `CLAUDE_ENABLED=true`
3. Check logs for "Claude" errors

---

## Kubernetes Deployment

For K8s deployment instead of Docker Compose:

```bash
# Create namespace
kubectl create namespace shahin-grc

# Apply PVCs first
kubectl apply -f k8s/applications/grc-portal-pvc.yaml

# Create secrets (replace with your values)
kubectl create secret generic db-credentials \
  --from-literal=CONNECTION_STRING="Host=db;Database=GrcMvcDb;Username=shahin_admin;Password=YOUR_PASSWORD" \
  --from-literal=AUTH_CONNECTION_STRING="Host=db;Database=GrcMvcDb_auth;Username=shahin_admin;Password=YOUR_PASSWORD" \
  -n shahin-grc

kubectl create secret generic jwt-secret \
  --from-literal=JWT_SECRET="$(openssl rand -base64 64)" \
  -n shahin-grc

# Apply deployment
kubectl apply -f k8s/applications/grc-portal-deployment.yaml

# Check status
kubectl get pods -n shahin-grc
```

---

## Next Steps After Deployment

1. **Configure Email** (for notifications):
   - Set `SMTP_HOST`, `SMTP_USERNAME`, `SMTP_PASSWORD`
   - Or use Microsoft Graph OAuth2

2. **Configure SSO** (for enterprise login):
   - Set `AZURE_TENANT_ID` and related credentials
   - Or configure Google/Okta in appsettings

3. **Enable AI Features**:
   - Get Claude API key from https://console.anthropic.com/
   - Set `CLAUDE_API_KEY` and `CLAUDE_ENABLED=true`

4. **Set up Monitoring**:
   - Configure Application Insights or Prometheus
   - Set up Sentry for error tracking

---

**Deployment Complete! 🎉**

For detailed documentation, see:
- `FINAL_PRODUCTION_DEPLOYMENT_PLAN.md` - Full action plan
- `PRODUCTION_INFRASTRUCTURE_SIZING.md` - Server requirements
- `src/GrcMvc/env.production.template` - All environment variables
