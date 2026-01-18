# Public Marketing Pages - Production Guide

## Overview

This directory contains documentation for the production-ready public marketing site built with Next.js. The site is integrated with the existing GRC application deployment and serves as the primary marketing and lead generation platform.

## Documentation Index

1. **[00-site-discovery.md](./00-site-discovery.md)** - Current Next.js setup analysis
2. **[01-pages-map.md](./01-pages-map.md)** - Information architecture and page structure
3. **[02-seo.md](./02-seo.md)** - SEO implementation (metadata, sitemap, JSON-LD)
4. **[03-performance-a11y.md](./03-performance-a11y.md)** - Performance and accessibility baselines
5. **[04-contact.md](./04-contact.md)** - Contact form integration
6. **[05-deployment.md](./05-deployment.md)** - Deployment integration (Docker + Nginx)

## Implementation Status

| Component | Status | Notes |
|-----------|--------|-------|
| **Core Pages** | ✅ READY | All core pages implemented (`/`, `/pricing`, `/about`, `/contact`, `/how-it-works`, `/services`) |
| **Legal Pages** | ✅ READY | Privacy, Terms, Cookies pages implemented |
| **SEO Implementation** | ✅ READY | Metadata, sitemap, robots.txt, JSON-LD all implemented |
| **Performance** | ✅ READY | Image optimization, font optimization, responsive design |
| **Accessibility** | ✅ READY | Semantic HTML, ARIA labels, keyboard navigation |
| **Reusable Components** | ✅ READY | PricingCard, FAQAccordion, Testimonials, FeatureGrid extracted/created |
| **Contact Form** | ✅ READY | Integrated with backend API, CSRF protection |
| **Deployment** | ✅ READY | Docker Compose config, Nginx routing configured |
| **Content Management** | ✅ READY | Centralized via `messages/{locale}.json` and `src/content/marketing.ts` |
| **OG Images** | ✅ READY | All OG images generated (homepage, pricing, about, contact) |
| **Favicon Set** | ✅ READY | Complete favicon set generated (16x16, 32x32, 48x48, 180x180, 192x192, 512x512, ICO) |
| **Analytics** | ⚠️ MISSING | Analytics integration pending (optional) |
| **Rate Limiting** | ⚠️ MISSING | Contact form rate limiting stubbed (see Missing Items below) |
| **Error Tracking** | ⚠️ MISSING | Error tracking integration pending (optional) |

## Quick Start

### Development

```bash
cd shahin-ai-website
npm install
npm run dev
# Site available at http://localhost:3000
```

### Production Build

```bash
cd shahin-ai-website
npm run build
npm start
```

### Docker Deployment

```bash
# From project root
docker-compose -f docker-compose.production.yml up -d --build marketing-prod
```

## Site Structure

### Core Pages

- `/` - Homepage (landing page)
- `/pricing` - Pricing plans and FAQs
- `/services` - Service offerings overview
- `/services/[slug]` - Individual service detail pages
- `/about` - About us page
- `/how-it-works` - Platform explanation
- `/contact` - Contact form and information

### Legal Pages

- `/privacy` - Privacy policy
- `/terms` - Terms of service
- `/cookies` - Cookie policy

### Integration Points

- `/api/*` - Proxied to GRC backend API
- `/trial` - Proxied to GRC trial registration
- `/OnboardingWizard` - Proxied to GRC onboarding wizard

## Content Management

### Localization

Content is managed through `messages/{locale}.json` files:
- `messages/en.json` - English content
- `messages/ar.json` - Arabic content

### Content Types

Marketing content types are defined in `src/content/marketing.ts`:
- `PricingPlan` - Pricing tier definitions
- `FAQItem` - FAQ questions and answers
- `Testimonial` - Customer testimonials
- `FeatureItem` - Feature descriptions

### Adding New Content

1. **Text Content**: Add to `messages/{locale}.json`
2. **Pricing Plans**: Update `messages.{locale}.pricing.plans`
3. **FAQs**: Update `messages.{locale}.pricing.faq.items`
4. **Features**: Update `messages.{locale}.expertise.items`

