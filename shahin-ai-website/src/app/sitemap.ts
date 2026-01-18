import { MetadataRoute } from 'next'
import { locales } from '@/i18n'

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || 'https://portal.shahin-ai.com'

// Core pages that should be indexed
const corePages = [
  '',
  'about',
  'contact',
  'pricing',
  'how-it-works',
  'services',
  'privacy',
  'terms',
  'cookies',
]

// Service detail pages
const servicePages = [
  'telecommunications',
  'data-centers',
  'cybersecurity',
  'governance',
]

export default function sitemap(): MetadataRoute.Sitemap {
  const routes: MetadataRoute.Sitemap = []

  // Generate routes for each locale
  locales.forEach((locale) => {
    // Home page
    routes.push({
      url: `${siteUrl}/${locale}`,
      lastModified: new Date(),
      changeFrequency: 'weekly',
      priority: 1.0,
      alternates: {
        languages: Object.fromEntries(
          locales.map((l) => [l, `${siteUrl}/${l}`])
        ),
      },
    })

    // Core pages
    corePages.forEach((page) => {
      if (page === '') return // Skip home (already added)
      
      routes.push({
        url: `${siteUrl}/${locale}/${page}`,
        lastModified: new Date(),
        changeFrequency: page === 'pricing' ? 'monthly' : 'weekly',
        priority: page === 'pricing' || page === 'contact' ? 0.9 : 0.8,
        alternates: {
          languages: Object.fromEntries(
            locales.map((l) => [l, `${siteUrl}/${l}/${page}`])
          ),
        },
      })
    })

    // Service detail pages
    servicePages.forEach((service) => {
      routes.push({
        url: `${siteUrl}/${locale}/services/${service}`,
        lastModified: new Date(),
        changeFrequency: 'monthly',
        priority: 0.7,
        alternates: {
          languages: Object.fromEntries(
            locales.map((l) => [l, `${siteUrl}/${l}/services/${service}`])
          ),
        },
      })
    })
  })

  return routes
}
