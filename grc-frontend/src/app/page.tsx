// Force dynamic rendering to support client-side providers
export const dynamic = 'force-dynamic'

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
import { UseCaseSwitcher } from "@/components/landing/UseCaseSwitcher"
import { ROICalculator } from "@/components/landing/ROICalculator"
import { TrustPanel } from "@/components/landing/TrustPanel"
import { LeadForm } from "@/components/landing/LeadForm"
import { AgentPanel } from "@/components/landing/AgentPanel"
import { WorkflowInbox } from "@/components/landing/WorkflowInbox"
import { IntegrationGallery } from "@/components/landing/IntegrationGallery"
import { PricingComparator } from "@/components/landing/PricingComparator"
import { BookingWidget } from "@/components/landing/BookingWidget"
import { AIAgentsShowcase } from "@/components/landing/AIAgentsShowcase"
import { AIDemo } from "@/components/landing/AIDemo"
import { AIIntegrationsShowcase } from "@/components/landing/AIIntegrationsShowcase"

export default function HomePage() {
  return (
    <main className="min-h-screen">
      <Navbar />

      {/* Hero Section */}
      <Hero />

      {/* Trust Strip - Platform Launch & Certifications */}
      <TrustStrip />

      {/* Stats Banner */}
      <Stats />

      {/* Problem Cards - Challenges */}
      <ProblemCards />

      {/* Industry Solutions - Use Case Switcher */}
      <section id="solutions">
        <UseCaseSwitcher />
      </section>

      {/* AI Safety - Propose vs Execute (Key Differentiation) */}
      <section id="ai-safety">
        <AgentPanel />
      </section>

      {/* Full AI Team Showcase - All 12 Agents */}
      <section id="ai-agents">
        <AIAgentsShowcase />
      </section>

      {/* AI Demo - Dry Run & Real Results */}
      <section id="ai-demo">
        <AIDemo />
      </section>

      {/* AI Integrations - Azure Bot, Copilot, Claude */}
      <section id="ai-integrations">
        <AIIntegrationsShowcase />
      </section>

      {/* Workflow Inbox - Operational Control */}
      <section id="workflow">
        <WorkflowInbox />
      </section>

      {/* Features Grid */}
      <section id="features">
        <Features />
      </section>

      {/* ERP Integrations Gallery */}
      <section id="integrations">
        <IntegrationGallery />
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

      {/* ROI Calculator - Calculate Your Savings */}
      <section id="roi">
        <ROICalculator />
      </section>

      {/* Security & Trust Panel */}
      <section id="security">
        <TrustPanel />
      </section>

      {/* Pricing - SaaS vs On-Premise */}
      <section id="pricing">
        <PricingComparator />
      </section>

      {/* Testimonials / New to Market */}
      <Testimonials />

      {/* Schedule Demo - Qualified Booking */}
      <section id="book-demo">
        <BookingWidget />
      </section>

      {/* Quick CTA */}
      <CTA />

      {/* Lead Capture Form - Final Conversion */}
      <section id="demo">
        <LeadForm />
      </section>

      <Footer />
    </main>
  )
}
