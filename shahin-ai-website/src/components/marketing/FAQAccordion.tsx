"use client"

import { useState, useRef } from "react"
import { motion, useInView } from "framer-motion"
import { HelpCircle, ChevronDown } from "lucide-react"
import { cn } from "@/lib/utils"

export interface FAQItem {
  question: string
  answer: string
}

interface FAQAccordionProps {
  items: FAQItem[]
  className?: string
}

export function FAQAccordion({ items, className }: FAQAccordionProps) {
  return (
    <div className={cn("space-y-4", className)}>
      {items.map((item, index) => (
        <FAQItem key={index} item={item} index={index} />
      ))}
    </div>
  )
}

function FAQItem({ item, index }: { item: FAQItem; index: number }) {
  const [isOpen, setIsOpen] = useState(false)
  const itemRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(itemRef, { once: true, margin: "-50px" })

  return (
    <motion.div
      ref={itemRef}
      className={cn(
        "border border-gray-200 dark:border-gray-800 rounded-xl overflow-hidden",
        "bg-white dark:bg-gray-900",
        "transition-all duration-300",
        isOpen && "shadow-md"
      )}
      initial={{ opacity: 0, y: 20 }}
      animate={isInView ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.5, delay: index * 0.1 }}
    >
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="w-full p-6 flex items-center justify-between text-left hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
        aria-expanded={isOpen}
        aria-controls={`faq-answer-${index}`}
      >
        <span className="font-semibold text-gray-900 dark:text-white pr-4 flex-1">
          {item.question}
        </span>
        <motion.div
          animate={{ rotate: isOpen ? 180 : 0 }}
          transition={{ duration: 0.3 }}
          className="flex-shrink-0"
        >
          <ChevronDown className="w-5 h-5 text-gray-400" />
        </motion.div>
      </button>
      <motion.div
        id={`faq-answer-${index}`}
        initial={false}
        animate={{ height: isOpen ? "auto" : 0 }}
        transition={{ duration: 0.3 }}
        className="overflow-hidden"
      >
        <div className="px-6 pb-6 text-gray-600 dark:text-gray-400 leading-relaxed">
          {item.answer}
        </div>
      </motion.div>
    </motion.div>
  )
}
