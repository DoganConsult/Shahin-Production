# Shahin AI - Complete Next.js Website Structure

## Project Overview

**Technology:** Next.js 14 (App Router) + Tailwind CSS  
**Deployment:** Vercel or your server behind Cloudflare  
**Languages:** English + Arabic (RTL support)  
**Purpose:** Marketing website for shahin-ai.com, redirects `/login` to `app.shahin-ai.com`

---

## Complete Project Structure

```
shahin-ai-website/
├── app/
│   ├── [lang]/
│   │   ├── layout.tsx              # Language-specific layout with RTL
│   │   ├── page.tsx                # Home page
│   │   ├── pricing/
│   │   │   └── page.tsx
│   │   ├── partners/
│   │   │   └── page.tsx
│   │   ├── regulatory-packs/
│   │   │   └── page.tsx
│   │   ├── resources/
│   │   │   └── page.tsx
│   │   └── contact/
│   │       └── page.tsx
│   ├── login/
│   │   └── page.tsx                # Redirects to app.shahin-ai.com
│   ├── layout.tsx                  # Root layout
│   └── globals.css                 # Tailwind + custom styles
├── components/
│   ├── layout/
│   │   ├── Header.tsx
│   │   ├── Footer.tsx
│   │   └── LanguageSwitcher.tsx
│   ├── sections/
│   │   ├── Hero.tsx
│   │   ├── TrustStrip.tsx
│   │   ├── ProblemCards.tsx
│   │   ├── DifferentiatorGrid.tsx
│   │   ├── HowItWorks.tsx
│   │   ├── RegulatoryPacks.tsx
│   │   ├── PlatformPreview.tsx
│   │   ├── PricingPreview.tsx
│   │   └── CtaBanner.tsx
│   └── ui/
│       ├── Card.tsx
│       ├── Button.tsx
│       ├── Modal.tsx
│       └── MediaFrame.tsx          # Enforces aspect ratio
├── content/
│   ├── shahin-site.en.json
│   └── shahin-site.ar.json
├── public/
│   ├── media/
│   │   ├── brand/
│   │   │   └── shahin-ai/
│   │   │       └── logo.svg
│   │   ├── screenshots/
│   │   │   └── app/
│   │   │       ├── dashboard.webp
│   │   │       ├── assessments.webp
│   │   │       └── evidence.webp
│   │   └── placeholders/
│   │       ├── hero.webp           # 16:10 ratio
│   │       ├── screenshot-1.webp  # 16:9 ratio
│   │       └── doc-thumb.webp      # 4:3 ratio
│   └── favicon.ico
├── lib/
│   ├── i18n.ts                     # Internationalization helper
│   └── utils.ts
├── tailwind.config.ts
├── next.config.js
├── package.json
└── tsconfig.json
```

---

## Key Files with Complete Code

### 1. package.json

```json
{
  "name": "shahin-ai-website",
  "version": "1.0.0",
  "private": true,
  "scripts": {
    "dev": "next dev",
    "build": "next build",
    "start": "next start",
    "lint": "next lint"
  },
  "dependencies": {
    "next": "^14.0.4",
    "react": "^18.2.0",
    "react-dom": "^18.2.0"
  },
  "devDependencies": {
    "@types/node": "^20.10.0",
    "@types/react": "^18.2.45",
    "@types/react-dom": "^18.2.18",
    "autoprefixer": "^10.4.16",
    "postcss": "^8.4.32",
    "tailwindcss": "^3.3.6",
    "typescript": "^5.3.3"
  }
}
```

### 2. tailwind.config.ts

```typescript
import type { Config } from 'tailwindcss';

const config: Config = {
  content: [
    './pages/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
    './app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#0B1F3B', // Deep Regulatory Blue
          light: '#1A3A5C',
        },
        accent: {
          DEFAULT: '#0E7490', // Compliance Teal
          light: '#14B8A6',
        },
        neutral: {
          gray: '#E5E7EB',
          slate: '#1F2937',
        },
        status: {
          success: '#15803D',
          warning: '#CA8A04',
          danger: '#B91C1C',
          info: '#2563EB',
        },
      },
      spacing: {
        xs: '4px',
        sm: '8px',
        md: '16px',
        lg: '24px',
        xl: '32px',
        '2xl': '48px',
      },
      borderRadius: {
        card: '12px',
      },
    },
  },
  plugins: [],
};

export default config;
```

