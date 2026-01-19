# Copilot Instructions Update Summary

**Updated**: January 19, 2026  
**File**: `.github/copilot-instructions.md` (445 lines)

## What Changed

The Copilot instructions have been **completely restructured** to be **agent-first** and **architecture-aware**, focusing on:

1. **Big Picture Context** (why the architecture exists)
2. **Multi-tenancy constraints** (the hardest problem to solve correctly)
3. **AI Agent integration** (12 Claude Sonnet agents, when to use them)
4. **Workflow engine patterns** (state machines for compliance)
5. **Practical code conventions** (copy-paste ready examples)
6. **Integration points** (external services: Claude, Graph, SMTP, Kafka)
7. **Step-by-step feature additions** (how to add a new entity safely)
8. **Security guardrails** (tenant isolation validation)

## Key Sections

### 1. **Big Picture** (Why This Architecture)
- Explains the decision to use a **single MVC monolith** (not microservices) 
- Data flow: User → Tenant context → Services → EF Core with tenant filters → Agents → Events → Dashboards
- Why: 230+ DbSets, 100+ controllers, 12 agent types require operational simplicity

### 2. **Multi-Tenancy: The Core Constraint** (CRITICAL)
- Every query must be tenant-scoped
- How it works: `BaseEntity` inheritance + global query filters in `GrcDbContext.OnModelCreating()`
- Code example: How to add a new tenant-safe entity
- **Anti-patterns**: Raw SQL without tenant filter, no tenant validation

### 3. **12 AI Agents: When and How**
- Table of all 8 agent types with their services and purposes
- Correct pattern: Always pass `TenantId`, always validate tenant context
- Key files to reference: `ClaudeAgentService.cs`, `LlmService.cs`, `DiagnosticAgentService.cs`

### 4. **Workflow Engine** (State Machines)
- 10 workflow types for compliance processes
- Pattern: `WorkflowInstance` + `WorkflowTask` entities
- Auto-escalation via Hangfire jobs

### 5. **Code Conventions** (Copy-Paste Ready)
| What | Pattern | Example |
|------|---------|---------|
| Entities | Singular, PascalCase, inherit `BaseEntity` | `Risk`, `Control` |
| Controllers | `{Entity}Controller` or `{Entity}ApiController` | `RiskController.cs` |
| DTOs | `{Entity}ReadDto`, `{Entity}CreateDto`, `{Entity}UpdateDto` | `RiskReadDto` |
| Services | `I{Entity}Service` interface + implementation | `IRiskService`, `RiskService` |
| Validators | `{Entity}Validators.cs` | `RiskCreateValidator` |
| Migrations | `Add{Entity}` or `{ChangeDescription}` | `AddRiskTable` |

### 6. **Integration Points** (External Services)
| Service | Purpose | Config Location |
|---------|---------|-----------------|
| Claude Sonnet 4.5 | AI agents | `appsettings.json` → `ClaudeAgents:ApiKey` |
| Microsoft Graph | OAuth email, teams | `Graph:*` |
| SMTP | Email delivery | `Email:Smtp:*` or stub mode |
| Camunda BPM | Complex workflows | Docker service |
| Kafka + RabbitMQ | Event streaming | `MassTransit:*` |
| PostgreSQL | Primary data store | Migrations auto-run on startup |
| Redis | Optional caching | `Redis:ConnectionString` |

### 7. **Step-by-Step: Adding a New Feature** (Complete Flow)
1. Entity + Migration (create DbSet)
2. DTOs + Service interface (contract first)
3. Controller + Validation (MVC + API)
4. Register in DI (Program.cs)

### 8. **Security & Multi-Tenancy Guardrails**
- Always validate tenant context before querying
- Background jobs must store `TenantId` explicitly (ambient context won't persist)
- Raw SQL queries must include `WHERE TenantId = {currentTenantId}`

### 9. **Key Files & Ratios**
| File | Lines | Purpose |
|------|-------|---------|
| `Program.cs` | 1,749 | DI, middleware, Hangfire, MassTransit |
| `GrcDbContext.cs` | 1,697 | 230+ DbSets, query filters |
| `ClaudeAgentService.cs` | 498 | Unified AI agent handler |
| `DashboardService.cs` | 31KB | Event-driven projections |
| `AdminCatalogService.cs` | 36KB | Control/framework catalog |
| `OnboardingWizard.cs` | 25KB | Multi-tenant onboarding data |

Test coverage: 34 test files for 833 source files (~4% ratio)

---

## Why This Matters for AI Agents

AI agents (like you!) need to understand:

1. **Architectural constraints**: Multi-tenancy is NOT optional—it's enforced everywhere
2. **Agent patterns**: There are 12 specialized Claude agents; know which one to use for each task
3. **Workflow state machines**: Compliance workflows require explicit state management, not ad-hoc logic
4. **Integration dependencies**: External services (Claude, Graph, SMTP) have specific config requirements
5. **Code conventions**: Consistency is a safety feature in a 833-file codebase
6. **Step-by-step playbooks**: Adding features safely means following a precise sequence

## Questions for Refinement

Based on this update, please provide feedback on:

1. **Architecture clarity**: Is the "Big Picture" section clear enough about why this is a monolith vs microservices?
2. **Multi-tenancy guardrails**: Are the security patterns obvious enough to prevent cross-tenant leaks?
3. **Agent integration**: Do the examples make it clear when to use `ClaudeAgentService` vs `LlmService` vs specialized agents?
4. **Workflow patterns**: Is the state machine pattern clear enough for background job scenarios?
5. **Feature addition flow**: Is the 4-step process comprehensive for a typical feature add?
6. **Integration config**: Should we document more detail on Hangfire, MassTransit, or Camunda setup?
7. **Missing sections**: What critical patterns are still not documented?

---

**Status**: ✅ Production Ready  
**Last Updated**: January 19, 2026  
**Maintained By**: AI Agent Instructions (Copilot/Claude/Cursor)
