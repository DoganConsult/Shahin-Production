"use client"

import { useState } from "react"
import { motion, AnimatePresence } from "framer-motion"
import {
  Plug,
  Check,
  ArrowRight,
  X,
  Zap,
  RefreshCw,
  Database,
  Users,
  Calendar,
  Mail,
  Clock
} from "lucide-react"
import { Button } from "@/components/ui/button"
import Link from "next/link"
import { useLocale } from "@/components/providers/locale-provider"

interface Integration {
  id: string
  name: string
  logo: string
  description: string
  descriptionAr: string
  category: "erp" | "hr" | "communication" | "productivity" | "coming"
  status: "available" | "coming"
  automations: string[]
  automationsAr: string[]
  popular?: boolean
}

const integrations: Integration[] = [
  {
    id: "erpnext",
    name: "ERPNext",
    logo: "/integrations/erpnext.svg",
    description: "Open-source ERP for manufacturing, distribution, retail, and services",
    descriptionAr: "نظام ERP مفتوح المصدر للتصنيع والتوزيع والتجزئة والخدمات",
    category: "erp",
    status: "available",
    popular: true,
    automations: [
      "Invoice 3-way matching & approval",
      "Purchase order workflow",
      "Vendor onboarding & compliance",
      "Month-end reconciliation",
      "Expense report processing",
      "Budget variance alerts"
    ],
    automationsAr: [
      "المطابقة الثلاثية للفواتير والموافقة",
      "سير عمل أوامر الشراء",
      "تهيئة الموردين والامتثال",
      "مطابقة نهاية الشهر",
      "معالجة تقارير المصروفات",
      "تنبيهات انحراف الميزانية"
    ]
  },
  {
    id: "odoo",
    name: "Odoo",
    logo: "/integrations/odoo.svg",
    description: "All-in-one business software with modular applications",
    descriptionAr: "برنامج أعمال متكامل مع تطبيقات نمطية",
    category: "erp",
    status: "available",
    popular: true,
    automations: [
      "Sales order approval workflow",
      "Inventory reorder automation",
      "Customer credit limit monitoring",
      "Manufacturing work order routing",
      "Quality control checkpoints",
      "Supplier performance tracking"
    ],
    automationsAr: [
      "سير عمل موافقة أوامر المبيعات",
      "أتمتة إعادة طلب المخزون",
      "مراقبة حدود ائتمان العملاء",
      "توجيه أوامر التصنيع",
      "نقاط مراقبة الجودة",
      "تتبع أداء الموردين"
    ]
  },
  {
    id: "sap",
    name: "SAP Business One",
    logo: "/integrations/sap.svg",
    description: "Enterprise resource planning for small and midsize businesses",
    descriptionAr: "تخطيط موارد المؤسسات للشركات الصغيرة والمتوسطة",
    category: "erp",
    status: "coming",
    automations: [
      "Journal entry approval",
      "Asset management workflows",
      "Intercompany transactions",
      "Tax compliance automation",
      "Financial close orchestration"
    ],
    automationsAr: [
      "موافقة القيود المحاسبية",
      "سير عمل إدارة الأصول",
      "المعاملات بين الشركات",
      "أتمتة الامتثال الضريبي",
      "تنسيق الإغلاق المالي"
    ]
  },
  {
    id: "microsoft365",
    name: "Microsoft 365",
    logo: "/integrations/microsoft365.svg",
    description: "Productivity suite including Teams, Outlook, and SharePoint",
    descriptionAr: "مجموعة إنتاجية تشمل Teams وOutlook وSharePoint",
    category: "productivity",
    status: "available",
    automations: [
      "Teams approval notifications",
      "SharePoint document workflows",
      "Outlook calendar integration",
      "Excel report generation",
      "Power Automate triggers"
    ],
    automationsAr: [
      "إشعارات الموافقة في Teams",
      "سير عمل مستندات SharePoint",
      "تكامل تقويم Outlook",
      "إنشاء تقارير Excel",
      "مشغلات Power Automate"
    ]
  },
  {
    id: "slack",
    name: "Slack",
    logo: "/integrations/slack.svg",
    description: "Business communication platform",
    descriptionAr: "منصة اتصالات الأعمال",
    category: "communication",
    status: "available",
    automations: [
      "Approval notifications in channels",
      "Workflow status updates",
      "SLA breach alerts",
      "Daily task digest",
      "Interactive approval buttons"
    ],
    automationsAr: [
      "إشعارات الموافقة في القنوات",
      "تحديثات حالة سير العمل",
      "تنبيهات تجاوز SLA",
      "ملخص المهام اليومية",
      "أزرار الموافقة التفاعلية"
    ]
  },
  {
    id: "bamboohr",
    name: "BambooHR",
    logo: "/integrations/bamboohr.svg",
    description: "HR software for small and medium businesses",
    descriptionAr: "برنامج موارد بشرية للشركات الصغيرة والمتوسطة",
    category: "hr",
    status: "coming",
    automations: [
      "Employee onboarding orchestration",
      "Time-off approval workflows",
      "Performance review automation",
      "Access provisioning triggers",
      "Offboarding checklist automation"
    ],
    automationsAr: [
      "تنسيق تهيئة الموظفين",
      "سير عمل موافقة الإجازات",
      "أتمتة تقييم الأداء",
      "مشغلات منح الصلاحيات",
      "أتمتة قائمة إنهاء الخدمة"
    ]
  }
]

