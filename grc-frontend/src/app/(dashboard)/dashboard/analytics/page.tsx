"use client"

export const dynamic = 'force-dynamic'

import { useState } from "react"
import { motion } from "framer-motion"
import { 
  BarChart3, 
  PieChart, 
  TrendingUp, 
  Activity,
  Database,
  Layers,
  RefreshCw,
  Download,
  Calendar,
  Filter
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { SupersetEmbed, GrafanaEmbed, MetabaseEmbed } from "@/components/dashboard/SupersetEmbed"

const analyticsTools = [
  {
    id: "superset",
    name: "Apache Superset",
    nameAr: "تحليلات متقدمة",
    description: "لوحات تحكم تفاعلية وتقارير مخصصة",
    icon: BarChart3,
    color: "emerald",
    license: "Apache 2.0",
    url: "http://localhost:8088",
  },
  {
    id: "grafana",
    name: "Grafana",
    nameAr: "مراقبة حية",
    description: "مراقبة النظام والتنبيهات في الوقت الفعلي",
    icon: Activity,
    color: "orange",
    license: "AGPL v3",
    url: "http://localhost:3030",
  },
  {
    id: "metabase",
    name: "Metabase",
    nameAr: "استكشاف البيانات",
    description: "استعلامات سهلة وتقارير سريعة",
    icon: Database,
    color: "blue",
    license: "AGPL",
    url: "http://localhost:3033",
  },
]

const dashboards = [
  { id: "compliance-overview", name: "نظرة عامة على الامتثال", tool: "superset" },
  { id: "risk-heatmap", name: "خريطة المخاطر الحرارية", tool: "superset" },
  { id: "audit-tracker", name: "متتبع التدقيق", tool: "superset" },
  { id: "grc-compliance-overview", name: "مراقبة الامتثال", tool: "grafana" },
  { id: "system-health", name: "صحة النظام", tool: "grafana" },
]

export default function AnalyticsPage() {
  const [selectedTool, setSelectedTool] = useState("superset")
  const [selectedDashboard, setSelectedDashboard] = useState("compliance-overview")
  const [dateRange, setDateRange] = useState("30d")

  const currentTool = analyticsTools.find(t => t.id === selectedTool)

  return (
    <motion.div
      className="space-y-6"
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
    >
      {/* Page Header */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">
            التحليلات والتقارير
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">
            لوحات تحكم تفاعلية ورؤى تحليلية متقدمة
          </p>
        </div>
        <div className="flex items-center gap-3">
          {/* Date Range Selector */}
          <select
            value={dateRange}
            onChange={(e) => setDateRange(e.target.value)}
            className="h-10 px-3 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm"
          >
            <option value="7d">آخر 7 أيام</option>
            <option value="30d">آخر 30 يوم</option>
            <option value="90d">آخر 90 يوم</option>
            <option value="1y">السنة الماضية</option>
          </select>
          <Button variant="outline" size="sm">
            <Filter className="w-4 h-4 ml-2" />
            تصفية
          </Button>
          <Button variant="outline" size="sm">
            <Download className="w-4 h-4 ml-2" />
            تصدير
          </Button>
        </div>
      </div>

      {/* Tools Selection */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {analyticsTools.map((tool) => (
          <motion.div
            key={tool.id}
            whileHover={{ scale: 1.02 }}
            whileTap={{ scale: 0.98 }}
          >
            <Card
              className={`cursor-pointer transition-all ${
                selectedTool === tool.id
                  ? `ring-2 ring-${tool.color}-500 border-${tool.color}-500`
                  : "hover:border-gray-300"
              }`}
              onClick={() => setSelectedTool(tool.id)}
            >
              <CardContent className="p-4">
                <div className="flex items-start justify-between">
                  <div className="flex items-center gap-3">
                    <div className={`w-12 h-12 rounded-xl bg-${tool.color}-100 dark:bg-${tool.color}-900/20 flex items-center justify-center`}>
                      <tool.icon className={`w-6 h-6 text-${tool.color}-600 dark:text-${tool.color}-400`} />
                    </div>
                    <div>
                      <h3 className="font-semibold text-gray-900 dark:text-white">
                        {tool.nameAr}
                      </h3>
                      <p className="text-sm text-gray-500 dark:text-gray-400">
                        {tool.name}
                      </p>
                    </div>
                  </div>
                  <Badge variant="secondary" className="text-xs">
                    {tool.license}
                  </Badge>
                </div>
                <p className="text-sm text-gray-600 dark:text-gray-400 mt-3">
                  {tool.description}
                </p>
              </CardContent>
            </Card>
          </motion.div>
        ))}
      </div>

      {/* Dashboard Selection Tabs */}
      <div className="flex items-center gap-2 overflow-x-auto pb-2">
        {dashboards
          .filter((d) => d.tool === selectedTool)
          .map((dashboard) => (
            <Button
              key={dashboard.id}
              variant={selectedDashboard === dashboard.id ? "default" : "outline"}
              size="sm"
              onClick={() => setSelectedDashboard(dashboard.id)}
              className="whitespace-nowrap"
            >
              {dashboard.name}
            </Button>
          ))}
        <Button
          variant="ghost"
          size="sm"
          onClick={() => window.open(currentTool?.url, "_blank")}
          className="mr-auto"
        >
          <Layers className="w-4 h-4 ml-2" />
          فتح {currentTool?.name}
        </Button>
      </div>

      {/* Embedded Dashboard */}
      <motion.div
        key={`${selectedTool}-${selectedDashboard}`}
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.3 }}
      >
        {selectedTool === "superset" && (
          <SupersetEmbed
            dashboardId={selectedDashboard}
            title={dashboards.find((d) => d.id === selectedDashboard)?.name}
            height="700px"
          />
        )}
        {selectedTool === "grafana" && (
          <GrafanaEmbed
            dashboardUid={selectedDashboard}
            title={dashboards.find((d) => d.id === selectedDashboard)?.name}
            height="700px"
            from={`now-${dateRange}`}
          />
        )}
        {selectedTool === "metabase" && (
          <MetabaseEmbed
            dashboardId={1}
            title="لوحة تحليلات Metabase"
            height="700px"
          />
        )}
      </motion.div>

      {/* Quick Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardContent className="p-4 flex items-center gap-4">
            <div className="w-12 h-12 rounded-xl bg-emerald-100 dark:bg-emerald-900/20 flex items-center justify-center">
              <TrendingUp className="w-6 h-6 text-emerald-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900 dark:text-white">87.5%</p>
              <p className="text-sm text-gray-500">نسبة الامتثال</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 flex items-center gap-4">
            <div className="w-12 h-12 rounded-xl bg-amber-100 dark:bg-amber-900/20 flex items-center justify-center">
              <Activity className="w-6 h-6 text-amber-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900 dark:text-white">24</p>
              <p className="text-sm text-gray-500">مخاطر مفتوحة</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 flex items-center gap-4">
            <div className="w-12 h-12 rounded-xl bg-blue-100 dark:bg-blue-900/20 flex items-center justify-center">
              <BarChart3 className="w-6 h-6 text-blue-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900 dark:text-white">156</p>
              <p className="text-sm text-gray-500">تقييم مكتمل</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4 flex items-center gap-4">
            <div className="w-12 h-12 rounded-xl bg-purple-100 dark:bg-purple-900/20 flex items-center justify-center">
              <PieChart className="w-6 h-6 text-purple-600" />
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900 dark:text-white">8</p>
              <p className="text-sm text-gray-500">تدقيق نشط</p>
            </div>
          </CardContent>
        </Card>
      </div>
    </motion.div>
  )
}
