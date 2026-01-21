# 🏢 SHAHIN AI GRC - ENTERPRISE ELITE DESIGN SYSTEM GUIDE

## 📋 **COMPLETE COMPONENT INDEX**

---

## 🎨 **DESIGN TOKENS**

### **COLOR SYSTEM**
```css
/* Apple-Inspired Primary */
--elite-primary: #007AFF
--elite-primary-variant: #0051D5
--elite-primary-on: #FFFFFF
--elite-primary-container: #E8F1FF
--elite-primary-on-container: #001D3D

/* Google Material 3 Secondary */
--elite-secondary: #6750A4
--elite-secondary-variant: #4F378B
--elite-secondary-on: #FFFFFF
--elite-secondary-container: #EADDFF
--elite-secondary-on-container: #21005D

/* Enterprise Tertiary */
--elite-tertiary: #7D5260
--elite-tertiary-container: #FFD8E4

/* Surface System */
--elite-surface: #FFFBFE
--elite-surface-variant: #E7E0EC
--elite-surface-container: #F3EDF7

/* Status Colors */
--elite-success: #00C853
--elite-warning: #FF8F00
--elite-error: #BA1A1A
--elite-info: #018786
```

### **TYPOGRAPHY SCALE**
```css
/* Display Sizes */
--elite-text-display-large: 57px
--elite-text-display-medium: 45px
--elite-text-display-small: 36px

/* Headlines */
--elite-text-headline-large: 32px
--elite-text-headline-medium: 28px
--elite-text-headline-small: 24px

/* Titles */
--elite-text-title-large: 22px
--elite-text-title-medium: 16px
--elite-text-title-small: 14px

/* Body Text */
--elite-text-body-large: 16px
--elite-text-body-medium: 14px
--elite-text-body-small: 12px

/* Labels */
--elite-text-label-large: 14px
--elite-text-label-medium: 12px
--elite-text-label-small: 11px
```

### **SPACING SYSTEM**
```css
--elite-space-1: 4px    /* Micro spacing */
--elite-space-2: 8px    /* Small spacing */
--elite-space-3: 12px   /* Compact spacing */
--elite-space-4: 16px   /* Standard spacing */
--elite-space-5: 20px   /* Medium spacing */
--elite-space-6: 24px   /* Large spacing */
--elite-space-8: 32px   /* Extra large */
--elite-space-10: 40px  /* Section spacing */
--elite-space-12: 48px  /* Component spacing */
--elite-space-16: 64px  /* Large sections */
--elite-space-20: 80px  /* Page sections */
--elite-space-24: 96px  /* Full sections */
--elite-space-32: 128px /* Super sections */
```

---

## 🎯 **COMPONENT LIBRARY**

### **BUTTONS**

#### **Primary Buttons**
```html
<!-- Standard Primary -->
<button class="elite-btn elite-btn-primary">
  Primary Action
</button>

<!-- Large Primary -->
<button class="elite-btn elite-btn-primary elite-btn-large">
  Large Primary
</button>

<!-- Hero Primary (CTA) -->
<button class="elite-btn elite-btn-primary elite-btn-hero">
  Start Journey
</button>

<!-- Icon Primary -->
<button class="elite-btn elite-btn-primary elite-btn-icon">
  <i class="bi bi-download"></i>
  Download
</button>
```

#### **Secondary Buttons**
```html
<!-- Standard Secondary -->
<button class="elite-btn elite-btn-secondary">
  Secondary Action
</button>

<!-- Outlined Secondary -->
<button class="elite-btn elite-btn-secondary elite-btn-outlined">
  Outlined Secondary
</button>

<!-- Text Secondary -->
<button class="elite-btn elite-btn-secondary elite-btn-text">
  Text Only
</button>
```

#### **Special Buttons**
```html
<!-- Tertiary Button -->
<button class="elite-btn elite-btn-tertiary">
  Tertiary Action
</button>

<!-- Surface Button -->
<button class="elite-btn elite-btn-surface">
  Surface Action
</button>

<!-- Floating Action Button -->
<button class="elite-btn elite-btn-fab">
  <i class="bi bi-plus"></i>
</button>
```

### **CARDS**

