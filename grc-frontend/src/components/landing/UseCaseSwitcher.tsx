"use client"

import { useState } from "react"
import { motion, AnimatePresence } from "framer-motion"
import {
  Building2,
  Landmark,
  HeartPulse,
  Factory,
  ShoppingBag,
  CheckCircle,
  ArrowRight,
  Shield,
  AlertTriangle,
  TrendingUp,
  BarChart3,
  Lock,
  FileCheck,
  Activity
} from "lucide-react"
import { Button } from "@/components/ui/button"
import Link from "next/link"
import { useLocale } from "@/components/providers/locale-provider"
import { useTranslations } from "next-intl"
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  RadarChart,
  Radar,
  PolarGrid,
  PolarAngleAxis,
  PolarRadiusAxis
} from "recharts"

// Sector-specific controls data
const sectorControls = {
  financial: [
    { id: "ctrl-1", name: { en: "Data Encryption", ar: "تشفير البيانات" }, status: "compliant", coverage: 95 },
    { id: "ctrl-2", name: { en: "Access Control", ar: "التحكم بالوصول" }, status: "compliant", coverage: 88 },
    { id: "ctrl-3", name: { en: "Audit Logging", ar: "سجلات التدقيق" }, status: "in_progress", coverage: 72 },
    { id: "ctrl-4", name: { en: "Incident Response", ar: "الاستجابة للحوادث" }, status: "compliant", coverage: 90 },
    { id: "ctrl-5", name: { en: "Vendor Management", ar: "إدارة الموردين" }, status: "pending", coverage: 45 },
  ],
  healthcare: [
    { id: "ctrl-1", name: { en: "Patient Data Protection", ar: "حماية بيانات المرضى" }, status: "compliant", coverage: 92 },
    { id: "ctrl-2", name: { en: "Medical Device Security", ar: "أمان الأجهزة الطبية" }, status: "in_progress", coverage: 68 },
    { id: "ctrl-3", name: { en: "Clinical Data Integrity", ar: "سلامة البيانات السريرية" }, status: "compliant", coverage: 85 },
    { id: "ctrl-4", name: { en: "Health Record Access", ar: "الوصول للسجلات الصحية" }, status: "compliant", coverage: 94 },
    { id: "ctrl-5", name: { en: "Consent Management", ar: "إدارة الموافقات" }, status: "pending", coverage: 55 },
  ],
  government: [
    { id: "ctrl-1", name: { en: "NCA ECC Controls", ar: "ضوابط NCA ECC" }, status: "compliant", coverage: 100 },
    { id: "ctrl-2", name: { en: "Data Classification", ar: "تصنيف البيانات" }, status: "compliant", coverage: 96 },
    { id: "ctrl-3", name: { en: "Network Security", ar: "أمن الشبكات" }, status: "compliant", coverage: 88 },
    { id: "ctrl-4", name: { en: "Identity Management", ar: "إدارة الهوية" }, status: "in_progress", coverage: 75 },
    { id: "ctrl-5", name: { en: "Cloud Security", ar: "أمن السحابة" }, status: "compliant", coverage: 82 },
  ],
  industrial: [
    { id: "ctrl-1", name: { en: "OT Security", ar: "أمن التقنية التشغيلية" }, status: "compliant", coverage: 78 },
    { id: "ctrl-2", name: { en: "SCADA Protection", ar: "حماية SCADA" }, status: "in_progress", coverage: 65 },
    { id: "ctrl-3", name: { en: "Supply Chain Security", ar: "أمن سلسلة التوريد" }, status: "compliant", coverage: 72 },
    { id: "ctrl-4", name: { en: "Safety Systems", ar: "أنظمة السلامة" }, status: "compliant", coverage: 90 },
    { id: "ctrl-5", name: { en: "Environmental Controls", ar: "الضوابط البيئية" }, status: "pending", coverage: 58 },
  ],
  retail: [
    { id: "ctrl-1", name: { en: "PCI DSS Compliance", ar: "امتثال PCI DSS" }, status: "compliant", coverage: 88 },
    { id: "ctrl-2", name: { en: "Customer Data Privacy", ar: "خصوصية بيانات العملاء" }, status: "compliant", coverage: 92 },
    { id: "ctrl-3", name: { en: "E-commerce Security", ar: "أمن التجارة الإلكترونية" }, status: "in_progress", coverage: 75 },
    { id: "ctrl-4", name: { en: "Payment Processing", ar: "معالجة المدفوعات" }, status: "compliant", coverage: 95 },
    { id: "ctrl-5", name: { en: "Inventory Protection", ar: "حماية المخزون" }, status: "pending", coverage: 60 },
  ],
}

