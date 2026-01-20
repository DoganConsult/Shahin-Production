# Golden Path Blockers - Verified from Source Code

**Date:** 2026-01-20
**Method:** Direct code review (not from documentation)

---

## Summary

| Category | Count | Source |
|----------|-------|--------|
| Critical Security | 2 | Verified in code |
| Code Bugs | 1 | Verified in code |
| Configuration Issues | 2 | Verified in code |
| Placeholder Content | 3 | Verified in code |

---

## CRITICAL SECURITY BLOCKERS

### 1. Hardcoded Database Credentials

**Location:** `db-explorer/DatabaseExplorer.cs` line 28

**Actual Code:**
```csharp
var connectionString = "Host=centerbeam.proxy.rlwy.net;Port=11539;Database=railway;Username=postgres;Password=VUykzDaybssURQkSAfxUYOBKBkDQSuVW";
```

**Risk:** Database credentials exposed in version control
**Blocks:** Security compliance, production deployment

---

### 2. JWT Secret in Configuration File

**Location:** `railway-variables.json` line 5

**Actual Code:**
```json
"JWT_SECRET": "etETf%Z9jqm-AiH_YlIBoudRU^bv+rK?c4XGQs#nh5pOJ*1!y2PC7F.@W0&w$Lkx"
```

**Risk:** JWT signing key exposed, tokens can be forged
**Blocks:** Secure authentication

---

## CONFIGURATION BLOCKERS

### 3. API URL Port Mismatch

**Location 1:** `landing-page/src/lib/api.ts` line 18
```typescript
const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5137';
```

**Location 2:** `landing-page/src/components/sections/Navigation.tsx` line 11
```typescript
const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000"
```

**Problem:** Two different default ports (5137 vs 5000)
**Blocks:** Login redirect if env var not set

---

### 4. Environment Variable Not Set

**Required:** `NEXT_PUBLIC_API_URL`
**Fallback:** localhost (breaks in production)
**Blocks:** All API functionality

---

## CODE BUGS

### 5. Memory Leak in Footer Component

**Location:** `landing-page/src/components/sections/Footer.tsx` lines 29-33

**Actual Code:**
```typescript
if (typeof window !== 'undefined') {
  window.addEventListener('scroll', () => {
    setIsScrollVisible(window.scrollY > 500)
  })
}
```

**Problem:**
- Event listener added outside useEffect
- No cleanup function
- New listener added on every render

**Impact:** Memory leak, performance degradation

**Correct Pattern:**
```typescript
useEffect(() => {
  const handleScroll = () => setIsScrollVisible(window.scrollY > 500)
  window.addEventListener('scroll', handleScroll)
  return () => window.removeEventListener('scroll', handleScroll)
}, [])
```

---

## PLACEHOLDER CONTENT ISSUES

### 6. Fake Phone Number

**Location:** `landing-page/src/app/[locale]/contact/page.tsx` lines 164-167

**Actual Code:**
```typescript
<ContactInfo
  icon={Phone}
  title={t('info.phone')}
  value="+966 XX XXX XXXX"
  href="tel:+966XXXXXXXX"
/>
```

**Impact:** Users cannot call

---

### 7. Dead Social Media Links

**Location:** `landing-page/src/components/sections/Footer.tsx` lines 11-14

**Actual Code:**
```typescript
const socialLinks = [
  { icon: Linkedin, href: "#", label: "LinkedIn" },
  { icon: Twitter, href: "#", label: "Twitter" },
  { icon: Mail, href: "mailto:info@doganconsult.com", label: "Email" },
]
```

**Impact:** LinkedIn and Twitter links go nowhere (href="#")

---

### 8. Map Placeholder (No Real Map)

**Location:** `landing-page/src/app/[locale]/contact/page.tsx` lines 180-189

**Actual Code:**
```typescript
<div className={cn(
  "aspect-video rounded-2xl overflow-hidden",
  "bg-gray-100 dark:bg-gray-800",
  "border border-gray-200 dark:border-gray-700"
)}>
  <div className="w-full h-full flex items-center justify-center text-gray-400">
    <MapPin className="w-12 h-12" />
  </div>
</div>
```

**Impact:** Just shows a map icon, no actual map integration

---

## USER JOURNEY IMPACT

```
Landing Page
    |
    |--[BLOCKED]-- API calls fail if NEXT_PUBLIC_API_URL not set
    |              (api.ts:18, uses localhost:5137)
    |
    v
Login Button
    |
    |--[BLOCKED]-- Redirects to wrong port
    |              (Navigation.tsx:11, uses localhost:5000)
    |
    v
Contact Page
    |
    |--[DEGRADED]-- Phone number is placeholder
    |--[DEGRADED]-- Map is just an icon
    |
    v
Footer
    |
    |--[BUG]-- Memory leak from scroll listener
    |--[DEGRADED]-- Social links go nowhere
```

---

## FIX PRIORITY

### Immediate (Security)
1. Remove hardcoded DB credentials from DatabaseExplorer.cs
2. Move JWT_SECRET to Railway secrets (not in file)

### Before Production
3. Set NEXT_PUBLIC_API_URL in Railway/deployment
4. Standardize default port in both files

### Before Launch
5. Fix memory leak in Footer.tsx
6. Replace placeholder phone number
7. Add real social media URLs or remove
8. Add real map or remove placeholder

---

## VERIFICATION CHECKLIST

- [x] Read DatabaseExplorer.cs - Found hardcoded credentials
- [x] Read railway-variables.json - Found exposed JWT secret
- [x] Read api.ts - Found port 5137 default
- [x] Read Navigation.tsx - Found port 5000 default (mismatch)
- [x] Read Footer.tsx - Found memory leak, placeholder social links
- [x] Read contact/page.tsx - Found placeholder phone and map

---

**Status:** All findings verified directly from source code
**Documentation claims not verified:** Backend code (ASP.NET, ABP, controllers, services) does not exist in this repository
