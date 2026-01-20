"use client"

import { useState, useEffect } from "react"
import { motion, AnimatePresence } from "framer-motion"
import {
  Bot,
  Eye,
  CheckCircle,
  Play,
  Shield,
  ArrowRight,
  FileText,
  Receipt,
  Users,
  AlertTriangle,
  Clock,
  Sparkles
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { useLocale } from "@/components/providers/locale-provider"

type AgentStep = "propose" | "review" | "approve" | "execute"

interface AgentStepData {
  title: string
  titleAr: string
  description: string
  descriptionAr: string
  details: string[]
  detailsAr: string[]
  badge: string
  badgeAr: string
}

interface Agent {
  id: string
  name: string
  nameAr: string
  description: string
  descriptionAr: string
  icon: typeof Receipt
  steps: Record<AgentStep, AgentStepData>
}

const agents: Agent[] = [
  {
    id: "invoice",
    name: "Invoice Processing Agent",
    nameAr: "وكيل معالجة الفواتير",
    description: "Automatically reviews, matches, and processes vendor invoices",
    descriptionAr: "يراجع ويطابق ويعالج فواتير الموردين تلقائياً",
    icon: Receipt,
    steps: {
      propose: {
        title: "AI Proposes Action",
        titleAr: "الذكاء الاصطناعي يقترح إجراء",
        description: "Agent analyzes invoice from ABC Corp for SAR 45,000",
        descriptionAr: "الوكيل يحلل فاتورة من شركة ABC بقيمة 45,000 ريال",
        details: [
          "Extracted: Vendor ABC Corp, Amount SAR 45,000",
          "Matched to PO #12345 (3-way match verified)",
          "GL coding suggested: 6200 - Office Supplies",
          "Confidence: 98%"
        ],
        detailsAr: [
          "تم الاستخراج: المورد ABC Corp، المبلغ 45,000 ريال",
          "تمت المطابقة مع أمر الشراء #12345 (تم التحقق من المطابقة الثلاثية)",
          "ترميز GL المقترح: 6200 - مستلزمات مكتبية",
          "نسبة الثقة: 98%"
        ],
        badge: "Pending Review",
        badgeAr: "في انتظار المراجعة"
      },
      review: {
        title: "Human Reviews",
        titleAr: "المراجعة البشرية",
        description: "Finance team reviews AI recommendations",
        descriptionAr: "فريق المالية يراجع توصيات الذكاء الاصطناعي",
        details: [
          "All extracted fields highlighted for verification",
          "Historical vendor data shown for context",
          "Policy compliance checked automatically",
          "One-click to approve or modify"
        ],
        detailsAr: [
          "جميع الحقول المستخرجة مميزة للتحقق",
          "بيانات المورد التاريخية معروضة للسياق",
          "التحقق من الامتثال للسياسة تلقائياً",
          "نقرة واحدة للموافقة أو التعديل"
        ],
        badge: "Under Review",
        badgeAr: "قيد المراجعة"
      },
      approve: {
        title: "Approval Required",
        titleAr: "الموافقة مطلوبة",
        description: "Authorized approver signs off",
        descriptionAr: "المعتمد المفوض يوقع",
        details: [
          "Multi-level approval workflow triggered",
          "Digital signature captured",
          "Full audit trail recorded",
          "Compliance rules enforced"
        ],
        detailsAr: [
          "تم تفعيل سير عمل الموافقة متعدد المستويات",
          "تم التقاط التوقيع الرقمي",
          "تم تسجيل مسار التدقيق الكامل",
          "تم تطبيق قواعد الامتثال"
        ],
        badge: "Awaiting Approval",
        badgeAr: "في انتظار الموافقة"
      },
      execute: {
        title: "Safe Execution",
        titleAr: "التنفيذ الآمن",
        description: "Agent executes approved action",
        descriptionAr: "الوكيل ينفذ الإجراء المعتمد",
        details: [
          "Payment scheduled for May 15",
          "ERP journal entry created",
          "Vendor notified via email",
          "Audit log updated"
        ],
        detailsAr: [
          "تمت جدولة الدفع ليوم 15 مايو",
          "تم إنشاء قيد ERP",
          "تم إخطار المورد عبر البريد الإلكتروني",
          "تم تحديث سجل التدقيق"
        ],
        badge: "Completed",
        badgeAr: "مكتمل"
      }
    }
  },
  {
    id: "onboarding",
    name: "Employee Onboarding Agent",
    nameAr: "وكيل تهيئة الموظفين",
    description: "Orchestrates new hire setup across HR, IT, and Finance",
    descriptionAr: "ينسق إعداد الموظفين الجدد عبر الموارد البشرية وتقنية المعلومات والمالية",
    icon: Users,
    steps: {
      propose: {
        title: "AI Proposes Action",
        titleAr: "الذكاء الاصطناعي يقترح إجراء",
        description: "Agent prepares onboarding for Sarah Ahmed - Software Engineer",
        descriptionAr: "الوكيل يجهز التهيئة لسارة أحمد - مهندسة برمجيات",
        details: [
          "Role: Software Engineer, Department: Engineering",
          "Equipment needed: MacBook Pro, monitors",
          "Access groups: Engineering, GitHub, Slack",
          "Training plan: 5-day orientation"
        ],
        detailsAr: [
          "الدور: مهندس برمجيات، القسم: الهندسة",
          "المعدات المطلوبة: MacBook Pro، شاشات",
          "مجموعات الوصول: الهندسة، GitHub، Slack",
          "خطة التدريب: توجيه 5 أيام"
        ],
        badge: "Pending Review",
        badgeAr: "في انتظار المراجعة"
      },
      review: {
        title: "Human Reviews",
        titleAr: "المراجعة البشرية",
        description: "HR manager reviews onboarding package",
        descriptionAr: "مدير الموارد البشرية يراجع حزمة التهيئة",
        details: [
          "Verify job details and start date",
          "Review access permissions",
          "Confirm equipment allocation",
          "Approve training schedule"
        ],
        detailsAr: [
          "التحقق من تفاصيل الوظيفة وتاريخ البدء",
          "مراجعة صلاحيات الوصول",
          "تأكيد تخصيص المعدات",
          "الموافقة على جدول التدريب"
        ],
        badge: "Under Review",
        badgeAr: "قيد المراجعة"
      },
      approve: {
        title: "Approval Required",
        titleAr: "الموافقة مطلوبة",
        description: "Department head and IT approve",
        descriptionAr: "رئيس القسم وتقنية المعلومات يوافقون",
        details: [
          "Department head approves role access",
          "IT Security approves system access",
          "Finance approves equipment budget",
          "Digital signatures collected"
        ],
        detailsAr: [
          "رئيس القسم يوافق على وصول الدور",
          "أمن تقنية المعلومات يوافق على وصول النظام",
          "المالية توافق على ميزانية المعدات",
          "تم جمع التوقيعات الرقمية"
        ],
        badge: "Awaiting Approval",
        badgeAr: "في انتظار الموافقة"
      },
      execute: {
        title: "Safe Execution",
        titleAr: "التنفيذ الآمن",
        description: "Agent executes onboarding tasks",
        descriptionAr: "الوكيل ينفذ مهام التهيئة",
        details: [
          "Email account created",
          "Slack and GitHub invites sent",
          "IT ticket for equipment opened",
          "Welcome email scheduled"
        ],
        detailsAr: [
          "تم إنشاء حساب البريد الإلكتروني",
          "تم إرسال دعوات Slack وGitHub",
          "تم فتح تذكرة IT للمعدات",
          "تمت جدولة بريد الترحيب"
        ],
        badge: "Completed",
        badgeAr: "مكتمل"
      }
    }
  },
  {
    id: "compliance",
    name: "Compliance Audit Agent",
    nameAr: "وكيل تدقيق الامتثال",
    description: "Continuously monitors controls and flags exceptions",
    descriptionAr: "يراقب الضوابط باستمرار ويحدد الاستثناءات",
    icon: Shield,
    steps: {
      propose: {
        title: "AI Detects Issue",
        titleAr: "الذكاء الاصطناعي يكتشف مشكلة",
        description: "Agent identified potential segregation of duties violation",
        descriptionAr: "الوكيل حدد انتهاكاً محتملاً لفصل المهام",
        details: [
          "User: Ahmed K. has both create and approve access",
          "Risk Level: High",
          "Policy Reference: SOD-001",
          "Recommendation: Remove approve access"
        ],
        detailsAr: [
          "المستخدم: أحمد ك. لديه صلاحية الإنشاء والموافقة",
          "مستوى المخاطر: عالي",
          "مرجع السياسة: SOD-001",
          "التوصية: إزالة صلاحية الموافقة"
        ],
        badge: "Risk Detected",
        badgeAr: "تم اكتشاف خطر"
      },
      review: {
        title: "Human Reviews",
        titleAr: "المراجعة البشرية",
        description: "Compliance officer investigates finding",
        descriptionAr: "مسؤول الامتثال يحقق في النتيجة",
        details: [
          "View user's full access matrix",
          "Check if temporary exception exists",
          "Review business justification",
          "Document investigation notes"
        ],
        detailsAr: [
          "عرض مصفوفة وصول المستخدم الكاملة",
          "التحقق من وجود استثناء مؤقت",
          "مراجعة المبرر التجاري",
          "توثيق ملاحظات التحقيق"
        ],
        badge: "Under Review",
        badgeAr: "قيد المراجعة"
      },
      approve: {
        title: "Approval Required",
        titleAr: "الموافقة مطلوبة",
        description: "Remediation plan requires sign-off",
        descriptionAr: "خطة المعالجة تتطلب التوقيع",
        details: [
          "CISO approval for access change",
          "Manager notification",
          "Exception documented if needed",
          "Audit committee notified"
        ],
        detailsAr: [
          "موافقة CISO على تغيير الوصول",
          "إخطار المدير",
          "توثيق الاستثناء إذا لزم الأمر",
          "إخطار لجنة التدقيق"
        ],
        badge: "Awaiting Approval",
        badgeAr: "في انتظار الموافقة"
      },
      execute: {
        title: "Safe Execution",
        titleAr: "التنفيذ الآمن",
        description: "Agent executes remediation",
        descriptionAr: "الوكيل ينفذ المعالجة",
        details: [
          "Approve access revoked from user",
          "Notification sent to user & manager",
          "Audit finding marked as resolved",
          "Compliance report updated"
        ],
        detailsAr: [
          "تم إلغاء صلاحية الموافقة من المستخدم",
          "تم إرسال إشعار للمستخدم والمدير",
          "تم وضع علامة حل على نتيجة التدقيق",
          "تم تحديث تقرير الامتثال"
        ],
        badge: "Resolved",
        badgeAr: "تم الحل"
      }
    }
  }
]

export function AgentPanel() {
  const [selectedAgent, setSelectedAgent] = useState(agents[0])
  const [currentStep, setCurrentStep] = useState<AgentStep>("propose")
  const [isAnimating, setIsAnimating] = useState(false)
  const [currentTime, setCurrentTime] = useState<string>("")
  const { locale } = useLocale()
  const isArabic = locale === "ar"

  // Set time only on client to avoid hydration mismatch
  useEffect(() => {
    setCurrentTime(new Date().toLocaleTimeString(isArabic ? "ar-SA" : "en-US"))
  }, [isArabic, currentStep])

  const steps: AgentStep[] = ["propose", "review", "approve", "execute"]
  const stepIndex = steps.indexOf(currentStep)

  const advanceStep = () => {
    if (stepIndex < steps.length - 1) {
      setIsAnimating(true)
      setTimeout(() => {
        setCurrentStep(steps[stepIndex + 1])
        setIsAnimating(false)
      }, 300)
    } else {
      setCurrentStep("propose")
    }
  }

  const stepData = selectedAgent.steps[currentStep]

  // Localized step labels
  const stepLabels = {
    propose: { en: "AI Proposes", ar: "الذكاء الاصطناعي يقترح" },
    review: { en: "Human Reviews", ar: "المراجعة البشرية" },
    approve: { en: "Approval", ar: "الموافقة" },
    execute: { en: "Execute", ar: "التنفيذ" }
  }

  // Localized button labels
  const buttonLabels = {
    propose: { en: "Review", ar: "مراجعة" },
    review: { en: "Approve", ar: "موافقة" },
    approve: { en: "Execute", ar: "تنفيذ" },
    execute: { en: "Restart Demo", ar: "إعادة العرض" }
  }

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
          <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-violet-100 dark:bg-violet-900/30 text-violet-700 dark:text-violet-300 text-sm font-medium mb-6">
            <Shield className="w-4 h-4" />
            {isArabic ? "الذكاء الاصطناعي الآمن بالتصميم" : "AI Safety by Design"}
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
            {isArabic ? "الذكاء الاصطناعي يقترح. البشر يوافقون." : "AI That Proposes. Humans That Approve."}
          </h2>
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-3xl mx-auto">
            {isArabic
              ? "وكلاؤنا لا يتخذون أي إجراء بدون موافقة بشرية صريحة. كل توصية من الذكاء الاصطناعي تمر عبر سير عمل المراجعة والموافقة قبل التنفيذ."
              : "Our agents never take action without explicit human approval. Every AI recommendation goes through your review and approval workflow before execution."}
          </p>
        </motion.div>

        {/* Agent Selector */}
        <motion.div
          className="flex flex-wrap justify-center gap-3 mb-10"
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
        >
          {agents.map((agent) => (
            <button
              key={agent.id}
              onClick={() => {
                setSelectedAgent(agent)
                setCurrentStep("propose")
              }}
              className={`flex items-center gap-2 px-5 py-3 rounded-xl font-medium transition-all ${
                selectedAgent.id === agent.id
                  ? "bg-violet-600 text-white shadow-lg shadow-violet-500/25"
                  : "bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-700 hover:border-violet-300 dark:hover:border-violet-700"
              }`}
            >
              <agent.icon className="w-5 h-5" />
              <span className="hidden sm:inline">{isArabic ? agent.nameAr : agent.name}</span>
            </button>
          ))}
        </motion.div>

        {/* Main Panel */}
        <motion.div
          className="max-w-5xl mx-auto"
          initial={{ opacity: 0, y: 30 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          {/* Progress Steps */}
          <div className="bg-white dark:bg-gray-800 rounded-t-2xl border border-gray-200 dark:border-gray-700 border-b-0 p-6">
            <div className="flex items-center justify-between">
              {steps.map((step, index) => (
                <div key={step} className="flex items-center">
                  <div className="flex flex-col items-center">
                    <div className={`w-10 h-10 rounded-full flex items-center justify-center transition-all ${
                      index <= stepIndex
                        ? "bg-violet-600 text-white"
                        : "bg-gray-100 dark:bg-gray-700 text-gray-400 dark:text-gray-500"
                    }`}>
                      {index < stepIndex ? (
                        <CheckCircle className="w-5 h-5" />
                      ) : index === stepIndex ? (
                        <div className="w-3 h-3 bg-white rounded-full" />
                      ) : (
                        <span className="text-sm font-bold">{index + 1}</span>
                      )}
                    </div>
                    <span className={`text-xs mt-2 font-medium hidden sm:block ${
                      index <= stepIndex
                        ? "text-violet-600 dark:text-violet-400"
                        : "text-gray-400 dark:text-gray-500"
                    }`}>
                      {isArabic ? stepLabels[step].ar : stepLabels[step].en}
                    </span>
                  </div>
                  {index < steps.length - 1 && (
                    <div className={`w-16 sm:w-24 h-1 mx-2 rounded-full transition-all ${
                      index < stepIndex
                        ? "bg-violet-600"
                        : "bg-gray-200 dark:bg-gray-700"
                    }`} />
                  )}
                </div>
              ))}
            </div>
          </div>

          {/* Step Content */}
          <div className="bg-gray-900 rounded-b-2xl border border-gray-700 border-t-0 overflow-hidden">
            {/* Terminal Header */}
            <div className="flex items-center gap-2 px-4 py-3 border-b border-gray-700">
              <div className="flex items-center gap-1.5">
                <div className="w-3 h-3 rounded-full bg-red-500" />
                <div className="w-3 h-3 rounded-full bg-yellow-500" />
                <div className="w-3 h-3 rounded-full bg-green-500" />
              </div>
              <div className="flex items-center gap-2 ml-4">
                <Bot className="w-4 h-4 text-violet-400" />
                <span className="text-sm text-gray-400">{isArabic ? selectedAgent.nameAr : selectedAgent.name}</span>
              </div>
              <div className="ml-auto">
                <span className={`text-xs px-2 py-1 rounded-full ${
                  currentStep === "propose" ? "bg-blue-500/20 text-blue-400" :
                  currentStep === "review" ? "bg-yellow-500/20 text-yellow-400" :
                  currentStep === "approve" ? "bg-orange-500/20 text-orange-400" :
                  "bg-green-500/20 text-green-400"
                }`}>
                  {isArabic ? stepData.badgeAr : stepData.badge}
                </span>
              </div>
            </div>

            {/* Content */}
            <AnimatePresence mode="wait">
              <motion.div
                key={currentStep}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                exit={{ opacity: 0, y: -10 }}
                className="p-6"
              >
                <div className="mb-4">
                  <h3 className="text-lg font-bold text-white mb-1">{isArabic ? stepData.titleAr : stepData.title}</h3>
                  <p className="text-gray-400 text-sm">{isArabic ? stepData.descriptionAr : stepData.description}</p>
                </div>

                <div className="bg-gray-800/50 rounded-xl p-4 mb-6">
                  <div className="space-y-2">
                    {(isArabic ? stepData.detailsAr : stepData.details).map((detail, index) => (
                      <motion.div
                        key={index}
                        initial={{ opacity: 0, x: isArabic ? 10 : -10 }}
                        animate={{ opacity: 1, x: 0 }}
                        transition={{ delay: index * 0.1 }}
                        className="flex items-start gap-3"
                      >
                        <span className="text-violet-400 text-sm">{isArabic ? '<' : '>'}</span>
                        <span className="text-gray-300 text-sm font-mono">{detail}</span>
                      </motion.div>
                    ))}
                  </div>
                </div>

                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2 text-sm text-gray-500">
                    <Clock className="w-4 h-4" />
                    <span>{isArabic ? "سُجل في" : "Logged at"} {currentTime || "--:--:--"}</span>
                  </div>
                  <Button
                    onClick={advanceStep}
                    className="bg-violet-600 hover:bg-violet-700 text-white group"
                  >
                    {stepIndex < steps.length - 1 ? (
                      <>
                        {isArabic ? buttonLabels[currentStep].ar : buttonLabels[currentStep].en}
                        <ArrowRight className={`w-4 h-4 ${isArabic ? "mr-2 group-hover:-translate-x-1" : "ml-2 group-hover:translate-x-1"} transition-transform ${isArabic ? "rotate-180" : ""}`} />
                      </>
                    ) : (
                      <>
                        <Play className={`w-4 h-4 ${isArabic ? "ml-2" : "mr-2"}`} />
                        {isArabic ? buttonLabels.execute.ar : buttonLabels.execute.en}
                      </>
                    )}
                  </Button>
                </div>
              </motion.div>
            </AnimatePresence>
          </div>

          {/* Trust Badge */}
          <div className="mt-8 text-center">
            <div className="inline-flex items-center gap-3 px-6 py-4 bg-emerald-50 dark:bg-emerald-900/20 border border-emerald-200 dark:border-emerald-800 rounded-xl">
              <Shield className="w-5 h-5 text-emerald-600" />
              <span className="text-emerald-700 dark:text-emerald-400 text-sm font-medium">
                {isArabic
                  ? "لا إجراء للذكاء الاصطناعي بدون موافقة بشرية — بياناتك، تحكمك"
                  : "No AI action without human approval — your data, your control"}
              </span>
            </div>
          </div>
        </motion.div>
      </div>
    </section>
  )
}
