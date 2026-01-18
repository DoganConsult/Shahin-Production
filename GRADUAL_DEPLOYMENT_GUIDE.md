# Gradual Production Deployment - Execution Guide
## Step-by-Step Implementation

---

## 🚀 Deployment Strategy Overview

### Gradual Rollout Approach
```
Week 1-2:  5% users (Canary)
Week 3-4:  25% users (Pilot)
Week 5-6:  50% users (Partial)
Week 7-8:  100% users (Full)
```

---

## Step 1: Infrastructure Setup

### 1.1 Create Multiple Deployment Environments
```bash
# Production Environments
prod-canary.shahin-grc.com    # 5% traffic
prod-pilot.shahin-grc.com     # 25% traffic
prod-partial.shahin-grc.com   # 50% traffic
prod-full.shahin-grc.com      # 100% traffic
```

### 1.2 Docker Deployment Configuration
```yaml
# docker-compose.prod.yml
version: '3.8'
services:
  grc-canary:
    image: shahin-grc:${VERSION}
    environment:
      - DEPLOYMENT_STAGE=canary
      - FEATURE_FLAGS_ENABLED=true
      - MAX_USERS=50
    deploy:
      replicas: 1
      
  grc-pilot:
    image: shahin-grc:${VERSION}
    environment:
      - DEPLOYMENT_STAGE=pilot
      - FEATURE_FLAGS_ENABLED=true
      - MAX_USERS=250
    deploy:
      replicas: 2
      
  grc-stable:
    image: shahin-grc:stable
    environment:
      - DEPLOYMENT_STAGE=stable
    deploy:
      replicas: 5
```

---

## Step 2: Feature Flag Implementation

### 2.1 Create Feature Flag Configuration
```csharp
// appsettings.Production.json
{
  "FeatureManagement": {
    "Phase1_Foundation": true,
    "Phase2_CoreGRC_Risk": false,
    "Phase2_CoreGRC_Control": false,
    "Phase2_CoreGRC_Assessment": false,
    "Phase3_CoreGRC_Audit": false,
    "Phase3_CoreGRC_Policy": false,
    "Phase4_Workflow": false,
    "Phase5_Integrations": false,
    "Phase6_AI_Analytics": false
  }
}
```

### 2.2 Implement Feature Toggle Service
```csharp
// FeatureToggleService.cs
public class FeatureToggleService
{
    private readonly IConfiguration _configuration;
    
    public bool IsFeatureEnabled(string feature)
    {
        return _configuration.GetValue<bool>($"FeatureManagement:{feature}");
    }
    
    public void EnableFeature(string feature)
    {
        // Update configuration in database/config server
        var settings = new Dictionary<string, string>
        {
            {$"FeatureManagement:{feature}", "true"}
        };
        UpdateConfiguration(settings);
    }
}
```

### 2.3 Controller Feature Gates
```csharp
// RiskController.cs
[Authorize]
[FeatureGate("Phase2_CoreGRC_Risk")]
public class RiskController : Controller
{
    // Controller only accessible when feature is enabled
}
```

---

## Step 3: Traffic Routing Configuration

### 3.1 Nginx Load Balancer Configuration
```nginx
# /etc/nginx/sites-available/grc-gradual
upstream grc_backend {
    # Canary deployment (5% traffic)
    server prod-canary.internal:5000 weight=5;
    
    # Stable deployment (95% traffic)
    server prod-stable.internal:5000 weight=95;
}

server {
    listen 443 ssl;
    server_name app.shahin-grc.com;
    
    location / {
        proxy_pass http://grc_backend;
        proxy_set_header X-Deployment-Stage $upstream_addr;
    }
    
    # Sticky sessions for consistent user experience
    ip_hash;
}
```