// Sector-specific risks data
const sectorRisks = {
  financial: [
    { id: "risk-1", name: { en: "Cyber Attack", ar: "هجوم سيبراني" }, severity: "high", likelihood: 75, impact: 90 },
    { id: "risk-2", name: { en: "Data Breach", ar: "اختراق البيانات" }, severity: "critical", likelihood: 60, impact: 95 },
    { id: "risk-3", name: { en: "Fraud Attempts", ar: "محاولات الاحتيال" }, severity: "medium", likelihood: 80, impact: 70 },
    { id: "risk-4", name: { en: "Regulatory Fines", ar: "غرامات تنظيمية" }, severity: "high", likelihood: 40, impact: 85 },
  ],
  healthcare: [
    { id: "risk-1", name: { en: "Patient Data Leak", ar: "تسريب بيانات المرضى" }, severity: "critical", likelihood: 55, impact: 98 },
    { id: "risk-2", name: { en: "Ransomware", ar: "برامج الفدية" }, severity: "high", likelihood: 70, impact: 88 },
    { id: "risk-3", name: { en: "Device Tampering", ar: "العبث بالأجهزة" }, severity: "medium", likelihood: 35, impact: 75 },
    { id: "risk-4", name: { en: "Compliance Violation", ar: "مخالفة الامتثال" }, severity: "high", likelihood: 45, impact: 80 },
  ],
  government: [
    { id: "risk-1", name: { en: "Nation-State Attack", ar: "هجوم حكومي" }, severity: "critical", likelihood: 65, impact: 100 },
    { id: "risk-2", name: { en: "Insider Threat", ar: "تهديد داخلي" }, severity: "high", likelihood: 50, impact: 85 },
    { id: "risk-3", name: { en: "Data Exfiltration", ar: "تسريب البيانات" }, severity: "critical", likelihood: 55, impact: 95 },
    { id: "risk-4", name: { en: "Service Disruption", ar: "تعطل الخدمات" }, severity: "medium", likelihood: 60, impact: 70 },
  ],
  industrial: [
    { id: "risk-1", name: { en: "OT System Breach", ar: "اختراق نظام OT" }, severity: "critical", likelihood: 45, impact: 95 },
    { id: "risk-2", name: { en: "Supply Chain Attack", ar: "هجوم سلسلة التوريد" }, severity: "high", likelihood: 55, impact: 80 },
    { id: "risk-3", name: { en: "Safety Incident", ar: "حادث سلامة" }, severity: "critical", likelihood: 30, impact: 100 },
    { id: "risk-4", name: { en: "Environmental Damage", ar: "ضرر بيئي" }, severity: "high", likelihood: 35, impact: 85 },
  ],
  retail: [
    { id: "risk-1", name: { en: "Payment Fraud", ar: "احتيال المدفوعات" }, severity: "high", likelihood: 70, impact: 80 },
    { id: "risk-2", name: { en: "Customer Data Theft", ar: "سرقة بيانات العملاء" }, severity: "critical", likelihood: 60, impact: 90 },
    { id: "risk-3", name: { en: "Website Compromise", ar: "اختراق الموقع" }, severity: "medium", likelihood: 65, impact: 65 },
    { id: "risk-4", name: { en: "Vendor Breach", ar: "اختراق الموردين" }, severity: "high", likelihood: 50, impact: 75 },
  ],
}

// Chart colors
const CHART_COLORS = {
  compliant: "#10b981",
  in_progress: "#f59e0b",
  pending: "#ef4444",
  primary: "#10b981",
  secondary: "#3b82f6",
  tertiary: "#8b5cf6",
}

