/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  images: {
    domains: ['portal.shahin-ai.com', 'app.shahin-ai.com'],
    formats: ['image/webp', 'image/avif'],
  },
  async redirects() {
    return [
      {
        source: '/login',
        destination: 'https://app.shahin-ai.com/Account/Login',
        permanent: true,
      },
    ];
  },
  i18n: {
    locales: ['en', 'ar'],
    defaultLocale: 'ar',
    localeDetection: true,
  },
  // Suppress development warnings
  reactStrictMode: true,
  // Suppress webpack warnings in development
  webpack: (config, { dev, isServer }) => {
    if (dev && !isServer) {
      config.ignoreWarnings = [
        { module: /webpack-internal/ },
        { file: /webpack-internal/ },
      ];
    }
    return config;
  },
};

module.exports = nextConfig;