### 3.2 Azure Application Gateway Rules
```powershell
# PowerShell script for Azure
$gateway = Get-AzApplicationGateway -Name "grc-gateway" -ResourceGroupName "grc-prod"

# Add backend pools
$canaryPool = New-AzApplicationGatewayBackendAddressPool `
    -Name "canary-pool" `
    -BackendAddresses "10.0.1.10"

$stablePool = New-AzApplicationGatewayBackendAddressPool `
    -Name "stable-pool" `
    -BackendAddresses "10.0.1.20","10.0.1.21","10.0.1.22"

# Configure weighted routing
$routingRule = New-AzApplicationGatewayRequestRoutingRule `
    -Name "gradual-routing" `
    -RuleType PathBasedRouting `
    -BackendAddressPool $canaryPool,$stablePool `
    -WeightedTargets @{$canaryPool=5; $stablePool=95}
```

---

## Step 4: Deployment Automation Scripts

### 4.1 Main Deployment Script
```bash
#!/bin/bash
# deploy-gradual.sh

STAGE=$1  # canary, pilot, partial, full
VERSION=$2

echo "🚀 Starting gradual deployment: Stage=$STAGE, Version=$VERSION"

# Step 1: Build and tag Docker image
docker build -t shahin-grc:$VERSION .
docker tag shahin-grc:$VERSION registry.shahin-grc.com/grc:$VERSION
docker push registry.shahin-grc.com/grc:$VERSION

# Step 2: Update deployment based on stage
case $STAGE in
  canary)
    ./scripts/deploy-canary.sh $VERSION
    ;;
  pilot)
    ./scripts/deploy-pilot.sh $VERSION
    ;;
  partial)
    ./scripts/deploy-partial.sh $VERSION
    ;;
  full)
    ./scripts/deploy-full.sh $VERSION
    ;;
esac

# Step 3: Run health checks
./scripts/health-check.sh $STAGE

# Step 4: Monitor metrics
./scripts/monitor-deployment.sh $STAGE
```

### 4.2 Canary Deployment Script
```bash
#!/bin/bash
# scripts/deploy-canary.sh

VERSION=$1
echo "📦 Deploying canary version: $VERSION"

# Update canary environment
kubectl set image deployment/grc-canary \
  grc=registry.shahin-grc.com/grc:$VERSION \
  -n production

# Wait for rollout
kubectl rollout status deployment/grc-canary -n production

# Enable Phase 1 features only
curl -X POST https://config.shahin-grc.com/api/features \
  -H "Content-Type: application/json" \
  -d '{
    "environment": "canary",
    "features": {
      "Phase1_Foundation": true,
      "Phase2_CoreGRC_Risk": false
    }
  }'

# Validate deployment
./scripts/validate-canary.sh
```

---

## Step 5: Database Migration Strategy

### 5.1 Blue-Green Database Migration
```bash
#!/bin/bash
# migrate-database-gradual.sh

PHASE=$1

echo "🗄️ Running database migration for Phase: $PHASE"

# Create backup
docker exec shahin-postgres pg_dump -U shahin_admin shahin_grc > backup_$(date +%Y%m%d_%H%M%S).sql

# Run migrations based on phase
case $PHASE in
  phase1)
    dotnet ef database update Phase1_Foundation --context GrcDbContext
    dotnet ef database update Phase1_Identity --context GrcAuthDbContext
    ;;
  phase2)
    dotnet ef database update Phase2_CoreGRC --context GrcDbContext
    ;;
  phase3)
    dotnet ef database update Phase3_Advanced --context GrcDbContext
    ;;
esac

# Verify migration
dotnet run --project src/GrcMvc/GrcMvc.csproj -- --validate-migration
```

---

## Step 6: User Segmentation

### 6.1 User Group Configuration
```csharp
// UserSegmentationService.cs
public class UserSegmentationService
{
    public DeploymentStage GetUserDeploymentStage(string userId)
    {
        var user = GetUser(userId);
        
        // Early adopters (5%)
        if (user.Tags.Contains("early-adopter") || 
            user.Department == "IT")
        {
            return DeploymentStage.Canary;
        }
        
        // Pilot users (25%)
        if (user.Tags.Contains("pilot") || 
            user.Department == "Finance" || 
            user.Department == "Risk")
        {
            return DeploymentStage.Pilot;
        }
        
        // Partial rollout (50%)
        if (user.CreatedDate < DateTime.Now.AddMonths(-6))
        {
            return DeploymentStage.Partial;
        }
        
        // Everyone else stays on stable
        return DeploymentStage.Stable;
    }
}
```

