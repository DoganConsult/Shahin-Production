/**
 * Generate Open Graph Images for Marketing Site
 * 
 * This script generates OG images (1200x630px) for key pages:
 * - Homepage (og-image.jpg)
 * - Pricing (og-pricing.jpg)
 * - About (og-about.jpg)
 * - Contact (og-contact.jpg)
 * 
 * Requirements:
 *   npm install sharp
 * 
 * Usage:
 *   node scripts/generate-og-images.js
 */

const sharp = require('sharp');
const fs = require('fs');
const path = require('path');

const OUTPUT_DIR = path.join(__dirname, '../public/images');
const LOGO_PATH = path.join(__dirname, '../public/images/logo.svg');

// Ensure output directory exists
if (!fs.existsSync(OUTPUT_DIR)) {
  fs.mkdirSync(OUTPUT_DIR, { recursive: true });
}

// OG Image dimensions
const OG_WIDTH = 1200;
const OG_HEIGHT = 630;

// Escape XML special characters
function escapeXml(text) {
  return String(text)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&apos;');
}

// Brand colors
const COLORS = {
  background: '#000000', // Black background
  primary: '#D4AF37', // Gold (approximate from logo)
  text: '#FFFFFF',
  textSecondary: '#E5E5E5',
};

// Page configurations
const PAGES = [
  {
    filename: 'og-image.jpg',
    title: 'Shahin AI',
    subtitle: 'AI-Powered GRC Platform',
    description: 'Governance, Risk & Compliance Solutions for Saudi Arabia',
  },
  {
    filename: 'og-pricing.jpg',
    title: 'Pricing Plans',
    subtitle: 'Shahin AI GRC Platform',
    description: 'Choose the perfect plan for your compliance needs',
  },
  {
    filename: 'og-about.jpg',
    title: 'About Shahin AI',
    subtitle: 'Leading GRC Solutions',
    description: 'Empowering organizations with AI-driven compliance',
  },
  {
    filename: 'og-contact.jpg',
    title: 'Contact Us',
    subtitle: 'Shahin AI Support',
    description: 'Get in touch with our team',
  },
];

async function generateOGImage(config) {
  const { filename, title, subtitle, description } = config;
  const outputPath = path.join(OUTPUT_DIR, filename);

  try {
    // Create SVG template for OG image
    const svg = `
      <svg width="${OG_WIDTH}" height="${OG_HEIGHT}" xmlns="http://www.w3.org/2000/svg">
        <!-- Background -->
        <rect width="${OG_WIDTH}" height="${OG_HEIGHT}" fill="${COLORS.background}"/>
        
        <!-- Logo (centered, top area) -->
        <g transform="translate(${OG_WIDTH / 2 - 100}, 80)">
          <svg width="200" height="200" viewBox="0 0 1024 1024" fill="${COLORS.primary}">
            <path d="M 481,795 L 482,796 L 481,797 L 482,796 Z"/>
            <path d="M 758,451 L 662,438 L 530,460 L 674,462 L 646,474 L 625,511 L 633,581 L 673,662 L 653,687 L 590,604 L 579,519 L 569,547 L 563,616 L 633,710 L 606,738 L 497,604 L 501,524 L 472,577 L 466,619 L 582,765 L 543,799 L 388,640 L 414,535 L 479,440 L 440,464 L 376,528 L 351,653 L 437,751 L 541,847 L 637,763 L 711,670 L 666,582 L 656,523 L 690,481 L 773,472 Z"/>
            <path d="M 817,419 L 816,420 L 817,421 L 816,420 Z"/>
            <path d="M 427,382 L 416,387 L 386,411 L 341,456 L 340,460 L 328,472 L 328,475 L 324,477 L 324,480 L 297,518 L 272,563 L 258,597 L 246,639 L 243,673 L 247,671 L 249,661 L 277,603 L 311,544 L 318,537 L 317,535 L 319,535 L 329,516 L 363,466 L 366,465 L 375,449 L 387,433 L 391,431 L 390,429 L 418,393 L 422,391 Z"/>
            <path d="M 502,313 L 502,319 L 506,327 L 518,341 L 517,342 L 518,341 L 528,350 L 527,351 L 529,350 L 534,354 L 547,361 L 549,361 L 552,363 L 556,363 L 560,365 L 574,365 L 584,362 L 592,358 L 598,353 L 599,354 L 598,353 L 603,348 L 608,339 L 607,333 L 603,332 L 594,337 L 592,336 L 593,337 L 585,341 L 577,340 L 569,332 L 562,321 L 560,316 L 553,310 L 549,310 L 547,311 L 545,314 L 545,319 L 544,320 L 545,321 L 545,327 L 547,332 L 547,336 L 546,337 L 545,336 L 543,338 L 540,338 L 534,336 L 526,330 L 511,311 L 505,310 L 503,311 Z"/>
            <path d="M 668,312 L 630,270 L 583,256 L 487,240 L 345,240 L 288,295 L 216,386 L 157,474 L 160,495 L 175,514 L 259,384 L 361,269 L 479,275 L 444,292 L 378,298 L 296,392 L 192,565 L 213,598 L 287,464 L 394,327 L 456,324 L 511,284 L 636,322 Z"/>
            <path d="M 474,213 L 475,214 L 476,213 L 475,214 Z"/>
            <path d="M 142,376 L 181,378 L 249,290 L 328,213 L 477,211 L 558,221 L 642,244 L 690,301 L 798,350 L 812,370 L 820,407 L 815,432 L 783,367 L 687,322 L 635,372 L 558,424 L 695,423 L 762,436 L 795,464 L 804,513 L 831,475 L 848,429 L 845,391 L 823,346 L 796,324 L 728,291 L 671,226 L 582,192 L 480,179 L 311,183 L 232,256 Z"/>
          </svg>
        </g>
        
        <!-- Title -->
        <text x="${OG_WIDTH / 2}" y="380" 
              font-family="Arial, sans-serif" 
              font-size="64" 
              font-weight="bold" 
              fill="${COLORS.text}" 
              text-anchor="middle">${escapeXml(title)}</text>
        
        <!-- Subtitle -->
        <text x="${OG_WIDTH / 2}" y="450" 
              font-family="Arial, sans-serif" 
              font-size="36" 
              fill="${COLORS.primary}" 
              text-anchor="middle">${escapeXml(subtitle)}</text>
        
        <!-- Description -->
        <text x="${OG_WIDTH / 2}" y="520" 
              font-family="Arial, sans-serif" 
              font-size="28" 
              fill="${COLORS.textSecondary}" 
              text-anchor="middle">${escapeXml(description)}</text>
      </svg>
    `;

    // Convert SVG to PNG, then to JPG
    await sharp(Buffer.from(svg))
      .resize(OG_WIDTH, OG_HEIGHT)
      .jpeg({ quality: 90 })
      .toFile(outputPath);

    console.log(`✅ Generated: ${filename}`);
  } catch (error) {
    console.error(`❌ Error generating ${filename}:`, error.message);
  }
}

async function main() {
  console.log('🎨 Generating Open Graph images...\n');

  // Check if sharp is installed
  try {
    require('sharp');
  } catch (error) {
    console.error('❌ Error: sharp is not installed.');
    console.log('📦 Please install it with: npm install sharp');
    process.exit(1);
  }

  // Generate all OG images
  for (const page of PAGES) {
    await generateOGImage(page);
  }

  console.log('\n✨ All OG images generated successfully!');
  console.log(`📁 Output directory: ${OUTPUT_DIR}`);
}

main().catch(console.error);
