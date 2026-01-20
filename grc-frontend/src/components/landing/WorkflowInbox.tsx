"use client"

import { useState } from "react"
import { motion, AnimatePresence } from "framer-motion"
import {
  Inbox,
  Clock,
  AlertTriangle,
  CheckCircle,
  ArrowUpRight,
  Filter,
  Bell,
  User,
  FileText,
  Receipt,
  Users,
  Shield,
  ChevronRight,
  Zap
} from "lucide-react"
import { useLocale } from "@/components/providers/locale-provider"

type TaskStatus = "urgent" | "pending" | "escalated" | "approved"
type TaskCategory = "all" | "approvals" | "reviews" | "alerts"

interface WorkflowTask {
  id: string
  title: string
  titleAr: string
  description: string
  descriptionAr: string
  category: "approval" | "review" | "alert"
  status: TaskStatus
  slaHours: number
  slaRemaining: number
  requester: string
  requesterAr: string
  icon: typeof FileText
  priority: "high" | "medium" | "low"
}

const mockTasks: WorkflowTask[] = [
  {
    id: "1",
    title: "Invoice Approval Required",
    titleAr: "مطلوب موافقة على الفاتورة",
    description: "ABC Corp - SAR 45,000 - Office Equipment",
    descriptionAr: "شركة ABC - 45,000 ريال - معدات مكتبية",
    category: "approval",
    status: "urgent",
    slaHours: 24,
    slaRemaining: 2,
    requester: "Ahmed Al-Salem",
    requesterAr: "أحمد السالم",
    icon: Receipt,
    priority: "high"
  },
  {
    id: "2",
    title: "New Hire Onboarding",
    titleAr: "تهيئة موظف جديد",
    description: "Sarah Ahmed - Software Engineer - Start: May 15",
    descriptionAr: "سارة أحمد - مهندسة برمجيات - البدء: 15 مايو",
    category: "approval",
    status: "pending",
    slaHours: 48,
    slaRemaining: 36,
    requester: "HR Department",
    requesterAr: "قسم الموارد البشرية",
    icon: Users,
    priority: "medium"
  },
  {
    id: "3",
    title: "Access Review Due",
    titleAr: "مراجعة الصلاحيات مستحقة",
    description: "Q2 User Access Certification - 15 users pending",
    descriptionAr: "شهادة وصول المستخدم Q2 - 15 مستخدم معلق",
    category: "review",
    status: "escalated",
    slaHours: 72,
    slaRemaining: -4,
    requester: "Compliance Team",
    requesterAr: "فريق الامتثال",
    icon: Shield,
    priority: "high"
  },
  {
    id: "4",
    title: "Policy Update Review",
    titleAr: "مراجعة تحديث السياسة",
    description: "Data Retention Policy v2.1 - Final Review",
    descriptionAr: "سياسة الاحتفاظ بالبيانات v2.1 - المراجعة النهائية",
    category: "review",
    status: "pending",
    slaHours: 120,
    slaRemaining: 96,
    requester: "Legal Team",
    requesterAr: "الفريق القانوني",
    icon: FileText,
    priority: "low"
  },
  {
    id: "5",
    title: "Segregation of Duties Alert",
    titleAr: "تنبيه فصل المهام",
    description: "User has conflicting permissions in Finance module",
    descriptionAr: "المستخدم لديه صلاحيات متعارضة في وحدة المالية",
    category: "alert",
    status: "urgent",
    slaHours: 8,
    slaRemaining: 3,
    requester: "System",
    requesterAr: "النظام",
    icon: AlertTriangle,
    priority: "high"
  }
]

