# Shahin AI GRC - Design System Documentation

## Overview

The Shahin AI GRC Design System is a comprehensive component library built for the enterprise GRC platform. It provides a consistent, accessible, and beautiful user interface with full RTL support for Arabic.

**Version**: 1.0.0
**Theme**: Light (Primary)
**RTL Support**: Full bidirectional support
**Framework**: ASP.NET Core 8.0 MVC + Razor Views

---

## Design Principles

1. **Clarity**: Clean, modern, and professional interface
2. **Consistency**: Unified visual language across all components
3. **Accessibility**: WCAG 2.1 AA compliant
4. **Responsiveness**: Mobile-first approach
5. **Performance**: Optimized CSS with minimal dependencies
6. **Bilingual**: Seamless English ↔ Arabic switching

---

## Installation

### 1. Add CSS to Layout

```cshtml
<!-- In Views/Shared/_Layout.cshtml -->
<link rel="stylesheet" href="~/css/shahin-design-system.css" />
```

### 2. Add Bootstrap Icons (for component icons)

```cshtml
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css" />
```

### 3. Enable RTL Support

```cshtml
@if (CultureInfo.CurrentCulture.TextInfo.IsRightToLeft)
{
    <html lang="ar" dir="rtl">
}
else
{
    <html lang="en" dir="ltr">
}
```

---

## Design Tokens

All design values are defined as CSS variables for easy theming and consistency.

### Colors

#### Brand Colors
- `--shahin-primary`: #0066cc (Main brand color)
- `--shahin-secondary`: #6366f1 (Accent color)
- `--shahin-accent`: #10b981 (Success/positive actions)

#### Semantic Colors
- `--shahin-success`: #10b981 (Green)
- `--shahin-warning`: #f59e0b (Amber)
- `--shahin-error`: #ef4444 (Red)
- `--shahin-info`: #3b82f6 (Blue)

#### Neutral Scale
- `--shahin-gray-50` through `--shahin-gray-900`
- `--shahin-white`: #ffffff
- `--shahin-black`: #000000

### Typography

#### Font Families
- **English**: `-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto`
- **Arabic**: `'Segoe UI', Tahoma, Arial, sans-serif`
- **Monospace**: `'Cascadia Code', 'Consolas', 'Monaco'`

#### Font Sizes
- `--shahin-font-size-xs`: 0.75rem (12px)
- `--shahin-font-size-sm`: 0.875rem (14px)
- `--shahin-font-size-base`: 1rem (16px)
- `--shahin-font-size-lg`: 1.125rem (18px)
- `--shahin-font-size-xl`: 1.25rem (20px)
- `--shahin-font-size-2xl`: 1.5rem (24px)
- `--shahin-font-size-3xl`: 1.875rem (30px)
- `--shahin-font-size-4xl`: 2.25rem (36px)
- `--shahin-font-size-5xl`: 3rem (48px)

#### Font Weights
- `--shahin-font-weight-normal`: 400
- `--shahin-font-weight-medium`: 500
- `--shahin-font-weight-semibold`: 600
- `--shahin-font-weight-bold`: 700

### Spacing

8px base scale:
- `--shahin-space-1`: 0.25rem (4px)
- `--shahin-space-2`: 0.5rem (8px)
- `--shahin-space-3`: 0.75rem (12px)
- `--shahin-space-4`: 1rem (16px)
- `--shahin-space-5`: 1.25rem (20px)
- `--shahin-space-6`: 1.5rem (24px)
- `--shahin-space-8`: 2rem (32px)
- `--shahin-space-10`: 2.5rem (40px)
- `--shahin-space-12`: 3rem (48px)
- `--shahin-space-16`: 4rem (64px)
- `--shahin-space-20`: 5rem (80px)

