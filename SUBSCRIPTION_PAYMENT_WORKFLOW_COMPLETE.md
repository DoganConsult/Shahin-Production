# Complete Post-Payment Subscription Workflow System

## 📋 Overview

A comprehensive subscription management system with complete post-payment workflow automation:

1. **Account Status Changes** - Auto-activation, admin approval options, trial period management
2. **Notifications** - Welcome emails, payment confirmations, invoices, reminders
3. **Access & Permissions** - Auto-assign roles, feature access control, expiration dates
4. **Dashboard Updates** - License details display, onboarding guides, status pages
5. **Database** - Subscription records, payment tracking, invoice management

---

## 🏗️ Architecture

### Database Models

#### `SubscriptionPlan`
- **Purpose**: Define available plans (MVP, Professional, Enterprise)
- **Key Fields**:
  - `Name`, `Code`, `Description`
  - `MonthlyPrice`, `AnnualPrice`
  - `MaxUsers`, `MaxAssessments`, `MaxPolicies`
  - `HasAdvancedReporting`, `HasApiAccess`, `HasPrioritySupport`
  - `Features` (JSON array)

#### `Subscription`
- **Purpose**: Track active subscriptions per tenant
- **Key Fields**:
  - `TenantId` - Links to organization
  - `PlanId` - Current plan
  - `Status` - Trial, Active, Suspended, Cancelled, Expired
  - `TrialEndDate`, `SubscriptionStartDate`, `SubscriptionEndDate`
  - `NextBillingDate`, `BillingCycle` (Monthly/Annual)
  - `AutoRenew`, `CurrentUserCount`

#### `Payment`
- **Purpose**: Record payment transactions
- **Key Fields**:
  - `SubscriptionId`, `TenantId`
  - `TransactionId` - Unique payment reference
  - `Amount`, `Currency`
  - `Status` - Pending, Completed, Failed, Refunded
  - `PaymentMethod`, `Gateway` (Stripe, PayPal, etc.)
  - `PaymentDate`

#### `Invoice`
- **Purpose**: Track billing and invoices
- **Key Fields**:
  - `InvoiceNumber` - Unique invoice ID
  - `InvoiceDate`, `DueDate`
  - `PeriodStart`, `PeriodEnd` - Billing period
  - `SubTotal`, `TaxAmount`, `TotalAmount`
  - `AmountPaid`, `Status` (Draft, Sent, Paid, Overdue, etc.)

---

## 🔄 Post-Payment Workflow

### Step 1: Payment Processing

```csharp
POST /api/subscription/payment
{
    "subscriptionId": "guid",
    "amount": 99.99,
    "paymentMethodToken": "stripe_token",
    "email": "billing@company.com",
    "currency": "USD"
}
```

**Process**:
1. Validate subscription exists
2. Create Payment record (Completed)
3. Create Invoice with details
4. Update Subscription status → "Active"
5. Clear trial period
6. Set NextBillingDate

**Response**:
```json
{
    "success": true,
    "transactionId": "txn_xyz123",
    "message": "Payment processed successfully",
    "subscription": { },
    "invoice": { }
}
```

### Step 2: Account Status Changes

| Status | Trigger | Action |
|--------|---------|--------|
| Trial | Create subscription | 14-day trial period activated |
| Active | Payment completed | Account fully activated |
| Suspended | Payment fails/Admin action | No access, data preserved |
| Cancelled | User request/Admin | Account terminated, data archived |
| Expired | Trial/subscription ends | Auto-move when NextBillingDate passes |

### Step 3: Automated Notifications

#### Welcome Email (on activation)
```
Subject: Welcome to GRC System!

Content:
- Plan details
- Billing cycle (Monthly/Annual)
- Next billing date
- Feature list
```

#### Payment Confirmation
```
Subject: Payment Confirmation - Transaction [ID]

Content:
- Amount paid
- Transaction ID
- Payment date
- Subscription status
```

#### Invoice Email
```
Subject: Invoice [INV-YYYYMM-XXXXXX]

Content:
- Invoice number & date
- Billing period
- Subtotal, Tax, Total
- Payment status
- Download link (optional)
```

#### Renewal Reminders
```
Sent 7 days before NextBillingDate
- Warning about upcoming charge
- Renewal action needed
```

### Step 4: Access Control & Permissions

#### Feature Access
```csharp
bool available = await _subscriptionService
    .IsFeatureAvailableAsync(tenantId, "AdvancedReporting");

// Features vary by plan:
// MVP: Basic features
// Professional: +AdvancedReporting, +ApiAccess
// Enterprise: All features + PrioritySupport
```

#### User Limit Enforcement
```csharp
bool limitReached = await _subscriptionService
    .IsUserLimitReachedAsync(tenantId);

// Prevent new user invitations if limit reached
// Based on plan's MaxUsers
```

