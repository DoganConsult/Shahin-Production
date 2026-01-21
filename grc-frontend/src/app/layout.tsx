import type { Metadata } from "next"
import "@/styles/globals.css"

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

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="ar" dir="rtl" suppressHydrationWarning>
      <head>
        <link rel="preconnect" href="https://fonts.googleapis.com" />
        <link rel="preconnect" href="https://fonts.gstatic.com" crossOrigin="anonymous" />
        <link
          href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&family=Tajawal:wght@300;400;500;700;800&display=swap"
          rel="stylesheet"
        />
      </head>
      <body className="min-h-screen antialiased">
        {children}
      </body>
    </html>
  )
}