### 6.2 Middleware for Routing
```csharp
// GradualDeploymentMiddleware.cs
public class GradualDeploymentMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.Identity.Name;
        var stage = _segmentation.GetUserDeploymentStage(userId);
        
        // Route to appropriate backend
        context.Request.Headers.Add("X-Deployment-Stage", stage.ToString());
        
        // Enable features based on stage
        var features = GetFeaturesForStage(stage);
        context.Items["EnabledFeatures"] = features;
        
        await _next(context);
    }
}
```

---

## Step 7: Monitoring & Validation

### 7.1 Health Check Endpoints
```csharp
// HealthCheckController.cs
[ApiController]
[Route("api/health")]
public class HealthCheckController : ControllerBase
{
    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        var checks = new Dictionary<string, bool>
        {
            ["database"] = await CheckDatabase(),
            ["redis"] = await CheckRedis(),
            ["auth"] = await CheckAuthentication(),
            ["features"] = await CheckFeatures()
        };
        
        return Ok(new { 
            status = checks.All(c => c.Value) ? "healthy" : "unhealthy",
            checks,
            stage = GetDeploymentStage(),
            version = GetVersion()
        });
    }
}
```

### 7.2 Monitoring Script
```bash
#!/bin/bash
# monitor-deployment.sh

STAGE=$1
THRESHOLD_ERROR_RATE=0.01  # 1%
THRESHOLD_RESPONSE_TIME=500  # ms

while true; do
  # Check error rate
  ERROR_RATE=$(curl -s https://metrics.shahin-grc.com/api/errors?stage=$STAGE | jq .rate)
  
  # Check response time
  RESPONSE_TIME=$(curl -s https://metrics.shahin-grc.com/api/performance?stage=$STAGE | jq .p95)
  
  # Alert if thresholds exceeded
  if (( $(echo "$ERROR_RATE > $THRESHOLD_ERROR_RATE" | bc -l) )); then
    echo "⚠️ HIGH ERROR RATE: $ERROR_RATE"
    ./scripts/alert-team.sh "High error rate in $STAGE: $ERROR_RATE"
  fi
  
  if (( $(echo "$RESPONSE_TIME > $THRESHOLD_RESPONSE_TIME" | bc -l) )); then
    echo "⚠️ SLOW RESPONSE: ${RESPONSE_TIME}ms"
    ./scripts/alert-team.sh "Slow response in $STAGE: ${RESPONSE_TIME}ms"
  fi
  
  sleep 60
done
```

---

## Step 8: Rollback Procedures

### 8.1 Automatic Rollback Script
```bash
#!/bin/bash
# rollback-deployment.sh

STAGE=$1
REASON=$2

echo "🔄 Rolling back $STAGE deployment. Reason: $REASON"

# Stop bad deployment
kubectl rollout undo deployment/grc-$STAGE -n production

# Restore previous version
PREVIOUS_VERSION=$(kubectl rollout history deployment/grc-$STAGE -n production | tail -2 | head -1 | awk '{print $1}')
kubectl rollout undo deployment/grc-$STAGE --to-revision=$PREVIOUS_VERSION -n production

# Disable problematic features
curl -X POST https://config.shahin-grc.com/api/features/disable \
  -d "{ \"stage\": \"$STAGE\", \"reason\": \"$REASON\" }"

# Notify team
./scripts/notify-rollback.sh "$STAGE" "$REASON"
```

