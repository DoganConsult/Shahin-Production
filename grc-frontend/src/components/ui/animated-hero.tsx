"use client"

import * as React from "react"
import { cn } from "@/lib/utils"
import { ArrowRight, Sparkles, Shield, TrendingUp, CheckCircle } from "lucide-react"

/**
 * Animated Hero Components - Catchy UI Effects
 *
 * A collection of eye-catching hero section components
 * with modern animations and visual effects.
 */

// Animated Background Blobs
export function AnimatedBlobs({ className }: { className?: string }) {
  return (
    <div className={cn("absolute inset-0 overflow-hidden -z-10", className)}>
      <div className="blob blob-1 top-0 -left-20" />
      <div className="blob blob-2 top-1/3 -right-32" />
      <div className="blob blob-3 bottom-0 left-1/3" />
    </div>
  )
}

// Floating Icons
export function FloatingIcons({ className }: { className?: string }) {
  return (
    <div className={cn("absolute inset-0 overflow-hidden pointer-events-none", className)}>
      <div className="absolute top-20 left-[10%] float-slow">
        <div className="w-12 h-12 rounded-xl bg-emerald-500/20 backdrop-blur-sm flex items-center justify-center">
          <Shield className="w-6 h-6 text-emerald-600" />
        </div>
      </div>
      <div className="absolute top-40 right-[15%] float-medium">
        <div className="w-10 h-10 rounded-full bg-teal-500/20 backdrop-blur-sm flex items-center justify-center">
          <TrendingUp className="w-5 h-5 text-teal-600" />
        </div>
      </div>
      <div className="absolute bottom-32 left-[20%] float-fast">
        <div className="w-14 h-14 rounded-2xl bg-cyan-500/20 backdrop-blur-sm flex items-center justify-center">
          <CheckCircle className="w-7 h-7 text-cyan-600" />
        </div>
      </div>
      <div className="absolute bottom-20 right-[25%] float-slow" style={{ animationDelay: "-2s" }}>
        <div className="w-8 h-8 rounded-lg bg-purple-500/20 backdrop-blur-sm flex items-center justify-center">
          <Sparkles className="w-4 h-4 text-purple-600" />
        </div>
      </div>
    </div>
  )
}

// Catchy Hero Section
interface HeroSectionProps {
  title: string
  titleHighlight?: string
  subtitle: string
  ctaText?: string
  ctaHref?: string
  secondaryCtaText?: string
  secondaryCtaHref?: string
  className?: string
}

export const HeroSection = React.memo(function HeroSection({
  title,
  titleHighlight,
  subtitle,
  ctaText = "Get Started",
  ctaHref = "/trial",
  secondaryCtaText = "Learn More",
  secondaryCtaHref = "/about",
  className,
}: HeroSectionProps) {
  return (
    <section className={cn("hero-catchy py-20 md:py-32", className)}>
      <AnimatedBlobs />
      <FloatingIcons />

      <div className="container mx-auto px-4 relative z-10">
        <div className="max-w-4xl mx-auto text-center">
          {/* Badge */}
          <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-white/80 backdrop-blur-sm border border-emerald-200 mb-8 animate-fade-in-down">
            <Sparkles className="w-4 h-4 text-emerald-500 icon-pulse" />
            <span className="text-sm font-medium text-emerald-700">
              Platform GRC الأحدث في المملكة
            </span>
          </div>

          {/* Title */}
          <h1 className="text-4xl md:text-6xl lg:text-7xl font-bold text-gray-900 mb-6 animate-fade-in-up">
            {title}{" "}
            {titleHighlight && (
              <span className="text-gradient">{titleHighlight}</span>
            )}
          </h1>

          {/* Subtitle */}
          <p className="text-xl md:text-2xl text-gray-600 mb-10 max-w-2xl mx-auto animate-fade-in-up" style={{ animationDelay: "0.2s" }}>
            {subtitle}
          </p>

          {/* CTA Buttons */}
          <div className="flex flex-col sm:flex-row gap-4 justify-center animate-fade-in-up" style={{ animationDelay: "0.4s" }}>
            <a href={ctaHref} className="btn-glow inline-flex items-center justify-center gap-2 group">
              {ctaText}
              <ArrowRight className="w-5 h-5 transition-transform group-hover:translate-x-1" />
            </a>
            <a href={secondaryCtaHref} className="btn-enterprise-outline shine-hover">
              {secondaryCtaText}
            </a>
          </div>

          {/* Trust Badges */}
          <div className="mt-16 flex flex-wrap justify-center gap-8 items-center opacity-60 animate-fade-in-up" style={{ animationDelay: "0.6s" }}>
            <div className="text-sm text-gray-500">Trusted by 100+ organizations</div>
            <div className="flex gap-4">
              <div className="w-8 h-8 rounded bg-gray-200" />
              <div className="w-8 h-8 rounded bg-gray-200" />
              <div className="w-8 h-8 rounded bg-gray-200" />
            </div>
          </div>
        </div>
      </div>
    </section>
  )
})

