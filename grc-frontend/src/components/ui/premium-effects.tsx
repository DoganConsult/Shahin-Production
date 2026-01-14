"use client"

import * as React from "react"
import { cn } from "@/lib/utils"

/**
 * Premium Effects - High-End Enterprise UI Components
 *
 * Award-winning visual effects for world-class web experiences.
 * Inspired by Apple, Stripe, Linear, and Awwwards winners.
 */

// ============================================================================
// 1. CUSTOM CURSOR WITH SPOTLIGHT EFFECT
// ============================================================================

interface CustomCursorProps {
  children: React.ReactNode
  className?: string
  spotlightColor?: string
  cursorSize?: number
}

export function CustomCursorProvider({
  children,
  className,
  spotlightColor = "rgba(16, 185, 129, 0.15)",
  cursorSize = 400,
}: CustomCursorProps) {
  const containerRef = React.useRef<HTMLDivElement>(null)
  const spotlightRef = React.useRef<HTMLDivElement>(null)
  const cursorRef = React.useRef<HTMLDivElement>(null)

  React.useEffect(() => {
    const container = containerRef.current
    const spotlight = spotlightRef.current
    const cursor = cursorRef.current
    if (!container || !spotlight || !cursor) return

    const handleMouseMove = (e: MouseEvent) => {
      const { clientX, clientY } = e

      // Update spotlight
      spotlight.style.background = `radial-gradient(${cursorSize}px circle at ${clientX}px ${clientY}px, ${spotlightColor}, transparent 80%)`

      // Update custom cursor
      cursor.style.left = `${clientX}px`
      cursor.style.top = `${clientY}px`
    }

    const handleMouseEnter = () => {
      cursor.style.opacity = "1"
      spotlight.style.opacity = "1"
    }

    const handleMouseLeave = () => {
      cursor.style.opacity = "0"
      spotlight.style.opacity = "0"
    }

    container.addEventListener("mousemove", handleMouseMove)
    container.addEventListener("mouseenter", handleMouseEnter)
    container.addEventListener("mouseleave", handleMouseLeave)

    return () => {
      container.removeEventListener("mousemove", handleMouseMove)
      container.removeEventListener("mouseenter", handleMouseEnter)
      container.removeEventListener("mouseleave", handleMouseLeave)
    }
  }, [spotlightColor, cursorSize])

  return (
    <div ref={containerRef} className={cn("relative", className)}>
      {/* Spotlight overlay */}
      <div
        ref={spotlightRef}
        className="pointer-events-none fixed inset-0 z-30 transition-opacity duration-300 opacity-0"
        aria-hidden="true"
      />
      {/* Custom cursor dot */}
      <div
        ref={cursorRef}
        className="pointer-events-none fixed z-50 -translate-x-1/2 -translate-y-1/2 transition-opacity duration-300 opacity-0"
        aria-hidden="true"
      >
        <div className="w-4 h-4 rounded-full bg-emerald-500 mix-blend-difference" />
        <div className="absolute inset-0 w-4 h-4 rounded-full bg-emerald-500 animate-ping opacity-50" />
      </div>
      {children}
    </div>
  )
}

// ============================================================================
// 2. 3D TILT CARD WITH MOUSE TRACKING
// ============================================================================

interface TiltCardProps {
  children: React.ReactNode
  className?: string
  tiltAmount?: number
  glareEnabled?: boolean
  perspective?: number
}