### Border Radius
- `--shahin-radius-sm`: 0.25rem (4px)
- `--shahin-radius-base`: 0.5rem (8px)
- `--shahin-radius-md`: 0.75rem (12px)
- `--shahin-radius-lg`: 1rem (16px)
- `--shahin-radius-xl`: 1.5rem (24px)
- `--shahin-radius-full`: 9999px (Pill shape)

### Shadows
- `--shahin-shadow-sm`: Subtle shadow
- `--shahin-shadow-base`: Default shadow
- `--shahin-shadow-md`: Medium elevation
- `--shahin-shadow-lg`: High elevation
- `--shahin-shadow-xl`: Very high elevation
- `--shahin-shadow-2xl`: Maximum elevation

---

## Components

### 1. Button Component

#### Razor Partial Usage

```cshtml
<partial name="Components/_ShahinButton" model='new {
    Text = "Save Changes",
    Type = "primary",
    Size = "md",
    Icon = "check-circle",
    IsBlock = false,
    Disabled = false
}' />
```

#### HTML Usage

```html
<button class="shahin-btn shahin-btn-primary">
    <i class="bi bi-check-circle"></i>
    Save Changes
</button>
```

#### Variants
- `shahin-btn-primary` (Default brand color)
- `shahin-btn-secondary` (Neutral)
- `shahin-btn-success` (Green)
- `shahin-btn-danger` (Red)
- `shahin-btn-ghost` (Transparent with colored text)
- `shahin-btn-outline` (Border only)

#### Sizes
- `shahin-btn-xs` (Extra small)
- `shahin-btn-sm` (Small)
- Default (No class needed)
- `shahin-btn-lg` (Large)
- `shahin-btn-xl` (Extra large)

#### Modifiers
- `shahin-btn-block` (Full width)
- `disabled` attribute (Disabled state)

#### Example

```cshtml
@* Primary button with icon *@
<button class="shahin-btn shahin-btn-primary">
    <i class="bi bi-save"></i>
    @L["Save"]
</button>

@* Danger button - large size *@
<button class="shahin-btn shahin-btn-danger shahin-btn-lg">
    <i class="bi bi-trash"></i>
    @L["Delete"]
</button>

@* Full-width button *@
<button class="shahin-btn shahin-btn-success shahin-btn-block">
    @L["Submit"]
</button>
```

---

### 2. Card Component

#### Razor Partial Usage

```cshtml
<partial name="Components/_ShahinCard" model='new {
    Title = "Risk Assessment",
    Subtitle = "12 items requiring attention",
    Body = Html.Raw("<p>Card content goes here...</p>"),
    Footer = Html.Raw("<button class=\"shahin-btn shahin-btn-primary\">View All</button>"),
    Variant = "elevated"
}' />
```

#### HTML Usage

```html
<div class="shahin-card">
    <div class="shahin-card-header">
        <h3 class="shahin-card-title">Risk Assessment</h3>
        <p class="shahin-card-subtitle">12 items requiring attention</p>
    </div>
    <div class="shahin-card-body">
        <p>Card content goes here...</p>
    </div>
    <div class="shahin-card-footer">
        <button class="shahin-btn shahin-btn-primary">View All</button>
    </div>
</div>
```

#### Variants
- Default (Standard card)
- `shahin-card-elevated` (Higher shadow)
- `shahin-card-flat` (No shadow, border only)
- `shahin-card-interactive` (Clickable with hover effects)

---

### 3. Alert Component

#### Razor Partial Usage

```cshtml
<partial name="Components/_ShahinAlert" model='new {
    Type = "success",
    Title = "Success!",
    Message = "Your risk assessment has been saved successfully.",
    ShowIcon = true
}' />
```

#### HTML Usage

```html
<div class="shahin-alert shahin-alert-success" role="alert">
    <div class="shahin-alert-icon">
        <i class="bi bi-check-circle-fill"></i>
    </div>
    <div class="shahin-alert-content">
        <div class="shahin-alert-title">Success!</div>
        <div class="shahin-alert-description">Your risk assessment has been saved successfully.</div>
    </div>
</div>
```

