# GRC System - Quick Fix Guide for Critical Issues

**Purpose:** Step-by-step guide to fix the 8 critical blockers preventing production deployment
**Estimated Time:** 36-48 hours
**Outcome:** 85% Production Ready

---

## Fix 1: AuthenticationService - Replace Mock with Database (6-8 hours)

### Current Problem
```csharp
// Lines 14-15: Mock data in memory
private readonly Dictionary<string, AuthUserDto> _mockUsers = new();
private readonly Dictionary<string, string> _tokenStore = new();
```

### Solution Steps

**Step 1: Update Dependencies** (15 mins)
```csharp
// File: src/GrcMvc/Services/Implementations/AuthenticationService.cs

// REMOVE:
// private readonly Dictionary<string, AuthUserDto> _mockUsers = new();
// private readonly Dictionary<string, string> _tokenStore = new();

// ADD:
private readonly UserManager<ApplicationUser> _userManager;
private readonly SignInManager<ApplicationUser> _signInManager;
private readonly ITokenService _tokenService;
private readonly GrcDbContext _context;

public AuthenticationService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    GrcDbContext context,
    ILogger<AuthenticationService> logger)
{
    _userManager = userManager;
    _signInManager = signInManager;
    _tokenService = tokenService;
    _context = context;
    _logger = logger;
}
```

**Step 2: Implement LoginAsync** (2 hrs)
```csharp
public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
{
    try
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = "Invalid credentials"
            };
        }

        // Check password
        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            loginDto.Password,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = result.IsLockedOut
                    ? "Account locked"
                    : "Invalid credentials"
            };
        }

        // Generate JWT token
        var token = await _tokenService.GenerateTokenAsync(user);

        return new AuthResultDto
        {
            Success = true,
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Login failed for {Email}", loginDto.Email);
        return new AuthResultDto
        {
            Success = false,
            ErrorMessage = "Login failed"
        };
    }
}
```

**Step 3: Implement RegisterAsync** (2 hrs)
```csharp
public async Task<AuthResultDto> RegisterAsync(RegisterDto registerDto)
{
    try
    {
        // Check if user exists
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = "User already exists"
            };
        }

        // Create user
        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            TenantId = registerDto.TenantId,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return new AuthResultDto
            {
                Success = false,
                ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        // Assign default role
        await _userManager.AddToRoleAsync(user, "User");

        // Generate token
        var token = await _tokenService.GenerateTokenAsync(user);

        return new AuthResultDto
        {
            Success = true,
            Token = token,
            UserId = user.Id,
            Email = user.Email
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Registration failed for {Email}", registerDto.Email);
        return new AuthResultDto
        {
            Success = false,
            ErrorMessage = "Registration failed"
        };
    }
}
```

**Step 4: Remove all Task.Delay()** (30 mins)
- Delete lines 58, 79, 111, 128, 134, 173, 197, 223, 234, 256

**Step 5: Test** (1 hr)
```bash
# Start application
cd src/GrcMvc
dotnet run

# Test login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@grc.com","password":"Admin123!"}'

# Restart application and test again (token should still work)
```

---

## Fix 2: AuthorizationService - Replace Mock with Database (8-10 hours)

### Current Problem
```csharp
// Lines 15-16: Mock data in memory
private readonly Dictionary<string, List<string>> _userRoles = new();
private readonly Dictionary<string, List<string>> _rolePermissions = new();
```

### Solution Steps

**Step 1: Update Dependencies** (30 mins)
```csharp
// File: src/GrcMvc/Services/Implementations/AuthorizationService.cs

// REMOVE:
// private readonly Dictionary<string, List<string>> _userRoles = new();
// private readonly Dictionary<string, List<string>> _rolePermissions = new();

// ADD:
private readonly UserManager<ApplicationUser> _userManager;
private readonly RoleManager<ApplicationRole> _roleManager;
private readonly GrcDbContext _context;

public AuthorizationService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    GrcDbContext context,
    ILogger<AuthorizationService> logger)
{
    _userManager = userManager;
    _roleManager = roleManager;
    _context = context;
    _logger = logger;
}
```