export const TiltCard = React.memo(function TiltCard({
  children,
  className,
  tiltAmount = 15,
  glareEnabled = true,
  perspective = 1000,
}: TiltCardProps) {
  const cardRef = React.useRef<HTMLDivElement>(null)
  const glareRef = React.useRef<HTMLDivElement>(null)

  const handleMouseMove = React.useCallback((e: React.MouseEvent<HTMLDivElement>) => {
    const card = cardRef.current
    const glare = glareRef.current
    if (!card) return

    const rect = card.getBoundingClientRect()
    const x = e.clientX - rect.left
    const y = e.clientY - rect.top
    const centerX = rect.width / 2
    const centerY = rect.height / 2

    const rotateX = ((y - centerY) / centerY) * -tiltAmount
    const rotateY = ((x - centerX) / centerX) * tiltAmount

    card.style.transform = `perspective(${perspective}px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) scale3d(1.02, 1.02, 1.02)`

    if (glare && glareEnabled) {
      const glareX = (x / rect.width) * 100
      const glareY = (y / rect.height) * 100
      glare.style.background = `radial-gradient(circle at ${glareX}% ${glareY}%, rgba(255,255,255,0.3) 0%, transparent 80%)`
    }
  }, [tiltAmount, perspective, glareEnabled])

  const handleMouseLeave = React.useCallback(() => {
    const card = cardRef.current
    const glare = glareRef.current
    if (card) {
      card.style.transform = `perspective(${perspective}px) rotateX(0deg) rotateY(0deg) scale3d(1, 1, 1)`
    }
    if (glare) {
      glare.style.background = "transparent"
    }
  }, [perspective])

  return (
    <div
      ref={cardRef}
      onMouseMove={handleMouseMove}
      onMouseLeave={handleMouseLeave}
      className={cn(
        "relative rounded-2xl overflow-hidden transition-transform duration-200 ease-out",
        "bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800",
        "shadow-xl hover:shadow-2xl",
        className
      )}
      style={{ transformStyle: "preserve-3d" }}
    >
      {glareEnabled && (
        <div
          ref={glareRef}
          className="absolute inset-0 pointer-events-none z-10 transition-all duration-200"
          aria-hidden="true"
        />
      )}
      <div style={{ transform: "translateZ(50px)" }}>
        {children}
      </div>
    </div>
  )
})

// ============================================================================
// 3. MAGNETIC BUTTON
// ============================================================================

interface MagneticButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  children: React.ReactNode
  magneticStrength?: number
  variant?: "primary" | "secondary" | "outline" | "ghost"
  size?: "sm" | "md" | "lg"
}

export const MagneticButton = React.memo(function MagneticButton({
  children,
  className,
  magneticStrength = 0.4,
  variant = "primary",
  size = "md",
  ...props
}: MagneticButtonProps) {
  const buttonRef = React.useRef<HTMLButtonElement>(null)
  const contentRef = React.useRef<HTMLSpanElement>(null)

  const handleMouseMove = React.useCallback((e: React.MouseEvent<HTMLButtonElement>) => {
    const button = buttonRef.current
    const content = contentRef.current
    if (!button || !content) return

    const rect = button.getBoundingClientRect()
    const x = e.clientX - rect.left - rect.width / 2
    const y = e.clientY - rect.top - rect.height / 2

    button.style.transform = `translate(${x * magneticStrength}px, ${y * magneticStrength}px)`
    content.style.transform = `translate(${x * magneticStrength * 0.5}px, ${y * magneticStrength * 0.5}px)`
  }, [magneticStrength])

  const handleMouseLeave = React.useCallback(() => {
    const button = buttonRef.current
    const content = contentRef.current
    if (button) button.style.transform = "translate(0, 0)"
    if (content) content.style.transform = "translate(0, 0)"
  }, [])

  const variants = {
    primary: "bg-gradient-to-r from-emerald-500 to-teal-600 text-white shadow-lg shadow-emerald-500/30 hover:shadow-emerald-500/50",
    secondary: "bg-gray-900 dark:bg-white text-white dark:text-gray-900 shadow-lg",
    outline: "border-2 border-emerald-500 text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-950",
    ghost: "text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800",
  }

  const sizes = {
    sm: "px-4 py-2 text-sm",
    md: "px-6 py-3 text-base",
    lg: "px-8 py-4 text-lg",
  }

  return (
    <button
      ref={buttonRef}
      onMouseMove={handleMouseMove}
      onMouseLeave={handleMouseLeave}
      className={cn(
        "relative rounded-xl font-semibold transition-all duration-200 ease-out",
        "focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2",
        variants[variant],
        sizes[size],
        className
      )}
      {...props}
    >
      <span ref={contentRef} className="relative z-10 flex items-center justify-center gap-2 transition-transform duration-200">
        {children}
      </span>
    </button>
  )
})