#### Types
- `shahin-alert-success` (Green - Success messages)
- `shahin-alert-warning` (Amber - Warnings)
- `shahin-alert-error` (Red - Errors)
- `shahin-alert-info` (Blue - Information)

---

### 4. Badge Component

#### Razor Partial Usage

```cshtml
<partial name="Components/_ShahinBadge" model='new {
    Text = "Active",
    Type = "success"
}' />
```

#### HTML Usage

```html
<span class="shahin-badge shahin-badge-success">Active</span>
<span class="shahin-badge shahin-badge-warning">Pending</span>
<span class="shahin-badge shahin-badge-danger">Critical</span>
```

#### Types
- `shahin-badge-primary`
- `shahin-badge-success`
- `shahin-badge-warning`
- `shahin-badge-danger`
- `shahin-badge-info`
- `shahin-badge-neutral`

---

### 5. Form Components

#### Form Group Razor Partial

```cshtml
<partial name="Components/_ShahinFormGroup" model='new {
    Label = "Email Address",
    Name = "email",
    Type = "email",
    Placeholder = "Enter your email",
    Required = true,
    HelpText = "We'll never share your email.",
    Error = ""
}' />
```

#### HTML Usage

```html
<div class="shahin-form-group">
    <label for="email" class="shahin-form-label shahin-form-label-required">Email Address</label>
    <input type="email"
           id="email"
           name="email"
           class="shahin-form-input"
           placeholder="Enter your email"
           required />
    <span class="shahin-form-help">We'll never share your email.</span>
</div>
```

#### Form Input States
- Default
- `:focus` (Blue border with shadow)
- `.is-invalid` (Red border - validation error)
- `.is-valid` (Green border - validation success)
- `disabled` (Grayed out, not editable)

#### Form Elements
- `shahin-form-input` (Text inputs)
- `shahin-form-select` (Dropdowns)
- `shahin-form-textarea` (Multi-line text)
- `shahin-form-check` (Checkboxes and radios)

---

### 6. Progress Bar Component

#### Razor Partial Usage

```cshtml
<partial name="Components/_ShahinProgress" model='new {
    Value = 75,
    Type = "success",
    ShowLabel = true
}' />
```

#### HTML Usage

```html
<div class="shahin-progress">
    <div class="shahin-progress-bar shahin-progress-bar-success"
         style="width: 75%"
         role="progressbar"
         aria-valuenow="75"
         aria-valuemin="0"
         aria-valuemax="100">
    </div>
</div>
```

#### Types
- Default (Primary blue)
- `shahin-progress-bar-success`
- `shahin-progress-bar-warning`
- `shahin-progress-bar-danger`

---

### 7. Table Component

#### HTML Usage

```html
<div class="shahin-table-container">
    <table class="shahin-table">
        <thead>
            <tr>
                <th>@L["RiskID"]</th>
                <th>@L["Description"]</th>
                <th>@L["Severity"]</th>
                <th>@L["Status"]</th>
                <th>@L["Actions"]</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>RISK-001</td>
                <td>Data Breach Risk</td>
                <td><span class="shahin-badge shahin-badge-danger">Critical</span></td>
                <td><span class="shahin-badge shahin-badge-warning">In Progress</span></td>
                <td>
                    <button class="shahin-btn shahin-btn-sm shahin-btn-ghost">
                        <i class="bi bi-eye"></i> View
                    </button>
                </td>
            </tr>
        </tbody>
    </table>
</div>
```

#### Variants
- Default (Hover effect on rows)
- `shahin-table-striped` (Alternating row colors)

---

### 8. Modal Component

#### HTML Usage

