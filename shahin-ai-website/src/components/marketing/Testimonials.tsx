"use client"

import { useRef } from "react"
import { motion, useInView } from "framer-motion"
import { Quote, Star } from "lucide-react"
import { useTranslations } from "next-intl"
import { cn } from "@/lib/utils"

export interface Testimonial {
  name: string
  role: string
  company: string
  content: string
  rating?: number
  avatar?: string
}

interface TestimonialsProps {
  testimonials?: Testimonial[]
  className?: string
}

export function Testimonials({ testimonials, className }: TestimonialsProps) {
  const sectionRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(sectionRef, { once: true, margin: "-100px" })
  const t = useTranslations('testimonials')

  // If testimonials not provided, try to get from translations
  const items = testimonials || (t.raw('items') as Testimonial[] | undefined) || []

  if (items.length === 0) {
    return null
  }

  return (
    <section
      ref={sectionRef}
      className={cn("py-24 md:py-32 bg-gray-50 dark:bg-gray-950", className)}
    >
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-16"
          initial={{ opacity: 0, y: 30 }}
          animate={isInView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.6 }}
        >
          <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-accent-100 dark:bg-accent-900/30 text-accent-700 dark:text-accent-400 text-sm font-semibold mb-4">
            <Star className="w-4 h-4" />
            {t('badge') || 'Testimonials'}
          </span>
          <h2 className="text-3xl md:text-4xl lg:text-5xl font-extrabold text-gray-900 dark:text-white mb-4">
            {t('title') || 'What Our Clients Say'}
          </h2>
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
            {t('subtitle') || 'Trusted by leading organizations across Saudi Arabia'}
          </p>
        </motion.div>

        {/* Testimonials Grid */}
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6 lg:gap-8">
          {items.map((testimonial, index) => (
            <TestimonialCard key={index} testimonial={testimonial} index={index} />
          ))}
        </div>
      </div>
    </section>
  )
}

function TestimonialCard({ testimonial, index }: { testimonial: Testimonial; index: number }) {
  const cardRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(cardRef, { once: true, margin: "-100px" })

  return (
    <motion.div
      ref={cardRef}
      className={cn(
        "relative p-8 rounded-2xl bg-white dark:bg-gray-900",
        "border border-gray-100 dark:border-gray-800",
        "transition-all duration-500 ease-out",
        "hover:shadow-2xl hover:shadow-gray-200/50 dark:hover:shadow-gray-900/50",
        "hover:-translate-y-2"
      )}
      initial={{ opacity: 0, y: 40 }}
      animate={isInView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.6, delay: index * 0.1 }}
    >
      {/* Quote Icon */}
      <div className="absolute top-6 right-6 opacity-10">
        <Quote className="w-12 h-12 text-accent-500" />
      </div>

      {/* Rating */}
      {testimonial.rating && (
        <div className="flex gap-1 mb-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <Star
              key={i}
              className={cn(
                "w-4 h-4",
                i < testimonial.rating!
                  ? "text-yellow-400 fill-yellow-400"
                  : "text-gray-300 dark:text-gray-700"
              )}
            />
          ))}
        </div>
      )}

      {/* Content */}
      <p className="text-gray-700 dark:text-gray-300 mb-6 leading-relaxed relative z-10">
        {testimonial.content}
      </p>

      {/* Author */}
      <div className="flex items-center gap-4">
        {testimonial.avatar ? (
          <img
            src={testimonial.avatar}
            alt={testimonial.name}
            className="w-12 h-12 rounded-full object-cover"
          />
        ) : (
          <div className="w-12 h-12 rounded-full bg-gradient-to-br from-accent-400 to-primary-500 flex items-center justify-center text-white font-semibold">
            {testimonial.name.charAt(0).toUpperCase()}
          </div>
        )}
        <div>
          <p className="font-semibold text-gray-900 dark:text-white">
            {testimonial.name}
          </p>
          <p className="text-sm text-gray-600 dark:text-gray-400">
            {testimonial.role}
          </p>
          <p className="text-xs text-gray-500 dark:text-gray-500">
            {testimonial.company}
          </p>
        </div>
      </div>
    </motion.div>
  )
}
