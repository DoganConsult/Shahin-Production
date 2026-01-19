# GitHub Environment Configuration Guide

## Overview

This guide explains how to configure GitHub Environments for the Shahin GRC production readiness pipeline with deployment protection rules.

## Required Environments

Configure these three environments in GitHub Settings > Environments:

### 1. Development Environment

**Name**: `development`

**Configuration**:
- Deployment branches: `develop` only
- Required reviewers: None
- Wait timer: 0 minutes
- Allow administrators to bypass: Yes

**Environment Secrets**:
```
DEV_DB_CONNECTION=Host=localhost;Database=GrcMvcDb_Dev;...
JWT_SECRET=dev-jwt-secret-min-32-characters
```

**Environment Variables**:
```
ASPNETCORE_ENVIRONMENT=Development
PORT=5002
```

---

### 2. Staging Environment

**Name**: `staging`

**Configuration**:
- Deployment branches: `staging`, `develop`
- Required reviewers: None (or 1 reviewer for extra safety)
- Wait timer: 0 minutes
- Allow administrators to bypass: Yes

**Required Gates** (verified by CI/CD):
- Gate A: Build & Release Quality ✅
- Gate B: Golden Flow Tests ✅
- Gate C: Audit & Security Controls ✅

**Environment Secrets**:
```
STAGING_DB_CONNECTION=Host=localhost;Database=GrcMvcDb_Staging;...
JWT_SECRET=staging-jwt-secret-min-32-characters
SERVER_HOST=your-server-ip
SERVER_USER=deploy
SSH_PRIVATE_KEY=-----BEGIN RSA PRIVATE KEY-----...
```

**Environment Variables**:
```
ASPNETCORE_ENVIRONMENT=Staging
PORT=5001
```

---

### 3. Production Environment

**Name**: `production`

**Configuration**:
- Deployment branches: `main` only
- **Required reviewers**: Add at least 1 reviewer (admin or lead developer)
- Wait timer: 5 minutes (recommended)
- Allow administrators to bypass: **NO** (for audit compliance)

**Required Gates** (ALL must pass - NON-NEGOTIABLE):
- Gate A: Build & Release Quality ✅
- Gate B: Golden Flow Tests ✅
- Gate C: Audit & Security Controls ✅
- Gate D: Operational Readiness ✅

**Environment Secrets**:
```
PROD_DB_CONNECTION=Host=localhost;Database=GrcMvcDb_Production;...
PROD_DB_PASSWORD=your-secure-production-password
DB_USER=postgres
JWT_SECRET=production-jwt-secret-min-64-characters-change-this!
SERVER_HOST=your-production-server-ip
SERVER_USER=deploy
SSH_PRIVATE_KEY=-----BEGIN RSA PRIVATE KEY-----...
```

**Environment Variables**:
```
ASPNETCORE_ENVIRONMENT=Production
PORT=5000
```

---

## Setting Up Environments in GitHub

### Step 1: Navigate to Environments

1. Go to your repository on GitHub
2. Click **Settings** tab
3. In the left sidebar, click **Environments**
4. Click **New environment**

### Step 2: Configure Protection Rules

For **production** environment:

1. **Required reviewers**: Check this box
   - Add at least 1 person (e.g., `@admin`, `@lead-developer`)
   - This enforces human approval before production deploys

2. **Wait timer**: Set to 5 minutes
   - Gives time to catch issues before deployment proceeds

3. **Deployment branches**: Select "Selected branches"
   - Add rule: `main`
   - This prevents accidental production deploys from other branches

4. **Allow administrators to bypass**: Leave UNCHECKED
   - For audit compliance, even admins should follow the process

### Step 3: Add Secrets

For each environment, add the required secrets:

1. Click on the environment name
2. Under "Environment secrets", click **Add secret**
3. Add each secret listed above

**Important**: Never commit secrets to the repository!

### Step 4: Add Variables

1. Under "Environment variables", click **Add variable**
2. Add each variable listed above

