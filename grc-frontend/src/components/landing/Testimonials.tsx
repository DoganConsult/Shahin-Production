"use client"

import { motion } from "framer-motion"
import { Quote, Star } from "lucide-react"
import { useTranslations } from 'next-intl'

const testimonialsData = [
  {
    quote: "منصة شاهين ساعدتنا في تحقيق الامتثال الكامل مع متطلبات الهيئة الوطنية للأمن السيبراني في وقت قياسي",
    author: "أحمد الغامدي",
    role: "مدير أمن المعلومات",
    company: "شركة تقنية رائدة",
    rating: 5,
  },
  {
    quote: "أفضل منصة GRC استخدمناها. واجهة سهلة ودعم فني ممتاز باللغة العربية",
    author: "سارة المالكي",
    role: "مديرة الامتثال",
    company: "مؤسسة مالية كبرى",
    rating: 5,
  },
  {
    quote: "وفرت علينا المنصة أكثر من 70% من الوقت المستغرق في عمليات التدقيق والمتابعة",
    author: "محمد القحطاني",
    role: "رئيس قسم المراجعة الداخلية",
    company: "شركة صناعية",
    rating: 5,
  },
]

export function Testimonials() {
  const t = useTranslations('landing.testimonials')
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
            {t('sectionLabel')}
          </span>
          <h2 className="section-title">
            {t('title')}
          </h2>
          <p className="section-subtitle mx-auto">
            {t('subtitle')}
          </p>
        </motion.div>

        {/* Testimonials Grid */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {testimonialsData.map((testimonial, index) => (
            <motion.div
              key={testimonial.author}
              className="relative p-8 rounded-2xl bg-white dark:bg-gray-800 shadow-lg border border-gray-100 dark:border-gray-700"
              initial={{ opacity: 0, y: 30 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: index * 0.2 }}
            >
              {/* Quote Icon */}
              <div className="absolute -top-4 right-6">
                <div className="w-10 h-10 rounded-full bg-emerald-500 flex items-center justify-center shadow-lg">
                  <Quote className="w-5 h-5 text-white" />
                </div>
              </div>

              {/* Rating */}
              <div className="flex gap-1 mb-4 pt-2">
                {[...Array(testimonial.rating)].map((_, i) => (
                  <Star key={i} className="w-5 h-5 text-amber-400 fill-current" />
                ))}
              </div>

              {/* Quote */}
              <p className="text-gray-700 dark:text-gray-300 mb-6 leading-relaxed">
                "{testimonial.quote}"
              </p>

              {/* Author */}
              <div className="flex items-center gap-4">
                <div className="w-12 h-12 rounded-full bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center text-white font-bold">
                  {testimonial.author.charAt(0)}
                </div>
                <div>
                  <div className="font-semibold text-gray-900 dark:text-white">
                    {testimonial.author}
                  </div>
                  <div className="text-sm text-gray-500 dark:text-gray-400">
                    {testimonial.role}
                  </div>
                  <div className="text-xs text-emerald-600 dark:text-emerald-400">
                    {testimonial.company}
                  </div>
                </div>
              </div>
            </motion.div>
          ))}
        </div>
      </div>
    </section>
  )
}
