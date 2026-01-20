import type { Metadata } from "next"
import "@/styles/globals.css"
import { GlobalProviders } from "@/components/providers"
import { FloatingAIAssistant } from "@/components/landing/FloatingAIAssistant"

export const metadata: Metadata = {
  metadataBase: new URL("https://shahin-ai.com"),
  title: "شاهين | منصة الحوكمة والمخاطر والامتثال بالذكاء الاصطناعي",
  description: "منصة GRC متكاملة مصممة للمؤسسات السعودية. دعم كامل للأطر التنظيمية NCA، SAMA، PDPL",
  keywords: ["GRC", "الامتثال", "المخاطر", "الحوكمة", "NCA", "SAMA", "الأمن السيبراني", "السعودية"],
  authors: [{ name: "Shahin AI" }],
  icons: {
    icon: [
      { url: "/favicon-32.png", sizes: "32x32", type: "image/png" },
      { url: "/favicon-16.png", sizes: "16x16", type: "image/png" },
    ],
    shortcut: "/favicon.ico",
    apple: "/logo.png",
  },
  openGraph: {
    title: "شاهين | منصة الحوكمة والمخاطر والامتثال",
    description: "منصة GRC بالذكاء الاصطناعي للمؤسسات السعودية",
    url: "https://shahin-ai.com",
    siteName: "شاهين GRC",
    locale: "ar_SA",
    type: "website",
    images: ["/logo.png"],
  },
  twitter: {
    card: "summary_large_image",
    title: "شاهين GRC",
    description: "منصة GRC بالذكاء الاصطناعي",
    images: ["/logo.png"],
  },
  robots: {
    index: true,
    follow: true,
  },
}

/**
 * Root Layout - Design System 2.0
 *
 * Features:
 * - Dynamic theme switching (light/dark)
 * - Skip link for keyboard navigation
 * - Font optimization with Inter & Tajawal
 */
export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="ar" dir="rtl" suppressHydrationWarning>
      <head>
        {/* Theme and locale initialization - check localStorage before render */}
        <script
          dangerouslySetInnerHTML={{
            __html: `
              (function() {
                // Theme
                const theme = localStorage.getItem('theme') || 'light';
                document.documentElement.classList.add(theme);
                // Locale (Arabic is default)
                const locale = localStorage.getItem('locale') || 'ar';
                document.documentElement.lang = locale;
                document.documentElement.dir = locale === 'ar' ? 'rtl' : 'ltr';
              })();
            `,
          }}
        />
        {/* Preload fonts - Arabic (Tajawal) and English (Inter) */}
        <link rel="preconnect" href="https://fonts.googleapis.com" />
        <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="anonymous" />
        <link
          href="https://fonts.googleapis.com/css2?family=Tajawal:wght@300;400;500;700;800&family=Inter:wght@300;400;500;600;700;800&display=swap"
          rel="stylesheet"
        />
        <meta name="theme-color" content="#10b981" media="(prefers-color-scheme: light)" />
        <meta name="theme-color" content="#059669" media="(prefers-color-scheme: dark)" />
      </head>
      <body className="min-h-screen antialiased bg-white dark:bg-gray-950 text-gray-900 dark:text-gray-100 transition-colors duration-300">
        <GlobalProviders>
          {/* Skip Link for Accessibility */}
          <a
            href="#main-content"
            className="sr-only focus:not-sr-only focus:absolute focus:top-4 focus:left-4 focus:z-50 focus:px-4 focus:py-2 focus:bg-emerald-600 focus:text-white focus:rounded-lg"
          >
            Skip to main content
          </a>

          {/* Main Content */}
          <div id="app-root" className="relative flex min-h-screen flex-col">
            {children}
          </div>

          {/* Portal root for modals */}
          <div id="portal-root" />

          {/* Floating AI Assistant - Available on all pages */}
          <FloatingAIAssistant />
        </GlobalProviders>
      </body>
    </html>
  )
}
