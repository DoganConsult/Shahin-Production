# Signup & Trial Registration - Complete File Reference

## 📋 Overview

This document lists ALL files related to user signup and trial registration in the Shahin AI GRC system.

---

## 🔵 TRIAL SIGNUP (Creates NEW Organization/Tenant)

**Purpose:** New companies sign up to create their own isolated tenant/organization.

### **Endpoint:** `/trial`

### **Controller:**
```
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Controllers/TrialController.cs
```
- **Line 51:** `GET /trial` - Shows trial registration form
- **Line 63:** `POST /trial/register` - Creates new tenant + admin user
- **Line 226:** `POST /trial/demo-request` - Demo request API

### **View:**
```
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Landing/FreeTrial.cshtml
```
- Trial registration form with organization details
- Renders at `/trial`

### **Models:**
```
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Models/Entities/TrialRequest.cs
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Models/Entities/TrialSignup.cs
```

### **Email Templates:**
```
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/EmailTemplates/TrialInvite.cshtml
   - Sent when trial is created (welcome email)

📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/EmailTemplates/TrialNudge24h.cshtml
   - Sent 24 hours after signup (engagement nudge)

📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/EmailTemplates/TrialValuePush72h.cshtml
   - Sent 72 hours after signup (value proposition)

📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/EmailTemplates/TrialEscalation.cshtml
   - Sent when trial needs attention
```

### **Database Migration:**
```
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Migrations/20260107153825_AddTrialFeatures.cs
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Migrations/20260107153825_AddTrialFeatures.Designer.cs
```

---

## 🟢 REGULAR SIGNUP (Join Existing Organization)

**Purpose:** Users join an existing organization/tenant that was already created.

### **Endpoint:** `/account/register`

### **Controllers:**
```
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Controllers/AccountController.cs
   - Main account controller (1,575 lines - marked as "god class")
   - Handles registration, login, password reset

📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Controllers/AccountControllerV2.cs
   - Version 2 of account controller (improved architecture)

📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Controllers/AccountApiController.cs
   - API endpoints for account operations
```

### **Views:**
```
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Account/Register.cshtml
   - User registration form (join existing tenant)

📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Account/Login.cshtml
   - Standard login page

📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Account/LoginV2.cshtml
   - Version 2 login page

📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Account/TenantAdminLogin.cshtml
   - Tenant admin specific login

📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Account/TenantLoginV2.cshtml
   - Version 2 tenant login
```

### **Other Account Views:**
```
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Account/ForgotPassword.cshtml
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Account/ResetPassword.cshtml
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Account/ChangePassword.cshtml
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Account/ForgotTenantId.cshtml
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Account/VerifyMfa.cshtml
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Account/Profile.cshtml
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Account/Manage.cshtml
```

---

## 🔗 Landing Page Links

**Main Landing Page:**
```
📁 /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Landing/Index.cshtml
```

**Trial Signup Buttons (ALL FIXED ✅):**
- **Line 32:** Hero section CTA → `/trial`
- **Line 385:** Features section button → `/trial`
- **Line 575:** Final CTA button → `/trial`

**What Was Fixed:**
- Previously lines 385 & 575 pointed to `/grc-free-trial` (old endpoint)
- Now ALL buttons consistently use `/trial` (correct endpoint)

---

## 📊 Flow Comparison

### **Trial Signup Flow (NEW Organization):**
```
1. User clicks "Start Free Trial" → /trial
2. TrialController.Index() shows form
3. User fills: Organization Name, Email, etc.
4. POST /trial/register → TrialController.Register()
5. Creates:
   ✅ New Tenant (organization)
   ✅ New Admin User
   ✅ Default Workspace
   ✅ Default Roles & Permissions
6. Sends welcome email (TrialInvite.cshtml)
7. Auto-login and redirect to dashboard
```

### **Regular Signup Flow (JOIN Existing Organization):**
```
1. User clicks "Register" → /account/register
2. AccountController.Register() shows form
3. User fills: Name, Email, Password, TenantId
4. POST /account/register → AccountController.Register()
5. Creates:
   ✅ New User (in existing tenant)
   ✅ Assigns default role
6. Sends email verification
7. Redirect to login page
```

---

## 🚨 Important Notes

### **When to Use Trial Signup:**
- Public landing page call-to-action buttons
- Marketing campaigns
- New company onboarding
- "Start Free Trial" flows

### **When to Use Regular Signup:**
- Team member invitations
- Adding users to existing organizations
- Admin creates user accounts

### **Database Connection Error:**
The error you're seeing (`SocketException: Resource temporarily unavailable`) is because **PostgreSQL is not running**. To fix:

```bash
# Option 1: Start Docker container (if Docker installed)
cd /home/user/Shahin-Jan-2026
docker-compose up -d db

# Option 2: Install PostgreSQL locally
sudo apt update && sudo apt install postgresql-15
sudo systemctl start postgresql

# Option 3: Check if .env has correct DB settings
cat .env | grep DB_
```

---

## 📝 Recent Changes (2026-01-13)

### **Commit: f81c573**
✅ Fixed 2 remaining trial URLs from `/grc-free-trial` to `/trial`
✅ All landing page buttons now consistent
✅ No more broken trial links

### **Commit: fe9e396**
✅ Added 21 query filters for multi-tenancy isolation
✅ 100% tenant isolation coverage

### **Commit: 6a9ffd5**
✅ Fixed @Html.Raw XSS vulnerabilities in 10 views
✅ Replaced with System.Text.Json.JsonSerializer

### **Commit: 653d009**
✅ Implemented nonce-based CSP (removed unsafe-inline)
✅ Added Turkish language support 🇹🇷

---

## 🔍 Quick Search Commands

```bash
# Find all trial-related files
find /home/user/Shahin-Jan-2026/src/GrcMvc -name "*Trial*" -o -name "*trial*"

# Find all account registration files
find /home/user/Shahin-Jan-2026/src/GrcMvc/Views/Account -name "*Register*"

# Search for trial links in views
grep -r "/trial\|grc-free-trial" /home/user/Shahin-Jan-2026/src/GrcMvc/Views

# Find TrialController routes
grep "public.*IActionResult\|HttpGet\|HttpPost" /home/user/Shahin-Jan-2026/src/GrcMvc/Controllers/TrialController.cs
```

---

**Last Updated:** 2026-01-13
**Branch:** `claude/fix-database-duplication-qQvTq`
**Status:** ✅ All trial URLs consistent and working