**Step 2: Implement CheckPermissionAsync** (3 hrs)
```csharp
public async Task<bool> CheckPermissionAsync(string userId, string permission)
{
    try
    {
        // Get user
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        // Get user roles
        var userRoles = await _userManager.GetRolesAsync(user);

        // Get permissions for each role
        foreach (var roleName in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            // Get role permissions from database
            var claims = await _roleManager.GetClaimsAsync(role);

            if (claims.Any(c => c.Type == "Permission" && c.Value == permission))
            {
                return true;
            }
        }

        return false;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Permission check failed for user {UserId}", userId);
        return false;
    }
}
```

**Step 3: Implement AssignRoleAsync** (2 hrs)
```csharp
public async Task<bool> AssignRoleAsync(string userId, string roleName)
{
    try
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists) return false;

        var result = await _userManager.AddToRoleAsync(user, roleName);

        if (result.Succeeded)
        {
            // Audit log
            await _context.AuditEvents.AddAsync(new AuditEvent
            {
                EventType = "RoleAssigned",
                UserId = userId,
                Details = $"Role {roleName} assigned",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        return result.Succeeded;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Role assignment failed");
        return false;
    }
}
```

**Step 4: Remove all Task.Delay()** (30 mins)
- Delete lines 34, 51, 58, 86, 102, 112, 134

**Step 5: Test** (2 hrs)
```bash
# Test role assignment
curl -X POST http://localhost:5000/api/auth/assign-role \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"userId":"user-id","roleName":"Admin"}'

# Test permission check
curl -X GET http://localhost:5000/api/auth/check-permission?permission=CreateRisk \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## Fix 3: EvidenceService - Replace Mock with Database (4-6 hours)

### Current Problem
```csharp
// Line 15: Mock list
private readonly List<EvidenceDto> _mockEvidence = new();
```

### Solution Steps

**Step 1: Update Dependencies** (15 mins)
```csharp
// File: src/GrcMvc/Services/Implementations/EvidenceService.cs

// REMOVE:
// private readonly List<EvidenceDto> _mockEvidence = new();

// ADD:
private readonly GrcDbContext _context;

