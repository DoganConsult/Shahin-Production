"use client"

import { useState } from "react"
import { motion, AnimatePresence } from "framer-motion"
import {
  Mail,
  Building2,
  Users,
  Settings,
  ArrowRight,
  ArrowLeft,
  CheckCircle,
  Sparkles,
  Database,
  AlertCircle,
  Loader2
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { SectorSelect } from "@/components/ui/sector-select"
import { useLocale } from "@/components/providers/locale-provider"
import { leadApi, ApiError } from "@/lib/api"

type FormData = {
  email: string
  companySize: string
  sector: string
  erpSystem: string
  mainPain: string
}

const companySizes = [
  { value: "1-50", label: "1-50 employees", labelAr: "1-50 موظف", icon: "👤" },
  { value: "51-200", label: "51-200 employees", labelAr: "51-200 موظف", icon: "👥" },
  { value: "201-500", label: "201-500 employees", labelAr: "201-500 موظف", icon: "🏢" },
  { value: "501-1000", label: "501-1,000 employees", labelAr: "501-1,000 موظف", icon: "🏛️" },
  { value: "1000+", label: "1,000+ employees", labelAr: "1,000+ موظف", icon: "🌐" }
]

const erpSystems = [
  { value: "erpnext", label: "ERPNext", description: "Open-source ERP", descriptionAr: "ERP مفتوح المصدر" },
  { value: "odoo", label: "Odoo", description: "Modular business apps", descriptionAr: "تطبيقات أعمال معيارية" },
  { value: "sap", label: "SAP", description: "Enterprise ERP", descriptionAr: "ERP للمؤسسات" },
  { value: "oracle", label: "Oracle", description: "Cloud ERP", descriptionAr: "ERP سحابي" },
  { value: "microsoft", label: "Microsoft Dynamics", description: "Business applications", descriptionAr: "تطبيقات الأعمال" },
  { value: "other", label: "Other / Custom", labelAr: "أخرى / مخصص", description: "Tell us more", descriptionAr: "أخبرنا المزيد" },
  { value: "none", label: "No ERP Yet", labelAr: "لا يوجد ERP بعد", description: "Looking to implement", descriptionAr: "نتطلع للتنفيذ" }
]

const mainPains = [
  { value: "approvals", label: "Slow Approvals", labelAr: "بطء الموافقات", description: "Bottlenecks in approval workflows", descriptionAr: "اختناقات في سير عمل الموافقات", icon: "⏱️" },
  { value: "close", label: "Month-End Close", labelAr: "إغلاق نهاية الشهر", description: "Manual reconciliation & reporting", descriptionAr: "التسوية اليدوية والتقارير", icon: "📊" },
  { value: "onboarding", label: "Employee Onboarding", labelAr: "تأهيل الموظفين", description: "HR & IT provisioning delays", descriptionAr: "تأخير في توفير الموارد البشرية وتقنية المعلومات", icon: "🚀" },
  { value: "compliance", label: "Compliance Tracking", labelAr: "تتبع الامتثال", description: "Audit readiness & documentation", descriptionAr: "الجاهزية للتدقيق والتوثيق", icon: "✅" },
  { value: "visibility", label: "Lack of Visibility", labelAr: "نقص الرؤية", description: "No unified dashboard", descriptionAr: "لا توجد لوحة تحكم موحدة", icon: "👁️" }
]

const content = {
  badge: { en: "Get Started Today", ar: "ابدأ اليوم" },
  title: { en: "Let's Find the Right Solution for You", ar: "دعنا نجد الحل المناسب لك" },
  subtitle: { en: "Tell us about your organization and we'll customize a demo for your needs.", ar: "أخبرنا عن مؤسستك وسنخصص عرضاً توضيحياً لاحتياجاتك." },
  success: {
    title: { en: "Thank You!", ar: "شكراً لك!" },
    message: { en: "We'll reach out within 24 hours to schedule your personalized demo.", ar: "سنتواصل معك خلال 24 ساعة لجدولة العرض التوضيحي المخصص لك." },
    checkInbox: { en: "Check your inbox for confirmation", ar: "تحقق من صندوق الوارد للتأكيد" }
  },
  progress: {
    step: { en: "Step", ar: "الخطوة" },
    of: { en: "of", ar: "من" },
    complete: { en: "complete", ar: "مكتملة" }
  },
  step1: {
    title: { en: "Let's Get Started", ar: "لنبدأ" },
    subtitle: { en: "Tell us about yourself and your organization", ar: "أخبرنا عن نفسك ومؤسستك" },
    emailLabel: { en: "Work Email *", ar: "البريد الإلكتروني للعمل *" },
    emailPlaceholder: { en: "you@company.com", ar: "you@company.com" },
    companySizeLabel: { en: "Company Size *", ar: "حجم الشركة *" },
    sectorLabel: { en: "Organization Sector *", ar: "قطاع المؤسسة *" }
  },
  step2: {
    title: { en: "Your ERP System", ar: "نظام ERP الخاص بك" },
    subtitle: { en: "Which ERP do you currently use?", ar: "أي ERP تستخدم حالياً؟" },
    selectLabel: { en: "Select Your ERP *", ar: "اختر نظام ERP الخاص بك *" }
  },
  step3: {
    title: { en: "Your Main Challenge", ar: "التحدي الرئيسي لديك" },
    subtitle: { en: "What's slowing your team down the most?", ar: "ما الذي يبطئ فريقك أكثر؟" },
    selectLabel: { en: "Select Primary Pain Point *", ar: "اختر نقطة الألم الرئيسية *" }
  },
  buttons: {
    back: { en: "Back", ar: "رجوع" },
    continue: { en: "Continue", ar: "متابعة" },
    getDemo: { en: "Get My Demo", ar: "احصل على العرض التوضيحي" }
  },
  trustNote: { en: "No credit card required • 14-day free trial • Cancel anytime", ar: "لا يتطلب بطاقة ائتمان • تجربة مجانية 14 يوماً • إلغاء في أي وقت" }
}

export function LeadForm() {
  const { locale } = useLocale()
  const isArabic = locale === "ar"
  const [step, setStep] = useState(1)
  const [formData, setFormData] = useState<FormData>({
    email: "",
    companySize: "",
    sector: "",
    erpSystem: "",
    mainPain: ""
  })
  const [submitted, setSubmitted] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const totalSteps = 3

  const updateForm = (field: keyof FormData, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }))
  }

  const nextStep = () => {
    if (step < totalSteps) setStep(step + 1)
  }

  const prevStep = () => {
    if (step > 1) setStep(step - 1)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    setError(null)

    try {
      // Get UTM parameters if available
      const urlParams = typeof window !== 'undefined'
        ? new URLSearchParams(window.location.search)
        : null

      const response = await leadApi.submit({
        email: formData.email,
        companySize: formData.companySize,
        sector: formData.sector,
        erpSystem: formData.erpSystem,
        mainPain: formData.mainPain,
        source: 'landing_page',
        utmSource: urlParams?.get('utm_source') || undefined,
        utmMedium: urlParams?.get('utm_medium') || undefined,
        utmCampaign: urlParams?.get('utm_campaign') || undefined
      })

      if (response.success) {
        setSubmitted(true)
      } else {
        setError(response.message || (isArabic ? "فشل إرسال الطلب" : "Failed to submit request"))
      }
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message || (isArabic ? "فشل إرسال الطلب" : "Failed to submit request"))
      } else {
        setError(isArabic ? "حدث خطأ في الاتصال بالخادم" : "Connection error. Please try again.")
      }
    } finally {
      setIsLoading(false)
    }
  }

  const canProceed = () => {
    switch (step) {
      case 1:
        return formData.email && formData.companySize && formData.sector
      case 2:
        return formData.erpSystem
      case 3:
        return formData.mainPain
      default:
        return false
    }
  }

  return (
    <section className="py-24 bg-gradient-to-br from-emerald-600 via-emerald-700 to-teal-800 relative overflow-hidden">
      {/* Background Pattern */}
      <div className="absolute inset-0 opacity-10">
        <svg className="w-full h-full" xmlns="http://www.w3.org/2000/svg">
          <defs>
            <pattern id="lead-grid" width="40" height="40" patternUnits="userSpaceOnUse">
              <path d="M 40 0 L 0 0 0 40" fill="none" stroke="white" strokeWidth="1"/>
            </pattern>
          </defs>
          <rect width="100%" height="100%" fill="url(#lead-grid)" />
        </svg>
      </div>

      <div className="container mx-auto px-6 relative z-10">
        <div className="max-w-2xl mx-auto">
          {/* Header */}
          <motion.div
            className="text-center mb-10"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-white/10 text-white text-sm font-medium mb-6">
              <Sparkles className="w-4 h-4" />
              {isArabic ? content.badge.ar : content.badge.en}
            </span>
            <h2 className="text-3xl md:text-4xl font-bold text-white mb-4">
              {isArabic ? content.title.ar : content.title.en}
            </h2>
            <p className="text-emerald-100 text-lg">
              {isArabic ? content.subtitle.ar : content.subtitle.en}
            </p>
          </motion.div>

          {/* Form Card */}
          <motion.div
            className="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl overflow-hidden"
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            {submitted ? (
              /* Success State */
              <div className="p-12 text-center">
                <div className="w-20 h-20 rounded-full bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center mx-auto mb-6">
                  <CheckCircle className="w-10 h-10 text-emerald-600" />
                </div>
                <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-3">
                  {isArabic ? content.success.title.ar : content.success.title.en}
                </h3>
                <p className="text-gray-600 dark:text-gray-400 mb-6">
                  {isArabic ? content.success.message.ar : content.success.message.en}
                </p>
                <div className="inline-flex items-center gap-2 px-4 py-2 bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400 rounded-lg text-sm">
                  <Mail className="w-4 h-4" />
                  {isArabic ? content.success.checkInbox.ar : content.success.checkInbox.en}
                </div>
              </div>
            ) : (
              <>
                {/* Progress Bar */}
                <div className="bg-gray-50 dark:bg-gray-900 px-8 py-4">
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-sm font-medium text-gray-600 dark:text-gray-400">
                      {isArabic ? content.progress.step.ar : content.progress.step.en} {step} {isArabic ? content.progress.of.ar : content.progress.of.en} {totalSteps}
                    </span>
                    <span className="text-sm text-gray-500 dark:text-gray-500">
                      {Math.round((step / totalSteps) * 100)}% {isArabic ? content.progress.complete.ar : content.progress.complete.en}
                    </span>
                  </div>
                  <div className="h-2 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
                    <motion.div
                      className="h-full bg-emerald-500"
                      initial={{ width: 0 }}
                      animate={{ width: `${(step / totalSteps) * 100}%` }}
                      transition={{ duration: 0.3 }}
                    />
                  </div>
                </div>

                {/* Form Steps */}
                <form onSubmit={handleSubmit} className="p-8">
                  <AnimatePresence mode="wait">
                    {step === 1 && (
                      <motion.div
                        key="step1"
                        initial={{ opacity: 0, x: isArabic ? -20 : 20 }}
                        animate={{ opacity: 1, x: 0 }}
                        exit={{ opacity: 0, x: isArabic ? 20 : -20 }}
                        className="space-y-6"
                      >
                        <div className="flex items-center gap-3 mb-6">
                          <div className="w-10 h-10 rounded-xl bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center">
                            <Mail className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
                          </div>
                          <div>
                            <h3 className="font-semibold text-gray-900 dark:text-white">
                              {isArabic ? content.step1.title.ar : content.step1.title.en}
                            </h3>
                            <p className="text-sm text-gray-500 dark:text-gray-400">
                              {isArabic ? content.step1.subtitle.ar : content.step1.subtitle.en}
                            </p>
                          </div>
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                            {isArabic ? content.step1.emailLabel.ar : content.step1.emailLabel.en}
                          </label>
                          <input
                            type="email"
                            required
                            value={formData.email}
                            onChange={(e) => updateForm("email", e.target.value)}
                            placeholder={isArabic ? content.step1.emailPlaceholder.ar : content.step1.emailPlaceholder.en}
                            className="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-xl bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-400 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 outline-none"
                          />
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                            {isArabic ? content.step1.companySizeLabel.ar : content.step1.companySizeLabel.en}
                          </label>
                          <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                            {companySizes.map((size) => (
                              <button
                                key={size.value}
                                type="button"
                                onClick={() => updateForm("companySize", size.value)}
                                className={`p-3 rounded-xl border text-sm font-medium transition-all ${
                                  formData.companySize === size.value
                                    ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400"
                                    : "border-gray-200 dark:border-gray-600 text-gray-700 dark:text-gray-300 hover:border-gray-300 dark:hover:border-gray-500"
                                }`}
                              >
                                <span className="text-lg mb-1 block">{size.icon}</span>
                                <span className="text-xs">{isArabic ? size.labelAr : size.label}</span>
                              </button>
                            ))}
                          </div>
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                            {isArabic ? content.step1.sectorLabel.ar : content.step1.sectorLabel.en}
                          </label>
                          <SectorSelect
                            value={formData.sector}
                            onChange={(value) => updateForm("sector", value)}
                            required
                            placeholder="Select your sector"
                            placeholderAr="اختر قطاع المؤسسة"
                          />
                        </div>
                      </motion.div>
                    )}

                    {step === 2 && (
                      <motion.div
                        key="step2"
                        initial={{ opacity: 0, x: isArabic ? -20 : 20 }}
                        animate={{ opacity: 1, x: 0 }}
                        exit={{ opacity: 0, x: isArabic ? 20 : -20 }}
                        className="space-y-6"
                      >
                        <div className="flex items-center gap-3 mb-6">
                          <div className="w-10 h-10 rounded-xl bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center">
                            <Database className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
                          </div>
                          <div>
                            <h3 className="font-semibold text-gray-900 dark:text-white">
                              {isArabic ? content.step2.title.ar : content.step2.title.en}
                            </h3>
                            <p className="text-sm text-gray-500 dark:text-gray-400">
                              {isArabic ? content.step2.subtitle.ar : content.step2.subtitle.en}
                            </p>
                          </div>
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
                            {isArabic ? content.step2.selectLabel.ar : content.step2.selectLabel.en}
                          </label>
                          <div className="grid grid-cols-2 gap-3">
                            {erpSystems.map((erp) => (
                              <button
                                key={erp.value}
                                type="button"
                                onClick={() => updateForm("erpSystem", erp.value)}
                                className={`p-4 rounded-xl border ${isArabic ? "text-right" : "text-left"} transition-all ${
                                  formData.erpSystem === erp.value
                                    ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-900/20"
                                    : "border-gray-200 dark:border-gray-600 hover:border-gray-300 dark:hover:border-gray-500"
                                }`}
                              >
                                <span className={`font-semibold text-sm ${
                                  formData.erpSystem === erp.value
                                    ? "text-emerald-700 dark:text-emerald-400"
                                    : "text-gray-900 dark:text-white"
                                }`}>
                                  {isArabic && erp.labelAr ? erp.labelAr : erp.label}
                                </span>
                                <span className="text-xs text-gray-500 dark:text-gray-400 block mt-1">
                                  {isArabic ? erp.descriptionAr : erp.description}
                                </span>
                              </button>
                            ))}
                          </div>
                        </div>
                      </motion.div>
                    )}

                    {step === 3 && (
                      <motion.div
                        key="step3"
                        initial={{ opacity: 0, x: isArabic ? -20 : 20 }}
                        animate={{ opacity: 1, x: 0 }}
                        exit={{ opacity: 0, x: isArabic ? 20 : -20 }}
                        className="space-y-6"
                      >
                        <div className="flex items-center gap-3 mb-6">
                          <div className="w-10 h-10 rounded-xl bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center">
                            <Settings className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
                          </div>
                          <div>
                            <h3 className="font-semibold text-gray-900 dark:text-white">
                              {isArabic ? content.step3.title.ar : content.step3.title.en}
                            </h3>
                            <p className="text-sm text-gray-500 dark:text-gray-400">
                              {isArabic ? content.step3.subtitle.ar : content.step3.subtitle.en}
                            </p>
                          </div>
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
                            {isArabic ? content.step3.selectLabel.ar : content.step3.selectLabel.en}
                          </label>
                          <div className="space-y-3">
                            {mainPains.map((pain) => (
                              <button
                                key={pain.value}
                                type="button"
                                onClick={() => updateForm("mainPain", pain.value)}
                                className={`w-full p-4 rounded-xl border ${isArabic ? "text-right" : "text-left"} transition-all flex items-start gap-4 ${
                                  formData.mainPain === pain.value
                                    ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-900/20"
                                    : "border-gray-200 dark:border-gray-600 hover:border-gray-300 dark:hover:border-gray-500"
                                }`}
                              >
                                <span className="text-2xl">{pain.icon}</span>
                                <div>
                                  <span className={`font-semibold block ${
                                    formData.mainPain === pain.value
                                      ? "text-emerald-700 dark:text-emerald-400"
                                      : "text-gray-900 dark:text-white"
                                  }`}>
                                    {isArabic ? pain.labelAr : pain.label}
                                  </span>
                                  <span className="text-sm text-gray-500 dark:text-gray-400">
                                    {isArabic ? pain.descriptionAr : pain.description}
                                  </span>
                                </div>
                              </button>
                            ))}
                          </div>
                        </div>
                      </motion.div>
                    )}
                  </AnimatePresence>

                  {/* Navigation */}
                  <div className="flex justify-between mt-8 pt-6 border-t border-gray-200 dark:border-gray-700">
                    {step > 1 ? (
                      <Button
                        type="button"
                        variant="outline"
                        onClick={prevStep}
                        className="gap-2"
                      >
                        <ArrowLeft className={`w-4 h-4 ${isArabic ? "order-last" : ""}`} />
                        {isArabic ? content.buttons.back.ar : content.buttons.back.en}
                      </Button>
                    ) : (
                      <div />
                    )}

                    {step < totalSteps ? (
                      <Button
                        type="button"
                        onClick={nextStep}
                        disabled={!canProceed()}
                        className="bg-emerald-600 hover:bg-emerald-700 text-white gap-2"
                      >
                        {isArabic ? content.buttons.continue.ar : content.buttons.continue.en}
                        <ArrowRight className={`w-4 h-4 ${isArabic ? "rotate-180" : ""}`} />
                      </Button>
                    ) : (
                      <Button
                        type="submit"
                        disabled={!canProceed()}
                        className="bg-emerald-600 hover:bg-emerald-700 text-white gap-2"
                      >
                        {isArabic ? content.buttons.getDemo.ar : content.buttons.getDemo.en}
                        <ArrowRight className={`w-4 h-4 ${isArabic ? "rotate-180" : ""}`} />
                      </Button>
                    )}
                  </div>
                </form>
              </>
            )}
          </motion.div>

          {/* Trust Note */}
          <p className="text-center text-emerald-200 text-sm mt-6">
            {isArabic ? content.trustNote.ar : content.trustNote.en}
          </p>
        </div>
      </div>
    </section>
  )
}