const content = {
  sectionLabel: { en: "Unified Workflow", ar: "سير العمل الموحد" },
  title: { en: "One Inbox for All Your Approvals", ar: "صندوق واحد لجميع موافقاتك" },
  subtitle: {
    en: "Never miss a deadline. See all pending approvals, reviews, and alerts with SLA tracking and automatic escalations.",
    ar: "لا تفوت أي موعد نهائي. شاهد جميع الموافقات والمراجعات والتنبيهات المعلقة مع تتبع SLA والتصعيد التلقائي."
  },
  inboxTitle: { en: "My Workflow Inbox", ar: "صندوق سير العمل الخاص بي" },
  itemsAttention: { en: "5 items need your attention", ar: "5 عناصر تحتاج انتباهك" },
  categories: {
    all: { en: "All Tasks", ar: "جميع المهام" },
    approvals: { en: "Approvals", ar: "الموافقات" },
    reviews: { en: "Reviews", ar: "المراجعات" },
    alerts: { en: "Alerts", ar: "التنبيهات" }
  },
  statusLabels: {
    urgent: { en: "Urgent", ar: "عاجل" },
    escalated: { en: "Escalated", ar: "تم التصعيد" },
    pending: { en: "Pending", ar: "معلق" },
    approved: { en: "Approved", ar: "تمت الموافقة" }
  },
  sla: {
    overdue: { en: "Overdue by", ar: "متأخر بـ" },
    remaining: { en: "remaining", ar: "متبقي" },
    hours: { en: "h", ar: "س" }
  },
  footer: {
    urgent: { en: "Urgent", ar: "عاجل" },
    escalated: { en: "Escalated", ar: "تم التصعيد" },
    pending: { en: "Pending", ar: "معلق" },
    autoEscalation: { en: "Auto-escalation enabled", ar: "التصعيد التلقائي مفعّل" }
  },
  features: {
    slaTracking: {
      title: { en: "SLA Tracking", ar: "تتبع SLA" },
      description: {
        en: "Real-time countdown timers for every task with automatic reminders",
        ar: "مؤقتات عد تنازلي في الوقت الفعلي لكل مهمة مع تذكيرات تلقائية"
      }
    },
    autoEscalation: {
      title: { en: "Auto-Escalation", ar: "التصعيد التلقائي" },
      description: {
        en: "Tasks automatically escalate to managers when SLAs are breached",
        ar: "تصعيد المهام تلقائياً للمديرين عند تجاوز SLA"
      }
    },
    smartNotifications: {
      title: { en: "Smart Notifications", ar: "الإشعارات الذكية" },
      description: {
        en: "Email, Slack, and mobile push notifications based on urgency",
        ar: "إشعارات البريد الإلكتروني وSlack والدفع للجوال بناءً على الأولوية"
      }
    }
  }
}

function getSlaColor(remaining: number, total: number): string {
  const percentage = remaining / total
  if (remaining <= 0) return "text-red-500"
  if (percentage < 0.25) return "text-red-500"
  if (percentage < 0.5) return "text-orange-500"
  return "text-emerald-500"
}