// ============================================================================
// 4. TEXT REVEAL ANIMATION (WORD BY WORD)
// ============================================================================

interface TextRevealProps {
  text: string
  className?: string
  delay?: number
  staggerDelay?: number
  as?: "h1" | "h2" | "h3" | "h4" | "p" | "span"
}

export const TextReveal = React.memo(function TextReveal({
  text,
  className,
  delay = 0,
  staggerDelay = 0.05,
  as: Component = "p",
}: TextRevealProps) {
  const words = text.split(" ")
  const [isVisible, setIsVisible] = React.useState(false)
  const containerRef = React.useRef<HTMLElement>(null)

  React.useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true)
          observer.disconnect()
        }
      },
      { threshold: 0.1 }
    )

    if (containerRef.current) {
      observer.observe(containerRef.current)
    }

    return () => observer.disconnect()
  }, [])

  return (
    <Component
      ref={containerRef as React.RefObject<HTMLHeadingElement & HTMLParagraphElement>}
      className={cn("overflow-hidden", className)}
    >
      {words.map((word, i) => (
        <span key={i} className="inline-block overflow-hidden">
          <span
            className="inline-block transition-all duration-700 ease-out"
            style={{
              transform: isVisible ? "translateY(0)" : "translateY(100%)",
              opacity: isVisible ? 1 : 0,
              transitionDelay: `${delay + i * staggerDelay}s`,
            }}
          >
            {word}&nbsp;
          </span>
        </span>
      ))}
    </Component>
  )
})

// ============================================================================
// 5. CHARACTER REVEAL ANIMATION
// ============================================================================

interface CharRevealProps {
  text: string
  className?: string
  delay?: number
  staggerDelay?: number
}

export const CharReveal = React.memo(function CharReveal({
  text,
  className,
  delay = 0,
  staggerDelay = 0.03,
}: CharRevealProps) {
  const chars = text.split("")
  const [isVisible, setIsVisible] = React.useState(false)
  const containerRef = React.useRef<HTMLSpanElement>(null)

  React.useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true)
          observer.disconnect()
        }
      },
      { threshold: 0.1 }
    )

    if (containerRef.current) {
      observer.observe(containerRef.current)
    }

    return () => observer.disconnect()
  }, [])

  return (
    <span ref={containerRef} className={cn("inline-block", className)}>
      {chars.map((char, i) => (
        <span
          key={i}
          className="inline-block transition-all duration-500 ease-out"
          style={{
            transform: isVisible ? "translateY(0) rotateX(0)" : "translateY(50px) rotateX(-90deg)",
            opacity: isVisible ? 1 : 0,
            transitionDelay: `${delay + i * staggerDelay}s`,
          }}
        >
          {char === " " ? "\u00A0" : char}
        </span>
      ))}
    </span>
  )
})

// ============================================================================
// 6. PARALLAX CONTAINER
// ============================================================================

interface ParallaxLayerProps {
  children: React.ReactNode
  speed?: number
  className?: string
}

export const ParallaxLayer = React.memo(function ParallaxLayer({
  children,
  speed = 0.5,
  className,
}: ParallaxLayerProps) {
  const layerRef = React.useRef<HTMLDivElement>(null)

  React.useEffect(() => {
    const handleScroll = () => {
      const layer = layerRef.current
      if (!layer) return

      const scrollY = window.scrollY
      const offset = scrollY * speed
      layer.style.transform = `translateY(${offset}px)`
    }

    window.addEventListener("scroll", handleScroll, { passive: true })
    return () => window.removeEventListener("scroll", handleScroll)
  }, [speed])

  return (
    <div ref={layerRef} className={cn("will-change-transform", className)}>
      {children}
    </div>
  )
})

