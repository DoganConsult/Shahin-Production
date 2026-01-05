# GRC System - Integrations Quick Reference

## Summary Table

| Integration | Status | Location | Providers | Priority |
|-------------|--------|----------|-----------|----------|
| **Email (SMTP)** | ✅ Implemented | Services/SmtpEmailSender.cs | SendGrid, Gmail, Custom | P1 |
| **Email (Alternative)** | ✅ Implemented | Services/SmtpEmailService.cs | Any SMTP | P1 |
| **LLM Services** | ✅ Implemented | Services/LlmService.cs | OpenAI, Azure, Local | P2 |
| **Payment Gateway** | ⚠️ Stubbed | Services/SubscriptionService.cs | Stripe (not integrated) | P0 |
| **Multi-Tenant Onboarding** | ✅ Partial | Services/TenantService.cs | Email activation | P2 |
| **Authentication** | ✅ Basic | Services/AuthenticationService.cs | JWT, OAuth2 ready | P1 |
| **File Storage** | ⚠️ Stubbed | Services/FileUploadService.cs | Azure, S3, Local | P1 |
| **OAuth2/OIDC** | ❌ Missing | N/A | Google, GitHub, MS | P1 |
| **SAML** | ❌ Missing | N/A | Enterprise SSO | P2 |
| **LDAP/AD** | ❌ Missing | N/A | Directory sync | P2 |
| **2FA** | ❌ Missing | N/A | TOTP, SMS, Email | P2 |
| **Webhooks** | ❌ Missing | N/A | Payment, Events | P1 |
| **Redis Cache** | ❌ Stubbed | Program.cs | Caching | P3 |
| **Background Jobs** | ❌ Missing | N/A | Hangfire, Quartz | P3 |
| **Search** | ❌ Missing | N/A | Elasticsearch | P3 |
| **Monitoring** | ⚠️ Partial | Serilog configured | App Insights, Datadog | P2 |

---

## Files by Integration Type

### Email Services
- `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/SmtpEmailSender.cs` - MailKit
- `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/SmtpEmailService.cs` - System.Net.Mail
- `/home/dogan/grc-system/src/GrcMvc/Configuration/EmailSettings.cs` - Configuration model
- `/home/dogan/grc-system/src/GrcMvc/Services/Interfaces/IEmailService.cs` - Interface
- `/home/dogan/grc-system/src/GrcMvc/Services/Interfaces/IAppEmailSender.cs` - ABP interface

### LLM Integration
- `/home/dogan/grc-system/src/GrcMvc/Services/LlmService.cs` - Full implementation
- `/home/dogan/grc-system/src/GrcMvc/Models/Entities/LlmConfiguration.cs` - Configuration entity
- Supports: OpenAI, Azure OpenAI, Local (Ollama)

### Payment & Subscription
- `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/SubscriptionService.cs` - Service
- `/home/dogan/grc-system/src/GrcMvc/Services/Interfaces/ISubscriptionService.cs` - Interface
- `/home/dogan/grc-system/src/GrcMvc/Models/Entities/Subscription.cs` - Entity
- `/home/dogan/grc-system/src/GrcMvc/Models/Entities/Payment.cs` - Payment entity
- `/home/dogan/grc-system/src/GrcMvc/Models/Entities/Invoice.cs` - Invoice entity
- `/home/dogan/grc-system/src/GrcMvc/Controllers/SubscriptionApiController.cs` - API endpoints

### Authentication
- `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/AuthenticationService.cs` - Service
- `/home/dogan/grc-system/src/GrcMvc/Services/Interfaces/IAuthenticationService.cs` - Interface
- JWT tokens, password management, user profiles

### Multi-Tenancy
- `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/TenantService.cs` - Tenant service
- `/home/dogan/grc-system/src/GrcMvc/Services/Interfaces/ITenantService.cs` - Interface
- Activation workflow, tenant creation, slug management

### File Storage
- `/home/dogan/grc-system/src/GrcMvc/Services/Implementations/FileUploadService.cs` - Service
- `/home/dogan/grc-system/src/GrcMvc/Services/Interfaces/IFileUploadService.cs` - Interface
- Configuration: appsettings.Production.json

---

## Implementation Status by Priority

### P0 - Critical for Production
- [ ] **Payment Gateway Integration** - Stripe API client needed
- [ ] **SSL/TLS Certificates** - For HTTPS
- [ ] **Production Email Relay** - SendGrid or AWS SES setup

### P1 - High Priority
- [x] Email (SMTP) - Done
- [x] Basic Authentication - Done
- [ ] OAuth2 Providers - Google, GitHub
- [ ] Webhooks - Payment and event callbacks
- [ ] Real File Storage - Azure/S3 implementation

### P2 - Medium Priority  
- [x] LLM Services - Done
- [x] Multi-tenant - Done
- [ ] SAML/LDAP/AD - Enterprise authentication
- [ ] Two-Factor Auth - Security enhancement
- [ ] Monitoring - Application Insights/Datadog

