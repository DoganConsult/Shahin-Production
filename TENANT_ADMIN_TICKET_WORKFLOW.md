# ✅ Tenant Admin Ticket Workflow - Complete Implementation

## Overview
Support tickets now follow a **two-tier workflow**: User → Tenant Admin → Platform Admin (if needed)

---

## 🔄 Ticket Workflow

### 1. User Submits Ticket
- **Endpoint**: `/Support` (public, no login required)
- **Action**: User fills form and submits
- **Auto-Assignment**: Ticket is **automatically assigned to Tenant Admin** (if tenant ID is provided)
- **Status**: Changes from "New" to "Open" when assigned
- **Email**: Sent to customer and tenant admin

### 2. Tenant Admin Manages Ticket
- **Access**: Only tenant admin can see/manage tickets for their tenant
- **Endpoints**:
  - `GET /t/{tenantSlug}/admin/tickets` - List tickets assigned to tenant admin
  - `GET /t/{tenantSlug}/admin/tickets/{id}` - View ticket details
  - `POST /t/{tenantSlug}/admin/tickets/{id}/status` - Update status
  - `POST /t/{tenantSlug}/admin/tickets/{id}/comment` - Add comment
  - `POST /t/{tenantSlug}/admin/tickets/{id}/escalate` - Escalate to platform admin

### 3. Escalation to Platform Admin
- **Who Can Escalate**: Only tenant admin assigned to the ticket
- **When**: If tenant admin cannot resolve the issue
- **Action**: Calls `EscalateToPlatformAdminAsync()`
- **Result**:
  - Ticket reassigned to platform admin
  - Priority automatically increased (Low→Medium, Medium→High, High→Urgent)
  - SLA deadline recalculated
  - Email sent to platform admin and customer
  - Full audit trail logged

---

## 🔐 Security & Access Control

### Tenant Admin Access
- **Policy**: `ActiveTenantAdmin` (requires role + active record)
- **Scope**: Can only see/manage tickets for their own tenant
- **Assignment**: Tickets auto-assigned to tenant admin on creation
- **Escalation**: Only assigned tenant admin can escalate

### Platform Admin Access
- **Policy**: `ActivePlatformAdmin`
- **Scope**: Can see all tickets (all tenants)
- **Assignment**: Receives tickets via escalation from tenant admin

---

## 📧 Email Notifications

### Ticket Created
- **To Customer**: Confirmation with ticket number
- **To Tenant Admin**: Assignment notification (if tenant admin found)

### Status Updated
- **To Customer**: Status change notification

### Escalated
- **To Platform Admin**: Assignment notification
- **To Customer**: Escalation notification with explanation

---

## 🗄️ Database Changes

### Auto-Assignment Logic
```csharp
// In CreateTicketAsync():
if (dto.TenantId.HasValue)
{
    var tenantAdmin = await GetTenantAdminAsync(dto.TenantId.Value);
    if (tenantAdmin != null)
    {
        ticket.AssignedToUserId = tenantAdmin.UserId;
        ticket.Status = "Open"; // Auto-open when assigned
    }
}
```

### Tenant Admin Lookup
```csharp
// Gets first active tenant admin with RoleCode == "Admin"
private async Task<TenantUser?> GetTenantAdminAsync(Guid tenantId)
{
    return await _context.TenantUsers
        .Where(tu => tu.TenantId == tenantId &&
                    tu.RoleCode == "Admin" &&
                    tu.Status == "Active" &&
                    !tu.IsDeleted)
        .OrderBy(tu => tu.CreatedAt)
        .FirstOrDefaultAsync();
}
```

---

## 📋 API Endpoints Summary

### Public (No Auth)
| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/Support` | GET | Submit ticket form |
| `/Support/Submit` | POST | Create ticket |
| `/Support/Status?ticketNumber=XXX` | GET | Check ticket status |

### Tenant Admin (Requires ActiveTenantAdmin)
| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/t/{tenantSlug}/admin/tickets` | GET | List tickets (assigned to me) |
| `/t/{tenantSlug}/admin/tickets/{id}` | GET | View ticket details |
| `/t/{tenantSlug}/admin/tickets/{id}/status` | POST | Update ticket status |
| `/t/{tenantSlug}/admin/tickets/{id}/comment` | POST | Add comment |
| `/t/{tenantSlug}/admin/tickets/{id}/escalate` | POST | Escalate to platform admin |

### Platform Admin (Requires ActivePlatformAdmin)
| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/platform-admin/tickets` | GET | List all tickets |
| `/platform-admin/tickets/{id}` | GET | View ticket details |
| `/platform-admin/tickets/dashboard` | GET | Ticket metrics |
| `/platform-admin/tickets/reports/sla` | GET | SLA compliance report |

---

## ✅ Implementation Status

- ✅ Auto-assignment to tenant admin on ticket creation
- ✅ Tenant admin ticket management endpoints
- ✅ Escalation to platform admin functionality
- ✅ Email notifications (customer, tenant admin, platform admin)
- ✅ Full audit trail for all actions
- ✅ Security: Only assigned tenant admin can manage/escalate
- ✅ Priority auto-increase on escalation
- ✅ SLA recalculation on escalation

---

## 🎯 Workflow Diagram

```
User Submits Ticket
    ↓
Auto-Assigned to Tenant Admin
    ↓
Tenant Admin Reviews Ticket
    ├─ Can Resolve → Update Status → Close
    └─ Cannot Resolve → Escalate to Platform Admin
            ↓
        Platform Admin Handles
            ↓
        Resolved/Closed
```

---

## 📝 Next Steps (View Files)

Create these Razor views for tenant admin:
1. `Views/TenantAdmin/Tickets.cshtml` - Ticket list
2. `Views/TenantAdmin/TicketDetails.cshtml` - Ticket details with escalation button

All backend logic is **COMPLETE** and ready! ✅