interface ParallaxContainerProps {
  children: React.ReactNode
  className?: string
}

export function ParallaxContainer({ children, className }: ParallaxContainerProps) {
  return (
    <div className={cn("relative overflow-hidden", className)}>
      {children}
    </div>
  )
}

// ============================================================================
// 7. SMOOTH SCROLL LINK
// ============================================================================

interface SmoothScrollLinkProps extends React.AnchorHTMLAttributes<HTMLAnchorElement> {
  href: string
  children: React.ReactNode
  offset?: number
}

export const SmoothScrollLink = React.memo(function SmoothScrollLink({
  href,
  children,
  offset = 0,
  className,
  ...props
}: SmoothScrollLinkProps) {
  const handleClick = React.useCallback((e: React.MouseEvent<HTMLAnchorElement>) => {
    if (href.startsWith("#")) {
      e.preventDefault()
      const element = document.querySelector(href)
      if (element) {
        const top = element.getBoundingClientRect().top + window.scrollY - offset
        window.scrollTo({ top, behavior: "smooth" })
      }
    }
  }, [href, offset])

  return (
    <a href={href} onClick={handleClick} className={className} {...props}>
      {children}
    </a>
  )
})

// ============================================================================
// 8. ANIMATED COUNTER
// ============================================================================

interface AnimatedCounterProps {
  end: number
  duration?: number
  prefix?: string
  suffix?: string
  className?: string
}

export const AnimatedCounter = React.memo(function AnimatedCounter({
  end,
  duration = 2000,
  prefix = "",
  suffix = "",
  className,
}: AnimatedCounterProps) {
  const [count, setCount] = React.useState(0)
  const [isVisible, setIsVisible] = React.useState(false)
  const counterRef = React.useRef<HTMLSpanElement>(null)

  React.useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true)
          observer.disconnect()
        }
      },
      { threshold: 0.1 }
    )

    if (counterRef.current) {
      observer.observe(counterRef.current)
    }

    return () => observer.disconnect()
  }, [])

  React.useEffect(() => {
    if (!isVisible) return

    let startTime: number
    let animationFrame: number

    const animate = (timestamp: number) => {
      if (!startTime) startTime = timestamp
      const progress = Math.min((timestamp - startTime) / duration, 1)

      // Easing function for smooth animation
      const easeOutQuart = 1 - Math.pow(1 - progress, 4)
      setCount(Math.floor(easeOutQuart * end))

      if (progress < 1) {
        animationFrame = requestAnimationFrame(animate)
      }
    }

    animationFrame = requestAnimationFrame(animate)
    return () => cancelAnimationFrame(animationFrame)
  }, [isVisible, end, duration])

  return (
    <span ref={counterRef} className={className}>
      {prefix}{count.toLocaleString()}{suffix}
    </span>
  )
})

// ============================================================================
// 9. CLIENT LOGO CAROUSEL
// ============================================================================

interface LogoCarouselProps {
  logos: Array<{
    name: string
    src: string
    href?: string
  }>
  className?: string
  speed?: number
}

export const LogoCarousel = React.memo(function LogoCarousel({
  logos,
  className,
  speed = 30,
}: LogoCarouselProps) {
  // Duplicate logos for seamless loop
  const duplicatedLogos = [...logos, ...logos]

  return (
    <div className={cn("relative overflow-hidden py-8", className)}>
      {/* Gradient overlays */}
      <div className="absolute left-0 top-0 bottom-0 w-32 z-10 bg-gradient-to-r from-white dark:from-gray-900 to-transparent" />
      <div className="absolute right-0 top-0 bottom-0 w-32 z-10 bg-gradient-to-l from-white dark:from-gray-900 to-transparent" />

      <div
        className="flex items-center gap-16 animate-scroll-x"
        style={{
          animationDuration: `${speed}s`,
          width: "fit-content",
        }}
      >
        {duplicatedLogos.map((logo, i) => (
          <div
            key={`${logo.name}-${i}`}
            className="flex-shrink-0 h-12 grayscale opacity-60 hover:grayscale-0 hover:opacity-100 transition-all duration-300"
          >
            {logo.href ? (
              <a href={logo.href} target="_blank" rel="noopener noreferrer" aria-label={logo.name}>
                <img src={logo.src} alt={logo.name} className="h-full w-auto object-contain" />
              </a>
            ) : (
              <img src={logo.src} alt={logo.name} className="h-full w-auto object-contain" />
            )}
          </div>
        ))}
      </div>
    </div>
  )
})