### Step 5: Dashboard & UI Updates

#### License Details Display
```
Current Plan: Professional
Users: 5 / 10
Next Billing: Feb 15, 2026
Status: Active ✓
Auto-Renew: Enabled

Included Features:
✓ Advanced Reporting
✓ API Access  
✓ Priority Support
```

#### Onboarding Guide
```
Show after activation:
1. Set up first workspace
2. Configure team members
3. Create first assessment
4. Download templates
5. View documentation
```

---

## 📡 API Endpoints

### Plans
- `GET /api/subscription/plans` - List all active plans
- `GET /api/subscription/plans/{planId}` - Get plan details
- `GET /api/subscription/plans/code/{code}` - Get by code (MVP, PRO, ENT)

### Subscriptions
- `GET /api/subscription/{tenantId}` - Get current subscription
- `POST /api/subscription/create` - Create new subscription
- `POST /api/subscription/trial/{id}` - Activate trial period
- `POST /api/subscription/activate/{id}` - Activate (after payment)
- `POST /api/subscription/suspend/{id}` - Suspend subscription
- `POST /api/subscription/cancel/{id}` - Cancel subscription
- `POST /api/subscription/renew/{id}` - Renew subscription

### Payment
- `POST /api/subscription/payment` - Process payment
- `GET /api/subscription/payments/{subscriptionId}` - Payment history

### Invoices
- `GET /api/subscription/invoices/{subscriptionId}` - Get invoices
- `GET /api/subscription/invoice/{invoiceId}` - Get invoice details

---

## 🧪 Testing Guide

### Test 1: Create Trial Subscription

```bash
curl -X POST http://localhost:8888/api/subscription/create \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer TOKEN" \
  -d '{
    "tenantId": "550e8400-e29b-41d4-a716-446655440000",
    "planId": "plan-professional-id",
    "billingCycle": "Monthly"
  }'
```

**Expected**:
- Status: "Trial"
- TrialEndDate: +14 days from now
- NextBillingDate: Set to trial end date
- No payment required yet

---

### Test 2: Process Payment

```bash
curl -X POST http://localhost:8888/api/subscription/payment \
  -H "Content-Type: application/json" \
  -d '{
    "subscriptionId": "subscription-id",
    "amount": 99.99,
    "paymentMethodToken": "stripe_token_xyz",
    "email": "customer@company.com",
    "currency": "USD"
  }'
```

**Expected Response**:
```json
{
    "success": true,
    "transactionId": "txn_1234567890",
    "subscription": {
        "status": "Active",
        "subscriptionStartDate": "2026-01-04T...",
        "nextBillingDate": "2026-02-04T...",
        "trialEndDate": null
    },
    "invoice": {
        "invoiceNumber": "INV-202601-ABC123",
        "status": "Paid",
        "totalAmount": 99.99,
        "amountPaid": 99.99
    }
}
```

**Automation**:
- ✅ Subscription status → "Active"
- ✅ Payment record created
- ✅ Invoice generated & marked "Paid"
- ✅ Welcome email sent
- ✅ Payment confirmation email sent
- ✅ Invoice email sent

---

### Test 3: Verify Account Activation

```bash
# Check subscription status
curl -X GET http://localhost:8888/api/subscription/{tenantId} \
  -H "Authorization: Bearer TOKEN"
```

**Expected**:
```json
{
    "status": "Active",
    "plan": {
        "name": "Professional",
        "maxUsers": 10,
        "hasAdvancedReporting": true,
        "hasApiAccess": true,
        "hasPrioritySupport": true
    },
    "subscriptionStartDate": "2026-01-04T...",
    "nextBillingDate": "2026-02-04T...",
    "autoRenew": true,
    "currentUserCount": 1
}
```

---

### Test 4: Check Feature Access

```bash
# Check if feature available for tenant
curl -X GET http://localhost:8888/api/subscription/features/{tenantId} \
  -H "Authorization: Bearer TOKEN"
```

**Expected Features** (based on Professional plan):
```json
{
    "advancedReporting": true,
    "apiAccess": true,
    "prioritySupport": true,
    "userLimit": 10,
    "assessmentLimit": 100,
    "policyLimit": 50
}
```

---

### Test 5: User Limit Enforcement

```bash
// In business logic:
if (await _subscriptionService.IsUserLimitReachedAsync(tenantId))
{
    return BadRequest("User limit reached for your plan");
}

// Create new tenant user...
```

---

### Test 6: Subscription Renewal

```bash
curl -X POST http://localhost:8888/api/subscription/renew/{subscriptionId} \
  -H "Authorization: Bearer TOKEN"
```

**Expected**:
- Status → "Active"
- SubscriptionEndDate → null
- SubscriptionStartDate → now
- NextBillingDate → +1 month (or +1 year for Annual)

---

### Test 7: Subscription Expiration