```html
<div class="shahin-modal-overlay">
    <div class="shahin-modal">
        <div class="shahin-modal-header">
            <h3 class="shahin-modal-title">@L["CreateRisk"]</h3>
            <button class="shahin-modal-close" aria-label="Close">×</button>
        </div>
        <div class="shahin-modal-body">
            <!-- Modal content -->
        </div>
        <div class="shahin-modal-footer">
            <button class="shahin-btn shahin-btn-secondary">@L["Cancel"]</button>
            <button class="shahin-btn shahin-btn-primary">@L["Save"]</button>
        </div>
    </div>
</div>
```

---

### 9. Dropdown Component

#### HTML Usage

```html
<div class="shahin-dropdown">
    <button class="shahin-dropdown-toggle shahin-btn shahin-btn-secondary">
        Actions <i class="bi bi-chevron-down"></i>
    </button>
    <div class="shahin-dropdown-menu">
        <a href="#" class="shahin-dropdown-item">
            <i class="bi bi-eye"></i> View Details
        </a>
        <a href="#" class="shahin-dropdown-item">
            <i class="bi bi-pencil"></i> Edit
        </a>
        <div class="shahin-dropdown-divider"></div>
        <a href="#" class="shahin-dropdown-item">
            <i class="bi bi-trash"></i> Delete
        </a>
    </div>
</div>
```

---

### 10. Loading Spinner

#### Razor Partial Usage

```cshtml
<partial name="Components/_ShahinSpinner" model='new {
    Size = "lg",
    Text = "Loading risk assessments..."
}' />
```

#### HTML Usage

```html
<div class="shahin-spinner" role="status">
    <span class="shahin-hidden">Loading...</span>
</div>
```

#### Sizes
- `shahin-spinner-sm`
- Default
- `shahin-spinner-lg`

---

## Utility Classes

### Display
- `shahin-hidden` - Hide element
- `shahin-block` - Display block
- `shahin-flex` - Display flex
- `shahin-inline-flex` - Display inline-flex
- `shahin-grid` - Display grid

### Flexbox
- `shahin-flex-col` - Flex direction column
- `shahin-flex-row` - Flex direction row
- `shahin-items-center` - Align items center
- `shahin-items-start` - Align items start
- `shahin-items-end` - Align items end
- `shahin-justify-center` - Justify content center
- `shahin-justify-between` - Justify content space-between
- `shahin-justify-end` - Justify content end
- `shahin-gap-2`, `shahin-gap-4`, `shahin-gap-6` - Gap between items

### Text
- `shahin-text-center`, `shahin-text-left`, `shahin-text-right`
- `shahin-text-xs`, `shahin-text-sm`, `shahin-text-base`, `shahin-text-lg`, `shahin-text-xl`
- `shahin-font-medium`, `shahin-font-semibold`, `shahin-font-bold`
- `shahin-text-primary`, `shahin-text-success`, `shahin-text-warning`, `shahin-text-danger`, `shahin-text-muted`

### Spacing
- `shahin-m-0` - Remove margin
- `shahin-mt-4`, `shahin-mb-4` - Margin top/bottom
- `shahin-p-4`, `shahin-p-6` - Padding

### Shadows
- `shahin-shadow-sm`, `shahin-shadow`, `shahin-shadow-md`, `shahin-shadow-lg`

### Border Radius
- `shahin-rounded`, `shahin-rounded-lg`, `shahin-rounded-full`

---

## RTL (Right-to-Left) Support

The design system has full RTL support for Arabic. All components automatically adapt to RTL layout when `dir="rtl"` is set on the `<html>` tag.

### Automatic RTL Adaptations
- Text alignment reverses
- Flex directions mirror
- Padding/margin sides flip
- Dropdown menus position correctly
- Modal buttons reorder
- Icons position appropriately

### Testing RTL

```cshtml
@if (CultureInfo.CurrentCulture.Name == "ar-SA")
{
    <html lang="ar" dir="rtl">
}
```

---

## Responsive Design

All components are mobile-first and responsive.