const useCases = [
  {
    id: "financial",
    icon: Landmark,
    name: "Financial Services",
    nameAr: "الخدمات المالية",
    tagline: "Banks, Insurance, Investment Firms",
    taglineAr: "البنوك، التأمين، شركات الاستثمار",
    description: "Meet SAMA cybersecurity requirements and protect sensitive financial data with comprehensive compliance automation.",
    descriptionAr: "تلبية متطلبات الأمن السيبراني لـ SAMA وحماية البيانات المالية الحساسة مع أتمتة الامتثال الشاملة.",
    benefits: [
      "SAMA Cybersecurity Framework compliance",
      "PCI DSS audit readiness",
      "Anti-money laundering (AML) controls",
      "Real-time fraud risk monitoring",
      "Third-party vendor risk management"
    ],
    benefitsAr: [
      "الامتثال لإطار الأمن السيبراني SAMA",
      "الجاهزية لتدقيق PCI DSS",
      "ضوابط مكافحة غسيل الأموال (AML)",
      "مراقبة مخاطر الاحتيال في الوقت الفعلي",
      "إدارة مخاطر الطرف الثالث"
    ],
    stat: "85%",
    statLabel: "faster SAMA compliance",
    statLabelAr: "امتثال SAMA أسرع",
    gradient: "from-blue-500 to-cyan-600"
  },
  {
    id: "healthcare",
    icon: HeartPulse,
    name: "Healthcare",
    nameAr: "الرعاية الصحية",
    tagline: "Hospitals, Clinics, Health Tech",
    taglineAr: "المستشفيات، العيادات، التقنية الصحية",
    description: "Protect patient data and ensure regulatory compliance across healthcare operations with HIPAA and local health regulations.",
    descriptionAr: "حماية بيانات المرضى وضمان الامتثال التنظيمي عبر عمليات الرعاية الصحية مع HIPAA واللوائح الصحية المحلية.",
    benefits: [
      "PDPL patient data protection",
      "Healthcare facility accreditation",
      "Medical device compliance",
      "Clinical trial audit trails",
      "Health information security"
    ],
    benefitsAr: [
      "حماية بيانات المرضى PDPL",
      "اعتماد المنشآت الصحية",
      "امتثال الأجهزة الطبية",
      "مسارات تدقيق التجارب السريرية",
      "أمن المعلومات الصحية"
    ],
    stat: "99%",
    statLabel: "audit success rate",
    statLabelAr: "معدل نجاح التدقيق",
    gradient: "from-rose-500 to-pink-600"
  },
  {
    id: "government",
    icon: Building2,
    name: "Government",
    nameAr: "القطاع الحكومي",
    tagline: "Ministries, Agencies, Public Sector",
    taglineAr: "الوزارات، الجهات الحكومية، القطاع العام",
    description: "Achieve NCA compliance and implement robust governance frameworks for public sector organizations.",
    descriptionAr: "تحقيق امتثال NCA وتطبيق أطر حوكمة قوية لمنظمات القطاع العام.",
    benefits: [
      "NCA ECC-1:2018 full compliance",
      "National data classification",
      "Secure government cloud readiness",
      "Inter-agency audit coordination",
      "Public service risk management"
    ],
    benefitsAr: [
      "الامتثال الكامل لـ NCA ECC-1:2018",
      "تصنيف البيانات الوطنية",
      "جاهزية السحابة الحكومية الآمنة",
      "تنسيق التدقيق بين الجهات",
      "إدارة مخاطر الخدمات العامة"
    ],
    stat: "100%",
    statLabel: "NCA framework coverage",
    statLabelAr: "تغطية إطار NCA",
    gradient: "from-emerald-500 to-teal-600"
  },
  {
    id: "industrial",
    icon: Factory,
    name: "Industrial",
    nameAr: "القطاع الصناعي",
    tagline: "Manufacturing, Energy, Utilities",
    taglineAr: "التصنيع، الطاقة، المرافق",
    description: "Secure operational technology and ensure industrial compliance with sector-specific cybersecurity standards.",
    descriptionAr: "تأمين التكنولوجيا التشغيلية وضمان الامتثال الصناعي مع معايير الأمن السيبراني الخاصة بالقطاع.",
    benefits: [
      "OT/IT security convergence",
      "Industrial control system protection",
      "Supply chain risk management",
      "Environmental compliance tracking",
      "Safety incident management"
    ],
    benefitsAr: [
      "تقارب أمن OT/IT",
      "حماية أنظمة التحكم الصناعي",
      "إدارة مخاطر سلسلة التوريد",
      "تتبع الامتثال البيئي",
      "إدارة حوادث السلامة"
    ],
    stat: "70%",
    statLabel: "risk reduction",
    statLabelAr: "تخفيض المخاطر",
    gradient: "from-orange-500 to-amber-600"
  },
  {
    id: "retail",
    icon: ShoppingBag,
    name: "Retail & E-commerce",
    nameAr: "التجزئة والتجارة الإلكترونية",
    tagline: "Retailers, Marketplaces, Logistics",
    taglineAr: "تجار التجزئة، المتاجر الإلكترونية، الخدمات اللوجستية",
    description: "Protect customer data and ensure payment compliance while managing multi-channel retail operations.",
    descriptionAr: "حماية بيانات العملاء وضمان امتثال الدفع أثناء إدارة عمليات التجزئة متعددة القنوات.",
    benefits: [
      "PCI DSS payment security",
      "Customer data protection (PDPL)",
      "E-commerce platform security",
      "Vendor compliance monitoring",
      "Logistics and fulfillment audits"
    ],
    benefitsAr: [
      "أمان الدفع PCI DSS",
      "حماية بيانات العملاء (PDPL)",
      "أمان منصات التجارة الإلكترونية",
      "مراقبة امتثال الموردين",
      "تدقيق الخدمات اللوجستية والتوصيل"
    ],
    stat: "60%",
    statLabel: "compliance cost reduction",
    statLabelAr: "تخفيض تكاليف الامتثال",
    gradient: "from-purple-500 to-violet-600"
  }
]