const content = {
  sectionLabel: { en: "Integrations", ar: "التكاملات" },
  title: { en: "Connect Your Existing Tools", ar: "اربط أدواتك الحالية" },
  subtitle: {
    en: "Shahin integrates with your ERP, HR systems, and productivity tools to automate workflows across your entire tech stack.",
    ar: "شاهين يتكامل مع نظام ERP وأنظمة الموارد البشرية وأدوات الإنتاجية لأتمتة سير العمل عبر كامل بنيتك التقنية."
  },
  categories: {
    all: { en: "All Integrations", ar: "جميع التكاملات" },
    erp: { en: "ERP Systems", ar: "أنظمة ERP" },
    hr: { en: "HR & People", ar: "الموارد البشرية" },
    communication: { en: "Communication", ar: "الاتصالات" },
    productivity: { en: "Productivity", ar: "الإنتاجية" }
  },
  popular: { en: "POPULAR", ar: "شائع" },
  comingSoon: { en: "Coming Soon", ar: "قريباً" },
  automations: { en: "automations", ar: "أتمتة" },
  availableNow: { en: "Available Now", ar: "متاح الآن" },
  whatWeAutomate: { en: "What We Automate", ar: "ما نقوم بأتمتته" },
  connect: { en: "Connect", ar: "ربط" },
  notifyMe: { en: "Notify Me", ar: "أعلمني" },
  comingSoonMessage: {
    en: "This integration is coming soon. Sign up to be notified when it's available.",
    ar: "هذا التكامل قادم قريباً. سجّل ليتم إعلامك عند توفره."
  },
  dontSeeYourTool: { en: "Don't see your tool?", ar: "لا ترى أداتك؟" },
  requestIntegration: { en: "Request Integration", ar: "اطلب تكامل" }
}

