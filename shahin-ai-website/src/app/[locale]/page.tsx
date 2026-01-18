"use client"

import { Navigation } from "@/components/sections/Navigation"
import { Hero } from "@/components/sections/Hero"
import { Expertise } from "@/components/sections/Expertise"
import { Approach } from "@/components/sections/Approach"
import { Sectors } from "@/components/sections/Sectors"
import { WhyUs } from "@/components/sections/WhyUs"
import { CTA } from "@/components/sections/CTA"
import { Footer } from "@/components/sections/Footer"
import { OrganizationJsonLd, WebsiteJsonLd } from "@/components/seo/JsonLd"
import { useLocale } from "next-intl"

export default function HomePage() {
  const locale = useLocale()
  
  return (
    <>
      <OrganizationJsonLd locale={locale as string} />
      <WebsiteJsonLd locale={locale as string} />
      <main className="min-h-screen">
        <Navigation />
        <Hero />
        <Expertise />
        <Approach />
        <Sectors />
        <WhyUs />
        <CTA />
        <Footer />
      </main>
    </>
  )
}
