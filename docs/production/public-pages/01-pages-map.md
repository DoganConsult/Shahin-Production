# Information Architecture - Pages Map

**Generated:** 2026-01-12  
**Project:** Shahin AI GRC Platform - Public Marketing Pages

---

## Core Pages (Required)

| Route | Status | File Path | Purpose |
|-------|--------|-----------|---------|
| `/` | ✅ Exists | `app/[locale]/page.tsx` | Home landing page |
| `/pricing` | ❌ **TO CREATE** | `app/[locale]/pricing/page.tsx` | Pricing overview with tiers |
| `/features` | ❌ **TO CREATE** | `app/[locale]/features/page.tsx` | Detailed features showcase |
| `/how-it-works` | ❌ **TO CREATE** | `app/[locale]/how-it-works/page.tsx` | Product explanation & workflow |
| `/contact` | ✅ Exists | `app/[locale]/contact/page.tsx` | Contact form & info |
| `/privacy` | ✅ Exists | `app/[locale]/privacy/page.tsx` | Privacy Policy |
| `/terms` | ✅ Exists | `app/[locale]/terms/page.tsx` | Terms of Service |

---

## Optional Pages

| Route | Status | File Path | Purpose |
|-------|--------|-----------|---------|
| `/about` | ✅ Exists | `app/[locale]/about/page.tsx` | About us page |
| `/status` | ⚠️ Optional | `app/[locale]/status/page.tsx` | System status (can link to external) |

---

## Page Details

### 1. Home (`/`)
**Status:** ✅ Implemented  
**Components Used:**
- Navigation
- Hero
- Expertise (features grid)
- Approach (how it works)
- Sectors (industries)
- WhyUs (benefits)
- CTA
- Footer

**Content Source:** `messages/{locale}.json`

---

### 2. Pricing (`/pricing`) - **TO CREATE**

**Purpose:** Display pricing tiers and subscription options

**Required Sections:**
- Hero section (title, subtitle)
- Pricing tiers (Starter, Professional, Enterprise)
- Feature comparison table
- FAQ section (pricing-related)
- CTA section

**Content to Pull:**
- From product docs: Subscription tiers mentioned in onboarding
- Pricing structure (if available in repo)

**Components Needed:**
- Pricing cards component
- Feature comparison table
- FAQ accordion

**Metadata:**
- Title: "Pricing - Shahin AI GRC Platform"
- Description: "Transparent pricing for enterprise GRC automation"

---

### 3. Features (`/features`) - **TO CREATE**

**Purpose:** Detailed feature showcase

**Required Sections:**
- Hero section
- Feature categories:
  - Governance & Policy Management
  - Risk Identification & Assessment
  - Compliance Controls & Evidence
  - Audit Readiness & Reporting
  - AI Agents & Automation
- Feature grid with icons
- Use cases
- CTA section

**Content to Pull:**
- `FEATURES_QUICK_REFERENCE.md`
- `MARKETING_SALES_FEATURES_PLAYBOOK.md`
- `messages/{locale}.json` (expertise section)

**Components Needed:**
- Feature grid (reuse from Expertise)
- Feature detail cards

**Metadata:**
- Title: "Features - Shahin AI GRC Platform"
- Description: "Comprehensive GRC features powered by AI"

---

### 4. How It Works (`/how-it-works`) - **TO CREATE**

**Purpose:** Product explanation and workflow

**Required Sections:**
- Hero section
- Step-by-step workflow:
  1. Connect (integrations)
  2. Analyze (AI monitoring)
  3. Automate (workflows)
  4. Report (dashboards)
- AI agents overview
- Integration capabilities
- CTA section

**Content to Pull:**
- `messages/{locale}.json` (approach section)
- `COMPLETE_SYSTEM_OVERVIEW.md`

**Components Needed:**
- Step-by-step timeline
- Integration logos/icons

**Metadata:**
- Title: "How It Works - Shahin AI GRC Platform"
- Description: "Learn how Shahin AI automates your GRC operations"

---

### 5. Contact (`/contact`)
**Status:** ✅ Implemented  
**Features:**
- Contact form (name, email, phone, company, service, message)
- Contact info (email, phone, address, hours)
- Map placeholder
- Integrated with backend API (`/api/Landing/Contact`)

---

### 6. Privacy (`/privacy`)
**Status:** ✅ Exists  
**Note:** Verify content is production-ready (not placeholder)

---

### 7. Terms (`/terms`)
**Status:** ✅ Exists  
**Note:** Verify content is production-ready (not placeholder)

---

## Routing Strategy

**Deployment Choice:** Marketing at `/`, App at `/app`

**Implementation:**
- Marketing pages: `app/[locale]/{page}/page.tsx`
- App redirect: Already configured in `next.config.js` (`/login` → `app.shahin-ai.com`)
- No path-based routing needed (marketing is root)

---

## Navigation Updates Required

**Current Navigation Links:**
```typescript
const navLinks = [
  { href: "#expertise", label: t('expertise') },
  { href: "#approach", label: t('approach') },
  { href: "#sectors", label: t('sectors') },
  { href: `/${locale}/contact`, label: t('contact') },
]
```

**Recommended Updates:**
- Add `/pricing` link
- Add `/features` link
- Keep anchor links for home page sections
- Add dropdown menu for "Platform" → Features, How It Works, Pricing

---

## Content Strategy

### Centralized Content Config

**File to Create:** `content/marketing.ts` or `content/marketing.json`

**Purpose:** Store reusable content for:
- Pricing tiers
- Feature descriptions
- FAQs
- Testimonials (if added)
- CTA copy

**Benefits:**
- Single source of truth
- Easy to update
- Type-safe (if using TypeScript)

---

## Implementation Priority

1. **High Priority:**
   - `/pricing` page
   - `/features` page
   - `/how-it-works` page

2. **Medium Priority:**
   - SEO metadata for all pages
   - Centralized content config
   - Navigation updates

3. **Low Priority:**
   - `/status` page (optional)
   - Testimonials component (optional)

---

## Next Steps

1. ✅ Create `/pricing` page with pricing tiers
2. ✅ Create `/features` page with feature showcase
3. ✅ Create `/how-it-works` page with workflow explanation
4. ✅ Update navigation to include new pages
5. ✅ Add SEO metadata to all pages
6. ✅ Create centralized content config

---

**Status:** ✅ **READY FOR IMPLEMENTATION**
