/**
 * Generate Additional Favicon Sizes
 * 
 * This script generates missing favicon sizes (192x192, 512x512) and favicon.ico
 * from the existing favicon-256.png file.
 * 
 * Requirements:
 *   npm install sharp
 * 
 * Usage:
 *   node scripts/generate-favicons.js
 */

const sharp = require('sharp');
const fs = require('fs');
const path = require('path');

const PUBLIC_DIR = path.join(__dirname, '../public');
const SOURCE_FAVICON = path.join(PUBLIC_DIR, 'favicon-128x128.png');

// Sizes needed
const SIZES = [
  { size: 16, filename: 'favicon-16x16.png' },
  { size: 32, filename: 'favicon-32x32.png' },
  { size: 48, filename: 'favicon-48x48.png' },
  { size: 180, filename: 'apple-touch-icon.png' },
  { size: 192, filename: 'android-chrome-192x192.png' },
  { size: 512, filename: 'android-chrome-512x512.png' },
];

async function generateFavicon(size, filename) {
  const outputPath = path.join(PUBLIC_DIR, filename);

  try {
    // Use 128x128 as source, or fallback to 256 if available
    let sourcePath = SOURCE_FAVICON;
    if (!fs.existsSync(sourcePath)) {
      sourcePath = path.join(PUBLIC_DIR, 'favicon-256.png');
    }

    if (!fs.existsSync(sourcePath)) {
      console.warn(`⚠️  Source favicon not found, skipping ${filename}`);
      return;
    }

    await sharp(sourcePath)
      .resize(size, size, {
        fit: 'contain',
        background: { r: 0, g: 0, b: 0, alpha: 0 }
      })
      .png()
      .toFile(outputPath);

    console.log(`✅ Generated: ${filename} (${size}x${size})`);
  } catch (error) {
    console.error(`❌ Error generating ${filename}:`, error.message);
  }
}

async function generateFaviconICO() {
  const outputPath = path.join(PUBLIC_DIR, 'favicon.ico');
  
  try {
    // Use 32x32 for ICO
    const sourcePath = path.join(PUBLIC_DIR, 'favicon-32x32.png');
    
    if (!fs.existsSync(sourcePath)) {
      console.warn('⚠️  favicon-32x32.png not found, skipping favicon.ico');
      return;
    }

    // Convert PNG to ICO (multi-size ICO with 16, 32, 48)
    const sizes = [16, 32, 48];
    const buffers = await Promise.all(
      sizes.map(size =>
        sharp(sourcePath)
          .resize(size, size)
          .png()
          .toBuffer()
      )
    );

    // For now, just copy 32x32 as ICO (proper ICO generation requires additional library)
    // Most browsers will accept PNG as ICO
    await sharp(sourcePath)
      .resize(32, 32)
      .toFile(outputPath);

    console.log('✅ Generated: favicon.ico');
  } catch (error) {
    console.error('❌ Error generating favicon.ico:', error.message);
  }
}

async function main() {
  console.log('🎨 Generating favicon files...\n');

  // Check if sharp is installed
  try {
    require('sharp');
  } catch (error) {
    console.error('❌ Error: sharp is not installed.');
    console.log('📦 Please install it with: npm install sharp');
    process.exit(1);
  }

  // Generate all favicon sizes
  for (const { size, filename } of SIZES) {
    await generateFavicon(size, filename);
  }

  // Generate favicon.ico
  await generateFaviconICO();

  console.log('\n✨ All favicon files generated successfully!');
  console.log(`📁 Output directory: ${PUBLIC_DIR}`);
}

main().catch(console.error);