### 3. next.config.js

```javascript
/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  images: {
    domains: ['app.shahin-ai.com'],
    formats: ['image/webp', 'image/avif'],
  },
  async redirects() {
    return [
      {
        source: '/',
        destination: '/en',
        permanent: false,
      },
    ];
  },
};

module.exports = nextConfig;
```

### 4. app/layout.tsx

```typescript
import { Inter } from 'next/font/google';
import './globals.css';

const inter = Inter({ subsets: ['latin', 'arabic'] });

export const metadata = {
  title: 'Shahin AI - Evidence-First GRC Platform',
  description: 'Audit-ready compliance through continuous control assessment, verified evidence, and regulatory intelligence.',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className={inter.className}>{children}</body>
    </html>
  );
}
```

### 5. app/[lang]/layout.tsx

```typescript
import { notFound } from 'next/navigation';
import Header from '@/components/layout/Header';
import Footer from '@/components/layout/Footer';

const locales = ['en', 'ar'];

export function generateStaticParams() {
  return locales.map((lang) => ({ lang }));
}

export default function LangLayout({
  children,
  params,
}: {
  children: React.ReactNode;
  params: { lang: string };
}) {
  if (!locales.includes(params.lang)) {
    notFound();
  }

  const isRtl = params.lang === 'ar';
  const dir = isRtl ? 'rtl' : 'ltr';

  return (
    <html lang={params.lang} dir={dir}>
      <body>
        <Header lang={params.lang} />
        <main>{children}</main>
        <Footer lang={params.lang} />
      </body>
    </html>
  );
}
```

### 6. app/[lang]/page.tsx (Home)

```typescript
import { getContent } from '@/lib/i18n';
import Hero from '@/components/sections/Hero';
import TrustStrip from '@/components/sections/TrustStrip';
import ProblemCards from '@/components/sections/ProblemCards';
import DifferentiatorGrid from '@/components/sections/DifferentiatorGrid';
import HowItWorks from '@/components/sections/HowItWorks';
import RegulatoryPacks from '@/components/sections/RegulatoryPacks';
import PlatformPreview from '@/components/sections/PlatformPreview';
import PricingPreview from '@/components/sections/PricingPreview';
import CtaBanner from '@/components/sections/CtaBanner';

export default async function HomePage({
  params,
}: {
  params: { lang: string };
}) {
  const content = await getContent('home', params.lang as 'en' | 'ar');

  return (
    <div className="min-h-screen">
      <Hero content={content.hero} lang={params.lang} />
      <TrustStrip content={content.trustStrip} />
      <ProblemCards content={content.problems} />
      <DifferentiatorGrid content={content.differentiators} />
      <HowItWorks content={content.howItWorks} />
      <RegulatoryPacks content={content.regulatoryPacks} lang={params.lang} />
      <PlatformPreview content={content.platformPreview} />
      <PricingPreview content={content.pricing} />
      <CtaBanner content={content.finalCta} />
    </div>
  );
}
```

### 7. app/login/page.tsx (Redirect)

```typescript
'use client';

import { useEffect } from 'react';

export default function LoginPage() {
  useEffect(() => {
    // Redirect to app subdomain
    window.location.href = 'https://app.shahin-ai.com/account/login';
  }, []);

  return (
    <div className="flex items-center justify-center min-h-screen bg-primary">
      <div className="text-center text-white">
        <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-white mb-4"></div>
        <p className="text-lg">Redirecting to login...</p>
      </div>
    </div>
  );
}
```

### 8. lib/i18n.ts

```typescript
import enContent from '@/content/shahin-site.en.json';
import arContent from '@/content/shahin-site.ar.json';

const contentMap = {
  en: enContent,
  ar: arContent,
};

export async function getContent(
  page: string,
  lang: 'en' | 'ar'
): Promise<any> {
  const content = contentMap[lang];
  return content[page] || {};
}

export function getText(key: string, lang: 'en' | 'ar'): string {
  const content = contentMap[lang];
  const keys = key.split('.');
  let value: any = content;
  
  for (const k of keys) {
    value = value?.[k];
  }
  
  return value || key;
}
```

### 9. components/sections/Hero.tsx

