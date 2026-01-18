"use client"

import { useState, useRef } from "react"
import { motion, useInView } from "framer-motion"
import { Mail, Phone, MapPin, Send, Clock, CheckCircle2, AlertCircle } from "lucide-react"
import { useLocale, useTranslations } from "next-intl"
import { Navigation } from "@/components/sections/Navigation"
import { Footer } from "@/components/sections/Footer"
import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"
import { submitContact } from "@/lib/api"

function ContactInfo({ icon: Icon, title, value, href }: {
  icon: React.ElementType
  title: string
  value: string
  href?: string
}) {
  const content = (
    <div className="flex items-start gap-4">
      <div className={cn(
        "w-12 h-12 rounded-xl flex-shrink-0",
        "bg-gradient-to-br from-accent-100 to-accent-50 dark:from-accent-900/30 dark:to-accent-800/20",
        "flex items-center justify-center"
      )}>
        <Icon className="w-5 h-5 text-accent-600 dark:text-accent-400" />
      </div>
      <div>
        <p className="text-sm text-gray-500 dark:text-gray-400 mb-1">{title}</p>
        <p className="text-gray-900 dark:text-white font-medium">{value}</p>
      </div>
    </div>
  )

  if (href) {
    return (
      <a href={href} className="block hover:opacity-80 transition-opacity">
        {content}
      </a>
    )
  }

  return content
}

