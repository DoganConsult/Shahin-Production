"use client"

import { useState } from "react"
import { motion } from "framer-motion"
import {
  Shield,
  Lock,
  Key,
  Server,
  FileText,
  Users,
  Eye,
  CheckCircle,
  Download,
  X
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { useLocale } from "@/components/providers/locale-provider"

const securityFeatures = [
  {
    icon: Lock,
    title: "End-to-End Encryption",
    titleAr: "التشفير من طرف إلى طرف",
    description: "AES-256 encryption for data at rest and TLS 1.3 for data in transit",
    descriptionAr: "تشفير AES-256 للبيانات المخزنة و TLS 1.3 للبيانات المنقولة"
  },
  {
    icon: Key,
    title: "Role-Based Access Control",
    titleAr: "التحكم بالوصول حسب الدور",
    description: "Granular permissions with multi-level approval workflows",
    descriptionAr: "صلاحيات دقيقة مع سير عمل الموافقات متعدد المستويات"
  },
  {
    icon: Eye,
    title: "Complete Audit Trail",
    titleAr: "سجل تدقيق كامل",
    description: "Every action logged and timestamped for full accountability",
    descriptionAr: "كل إجراء مسجل ومؤرخ للمساءلة الكاملة"
  },
  {
    icon: Server,
    title: "Saudi Data Residency",
    titleAr: "استضافة البيانات في السعودية",
    description: "Data stored in Saudi Arabia with local cloud options",
    descriptionAr: "البيانات مخزنة في السعودية مع خيارات السحابة المحلية"
  },
  {
    icon: Users,
    title: "SSO & MFA Support",
    titleAr: "دعم SSO و MFA",
    description: "Enterprise authentication with SAML, OAuth, and MFA",
    descriptionAr: "مصادقة المؤسسات مع SAML و OAuth و MFA"
  },
  {
    icon: FileText,
    title: "Compliance Certifications",
    titleAr: "شهادات الامتثال",
    description: "SOC 2 Type II, ISO 27001, and NCA certified",
    descriptionAr: "معتمد من SOC 2 Type II و ISO 27001 و NCA"
  }
]

const complianceStandards = [
  { name: "ISO 27001", status: "Certified", statusAr: "معتمد" },
  { name: "SOC 2 Type II", status: "Certified", statusAr: "معتمد" },
  { name: "NCA ECC-1", status: "Compliant", statusAr: "ممتثل" },
  { name: "SAMA CSF", status: "Compliant", statusAr: "ممتثل" },
  { name: "PDPL", status: "Compliant", statusAr: "ممتثل" },
  { name: "GDPR", status: "Ready", statusAr: "جاهز" }
]

const content = {
  sectionLabel: { en: "Enterprise Security", ar: "أمان المؤسسات" },
  title: { en: "Security You Can Trust", ar: "أمان يمكنك الوثوق به" },
  subtitle: {
    en: "Built from the ground up with enterprise-grade security and compliance at its core.",
    ar: "مبني من الأساس بأمان على مستوى المؤسسات والامتثال في جوهره."
  },
  complianceStandards: { en: "Compliance Standards", ar: "معايير الامتثال" },
  securityBrief: {
    title: { en: "Security & Compliance Brief", ar: "ملخص الأمان والامتثال" },
    description: {
      en: "Download our comprehensive security documentation, including architecture details, compliance certifications, and penetration test summaries.",
      ar: "قم بتنزيل وثائق الأمان الشاملة لدينا، بما في ذلك تفاصيل البنية، وشهادات الامتثال، وملخصات اختبارات الاختراق."
    },
    items: {
      architecture: { en: "Security architecture overview", ar: "نظرة عامة على بنية الأمان" },
      certifications: { en: "Compliance certifications", ar: "شهادات الامتثال" },
      dataProtection: { en: "Data protection policies", ar: "سياسات حماية البيانات" },
      incidentResponse: { en: "Incident response procedures", ar: "إجراءات الاستجابة للحوادث" }
    },
    cta: { en: "Download Security Brief", ar: "تنزيل ملخص الأمان" }
  },
  modal: {
    title: { en: "Download Security Brief", ar: "تنزيل ملخص الأمان" },
    subtitle: { en: "Enter your email to receive the document", ar: "أدخل بريدك الإلكتروني لاستلام المستند" },
    emailLabel: { en: "Work Email", ar: "البريد الإلكتروني للعمل" },
    emailPlaceholder: { en: "you@company.com", ar: "you@company.com" },
    submitBtn: { en: "Send Me the Brief", ar: "أرسل لي الملخص" },
    disclaimer: {
      en: "By downloading, you agree to receive occasional product updates. Unsubscribe anytime.",
      ar: "بالتنزيل، أنت توافق على تلقي تحديثات المنتج من حين لآخر. يمكنك إلغاء الاشتراك في أي وقت."
    },
    success: {
      title: { en: "Check Your Email!", ar: "تحقق من بريدك الإلكتروني!" },
      description: { en: "We've sent the security brief to your inbox.", ar: "لقد أرسلنا ملخص الأمان إلى صندوق الوارد الخاص بك." }
    }
  }
}

export function TrustPanel() {
  const { locale } = useLocale()
  const isArabic = locale === "ar"
  const [showLeadModal, setShowLeadModal] = useState(false)
  const [email, setEmail] = useState("")
  const [submitted, setSubmitted] = useState(false)

  const handleDownload = (e: React.FormEvent) => {
    e.preventDefault()
    // In production, this would submit to your backend
    setSubmitted(true)
    setTimeout(() => {
      setShowLeadModal(false)
      setSubmitted(false)
      setEmail("")
    }, 2000)
  }

  return (
    <section className="py-24 bg-gray-900">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-16"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <span className="text-emerald-400 font-semibold mb-4 block">
            {isArabic ? content.sectionLabel.ar : content.sectionLabel.en}
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-white mb-4">
            {isArabic ? content.title.ar : content.title.en}
          </h2>
          <p className="text-lg text-gray-400 max-w-2xl mx-auto">
            {isArabic ? content.subtitle.ar : content.subtitle.en}
          </p>
        </motion.div>

        <div className="max-w-6xl mx-auto">
          {/* Security Features Grid */}
          <motion.div
            className="grid md:grid-cols-2 lg:grid-cols-3 gap-6 mb-12"
            initial={{ opacity: 0 }}
            whileInView={{ opacity: 1 }}
            viewport={{ once: true }}
          >
            {securityFeatures.map((feature, index) => (
              <motion.div
                key={feature.title}
                className="bg-gray-800/50 backdrop-blur border border-gray-700 rounded-xl p-6 hover:border-emerald-500/50 transition-colors"
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
              >
                <div className="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center mb-4">
                  <feature.icon className="w-6 h-6 text-emerald-400" />
                </div>
                <h3 className="text-lg font-semibold text-white mb-2">
                  {isArabic ? feature.titleAr : feature.title}
                </h3>
                <p className="text-gray-400 text-sm">
                  {isArabic ? feature.descriptionAr : feature.description}
                </p>
              </motion.div>
            ))}
          </motion.div>

          {/* Compliance Standards & Download */}
          <motion.div
            className="grid lg:grid-cols-2 gap-8"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            {/* Standards Grid */}
            <div className="bg-gray-800/50 backdrop-blur border border-gray-700 rounded-xl p-8">
              <h3 className="text-xl font-bold text-white mb-6 flex items-center gap-2">
                <Shield className="w-5 h-5 text-emerald-400" />
                {isArabic ? content.complianceStandards.ar : content.complianceStandards.en}
              </h3>
              <div className="grid grid-cols-2 gap-4">
                {complianceStandards.map((standard) => (
                  <div
                    key={standard.name}
                    className="flex items-center justify-between p-3 bg-gray-900/50 rounded-lg"
                  >
                    <span className="text-gray-300 text-sm font-medium">
                      {standard.name}
                    </span>
                    <span className={`text-xs px-2 py-1 rounded-full ${
                      standard.status === "Certified"
                        ? "bg-emerald-500/20 text-emerald-400"
                        : standard.status === "Compliant"
                        ? "bg-blue-500/20 text-blue-400"
                        : "bg-yellow-500/20 text-yellow-400"
                    }`}>
                      {isArabic ? standard.statusAr : standard.status}
                    </span>
                  </div>
                ))}
              </div>
            </div>

            {/* Security Brief Download */}
            <div className="bg-gradient-to-br from-emerald-600 to-teal-700 rounded-xl p-8 text-white">
              <div className="w-16 h-16 rounded-xl bg-white/20 flex items-center justify-center mb-6">
                <FileText className="w-8 h-8" />
              </div>
              <h3 className="text-2xl font-bold mb-3">
                {isArabic ? content.securityBrief.title.ar : content.securityBrief.title.en}
              </h3>
              <p className="text-emerald-100 mb-6">
                {isArabic ? content.securityBrief.description.ar : content.securityBrief.description.en}
              </p>
              <ul className="space-y-2 mb-8">
                <li className="flex items-center gap-2 text-emerald-100">
                  <CheckCircle className="w-4 h-4" />
                  <span className="text-sm">{isArabic ? content.securityBrief.items.architecture.ar : content.securityBrief.items.architecture.en}</span>
                </li>
                <li className="flex items-center gap-2 text-emerald-100">
                  <CheckCircle className="w-4 h-4" />
                  <span className="text-sm">{isArabic ? content.securityBrief.items.certifications.ar : content.securityBrief.items.certifications.en}</span>
                </li>
                <li className="flex items-center gap-2 text-emerald-100">
                  <CheckCircle className="w-4 h-4" />
                  <span className="text-sm">{isArabic ? content.securityBrief.items.dataProtection.ar : content.securityBrief.items.dataProtection.en}</span>
                </li>
                <li className="flex items-center gap-2 text-emerald-100">
                  <CheckCircle className="w-4 h-4" />
                  <span className="text-sm">{isArabic ? content.securityBrief.items.incidentResponse.ar : content.securityBrief.items.incidentResponse.en}</span>
                </li>
              </ul>
              <Button
                onClick={() => setShowLeadModal(true)}
                className="w-full bg-white text-emerald-700 hover:bg-gray-100 py-6 text-lg font-semibold group"
              >
                <Download className={`w-5 h-5 ${isArabic ? "ml-2" : "mr-2"}`} />
                {isArabic ? content.securityBrief.cta.ar : content.securityBrief.cta.en}
              </Button>
            </div>
          </motion.div>
        </div>
      </div>

      {/* Lead Capture Modal */}
      {showLeadModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <motion.div
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            className="bg-white dark:bg-gray-800 rounded-2xl p-8 max-w-md w-full shadow-2xl"
          >
            <div className="flex justify-between items-start mb-6">
              <div>
                <h3 className="text-xl font-bold text-gray-900 dark:text-white">
                  {isArabic ? content.modal.title.ar : content.modal.title.en}
                </h3>
                <p className="text-gray-600 dark:text-gray-400 text-sm mt-1">
                  {isArabic ? content.modal.subtitle.ar : content.modal.subtitle.en}
                </p>
              </div>
              <button
                onClick={() => setShowLeadModal(false)}
                className="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
              >
                <X className="w-5 h-5 text-gray-500" />
              </button>
            </div>

            {submitted ? (
              <div className="text-center py-8">
                <div className="w-16 h-16 rounded-full bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center mx-auto mb-4">
                  <CheckCircle className="w-8 h-8 text-emerald-600" />
                </div>
                <h4 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                  {isArabic ? content.modal.success.title.ar : content.modal.success.title.en}
                </h4>
                <p className="text-gray-600 dark:text-gray-400 text-sm">
                  {isArabic ? content.modal.success.description.ar : content.modal.success.description.en}
                </p>
              </div>
            ) : (
              <form onSubmit={handleDownload}>
                <div className="mb-4">
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    {isArabic ? content.modal.emailLabel.ar : content.modal.emailLabel.en}
                  </label>
                  <input
                    type="email"
                    required
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder={isArabic ? content.modal.emailPlaceholder.ar : content.modal.emailPlaceholder.en}
                    className="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-xl bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-400 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 outline-none transition-colors"
                  />
                </div>
                <Button
                  type="submit"
                  className="w-full bg-emerald-600 hover:bg-emerald-700 text-white py-6"
                >
                  <Download className={`w-5 h-5 ${isArabic ? "ml-2" : "mr-2"}`} />
                  {isArabic ? content.modal.submitBtn.ar : content.modal.submitBtn.en}
                </Button>
                <p className="text-xs text-gray-500 dark:text-gray-400 text-center mt-4">
                  {isArabic ? content.modal.disclaimer.ar : content.modal.disclaimer.en}
                </p>
              </form>
            )}
          </motion.div>
        </div>
      )}
    </section>
  )
}