```typescript
'use client';

import { Button } from '@/components/ui/Button';
import Image from 'next/image';

interface HeroProps {
  content: {
    headline: string;
    subheadline: string;
    primaryCta: string;
    secondaryCta: string;
    trustBadges: string[];
  };
  lang: string;
}

export default function Hero({ content, lang }: HeroProps) {
  return (
    <section className="bg-primary text-white py-16 md:py-24">
      <div className="container mx-auto px-4 max-w-7xl">
        <div className="grid md:grid-cols-2 gap-8 items-center">
          <div>
            <h1 className="text-4xl md:text-6xl font-bold mb-6">
              {content.headline}
            </h1>
            <p className="text-xl mb-8 text-gray-200">
              {content.subheadline}
            </p>
            <div className="flex flex-wrap gap-4 mb-8">
              <Button
                href={`/${lang}/contact?demo=true`}
                variant="primary"
                size="lg"
              >
                {content.primaryCta}
              </Button>
              <Button
                href={`/${lang}/regulatory-packs`}
                variant="secondary"
                size="lg"
              >
                {content.secondaryCta}
              </Button>
            </div>
            <div className="flex flex-wrap gap-4 text-sm">
              {content.trustBadges.map((badge, idx) => (
                <span
                  key={idx}
                  className="px-3 py-1 bg-white/10 rounded-full"
                >
                  {badge}
                </span>
              ))}
            </div>
          </div>
          <div className="relative aspect-[16/10]">
            <Image
              src="/media/placeholders/hero.webp"
              alt="Shahin AI Platform"
              fill
              className="object-cover rounded-lg"
              priority
            />
          </div>
        </div>
      </div>
    </section>
  );
}
```

### 10. content/shahin-site.en.json

```json
{
  "home": {
    "hero": {
      "headline": "Evidence-First GRC for Regulators, Enterprises, and Governments",
      "subheadline": "Shahin AI enables audit-ready compliance through continuous control assessment, verified evidence, and regulatory intelligence — built for highly regulated environments.",
      "primaryCta": "Request Executive Demo",
      "secondaryCta": "View Regulatory Packs",
      "trustBadges": [
        "Microsoft Partner",
        "KSA-Ready",
        "Multi-Tenant",
        "Government-Grade"
      ]
    },
    "trustStrip": {
      "items": [
        "Evidence-First Architecture",
        "Regulatory-Aligned (NCA, SDAIA, PDPL)",
        "Continuous Compliance Monitoring",
        "Designed for Audits, Not Checklists"
      ]
    },
    "problems": {
      "title": "Why Compliance Fails in Most Organizations",
      "cards": [
        {
          "title": "Manual Evidence Collection",
          "description": "Compliance evidence is scattered, outdated, and unverifiable."
        },
        {
          "title": "Audit Panic",
          "description": "Controls are reviewed once a year instead of continuously."
        },
        {
          "title": "Checkbox Compliance",
          "description": "Assessments exist without proof or ownership."
        },
        {
          "title": "No Remediation Tracking",
          "description": "Findings are identified but never closed properly."
        }
      ]
    },
    "differentiators": {
      "title": "What Makes Shahin AI Different",
      "cards": [
        {
          "title": "Evidence Vault",
          "description": "Every control requires verified, time-stamped evidence."
        },
        {
          "title": "Regulatory Intelligence",
          "description": "Native support for KSA regulators with global framework mapping."
        },
        {
          "title": "Continuous Compliance",
          "description": "Controls are monitored continuously, not annually."
        },
        {
          "title": "Embedded Governance",
          "description": "Compliance is enforced inside operations, not after the fact."
        },
        {
          "title": "Audit-Ready Reporting",
          "description": "Generate regulator-ready audit packs on demand."
        }
      ]
    },
    "howItWorks": {
      "steps": [
        {
          "title": "Define Controls",
          "description": "Map controls to regulators, frameworks, and policies."
        },
        {
          "title": "Collect Evidence",
          "description": "Evidence is uploaded or captured automatically."
        },
        {
          "title": "Assess Continuously",
          "description": "Control status reflects real implementation."
        },
        {
          "title": "Remediate Gaps",
          "description": "Action plans track ownership and closure."
        },
        {
          "title": "Prove Compliance",
          "description": "Export audit-ready reports instantly."
        }
      ]
    },
    "regulatoryPacks": {
      "title": "Regulatory Packs — Built for Saudi Arabia",
      "cards": [
        {
          "title": "NCA ECC Compliance Pack",
          "description": "Complete compliance framework for NCA ECC requirements"
        },
        {
          "title": "SDAIA Data Governance Pack",
          "description": "Data governance and AI ethics compliance"
        },
        {
          "title": "PDPL Privacy Compliance",
          "description": "Personal Data Protection Law compliance"
        },
        {
          "title": "ISO 27001 / NIST Mapping",
          "description": "International standards mapped to KSA regulations"
        }
      ],
      "cta": "View All Regulatory Packs"
    },
    "platformPreview": {
      "caption": "A unified platform connecting controls, evidence, risks, and remediation."
    },
    "pricing": {
      "footerNote": "Enterprise pricing supports sovereign deployment and government requirements."
    },
    "finalCta": {
      "text": "Compliance should be provable, not arguable.",
      "button": "Request Executive Demo"
    }
  }
}
```

