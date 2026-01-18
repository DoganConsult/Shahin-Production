"use client"

import { Navigation } from "@/components/sections/Navigation"
import { Hero } from "@/components/sections/Hero"
import { Expertise } from "@/components/sections/Expertise"
import { Approach } from "@/components/sections/Approach"
import { Sectors } from "@/components/sections/Sectors"
import { WhyUs } from "@/components/sections/WhyUs"
import { CTA } from "@/components/sections/CTA"
import { Footer } from "@/components/sections/Footer"

export default function HomePage() {
  return (
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
  )
}