export default function ContactPage() {
  const t = useTranslations('contact')
  const locale = useLocale()
  const isRTL = locale === 'ar'

  const sectionRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(sectionRef, { once: true, margin: "-100px" })

  const [formData, setFormData] = useState({
    name: "",
    email: "",
    phone: "",
    company: "",
    service: "",
    message: ""
  })
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSubmitted, setIsSubmitted] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsSubmitting(true)
    setError(null)

    try {
      const response = await submitContact({
        name: formData.name,
        email: formData.email,
        phone: formData.phone || undefined,
        company: formData.company || undefined,
        subject: formData.service || 'General Inquiry',
        message: formData.message
      })

      if (response.success) {
        setIsSubmitted(true)
        setFormData({ name: "", email: "", phone: "", company: "", service: "", message: "" })
        // Reset success message after 5 seconds
        setTimeout(() => setIsSubmitted(false), 5000)
      } else {
        setError(response.message || (locale === 'ar' ? 'فشل إرسال الرسالة. حاول مرة أخرى.' : 'Failed to send message. Please try again.'))
      }
    } catch (err) {
      console.error('Contact form error:', err)
      setError(locale === 'ar'
        ? 'حدث خطأ أثناء إرسال الرسالة. حاول مرة أخرى.'
        : 'An error occurred while sending your message. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }))
  }

  const services = [
    { value: "telecommunications", label: t('form.services.telecommunications') },
    { value: "data-centers", label: t('form.services.dataCenters') },
    { value: "cybersecurity", label: t('form.services.cybersecurity') },
    { value: "governance", label: t('form.services.governance') },
    { value: "other", label: t('form.services.other') },
  ]

  return (
    <main className="min-h-screen" dir={isRTL ? 'rtl' : 'ltr'}>
      <Navigation />

      {/* Hero Section */}
      <section className="relative pt-32 pb-20 overflow-hidden">
        <div className="absolute inset-0 hero-gradient" />
        <div className="absolute inset-0 grid-pattern" />

        <div className="container mx-auto px-6 relative z-10">
          <motion.div
            className="text-center max-w-3xl mx-auto"
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
          >
            <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-accent-500/10 border border-accent-500/30 text-accent-400 text-sm font-semibold mb-6">
              <Mail className="w-4 h-4" />
              {t('badge')}
            </span>
            <h1 className="text-4xl md:text-5xl lg:text-6xl font-extrabold text-white mb-6">
              {t('title')}
            </h1>
            <p className="text-lg text-white/80">
              {t('subtitle')}
            </p>
          </motion.div>
        </div>
      </section>

      {/* Contact Section */}
      <section ref={sectionRef} className="py-24 bg-white dark:bg-gray-900">
        <div className="container mx-auto px-6">
          <div className="grid lg:grid-cols-2 gap-16">
            {/* Contact Info */}
            <motion.div
              initial={{ opacity: 0, x: isRTL ? 30 : -30 }}
              animate={isInView ? { opacity: 1, x: 0 } : {}}
              transition={{ duration: 0.6 }}
            >
              <h2 className="text-2xl md:text-3xl font-bold text-gray-900 dark:text-white mb-8">
                {t('info.title')}
              </h2>

              <div className="space-y-6 mb-12">
                <ContactInfo
                  icon={Mail}
                  title={t('info.email')}
                  value="info@doganconsult.com"
                  href="mailto:info@doganconsult.com"
                />
                <ContactInfo
                  icon={Phone}
                  title={t('info.phone')}
                  value="+966 XX XXX XXXX"
                  href="tel:+966XXXXXXXX"
                />
                <ContactInfo
                  icon={MapPin}
                  title={t('info.address')}
                  value={t('info.addressValue')}
                />
                <ContactInfo
                  icon={Clock}
                  title={t('info.hours')}
                  value={t('info.hoursValue')}
                />
              </div>

              {/* Map placeholder */}
              <div className={cn(
                "aspect-video rounded-2xl overflow-hidden",
                "bg-gray-100 dark:bg-gray-800",
                "border border-gray-200 dark:border-gray-700"
              )}>
                <div className="w-full h-full flex items-center justify-center text-gray-400">
                  <MapPin className="w-12 h-12" />
                </div>
              </div>
            </motion.div>

            {/* Contact Form */}
            <motion.div
              initial={{ opacity: 0, x: isRTL ? -30 : 30 }}
              animate={isInView ? { opacity: 1, x: 0 } : {}}
              transition={{ duration: 0.6, delay: 0.2 }}
            >
              <div className={cn(
                "p-8 rounded-2xl",
                "bg-gray-50 dark:bg-gray-800",
                "border border-gray-200 dark:border-gray-700"
              )}>
                <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">
                  {t('form.title')}
                </h2>

                {isSubmitted ? (
                  <motion.div
                    className="text-center py-12"
                    initial={{ opacity: 0, scale: 0.9 }}
                    animate={{ opacity: 1, scale: 1 }}
                  >
                    <div className="w-16 h-16 rounded-full bg-success-100 dark:bg-success-900/30 flex items-center justify-center mx-auto mb-4">
                      <CheckCircle2 className="w-8 h-8 text-success-500" />
                    </div>
                    <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                      {t('form.success.title')}
                    </h3>
                    <p className="text-gray-600 dark:text-gray-400">
                      {t('form.success.message')}
                    </p>
                  </motion.div>
                ) : (
                  <form onSubmit={handleSubmit} className="space-y-6">
                    {error && (
                      <motion.div
                        className="p-4 rounded-xl bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 flex items-start gap-3"
                        initial={{ opacity: 0, y: -10 }}
                        animate={{ opacity: 1, y: 0 }}
                      >
                        <AlertCircle className="w-5 h-5 text-red-500 flex-shrink-0 mt-0.5" />
                        <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
                      </motion.div>
                    )}
                    <div className="grid md:grid-cols-2 gap-6">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                          {t('form.name')} *
                        </label>
                        <input
                          type="text"
                          name="name"
                          value={formData.name}
                          onChange={handleChange}
                          required
                          className={cn(
                            "w-full px-4 py-3 rounded-xl text-sm",
                            "bg-white dark:bg-gray-900",
                            "border border-gray-200 dark:border-gray-700",
                            "focus:outline-none focus:border-accent-500 focus:ring-1 focus:ring-accent-500",
                            "transition-all duration-300"
                          )}
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                          {t('form.email')} *
                        </label>
                        <input
                          type="email"
                          name="email"
                          value={formData.email}
                          onChange={handleChange}
                          required
                          className={cn(
                            "w-full px-4 py-3 rounded-xl text-sm",
                            "bg-white dark:bg-gray-900",
                            "border border-gray-200 dark:border-gray-700",
                            "focus:outline-none focus:border-accent-500 focus:ring-1 focus:ring-accent-500",
                            "transition-all duration-300"
                          )}
                        />
                      </div>
                    </div>

                    <div className="grid md:grid-cols-2 gap-6">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                          {t('form.phone')}
                        </label>
                        <input
                          type="tel"
                          name="phone"
                          value={formData.phone}
                          onChange={handleChange}
                          className={cn(
                            "w-full px-4 py-3 rounded-xl text-sm",
                            "bg-white dark:bg-gray-900",
                            "border border-gray-200 dark:border-gray-700",
                            "focus:outline-none focus:border-accent-500 focus:ring-1 focus:ring-accent-500",
                            "transition-all duration-300"
                          )}
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                          {t('form.company')}
                        </label>
                        <input
                          type="text"
                          name="company"
                          value={formData.company}
                          onChange={handleChange}
                          className={cn(
                            "w-full px-4 py-3 rounded-xl text-sm",
                            "bg-white dark:bg-gray-900",
                            "border border-gray-200 dark:border-gray-700",
                            "focus:outline-none focus:border-accent-500 focus:ring-1 focus:ring-accent-500",
                            "transition-all duration-300"
                          )}
                        />
                      </div>
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                        {t('form.service')}
                      </label>
                      <select
                        name="service"
                        value={formData.service}
                        onChange={handleChange}
                        className={cn(
                          "w-full px-4 py-3 rounded-xl text-sm",
                          "bg-white dark:bg-gray-900",
                          "border border-gray-200 dark:border-gray-700",
                          "focus:outline-none focus:border-accent-500 focus:ring-1 focus:ring-accent-500",
                          "transition-all duration-300"
                        )}
                      >
                        <option value="">{t('form.selectService')}</option>
                        {services.map(service => (
                          <option key={service.value} value={service.value}>
                            {service.label}
                          </option>
                        ))}
                      </select>
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                        {t('form.message')} *
                      </label>
                      <textarea
                        name="message"
                        value={formData.message}
                        onChange={handleChange}
                        required
                        rows={5}
                        className={cn(
                          "w-full px-4 py-3 rounded-xl text-sm resize-none",
                          "bg-white dark:bg-gray-900",
                          "border border-gray-200 dark:border-gray-700",
                          "focus:outline-none focus:border-accent-500 focus:ring-1 focus:ring-accent-500",
                          "transition-all duration-300"
                        )}
                      />
                    </div>

                    <Button
                      type="submit"
                      variant="gradient"
                      size="lg"
                      className="w-full"
                      disabled={isSubmitting}
                    >
                      {isSubmitting ? (
                        <span className="flex items-center gap-2">
                          <motion.span
                            className="w-5 h-5 border-2 border-current border-t-transparent rounded-full"
                            animate={{ rotate: 360 }}
                            transition={{ duration: 1, repeat: Infinity, ease: "linear" }}
                          />
                          {t('form.submitting')}
                        </span>
                      ) : (
                        <>
                          <Send className="w-5 h-5" />
                          {t('form.submit')}
                        </>
                      )}
                    </Button>
                  </form>
                )}
              </div>
            </motion.div>
          </div>
        </div>
      </section>

      <Footer />
    </main>
  )
}