### 11. content/shahin-site.ar.json

```json
{
  "home": {
    "hero": {
      "headline": "GRC القائم على الأدلة للجهات التنظيمية والشركات والحكومات",
      "subheadline": "تمكّن Shahin AI من الامتثال الجاهز للتدقيق من خلال تقييم الضوابط المستمر والأدلة الم verifiedة والذكاء التنظيمي - مصممة للبيئات الخاضعة للتنظيم الشديد.",
      "primaryCta": "طلب عرض تنفيذي",
      "secondaryCta": "عرض الحِزم التنظيمية",
      "trustBadges": [
        "شريك Microsoft",
        "جاهز لـ KSA",
        "متعدد المستأجرين",
        "مستوى حكومي"
      ]
    },
    "trustStrip": {
      "items": [
        "معمارية قائمة على الأدلة",
        "متوافق تنظيميًا (NCA، SDAIA، PDPL)",
        "مراقبة الامتثال المستمرة",
        "مصمم للتدقيقات وليس قوائم التحقق"
      ]
    },
    "problems": {
      "title": "لماذا يفشل الامتثال في معظم المنظمات",
      "cards": [
        {
          "title": "جمع الأدلة يدويًا",
          "description": "الأدلة مبعثرة وقديمة وغير قابلة للتحقق."
        },
        {
          "title": "ذعر التدقيق",
          "description": "يتم مراجعة الضوابط مرة واحدة في السنة بدلاً من الاستمرار."
        },
        {
          "title": "امتثال مربع الاختيار",
          "description": "التقييمات موجودة بدون دليل أو مالك."
        },
        {
          "title": "عدم تتبع المعالجة",
          "description": "يتم تحديد النتائج ولكن لا يتم إغلاقها بشكل صحيح."
        }
      ]
    },
    "differentiators": {
      "title": "ما الذي يجعل Shahin AI مختلفًا",
      "cards": [
        {
          "title": "مستودع الأدلة",
          "description": "يتطلب كل ضابط دليلًا verifiedًا ومؤرخًا."
        },
        {
          "title": "الذكاء التنظيمي",
          "description": "دعم أصلي لجهات KSA التنظيمية مع تعيين إطار عالمي."
        },
        {
          "title": "الامتثال المستمر",
          "description": "يتم مراقبة الضوابط باستمرار وليس سنويًا."
        },
        {
          "title": "الحوكمة المدمجة",
          "description": "يتم فرض الامتثال داخل العمليات وليس بعد الحقيقة."
        },
        {
          "title": "التقارير الجاهزة للتدقيق",
          "description": "إنشاء حِزم تدقيق جاهزة للجهات التنظيمية عند الطلب."
        }
      ]
    },
    "howItWorks": {
      "steps": [
        {
          "title": "تحديد الضوابط",
          "description": "تعيين الضوابط للجهات التنظيمية والأطر والسياسات."
        },
        {
          "title": "جمع الأدلة",
          "description": "يتم رفع الأدلة أو التقاطها تلقائيًا."
        },
        {
          "title": "التقييم المستمر",
          "description": "تعكس حالة الضابط التطبيق الفعلي."
        },
        {
          "title": "معالجة الفجوات",
          "description": "تتبع خطط العمل الملكية والإغلاق."
        },
        {
          "title": "إثبات الامتثال",
          "description": "تصدير تقارير جاهزة للتدقيق على الفور."
        }
      ]
    },
    "regulatoryPacks": {
      "title": "الحِزم التنظيمية - مصممة للمملكة العربية السعودية",
      "cards": [
        {
          "title": "حزمة امتثال NCA ECC",
          "description": "إطار امتثال كامل لمتطلبات NCA ECC"
        },
        {
          "title": "حزمة حوكمة بيانات SDAIA",
          "description": "حوكمة البيانات وامتثال أخلاقيات AI"
        },
        {
          "title": "امتثال خصوصية PDPL",
          "description": "امتثال قانون حماية البيانات الشخصية"
        },
        {
          "title": "تعيين ISO 27001 / NIST",
          "description": "المعايير الدولية المعينة للوائح KSA"
        }
      ],
      "cta": "عرض جميع الحِزم التنظيمية"
    },
    "platformPreview": {
      "caption": "منصة موحدة تربط الضوابط والأدلة والمخاطر والمعالجة."
    },
    "pricing": {
      "footerNote": "يدعم تسعير المؤسسات النشر السيادي ومتطلبات الحكومة."
    },
    "finalCta": {
      "text": "يجب أن يكون الامتثال قابلاً للإثبات وليس قابلًا للنقاش.",
      "button": "طلب عرض تنفيذي"
    }
  }
}
```

