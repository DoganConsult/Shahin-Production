"use client"

import { motion } from "framer-motion"
import { 
  Shield, 
  AlertTriangle, 
  FileCheck, 
  CheckCircle, 
  Clock, 
  TrendingUp, 
  TrendingDown,
  ArrowUpRight,
  ArrowDownRight,
  BarChart3,
  PieChart,
  Activity,
} from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"

const stats = [
  {
    title: "نسبة الامتثال الكلية",
    value: "87.5%",
    change: "+2.5%",
    trend: "up",
    icon: Shield,
    color: "emerald",
  },
  {
    title: "المخاطر المفتوحة",
    value: "24",
    change: "-3",
    trend: "down",
    icon: AlertTriangle,
    color: "amber",
  },
  {
    title: "عمليات التدقيق النشطة",
    value: "8",
    change: "+2",
    trend: "up",
    icon: FileCheck,
    color: "blue",
  },
  {
    title: "المهام المكتملة",
    value: "156",
    change: "+12",
    trend: "up",
    icon: CheckCircle,
    color: "purple",
  },
]

const complianceByFramework = [
  { name: "NCA ECC-1:2018", value: 92, total: 100 },
  { name: "SAMA Cybersecurity", value: 85, total: 100 },
  { name: "ISO 27001:2022", value: 78, total: 100 },
  { name: "PDPL", value: 88, total: 100 },
  { name: "PCI DSS 4.0", value: 72, total: 100 },
]

const recentActivities = [
  { id: 1, action: "تم إكمال تقييم", item: "NCA ECC Domain 1", user: "أحمد", time: "منذ 5 دقائق", type: "success" },
  { id: 2, action: "تم رفع دليل جديد", item: "سياسة أمن المعلومات", user: "سارة", time: "منذ 15 دقيقة", type: "info" },
  { id: 3, action: "مخاطرة جديدة", item: "ثغرة في نظام الدفع", user: "محمد", time: "منذ 30 دقيقة", type: "warning" },
  { id: 4, action: "تم إغلاق مهمة", item: "مراجعة السياسات الأمنية", user: "فاطمة", time: "منذ ساعة", type: "success" },
  { id: 5, action: "تعليق جديد", item: "خطة الاستجابة للحوادث", user: "خالد", time: "منذ ساعتين", type: "default" },
]

const upcomingDeadlines = [
  { id: 1, title: "تقرير الامتثال الربعي", date: "15 يناير 2024", priority: "high" },
  { id: 2, title: "مراجعة سياسة الخصوصية", date: "20 يناير 2024", priority: "medium" },
  { id: 3, title: "تدقيق ISO 27001", date: "1 فبراير 2024", priority: "high" },
  { id: 4, title: "تحديث سجل المخاطر", date: "5 فبراير 2024", priority: "low" },
]

const containerVariants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: { staggerChildren: 0.1 }
  }
}

const itemVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: { opacity: 1, y: 0 }
}