## Component Library

Reusable marketing components are located in `src/components/marketing/`:

- `PricingCard` - Pricing plan display card
- `FAQAccordion` - FAQ accordion component
- `Testimonials` - Testimonials section
- `FeatureGrid` - Feature grid display

All components are exported from `src/components/marketing/index.ts`.

## Adding a New Landing Page

### Checklist

- [ ] **Create Page File**
  - Create `src/app/[locale]/your-page/page.tsx`
  - Use existing pages as templates (e.g., `about/page.tsx`)

- [ ] **Add Metadata**
  - Create `src/app/[locale]/your-page/layout.tsx`
  - Implement `generateMetadata()` function
  - Include Open Graph and Twitter card metadata
  - Set canonical URL

- [ ] **Add Content**
  - Add content to `messages/en.json` and `messages/ar.json`
  - Use `useTranslations()` hook in page component
  - Follow existing content structure patterns

- [ ] **Update Navigation**
  - Add link to `src/components/sections/Navigation.tsx`
  - Add link to `src/components/sections/Footer.tsx` (if appropriate)

- [ ] **Update Sitemap**
  - Add page to `src/app/sitemap.ts` core pages array
  - Set appropriate priority and change frequency

- [ ] **Test Locally**
  - Run `npm run dev`
  - Test both English and Arabic versions
  - Verify metadata in browser dev tools
  - Check mobile responsiveness