export function WorkflowInbox() {
  const [activeCategory, setActiveCategory] = useState<TaskCategory>("all")
  const [selectedTask, setSelectedTask] = useState<WorkflowTask | null>(null)
  const { locale } = useLocale()
  const isArabic = locale === "ar"

  const categories: { value: TaskCategory; label: string; count: number }[] = [
    { value: "all", label: isArabic ? content.categories.all.ar : content.categories.all.en, count: 5 },
    { value: "approvals", label: isArabic ? content.categories.approvals.ar : content.categories.approvals.en, count: 2 },
    { value: "reviews", label: isArabic ? content.categories.reviews.ar : content.categories.reviews.en, count: 2 },
    { value: "alerts", label: isArabic ? content.categories.alerts.ar : content.categories.alerts.en, count: 1 }
  ]

  const getStatusBadge = (status: TaskStatus) => {
    const labels = content.statusLabels
    switch (status) {
      case "urgent":
        return { bg: "bg-red-100 dark:bg-red-900/30", text: "text-red-700 dark:text-red-400", label: isArabic ? labels.urgent.ar : labels.urgent.en }
      case "escalated":
        return { bg: "bg-orange-100 dark:bg-orange-900/30", text: "text-orange-700 dark:text-orange-400", label: isArabic ? labels.escalated.ar : labels.escalated.en }
      case "pending":
        return { bg: "bg-blue-100 dark:bg-blue-900/30", text: "text-blue-700 dark:text-blue-400", label: isArabic ? labels.pending.ar : labels.pending.en }
      case "approved":
        return { bg: "bg-emerald-100 dark:bg-emerald-900/30", text: "text-emerald-700 dark:text-emerald-400", label: isArabic ? labels.approved.ar : labels.approved.en }
    }
  }

  const filteredTasks = mockTasks.filter(task => {
    if (activeCategory === "all") return true
    if (activeCategory === "approvals") return task.category === "approval"
    if (activeCategory === "reviews") return task.category === "review"
    if (activeCategory === "alerts") return task.category === "alert"
    return true
  })

  const formatSlaTime = (remaining: number) => {
    if (remaining <= 0) {
      return `${isArabic ? content.sla.overdue.ar : content.sla.overdue.en} ${Math.abs(remaining)}${isArabic ? content.sla.hours.ar : content.sla.hours.en}`
    }
    return `${remaining}${isArabic ? content.sla.hours.ar : content.sla.hours.en} ${isArabic ? content.sla.remaining.ar : content.sla.remaining.en}`
  }

  const featuresList = [
    {
      icon: Clock,
      title: isArabic ? content.features.slaTracking.title.ar : content.features.slaTracking.title.en,
      description: isArabic ? content.features.slaTracking.description.ar : content.features.slaTracking.description.en
    },
    {
      icon: ArrowUpRight,
      title: isArabic ? content.features.autoEscalation.title.ar : content.features.autoEscalation.title.en,
      description: isArabic ? content.features.autoEscalation.description.ar : content.features.autoEscalation.description.en
    },
    {
      icon: Bell,
      title: isArabic ? content.features.smartNotifications.title.ar : content.features.smartNotifications.title.en,
      description: isArabic ? content.features.smartNotifications.description.ar : content.features.smartNotifications.description.en
    }
  ]

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
            {isArabic ? content.sectionLabel.ar : content.sectionLabel.en}
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
            {isArabic ? content.title.ar : content.title.en}
          </h2>
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-2xl mx-auto">
            {isArabic ? content.subtitle.ar : content.subtitle.en}
          </p>
        </motion.div>

        {/* Inbox Preview */}
        <motion.div
          className="max-w-5xl mx-auto"
          initial={{ opacity: 0, y: 30 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <div className="bg-gray-50 dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 overflow-hidden shadow-xl">
            {/* Header */}
            <div className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 p-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-xl bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center">
                    <Inbox className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
                  </div>
                  <div>
                    <h3 className="font-semibold text-gray-900 dark:text-white">
                      {isArabic ? content.inboxTitle.ar : content.inboxTitle.en}
                    </h3>
                    <p className="text-sm text-gray-500">
                      {isArabic ? content.itemsAttention.ar : content.itemsAttention.en}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <button className="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors">
                    <Bell className="w-5 h-5 text-gray-500" />
                  </button>
                  <button className="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors">
                    <Filter className="w-5 h-5 text-gray-500" />
                  </button>
                </div>
              </div>

              {/* Category Tabs */}
              <div className="flex gap-2 mt-4">
                {categories.map((cat) => (
                  <button
                    key={cat.value}
                    onClick={() => setActiveCategory(cat.value)}
                    className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${
                      activeCategory === cat.value
                        ? "bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400"
                        : "text-gray-600 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700"
                    }`}
                  >
                    {cat.label}
                    <span className={`${isArabic ? "mr-2" : "ml-2"} px-2 py-0.5 rounded-full bg-gray-200 dark:bg-gray-600 text-xs`}>
                      {cat.count}
                    </span>
                  </button>
                ))}
              </div>
            </div>

            {/* Task List */}
            <div className="divide-y divide-gray-100 dark:divide-gray-800">
              <AnimatePresence>
                {filteredTasks.map((task, index) => {
                  const statusBadge = getStatusBadge(task.status)
                  return (
                    <motion.div
                      key={task.id}
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, y: -10 }}
                      transition={{ delay: index * 0.05 }}
                      onClick={() => setSelectedTask(task)}
                      className={`p-4 hover:bg-gray-100 dark:hover:bg-gray-800/50 cursor-pointer transition-colors ${
                        selectedTask?.id === task.id ? "bg-emerald-50 dark:bg-emerald-900/10" : ""
                      }`}
                    >
                      <div className="flex items-start gap-4">
                        {/* Icon */}
                        <div className={`w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0 ${
                          task.priority === "high"
                            ? "bg-red-100 dark:bg-red-900/30"
                            : task.priority === "medium"
                            ? "bg-blue-100 dark:bg-blue-900/30"
                            : "bg-gray-100 dark:bg-gray-800"
                        }`}>
                          <task.icon className={`w-5 h-5 ${
                            task.priority === "high"
                              ? "text-red-600 dark:text-red-400"
                              : task.priority === "medium"
                              ? "text-blue-600 dark:text-blue-400"
                              : "text-gray-600 dark:text-gray-400"
                          }`} />
                        </div>

                        {/* Content */}
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2 mb-1">
                            <h4 className="font-semibold text-gray-900 dark:text-white truncate">
                              {isArabic ? task.titleAr : task.title}
                            </h4>
                            <span className={`text-xs px-2 py-0.5 rounded-full ${statusBadge.bg} ${statusBadge.text}`}>
                              {statusBadge.label}
                            </span>
                          </div>
                          <p className="text-sm text-gray-600 dark:text-gray-400 truncate mb-2">
                            {isArabic ? task.descriptionAr : task.description}
                          </p>
                          <div className="flex items-center gap-4 text-xs text-gray-500">
                            <span className="flex items-center gap-1">
                              <User className="w-3 h-3" />
                              {isArabic ? task.requesterAr : task.requester}
                            </span>
                            <span className={`flex items-center gap-1 ${getSlaColor(task.slaRemaining, task.slaHours)}`}>
                              <Clock className="w-3 h-3" />
                              {formatSlaTime(task.slaRemaining)}
                            </span>
                          </div>
                        </div>

                        {/* Actions */}
                        <div className="flex items-center gap-2">
                          <button className="p-2 rounded-lg bg-emerald-100 dark:bg-emerald-900/30 text-emerald-600 dark:text-emerald-400 hover:bg-emerald-200 dark:hover:bg-emerald-900/50 transition-colors">
                            <CheckCircle className="w-4 h-4" />
                          </button>
                          <button className="p-2 rounded-lg bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors">
                            <ChevronRight className={`w-4 h-4 ${isArabic ? "rotate-180" : ""}`} />
                          </button>
                        </div>
                      </div>
                    </motion.div>
                  )
                })}
              </AnimatePresence>
            </div>

            {/* Footer Stats */}
            <div className="bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700 p-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-6 text-sm">
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 rounded-full bg-red-500" />
                    <span className="text-gray-600 dark:text-gray-400">2 {isArabic ? content.footer.urgent.ar : content.footer.urgent.en}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 rounded-full bg-orange-500" />
                    <span className="text-gray-600 dark:text-gray-400">1 {isArabic ? content.footer.escalated.ar : content.footer.escalated.en}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 rounded-full bg-blue-500" />
                    <span className="text-gray-600 dark:text-gray-400">2 {isArabic ? content.footer.pending.ar : content.footer.pending.en}</span>
                  </div>
                </div>
                <div className="flex items-center gap-2 text-sm text-emerald-600 dark:text-emerald-400">
                  <Zap className="w-4 h-4" />
                  <span>{isArabic ? content.footer.autoEscalation.ar : content.footer.autoEscalation.en}</span>
                </div>
              </div>
            </div>
          </div>

          {/* Feature Highlights */}
          <div className="grid md:grid-cols-3 gap-6 mt-8">
            {featuresList.map((feature, index) => (
              <motion.div
                key={index}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className="flex items-start gap-4 p-4 bg-gray-50 dark:bg-gray-900 rounded-xl border border-gray-200 dark:border-gray-800"
              >
                <div className="w-10 h-10 rounded-lg bg-emerald-100 dark:bg-emerald-900/30 flex items-center justify-center flex-shrink-0">
                  <feature.icon className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
                </div>
                <div>
                  <h4 className="font-semibold text-gray-900 dark:text-white mb-1">{feature.title}</h4>
                  <p className="text-sm text-gray-600 dark:text-gray-400">{feature.description}</p>
                </div>
              </motion.div>
            ))}
          </div>
        </motion.div>
      </div>
    </section>
  )
}
