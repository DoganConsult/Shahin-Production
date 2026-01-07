import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'Shahin AI - GRC Platform | حوكمة المخاطر والامتثال',
  description: 'Enterprise GRC platform with AI-powered compliance, risk management, and regulatory intelligence for KSA market.',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="ar" dir="rtl" suppressHydrationWarning>
      <body suppressHydrationWarning>{children}</body>
    </html>
  );
}
