import { Navbar } from "@/components/layout/Navbar"
import { Footer } from "@/components/layout/Footer"
import { Hero } from "@/components/landing/Hero"
import { TrustStrip } from "@/components/landing/TrustStrip"
import { Stats } from "@/components/landing/Stats"
import { ProblemCards } from "@/components/landing/ProblemCards"
import { Features } from "@/components/landing/Features"
import { Differentiators } from "@/components/landing/Differentiators"
import { HowItWorks } from "@/components/landing/HowItWorks"
import { Regulators } from "@/components/landing/Regulators"
import { PlatformPreview } from "@/components/landing/PlatformPreview"
import { Testimonials } from "@/components/landing/Testimonials"
import { CTA } from "@/components/landing/CTA"

export default function HomePage() {
  return (
    <main className="min-h-screen">
      <Navbar />
      
      {/* Hero Section */}
      <Hero />
      
      {/* Trust Strip - Partner Logos & Certifications */}
      <TrustStrip />
      
      {/* Stats Banner */}
      <Stats />
      
      {/* Problem Cards - Challenges */}
      <ProblemCards />
      
      {/* Features Grid */}
      <section id="features">
        <Features />
      </section>
      
      {/* Differentiators - Why Shahin */}
      <Differentiators />
      
      {/* How It Works */}
      <section id="how-it-works">
        <HowItWorks />
      </section>
      
      {/* Regulatory Frameworks */}
      <section id="regulators">
        <Regulators />
      </section>
      
      {/* Platform Preview */}
      <PlatformPreview />
      
      {/* Testimonials */}
      <Testimonials />
      
      {/* Final CTA */}
      <CTA />
      
      <Footer />
    </main>
  )
}