### Breakpoints (Standard)
- Mobile: < 640px
- Tablet: 640px - 1024px
- Desktop: > 1024px

### Mobile Optimizations
- Buttons scale appropriately
- Cards have reduced padding
- Modals take up more screen space (95% width)
- Tables scroll horizontally in container

---

## Accessibility

### WCAG 2.1 AA Compliance
- ✅ Color contrast ratios meet 4.5:1 minimum
- ✅ Focus states visible on all interactive elements
- ✅ Proper ARIA labels and roles
- ✅ Keyboard navigation support
- ✅ Screen reader friendly

### Best Practices
1. Always include `aria-label` for icon-only buttons
2. Use semantic HTML (`<button>`, `<nav>`, `<main>`)
3. Provide form labels (not just placeholders)
4. Use proper heading hierarchy (h1 → h2 → h3)
5. Add `role="alert"` to alert components

---

## Usage Examples

### Complete Form Example

```cshtml
<div class="shahin-card">
    <div class="shahin-card-header">
        <h3 class="shahin-card-title">@L["CreateRiskAssessment"]</h3>
    </div>
    <div class="shahin-card-body">
        <form asp-action="Create" method="post">
            <partial name="Components/_ShahinFormGroup" model='new {
                Label = L["RiskTitle"],
                Name = "title",
                Type = "text",
                Required = true
            }' />

            <partial name="Components/_ShahinFormGroup" model='new {
                Label = L["Description"],
                Name = "description",
                Type = "textarea",
                Required = true
            }' />

            <div class="shahin-flex shahin-gap-4 shahin-justify-end">
                <button type="button" class="shahin-btn shahin-btn-secondary">
                    @L["Cancel"]
                </button>
                <button type="submit" class="shahin-btn shahin-btn-primary">
                    <i class="bi bi-check-circle"></i>
                    @L["Save"]
                </button>
            </div>
        </form>
    </div>
</div>
```

### Dashboard Card with Stats

```cshtml
<div class="shahin-card shahin-card-elevated">
    <div class="shahin-card-body">
        <div class="shahin-flex shahin-justify-between shahin-items-center">
            <div>
                <p class="shahin-text-sm shahin-text-muted">@L["TotalRisks"]</p>
                <h2 class="shahin-text-3xl shahin-font-bold shahin-text-primary">124</h2>
            </div>
            <div class="shahin-rounded-full" style="width: 64px; height: 64px; background: var(--shahin-primary-light); display: flex; align-items: center; justify-content: center;">
                <i class="bi bi-exclamation-triangle-fill shahin-text-2xl" style="color: var(--shahin-primary);"></i>
            </div>
        </div>
        <partial name="Components/_ShahinProgress" model='new { Value = 65, Type = "primary" }' />
        <p class="shahin-text-xs shahin-text-muted shahin-mt-4">
            @L["ComparedToLastMonth"]: <span class="shahin-text-success">+12%</span>
        </p>
    </div>
</div>
```

---

## Browser Support

- Chrome/Edge: Last 2 versions
- Firefox: Last 2 versions
- Safari: Last 2 versions
- Mobile Safari: iOS 12+
- Chrome Mobile: Android 8+

---

## Performance

- **CSS File Size**: ~25KB (unminified)
- **No JavaScript Dependencies**: Pure CSS components
- **Optimized Variables**: Efficient CSS custom properties
- **Tree-shakeable**: Unused components can be removed

---

## Customization

To customize colors, spacing, or other tokens, override CSS variables:

```css
:root {
  --shahin-primary: #your-color;
  --shahin-space-4: 1.5rem;
  /* ... other overrides */
}
```

---

## Support

For issues or questions:
- GitHub Issues: https://github.com/doganlap/Shahin-Ai/issues
- Documentation: This file
- Internal Team: GRC Development Team

---

**Version**: 1.0.0
**Last Updated**: 2026-01-13
**Maintained By**: Shahin AI GRC Team
