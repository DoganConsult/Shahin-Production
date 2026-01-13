# CSP Nonce Usage Guide

## Overview

The application now uses nonce-based Content Security Policy (CSP) to protect against XSS attacks. The `unsafe-inline` and `unsafe-eval` directives have been **removed** from the CSP header.

## What Changed

### Before (INSECURE):
```
Content-Security-Policy: script-src 'self' 'unsafe-inline' 'unsafe-eval' ...
```

### After (SECURE):
```
Content-Security-Policy: script-src 'self' 'nonce-{random-value}' ...
```

## How It Works

1. **SecurityHeadersMiddleware** generates a cryptographically secure nonce per request
2. **CspNonceFilter** makes the nonce available to views via `ViewData["CSPNonce"]`
3. Views must add the nonce attribute to ALL inline `<script>` tags

## Usage in Views

### Inline Scripts

**BEFORE (will be blocked by CSP):**
```html
<script>
    console.log('Hello World');
</script>
```

**AFTER (works with CSP):**
```html
<script nonce="@ViewData["CSPNonce"]">
    console.log('Hello World');
</script>
```

### External Scripts

External scripts loaded from CDN or same-origin don't need nonces:
```html
<!-- No nonce needed for external scripts -->
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
<script src="/js/app.js"></script>
```

### Event Handlers

Inline event handlers like `onclick`, `onload` are also blocked by CSP. Move them to external scripts:

**BEFORE (blocked):**
```html
<button onclick="doSomething()">Click Me</button>
```

**AFTER (works):**
```html
<button id="myButton">Click Me</button>
<script nonce="@ViewData["CSPNonce"]">
    document.getElementById('myButton').addEventListener('click', doSomething);
</script>
```

## Best Practices

1. **Prefer External Scripts**: Move reusable JavaScript to `.js` files in `wwwroot/js/`
2. **Use Nonces for Page-Specific Logic**: Small inline scripts specific to a view should use nonces
3. **Never Use `javascript:` URLs**: These are blocked by CSP
4. **Avoid `eval()` and `new Function()`**: These are blocked by removing `unsafe-eval`

## Migration Checklist

For each view file (.cshtml):

- [ ] Find all `<script>` tags without `src` attribute (inline scripts)
- [ ] Add `nonce="@ViewData["CSPNonce"]"` to each inline script tag
- [ ] Test the page - check browser console for CSP violations
- [ ] Consider moving inline scripts to external `.js` files for better separation

## Testing

### Browser Console

CSP violations appear as errors in the browser console:
```
Refused to execute inline script because it violates the following Content Security Policy directive: "script-src 'self' 'nonce-xyz'". Either the 'unsafe-inline' keyword, a hash ('sha256-...'), or a nonce ('nonce-...') is required to enable inline execution.
```

### Fix CSP Violations

1. Open browser DevTools (F12)
2. Go to Console tab
3. Look for CSP violation errors
4. Add nonces to the reported script tags

## Implementation Status

### ✅ Completed
- SecurityHeadersMiddleware generates nonces
- CspNonceFilter makes nonces available to views
- CSP header updated (removed unsafe-inline/unsafe-eval)

### ⏳ In Progress
- Updating 150+ views with nonce attributes
- Moving common scripts to external files

## Files Modified

| File | Purpose |
|------|---------|
| `Middleware/SecurityHeadersMiddleware.cs` | Generates nonce, adds to CSP header |
| `Filters/CspNonceFilter.cs` | Adds nonce to ViewData |
| `Program.cs` | Registers CspNonceFilter globally |

## Security Benefits

- **XSS Protection**: Only scripts with valid nonces can execute
- **No Unsafe Directives**: Removed `unsafe-inline` and `unsafe-eval`
- **Per-Request Nonces**: Each request gets a unique nonce (replay attack protection)
- **Cryptographically Secure**: Nonces generated with `RandomNumberGenerator`

## Example: Updating a View

**Before:**
```cshtml
@{
    ViewData["Title"] = "Dashboard";
}

<div id="chart"></div>

<script>
    // Initialize chart
    const data = @Html.Raw(Json.Serialize(Model.ChartData));
    renderChart('#chart', data);
</script>
```

**After:**
```cshtml
@{
    ViewData["Title"] = "Dashboard";
}

<div id="chart"></div>

<script nonce="@ViewData["CSPNonce"]">
    // Initialize chart
    const data = @Html.Raw(Json.Serialize(Model.ChartData));
    renderChart('#chart', data);
</script>
```

## Troubleshooting

### "CSPNonce not found in ViewData"

**Cause**: Controller action not going through CspNonceFilter (API controller?)

**Fix**: Add `[ServiceFilter(typeof(CspNonceFilter))]` attribute to controller or action

### "Script still blocked after adding nonce"

**Cause**: Nonce value might be empty or incorrect

**Fix**:
1. Check that SecurityHeadersMiddleware is registered before MVC middleware
2. Verify `@ViewData["CSPNonce"]` outputs a non-empty value
3. Inspect HTTP response headers - CSP should contain `'nonce-{value}'`

## Related Issues

- Issue #4: Fix CSP unsafe-inline/eval (✅ Completed)
- Issue #6: Fix @Html.Raw XSS risks (⏳ In Progress)

---

**Last Updated**: 2026-01-13
**Security Contact**: GRC Team
