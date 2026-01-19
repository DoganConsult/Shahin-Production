# Module Register (MG-01 Evidence)

**System:** GRC Platform (ABP Framework-based)
**Version:** 1.0
**Last Updated:** 2026-01-19
**Owner:** Platform Engineering
**Control Reference:** MG-01 (Module Inventory and Traceability)

---

## Summary

| Category | Total | Active | Disabled |
|----------|-------|--------|----------|
| ABP Framework Modules | 23 | 20 | 3 |
| Custom Business Modules | 8 | 8 | 0 |
| Integration Modules | 6 | 4 | 2 |
| AI Modules | 4 | 0 | 4 |
| **Total** | **41** | **32** | **9** |

---

## 1. ABP Framework Modules (23)

### 1.1 Core Modules (Active)

| ModuleName | Category | Default | Dev | Staging | Prod | Owner | Dependencies | Data Sensitivity | Enablement Criteria | Last Reviewed |
|------------|----------|---------|-----|---------|------|-------|--------------|------------------|--------------------|--------------|
| AbpAutofacModule | ABP Core | true | Active | Active | Active | Platform Eng | .NET DI | Low | N/A (required) | 2026-01-01 |
| AbpAspNetCoreMvcModule | ABP Core | true | Active | Active | Active | Platform Eng | ASP.NET Core | Moderate | N/A (required) | 2026-01-01 |
| AbpEntityFrameworkCoreModule | ABP Core | true | Active | Active | Active | Platform Eng | EF Core | High | N/A (required) | 2026-01-01 |
| AbpEntityFrameworkCorePostgreSqlModule | ABP Core | true | Active | Active | Active | Platform Eng | PostgreSQL | High | N/A (required) | 2026-01-01 |
| AbpCachingModule | ABP Core | true | Active | Active | Active | Platform Eng | Memory/Redis | Moderate | N/A (required) | 2026-01-01 |
| AbpValidationModule | ABP Core | true | Active | Active | Active | Platform Eng | FluentValidation | Low | N/A (required) | 2026-01-01 |
| AbpJsonSystemTextJsonModule | ABP Core | true | Active | Active | Active | Platform Eng | System.Text.Json | Low | N/A (required) | 2026-01-01 |
| AbpAutoMapperModule | ABP Core | true | Active | Active | Active | Platform Eng | AutoMapper | Low | N/A (required) | 2026-01-01 |
| AbpTimingModule | ABP Core | true | Active | Active | Active | Platform Eng | None | Low | N/A (required) | 2026-01-01 |
| AbpGuidsModule | ABP Core | true | Active | Active | Active | Platform Eng | None | Low | N/A (required) | 2026-01-01 |
| AbpSwashbuckleModule | ABP Core | true | Active | Active | Active | Platform Eng | Swashbuckle | Low | N/A (required) | 2026-01-01 |
| AbpSerilogModule | ABP Core | true | Active | Active | Active | Platform Eng | Serilog | Moderate | N/A (required) | 2026-01-01 |

### 1.2 Identity & Authorization Modules (Active)

| ModuleName | Category | Default | Dev | Staging | Prod | Owner | Dependencies | Data Sensitivity | Enablement Criteria | Last Reviewed |
|------------|----------|---------|-----|---------|------|-------|--------------|------------------|--------------------|--------------|
| AbpIdentityDomainModule | ABP Identity | true | Active | Active | Active | Security Eng | ASP.NET Identity | High | N/A (required) | 2026-01-01 |
| AbpIdentityApplicationModule | ABP Identity | true | Active | Active | Active | Security Eng | Identity Domain | High | N/A (required) | 2026-01-01 |
| AbpIdentityAspNetCoreModule | ABP Identity | true | Active | Active | Active | Security Eng | ASP.NET Core | High | N/A (required) | 2026-01-01 |
| AbpIdentityEntityFrameworkCoreModule | ABP Identity | true | Active | Active | Active | Security Eng | EF Core | High | N/A (required) | 2026-01-01 |
| AbpPermissionManagementDomainModule | ABP Permissions | true | Active | Active | Active | Security Eng | Identity | High | N/A (required) | 2026-01-01 |
| AbpPermissionManagementApplicationModule | ABP Permissions | true | Active | Active | Active | Security Eng | Perm Domain | High | N/A (required) | 2026-01-01 |
| AbpPermissionManagementIdentityModule | ABP Permissions | true | Active | Active | Active | Security Eng | Identity + Perm | High | N/A (required) | 2026-01-01 |
| AbpPermissionManagementEntityFrameworkCoreModule | ABP Permissions | true | Active | Active | Active | Security Eng | EF Core | High | N/A (required) | 2026-01-01 |

