"use client"

import * as React from "react"
import { cn } from "@/lib/utils"
import { ArrowRight, Play, Shield, TrendingUp, Award, ChevronDown } from "lucide-react"
import {
  TextReveal,
  MagneticButton,
  GradientText,
  AnimatedCounter,
  StaggerContainer,
  ScrollProgress,
  ParallaxContainer,
  ParallaxLayer,
  SpotlightCard,
  TiltCard,
} from "./premium-effects"

/**
 * Premium Hero Section - High-End Enterprise Landing
 *
 * Award-winning hero section with:
 * - Animated text reveals
 * - Magnetic buttons
 * - 3D floating elements
 * - Parallax backgrounds
 * - Trust signals
 */

// ============================================================================
// PREMIUM HERO SECTION
// ============================================================================

interface PremiumHeroProps {
  badge?: string
  title: string
  titleHighlight?: string
  subtitle: string
  ctaText?: string
  ctaHref?: string
  secondaryCtaText?: string
  secondaryCtaHref?: string
  showVideo?: boolean
  videoId?: string
  stats?: Array<{ value: number; label: string; suffix?: string }>
  trustedBy?: string[]
  className?: string
}

export const PremiumHero = React.memo(function PremiumHero({
  badge = "Introducing the Future of GRC",
  title,
  titleHighlight,
  subtitle,
  ctaText = "Start Free Trial",
  ctaHref = "/trial",
  secondaryCtaText = "Watch Demo",
  secondaryCtaHref,
  showVideo = false,
  videoId,
  stats,
  trustedBy,
  className,
}: PremiumHeroProps) {
  return (
    <section className={cn("relative min-h-screen overflow-hidden", className)}>
      <ScrollProgress />

      {/* Background Effects */}
      <PremiumHeroBackground />

      {/* Content */}
      <div className="relative z-10 container mx-auto px-6 pt-32 pb-20">
        <div className="max-w-5xl mx-auto">
          {/* Badge */}
          <div className="flex justify-center mb-8 animate-fade-in-down">
            <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-white/80 dark:bg-gray-900/80 backdrop-blur-xl border border-gray-200/50 dark:border-gray-700/50 shadow-lg shadow-emerald-500/10">
              <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                {badge}
              </span>
            </span>
          </div>

          {/* Title */}
          <div className="text-center mb-8">
            <TextReveal
              text={title}
              as="h1"
              className="text-5xl md:text-6xl lg:text-7xl font-bold text-gray-900 dark:text-white leading-tight"
              delay={0.2}
            />
            {titleHighlight && (
              <div className="mt-2">
                <GradientText
                  className="text-5xl md:text-6xl lg:text-7xl font-bold"
                  animate
                >
                  {titleHighlight}
                </GradientText>
              </div>
            )}
          </div>

          {/* Subtitle */}
          <div className="text-center mb-12 animate-fade-in-up" style={{ animationDelay: "0.4s" }}>
            <p className="text-xl md:text-2xl text-gray-600 dark:text-gray-400 max-w-3xl mx-auto leading-relaxed">
              {subtitle}
            </p>
          </div>

          {/* CTA Buttons */}
          <div
            className="flex flex-col sm:flex-row items-center justify-center gap-4 mb-16 animate-fade-in-up"
            style={{ animationDelay: "0.6s" }}
          >
            <MagneticButton
              variant="primary"
              size="lg"
              onClick={() => window.location.href = ctaHref}
            >
              {ctaText}
              <ArrowRight className="w-5 h-5" />
            </MagneticButton>

            {secondaryCtaHref && (
              <MagneticButton
                variant="outline"
                size="lg"
                onClick={() => window.location.href = secondaryCtaHref}
              >
                {showVideo && <Play className="w-5 h-5" />}
                {secondaryCtaText}
              </MagneticButton>
            )}
          </div>

          {/* Stats */}
          {stats && stats.length > 0 && (
            <div className="grid grid-cols-2 md:grid-cols-4 gap-8 mb-16">
              <StaggerContainer className="contents" staggerDelay={0.15}>
                {stats.map((stat, i) => (
                  <div key={i} className="text-center">
                    <div className="text-4xl md:text-5xl font-bold text-gradient mb-2">
                      <AnimatedCounter
                        end={stat.value}
                        suffix={stat.suffix}
                        duration={2500}
                      />
                    </div>
                    <div className="text-gray-600 dark:text-gray-400 font-medium">
                      {stat.label}
                    </div>
                  </div>
                ))}
              </StaggerContainer>
            </div>
          )}

          {/* Trusted By */}
          {trustedBy && trustedBy.length > 0 && (
            <div className="text-center animate-fade-in-up" style={{ animationDelay: "0.8s" }}>
              <p className="text-sm text-gray-500 dark:text-gray-400 mb-6 uppercase tracking-wider font-medium">
                Trusted by industry leaders
              </p>
              <div className="flex flex-wrap items-center justify-center gap-8 md:gap-12 opacity-60">
                {trustedBy.map((company, i) => (
                  <span
                    key={i}
                    className="text-gray-400 font-semibold text-lg tracking-wide"
                  >
                    {company}
                  </span>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Scroll Indicator */}
        <div className="absolute bottom-8 left-1/2 -translate-x-1/2 animate-bounce">
          <ChevronDown className="w-6 h-6 text-gray-400" />
        </div>
      </div>
    </section>
  )
})

// ============================================================================
// PREMIUM HERO BACKGROUND
// ============================================================================

function PremiumHeroBackground() {
  return (
    <ParallaxContainer className="absolute inset-0">
      {/* Gradient Base */}
      <div className="absolute inset-0 bg-gradient-to-br from-gray-50 via-emerald-50/30 to-cyan-50/20 dark:from-gray-950 dark:via-gray-900 dark:to-gray-950" />

      {/* Grid Pattern */}
      <div
        className="absolute inset-0 opacity-[0.03] dark:opacity-[0.05]"
        style={{
          backgroundImage: `
            linear-gradient(to right, #10b981 1px, transparent 1px),
            linear-gradient(to bottom, #10b981 1px, transparent 1px)
          `,
          backgroundSize: "60px 60px",
        }}
      />

      {/* Floating Orbs */}
      <ParallaxLayer speed={0.2} className="absolute inset-0">
        <div className="absolute top-[20%] left-[10%] w-96 h-96 rounded-full bg-emerald-400/20 blur-3xl" />
        <div className="absolute top-[60%] right-[15%] w-80 h-80 rounded-full bg-cyan-400/20 blur-3xl" />
        <div className="absolute bottom-[10%] left-[30%] w-72 h-72 rounded-full bg-teal-400/15 blur-3xl" />
      </ParallaxLayer>

      {/* Floating Shapes */}
      <ParallaxLayer speed={0.4} className="absolute inset-0 pointer-events-none">
        <FloatingShape
          className="absolute top-[15%] right-[20%]"
          icon={<Shield className="w-6 h-6 text-emerald-600" />}
          delay={0}
        />
        <FloatingShape
          className="absolute top-[45%] left-[8%]"
          icon={<TrendingUp className="w-6 h-6 text-cyan-600" />}
          delay={1}
        />
        <FloatingShape
          className="absolute bottom-[25%] right-[12%]"
          icon={<Award className="w-6 h-6 text-teal-600" />}
          delay={2}
        />
      </ParallaxLayer>

      {/* Radial Gradient Overlay */}
      <div className="absolute inset-0 bg-gradient-radial from-transparent via-transparent to-white/50 dark:to-gray-950/50" />
    </ParallaxContainer>
  )
}

// ============================================================================
// FLOATING SHAPE
// ============================================================================

interface FloatingShapeProps {
  className?: string
  icon: React.ReactNode
  delay?: number
}

const FloatingShape = React.memo(function FloatingShape({
  className,
  icon,
  delay = 0,
}: FloatingShapeProps) {
  return (
    <div
      className={cn(
        "w-14 h-14 rounded-2xl glass-card flex items-center justify-center",
        "shadow-lg shadow-emerald-500/10",
        "animate-float",
        className
      )}
      style={{
        animationDelay: `${delay}s`,
        animationDuration: "6s",
      }}
    >
      {icon}
    </div>
  )
})

// ============================================================================
// PREMIUM FEATURES SECTION
// ============================================================================

interface Feature {
  icon: React.ReactNode
  title: string
  description: string
  href?: string
}

interface PremiumFeaturesProps {
  title?: string
  subtitle?: string
  features: Feature[]
  className?: string
}

export const PremiumFeatures = React.memo(function PremiumFeatures({
  title = "Everything you need",
  subtitle = "Comprehensive GRC platform built for enterprise scale",
  features,
  className,
}: PremiumFeaturesProps) {
  return (
    <section className={cn("py-24 bg-white dark:bg-gray-950", className)}>
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <div className="text-center mb-16">
          <TextReveal
            text={title}
            as="h2"
            className="text-4xl md:text-5xl font-bold text-gray-900 dark:text-white mb-4"
          />
          <p className="text-xl text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
            {subtitle}
          </p>
        </div>

        {/* Features Grid */}
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
          <StaggerContainer className="contents" staggerDelay={0.1}>
            {features.map((feature, i) => (
              <TiltCard key={i} className="p-8" tiltAmount={8}>
                <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center mb-6 shadow-lg shadow-emerald-500/30">
                  <span className="text-white">{feature.icon}</span>
                </div>
                <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-3">
                  {feature.title}
                </h3>
                <p className="text-gray-600 dark:text-gray-400 leading-relaxed">
                  {feature.description}
                </p>
                {feature.href && (
                  <a
                    href={feature.href}
                    className="inline-flex items-center gap-2 mt-4 text-emerald-600 dark:text-emerald-400 font-medium underline-premium"
                  >
                    Learn more
                    <ArrowRight className="w-4 h-4" />
                  </a>
                )}
              </TiltCard>
            ))}
          </StaggerContainer>
        </div>
      </div>
    </section>
  )
})

// ============================================================================
// PREMIUM STATS SECTION
// ============================================================================

interface Stat {
  value: number
  label: string
  prefix?: string
  suffix?: string
}

interface PremiumStatsProps {
  stats: Stat[]
  className?: string
}

export const PremiumStats = React.memo(function PremiumStats({
  stats,
  className,
}: PremiumStatsProps) {
  return (
    <section className={cn("py-20 bg-gray-900", className)}>
      <div className="container mx-auto px-6">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-8 md:gap-12">
          {stats.map((stat, i) => (
            <SpotlightCard key={i} className="p-8 bg-gray-800/50 border-gray-700/50">
              <div className="text-center">
                <div className="text-4xl md:text-5xl font-bold text-white mb-2">
                  {stat.prefix}
                  <AnimatedCounter end={stat.value} duration={2000} />
                  {stat.suffix}
                </div>
                <div className="text-gray-400 font-medium">{stat.label}</div>
              </div>
            </SpotlightCard>
          ))}
        </div>
      </div>
    </section>
  )
})

// ============================================================================
// PREMIUM CTA SECTION
// ============================================================================

interface PremiumCtaProps {
  title?: string
  subtitle?: string
  ctaText?: string
  ctaHref?: string
  className?: string
}

export const PremiumCta = React.memo(function PremiumCta({
  title = "Ready to get started?",
  subtitle = "Join thousands of organizations using Shahin AI",
  ctaText = "Start Free Trial",
  ctaHref = "/trial",
  className,
}: PremiumCtaProps) {
  return (
    <section className={cn("py-24 relative overflow-hidden", className)}>
      {/* Background */}
      <div className="absolute inset-0 bg-gradient-to-br from-emerald-600 via-teal-600 to-cyan-600" />
      <div className="absolute inset-0 bg-[url('/grid.svg')] opacity-10" />

      {/* Content */}
      <div className="relative z-10 container mx-auto px-6 text-center">
        <h2 className="text-4xl md:text-5xl font-bold text-white mb-4">
          {title}
        </h2>
        <p className="text-xl text-white/80 mb-10 max-w-2xl mx-auto">
          {subtitle}
        </p>
        <MagneticButton
          variant="secondary"
          size="lg"
          onClick={() => window.location.href = ctaHref}
        >
          {ctaText}
          <ArrowRight className="w-5 h-5" />
        </MagneticButton>
      </div>
    </section>
  )
})

// ============================================================================
// ADDITIONAL STYLES (Add to globals.css)
// ============================================================================

/*
@keyframes float {
  0%, 100% {
    transform: translateY(0) rotate(0deg);
  }
  50% {
    transform: translateY(-20px) rotate(5deg);
  }
}

.animate-float {
  animation: float 6s ease-in-out infinite;
}

.bg-gradient-radial {
  background: radial-gradient(circle at center, var(--tw-gradient-stops));
}
*/

export default PremiumHero