#### **Elevated Cards**
```html
<!-- Standard Elevated -->
<div class="elite-card elite-card-elevated">
  <div class="elite-card-header">
    <h3 class="elite-card-title">Card Title</h3>
  </div>
  <div class="elite-card-body">
    <p class="elite-text-body-medium">Card content goes here</p>
  </div>
</div>

<!-- Interactive Elevated -->
<div class="elite-card elite-card-elevated elite-card-interactive">
  <div class="elite-card-body">
    <h4 class="elite-card-title">Interactive Card</h4>
    <p>Hover for effects</p>
  </div>
</div>
```

#### **Outlined Cards**
```html
<div class="elite-card elite-card-outlined">
  <div class="elite-card-body">
    <h4 class="elite-card-title">Outlined Card</h4>
    <p class="elite-text-body-medium">Clean border design</p>
  </div>
</div>
```

#### **Filled Cards**
```html
<div class="elite-card elite-card-filled">
  <div class="elite-card-body">
    <h4 class="elite-card-title">Filled Card</h4>
    <p class="elite-text-body-medium">Solid background</p>
  </div>
</div>
```

### **FORMS**

#### **Input Fields**
```html
<!-- Standard Input -->
<div class="elite-form-group">
  <label class="elite-form-label">Email Address</label>
  <input type="email" class="elite-form-input" placeholder="name@company.com">
  <small class="elite-form-helper">Enter your work email</small>
</div>

<!-- Floating Label Input -->
<div class="elite-form-group elite-form-floating">
  <input type="text" class="elite-form-input" id="floatingInput" placeholder=" ">
  <label for="floatingInput" class="elite-form-label">Floating Label</label>
</div>

<!-- Textarea -->
<div class="elite-form-group">
  <label class="elite-form-label">Message</label>
  <textarea class="elite-form-textarea" rows="4" placeholder="Type your message..."></textarea>
</div>
```

#### **Select & Checkboxes**
```html
<!-- Select Dropdown -->
<div class="elite-form-group">
  <label class="elite-form-label">Department</label>
  <select class="elite-form-select">
    <option>Choose department...</option>
    <option>Engineering</option>
    <option>Marketing</option>
    <option>Sales</option>
  </select>
</div>

<!-- Checkbox -->
<div class="elite-form-check">
  <input type="checkbox" class="elite-form-check-input" id="check1">
  <label class="elite-form-check-label" for="check1">
    I agree to the terms
  </label>
</div>

<!-- Radio Button -->
<div class="elite-form-check">
  <input type="radio" class="elite-form-check-input" name="radio" id="radio1">
  <label class="elite-form-check-label" for="radio1">
    Option 1
  </label>
</div>
```

### **ALERTS & NOTIFICATIONS**

#### **Status Alerts**
```html
<!-- Success Alert -->
<div class="elite-alert elite-alert-success">
  <i class="bi bi-check-circle"></i>
  <strong>Success!</strong> Operation completed successfully.
</div>

<!-- Warning Alert -->
<div class="elite-alert elite-alert-warning">
  <i class="bi bi-exclamation-triangle"></i>
  <strong>Warning:</strong> Please review your input.
</div>

<!-- Error Alert -->
<div class="elite-alert elite-alert-error">
  <i class="bi bi-x-circle"></i>
  <strong>Error:</strong> Something went wrong.
</div>

<!-- Info Alert -->
<div class="elite-alert elite-alert-info">
  <i class="bi bi-info-circle"></i>
  <strong>Info:</strong> Additional details available.
</div>
```

#### **Toast Notifications**
```html
<div class="elite-toast elite-toast-success">
  <div class="elite-toast-header">
    <i class="bi bi-check-circle"></i>
    <strong>Success</strong>
  </div>
  <div class="elite-toast-body">
    Your changes have been saved.
  </div>
</div>
```

### **TABLES**

#### **Elite Data Table**
```html
<div class="elite-table-container">
  <table class="elite-table">
    <thead>
      <tr>
        <th class="elite-table-header">Name</th>
        <th class="elite-table-header">Department</th>
        <th class="elite-table-header">Status</th>
        <th class="elite-table-header">Actions</th>
      </tr>
    </thead>
    <tbody>
      <tr class="elite-table-row">
        <td class="elite-table-cell">John Doe</td>
        <td class="elite-table-cell">Engineering</td>
        <td class="elite-table-cell">
          <span class="elite-badge elite-badge-success">Active</span>
        </td>
        <td class="elite-table-cell">
          <button class="elite-btn elite-btn-sm elite-btn-primary">Edit</button>
        </td>
      </tr>
    </tbody>
  </table>
</div>
```