// ============================================================================
// 10. SCROLL PROGRESS INDICATOR
// ============================================================================

interface ScrollProgressProps {
  className?: string
  color?: string
}

export function ScrollProgress({ className, color = "#10b981" }: ScrollProgressProps) {
  const [progress, setProgress] = React.useState(0)

  React.useEffect(() => {
    const handleScroll = () => {
      const scrollHeight = document.documentElement.scrollHeight - window.innerHeight
      const scrollProgress = (window.scrollY / scrollHeight) * 100
      setProgress(scrollProgress)
    }

    window.addEventListener("scroll", handleScroll, { passive: true })
    return () => window.removeEventListener("scroll", handleScroll)
  }, [])

  return (
    <div className={cn("fixed top-0 left-0 right-0 h-1 z-50", className)}>
      <div
        className="h-full transition-all duration-150 ease-out"
        style={{
          width: `${progress}%`,
          background: `linear-gradient(90deg, ${color}, ${color}dd)`,
        }}
      />
    </div>
  )
}

// ============================================================================
// 11. SPOTLIGHT CARD
// ============================================================================

interface SpotlightCardProps {
  children: React.ReactNode
  className?: string
  spotlightColor?: string
}

export const SpotlightCard = React.memo(function SpotlightCard({
  children,
  className,
  spotlightColor = "rgba(16, 185, 129, 0.15)",
}: SpotlightCardProps) {
  const cardRef = React.useRef<HTMLDivElement>(null)

  const handleMouseMove = React.useCallback((e: React.MouseEvent<HTMLDivElement>) => {
    const card = cardRef.current
    if (!card) return

    const rect = card.getBoundingClientRect()
    const x = e.clientX - rect.left
    const y = e.clientY - rect.top

    card.style.setProperty("--mouse-x", `${x}px`)
    card.style.setProperty("--mouse-y", `${y}px`)
  }, [])

  return (
    <div
      ref={cardRef}
      onMouseMove={handleMouseMove}
      className={cn(
        "group relative rounded-2xl bg-white dark:bg-gray-900",
        "border border-gray-200 dark:border-gray-800",
        "overflow-hidden transition-all duration-300",
        "hover:border-emerald-500/50 hover:shadow-lg hover:shadow-emerald-500/10",
        className
      )}
    >
      {/* Spotlight effect */}
      <div
        className="pointer-events-none absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-500"
        style={{
          background: `radial-gradient(600px circle at var(--mouse-x) var(--mouse-y), ${spotlightColor}, transparent 40%)`,
        }}
        aria-hidden="true"
      />
      {/* Border glow */}
      <div
        className="pointer-events-none absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-500"
        style={{
          background: `radial-gradient(400px circle at var(--mouse-x) var(--mouse-y), rgba(16, 185, 129, 0.4), transparent 40%)`,
          mask: "linear-gradient(black, black) content-box, linear-gradient(black, black)",
          maskComposite: "xor",
          padding: "1px",
          borderRadius: "1rem",
        }}
        aria-hidden="true"
      />
      <div className="relative z-10">
        {children}
      </div>
    </div>
  )
})

// ============================================================================
// 12. GRADIENT TEXT
// ============================================================================

interface GradientTextProps {
  children: React.ReactNode
  className?: string
  from?: string
  via?: string
  to?: string
  animate?: boolean
}

