"use client"

import { useRef, useState } from "react"
import { motion, useInView, AnimatePresence } from "framer-motion"
import { Mail, ArrowRight, ArrowLeft, Rocket, X, CheckCircle2, AlertCircle, Loader2 } from "lucide-react"
import Link from "next/link"
import { useLocale, useTranslations } from "next-intl"
import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"
import { startTrial } from "@/lib/api"

export function CTA() {
  const t = useTranslations('cta')
  const locale = useLocale()
  const isRTL = locale === 'ar'

  const sectionRef = useRef<HTMLDivElement>(null)
  const isInView = useInView(sectionRef, { once: true, margin: "-100px" })

  const ArrowIcon = isRTL ? ArrowLeft : ArrowRight

  // Trial modal state
  const [isModalOpen, setIsModalOpen] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSuccess, setIsSuccess] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [formData, setFormData] = useState({
    fullName: "",
    email: "",
    companyName: "",
    phoneNumber: "",
    companySize: "",
    industry: ""
  })

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }))
  }

  const handleTrialSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (isSubmitting) return

    setIsSubmitting(true)
    setError(null)

    try {
      const response = await startTrial({
        fullName: formData.fullName,
        email: formData.email,
        companyName: formData.companyName,
        phoneNumber: formData.phoneNumber || undefined,
        companySize: formData.companySize || undefined,
        industry: formData.industry || undefined,
        locale
      })

      if (response.success) {
        setIsSuccess(true)
        setFormData({ fullName: "", email: "", companyName: "", phoneNumber: "", companySize: "", industry: "" })
      } else {
        const message = locale === 'ar' ? response.messageAr : response.messageEn
        setError(message || (locale === 'ar' ? 'فشل بدء النسخة التجريبية.' : 'Failed to start trial.'))
      }
    } catch (err) {
      console.error('Trial signup error:', err)
      setError(locale === 'ar'
        ? 'حدث خطأ. حاول مرة أخرى.'
        : 'An error occurred. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  const closeModal = () => {
    setIsModalOpen(false)
    setIsSuccess(false)
    setError(null)
  }

  const companySizes = [
    { value: "1-10", label: locale === 'ar' ? '1-10 موظفين' : '1-10 employees' },
    { value: "11-50", label: locale === 'ar' ? '11-50 موظف' : '11-50 employees' },
    { value: "51-200", label: locale === 'ar' ? '51-200 موظف' : '51-200 employees' },
    { value: "201-500", label: locale === 'ar' ? '201-500 موظف' : '201-500 employees' },
    { value: "500+", label: locale === 'ar' ? 'أكثر من 500' : '500+ employees' },
  ]

  const industries = [
    { value: "technology", label: locale === 'ar' ? 'التكنولوجيا' : 'Technology' },
    { value: "finance", label: locale === 'ar' ? 'المالية' : 'Finance' },
    { value: "healthcare", label: locale === 'ar' ? 'الرعاية الصحية' : 'Healthcare' },
    { value: "government", label: locale === 'ar' ? 'الحكومة' : 'Government' },
    { value: "energy", label: locale === 'ar' ? 'الطاقة' : 'Energy' },
    { value: "retail", label: locale === 'ar' ? 'التجزئة' : 'Retail' },
    { value: "other", label: locale === 'ar' ? 'أخرى' : 'Other' },
  ]

  return (
    <section
      id="contact"
      ref={sectionRef}
      className="relative py-24 md:py-32 overflow-hidden"
      dir={isRTL ? 'rtl' : 'ltr'}
    >
      {/* Background */}
      <div className="absolute inset-0 hero-gradient" />
      <div className="absolute inset-0 grid-pattern" />

      {/* Glow orbs */}
      <motion.div
        className="absolute -top-20 -right-20 w-[400px] h-[400px] rounded-full"
        style={{
          background: 'radial-gradient(circle, rgba(245, 158, 11, 0.2) 0%, transparent 70%)',
          filter: 'blur(60px)',
        }}
        animate={{ scale: [1, 1.2, 1], opacity: [0.3, 0.5, 0.3] }}
        transition={{ duration: 8, repeat: Infinity }}
      />
      <motion.div
        className="absolute -bottom-20 -left-20 w-[300px] h-[300px] rounded-full"
        style={{
          background: 'radial-gradient(circle, rgba(59, 130, 246, 0.2) 0%, transparent 70%)',
          filter: 'blur(60px)',
        }}
        animate={{ scale: [1.2, 1, 1.2], opacity: [0.2, 0.4, 0.2] }}
        transition={{ duration: 10, repeat: Infinity }}
      />

      {/* Content */}
      <div className="container mx-auto px-6 relative z-10">
        <motion.div
          className="text-center max-w-2xl mx-auto"
          initial={{ opacity: 0, y: 30 }}
          animate={isInView ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.6 }}
        >
          <h2 className="text-3xl md:text-4xl lg:text-5xl font-extrabold text-white mb-6">
            {t('title')}
          </h2>
          <p className="text-lg text-white/80 mb-10">
            {t('description')}
          </p>

          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Button
              variant="gradient"
              size="xl"
              className="group"
              onClick={() => setIsModalOpen(true)}
            >
              <Rocket className="w-5 h-5" />
              {locale === 'ar' ? 'ابدأ نسختك التجريبية المجانية' : 'Start Free Trial'}
              <ArrowIcon className={`w-5 h-5 transition-transform ${isRTL ? 'group-hover:-translate-x-1' : 'group-hover:translate-x-1'}`} />
            </Button>

            <Link href="mailto:info@doganconsult.com">
              <Button variant="outline" size="xl" className="group border-white/30 text-white hover:bg-white/10">
                <Mail className="w-5 h-5" />
                {t('button')}
              </Button>
            </Link>
          </div>
        </motion.div>
      </div>

      {/* Trial Signup Modal */}
      <AnimatePresence>
        {isModalOpen && (
          <motion.div
            className="fixed inset-0 z-50 flex items-center justify-center p-4"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
          >
            {/* Backdrop */}
            <motion.div
              className="absolute inset-0 bg-black/60 backdrop-blur-sm"
              onClick={closeModal}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
            />

            {/* Modal */}
            <motion.div
              className={cn(
                "relative w-full max-w-lg max-h-[90vh] overflow-y-auto",
                "bg-white dark:bg-gray-900 rounded-2xl shadow-2xl",
                "p-6 md:p-8"
              )}
              initial={{ opacity: 0, scale: 0.9, y: 20 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.9, y: 20 }}
              dir={isRTL ? 'rtl' : 'ltr'}
            >
              {/* Close button */}
              <button
                onClick={closeModal}
                className={cn(
                  "absolute top-4 p-2 rounded-full",
                  "bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700",
                  "transition-colors",
                  isRTL ? "left-4" : "right-4"
                )}
              >
                <X className="w-5 h-5 text-gray-500" />
              </button>

              {isSuccess ? (
                <motion.div
                  className="text-center py-8"
                  initial={{ opacity: 0, scale: 0.9 }}
                  animate={{ opacity: 1, scale: 1 }}
                >
                  <div className="w-16 h-16 rounded-full bg-green-100 dark:bg-green-900/30 flex items-center justify-center mx-auto mb-4">
                    <CheckCircle2 className="w-8 h-8 text-green-500" />
                  </div>
                  <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                    {locale === 'ar' ? 'تم التسجيل بنجاح!' : 'Successfully Registered!'}
                  </h3>
                  <p className="text-gray-600 dark:text-gray-400 mb-6">
                    {locale === 'ar'
                      ? 'سنرسل لك بيانات الدخول إلى بريدك الإلكتروني قريباً.'
                      : 'We\'ll send your login details to your email shortly.'}
                  </p>
                  <Button variant="gradient" onClick={closeModal}>
                    {locale === 'ar' ? 'إغلاق' : 'Close'}
                  </Button>
                </motion.div>
              ) : (
                <>
                  <div className="mb-6">
                    <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                      {locale === 'ar' ? 'ابدأ نسختك التجريبية المجانية' : 'Start Your Free Trial'}
                    </h3>
                    <p className="text-gray-600 dark:text-gray-400">
                      {locale === 'ar'
                        ? 'احصل على 14 يوم تجربة مجانية بدون بطاقة ائتمان.'
                        : 'Get 14 days free trial. No credit card required.'}
                    </p>
                  </div>

                  <form onSubmit={handleTrialSubmit} className="space-y-4">
                    {error && (
                      <motion.div
                        className="p-3 rounded-xl bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 flex items-start gap-2"
                        initial={{ opacity: 0, y: -10 }}
                        animate={{ opacity: 1, y: 0 }}
                      >
                        <AlertCircle className="w-5 h-5 text-red-500 flex-shrink-0 mt-0.5" />
                        <p className="text-sm text-red-600 dark:text-red-400">{error}</p>
                      </motion.div>
                    )}

                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                        {locale === 'ar' ? 'الاسم الكامل' : 'Full Name'} *
                      </label>
                      <input
                        type="text"
                        name="fullName"
                        value={formData.fullName}
                        onChange={handleChange}
                        required
                        className={cn(
                          "w-full px-4 py-3 rounded-xl text-sm",
                          "bg-gray-50 dark:bg-gray-800",
                          "border border-gray-200 dark:border-gray-700",
                          "focus:outline-none focus:border-accent-500 focus:ring-1 focus:ring-accent-500",
                          "transition-all duration-300"
                        )}
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                        {locale === 'ar' ? 'البريد الإلكتروني' : 'Email'} *
                      </label>
                      <input
                        type="email"
                        name="email"
                        value={formData.email}
                        onChange={handleChange}
                        required
                        className={cn(
                          "w-full px-4 py-3 rounded-xl text-sm",
                          "bg-gray-50 dark:bg-gray-800",
                          "border border-gray-200 dark:border-gray-700",
                          "focus:outline-none focus:border-accent-500 focus:ring-1 focus:ring-accent-500",
                          "transition-all duration-300"
                        )}
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                        {locale === 'ar' ? 'اسم الشركة' : 'Company Name'} *
                      </label>
                      <input
                        type="text"
                        name="companyName"
                        value={formData.companyName}
                        onChange={handleChange}
                        required
                        className={cn(
                          "w-full px-4 py-3 rounded-xl text-sm",
                          "bg-gray-50 dark:bg-gray-800",
                          "border border-gray-200 dark:border-gray-700",
                          "focus:outline-none focus:border-accent-500 focus:ring-1 focus:ring-accent-500",
                          "transition-all duration-300"
                        )}
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                        {locale === 'ar' ? 'رقم الهاتف' : 'Phone Number'}
                      </label>
                      <input
                        type="tel"
                        name="phoneNumber"
                        value={formData.phoneNumber}
                        onChange={handleChange}
                        className={cn(
                          "w-full px-4 py-3 rounded-xl text-sm",
                          "bg-gray-50 dark:bg-gray-800",
                          "border border-gray-200 dark:border-gray-700",
                          "focus:outline-none focus:border-accent-500 focus:ring-1 focus:ring-accent-500",
                          "transition-all duration-300"
                        )}
                      />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          {locale === 'ar' ? 'حجم الشركة' : 'Company Size'}
                        </label>
                        <select
                          name="companySize"
                          value={formData.companySize}
                          onChange={handleChange}
                          className={cn(
                            "w-full px-4 py-3 rounded-xl text-sm",
                            "bg-gray-50 dark:bg-gray-800",
                            "border border-gray-200 dark:border-gray-700",
                            "focus:outline-none focus:border-accent-500 focus:ring-1 focus:ring-accent-500",
                            "transition-all duration-300"
                          )}
                        >
                          <option value="">{locale === 'ar' ? 'اختر...' : 'Select...'}</option>
                          {companySizes.map(size => (
                            <option key={size.value} value={size.value}>{size.label}</option>
                          ))}
                        </select>
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                          {locale === 'ar' ? 'القطاع' : 'Industry'}
                        </label>
                        <select
                          name="industry"
                          value={formData.industry}
                          onChange={handleChange}
                          className={cn(
                            "w-full px-4 py-3 rounded-xl text-sm",
                            "bg-gray-50 dark:bg-gray-800",
                            "border border-gray-200 dark:border-gray-700",
                            "focus:outline-none focus:border-accent-500 focus:ring-1 focus:ring-accent-500",
                            "transition-all duration-300"
                          )}
                        >
                          <option value="">{locale === 'ar' ? 'اختر...' : 'Select...'}</option>
                          {industries.map(industry => (
                            <option key={industry.value} value={industry.value}>{industry.label}</option>
                          ))}
                        </select>
                      </div>
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
                          <Loader2 className="w-5 h-5 animate-spin" />
                          {locale === 'ar' ? 'جاري التسجيل...' : 'Signing up...'}
                        </span>
                      ) : (
                        <>
                          <Rocket className="w-5 h-5" />
                          {locale === 'ar' ? 'ابدأ النسخة التجريبية' : 'Start Free Trial'}
                        </>
                      )}
                    </Button>
                  </form>
                </>
              )}
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </section>
  )
}
