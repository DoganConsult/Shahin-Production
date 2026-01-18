# SEO, Metadata, and Social Sharing Implementation

## Overview

This document describes the SEO implementation for the Shahin AI marketing site, including metadata, Open Graph, Twitter cards, canonical URLs, robots.txt, sitemap.xml, and JSON-LD structured data.

## Implementation Status

✅ **COMPLETED**

### 1. Per-Page Metadata

All pages now have unique titles and descriptions:

- **Home Page** (`/`): Uses default metadata from `messages.json`
- **Pricing** (`/pricing`): Custom metadata via `layout.tsx`
- **Contact** (`/contact`): Custom metadata via `layout.tsx`
- **About** (`/about`): Custom metadata via `layout.tsx`
- **How It Works** (`/how-it-works`): Custom metadata via `layout.tsx`
- **Services** (`/services`): Uses default metadata (can be enhanced)
- **Privacy/Terms/Cookies**: Uses default metadata

**Implementation Pattern:**
- Each route with custom metadata has a `layout.tsx` file that exports `generateMetadata()`
- Metadata is pulled from `messages/en.json` and `messages/ar.json` for i18n support
- All metadata includes canonical URLs and language alternates

### 2. Open Graph & Twitter Card Metadata

**Location:** `src/app/[locale]/layout.tsx` and individual page layouts

**Features:**
- Open Graph images (1200x630px) for each page
- Twitter Card with `summary_large_image` format
- Locale-specific metadata (ar_SA for Arabic, en_US for English)
- Site name and description from messages

**Example:**
```typescript
openGraph: {
  type: 'website',
  locale: locale === 'ar' ? 'ar_SA' : 'en_US',
  url: canonicalUrl,
  siteName: 'Shahin AI - GRC Platform',
  title: t.title,
  description: t.description,
  images: [{
    url: `${siteUrl}/images/og-image.jpg`,
    width: 1200,
    height: 630,
    alt: t.title,
  }],
}
```

**Required Images:**
- `/public/images/og-image.jpg` (home page)
- `/public/images/og-pricing.jpg` (pricing page)
- `/public/images/og-contact.jpg` (contact page)
- `/public/images/og-about.jpg` (about page)
- `/public/images/og-how-it-works.jpg` (how-it-works page)

**⚠️ MISSING:** These OG images need to be created and placed in `/public/images/`

### 3. Canonical URLs

**Implementation:**
- All pages include canonical URLs in metadata
- Language alternates for bilingual support
- Default language set to Arabic (`x-default: /ar`)

**Example:**
```typescript
alternates: {
  canonical: `${siteUrl}/${locale}/pricing`,
  languages: {
    'en': `${siteUrl}/en/pricing`,
    'ar': `${siteUrl}/ar/pricing`,
  },
}
```

### 4. robots.txt

**Location:** `/public/robots.txt`

**Configuration:**
- Allows all user agents to crawl the site
- Disallows `/api/`, `/_next/`, `/admin/` paths
- Includes sitemap location
- Crawl-delay for aggressive bots (AhrefsBot, SemrushBot, DotBot)

**Content:**
```
User-agent: *
Allow: /
Disallow: /api/
Disallow: /_next/
Disallow: /admin/

Sitemap: https://portal.shahin-ai.com/sitemap.xml
```

### 5. sitemap.xml

**Location:** `src/app/sitemap.ts` (dynamic sitemap route)

**Features:**
- Automatically generates sitemap for all locales
- Includes all core pages and service detail pages
- Sets appropriate priorities and change frequencies
- Includes language alternates for hreflang

**Pages Included:**
- Home (`/`)
- About, Contact, Pricing, How It Works
- Services listing and detail pages
- Privacy, Terms, Cookies

**Priority Levels:**
- Home: 1.0
- Pricing, Contact: 0.9
- Other core pages: 0.8
- Service detail pages: 0.7

**Change Frequencies:**
- Home: weekly
- Pricing: monthly
- Other pages: weekly/monthly as appropriate

