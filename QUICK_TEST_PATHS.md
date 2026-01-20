# Quick Test Paths - Golden Path Login Flow

## Application Paths

### Backend (ASP.NET Core MVC)
- **Path**: `C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc`
- **URL**: `http://localhost:5010`
- **Login Page**: `http://localhost:5010/Account/Login`
- **Start Command**: 
  ```powershell
  cd "C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc"
  dotnet run --urls "http://localhost:5010"
  ```

### Frontend (Next.js)
- **Path**: `C:\Shahin-ai\Shahin-Jan-2026\grc-frontend`
- **URL**: `http://localhost:3003`
- **Login Page**: `http://localhost:3003/login`
- **Start Command**:
  ```powershell
  cd "C:\Shahin-ai\Shahin-Jan-2026\grc-frontend"
  npm run dev
  ```

## Quick Test Steps

1. **Start Backend**:
   ```powershell
   cd "C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc"
   dotnet run --urls "http://localhost:5010"
   ```

2. **Start Frontend** (in another terminal):
   ```powershell
   cd "C:\Shahin-ai\Shahin-Jan-2026\grc-frontend"
   npm run dev
   ```

3. **Test Login Flow**:
   - Open: `http://localhost:3003/login`
   - Enter credentials
   - Check redirects

4. **Check [GOLDEN_PATH] Logs**:
   ```powershell
   cd "C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc"
   Get-Content "logs\grcmvc-*.log" | Select-String "\[GOLDEN_PATH\]" | Select-Object -Last 20
   ```

## Log File Location
- **Backend Logs**: `C:\Shahin-ai\Shahin-Jan-2026\src\GrcMvc\logs\grcmvc-*.log`

## [GOLDEN_PATH] Log Markers

The following events are logged with `[GOLDEN_PATH]` prefix:

1. **OnboardingRedirectMiddleware**:
   - User not authenticated
   - Tenant not found
   - Onboarding redirect decision
   - Onboarding completed

2. **AccountController Login**:
   - Login form submitted
   - SignIn result
   - User logged in successfully
   - ProcessPostLoginAsync started
   - TenantUser found
   - Redirect decision (to onboarding or dashboard)

## Expected Log Sequence

```
[GOLDEN_PATH] Login form submitted. Email=...
[GOLDEN_PATH] SignIn result. Email=..., Succeeded=True
[GOLDEN_PATH] User ... logged in successfully
[GOLDEN_PATH] ProcessPostLoginAsync started. UserId=...
[GOLDEN_PATH] TenantUser found. TenantId=..., RoleCode=...
[GOLDEN_PATH] Onboarding check. IsAdmin=..., IsCompleted=...
[GOLDEN_PATH] ✅ REDIRECT DECISION: User ... → OnboardingWizard/Index
```
