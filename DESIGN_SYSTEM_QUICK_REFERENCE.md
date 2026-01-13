# 🚀 Shahin AI Design System - Quick Reference

**Fast lookup guide for developers**

---

## 📦 Component Cheat Sheet

### **Buttons**

```cshtml
@* Razor Partial *@
<partial name="Components/_ShahinButton" model='new { Text = "Save", Type = "primary", Icon = "check" }' />

@* HTML *@
<button class="shahin-btn shahin-btn-primary"><i class="bi bi-check"></i> Save</button>
<button class="shahin-btn shahin-btn-secondary">Cancel</button>
<button class="shahin-btn shahin-btn-success">Approve</button>
<button class="shahin-btn shahin-btn-danger">Delete</button>
<button class="shahin-btn shahin-btn-ghost">View</button>
<button class="shahin-btn shahin-btn-outline">Download</button>

@* Sizes *@
<button class="shahin-btn shahin-btn-primary shahin-btn-xs">Extra Small</button>
<button class="shahin-btn shahin-btn-primary shahin-btn-sm">Small</button>
<button class="shahin-btn shahin-btn-primary">Medium</button>
<button class="shahin-btn shahin-btn-primary shahin-btn-lg">Large</button>
<button class="shahin-btn shahin-btn-primary shahin-btn-xl">Extra Large</button>

@* States *@
<button class="shahin-btn shahin-btn-primary" disabled>Disabled</button>
<button class="shahin-btn shahin-btn-primary shahin-btn-block">Full Width</button>
```

### **Cards**

```cshtml
@* Razor Partial *@
<partial name="Components/_ShahinCard" model='new {
    Title = "Card Title",
    Subtitle = "Card subtitle",
    Body = Html.Raw("<p>Content</p>"),
    Footer = Html.Raw("<button class=\"shahin-btn shahin-btn-primary\">Action</button>"),
    Variant = "elevated"
}' />

@* HTML *@
<div class="shahin-card">
    <div class="shahin-card-header">
        <h3 class="shahin-card-title">Title</h3>
        <p class="shahin-card-subtitle">Subtitle</p>
    </div>
    <div class="shahin-card-body">Content</div>
    <div class="shahin-card-footer">Footer</div>
</div>

@* Variants *@
<div class="shahin-card">Default</div>
<div class="shahin-card shahin-card-elevated">Elevated</div>
<div class="shahin-card shahin-card-flat">Flat</div>
<div class="shahin-card shahin-card-interactive">Interactive</div>
```

### **Alerts**

```cshtml
@* Razor Partial *@
<partial name="Components/_ShahinAlert" model='new {
    Type = "success",
    Title = "Success!",
    Message = "Operation completed.",
    ShowIcon = true
}' />

@* HTML *@
<div class="shahin-alert shahin-alert-success">
    <div class="shahin-alert-icon"><i class="bi bi-check-circle-fill"></i></div>
    <div class="shahin-alert-content">
        <div class="shahin-alert-title">Success!</div>
        <div class="shahin-alert-description">Message here</div>
    </div>
</div>

@* Types *@
<div class="shahin-alert shahin-alert-success">Success</div>
<div class="shahin-alert shahin-alert-warning">Warning</div>
<div class="shahin-alert shahin-alert-error">Error</div>
<div class="shahin-alert shahin-alert-info">Info</div>
```

### **Badges**

```cshtml
@* Razor Partial *@
<partial name="Components/_ShahinBadge" model='new { Text = "Active", Type = "success" }' />

@* HTML *@
<span class="shahin-badge shahin-badge-primary">Primary</span>
<span class="shahin-badge shahin-badge-success">Success</span>
<span class="shahin-badge shahin-badge-warning">Warning</span>
<span class="shahin-badge shahin-badge-danger">Danger</span>
<span class="shahin-badge shahin-badge-info">Info</span>
<span class="shahin-badge shahin-badge-neutral">Neutral</span>
```

### **Form Fields**

```cshtml
@* Razor Partial *@
<partial name="Components/_ShahinFormGroup" model='new {
    Label = "Email",
    Name = "email",
    Type = "email",
    Placeholder = "Enter email",
    Required = true,
    HelpText = "Help text here",
    Error = ""
}' />

@* HTML *@
<div class="shahin-form-group">
    <label for="email" class="shahin-form-label shahin-form-label-required">Email</label>
    <input type="email" id="email" class="shahin-form-input" required>
    <span class="shahin-form-help">Help text</span>
</div>

@* States *@
<input class="shahin-form-input">Normal</input>
<input class="shahin-form-input is-invalid">Error</input>
<input class="shahin-form-input is-valid">Valid</input>
<input class="shahin-form-input" disabled>Disabled</input>
```

### **Progress Bars**

