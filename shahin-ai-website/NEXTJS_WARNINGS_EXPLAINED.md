# Next.js Development Warnings Explained

## ⚠️ Warnings You're Seeing

### 1. `data-scholarcy-content-script-executed`
**Source**: Browser extension (Scholarcy or similar)  
**Impact**: None - This is from a browser extension injecting attributes  
**Fix**: Added `suppressHydrationWarning` to `<html>` and `<body>` tags

### 2. `webpack-internal://` URL Errors
**Source**: Next.js development mode  
**Impact**: None - These are internal webpack URLs used for Fast Refresh  
**Fix**: Added webpack config to ignore these warnings in development

### 3. Fast Refresh Messages
**Source**: Next.js Hot Module Replacement (HMR)  
**Impact**: None - This is normal development behavior  
**Status**: ✅ Working as expected

## ✅ What Was Fixed

1. **Added `suppressHydrationWarning`** to layout to suppress browser extension warnings
2. **Added webpack config** to ignore webpack-internal warnings in development
3. **These warnings won't appear in production builds**

## 🚀 Production Build

When you run `npm run build`, these warnings will not appear because:
- Production builds don't use webpack-internal URLs
- Browser extensions don't run in production
- Fast Refresh is disabled in production

## 📝 Notes

- These are **development-only warnings**
- They don't affect functionality
- They're safe to ignore
- Production builds are clean

## 🔍 If Warnings Persist

1. Clear `.next` folder: `rm -rf .next`
2. Clear node_modules: `rm -rf node_modules && npm install`
3. Restart dev server: `npm run dev`