### 1.3 Multi-Tenancy & Settings Modules (Active)

| ModuleName | Category | Default | Dev | Staging | Prod | Owner | Dependencies | Data Sensitivity | Enablement Criteria | Last Reviewed |
|------------|----------|---------|-----|---------|------|-------|--------------|------------------|--------------------|--------------|
| AbpTenantManagementDomainModule | ABP Tenancy | true | Active | Active | Active | Platform Eng | Multi-tenancy | High | N/A (required) | 2026-01-01 |
| AbpTenantManagementApplicationModule | ABP Tenancy | true | Active | Active | Active | Platform Eng | Tenant Domain | High | N/A (required) | 2026-01-01 |
| AbpTenantManagementEntityFrameworkCoreModule | ABP Tenancy | true | Active | Active | Active | Platform Eng | EF Core | High | N/A (required) | 2026-01-01 |
| AbpSettingManagementDomainModule | ABP Settings | true | Active | Active | Active | Platform Eng | Settings Store | Moderate | N/A (required) | 2026-01-01 |
| AbpSettingManagementApplicationModule | ABP Settings | true | Active | Active | Active | Platform Eng | Settings Domain | Moderate | N/A (required) | 2026-01-01 |
| AbpSettingManagementEntityFrameworkCoreModule | ABP Settings | true | Active | Active | Active | Platform Eng | EF Core | Moderate | N/A (required) | 2026-01-01 |

### 1.4 Feature Management & Audit Modules (Active)

| ModuleName | Category | Default | Dev | Staging | Prod | Owner | Dependencies | Data Sensitivity | Enablement Criteria | Last Reviewed |
|------------|----------|---------|-----|---------|------|-------|--------------|------------------|--------------------|--------------|
| AbpFeatureManagementDomainModule | ABP Features | true | Active | Active | Active | Platform Eng | Feature Flags | Moderate | N/A (required) | 2026-01-01 |
| AbpFeatureManagementApplicationModule | ABP Features | true | Active | Active | Active | Platform Eng | Feature Domain | Moderate | N/A (required) | 2026-01-01 |
| AbpFeatureManagementEntityFrameworkCoreModule | ABP Features | true | Active | Active | Active | Platform Eng | EF Core | Moderate | N/A (required) | 2026-01-01 |
| AbpAuditLoggingDomainModule | ABP Audit | true | Active | Active | Active | Security Eng | Audit Store | High | N/A (required) | 2026-01-01 |
| AbpAuditLoggingEntityFrameworkCoreModule | ABP Audit | true | Active | Active | Active | Security Eng | EF Core | High | N/A (required) | 2026-01-01 |

### 1.5 Disabled ABP Modules

