import Script from 'next/script'

interface JsonLdProps {
  data: Record<string, any>
}

/**
 * JSON-LD structured data component for SEO
 * Adds structured data to pages for better search engine understanding
 */
export function JsonLd({ data }: JsonLdProps) {
  return (
    <Script
      id="json-ld"
      type="application/ld+json"
      dangerouslySetInnerHTML={{ __html: JSON.stringify(data) }}
    />
  )
}

/**
 * Organization JSON-LD schema
 */
export function OrganizationJsonLd({ locale }: { locale: string }) {
  const isArabic = locale === 'ar'
  
  const data = {
    '@context': 'https://schema.org',
    '@type': 'Organization',
    name: isArabic ? 'شاهين للذكاء الاصطناعي' : 'Shahin AI',
    alternateName: 'Shahin AI - GRC Platform',
    url: 'https://portal.shahin-ai.com',
    logo: 'https://portal.shahin-ai.com/images/logo.png',
    description: isArabic
      ? 'منصة حوكمة المخاطر والامتثال المدعومة بالذكاء الاصطناعي'
      : 'AI-Powered Governance, Risk, and Compliance Platform',
    address: {
      '@type': 'PostalAddress',
      addressCountry: 'SA',
      addressRegion: 'Riyadh',
      addressLocality: 'Riyadh',
    },
    contactPoint: {
      '@type': 'ContactPoint',
      telephone: '+966-XX-XXX-XXXX',
      contactType: 'Customer Service',
      email: 'info@doganconsult.com',
      availableLanguage: ['Arabic', 'English'],
    },
    sameAs: [
      'https://www.linkedin.com/company/shahin-ai',
      'https://twitter.com/shahinai',
    ],
  }

  return <JsonLd data={data} />
}

/**
 * Website JSON-LD schema
 */
export function WebsiteJsonLd({ locale }: { locale: string }) {
  const siteUrl = 'https://portal.shahin-ai.com'
  const isArabic = locale === 'ar'

  const data = {
    '@context': 'https://schema.org',
    '@type': 'WebSite',
    name: isArabic ? 'شاهين للذكاء الاصطناعي' : 'Shahin AI',
    url: siteUrl,
    description: isArabic
      ? 'منصة GRC مدعومة بالذكاء الاصطناعي للمملكة العربية السعودية'
      : 'AI-Powered GRC Platform for Saudi Arabia',
    inLanguage: [locale, locale === 'ar' ? 'en' : 'ar'],
    potentialAction: {
      '@type': 'SearchAction',
      target: {
        '@type': 'EntryPoint',
        urlTemplate: `${siteUrl}/${locale}/search?q={search_term_string}`,
      },
      'query-input': 'required name=search_term_string',
    },
  }

  return <JsonLd data={data} />
}

/**
 * BreadcrumbList JSON-LD schema
 */
export function BreadcrumbJsonLd({ items }: { items: Array<{ name: string; url: string }> }) {
  const data = {
    '@context': 'https://schema.org',
    '@type': 'BreadcrumbList',
    itemListElement: items.map((item, index) => ({
      '@type': 'ListItem',
      position: index + 1,
      name: item.name,
      item: item.url,
    })),
  }

  return <JsonLd data={data} />
}

/**
 * Service JSON-LD schema
 */
export function ServiceJsonLd({
  name,
  description,
  provider,
  areaServed,
  locale,
}: {
  name: string
  description: string
  provider: string
  areaServed: string
  locale: string
}) {
  const data = {
    '@context': 'https://schema.org',
    '@type': 'Service',
    name,
    description,
    provider: {
      '@type': 'Organization',
      name: provider,
    },
    areaServed: {
      '@type': 'Country',
      name: areaServed,
    },
    availableLanguage: locale === 'ar' ? ['Arabic', 'English'] : ['English', 'Arabic'],
  }

  return <JsonLd data={data} />
}
