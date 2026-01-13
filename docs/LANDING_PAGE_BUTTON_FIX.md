# 🎯 Landing Page Button Fix Guide

## Issue Summary

Currently, all "Start Free Trial" buttons link to `/grc-free-trial` (marketing page), but they should link to `/trial` (actual registration).

---

## Files to Update

### 1. Landing/Index.cshtml (Main Landing Page)

**File:** `/home/user/Shahin-Jan-2026/src/GrcMvc/Views/Landing/Index.cshtml`

**Line 32 - CHANGE FROM:**
```html
<a href="/grc-free-trial" class="btn btn-gradient btn-lg">
    @L["Landing_StartFreeTrial"]
    <i class="bi bi-arrow-left-circle-fill" aria-hidden="true"></i>
</a>
```

**TO:**
```html
<a href="/trial" class="btn btn-gradient btn-lg">
    @L["Landing_StartFreeTrial"]
    <i class="bi bi-arrow-left-circle-fill" aria-hidden="true"></i>
</a>
```

---

### 2. Landing/FreeTrial.cshtml (Free Trial Marketing Page)

**File:** `/home/user/Shahin-Jan-2026/src/GrcMvc/Views/Landing/FreeTrial.cshtml`

**Line 285 - CHANGE FROM:**
```html
<a href="/grc-free-trial" class="trial-cta-btn">
    <i class="bi bi-rocket-takeoff"></i>
    Start Free Trial
</a>
```

**TO:**
```html
<a href="/trial" class="trial-cta-btn">
    <i class="bi bi-rocket-takeoff"></i>
    Start Free Trial
</a>
```

**Line 399 - CHANGE FROM:**
```html
<a href="/grc-free-trial" class="trial-cta-btn">
    <i class="bi bi-rocket-takeoff"></i>
    Start Free Trial
</a>
```

**TO:**
```html
<a href="/trial" class="trial-cta-btn">
    <i class="bi bi-rocket-takeoff"></i>
    Start Free Trial
</a>
```

---

## Testing the Fix

After making these changes, test the flow:

1. **Visit:** `http://localhost:5010/`
2. **Click:** "Start Free Trial" button
3. **Expected:** You should see the trial registration form (`/trial`)
4. **Fill form:** Organization name, email, password
5. **Submit:** Should create tenant and redirect to onboarding

---

## Correct User Journey

```
Landing Page (/)
    ↓ Click "Start Free Trial"
    ↓
Free Trial Marketing (/grc-free-trial) [OPTIONAL - Marketing info]
    ↓ Click "Start Free Trial"
    ↓
Trial Registration Form (/trial)
    ↓ Fill form + Submit
    ↓
Create Account (tenant + admin user)
    ↓
Onboarding Wizard (12 steps)
    ↓
Dashboard
```

---

## URLs Explained

| URL | Purpose | Shows |
|-----|---------|-------|
| `/` or `/landing/index` | Main landing page | Hero, features, pricing |
| `/grc-free-trial` | Marketing page | Trial benefits, FAQ, steps |
| **`/trial`** | **Registration form** | **Actual signup form** ⭐ |
| `/trial/register` | POST endpoint | Creates tenant + user |
| `/onboarding` | After registration | 12-step guided setup |

---

## Quick Fix Commands

Run these to make the changes automatically:

```bash
cd /home/user/Shahin-Jan-2026

# Fix Landing/Index.cshtml
sed -i 's|href="/grc-free-trial"|href="/trial"|g' src/GrcMvc/Views/Landing/Index.cshtml

# Fix Landing/FreeTrial.cshtml
sed -i 's|href="/grc-free-trial"|href="/trial"|g' src/GrcMvc/Views/Landing/FreeTrial.cshtml

# Verify changes
grep -n 'href="/trial"' src/GrcMvc/Views/Landing/Index.cshtml
grep -n 'href="/trial"' src/GrcMvc/Views/Landing/FreeTrial.cshtml
```

Expected output:
```
Index.cshtml:32:                <a href="/trial" class="btn btn-gradient btn-lg">
FreeTrial.cshtml:285:                <a href="/trial" class="trial-cta-btn">
FreeTrial.cshtml:399:        <a href="/trial" class="trial-cta-btn">
```

---

## ✅ Summary

**BEFORE:** All buttons point to `/grc-free-trial` (loops to marketing page)
**AFTER:** All buttons point to `/trial` (actual registration)

**Result:** Users can actually sign up for trial!