### 6. JSON-LD Structured Data

**Location:** `src/components/seo/JsonLd.tsx`

**Components:**
- `OrganizationJsonLd`: Organization schema with contact info
- `WebsiteJsonLd`: Website schema with search action
- `BreadcrumbJsonLd`: Breadcrumb navigation schema
- `ServiceJsonLd`: Service schema for service pages

**Usage:**
- Home page includes `OrganizationJsonLd` and `WebsiteJsonLd`
- Other pages can include `BreadcrumbJsonLd` and `ServiceJsonLd` as needed

**Example:**
```tsx
<OrganizationJsonLd locale={locale} />
<WebsiteJsonLd locale={locale} />
```

### 7. Favicon & App Icons

**Status:** ⚠️ **MISSING**

**Required Files:**
- `/public/favicon.ico`
- `/public/images/brand/favicon-32.png`
- `/public/images/brand/favicon-16.png`
- `/public/images/brand/favicon-128.png` (for Apple touch icon)

**Note:** The layout references these files, but they need to be created.

## Environment Variables

Required environment variables for SEO:

```env
NEXT_PUBLIC_SITE_URL=https://portal.shahin-ai.com
NEXT_PUBLIC_GOOGLE_VERIFICATION=your-google-verification-code
NEXT_PUBLIC_YANDEX_VERIFICATION=your-yandex-verification-code
```

## Testing SEO Implementation

### 1. Metadata Testing
- Use browser DevTools to inspect `<head>` tags
- Verify Open Graph tags with [Facebook Sharing Debugger](https://developers.facebook.com/tools/debug/)
- Verify Twitter Card with [Twitter Card Validator](https://cards-dev.twitter.com/validator)

### 2. Sitemap Testing
- Visit `https://portal.shahin-ai.com/sitemap.xml`
- Verify all pages are included
- Check language alternates are correct

### 3. robots.txt Testing
- Visit `https://portal.shahin-ai.com/robots.txt`
- Verify sitemap location is correct

### 4. JSON-LD Testing
- Use [Google Rich Results Test](https://search.google.com/test/rich-results)
- Verify structured data is valid

### 5. Canonical URLs
- Use browser DevTools to check `<link rel="canonical">` tags
- Verify no duplicate content issues

## Next Steps

1. **Create OG Images**: Generate 1200x630px images for each page
2. **Create Favicons**: Generate favicon set (ico, 16px, 32px, 128px)
3. **Add Google Search Console**: Set up verification and submit sitemap
4. **Add Analytics**: Integrate Google Analytics or similar (if not already done)
5. **Enhance Service Pages**: Add JSON-LD Service schema to service detail pages
6. **Add Breadcrumbs**: Implement breadcrumb navigation with JSON-LD on all pages

## Files Created/Modified

### Created:
- `/public/robots.txt`
- `/src/app/sitemap.ts`
- `/src/components/seo/JsonLd.tsx`
- `/src/app/[locale]/pricing/layout.tsx`
- `/src/app/[locale]/contact/layout.tsx`
- `/src/app/[locale]/about/layout.tsx`
- `/src/app/[locale]/how-it-works/layout.tsx`

### Modified:
- `/src/app/[locale]/layout.tsx` (enhanced metadata with OG and Twitter cards)
- `/src/app/[locale]/page.tsx` (added JSON-LD components)

## Quality Checklist

- ✅ Unique titles and descriptions for all pages
- ✅ Open Graph metadata implemented
- ✅ Twitter Card metadata implemented
- ✅ Canonical URLs for all pages
- ✅ Language alternates (hreflang) configured
- ✅ robots.txt created
- ✅ Dynamic sitemap.xml implemented
- ✅ JSON-LD structured data components created
- ⚠️ OG images need to be created
- ⚠️ Favicons need to be created
- ✅ No secrets exposed in frontend
- ✅ SEO-friendly URLs (no query parameters for content)