export default function DashboardPage() {
  return (
    <motion.div
      className="space-y-6"
      variants={containerVariants}
      initial="hidden"
      animate="visible"
    >
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
            لوحة التحكم
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">
            مرحباً بك في نظام شاهين لإدارة الحوكمة والمخاطر والامتثال
          </p>
        </div>
        <div className="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
          <Clock className="w-4 h-4" />
          آخر تحديث: اليوم الساعة 10:30 ص
        </div>
      </div>

      {/* Stats Grid */}
      <motion.div 
        className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6"
        variants={itemVariants}
      >
        {stats.map((stat, index) => (
          <motion.div
            key={stat.title}
            variants={itemVariants}
          >
            <Card hover className="relative overflow-hidden">
              <CardContent className="p-6">
                <div className="flex items-start justify-between">
                  <div>
                    <p className="text-sm font-medium text-gray-600 dark:text-gray-400 mb-1">
                      {stat.title}
                    </p>
                    <p className="text-3xl font-bold text-gray-900 dark:text-white">
                      {stat.value}
                    </p>
                    <div className={`flex items-center gap-1 mt-2 text-sm ${
                      stat.trend === "up" ? "text-emerald-600" : "text-amber-600"
                    }`}>
                      {stat.trend === "up" ? (
                        <ArrowUpRight className="w-4 h-4" />
                      ) : (
                        <ArrowDownRight className="w-4 h-4" />
                      )}
                      <span>{stat.change} من الشهر الماضي</span>
                    </div>
                  </div>
                  <div className={`w-12 h-12 rounded-xl flex items-center justify-center bg-${stat.color}-100 dark:bg-${stat.color}-900/20`}>
                    <stat.icon className={`w-6 h-6 text-${stat.color}-600 dark:text-${stat.color}-400`} />
                  </div>
                </div>
              </CardContent>
              <div className={`absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-${stat.color}-500 to-${stat.color}-600`} />
            </Card>
          </motion.div>
        ))}
      </motion.div>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Compliance by Framework */}
        <motion.div variants={itemVariants} className="lg:col-span-2">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle className="text-lg font-semibold">الامتثال حسب الإطار التنظيمي</CardTitle>
              <BarChart3 className="w-5 h-5 text-gray-400" />
            </CardHeader>
            <CardContent className="space-y-4">
              {complianceByFramework.map((item) => (
                <div key={item.name} className="space-y-2">
                  <div className="flex items-center justify-between text-sm">
                    <span className="font-medium text-gray-700 dark:text-gray-300">{item.name}</span>
                    <span className="text-gray-600 dark:text-gray-400">{item.value}%</span>
                  </div>
                  <div className="h-2 bg-gray-100 dark:bg-gray-700 rounded-full overflow-hidden">
                    <motion.div
                      className={`h-full rounded-full ${
                        item.value >= 90 ? "bg-emerald-500" :
                        item.value >= 70 ? "bg-amber-500" : "bg-red-500"
                      }`}
                      initial={{ width: 0 }}
                      animate={{ width: `${item.value}%` }}
                      transition={{ duration: 1, delay: 0.2 }}
                    />
                  </div>
                </div>
              ))}
            </CardContent>
          </Card>
        </motion.div>

        {/* Upcoming Deadlines */}
        <motion.div variants={itemVariants}>
          <Card className="h-full">
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle className="text-lg font-semibold">المواعيد القادمة</CardTitle>
              <Clock className="w-5 h-5 text-gray-400" />
            </CardHeader>
            <CardContent className="space-y-4">
              {upcomingDeadlines.map((item) => (
                <div key={item.id} className="flex items-start gap-3 p-3 rounded-xl bg-gray-50 dark:bg-gray-800/50">
                  <div className={`w-2 h-2 mt-2 rounded-full ${
                    item.priority === "high" ? "bg-red-500" :
                    item.priority === "medium" ? "bg-amber-500" : "bg-gray-400"
                  }`} />
                  <div className="flex-1">
                    <p className="font-medium text-gray-900 dark:text-white text-sm">
                      {item.title}
                    </p>
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                      {item.date}
                    </p>
                  </div>
                  <Badge variant={
                    item.priority === "high" ? "critical" :
                    item.priority === "medium" ? "warning" : "secondary"
                  }>
                    {item.priority === "high" ? "عاجل" :
                     item.priority === "medium" ? "متوسط" : "منخفض"}
                  </Badge>
                </div>
              ))}
            </CardContent>
          </Card>
        </motion.div>
      </div>

      {/* Recent Activity */}
      <motion.div variants={itemVariants}>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-lg font-semibold">النشاط الأخير</CardTitle>
            <Activity className="w-5 h-5 text-gray-400" />
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {recentActivities.map((activity, index) => (
                <motion.div
                  key={activity.id}
                  className="flex items-center gap-4 p-4 rounded-xl bg-gray-50 dark:bg-gray-800/50"
                  initial={{ opacity: 0, x: -20 }}
                  animate={{ opacity: 1, x: 0 }}
                  transition={{ delay: index * 0.1 }}
                >
                  <div className={`w-10 h-10 rounded-full flex items-center justify-center ${
                    activity.type === "success" ? "bg-emerald-100 dark:bg-emerald-900/20" :
                    activity.type === "warning" ? "bg-amber-100 dark:bg-amber-900/20" :
                    activity.type === "info" ? "bg-blue-100 dark:bg-blue-900/20" :
                    "bg-gray-100 dark:bg-gray-700"
                  }`}>
                    <span className="text-lg">
                      {activity.type === "success" ? "✓" :
                       activity.type === "warning" ? "⚠" :
                       activity.type === "info" ? "ℹ" : "💬"}
                    </span>
                  </div>
                  <div className="flex-1">
                    <p className="text-sm">
                      <span className="font-medium text-gray-900 dark:text-white">{activity.action}</span>
                      {" - "}
                      <span className="text-gray-600 dark:text-gray-400">{activity.item}</span>
                    </p>
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                      بواسطة {activity.user} • {activity.time}
                    </p>
                  </div>
                </motion.div>
              ))}
            </div>
          </CardContent>
        </Card>
      </motion.div>
    </motion.div>
  )
}