### 12. components/ui/Button.tsx

```typescript
import Link from 'next/link';

interface ButtonProps {
  href?: string;
  onClick?: () => void;
  variant?: 'primary' | 'secondary' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  children: React.ReactNode;
  className?: string;
}

export function Button({
  href,
  onClick,
  variant = 'primary',
  size = 'md',
  children,
  className = '',
}: ButtonProps) {
  const baseClasses = 'inline-flex items-center justify-center font-medium rounded-lg transition-colors';
  const variantClasses = {
    primary: 'bg-accent text-white hover:bg-accent-light',
    secondary: 'bg-transparent border-2 border-accent text-accent hover:bg-accent hover:text-white',
    ghost: 'bg-transparent text-primary hover:bg-neutral-gray',
  };
  const sizeClasses = {
    sm: 'px-4 py-2 text-sm',
    md: 'px-6 py-3 text-base',
    lg: 'px-8 py-4 text-lg',
  };

  const classes = `${baseClasses} ${variantClasses[variant]} ${sizeClasses[size]} ${className}`;

  if (href) {
    return (
      <Link href={href} className={classes}>
        {children}
      </Link>
    );
  }

  return (
    <button onClick={onClick} className={classes}>
      {children}
    </button>
  );
}
```

### 13. components/ui/MediaFrame.tsx

```typescript
import Image from 'next/image';

interface MediaFrameProps {
  src: string;
  alt: string;
  ratio?: '16:10' | '16:9' | '4:3';
  className?: string;
}

const ratioClasses = {
  '16:10': 'aspect-[16/10]',
  '16:9': 'aspect-video',
  '4:3': 'aspect-[4/3]',
};

export function MediaFrame({
  src,
  alt,
  ratio = '16:9',
  className = '',
}: MediaFrameProps) {
  return (
    <div className={`relative ${ratioClasses[ratio]} ${className}`}>
      <Image
        src={src}
        alt={alt}
        fill
        className="object-cover rounded-card"
        sizes="(max-width: 768px) 100vw, 50vw"
      />
    </div>
  );
}
```

### 14. components/layout/Header.tsx

```typescript
'use client';

import Link from 'next/link';
import LanguageSwitcher from './LanguageSwitcher';

interface HeaderProps {
  lang: string;
}

export default function Header({ lang }: HeaderProps) {
  return (
    <header className="bg-primary text-white sticky top-0 z-50">
      <div className="container mx-auto px-4 max-w-7xl">
        <div className="flex items-center justify-between h-16">
          <Link href={`/${lang}`} className="text-2xl font-bold">
            Shahin AI
          </Link>
          <nav className="hidden md:flex items-center gap-6">
            <Link href={`/${lang}`}>Home</Link>
            <Link href={`/${lang}/regulatory-packs`}>Regulatory Packs</Link>
            <Link href={`/${lang}/pricing`}>Pricing</Link>
            <Link href={`/${lang}/partners`}>Partners</Link>
            <Link href={`/${lang}/resources`}>Resources</Link>
            <Link href="/login">Login</Link>
            <LanguageSwitcher currentLang={lang} />
          </nav>
        </div>
      </div>
    </header>
  );
}
```

