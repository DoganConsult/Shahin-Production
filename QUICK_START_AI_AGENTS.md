# 🚀 QUICK START: Get All AI Agents Running ASAP

**Status**: Most services are configured! Only need 4 critical values to launch.

---

## ✅ What's Already Configured

### **Azure Services (READY)**
- ✅ Microsoft Copilot Agent
- ✅ Azure Bot Service (Shahin-ai)
- ✅ Azure AI Search (searchwhere)
- ✅ Microsoft Graph API (Email Operations)
- ✅ SMTP OAuth2

### **AI Agents Waiting for Claude API Key**
12 agents ready to deploy once you add Claude API key:
1. **SHAHIN_AI** - Main orchestrator (landing page support)
2. **SUPPORT_AGENT** - Customer support (landing page chat)
3. **COMPLIANCE_AGENT** - Compliance analysis
4. **RISK_AGENT** - Risk assessment
5. **AUDIT_AGENT** - Audit analysis
6. **POLICY_AGENT** - Policy review
7. **ANALYTICS_AGENT** - Insights generation
8. **REPORT_AGENT** - Report generation
9. **DIAGNOSTIC_AGENT** - System diagnostics
10. **WORKFLOW_AGENT** - Workflow optimization
11. **EVIDENCE_AGENT** - Evidence collection
12. **EMAIL_AGENT** - Email classification

---

## 🎯 4 Critical Steps to Launch (5 Minutes)

### **Step 1: Get Claude API Key** (2 minutes)
```bash
# Go to: https://console.anthropic.com/settings/keys
# Click: "Create Key"
# Name: "Shahin AI Production"
# Copy the key (starts with: sk-ant-...)
```

### **Step 2: Generate JWT Secret** (30 seconds)
```bash
# Run this command:
openssl rand -base64 48

# Copy the output
```

### **Step 3: Set Admin Password** (30 seconds)
```bash
# Choose a strong password for the admin account
# Example: Admin@SecureP@ssw0rd2026!
```

### **Step 4: Configure Database** (2 minutes)
```bash
# If using existing database:
DB_HOST=your-postgres-host.com
DB_USER=grc_production_user
DB_PASSWORD=your-secure-db-password

# If using Docker/Local:
DB_HOST=localhost
DB_USER=postgres
DB_PASSWORD=postgres
```

---

## 📋 Fill in the Production Environment File

Open the file: `.env.production.complete`

Replace these 4 placeholders:

```bash
# 1. Claude AI (CRITICAL for 12 agents)
CLAUDE_API_KEY=sk-ant-YOUR_ACTUAL_CLAUDE_API_KEY_HERE

# 2. JWT Secret (generate with: openssl rand -base64 48)
JWT_SECRET=YOUR_GENERATED_JWT_SECRET_HERE

# 3. Admin Password
AdminUser__Password=Admin@SecureP@ssw0rd2026!

# 4. Database (if using PostgreSQL)
DB_HOST=your-db-host
DB_USER=grc_production_user
DB_PASSWORD=your-db-password
```

**Optional** (can do later):
```bash
# Azure Search Admin Key (get from Azure Portal → searchwhere → Keys)
AZURE_SEARCH_ADMIN_KEY=<from_azure_portal>

# Mainserver Client Secret (if needed)
MAINSERVER_CLIENT_SECRET=<from_azure_portal>
```

---

## 🚀 Deploy to Production

### **Option 1: Copy to Server**
```bash
# Secure the file
chmod 600 .env.production.complete

# Copy to production server
scp .env.production.complete root@YOUR_SERVER:/app/.env

# SSH into server
ssh root@YOUR_SERVER

# Move to correct location
cd /app
mv .env.production.complete .env
```

### **Option 2: Use Docker**
```bash
# Use docker-compose with env file
docker-compose --env-file .env.production.complete up -d
```

### **Option 3: Manual Deployment**
```bash
# Set environment variables
export $(cat .env.production.complete | xargs)

# Run the application
cd /app/src/GrcMvc
dotnet run --environment Production
```

---

## 🧪 Test All AI Services

### **1. Test Azure Bot Service**
```bash
# Test bot endpoint
curl -X POST https://YOUR_DOMAIN/api/bot/messages \
  -H "Authorization: Bearer YOUR_BOT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"text": "Hello"}'
```

### **2. Test Claude AI Agents**
```bash
# Test agent status
curl https://app.shahin-ai.com/api/agent/status

# Expected response:
{
  "available": true,
  "agents": [
    {"name": "SHAHIN_AI", "enabled": true},
    {"name": "SUPPORT_AGENT", "enabled": true},
    ...
  ]
}
```

### **3. Test Landing Page Support Agent**
```bash
# Test public chat (landing page)
curl -X POST https://shahin-ai.com/api/agent/chat/public \
  -H "Content-Type: application/json" \
  -d '{"message": "مرحبا، كيف يمكنني التسجيل؟", "context": "trial_registration"}'

# Expected: Arabic response about registration
```

### **4. Test Email Operations**
```bash
# Test Microsoft Graph email
curl https://app.shahin-ai.com/api/email/test \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### **5. Test Azure Search**
```bash
# Test search service
curl -X GET "https://searchwhere.search.windows.net/indexes?api-version=2023-11-01" \
  -H "api-key: YOUR_SEARCH_ADMIN_KEY"