### **BADGES & TAGS**

#### **Status Badges**
```html
<!-- Success Badge -->
<span class="elite-badge elite-badge-success">Completed</span>

<!-- Warning Badge -->
<span class="elite-badge elite-badge-warning">Pending</span>

<!-- Error Badge -->
<span class="elite-badge elite-badge-error">Failed</span>

<!-- Info Badge -->
<span class="elite-badge elite-badge-info">New</span>

<!-- Neutral Badge -->
<span class="elite-badge elite-badge-neutral">Draft</span>
```

### **MODALS & DIALOGS**

#### **Elite Modal**
```html
<div class="elite-modal" tabindex="-1">
  <div class="elite-modal-dialog">
    <div class="elite-modal-content">
      <div class="elite-modal-header">
        <h5 class="elite-modal-title">Modal Title</h5>
        <button type="button" class="elite-btn-close" data-bs-dismiss="modal">
          <i class="bi bi-x"></i>
        </button>
      </div>
      <div class="elite-modal-body">
        <p>Modal content goes here...</p>
      </div>
      <div class="elite-modal-footer">
        <button type="button" class="elite-btn elite-btn-secondary">Cancel</button>
        <button type="button" class="elite-btn elite-btn-primary">Save Changes</button>
      </div>
    </div>
  </div>
</div>
```

---

## 🎪 **UTILITY CLASSES**

### **TYPOGRAPHY UTILITIES**
```html
<!-- Text Sizes -->
<p class="elite-text-display-large">Display Large</p>
<p class="elite-text-headline-medium">Headline Medium</p>
<p class="elite-text-title-large">Title Large</p>
<p class="elite-text-body-medium">Body Medium</p>
<p class="elite-text-label-medium">Label Medium</p>

<!-- Text Weights -->
<p class="elite-font-thin">Thin Weight</p>
<p class="elite-font-light">Light Weight</p>
<p class="elite-font-regular">Regular Weight</p>
<p class="elite-font-medium">Medium Weight</p>
<p class="elite-font-semibold">Semibold Weight</p>
<p class="elite-font-bold">Bold Weight</p>
```

### **SPACING UTILITIES**
```html
<!-- Margin -->
<div class="elite-m-0">No Margin</div>
<div class="elite-m-4">Standard Margin</div>
<div class="elite-mt-6">Top Margin</div>
<div class="elite-mb-8">Bottom Margin</div>

<!-- Padding -->
<div class="elite-p-4">Standard Padding</div>
<div class="elite-px-6">Horizontal Padding</div>
<div class="elite-py-8">Vertical Padding</div>
```

### **COLOR UTILITIES**
```html
<!-- Background Colors -->
<div class="elite-bg-primary">Primary Background</div>
<div class="elite-bg-surface">Surface Background</div>
<div class="elite-bg-success">Success Background</div>

<!-- Text Colors -->
<p class="elite-text-primary">Primary Text</p>
<p class="elite-text-secondary">Secondary Text</p>
<p class="elite-text-on-primary">On Primary Text</p>
```

### **SHADOW UTILITIES**
```html
<div class="elite-shadow-sm">Small Shadow</div>
<div class="elite-shadow-md">Medium Shadow</div>
<div class="elite-shadow-lg">Large Shadow</div>
<div class="elite-shadow-xl">Extra Large Shadow</div>
```

---

## 📱 **RESPONSIVE BREAKPOINTS**

```css
/* Mobile First Approach */
--elite-breakpoint-xs: 0px      /* Extra Small */
--elite-breakpoint-sm: 576px    /* Small */
--elite-breakpoint-md: 768px    /* Medium */
--elite-breakpoint-lg: 992px    /* Large */
--elite-breakpoint-xl: 1200px   /* Extra Large */
--elite-breakpoint-2xl: 1400px  /* 2X Large */
```