export const GradientText = React.memo(function GradientText({
  children,
  className,
  from = "#10b981",
  via = "#14b8a6",
  to = "#0891b2",
  animate = false,
}: GradientTextProps) {
  return (
    <span
      className={cn(
        "bg-clip-text text-transparent bg-gradient-to-r",
        animate && "animate-gradient-x bg-[length:200%_auto]",
        className
      )}
      style={{
        backgroundImage: via
          ? `linear-gradient(90deg, ${from}, ${via}, ${to})`
          : `linear-gradient(90deg, ${from}, ${to})`,
      }}
    >
      {children}
    </span>
  )
})

// ============================================================================
// 13. STAGGER CHILDREN ANIMATION
// ============================================================================

interface StaggerContainerProps {
  children: React.ReactNode
  className?: string
  staggerDelay?: number
  initialDelay?: number
}

export function StaggerContainer({
  children,
  className,
  staggerDelay = 0.1,
  initialDelay = 0,
}: StaggerContainerProps) {
  const [isVisible, setIsVisible] = React.useState(false)
  const containerRef = React.useRef<HTMLDivElement>(null)

  React.useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true)
          observer.disconnect()
        }
      },
      { threshold: 0.1 }
    )

    if (containerRef.current) {
      observer.observe(containerRef.current)
    }

    return () => observer.disconnect()
  }, [])

  return (
    <div ref={containerRef} className={className}>
      {React.Children.map(children, (child, index) => (
        <div
          className="transition-all duration-700 ease-out"
          style={{
            opacity: isVisible ? 1 : 0,
            transform: isVisible ? "translateY(0)" : "translateY(30px)",
            transitionDelay: `${initialDelay + index * staggerDelay}s`,
          }}
        >
          {child}
        </div>
      ))}
    </div>
  )
}

// ============================================================================
// 14. HOVER CARD WITH GLOW
// ============================================================================

interface GlowCardProps {
  children: React.ReactNode
  className?: string
  glowColor?: string
}

export const GlowCard = React.memo(function GlowCard({
  children,
  className,
  glowColor = "emerald",
}: GlowCardProps) {
  return (
    <div
      className={cn(
        "group relative rounded-2xl p-[1px] overflow-hidden",
        "bg-gradient-to-br from-gray-200 to-gray-300 dark:from-gray-800 dark:to-gray-900",
        className
      )}
    >
      {/* Animated gradient border */}
      <div
        className={cn(
          "absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-500",
          `bg-gradient-to-r from-${glowColor}-400 via-${glowColor}-500 to-${glowColor}-600`
        )}
        style={{
          background: `linear-gradient(90deg, #10b981, #14b8a6, #0891b2, #10b981)`,
          backgroundSize: "300% 100%",
          animation: "gradientMove 3s linear infinite",
        }}
      />
      {/* Card content */}
      <div className="relative rounded-2xl bg-white dark:bg-gray-900 p-6">
        {children}
      </div>
    </div>
  )
})

// ============================================================================
// ADDITIONAL KEYFRAME ANIMATIONS (Add to globals.css)
// ============================================================================

/*
Add these to globals.css:

@keyframes scroll-x {
  from { transform: translateX(0); }
  to { transform: translateX(-50%); }
}

.animate-scroll-x {
  animation: scroll-x linear infinite;
}

@keyframes gradient-x {
  0%, 100% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
}

.animate-gradient-x {
  animation: gradient-x 3s ease infinite;
}

@keyframes gradientMove {
  0% { background-position: 0% 50%; }
  100% { background-position: 300% 50%; }
}
*/

export default {
  CustomCursorProvider,
  TiltCard,
  MagneticButton,
  TextReveal,
  CharReveal,
  ParallaxLayer,
  ParallaxContainer,
  SmoothScrollLink,
  AnimatedCounter,
  LogoCarousel,
  ScrollProgress,
  SpotlightCard,
  GradientText,
  StaggerContainer,
  GlowCard,
}
