import type { Metadata } from "next"
import "@/styles/globals.css"
import { GlobalProviders } from "@/components/providers"
import { NextIntlClientProvider } from "next-intl"
import { getMessages } from "next-intl/server"

export const metadata: Metadata = {
  title: "شاهين GRC | منصة إدارة الحوكمة والمخاطر والامتثال",
  description: "منصة متكاملة لإدارة الحوكمة والمخاطر والامتثال مصممة خصيصاً للمؤسسات السعودية. دعم كامل للأطر التنظيمية السعودية مثل NCA و SAMA و PDPL.",
  keywords: ["GRC", "الامتثال", "المخاطر", "الحوكمة", "NCA", "SAMA", "الأمن السيبراني", "السعودية"],
  authors: [{ name: "Shahin AI" }],
  openGraph: {
    title: "شاهين GRC | منصة إدارة الحوكمة والمخاطر والامتثال",
    description: "منصة متكاملة لإدارة الحوكمة والمخاطر والامتثال مصممة خصيصاً للمؤسسات السعودية",
    url: "https://shahin-ai.com",
    siteName: "Shahin GRC",
    locale: "ar_SA",
    type: "website",
  },
  twitter: {
    card: "summary_large_image",
    title: "شاهين GRC",
    description: "منصة إدارة الحوكمة والمخاطر والامتثال",
  },
  robots: {
    index: true,
    follow: true,
  },
}

/**
 * Root Layout - Design System 2.0
 *
 * Includes accessibility features:
 * - Skip link for keyboard navigation
 * - Proper ARIA landmarks
 * - Font optimization
 */
export default async function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  // Get translations for Arabic (default locale)
  const messages = await getMessages()

  return (
    <html lang="ar" dir="rtl" className="light" suppressHydrationWarning>
      <head>
        {/* Force Light Mode - Remove dark class and set light theme */}
        <script
          dangerouslySetInnerHTML={{
            __html: `
              document.documentElement.classList.remove('dark');
              document.documentElement.classList.add('light');
              localStorage.setItem('theme', 'light');
            `,
          }}
        />
        {/* Preconnect for faster font loading */}
        <link rel="preconnect" href="https://fonts.googleapis.com" />
        <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="anonymous" />
        <link
          href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&family=Tajawal:wght@300;400;500;700;800&display=swap"
          rel="stylesheet"
        />
        {/* Theme color for mobile browsers */}
        <meta name="theme-color" content="#10b981" media="(prefers-color-scheme: light)" />
        <meta name="theme-color" content="#059669" media="(prefers-color-scheme: dark)" />
      </head>
      <body className="min-h-screen antialiased">
        <NextIntlClientProvider messages={messages} locale="ar">
          <GlobalProviders>
            {/* Skip Link for Accessibility */}
            <a
              href="#main-content"
              className="skip-link"
            >
              تخطي إلى المحتوى الرئيسي
            </a>

            {/* Main Content Wrapper */}
            <div id="app-root" className="relative flex min-h-screen flex-col">
              {children}
            </div>

            {/* Portal root for modals, tooltips, etc. */}
            <div id="portal-root" />
          </GlobalProviders>
        </NextIntlClientProvider>
      </body>
    </html>
  )
}