---

## Branch Protection Rules

Also configure branch protection for `main`:

1. Go to Settings > Branches
2. Click **Add branch protection rule**
3. Branch name pattern: `main`
4. Enable:
   - ✅ Require a pull request before merging
   - ✅ Require approvals (1+)
   - ✅ Require status checks to pass before merging
     - Add: `gate-a-build`, `gate-b-golden-flows`, `gate-c-audit`, `gate-d-operational`
   - ✅ Require branches to be up to date before merging
   - ✅ Do not allow bypassing the above settings

---

## Production Readiness Gates (Non-Negotiable)

### Gate A: Build & Release Quality
- Clean CI build (`dotnet build -c Release`)
- No compiler errors
- DB migrations apply cleanly
- Version file created

### Gate B: Golden Flow Tests
- B1: Self registration works
- B2: Trial signup works
- B3: Trial provision works
- B4: User invite works
- B5: Accept invite works
- B6: Role change works

### Gate C: Audit & Security Controls
- Required audit events are emitted
- Rate limiting is active
- Authentication audit logs exist

### Gate D: Operational Readiness
- TLS/SSL enabled
- Secrets not in code
- Backups configured
- Health checks work
- Error handling configured

---

## Versioning Scheme

The pipeline uses semantic versioning based on branch:

| Branch | Version Format | Example |
|--------|----------------|---------|
| `main` | `1.0.0` | Release version |
| `staging` | `1.0.0-beta.123` | Beta release |
| `develop` | `1.0.0-alpha.123` | Alpha release |
| `release/*` | `1.0.0-rc.123` | Release candidate |
| `hotfix/*` | `1.0.0-hotfix.123` | Hotfix |

---

## Required Repository Secrets (Global)

These secrets are needed at the repository level:

```
GITHUB_TOKEN        # Automatic (for container registry)
```

---

## Workflow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Push to Branch                           │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  📋 Calculate Version (Semantic Versioning)                 │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  🔨 GATE A: Build & Release Quality (NON-NEGOTIABLE)        │
│     • Clean build                                           │
│     • Version file                                          │
│     • Artifact hash                                         │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│  🔒 Security Scan                                           │
│     • Vulnerable packages                                   │
│     • Hardcoded secrets                                     │
└─────────────────────────────────────────────────────────────┘
                           │
            ┌──────────────┴──────────────┐
            │                             │
            ▼                             ▼
┌─────────────────────┐     ┌─────────────────────────────────┐
│  develop branch     │     │  staging/main branch            │
│  → Deploy to Dev    │     │                                 │
└─────────────────────┘     │  🌟 GATE B: Golden Flows        │
                            │  🛡️ GATE C: Audit Controls      │
                            │                                 │
                            └─────────────────────────────────┘
                                          │
                            ┌─────────────┴─────────────┐
                            │                           │
                            ▼                           ▼
                  ┌─────────────────┐     ┌─────────────────────┐
                  │ staging branch  │     │ main branch         │
                  │ → Staging       │     │                     │
                  │ (Gates A,B,C)   │     │ 🏭 GATE D: Ops      │
                  └─────────────────┘     │ → Production        │
                                          │ (ALL GATES A-D)     │
                                          │                     │
                                          │ ⏰ Required review  │
                                          │ ⏰ Wait timer       │
                                          │ 📦 DB Backup        │
                                          │ 🔄 Blue-Green       │
                                          └─────────────────────┘
```

---

## Troubleshooting

### "Environment not found" error
- Ensure the environment name in workflow matches exactly
- Check case sensitivity

### Deployment stuck waiting for approval
- Check if required reviewers are configured
- Reviewer must approve in GitHub Actions UI

### Gates failing
- Check the specific gate's output in Actions
- Review the quality-gates.json configuration

### Version not incrementing
- Ensure `Directory.Build.props` exists
- Check `github.run_number` is being used

---

**Last Updated**: 2026-01-19