```

---

## 🎯 Landing Page Agent Configuration

The **landing page chat** uses **SUPPORT_AGENT** powered by Claude:

### **Endpoint**
```
GET  /api/agent/chat/public?message=hello
POST /api/agent/chat/public
```

### **Landing Page Integration**
File: `shahin-ai-website/components/sections/OnboardingQuestionnaire.tsx`

```typescript
// Chat with AI support agent
const response = await fetch('/api/agent/chat/public', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    message: userMessage,
    context: 'trial_registration'
  })
});

const data = await response.json();
console.log(data.response); // AI response in Arabic
```

### **Fallback for No API Key**
If Claude API key is not configured, the system returns **static Arabic responses**:

Location: `src/GrcMvc/Controllers/Api/AgentController.cs:498-520`

```csharp
private string GetStaticTrialResponse(string message, string? context)
{
    // Provides helpful static responses in Arabic
    // Example: "التجربة مجانية لمدة 7 أيام!"
}
```

---

## 📊 All Configured Azure Resources

### **Resource Group: Shahin**
- Subscription: `Shahin-ai` (c2c3d463-2729-4592-a8b6-057dfd6344a8)
- Location: Global

### **App Registrations (4 Apps)**

| App Name | Client ID | Purpose | Secret Expires |
|----------|-----------|---------|----------------|
| **Shahin-ai Server** | 4e2575c6-e269-48eb-b055-ad730a2150a7 | Email (Graph API) | 7/9/2026 |
| **Agent (Copilot)** | 1bc8f3e9-f550-40e7-854d-9f60d7788423 | Copilot Studio | 7/9/2026 |
| **mainserver** | b542e4a7-ccf2-4b37-907d-7fb589990957 | Multi-org access | Not set |
| **Shahin-ai Bot** | b542e4a7-ccf2-4b37-907d-7fb589990957 | Bot Service | N/A |

### **Bot Service**
- Name: Shahin-ai
- Pricing: S1
- Keys: 2 secret keys configured ✅

### **AI Search**
- Service: searchwhere
- Deployment: searchservice-1768049371649
- Status: Deployed ✅

---

## 🔐 Security Checklist

Before deploying:

```bash
# 1. Secure the environment file
chmod 600 .env.production.complete

# 2. Add to .gitignore
echo ".env.production.complete" >> .gitignore
echo ".env" >> .gitignore

# 3. Never log secrets
# Already configured in code - secrets are not logged

# 4. Set calendar reminders for secret expiration
# - Microsoft Graph Secret: Expires 7/9/2026
# - Copilot Secret: Expires 7/9/2026
# - Action: Renew 1 month before expiration
```

---

## 📞 Support & Resources

### **Azure Portal**
- Tenant: https://portal.azure.com/#@c8847e8a-33a0-4b6c-8e01-2e0e6b4aaef5
- Resource Group: https://portal.azure.com/#@c8847e8a-33a0-4b6c-8e01-2e0e6b4aaef5/resource/subscriptions/c2c3d463-2729-4592-a8b6-057dfd6344a8/resourceGroups/Shahin

### **Claude Console**
- API Keys: https://console.anthropic.com/settings/keys
- Documentation: https://docs.anthropic.com/

### **Microsoft Copilot Studio**
- Portal: https://copilotstudio.microsoft.com/

### **File Locations**
```
.env.production.complete    ← USE THIS FILE (all configs)
.env.production.secure     ← Template version
.env.production.final      ← Intermediate version
```

---

## 🎉 Success Indicators

Once deployed successfully, you should see:

```bash
# ✅ All 12 AI agents available
GET /api/agent/status
{"available": true, "agents": 12}

# ✅ Landing page chat works
POST /api/agent/chat/public
{"success": true, "response": "مرحبا! ..."}

# ✅ Email operations functional
POST /api/email/send
{"success": true}

# ✅ Bot service responding
POST /api/bot/messages
{"success": true}
```

---

## 🚨 Troubleshooting

### **Problem: "Claude API not available"**
```bash
# Check if API key is set
echo $CLAUDE_API_KEY

# Should output: sk-ant-...
# If empty, API key not loaded
```

**Solution:**
```bash
# Verify .env file exists and has correct value
cat .env | grep CLAUDE_API_KEY

# Restart application to load new env vars
systemctl restart grcmvc
```

### **Problem: "Microsoft Graph authentication failed"**
```bash
# Check client secret expiration
# Secret expires: 7/9/2026
```

**Solution:**
```bash
# If expired, renew in Azure Portal:
# 1. Go to: App Registrations → Shahin-ai Server
# 2. Click: Certificates & secrets
# 3. Click: New client secret
# 4. Update MSGRAPH_CLIENT_SECRET in .env
```

### **Problem: "Database connection failed"**
```bash
# Check database is running
psql -h $DB_HOST -U $DB_USER -d $DB_NAME
```

**Solution:**
```bash
# Verify connection string
echo $GRCMVC_DB_CONNECTION

# Test connection
dotnet ef database update
```

---

## ✨ Next Steps After Launch

1. **Monitor AI agent usage** at `/api/agent/team`
2. **Set up monitoring** for API rate limits (Claude API)
3. **Configure Google AI** (optional, for redundancy)
4. **Set up Azure OpenAI** (optional)
5. **Enable Kafka** (optional, for event streaming)
6. **Deploy Multi-Agent Engine** from GitHub repo

---

**Total Time to Launch: ~5 minutes** (after filling in 4 values)

**File to Edit**: `.env.production.complete`

**Values Needed**:
1. Claude API Key (from console.anthropic.com)
2. JWT Secret (generate with: openssl rand -base64 48)
3. Admin Password (choose a strong password)
4. Database credentials (host, user, password)

🚀 **Ready to launch when you are!**
