"use client"

import { motion } from "framer-motion"
import { Star, Rocket } from "lucide-react"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { useLocale } from "@/components/providers/locale-provider"

const content = {
  sectionLabel: { en: "Early Access", ar: "الوصول المبكر" },
  title: { en: "Join Our Early Adopters", ar: "انضم إلى مستخدمينا الأوائل" },
  subtitle: {
    en: "Be part of the next generation of GRC platforms in Saudi Arabia",
    ar: "كن جزءاً من الجيل التالي من منصات GRC في المملكة العربية السعودية"
  },
  cardTitle: { en: "We're Just Getting Started!", ar: "نحن في البداية فقط!" },
  cardDescription: {
    en: "Shahin is a brand new platform launching in the Saudi market. Be among our first customers and help shape the future of GRC in the Kingdom.",
    ar: "شاهين منصة جديدة تُطلق في السوق السعودي. كن من أوائل عملائنا وساهم في صياغة مستقبل GRC في المملكة."
  },
  benefits: {
    earlyAdopter: { en: "Early Adopter Benefits", ar: "مزايا المستخدمين الأوائل" },
    prioritySupport: { en: "Priority Support", ar: "دعم أولوي" },
    shapeRoadmap: { en: "Shape Our Roadmap", ar: "ساهم في خارطة الطريق" }
  },
  cta: { en: "Become an Early Adopter", ar: "كن من المستخدمين الأوائل" }
}

export function Testimonials() {
  const { locale } = useLocale()
  const isArabic = locale === "ar"

  return (
    <section className="py-24 bg-gradient-to-b from-gray-50 to-white dark:from-gray-900 dark:to-gray-900/50">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-16"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <span className="text-emerald-600 dark:text-emerald-400 font-semibold mb-4 block">
            {isArabic ? content.sectionLabel.ar : content.sectionLabel.en}
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
            {isArabic ? content.title.ar : content.title.en}
          </h2>
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
            {isArabic ? content.subtitle.ar : content.subtitle.en}
          </p>
        </motion.div>

        {/* New to Market Card */}
        <motion.div
          className="max-w-2xl mx-auto"
          initial={{ opacity: 0, y: 30 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <div className="relative p-12 rounded-3xl bg-gradient-to-br from-emerald-50 to-teal-50 dark:from-emerald-900/20 dark:to-teal-900/20 border border-emerald-200 dark:border-emerald-800 text-center">
            {/* Icon */}
            <div className="w-20 h-20 rounded-full bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center mx-auto mb-6 shadow-lg">
              <Rocket className="w-10 h-10 text-white" />
            </div>

            {/* Message */}
            <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-4">
              {isArabic ? content.cardTitle.ar : content.cardTitle.en}
            </h3>
            <p className="text-gray-600 dark:text-gray-400 mb-8 max-w-md mx-auto">
              {isArabic ? content.cardDescription.ar : content.cardDescription.en}
            </p>

            {/* Benefits */}
            <div className="flex flex-wrap justify-center gap-4 mb-8">
              <div className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-gray-800 rounded-full shadow-sm">
                <Star className="w-4 h-4 text-amber-500" />
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  {isArabic ? content.benefits.earlyAdopter.ar : content.benefits.earlyAdopter.en}
                </span>
              </div>
              <div className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-gray-800 rounded-full shadow-sm">
                <Star className="w-4 h-4 text-amber-500" />
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  {isArabic ? content.benefits.prioritySupport.ar : content.benefits.prioritySupport.en}
                </span>
              </div>
              <div className="flex items-center gap-2 px-4 py-2 bg-white dark:bg-gray-800 rounded-full shadow-sm">
                <Star className="w-4 h-4 text-amber-500" />
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                  {isArabic ? content.benefits.shapeRoadmap.ar : content.benefits.shapeRoadmap.en}
                </span>
              </div>
            </div>

            {/* CTA */}
            <Link href="/trial">
              <Button className="bg-emerald-600 hover:bg-emerald-700 text-white px-8 py-6 text-lg">
                {isArabic ? content.cta.ar : content.cta.en}
              </Button>
            </Link>
          </div>
        </motion.div>
      </div>
    </section>
  )
}