### 8.2 Database Rollback
```sql
-- rollback-migration.sql
BEGIN TRANSACTION;

-- Remove new columns/tables from failed deployment
DELETE FROM "__EFMigrationsHistory" 
WHERE "MigrationId" = '20250117_FailedMigration';

-- Restore schema
ALTER TABLE "AspNetUsers" DROP COLUMN IF EXISTS "NewProblematicColumn";
DROP TABLE IF EXISTS "NewProblematicTable";

-- Verify rollback
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId" DESC LIMIT 5;

COMMIT;
```

---

## Step 9: Gradual Feature Enablement

### Week 1-2: Foundation Only
```json
{
  "canary": {
    "users": "5%",
    "features": ["authentication", "authorization", "tenant"],
    "modules": []
  }
}
```

### Week 3-4: Add Core GRC
```json
{
  "pilot": {
    "users": "25%",
    "features": ["authentication", "authorization", "tenant"],
    "modules": ["risk", "control", "assessment"]
  }
}
```

### Week 5-6: Full GRC Suite
```json
{
  "partial": {
    "users": "50%",
    "features": ["authentication", "authorization", "tenant"],
    "modules": ["risk", "control", "assessment", "audit", "policy", "evidence"]
  }
}
```

### Week 7-8: Everything
```json
{
  "full": {
    "users": "100%",
    "features": ["all"],
    "modules": ["all"]
  }
}
```

---

## Step 10: Validation Checklist

### Before Each Stage
```bash
#!/bin/bash
# pre-deployment-check.sh

echo "✅ Pre-deployment checklist:"

# 1. Backup exists
if [ -f "backup_latest.sql" ]; then
  echo "✓ Database backup exists"
else
  echo "✗ No backup found - creating..."
  ./scripts/backup-database.sh
fi

# 2. Tests pass
if dotnet test; then
  echo "✓ All tests pass"
else
  echo "✗ Tests failing - aborting"
  exit 1
fi

# 3. Health checks pass
if curl -f https://prod-stable.shahin-grc.com/api/health; then
  echo "✓ Current production healthy"
else
  echo "✗ Production unhealthy - investigate first"
  exit 1
fi

# 4. Team notification
./scripts/notify-team.sh "Starting deployment to $STAGE"

echo "✅ Ready to deploy!"
```

---

## Quick Start Commands

### Deploy Canary (5% users)
```bash
./deploy-gradual.sh canary v1.0.1
```

### Promote to Pilot (25% users)
```bash
./deploy-gradual.sh pilot v1.0.1
```

### Expand to Partial (50% users)
```bash
./deploy-gradual.sh partial v1.0.1
```

### Full Deployment (100% users)
```bash
./deploy-gradual.sh full v1.0.1
```

### Emergency Rollback
```bash
./rollback-deployment.sh canary "High error rate detected"
```

---

## Success Metrics Dashboard

```
┌─────────────────────────────────────┐
│     GRADUAL DEPLOYMENT MONITOR      │
├─────────────────────────────────────┤
│ Stage: CANARY (5%)                  │
│ Version: v1.0.1                     │
│ Users: 50/1000                     │
│                                     │
│ ▓▓░░░░░░░░░░░░░░░░░░ 5%           │
│                                     │
│ Metrics:                            │
│ • Error Rate: 0.03% ✅              │
│ • Response Time: 145ms ✅           │
│ • Success Rate: 99.97% ✅           │
│ • Active Users: 47                  │
│                                     │
│ Features Enabled:                   │
│ ✅ Authentication                   │
│ ✅ Authorization                    │
│ ✅ Tenant Management                │
│ ⬜ Risk Module                      │
│ ⬜ Control Module                   │
│                                     │
│ [Promote] [Hold] [Rollback]         │
└─────────────────────────────────────┘
```

---

## Timeline Summary

| Week | Stage | Users | Action |
|------|-------|-------|--------|
| 1-2 | Canary | 5% | Deploy foundation, monitor closely |
| 3-4 | Pilot | 25% | Enable core GRC modules |
| 5-6 | Partial | 50% | Add advanced features |
| 7-8 | Full | 100% | Complete rollout |

---

**Ready to start gradual deployment!**