| ModuleName | Category | Default | Dev | Staging | Prod | Owner | Disable Reason | Exit Criteria | Ticket | Last Reviewed |
|------------|----------|---------|-----|---------|------|-------|----------------|---------------|--------|--------------|
| AbpBackgroundWorkersModule | ABP Background | true | Disabled | Disabled | Disabled | Platform Eng | OpenIddict initialization bug | ABP/OpenIddict fix upstream | PLATFORM-001 | 2026-01-01 |
| AbpBackgroundJobsModule | ABP Background | true | Disabled | Disabled | Disabled | Platform Eng | Using Hangfire directly | Remove if ABP workers enabled | PLATFORM-001 | 2026-01-01 |
| AbpBackgroundJobsHangfireModule | ABP Background | true | Disabled | Disabled | Disabled | Platform Eng | Direct Hangfire preferred | Re-evaluate with ABP workers | PLATFORM-001 | 2026-01-01 |
| AbpOpenIddictDomainModule | ABP OpenIddict | true | Disabled | Disabled | Disabled | Security Eng | Initialization conflict | OpenIddict fix | PLATFORM-001 | 2026-01-01 |
| AbpOpenIddictEntityFrameworkCoreModule | ABP OpenIddict | true | Disabled | Disabled | Disabled | Security Eng | Initialization conflict | OpenIddict fix | PLATFORM-001 | 2026-01-01 |
| AbpAccountModule | ABP Account | true | Disabled | Disabled | Disabled | Security Eng | Custom account impl used | N/A (intentional) | N/A | 2026-01-01 |
| AbpRabbitMQModule | ABP Messaging | false | Disabled | Disabled | Disabled | Platform Eng | Using Azure Service Bus | N/A (architecture decision) | N/A | 2026-01-01 |
| AbpEventBusDistributedModule | ABP Events | false | Disabled | Disabled | Disabled | Platform Eng | Local event bus only | Enable for distributed events | N/A | 2026-01-01 |
| AbpDaprModule | ABP Dapr | false | Disabled | Disabled | Disabled | Platform Eng | Dapr not in architecture | N/A (architecture decision) | N/A | 2026-01-01 |

---

## 2. Custom Business Modules (8)

| ModuleName | Category | Default | Dev | Staging | Prod | Owner | Dependencies | Data Sensitivity | Enablement Criteria | Last Reviewed |
|------------|----------|---------|-----|---------|------|-------|--------------|------------------|--------------------|--------------|
| Risk Management | Business | true | Active | Active | Active | Product (Risk) | Workflow, DB | High | N/A (core) | 2026-01-01 |
| Audit Management | Business | true | Active | Active | Active | Product (Audit) | Workflow, DB | High | N/A (core) | 2026-01-01 |
| Evidence Management | Business | true | Active | Active | Active | Product (Evidence) | Storage, Workflow | High | N/A (core) | 2026-01-01 |
| Policy Management | Business | true | Active | Active | Active | Product (Policy) | DB | High | N/A (core) | 2026-01-01 |
| Vendor Management | Business | true | Active | Active | Active | Product (Vendor) | DB | High | N/A (core) | 2026-01-01 |
| Workflow Engine | Business | true | Active | Active | Active | Platform Eng | Workflow Runtime | High | N/A (core) | 2026-01-01 |
| Compliance Calendar | Business | true | Active | Active | Active | Product (Compliance) | Scheduling, Jobs | Moderate | N/A (core) | 2026-01-01 |
| Action Plans | Business | true | Active | Active | Active | Product (GRC Ops) | Workflow, DB | High | N/A (core) | 2026-01-01 |

---

## 3. Integration Modules (6)

### 3.1 Active Integrations

| ModuleName | Category | Default | Dev | Staging | Prod | Owner | Dependencies | Data Sensitivity | Enablement Criteria | Last Reviewed |
|------------|----------|---------|-----|---------|------|-------|--------------|------------------|--------------------|--------------|
| Email Notifications | Integration | true | Active | Active | Active | DevOps | SMTP/SendGrid | Moderate | Provider configured, SPF/DKIM | 2026-01-01 |
| SignalR Real-time | Integration | true | Active | Active | Active | Platform Eng | SignalR Hub | Moderate | Scale tested, authZ validated | 2026-01-01 |
| Slack Notifications | Integration | true | Active | Active | Active | DevOps | Slack API | Low | Webhook configured | 2026-01-01 |
| Teams Notifications | Integration | true | Active | Active | Active | DevOps | Teams API | Low | Webhook configured | 2026-01-01 |