```csharp
// Scheduled job (daily):
int expiredCount = await _subscriptionService.CheckSubscriptionStatusAsync();

// Automatically moves expired Trial → "Expired"
// Sets SubscriptionEndDate
// Sends expiration reminder emails
```

---

### Test 8: Email Notifications

**Check email logs**:
```
/app/logs/grcmvc-{date}.log

Look for:
- [INFO] SendEmailAsync: Welcome email sent
- [INFO] SendEmailAsync: Payment confirmation sent
- [INFO] SendEmailAsync: Invoice email sent
```

---

## 📊 Database Schema

```sql
-- Subscription Plans
CREATE TABLE SubscriptionPlans (
    Id UUID PRIMARY KEY,
    Name VARCHAR(100),
    Code VARCHAR(20),  -- MVP, PRO, ENT
    MonthlyPrice DECIMAL(10,2),
    AnnualPrice DECIMAL(10,2),
    MaxUsers INT,
    HasAdvancedReporting BOOLEAN,
    -- ... more fields
);

-- Subscriptions
CREATE TABLE Subscriptions (
    Id UUID PRIMARY KEY,
    TenantId UUID FOREIGN KEY,
    PlanId UUID FOREIGN KEY,
    Status VARCHAR(50),  -- Trial, Active, Suspended, Cancelled, Expired
    TrialEndDate TIMESTAMP NULL,
    SubscriptionStartDate TIMESTAMP,
    SubscriptionEndDate TIMESTAMP NULL,
    NextBillingDate TIMESTAMP NULL,
    BillingCycle VARCHAR(20),  -- Monthly, Annual
    AutoRenew BOOLEAN DEFAULT TRUE,
    -- ... more fields
);

-- Payments
CREATE TABLE Payments (
    Id UUID PRIMARY KEY,
    SubscriptionId UUID FOREIGN KEY,
    TenantId UUID FOREIGN KEY,
    TransactionId VARCHAR(255),
    Amount DECIMAL(10,2),
    Status VARCHAR(50),  -- Pending, Completed, Failed, Refunded
    PaymentDate TIMESTAMP,
    -- ... more fields
);

-- Invoices
CREATE TABLE Invoices (
    Id UUID PRIMARY KEY,
    SubscriptionId UUID FOREIGN KEY,
    TenantId UUID FOREIGN KEY,
    InvoiceNumber VARCHAR(100),
    InvoiceDate TIMESTAMP,
    DueDate TIMESTAMP,
    TotalAmount DECIMAL(10,2),
    AmountPaid DECIMAL(10,2),
    Status VARCHAR(50),  -- Draft, Sent, Paid, Overdue
    -- ... more fields
);
```

---

## 🔐 Security Considerations

1. **Payment Data**: Use tokenized payment methods (Stripe, PayPal)
2. **PCI Compliance**: Don't store credit card data
3. **Audit Trail**: Log all subscription changes
4. **Encryption**: Encrypt sensitive data at rest
5. **Authorization**: Verify user owns tenant before accessing

---

## 📈 Advanced Features

### Proration
When a user upgrades/downgrades mid-cycle, calculate prorated amount:
```csharp
decimal proratedAmount = (planPrice / daysInBillingCycle) * remainingDays;
```

### Recurring Billing
Scheduled job to auto-charge on `NextBillingDate`:
```csharp
var dueBillings = await _subscriptionService
    .GetSubscriptionsDueForRenewalAsync();

foreach (var subscription in dueBillings)
{
    await _subscriptionService.RenewSubscriptionAsync(subscription.Id);
    await _paymentService.AutoChargeAsync(subscription);
}
```

### Usage Tracking
Monitor actual usage vs. plan limits:
```csharp
var userCount = await _dbContext.TenantUsers
    .CountAsync(tu => tu.TenantId == tenantId && tu.Status == "Active");

var subscription = await _subscriptionService
    .GetSubscriptionByTenantAsync(tenantId);

subscription.CurrentUserCount = userCount;
await _dbContext.SaveChangesAsync();
```

---

## 🚀 Implementation Checklist

- [x] Database models created
- [x] Service interface defined
- [x] Service implementation
- [x] Controller endpoints
- [x] DTOs created
- [x] Email notifications
- [x] Database integration
- [ ] Stripe/PayPal integration
- [ ] Scheduled jobs for renewal
- [ ] Admin dashboard for subscriptions
- [ ] User-facing subscription pages
- [ ] Webhook handlers for payment confirmations
- [ ] Analytics and reporting
- [ ] Dunning (retry) logic for failed payments

---

## 📞 Support

For issues or questions:
- Check logs: `/app/logs/grcmvc-*.log`
- Review database: Check Subscriptions, Payments, Invoices tables
- Test endpoints: Use curl or Postman
- Monitor email queue: Check `SmtpEmailSender` logs