```cshtml
@* Razor Partial *@
<partial name="Components/_ShahinProgress" model='new { Value = 75, Type = "success" }' />

@* HTML *@
<div class="shahin-progress">
    <div class="shahin-progress-bar" style="width: 75%"></div>
</div>

@* Types *@
<div class="shahin-progress">
    <div class="shahin-progress-bar" style="width: 50%">Primary</div>
</div>
<div class="shahin-progress">
    <div class="shahin-progress-bar shahin-progress-bar-success" style="width: 75%">Success</div>
</div>
<div class="shahin-progress">
    <div class="shahin-progress-bar shahin-progress-bar-warning" style="width: 60%">Warning</div>
</div>
<div class="shahin-progress">
    <div class="shahin-progress-bar shahin-progress-bar-danger" style="width: 90%">Danger</div>
</div>
```

### **Loading Spinners**

```cshtml
@* Razor Partial *@
<partial name="Components/_ShahinSpinner" model='new { Size = "lg", Text = "Loading..." }' />

@* HTML *@
<div class="shahin-spinner"></div>
<div class="shahin-spinner shahin-spinner-sm">Small</div>
<div class="shahin-spinner shahin-spinner-lg">Large</div>
```

### **Tables**

```html
<div class="shahin-table-container">
    <table class="shahin-table">
        <thead>
            <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Status</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>001</td>
                <td>Item 1</td>
                <td><span class="shahin-badge shahin-badge-success">Active</span></td>
            </tr>
        </tbody>
    </table>
</div>

<!-- Striped variant -->
<table class="shahin-table shahin-table-striped">...</table>
```

### **Dashboard Stat Cards**

```html
<div class="stat-card">
    <div class="d-flex justify-content-between align-items-center">
        <div>
            <p class="stat-card-label">Total Risks</p>
            <h3 class="stat-card-value">124</h3>
            <p class="stat-card-change positive">
                <i class="bi bi-arrow-up"></i> +12%
            </p>
        </div>
        <div class="stat-card-icon primary">
            <i class="bi bi-exclamation-triangle-fill"></i>
        </div>
    </div>
</div>

<!-- Icon variants: primary, success, warning, danger -->
```

---

## 🎨 Design Tokens

### **Colors**

```css
/* Brand */
--shahin-primary: #0066cc
--shahin-secondary: #6366f1
--shahin-accent: #10b981

/* Semantic */
--shahin-success: #10b981
--shahin-warning: #f59e0b
--shahin-error: #ef4444
--shahin-info: #3b82f6

/* Neutrals */
--shahin-gray-50 through --shahin-gray-900
```

### **Typography**

```css
/* Sizes */
--shahin-font-size-xs: 0.75rem    /* 12px */
--shahin-font-size-sm: 0.875rem   /* 14px */
--shahin-font-size-base: 1rem     /* 16px */
--shahin-font-size-lg: 1.125rem   /* 18px */
--shahin-font-size-xl: 1.25rem    /* 20px */
--shahin-font-size-2xl: 1.5rem    /* 24px */
--shahin-font-size-3xl: 1.875rem  /* 30px */

/* Weights */
--shahin-font-weight-normal: 400
--shahin-font-weight-medium: 500
--shahin-font-weight-semibold: 600
--shahin-font-weight-bold: 700
```

### **Spacing**

```css
--shahin-space-1: 0.25rem   /* 4px */
--shahin-space-2: 0.5rem    /* 8px */
--shahin-space-3: 0.75rem   /* 12px */
--shahin-space-4: 1rem      /* 16px */
--shahin-space-5: 1.25rem   /* 20px */
--shahin-space-6: 1.5rem    /* 24px */
--shahin-space-8: 2rem      /* 32px */
```

### **Shadows**

```css
--shahin-shadow-sm: 0 1px 2px rgba(0,0,0,0.05)
--shahin-shadow-base: 0 1px 3px rgba(0,0,0,0.1)
--shahin-shadow-md: 0 4px 6px rgba(0,0,0,0.1)
--shahin-shadow-lg: 0 10px 15px rgba(0,0,0,0.1)
```

### **Border Radius**

```css
--shahin-radius-sm: 0.25rem    /* 4px */
--shahin-radius-base: 0.5rem   /* 8px */
--shahin-radius-lg: 1rem       /* 16px */
--shahin-radius-full: 9999px   /* Pill */
```

---

## 🛠️ Utility Classes

### **Display**
```html
<div class="shahin-hidden">Hidden</div>
<div class="shahin-block">Block</div>
<div class="shahin-flex">Flex</div>
<div class="shahin-grid">Grid</div>
```

### **Flexbox**
```html
<div class="shahin-flex shahin-items-center shahin-justify-between shahin-gap-4">
    Flexbox layout
</div>

<!-- Classes: -->
.shahin-flex-col, .shahin-flex-row
.shahin-items-center, .shahin-items-start, .shahin-items-end
.shahin-justify-center, .shahin-justify-between, .shahin-justify-end
.shahin-gap-2, .shahin-gap-4, .shahin-gap-6
```