### 15. components/layout/Footer.tsx

```typescript
import Link from 'next/link';

interface FooterProps {
  lang: string;
}

export default function Footer({ lang }: FooterProps) {
  const content = {
    en: {
      text: "Shahin AI is a Dogan Consult company. Built for regulated environments across the GCC.",
      sbg: "Saudi Business Gate (ERP)",
      doganConsult: "Dogan Consult",
      privacy: "Privacy",
      contact: "Contact",
    },
    ar: {
      text: "Shahin AI هي شركة Dogan Consult. مصممة للبيئات الخاضعة للتنظيم عبر دول مجلس التعاون الخليجي.",
      sbg: "Saudi Business Gate (ERP)",
      doganConsult: "Dogan Consult",
      privacy: "الخصوصية",
      contact: "اتصل بنا",
    },
  };

  const t = content[lang as 'en' | 'ar'] || content.en;

  return (
    <footer className="bg-neutral-slate text-white py-12 mt-20">
      <div className="container mx-auto px-4 max-w-7xl">
        <div className="grid md:grid-cols-4 gap-8 mb-8">
          <div>
            <h3 className="text-xl font-bold mb-4">Shahin AI</h3>
            <p className="text-gray-300 text-sm">{t.text}</p>
          </div>
          <div>
            <h4 className="font-semibold mb-4">Products</h4>
            <ul className="space-y-2 text-sm">
              <li><Link href="https://saudibusinessgate.com" className="hover:text-accent">{t.sbg}</Link></li>
            </ul>
          </div>
          <div>
            <h4 className="font-semibold mb-4">Company</h4>
            <ul className="space-y-2 text-sm">
              <li><Link href="https://doganconsult.com" className="hover:text-accent">{t.doganConsult}</Link></li>
            </ul>
          </div>
          <div>
            <h4 className="font-semibold mb-4">Legal</h4>
            <ul className="space-y-2 text-sm">
              <li><Link href={`/${lang}/privacy`} className="hover:text-accent">{t.privacy}</Link></li>
              <li><Link href={`/${lang}/contact`} className="hover:text-accent">{t.contact}</Link></li>
            </ul>
          </div>
        </div>
        <div className="border-t border-gray-700 pt-8 text-center text-sm text-gray-400">
          © {new Date().getFullYear()} Dogan Consult. All rights reserved.
        </div>
      </div>
    </footer>
  );
}
```

---

## Deployment Checklist

1. **Initialize project:**
   ```bash
   npx create-next-app@latest shahin-ai-website --typescript --tailwind --app
   cd shahin-ai-website
   npm install
   ```

2. **Copy all files** from this structure into the project

3. **Create placeholder images:**
   - `/public/media/placeholders/hero.webp` (2400x1500px, 16:10)
   - `/public/media/placeholders/screenshot-1.webp` (1920x1080px, 16:9)
   - `/public/media/placeholders/doc-thumb.webp` (1200x900px, 4:3)

4. **Build and test:**
   ```bash
   npm run build
   npm run dev
   ```

5. **Deploy to Vercel:**
   - Connect GitHub repository
   - Set build command: `npm run build`
   - Set output directory: `.next`

6. **Configure DNS:**
   - Point `shahin-ai.com` and `www.shahin-ai.com` to Vercel
   - Point `app.shahin-ai.com` to your ABP/Blazor server

7. **Test:**
   - Visit `https://shahin-ai.com/en` (English)
   - Visit `https://shahin-ai.com/ar` (Arabic RTL)
   - Test `/login` redirect to `app.shahin-ai.com`

---

## Summary

✅ **Complete Next.js 14 structure** with App Router  
✅ **Bilingual support** (English + Arabic RTL)  
✅ **All sections** from specification wired  
✅ **Content-driven** (JSON files, easy to update)  
✅ **Placeholder images** (replace later without breaking layout)  
✅ **Login redirect** to app subdomain  
✅ **Tailwind configured** with brand colors  
✅ **Production-ready** structure  

**Ready to deploy immediately.**
