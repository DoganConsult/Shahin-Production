# 🔧 Shahin AI - Deployment Issues & Fixes

## 📊 Current Status Summary

### ✅ Working Services
- **PostgreSQL Database** (shahin-grc-db) - Port 5432 - HEALTHY
- **Redis Cache** (shahin-grc-redis) - Port 6379 - HEALTHY  
- **PgAdmin** (shahin-grc-pgadmin) - Port 5050 - RUNNING

### ❌ Failing Services
- **GRC Portal** (shahin-portal) - Port 5000 - UNHEALTHY
- **Marketing Site** (marketing-prod) - BUILD FAILED

---

## 🚨 Critical Issues Identified

### Issue 1: GRC Portal - Missing Claude AI Service Dependency

**Error:**
```
Autofac.Core.DependencyResolutionException: Cannot resolve parameter 
'GrcMvc.Services.Interfaces.IClaudeAgentService claudeService' of constructor
```

**Root Cause:**
- The application requires `IClaudeAgentService` for email processing
- This service is not registered in the dependency injection container
- Hangfire background jobs are failing repeatedly trying to process emails

**Impact:**
- Application starts but is unhealthy
- Background email processing fails
- HTTP requests may hang or redirect infinitely

**Fix Required:**
1. Register `IClaudeAgentService` in DI container
2. OR make the service optional/nullable
3. OR disable email processing jobs temporarily

### Issue 2: Marketing Site - Missing next-intl Module

**Error:**
```
Cannot find module 'next-intl/server'
```

**Root Cause:**
- Missing npm dependency in marketing-site
- Package.json may not include next-intl
- OR node_modules not properly installed

**Fix Required:**
1. Add next-intl to package.json dependencies
2. Run npm install in marketing-site directory
3. Rebuild Docker image

### Issue 3: Port Binding & Network Issues

**Symptoms:**
- localhost:5000 refuses connection from host
- ERR_TOO_MANY_REDIRECTS when accessing portal
- Application running inside container but not accessible

**Possible Causes:**
1. Application not listening on 0.0.0.0 (listening on 127.0.0.1 only)
2. HTTPS redirect loop (app expects HTTPS but Docker exposes HTTP)
3. Authentication redirect loop
4. Reverse proxy misconfiguration

---

## 🔧 Immediate Fixes

### Fix 1: Disable Claude AI Service Dependency (Quick Fix)

**Option A: Make Service Optional**

Edit `src/GrcMvc/Services/EmailOperations/EmailAiService.cs`:

```csharp
public class EmailAiService
{
    private readonly IClaudeAgentService? _claudeService;
    private readonly ILogger<EmailAiService> _logger;

    public EmailAiService(
        IClaudeAgentService? claudeService,  // Make nullable
        ILogger<EmailAiService> logger)
    {
        _claudeService = claudeService;
        _logger = logger;
    }

    // Update methods to check for null
    public async Task ProcessEmail(...)
    {
        if (_claudeService == null)
        {
            _logger.LogWarning("Claude AI service not available, skipping AI processing");
            return;
        }
        // ... rest of code
    }
}
```

**Option B: Register Mock Service**

Edit `src/GrcMvc/Program.cs` or DI configuration:

```csharp
// Add this to service registration
builder.Services.AddSingleton<IClaudeAgentService, MockClaudeAgentService>();

// Create mock implementation
public class MockClaudeAgentService : IClaudeAgentService
{
    public Task<string> ProcessAsync(string input)
    {
        return Task.FromResult("Mock response");
    }
    // Implement other interface methods
}
```

**Option C: Disable Hangfire Email Jobs**

Edit `src/GrcMvc/Program.cs`:

```csharp
// Comment out or remove Hangfire job registration
// RecurringJob.AddOrUpdate<EmailProcessingJob>(...)
```

### Fix 2: Fix Marketing Site Dependencies

**Step 1: Check package.json**

```bash
cd Shahin-Jan-2026/marketing-site
cat package.json | grep next-intl
```

**Step 2: Add missing dependency**

```bash
cd Shahin-Jan-2026/marketing-site
npm install next-intl --save
```

**Step 3: Rebuild Docker image**

```bash
docker-compose -f docker-compose.production.yml build marketing-prod
```

### Fix 3: Fix Port Binding & HTTPS Redirects

