# Image Generation Scripts

This directory contains scripts for generating favicons and Open Graph images for the marketing site.

## Prerequisites

Install the required dependency:

```bash
npm install sharp --save-dev
```

## Scripts

### Generate Favicons

Generates all required favicon sizes from the source favicon:

```bash
npm run generate:favicons
```

**Output files:**
- `public/favicon-16x16.png`
- `public/favicon-32x32.png`
- `public/favicon-48x48.png`
- `public/favicon-128x128.png`
- `public/apple-touch-icon.png` (180x180)
- `public/android-chrome-192x192.png`
- `public/android-chrome-512x512.png`
- `public/favicon.ico`

### Generate Open Graph Images

Generates OG images (1200x630px) for key pages:

```bash
npm run generate:og
```

**Output files:**
- `public/images/og-image.jpg` (Homepage)
- `public/images/og-pricing.jpg` (Pricing page)
- `public/images/og-about.jpg` (About page)
- `public/images/og-contact.jpg` (Contact page)

## Customization

### OG Images

Edit `scripts/generate-og-images.js` to:
- Modify page configurations (titles, descriptions)
- Change colors (background, primary, text)
- Adjust layout and positioning
- Update logo size/position

### Favicons

Edit `scripts/generate-favicons.js` to:
- Add/remove favicon sizes
- Change source file
- Modify output format

## Notes

- OG images use the eagle logo from `public/images/logo.svg`
- Favicons are generated from `public/favicon-128x128.png` (or `favicon-256.png` if available)
- All images are optimized for web use (JPEG quality 90 for OG images, PNG for favicons)