// Feature Card with Hover Effects
interface FeatureCardProps {
  icon: React.ReactNode
  title: string
  description: string
  href?: string
  className?: string
}

export const FeatureCard = React.memo(function FeatureCard({
  icon,
  title,
  description,
  href,
  className,
}: FeatureCardProps) {
  const Wrapper = href ? "a" : "div"

  return (
    <Wrapper
      href={href}
      className={cn(
        "card-interactive card-lift p-8 block",
        className
      )}
    >
      {/* Icon */}
      <div className="w-14 h-14 rounded-2xl bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center mb-6 icon-bounce">
        <span className="text-white">{icon}</span>
      </div>

      {/* Content */}
      <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-3">
        {title}
      </h3>
      <p className="text-gray-600 dark:text-gray-400">
        {description}
      </p>

      {/* Arrow indicator */}
      {href && (
        <div className="mt-6 flex items-center text-emerald-600 font-medium">
          <span>Learn more</span>
          <ArrowRight className="w-4 h-4 ms-2 transition-transform group-hover:translate-x-1" />
        </div>
      )}
    </Wrapper>
  )
})

// Stats with Animation
interface StatCardProps {
  value: string
  label: string
  suffix?: string
  className?: string
}

export const StatCard = React.memo(function StatCard({
  value,
  label,
  suffix,
  className,
}: StatCardProps) {
  return (
    <div className={cn("stat-animated text-center p-6", className)}>
      <div className="text-4xl md:text-5xl font-bold text-gradient mb-2">
        {value}
        {suffix && <span className="text-2xl text-emerald-500">{suffix}</span>}
      </div>
      <div className="text-gray-600 dark:text-gray-400 font-medium">
        {label}
      </div>
    </div>
  )
})

// Animated Gradient Button
interface GradientButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  children: React.ReactNode
  variant?: "glow" | "pulse" | "shine"
  size?: "sm" | "md" | "lg"
}

export const GradientButton = React.memo(function GradientButton({
  children,
  variant = "glow",
  size = "md",
  className,
  ...props
}: GradientButtonProps) {
  const sizeClasses = {
    sm: "px-4 py-2 text-sm",
    md: "px-6 py-3 text-base",
    lg: "px-8 py-4 text-lg",
  }

  const variantClasses = {
    glow: "btn-glow",
    pulse: "btn-glow btn-pulse",
    shine: "btn-glow sparkle",
  }

  return (
    <button
      className={cn(
        variantClasses[variant],
        sizeClasses[size],
        className
      )}
      {...props}
    >
      {children}
    </button>
  )
})

// Animated Card with Gradient Border
interface GradientBorderCardProps {
  children: React.ReactNode
  className?: string
}

export const GradientBorderCard = React.memo(function GradientBorderCard({
  children,
  className,
}: GradientBorderCardProps) {
  return (
    <div className={cn("border-gradient-animated", className)}>
      <div className="p-6">{children}</div>
    </div>
  )
})

// Loader Components
export function LoaderDots({ className }: { className?: string }) {
  return (
    <div className={cn("loader-dots", className)}>
      <span />
      <span />
      <span />
    </div>
  )
}

export function LoaderSpinner({ className }: { className?: string }) {
  return <div className={cn("loader-spinner", className)} />
}

// Success Animation
export function SuccessCheckmark({ className }: { className?: string }) {
  return (
    <div className={cn("success-checkmark", className)}>
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3">
        <path d="M5 13l4 4L19 7" strokeLinecap="round" strokeLinejoin="round" />
      </svg>
    </div>
  )
}

export default HeroSection