### 3.2 Disabled Integrations

| ModuleName | Category | Default | Dev | Staging | Prod | Owner | Dependencies | Data Sensitivity | Enablement Criteria | Ticket | Last Reviewed |
|------------|----------|---------|-----|---------|------|-------|--------------|------------------|--------------------|--------------| --------------|
| ClickHouse Analytics | Integration | false | Disabled | Disabled | Disabled | Data Eng | ClickHouse cluster | Moderate | Infra ready, data classification, egress controls | TBD | 2026-01-01 |
| Kafka Integration | Integration | false | Disabled | Disabled | Disabled | DevOps | Kafka cluster | Moderate | Infra ready, TLS, topic governance | TBD | 2026-01-01 |
| Camunda Workflows | Integration | false | Disabled | Disabled | Disabled | Platform Eng | Camunda | High | Infra ready, SoD mapping, audit | TBD | 2026-01-01 |
| Redis Caching | Integration | false | Disabled | Disabled | Disabled | DevOps | Redis | Moderate | Infra ready, encryption, key mgmt | TBD | 2026-01-01 |

---

## 4. AI Modules (4)

All AI modules are **disabled by default** per AI-01 control.

| ModuleName | Category | Default | Dev | Staging | Prod | Owner | Dependencies | Data Sensitivity | Enablement Criteria | Ticket | Last Reviewed |
|------------|----------|---------|-----|---------|------|-------|--------------|------------------|--------------------|--------------| --------------|
| AI Assistant (Copilot) | AI Feature | false | Disabled | Disabled | Disabled | Product + Security | Claude API | High | AI policy approved, tenant opt-in, logging enabled, data controls | TBD | 2026-01-01 |
| AI Classification | AI Feature | false | Disabled | Disabled | Disabled | Product + Security | Claude API | High | Same + evaluation metrics | TBD | 2026-01-01 |
| AI Risk Assessment | AI Feature | false | Disabled | Disabled | Disabled | Product + Security | Claude API | High | Same + human-in-loop | TBD | 2026-01-01 |
| AI Compliance Analysis | AI Feature | false | Disabled | Disabled | Disabled | Product + Security | Claude API | High | Same + evidence boundaries | TBD | 2026-01-01 |

---

## 5. Change Log

| Date | Module | Action | Ticket | Approved By | Deployed By |
|------|--------|--------|--------|-------------|-------------|
| 2025-06-01 | AbpBackgroundWorkersModule | Disabled | PLATFORM-001 | Security Lead | Platform Eng |
| 2025-06-01 | AbpOpenIddictDomainModule | Disabled | PLATFORM-001 | Security Lead | Platform Eng |
| 2025-08-15 | Slack Notifications | Enabled | INT-042 | Platform Lead | DevOps |
| 2025-09-01 | Teams Notifications | Enabled | INT-043 | Platform Lead | DevOps |

---

## 6. Quarterly Review Checklist

- [ ] All 41 modules accounted for
- [ ] No undocumented modules in code
- [ ] Disabled modules have valid tickets/reasons
- [ ] Owners assigned for all categories
- [ ] Enablement criteria defined for disabled modules
- [ ] Environment parity verified (MG-03)
- [ ] Change log up to date

**Last Full Review:** 2026-01-01
**Next Scheduled Review:** 2026-04-01
**Reviewer:** Platform Engineering Lead

---

## 7. Field Definitions

| Field | Description |
|-------|-------------|
| ModuleName | Canonical module name |
| Category | ABP Core/Identity/Business/Integration/AI |
| Default | Designed default state (true/false) |
| Dev/Staging/Prod | Current environment status |
| Owner | Accountable team/role |
| Dependencies | Required infrastructure/services |
| Data Sensitivity | Low/Moderate/High |
| Enablement Criteria | Conditions for enabling |
| Ticket | Associated change ticket |
| Last Reviewed | Last review date |

---

*End of Module Register*
