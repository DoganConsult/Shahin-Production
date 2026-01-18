import { Metadata } from 'next'
import { getMessages } from 'next-intl/server'
import type { Locale } from '@/i18n'

const siteUrl = process.env.NEXT_PUBLIC_SITE_URL || 'https://portal.shahin-ai.com'

export async function generateMetadata({
  params: { locale }
}: {
  params: { locale: Locale }
}): Promise<Metadata> {
  const messages = await getMessages()
  const t = messages.howItWorks as {
    title: string
    description: string
    subtitle: string
  }

  const localePath = locale === 'ar' ? '/ar' : '/en'
  const canonicalUrl = `${siteUrl}${localePath}/how-it-works`

  return {
    title: t.title,
    description: t.description || t.subtitle,
    alternates: {
      canonical: canonicalUrl,
      languages: {
        'en': `${siteUrl}/en/how-it-works`,
        'ar': `${siteUrl}/ar/how-it-works`,
      },
    },
    openGraph: {
      title: t.title,
      description: t.description || t.subtitle,
      url: canonicalUrl,
      type: 'website',
      images: [
        {
          url: `${siteUrl}/images/og-how-it-works.jpg`,
          width: 1200,
          height: 630,
          alt: t.title,
        },
      ],
    },
    twitter: {
      card: 'summary_large_image',
      title: t.title,
      description: t.description || t.subtitle,
      images: [`${siteUrl}/images/og-how-it-works.jpg`],
    },
  }
}

export default function HowItWorksLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return children
}