export function IntegrationGallery() {
  const [activeCategory, setActiveCategory] = useState("all")
  const [selectedIntegration, setSelectedIntegration] = useState<Integration | null>(null)
  const { locale } = useLocale()
  const isArabic = locale === "ar"

  const categories = [
    { id: "all", label: isArabic ? content.categories.all.ar : content.categories.all.en, icon: Plug },
    { id: "erp", label: isArabic ? content.categories.erp.ar : content.categories.erp.en, icon: Database },
    { id: "hr", label: isArabic ? content.categories.hr.ar : content.categories.hr.en, icon: Users },
    { id: "communication", label: isArabic ? content.categories.communication.ar : content.categories.communication.en, icon: Mail },
    { id: "productivity", label: isArabic ? content.categories.productivity.ar : content.categories.productivity.en, icon: Calendar }
  ]

  const filteredIntegrations = integrations.filter(
    int => activeCategory === "all" || int.category === activeCategory
  )

  return (
    <section className="py-24 bg-gray-50 dark:bg-gray-900">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-12"
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

        {/* Category Filters */}
        <motion.div
          className="flex flex-wrap justify-center gap-3 mb-10"
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
        >
          {categories.map((cat) => (
            <button
              key={cat.id}
              onClick={() => setActiveCategory(cat.id)}
              className={`flex items-center gap-2 px-5 py-3 rounded-xl font-medium transition-all ${
                activeCategory === cat.id
                  ? "bg-emerald-600 text-white shadow-lg shadow-emerald-500/25"
                  : "bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 hover:border-emerald-300 dark:hover:border-emerald-700"
              }`}
            >
              <cat.icon className="w-4 h-4" />
              {cat.label}
            </button>
          ))}
        </motion.div>

        {/* Integration Cards */}
        <motion.div
          className="grid md:grid-cols-2 lg:grid-cols-3 gap-6 max-w-6xl mx-auto"
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
        >
          <AnimatePresence>
            {filteredIntegrations.map((integration, index) => (
              <motion.div
                key={integration.id}
                initial={{ opacity: 0, scale: 0.95 }}
                animate={{ opacity: 1, scale: 1 }}
                exit={{ opacity: 0, scale: 0.95 }}
                transition={{ delay: index * 0.05 }}
                onClick={() => setSelectedIntegration(integration)}
                className={`relative bg-white dark:bg-gray-800 rounded-2xl border-2 p-6 cursor-pointer transition-all hover:shadow-xl ${
                  integration.status === "coming"
                    ? "border-dashed border-gray-300 dark:border-gray-600"
                    : "border-gray-200 dark:border-gray-700 hover:border-emerald-500"
                }`}
              >
                {/* Popular Badge */}
                {integration.popular && (
                  <div className={`absolute -top-3 ${isArabic ? "-left-3" : "-right-3"}`}>
                    <div className="bg-emerald-500 text-white text-xs font-bold px-3 py-1 rounded-full shadow-lg">
                      {isArabic ? content.popular.ar : content.popular.en}
                    </div>
                  </div>
                )}

                {/* Status Badge */}
                {integration.status === "coming" && (
                  <div className={`absolute top-4 ${isArabic ? "left-4" : "right-4"}`}>
                    <span className="text-xs px-2 py-1 rounded-full bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-400">
                      {isArabic ? content.comingSoon.ar : content.comingSoon.en}
                    </span>
                  </div>
                )}

                {/* Logo Placeholder */}
                <div className={`w-16 h-16 rounded-xl mb-4 flex items-center justify-center text-2xl font-bold ${
                  integration.status === "coming"
                    ? "bg-gray-100 dark:bg-gray-700 text-gray-400"
                    : "bg-gradient-to-br from-emerald-100 to-teal-100 dark:from-emerald-900/30 dark:to-teal-900/30 text-emerald-600 dark:text-emerald-400"
                }`}>
                  {integration.name.charAt(0)}
                </div>

                <h3 className={`text-xl font-bold mb-2 ${
                  integration.status === "coming"
                    ? "text-gray-400 dark:text-gray-500"
                    : "text-gray-900 dark:text-white"
                }`}>
                  {integration.name}
                </h3>

                <p className="text-sm text-gray-600 dark:text-gray-400 mb-4 line-clamp-2">
                  {isArabic ? integration.descriptionAr : integration.description}
                </p>

                {/* Automation Count */}
                <div className="flex items-center gap-2 text-sm">
                  <Zap className={`w-4 h-4 ${
                    integration.status === "coming"
                      ? "text-gray-400"
                      : "text-emerald-500"
                  }`} />
                  <span className={
                    integration.status === "coming"
                      ? "text-gray-400"
                      : "text-gray-600 dark:text-gray-400"
                  }>
                    {integration.automations.length} {isArabic ? content.automations.ar : content.automations.en}
                  </span>
                </div>

                {/* Hover Arrow */}
                <div className={`absolute bottom-4 ${isArabic ? "left-4" : "right-4"} opacity-0 group-hover:opacity-100 transition-opacity`}>
                  <ArrowRight className={`w-5 h-5 text-emerald-500 ${isArabic ? "rotate-180" : ""}`} />
                </div>
              </motion.div>
            ))}
          </AnimatePresence>
        </motion.div>

        {/* Request Integration CTA */}
        <motion.div
          className="text-center mt-12"
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
        >
          <div className="inline-flex flex-col sm:flex-row items-center gap-4 px-6 py-4 bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700">
            <span className="text-gray-700 dark:text-gray-300">
              {isArabic ? content.dontSeeYourTool.ar : content.dontSeeYourTool.en}
            </span>
            <Link href="/contact">
              <Button variant="outline" className="gap-2">
                <Plug className="w-4 h-4" />
                {isArabic ? content.requestIntegration.ar : content.requestIntegration.en}
              </Button>
            </Link>
          </div>
        </motion.div>
      </div>

      {/* Integration Detail Modal */}
      <AnimatePresence>
        {selectedIntegration && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center z-50 p-4"
            onClick={() => setSelectedIntegration(null)}
          >
            <motion.div
              initial={{ opacity: 0, scale: 0.95, y: 20 }}
              animate={{ opacity: 1, scale: 1, y: 0 }}
              exit={{ opacity: 0, scale: 0.95, y: 20 }}
              className="bg-white dark:bg-gray-800 rounded-2xl max-w-lg w-full max-h-[90vh] overflow-y-auto shadow-2xl"
              onClick={(e) => e.stopPropagation()}
            >
              {/* Header */}
              <div className="p-6 border-b border-gray-200 dark:border-gray-700">
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-4">
                    <div className="w-14 h-14 rounded-xl bg-gradient-to-br from-emerald-100 to-teal-100 dark:from-emerald-900/30 dark:to-teal-900/30 flex items-center justify-center text-2xl font-bold text-emerald-600 dark:text-emerald-400">
                      {selectedIntegration.name.charAt(0)}
                    </div>
                    <div>
                      <h3 className="text-xl font-bold text-gray-900 dark:text-white">
                        {selectedIntegration.name}
                      </h3>
                      <span className={`text-xs px-2 py-1 rounded-full ${
                        selectedIntegration.status === "available"
                          ? "bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400"
                          : "bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-400"
                      }`}>
                        {selectedIntegration.status === "available"
                          ? (isArabic ? content.availableNow.ar : content.availableNow.en)
                          : (isArabic ? content.comingSoon.ar : content.comingSoon.en)}
                      </span>
                    </div>
                  </div>
                  <button
                    onClick={() => setSelectedIntegration(null)}
                    className="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
                  >
                    <X className="w-5 h-5 text-gray-500" />
                  </button>
                </div>
              </div>

              {/* Content */}
              <div className="p-6">
                <p className="text-gray-600 dark:text-gray-400 mb-6">
                  {isArabic ? selectedIntegration.descriptionAr : selectedIntegration.description}
                </p>

                <h4 className="font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                  <Zap className="w-5 h-5 text-emerald-500" />
                  {isArabic ? content.whatWeAutomate.ar : content.whatWeAutomate.en}
                </h4>

                <ul className="space-y-3 mb-6">
                  {(isArabic ? selectedIntegration.automationsAr : selectedIntegration.automations).map((automation, index) => (
                    <li key={index} className="flex items-start gap-3">
                      <Check className="w-5 h-5 text-emerald-500 flex-shrink-0 mt-0.5" />
                      <span className="text-gray-700 dark:text-gray-300 text-sm">{automation}</span>
                    </li>
                  ))}
                </ul>

                {selectedIntegration.status === "available" ? (
                  <Link href="/trial">
                    <Button className="w-full bg-emerald-600 hover:bg-emerald-700 text-white py-5">
                      <RefreshCw className={`w-5 h-5 ${isArabic ? "ml-2" : "mr-2"}`} />
                      {isArabic ? content.connect.ar : content.connect.en} {selectedIntegration.name}
                    </Button>
                  </Link>
                ) : (
                  <div className="bg-gray-50 dark:bg-gray-900 rounded-xl p-4 text-center">
                    <Clock className="w-8 h-8 text-gray-400 mx-auto mb-2" />
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                      {isArabic ? content.comingSoonMessage.ar : content.comingSoonMessage.en}
                    </p>
                    <Button variant="outline" className="mt-3">
                      {isArabic ? content.notifyMe.ar : content.notifyMe.en}
                    </Button>
                  </div>
                )}
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </section>
  )
}