### **Text**
```html
<p class="shahin-text-xs shahin-text-muted">Small muted text</p>
<p class="shahin-text-sm shahin-font-medium">Small medium text</p>
<p class="shahin-text-base shahin-text-primary">Base primary text</p>
<h2 class="shahin-text-2xl shahin-font-bold">Large bold heading</h2>

<!-- Alignment -->
<p class="shahin-text-left">Left</p>
<p class="shahin-text-center">Center</p>
<p class="shahin-text-right">Right</p>

<!-- Colors -->
<span class="shahin-text-primary">Primary</span>
<span class="shahin-text-success">Success</span>
<span class="shahin-text-warning">Warning</span>
<span class="shahin-text-danger">Danger</span>
<span class="shahin-text-muted">Muted</span>
```

### **Shadows**
```html
<div class="shahin-shadow-sm">Small shadow</div>
<div class="shahin-shadow">Default shadow</div>
<div class="shahin-shadow-md">Medium shadow</div>
<div class="shahin-shadow-lg">Large shadow</div>
```

### **Border Radius**
```html
<div class="shahin-rounded">Rounded</div>
<div class="shahin-rounded-lg">Large rounded</div>
<div class="shahin-rounded-full">Pill shape</div>
```

---

## 🌍 RTL Support

**Automatic**: Already configured in `_Layout.cshtml`

```csharp
var isRtl = currentCulture == "ar";
```

```html
<html dir="@(isRtl ? "rtl" : "ltr")">
```

**All components automatically flip when `dir="rtl"` is set.**

---

## 🎯 Common Patterns

### **Page Header**

```html
<div class="page-header">
    <h1 class="page-title">Dashboard</h1>
    <p class="page-subtitle">Monitor your GRC metrics</p>
</div>
```

### **Status Indicators**

```html
<span class="status-dot active"></span> Active
<span class="status-dot pending"></span> Pending
<span class="status-dot critical"></span> Critical
```

### **Priority Badges**

```html
<span class="badge priority-high">High</span>
<span class="badge priority-medium">Medium</span>
<span class="badge priority-low">Low</span>
```

### **Empty State**

```html
<div class="empty-state">
    <div class="empty-state-icon"><i class="bi bi-inbox"></i></div>
    <h3 class="empty-state-title">No Items Found</h3>
    <p class="empty-state-description">Get started by adding your first item.</p>
    <button class="shahin-btn shahin-btn-primary">Add Item</button>
</div>
```

### **Loading State**

```html
<div class="shahin-flex shahin-items-center shahin-justify-center" style="min-height: 200px;">
    <partial name="Components/_ShahinSpinner" model='new { Size = "lg", Text = "Loading..." }' />
</div>
```

---

## 📂 File Structure

```
Shahin-Ai/
├── wwwroot/css/
│   ├── shahin-design-system.css   (28 KB - Core system)
│   ├── shahin-rtl.css             (9 KB - RTL support)
│   └── shahin-custom.css          (16 KB - Customizations)
│
├── Views/Shared/Components/
│   ├── _ShahinButton.cshtml
│   ├── _ShahinCard.cshtml
│   ├── _ShahinAlert.cshtml
│   ├── _ShahinBadge.cshtml
│   ├── _ShahinFormGroup.cshtml
│   ├── _ShahinProgress.cshtml
│   └── _ShahinSpinner.cshtml
│
├── Views/Home/
│   └── DesignSystem.cshtml        (Live showcase)
│
├── DESIGN_SYSTEM.md               (Full documentation)
├── DESIGN_SYSTEM_REVIEW.md        (Comprehensive review)
└── DESIGN_SYSTEM_QUICK_REFERENCE.md (This file)
```

---

## 🚀 Quick Links

- **Live Showcase**: `http://localhost:5010/Home/DesignSystem`
- **Full Documentation**: `/DESIGN_SYSTEM.md`
- **Comprehensive Review**: `/DESIGN_SYSTEM_REVIEW.md`
- **GitHub Repo**: `https://github.com/doganlap/Shahin-Ai`

---

## 💡 Pro Tips

1. **Use Razor Partials** for dynamic content and localization
2. **Use HTML Classes** for static content and performance
3. **Bootstrap classes auto-styled** - no need to replace existing code
4. **Test RTL** by switching to Arabic language
5. **Customize** via CSS variables in `shahin-custom.css`
6. **Reference showcase** at `/Home/DesignSystem` for live examples

---

## 🎨 Customization Example

```css
/* In shahin-custom.css */
:root {
  /* Change primary color */
  --shahin-primary: #ff6b00;

  /* Larger font size */
  --shahin-font-size-base: 1.125rem;

  /* More spacing */
  --shahin-space-4: 1.5rem;

  /* Rounder corners */
  --shahin-radius-base: 0.75rem;
}

/* Custom component */
.my-custom-card {
  background: var(--shahin-white);
  border-radius: var(--shahin-radius-lg);
  padding: var(--shahin-space-6);
  box-shadow: var(--shahin-shadow-md);
}
```

---

**Version**: 1.0.0
**Last Updated**: 2026-01-13
**Status**: ✅ Production Ready