### **Responsive Utilities**
```html
<!-- Hide/Show by Breakpoint -->
<div class="elite-hidden-xs">Hidden on Mobile</div>
<div class="elite-visible-md">Visible on Medium+</div>

<!-- Responsive Spacing -->
<div class="elite-p-4 elite-p-md-8 elite-p-lg-12">
  Responsive Padding
</div>
```

---

## 🌙 **DARK MODE SUPPORT**

### **Dark Mode Classes**
```html
<!-- Dark Mode Container -->
<div class="elite-dark-mode">
  <!-- Content will adapt to dark theme -->
</div>

<!-- Dark Mode Toggle -->
<button class="elite-btn elite-btn-surface elite-dark-mode-toggle">
  <i class="bi bi-moon"></i>
</button>
```

---

## 🌐 **RTL SUPPORT**

### **RTL Utilities**
```html
<!-- RTL Container -->
<div class="elite-rtl" dir="rtl">
  <!-- RTL optimized content -->
</div>

<!-- RTL Specific Spacing -->
<div class="elite-rtl-ps-4">RTL Start Padding</div>
<div class="elite-rtl-pe-4">RTL End Padding</div>
```

---

## ⚡ **PERFORMANCE FEATURES**

### **Animation Classes**
```html
<!-- Fade In -->
<div class="elite-animate-fade-in">Fade In Animation</div>

<!-- Slide Up -->
<div class="elite-animate-slide-up">Slide Up Animation</div>

<!-- Scale In -->
<div class="elite-animate-scale-in">Scale In Animation</div>

<!-- Bounce -->
<div class="elite-animate-bounce">Bounce Animation</div>
```

### **Loading States**
```html
<!-- Loading Spinner -->
<div class="elite-loading-spinner"></div>

<!-- Skeleton Loader -->
<div class="elite-skeleton-loader"></div>

<!-- Progress Bar -->
<div class="elite-progress-bar">
  <div class="elite-progress-fill" style="width: 75%"></div>
</div>
```

---

## 🎯 **QUICK REFERENCE CHEAT SHEET**

### **Most Common Classes**
```css
/* Buttons */
.elite-btn-primary
.elite-btn-secondary
.elite-btn-hero

/* Cards */
.elite-card-elevated
.elite-card-outlined
.elite-card-interactive

/* Forms */
.elite-form-input
.elite-form-select
.elite-form-floating

/* Typography */
.elite-text-headline-medium
.elite-text-body-medium
.elite-font-semibold

/* Spacing */
.elite-p-4, .elite-m-4
.elite-px-6, .elite-mx-6
.elite-py-8, .elite-my-8

/* Colors */
.elite-bg-primary
.elite-text-primary
.elite-text-secondary
```

---

## 🔧 **IMPLEMENTATION CHECKLIST**

### **✅ Required Files**
- [x] `shahin-enterprise-elite.css` - Core design system
- [x] `shahin-elite-override.css` - Bootstrap integration
- [x] `_Layout.cshtml` - CSS includes

### **✅ Browser Support**
- [x] Chrome 90+
- [x] Firefox 88+
- [x] Safari 14+
- [x] Edge 90+

### **✅ Accessibility**
- [x] WCAG 2.1 AA Compliant
- [x] ARIA Labels
- [x] Keyboard Navigation
- [x] Screen Reader Support

### **✅ Performance**
- [x] Optimized CSS
- [x] Minimal Repaints
- [x] Hardware Acceleration
- [x] Lazy Loading Support

---

## 🚀 **GETTING STARTED**

1. **Include CSS Files**
```html
<link rel="stylesheet" href="~/css/shahin-enterprise-elite.css">
<link rel="stylesheet" href="~/css/shahin-elite-override.css">
```

2. **Use Component Classes**
```html
<button class="elite-btn elite-btn-primary">Get Started</button>
<div class="elite-card elite-card-elevated">Card Content</div>
```

3. **Apply Design Tokens**
```css
.my-component {
  background: var(--elite-primary);
  padding: var(--elite-space-4);
  border-radius: var(--elite-radius-lg);
}
```

---

## 📞 **SUPPORT & DOCUMENTATION**

For additional support:
- Review component examples above
- Check CSS comments in source files
- Test responsive behavior
- Validate accessibility compliance

---

**🏢 SHAHIN AI GRC - Enterprise Elite Design System v2.0.0**
*Apple & Google Inspired Excellence*