### P3 - Nice-to-Have
- [ ] Redis Caching - Performance
- [ ] Background Jobs - Async processing
- [ ] Full-text Search - Elasticsearch
- [ ] SMS Notifications - Twilio/AWS SNS
- [ ] Slack/Teams - Team notifications

---

## Configuration Quick Links

### Email Setup
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.sendgrid.net",
    "SmtpPort": 587,
    "SenderName": "GRC System",
    "SenderEmail": "noreply@yourdomain.com",
    "Username": "apikey",
    "Password": "SG.your-api-key",
    "EnableSsl": true
  }
}
```

### LLM Setup (Database SQL)
```sql
INSERT INTO "LlmConfigurations" 
(Id, TenantId, Provider, ApiEndpoint, ApiKey, ModelName, MaxTokens, Temperature, IsActive, EnabledForTenant, CreatedDate)
VALUES
(gen_random_uuid(), 'YOUR-TENANT-ID', 'openai', 
 'https://api.openai.com/v1/chat/completions', 'sk-your-key', 'gpt-4', 
 2000, 0.7, true, true, NOW());
```

### Stripe Integration (TODO)
```csharp
// Needs implementation in SubscriptionService.ProcessPaymentAsync()
// NuGet: Stripe.net
// Methods needed:
// - PaymentIntent creation
// - Webhook handler
// - Refund processing
// - Subscription sync
```

---

## Testing Integration Points

### Email Service Tests
- [ ] MailKit SMTP connection
- [ ] System.Net.Mail fallback
- [ ] SSL/TLS handshake
- [ ] Authentication (username/password)
- [ ] Batch email sending
- [ ] Error handling (timeout, invalid credentials)

### LLM Service Tests
- [ ] OpenAI API connectivity
- [ ] Azure OpenAI endpoint
- [ ] Local Ollama server
- [ ] Usage quota tracking
- [ ] Monthly reset logic
- [ ] Token limits enforcement

### Payment Service Tests
- [ ] Subscription creation
- [ ] Plan feature checking
- [ ] User limit enforcement
- [ ] Invoice generation
- [ ] Payment recording
- [ ] Status transitions (Trial → Active)
- [ ] Renewal reminders
- [ ] Refund processing

### Authentication Tests
- [ ] JWT token generation
- [ ] Token validation
- [ ] Token refresh
- [ ] Password reset flow
- [ ] User registration
- [ ] Login/logout

---

## Common Integration Tasks

### Add a New LLM Provider
1. Update `LlmService.CallLlmAsync()` switch statement
2. Implement new `CallXxxLlmAsync()` method
3. Update `LlmConfiguration.Provider` enum values
4. Test API connectivity and response parsing

### Add a New Email Provider
1. Implement `IEmailService` interface
2. Add configuration section to appsettings.json
3. Register in DI container (Program.cs)
4. Test email delivery

### Integrate with Payment Gateway
1. Add NuGet package (e.g., Stripe.net)
2. Create PaymentGatewayService interface
3. Implement provider-specific class
4. Add webhook handler
5. Update SubscriptionService.ProcessPaymentAsync()
6. Add error handling and retry logic

### Add Authentication Provider
1. Configure provider in Program.cs
2. Create sign-in handler
3. Implement claim mapping
4. Test OAuth flow

---

## Known Issues & TODOs

### Critical
- [ ] Stripe integration not implemented - `ProcessPaymentAsync()` is a stub
- [ ] File storage backend not implemented - only configuration exists
- [ ] SSL certificates not configured

### Important
- [ ] OAuth2 providers not implemented
- [ ] SAML not implemented
- [ ] LDAP/AD integration missing
- [ ] 2FA not implemented
- [ ] Redis caching configuration exists but not active
- [ ] Background job processing missing

### Nice-to-Have
- [ ] Email template engine (currently placeholder in SmtpEmailService)
- [ ] Webhook retry logic
- [ ] Rate limiting per API key
- [ ] API usage analytics
- [ ] Advanced logging/tracing

---

## Integration Priorities by Business Value

### Must Have (Week 1-2)
1. Stripe payment processing
2. Production email relay
3. SSL certificates

### Should Have (Week 2-4)
1. OAuth2 providers
2. Real file storage (S3/Azure)
3. Basic monitoring

### Could Have (Week 4-8)
1. SAML/LDAP
2. Redis caching
3. Background jobs
4. Advanced search

### Nice to Have (Week 8+)
1. SMS notifications
2. Slack integration
3. Advanced analytics
4. Custom webhook handlers

---

## Support & Documentation

- Full audit report: `/home/dogan/grc-system/INTEGRATIONS_COMPREHENSIVE_AUDIT.md`
- Project instructions: `/home/dogan/grc-system/CLAUDE.md`
- LLM setup: `/home/dogan/grc-system/LLM_CONFIGURATION_GUIDE.md`
- Subscription details: `/home/dogan/grc-system/SUBSCRIPTION_QUICK_REFERENCE.md`