export function UseCaseSwitcher() {
  const [activeCase, setActiveCase] = useState(useCases[0])
  const { locale } = useLocale()
  const isArabic = locale === "ar"
  const t = useTranslations("landing.useCases")

  return (
    <section className="py-24 bg-white dark:bg-gray-950">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-12"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <span className="text-emerald-600 dark:text-emerald-400 font-semibold mb-4 block">
            {t("sectionLabel")}
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
            {t("title")}
          </h2>
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
            {t("subtitle")}
          </p>
        </motion.div>

        {/* Tab Navigation */}
        <motion.div
          className="flex flex-wrap justify-center gap-2 mb-12"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          {useCases.map((useCase) => (
            <button
              key={useCase.id}
              onClick={() => setActiveCase(useCase)}
              className={`flex items-center gap-2 px-5 py-3 rounded-xl font-medium transition-all duration-300 ${
                activeCase.id === useCase.id
                  ? "bg-emerald-600 text-white shadow-lg shadow-emerald-500/25"
                  : "bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-700"
              }`}
            >
              <useCase.icon className="w-5 h-5" />
              <span className="hidden sm:inline">{isArabic ? useCase.nameAr : useCase.name}</span>
            </button>
          ))}
        </motion.div>

        {/* Content Panel */}
        <AnimatePresence mode="wait">
          <motion.div
            key={activeCase.id}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -20 }}
            transition={{ duration: 0.3 }}
            className="max-w-5xl mx-auto"
          >
            <div className="grid lg:grid-cols-2 gap-8 items-center">
              {/* Info Panel */}
              <div className="bg-gray-50 dark:bg-gray-900 rounded-2xl p-8 border border-gray-200 dark:border-gray-800">
                <div className={`inline-flex items-center gap-2 px-4 py-2 rounded-full bg-gradient-to-r ${activeCase.gradient} text-white text-sm font-medium mb-6`}>
                  <activeCase.icon className="w-4 h-4" />
                  {isArabic ? activeCase.nameAr : activeCase.name}
                </div>

                <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                  {isArabic ? activeCase.taglineAr : activeCase.tagline}
                </h3>
                <p className="text-gray-600 dark:text-gray-400 mb-6">
                  {isArabic ? activeCase.descriptionAr : activeCase.description}
                </p>

                {/* Benefits List */}
                <ul className="space-y-3 mb-8">
                  {(isArabic ? activeCase.benefitsAr : activeCase.benefits).map((benefit, index) => (
                    <motion.li
                      key={benefit}
                      initial={{ opacity: 0, x: isArabic ? 20 : -20 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ delay: index * 0.1 }}
                      className="flex items-start gap-3"
                    >
                      <CheckCircle className="w-5 h-5 text-emerald-500 mt-0.5 flex-shrink-0" />
                      <span className="text-gray-700 dark:text-gray-300 text-sm">{benefit}</span>
                    </motion.li>
                  ))}
                </ul>

                <Link href={`/solutions/${activeCase.id}`}>
                  <Button className="bg-emerald-600 hover:bg-emerald-700 text-white group">
                    {t("learnMore")}
                    <ArrowRight className={`w-4 h-4 ${isArabic ? "mr-2 group-hover:-translate-x-1 rotate-180" : "ml-2 group-hover:translate-x-1"} transition-transform`} />
                  </Button>
                </Link>
              </div>

              {/* Interactive Dashboard Panel */}
              <div className="space-y-6">
                {/* Compliance Chart */}
                <div className="bg-gray-50 dark:bg-gray-900 rounded-xl p-6 border border-gray-200 dark:border-gray-800">
                  <div className="flex items-center gap-2 mb-4">
                    <BarChart3 className="w-5 h-5 text-emerald-500" />
                    <h4 className="font-semibold text-gray-900 dark:text-white">
                      {t("charts.complianceRate")}
                    </h4>
                  </div>
                  <div className="h-48">
                    <ResponsiveContainer width="100%" height="100%">
                      <BarChart
                        data={sectorControls[activeCase.id as keyof typeof sectorControls].map(ctrl => ({
                          name: isArabic ? ctrl.name.ar : ctrl.name.en,
                          coverage: ctrl.coverage,
                          fill: ctrl.status === "compliant" ? CHART_COLORS.compliant :
                                ctrl.status === "in_progress" ? CHART_COLORS.in_progress : CHART_COLORS.pending
                        }))}
                        layout="vertical"
                        margin={{ top: 0, right: 20, left: isArabic ? 20 : 100, bottom: 0 }}
                      >
                        <XAxis type="number" domain={[0, 100]} tick={{ fontSize: 11 }} />
                        <YAxis type="category" dataKey="name" tick={{ fontSize: 11 }} width={isArabic ? 100 : 120} />
                        <Tooltip
                          formatter={(value: number) => [`${value}%`, t("charts.coverage")]}
                          contentStyle={{ background: "#1f2937", border: "none", borderRadius: "8px" }}
                          labelStyle={{ color: "#fff" }}
                        />
                        <Bar dataKey="coverage" radius={[0, 4, 4, 0]}>
                          {sectorControls[activeCase.id as keyof typeof sectorControls].map((ctrl, index) => (
                            <Cell
                              key={`cell-${index}`}
                              fill={ctrl.status === "compliant" ? CHART_COLORS.compliant :
                                    ctrl.status === "in_progress" ? CHART_COLORS.in_progress : CHART_COLORS.pending}
                            />
                          ))}
                        </Bar>
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                  {/* Legend */}
                  <div className="flex justify-center gap-6 mt-4">
                    <div className="flex items-center gap-2">
                      <div className="w-3 h-3 rounded-full bg-emerald-500" />
                      <span className="text-xs text-gray-600 dark:text-gray-400">{t("legend.compliant")}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <div className="w-3 h-3 rounded-full bg-amber-500" />
                      <span className="text-xs text-gray-600 dark:text-gray-400">{t("legend.inProgress")}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <div className="w-3 h-3 rounded-full bg-red-500" />
                      <span className="text-xs text-gray-600 dark:text-gray-400">{t("legend.pending")}</span>
                    </div>
                  </div>
                </div>

                {/* Risk Radar Chart */}
                <div className="bg-gray-50 dark:bg-gray-900 rounded-xl p-6 border border-gray-200 dark:border-gray-800">
                  <div className="flex items-center gap-2 mb-4">
                    <AlertTriangle className="w-5 h-5 text-amber-500" />
                    <h4 className="font-semibold text-gray-900 dark:text-white">
                      {t("charts.riskAssessment")}
                    </h4>
                  </div>
                  <div className="h-56">
                    <ResponsiveContainer width="100%" height="100%">
                      <RadarChart
                        data={sectorRisks[activeCase.id as keyof typeof sectorRisks].map(risk => ({
                          subject: isArabic ? risk.name.ar : risk.name.en,
                          likelihood: risk.likelihood,
                          impact: risk.impact,
                          fullMark: 100
                        }))}
                      >
                        <PolarGrid stroke="#374151" />
                        <PolarAngleAxis dataKey="subject" tick={{ fontSize: 10, fill: "#9ca3af" }} />
                        <PolarRadiusAxis angle={30} domain={[0, 100]} tick={{ fontSize: 9 }} />
                        <Radar
                          name={t("legend.likelihood")}
                          dataKey="likelihood"
                          stroke="#3b82f6"
                          fill="#3b82f6"
                          fillOpacity={0.3}
                        />
                        <Radar
                          name={t("legend.impact")}
                          dataKey="impact"
                          stroke="#ef4444"
                          fill="#ef4444"
                          fillOpacity={0.3}
                        />
                        <Tooltip
                          contentStyle={{ background: "#1f2937", border: "none", borderRadius: "8px" }}
                          labelStyle={{ color: "#fff" }}
                        />
                      </RadarChart>
                    </ResponsiveContainer>
                  </div>
                  {/* Risk Legend */}
                  <div className="flex justify-center gap-6 mt-2">
                    <div className="flex items-center gap-2">
                      <div className="w-3 h-3 rounded-full bg-blue-500" />
                      <span className="text-xs text-gray-600 dark:text-gray-400">{t("legend.likelihood")}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <div className="w-3 h-3 rounded-full bg-red-500" />
                      <span className="text-xs text-gray-600 dark:text-gray-400">{t("legend.impact")}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Related Controls & Risks Section */}
            <div className="grid md:grid-cols-2 gap-6 mt-8">
              {/* Controls Panel */}
              <div className="bg-gray-50 dark:bg-gray-900 rounded-xl p-6 border border-gray-200 dark:border-gray-800">
                <div className="flex items-center gap-2 mb-4">
                  <Shield className="w-5 h-5 text-emerald-500" />
                  <h4 className="font-semibold text-gray-900 dark:text-white">
                    {t("panels.relatedControls")}
                  </h4>
                </div>
                <div className="space-y-3">
                  {sectorControls[activeCase.id as keyof typeof sectorControls].map((control, idx) => (
                    <motion.div
                      key={control.id}
                      initial={{ opacity: 0, x: isArabic ? 20 : -20 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ delay: idx * 0.1 }}
                      className="flex items-center justify-between p-3 bg-white dark:bg-gray-800 rounded-lg border border-gray-100 dark:border-gray-700 hover:border-emerald-300 dark:hover:border-emerald-600 transition-colors cursor-pointer group"
                    >
                      <div className="flex items-center gap-3">
                        <div className={`p-2 rounded-lg ${
                          control.status === "compliant" ? "bg-emerald-100 dark:bg-emerald-900/30" :
                          control.status === "in_progress" ? "bg-amber-100 dark:bg-amber-900/30" :
                          "bg-red-100 dark:bg-red-900/30"
                        }`}>
                          {control.status === "compliant" ? (
                            <CheckCircle className="w-4 h-4 text-emerald-600" />
                          ) : control.status === "in_progress" ? (
                            <Activity className="w-4 h-4 text-amber-600" />
                          ) : (
                            <Lock className="w-4 h-4 text-red-600" />
                          )}
                        </div>
                        <span className="text-sm text-gray-700 dark:text-gray-300">
                          {isArabic ? control.name.ar : control.name.en}
                        </span>
                      </div>
                      <div className="flex items-center gap-2">
                        <div className="w-16 h-2 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
                          <motion.div
                            initial={{ width: 0 }}
                            animate={{ width: `${control.coverage}%` }}
                            transition={{ duration: 1, delay: idx * 0.1 }}
                            className={`h-full rounded-full ${
                              control.status === "compliant" ? "bg-emerald-500" :
                              control.status === "in_progress" ? "bg-amber-500" : "bg-red-500"
                            }`}
                          />
                        </div>
                        <span className="text-xs font-medium text-gray-500 dark:text-gray-400 w-8">
                          {control.coverage}%
                        </span>
                      </div>
                    </motion.div>
                  ))}
                </div>
              </div>

              {/* Risks Panel */}
              <div className="bg-gray-50 dark:bg-gray-900 rounded-xl p-6 border border-gray-200 dark:border-gray-800">
                <div className="flex items-center gap-2 mb-4">
                  <AlertTriangle className="w-5 h-5 text-amber-500" />
                  <h4 className="font-semibold text-gray-900 dark:text-white">
                    {t("panels.relatedRisks")}
                  </h4>
                </div>
                <div className="space-y-3">
                  {sectorRisks[activeCase.id as keyof typeof sectorRisks].map((risk, idx) => (
                    <motion.div
                      key={risk.id}
                      initial={{ opacity: 0, x: isArabic ? -20 : 20 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ delay: idx * 0.1 }}
                      className="flex items-center justify-between p-3 bg-white dark:bg-gray-800 rounded-lg border border-gray-100 dark:border-gray-700 hover:border-amber-300 dark:hover:border-amber-600 transition-colors cursor-pointer group"
                    >
                      <div className="flex items-center gap-3">
                        <div className={`px-2 py-1 rounded text-xs font-medium ${
                          risk.severity === "critical" ? "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400" :
                          risk.severity === "high" ? "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400" :
                          "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400"
                        }`}>
                          {risk.severity === "critical" ? t("severity.critical") :
                           risk.severity === "high" ? t("severity.high") :
                           t("severity.medium")}
                        </div>
                        <span className="text-sm text-gray-700 dark:text-gray-300">
                          {isArabic ? risk.name.ar : risk.name.en}
                        </span>
                      </div>
                      <div className="flex items-center gap-4 text-xs text-gray-500 dark:text-gray-400">
                        <div className="flex items-center gap-1">
                          <TrendingUp className="w-3 h-3" />
                          <span>{risk.likelihood}%</span>
                        </div>
                        <div className="flex items-center gap-1">
                          <Activity className="w-3 h-3" />
                          <span>{risk.impact}%</span>
                        </div>
                      </div>
                    </motion.div>
                  ))}
                </div>
                {/* Risk Summary */}
                <div className="mt-4 p-3 bg-gradient-to-r from-amber-50 to-orange-50 dark:from-amber-900/20 dark:to-orange-900/20 rounded-lg">
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-amber-800 dark:text-amber-300 font-medium">
                      {t("panels.totalRisks")}
                    </span>
                    <span className="text-amber-900 dark:text-amber-200 font-bold">
                      {sectorRisks[activeCase.id as keyof typeof sectorRisks].length}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            {/* Key Metrics Row */}
            <div className={`grid grid-cols-2 md:grid-cols-4 gap-4 mt-8`}>
              <div className={`bg-gradient-to-br ${activeCase.gradient} rounded-xl p-4 text-white text-center`}>
                <div className="text-3xl font-bold">{activeCase.stat}</div>
                <div className="text-sm text-white/80">{isArabic ? activeCase.statLabelAr : activeCase.statLabel}</div>
              </div>
              <div className="bg-gray-50 dark:bg-gray-900 rounded-xl p-4 text-center border border-gray-200 dark:border-gray-800">
                <div className="text-3xl font-bold text-emerald-600">24/7</div>
                <div className="text-sm text-gray-600 dark:text-gray-400">{t("metrics.continuousMonitoring")}</div>
              </div>
              <div className="bg-gray-50 dark:bg-gray-900 rounded-xl p-4 text-center border border-gray-200 dark:border-gray-800">
                <div className="text-3xl font-bold text-blue-600">
                  {sectorControls[activeCase.id as keyof typeof sectorControls].filter(c => c.status === "compliant").length}/{sectorControls[activeCase.id as keyof typeof sectorControls].length}
                </div>
                <div className="text-sm text-gray-600 dark:text-gray-400">{t("metrics.controlsCompliant")}</div>
              </div>
              <div className="bg-gray-50 dark:bg-gray-900 rounded-xl p-4 text-center border border-gray-200 dark:border-gray-800">
                <div className="text-3xl font-bold text-amber-600">
                  {sectorRisks[activeCase.id as keyof typeof sectorRisks].filter(r => r.severity === "critical").length}
                </div>
                <div className="text-sm text-gray-600 dark:text-gray-400">{t("metrics.criticalRisks")}</div>
              </div>
            </div>
          </motion.div>
        </AnimatePresence>
      </div>
    </section>
  )
}