**Option A: Update appsettings.json**

Edit `src/GrcMvc/appsettings.Production.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:80"
      }
    }
  },
  "ForwardedHeaders": {
    "ForwardedHeaders": "XForwardedFor, XForwardedProto"
  },
  "UseHttpsRedirection": false  // Disable for Docker
}
```

**Option B: Update Dockerfile**

Ensure Dockerfile exposes correct port:

```dockerfile
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
```

**Option C: Update docker-compose.yml**

```yaml
services:
  grcmvc-prod:
    environment:
      - ASPNETCORE_URLS=http://+:80
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_HTTPS_PORT=
    ports:
      - "5000:80"  # Map host 5000 to container 80
```

---

## 📋 Step-by-Step Fix Implementation

### Phase 1: Fix GRC Portal (Priority 1)

```bash
# 1. Stop the unhealthy container
docker stop shahin-portal

# 2. Apply code fixes (choose one option above)
# Edit the necessary files

# 3. Rebuild the image
cd Shahin-Jan-2026
docker-compose -f docker-compose.production.yml build grcmvc-prod

# 4. Start the container
docker-compose -f docker-compose.production.yml up -d grcmvc-prod

# 5. Check logs
docker logs -f shahin-portal

# 6. Test access
curl http://localhost:5000/health
```

### Phase 2: Fix Marketing Site (Priority 2)

```bash
# 1. Fix dependencies
cd Shahin-Jan-2026/marketing-site
npm install next-intl --save

# 2. Rebuild image
cd ..
docker-compose -f docker-compose.production.yml build marketing-prod

# 3. Start container
docker-compose -f docker-compose.production.yml up -d marketing-prod

# 4. Check logs
docker logs -f marketing-prod
```

### Phase 3: Verify Full Stack

```bash
# Check all services
docker-compose -f docker-compose.production.yml ps

# Test endpoints
curl http://localhost:5000/
curl http://localhost:3000/

# Check health
docker-compose -f docker-compose.production.yml exec grcmvc-prod curl http://localhost:80/health
```

---

## 🎯 Recommended Approach

### For Local Development/Testing:

1. **Simplify Configuration**
   - Disable Claude AI service temporarily
   - Use mock services for external dependencies
   - Disable HTTPS redirects
   - Use simple authentication

2. **Fix Marketing Site**
   - Install all npm dependencies
   - Ensure build succeeds locally first
   - Then containerize

3. **Test Incrementally**
   - Start with database only
   - Add Redis
   - Add GRC portal
   - Finally add marketing site

### For Production Deployment:

1. **Implement Proper Services**
   - Register all required services
   - Configure Claude AI properly
   - Set up proper authentication

2. **Use Environment Variables**
   - Externalize all configuration
   - Use secrets management
   - Configure per environment

3. **Set Up Reverse Proxy**
   - Use nginx for SSL termination
   - Configure proper headers
   - Handle redirects correctly

---

## 🔍 Debugging Commands

```bash
# Check container status
docker ps -a

# View logs
docker logs shahin-portal --tail 50
docker logs marketing-prod --tail 50

# Inspect container
docker inspect shahin-portal

# Check network
docker network inspect shahin-jan-2026_default

# Test from inside container
docker exec shahin-portal curl http://localhost:80/

# Check environment variables
docker exec shahin-portal env | grep ASPNETCORE

# Check listening ports (if tools available)
docker exec shahin-portal netstat -tln
```

---

## ✅ Success Criteria

### GRC Portal Working:
- [ ] Container status: healthy
- [ ] No Autofac errors in logs
- [ ] HTTP 200 response from http://localhost:5000/
- [ ] Can access login page
- [ ] No infinite redirects

### Marketing Site Working:
- [ ] Build completes successfully
- [ ] Container running
- [ ] HTTP 200 response from http://localhost:3000/
- [ ] Pages render correctly

### Full Stack Working:
- [ ] All containers healthy
- [ ] Database connections working
- [ ] Redis caching functional
- [ ] Inter-service communication working
- [ ] Can register and login users

---

## 📞 Next Steps

1. **Choose fix approach** (mock services vs full implementation)
2. **Apply fixes** to code
3. **Rebuild containers**
4. **Test locally**
5. **Document configuration**
6. **Prepare for production** (if needed)

Would you like me to implement any of these fixes?