public EvidenceService(
    GrcDbContext context,
    ILogger<EvidenceService> logger)
{
    _context = context;
    _logger = logger;
}
```

**Step 2: Implement GetAllEvidenceAsync** (1 hr)
```csharp
public async Task<List<EvidenceDto>> GetAllEvidenceAsync(Guid? controlId = null)
{
    try
    {
        var query = _context.Evidence
            .AsNoTracking()
            .Include(e => e.Control)
            .Include(e => e.UploadedByUser)
            .AsQueryable();

        if (controlId.HasValue)
        {
            query = query.Where(e => e.ControlId == controlId.Value);
        }

        var evidence = await query
            .OrderByDescending(e => e.UploadedDate)
            .Select(e => new EvidenceDto
            {
                Id = e.Id,
                ControlId = e.ControlId,
                ControlName = e.Control.Name,
                FileName = e.FileName,
                FileUrl = e.FileUrl,
                UploadedDate = e.UploadedDate,
                UploadedBy = e.UploadedByUser.FullName,
                Status = e.Status,
                ReviewedBy = e.ReviewedByUser.FullName,
                ReviewedDate = e.ReviewedDate
            })
            .ToListAsync();

        return evidence;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to retrieve evidence");
        return new List<EvidenceDto>();
    }
}
```

**Step 3: Implement SubmitEvidenceAsync** (2 hrs)
```csharp
public async Task<EvidenceDto> SubmitEvidenceAsync(CreateEvidenceDto dto)
{
    try
    {
        var evidence = new Evidence
        {
            Id = Guid.NewGuid(),
            ControlId = dto.ControlId,
            FileName = dto.FileName,
            FileUrl = dto.FileUrl,
            Description = dto.Description,
            UploadedDate = DateTime.UtcNow,
            UploadedById = dto.UploadedById,
            Status = "Pending Review"
        };

        _context.Evidence.Add(evidence);
        await _context.SaveChangesAsync();

        // Return DTO
        var control = await _context.Controls
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == dto.ControlId);

        return new EvidenceDto
        {
            Id = evidence.Id,
            ControlId = evidence.ControlId,
            ControlName = control?.Name,
            FileName = evidence.FileName,
            FileUrl = evidence.FileUrl,
            UploadedDate = evidence.UploadedDate,
            Status = evidence.Status
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to submit evidence");
        throw;
    }
}
```

**Step 4: Implement GetStatisticsAsync** (1 hr)
```csharp
public async Task<EvidenceStatisticsDto> GetStatisticsAsync()
{
    try
    {
        var total = await _context.Evidence.CountAsync();
        var pending = await _context.Evidence
            .CountAsync(e => e.Status == "Pending Review");
        var approved = await _context.Evidence
            .CountAsync(e => e.Status == "Approved");
        var rejected = await _context.Evidence
            .CountAsync(e => e.Status == "Rejected");

        return new EvidenceStatisticsDto
        {
            TotalEvidence = total,
            PendingReview = pending,
            Approved = approved,
            Rejected = rejected
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to get evidence statistics");
        return new EvidenceStatisticsDto();
    }
}
```

**Step 5: Remove all Task.Delay()** (15 mins)
- Delete lines 19, 25, 31, 54, 72

**Step 6: Test** (1 hr)
```bash
# Test evidence submission
curl -X POST http://localhost:5000/api/evidence \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"controlId":"guid","fileName":"test.pdf","fileUrl":"/uploads/test.pdf"}'

# Test retrieval
curl -X GET http://localhost:5000/api/evidence \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## Fix 4: Program.cs - Remove Duplicate Registrations (30 minutes)

### Solution
```csharp
// File: src/GrcMvc/Program.cs

// DELETE THESE LINES:
// Lines 393-394: Duplicate DbContext
// Lines 400-410: Duplicate Identity
// Line 317: Duplicate RulesEngineService

// KEEP ONLY THE FIRST REGISTRATION OF EACH
```

**Test:**
```bash
dotnet build
# Should compile without warnings about duplicate registrations
```

---

## Fix 5: Generate SSL Certificate (15 minutes)

### Solution
```bash
cd src/GrcMvc
mkdir -p certificates
dotnet dev-certs https -ep certificates/aspnetapp.pfx -p "SecurePassword123!"
```

**Update appsettings.Production.json:**
```json
{
  "Kestrel": {
    "Certificates": {
      "Default": {
        "Path": "certificates/aspnetapp.pfx",
        "Password": "${CERT_PASSWORD}"
      }
    }
  }
}
```

**Test:**
```bash
dotnet run --environment Production
# Should start with HTTPS enabled
```

---

## Fix 6: Create Production Environment File (30 minutes)

### Solution
```bash
# Create .env.grcmvc.production
cat > .env.grcmvc.production << 'EOF'
# Database
DB_USER=grc_prod_user
DB_PASSWORD=YourSecurePassword123!
DB_HOST=postgres
DB_PORT=5432
DB_NAME=grc_production

# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80

# SSL Certificate
CERT_PASSWORD=SecurePassword123!

# Admin Account
ADMIN_EMAIL=admin@yourcompany.com
ADMIN_PASSWORD=YourAdminPassword123!

# Email/SMTP
EMAIL_HOST=smtp.gmail.com
EMAIL_PORT=587
SMTP_USER=noreply@yourcompany.com
SMTP_PASSWORD=YourSMTPPassword
SENDER_EMAIL=noreply@yourcompany.com
SENDER_NAME=GRC System

# JWT
JWT_SECRET_KEY=YourVerySecureJWTSecretKeyThatIs256BitsLong1234567890
JWT_ISSUER=https://yourdomain.com
JWT_AUDIENCE=https://yourdomain.com
JWT_EXPIRATION_MINUTES=60

# Hangfire
HANGFIRE_DB_CONNECTION=Server=postgres;Port=5432;Database=grc_hangfire;User Id=${DB_USER};Password=${DB_PASSWORD};

# Redis (optional)
REDIS_CONNECTION=redis:6379
EOF
```

---

## Fix 7: Dashboard Mock Data (2-3 hours)

### Solution

**Step 1: Create API Endpoint** (1 hr)
```csharp
// File: src/GrcMvc/Controllers/Api/DashboardApiController.cs

[HttpGet("statistics")]
[ResponseCache(Duration = 60)]
public async Task<IActionResult> GetStatistics()
{
    var stats = new
    {
        plans = new
        {
            active = await _context.Plans.CountAsync(p => p.Status == "Active"),
            completed = await _context.Plans.CountAsync(p => p.Status == "Completed"),
            total = await _context.Plans.CountAsync()
        },
        baselines = await _context.Baselines.CountAsync(),
        workflows = new
        {
            pending = await _context.WorkflowInstances.CountAsync(w => w.Status == "Pending"),
            completed = await _context.WorkflowInstances.CountAsync(w => w.Status == "Completed")
        },
        assessments = new
        {
            pending = await _context.Assessments.CountAsync(a => a.Status == "Pending"),
            completed = await _context.Assessments.CountAsync(a => a.Status == "Completed")
        }
    };

    return Ok(stats);
}
```

**Step 2: Update View** (1 hr)
```javascript
// File: src/GrcMvc/Views/Dashboard/Index.cshtml

// REPLACE lines 195-315 with:
fetch('/api/dashboard/statistics')
    .then(response => response.json())
    .then(data => {
        // Update UI with real data
        document.getElementById('active-plans').textContent = data.plans.active;
        document.getElementById('completed-plans').textContent = data.plans.completed;
        document.getElementById('baselines').textContent = data.baselines;
        // ... etc
    })
    .catch(error => {
        console.error('Failed to load dashboard data:', error);
    });
```

**Step 3: Test** (30 mins)
```bash
# Verify API works
curl http://localhost:5000/api/dashboard/statistics

# Verify dashboard displays real data
open http://localhost:5000/dashboard
```

---

## Fix 8: Database Migration & Seeding (2 hours)

### Solution

**Step 1: Run Migrations** (30 mins)
```bash
cd src/GrcMvc
dotnet ef database update --connection "Server=localhost;Database=grc_production;User Id=grc_prod_user;Password=YourPassword;"
```

**Step 2: Run Seeder** (30 mins)
```bash
cd ../Grc.DbMigrator
dotnet run
```

**Step 3: Verify Seed Data** (1 hr)
```bash
# Connect to database
psql -U grc_prod_user -d grc_production

# Check tables
\dt

# Verify admin user
SELECT * FROM "AspNetUsers" WHERE "Email" = 'admin@grc.com';

# Verify roles
SELECT * FROM "AspNetRoles";

# Exit
\q
```

---

## Verification Checklist

After completing all fixes, verify:

```bash
# 1. Application starts
cd src/GrcMvc
dotnet run --environment Production

# 2. Health check passes
curl http://localhost:5000/health

# 3. Login works
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@grc.com","password":"Admin123!"}'

# 4. Dashboard shows real data
curl http://localhost:5000/api/dashboard/statistics

# 5. Evidence service works
curl -X GET http://localhost:5000/api/evidence \
  -H "Authorization: Bearer YOUR_TOKEN"

# 6. Restart application and verify persistence
# Stop and start application, then login again - should work
```

---

## Timeline

| Fix | Time | Cumulative |
|-----|------|------------|
| 1. AuthenticationService | 6-8 hrs | 6-8 hrs |
| 2. AuthorizationService | 8-10 hrs | 14-18 hrs |
| 3. EvidenceService | 4-6 hrs | 18-24 hrs |
| 4. Program.cs Duplicates | 30 mins | 18.5-24.5 hrs |
| 5. SSL Certificate | 15 mins | 18.75-24.75 hrs |
| 6. Environment Config | 30 mins | 19.25-25.25 hrs |
| 7. Dashboard Mock Data | 2-3 hrs | 21.25-28.25 hrs |
| 8. Database Migration | 2 hrs | 23.25-30.25 hrs |
| **Testing & Verification** | 6-8 hrs | **29.25-38.25 hrs** |
| **TOTAL** | | **30-40 hours** |

---

**Next Steps:** After completing these fixes, proceed to High Priority tasks in DEPLOYMENT_CHECKLIST.md

**Target:** 85% Production Ready after Phase 1 completion