- [ ] **SEO Verification**
  - Verify page title and description
  - Check Open Graph tags (use [Open Graph Debugger](https://www.opengraph.xyz/))
  - Verify canonical URL
  - Test structured data (use [Google Rich Results Test](https://search.google.com/test/rich-results))

- [ ] **Accessibility Check**
  - Run Lighthouse accessibility audit
  - Verify semantic HTML structure
  - Check keyboard navigation
  - Test with screen reader (optional)

- [ ] **Build Test**
  - Run `npm run build` to ensure no build errors
  - Check for TypeScript errors
  - Verify all translations are present

- [ ] **Documentation**
  - Update `01-pages-map.md` if adding a core page
  - Add any new content keys to this README

### Example: Adding a "Blog" Page

```typescript
// src/app/[locale]/blog/page.tsx
import { useTranslations } from 'next-intl'
import { Metadata } from 'next'

export async function generateMetadata({ params: { locale } }): Promise<Metadata> {
  const t = await getMessages()
  return {
    title: t.blog.title,
    description: t.blog.description,
    // ... other metadata
  }
}

export default function BlogPage() {
  const t = useTranslations('blog')
  return (
    <div>
      <h1>{t('title')}</h1>
      {/* ... */}
    </div>
  )
}
```

```json
// messages/en.json
{
  "blog": {
    "title": "Blog",
    "description": "Latest insights and updates"
  }
}
```

## Deployment

### Pre-Deployment Checklist

- [ ] All pages have unique titles and descriptions
- [ ] No broken links (run `npm run build` and check for errors)
- [ ] All images optimized (using Next.js Image component)
- [ ] Environment variables set in `.env.production`
- [ ] Docker build succeeds: `docker build -t marketing ./shahin-ai-website`
- [ ] Health check endpoint responds: `curl http://localhost:3000/`

### Deployment Steps

See [05-deployment.md](./05-deployment.md) for detailed deployment instructions.

### Rollback

If deployment fails:

```bash
# Stop marketing service
docker-compose -f docker-compose.production.yml stop marketing-prod

# Restore previous Nginx config
sudo cp /etc/nginx/sites-available/shahin-ai-domains.conf.backup /etc/nginx/sites-available/shahin-ai-domains.conf
sudo nginx -t && sudo systemctl reload nginx
```

## Performance Monitoring

### Key Metrics

- **Lighthouse Score**: Target 90+ for all categories
- **First Contentful Paint (FCP)**: < 1.8s
- **Largest Contentful Paint (LCP)**: < 2.5s
- **Time to Interactive (TTI)**: < 3.8s
- **Cumulative Layout Shift (CLS)**: < 0.1

### Monitoring Tools

- **Lighthouse**: Built into Chrome DevTools
- **PageSpeed Insights**: https://pagespeed.web.dev/
- **WebPageTest**: https://www.webpagetest.org/

## SEO Best Practices

1. **Unique Titles**: Every page must have a unique, descriptive title
2. **Meta Descriptions**: 150-160 characters, include primary keyword
3. **Canonical URLs**: Set for all pages to avoid duplicate content
4. **Structured Data**: Use JSON-LD for Organization and Website
5. **Sitemap**: Automatically generated at `/sitemap.xml`
6. **Robots.txt**: Configured at `/robots.txt`

## Accessibility Standards

- **WCAG 2.1 Level AA**: Target compliance
- **Semantic HTML**: Use proper heading hierarchy (h1 → h2 → h3)
- **ARIA Labels**: Add where needed for screen readers
- **Keyboard Navigation**: All interactive elements must be keyboard accessible
- **Color Contrast**: Minimum 4.5:1 for normal text, 3:1 for large text

## Missing Items / TODO

### Content

- [x] **OG Images**: ✅ Created optimized Open Graph images (1200x630px) for:
  - Homepage (`/images/og-image.jpg`)
  - Pricing page (`/images/og-pricing.jpg`)
  - About page (`/images/og-about.jpg`)
  - Contact page (`/images/og-contact.jpg`)
  - *Regenerate with: `npm run generate:og`*

- [x] **Favicon Set**: ✅ Complete favicon set generated:
  - `favicon.ico` (32x32)
  - `favicon-16x16.png`, `favicon-32x32.png`, `favicon-48x48.png`, `favicon-128x128.png`
  - Apple touch icon (`apple-touch-icon.png` - 180x180)
  - Android Chrome icons (`android-chrome-192x192.png`, `android-chrome-512x512.png`)
  - *Regenerate with: `npm run generate:favicons`*

- [ ] **Testimonials**: Add real customer testimonials with:
  - Customer name, title, company
  - Testimonial quote
  - Customer photo (if available)

- [ ] **Analytics**: Integrate analytics provider (if not already done):
  - Google Analytics 4
  - Or preferred analytics solution

### Technical

- [ ] **Rate Limiting**: Implement rate limiting for contact form (currently stubbed)
- [ ] **CAPTCHA**: Add CAPTCHA to contact form (optional, for spam prevention)
- [ ] **Error Tracking**: Integrate error tracking (Sentry, LogRocket, etc.)
- [ ] **CDN**: Configure CDN for static assets (Cloudflare recommended)
- [ ] **Monitoring**: Set up uptime monitoring (UptimeRobot, Pingdom, etc.)

### Legal

- [ ] **Privacy Policy**: Review and finalize privacy policy content
- [ ] **Terms of Service**: Review and finalize terms of service content
- [ ] **Cookie Policy**: Review and finalize cookie policy content
- [ ] **GDPR Compliance**: Ensure GDPR compliance if serving EU users
- [ ] **PDPL Compliance**: Ensure PDPL compliance for Saudi users

### Marketing

- [ ] **Newsletter Integration**: Connect newsletter signup to email service (Mailchimp, SendGrid, etc.)
- [ ] **Social Media Links**: Verify all social media links in footer
- [ ] **Live Chat**: Consider adding live chat widget (Intercom, Drift, etc.)
- [ ] **A/B Testing**: Set up A/B testing framework (if needed)

## Support & Maintenance

### Regular Tasks

- **Weekly**: Check for broken links, verify all pages load correctly
- **Monthly**: Review analytics, update content as needed
- **Quarterly**: Update pricing if needed, refresh testimonials
- **Annually**: Review and update legal pages (privacy, terms, cookies)

### Contact

For issues or questions about the marketing site:
- **Technical Issues**: Check deployment logs and health checks
- **Content Updates**: Update `messages/{locale}.json` files
- **New Features**: Follow the "Adding a New Landing Page" checklist above

## Related Documentation

- [Production Deployment Guide](../README.md) - Main production deployment documentation
- [GRC Application Docs](../../README.md) - GRC application documentation
