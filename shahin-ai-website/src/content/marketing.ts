/**
 * Marketing Content Configuration
 * 
 * This file provides TypeScript types and interfaces for marketing content.
 * Actual content is stored in messages/{locale}.json files and accessed via next-intl.
 * 
 * This file serves as:
 * 1. Type definitions for content structure
 * 2. Documentation of content schema
 * 3. Optional: Default/fallback content (if needed)
 */

// Re-export types from components for convenience
export type { PricingPlan } from '@/components/marketing/PricingCard'
export type { FAQItem } from '@/components/marketing/FAQAccordion'

// Define additional types
export interface Testimonial {
  quote: string
  name: string
  title: string
  image: string
}

export interface FeatureItem {
  icon: string // Name of Lucide icon
  title: string
  description: string
}

/**
 * Marketing content structure
 */
export interface MarketingContent {
  pricing: {
    plans: PricingPlan[]
    faq: {
      title: string
      items: FAQItem[]
    }
  }
  testimonials: Testimonial[]
  features: FeatureItem[]
}

/**
 * Content keys used in messages/{locale}.json
 * 
 * These keys should exist in your translation files:
 * - pricing.plans: Array of PricingPlan
 * - pricing.faq.items: Array of FAQItem
 * - testimonials.items: Array of Testimonial (optional)
 * - expertise.items: Array of feature items (used by FeatureGrid)
 */
export const CONTENT_KEYS = {
  PRICING_PLANS: 'pricing.plans',
  PRICING_FAQ: 'pricing.faq.items',
  TESTIMONIALS: 'testimonials.items',
  FEATURES: 'expertise.items',
} as const

/**
 * Helper to validate content structure at runtime (optional)
 */
export function validateMarketingContent(content: Partial<MarketingContent>): boolean {
  // Basic validation - can be extended
  if (content.pricing?.plans && !Array.isArray(content.pricing.plans)) {
    return false
  }
  if (content.pricing?.faq?.items && !Array.isArray(content.pricing.faq.items)) {
    return false
  }
  return true
}
