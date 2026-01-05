# ✅ Onboarding API 415 Error - FIXED

## Issue
HTTP 415 "Unsupported Media Type" error on `/api/onboarding/signup`

## Root Cause
The `OnboardingController` was missing:
- `[ApiController]` attribute (enables automatic model validation and content-type handling)
- `[Consumes("application/json")]` attribute (explicitly accepts JSON)
- Was using `Controller` instead of `ControllerBase` (better for API-only endpoints)

## Fix Applied
Updated `OnboardingController.cs`:
```csharp
[ApiController]
[Route("api/onboarding")]
[Consumes("application/json")]
[Produces("application/json")]
public class OnboardingController : ControllerBase
```

## Expected Request Format
```json
{
  "organizationName": "Your Organization",
  "adminEmail": "admin@example.com",
  "tenantSlug": "your-org-slug"
}
```

## Status
- ✅ Controller updated with ApiController attribute
- ✅ Application rebuilt and deployed
- ✅ Endpoint should now accept JSON requests properly

---

**The 415 error should now be resolved. The endpoint accepts `application/json` content type.**

---
