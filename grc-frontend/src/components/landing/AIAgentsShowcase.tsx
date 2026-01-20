"use client"

import { useState, useEffect, useCallback } from "react"
import { motion, AnimatePresence } from "framer-motion"
import {
  Bot,
  Shield,
  AlertTriangle,
  FileSearch,
  FileText,
  BarChart3,
  FileOutput,
  Activity,
  HeadphonesIcon,
  GitBranch,
  FolderCheck,
  Mail,
  ChevronRight,
  Sparkles,
  CheckCircle,
  Zap,
  Brain,
  X,
  Play,
  MessageSquare,
  ArrowRight,
  Clock,
  Users,
  Eye,
  Send,
  Loader2,
  Check,
  AlertCircle,
  TrendingUp,
  FileCheck,
  ChevronDown
} from "lucide-react"
import { Button } from "@/components/ui/button"
import Link from "next/link"
import { useLocale } from "@/components/providers/locale-provider"

interface AIAgent {
  id: string
  code: string
  name: string
  nameAr: string
  description: string
  descriptionAr: string
  icon: typeof Bot
  color: string
  bgColor: string
  capabilities: string[]
  capabilitiesAr: string[]
  category: "orchestrator" | "analysis" | "analytics" | "reporting" | "monitoring" | "support" | "automation" | "collection" | "communication"
  oversightRole: string
  oversightRoleAr: string
  autoApprovalThreshold: number
  dataSources: string[]
  scenarios: Scenario[]
}

interface Scenario {
  id: string
  title: string
  titleAr: string
  description: string
  descriptionAr: string
  steps: ScenarioStep[]
}

interface ScenarioStep {
  type: "analysis" | "finding" | "recommendation" | "action" | "approval"
  content: string
  contentAr: string
  status: "pending" | "processing" | "complete"
  confidence?: number
  details?: string
  detailsAr?: string
}

interface ChatMessage {
  role: "user" | "agent"
  content: string
  contentAr: string
  timestamp: Date
}

// Mirrored from backend: AiAgentTeamSeeds.cs with full scenarios
const agents: AIAgent[] = [
  {
    id: "shahin",
    code: "SHAHIN_AI",
    name: "Shahin AI Assistant",
    nameAr: "شاهين - المساعد الذكي",
    description: "Primary orchestrator that coordinates all AI agents for seamless GRC operations",
    descriptionAr: "المنسق الرئيسي الذي ينسق جميع وكلاء الذكاء الاصطناعي لعمليات GRC السلسة",
    icon: Brain,
    color: "text-violet-500",
    bgColor: "bg-violet-100 dark:bg-violet-900/30",
    category: "orchestrator",
    oversightRole: "Platform Admin",
    oversightRoleAr: "مسؤول المنصة",
    autoApprovalThreshold: 95,
    dataSources: ["AllModules", "UserContext", "TenantData", "SystemMetrics"],
    capabilities: ["Orchestrate Tasks", "Delegate to Agents", "Provide Guidance", "Answer Questions", "Generate Reports", "Monitor Agents"],
    capabilitiesAr: ["تنسيق المهام", "التفويض للوكلاء", "تقديم التوجيه", "الإجابة على الأسئلة", "إنشاء التقارير", "مراقبة الوكلاء"],
    scenarios: [
      {
        id: "orchestrate-assessment",
        title: "Coordinating NCA Assessment",
        titleAr: "تنسيق تقييم NCA",
        description: "Watch Shahin orchestrate multiple agents to complete a full NCA compliance assessment",
        descriptionAr: "شاهد شاهين ينسق عدة وكلاء لإكمال تقييم الامتثال الكامل لـ NCA",
        steps: [
          { type: "analysis", content: "Analyzing assessment request and identifying required agents...", contentAr: "تحليل طلب التقييم وتحديد الوكلاء المطلوبين...", status: "pending", confidence: 98 },
          { type: "action", content: "Delegating to Compliance Agent for NCA-ECC framework analysis", contentAr: "التفويض لوكيل الامتثال لتحليل إطار NCA-ECC", status: "pending" },
          { type: "action", content: "Delegating to Evidence Agent for automated evidence collection", contentAr: "التفويض لوكيل الأدلة للجمع التلقائي للأدلة", status: "pending" },
          { type: "finding", content: "3 agents working in parallel - estimated completion: 45 minutes", contentAr: "3 وكلاء يعملون بالتوازي - الوقت المتوقع: 45 دقيقة", status: "pending", details: "Compliance Agent: 40%, Evidence Agent: 25%, Risk Agent: 15%", detailsAr: "وكيل الامتثال: 40%، وكيل الأدلة: 25%، وكيل المخاطر: 15%" },
          { type: "recommendation", content: "Assessment draft ready for review - 87% compliance score identified", contentAr: "مسودة التقييم جاهزة للمراجعة - تم تحديد درجة امتثال 87%", status: "pending", confidence: 95 },
          { type: "approval", content: "Awaiting human approval before finalizing report", contentAr: "في انتظار الموافقة البشرية قبل إنهاء التقرير", status: "pending" }
        ]
      }
    ]
  },
  {
    id: "compliance",
    code: "COMPLIANCE_AGENT",
    name: "Compliance Analysis Agent",
    nameAr: "وكيل تحليل الامتثال",
    description: "Analyzes compliance requirements for NCA, SAMA, PDPL frameworks and identifies gaps",
    descriptionAr: "يحلل متطلبات الامتثال لأطر NCA وSAMA وPDPL ويحدد الثغرات",
    icon: Shield,
    color: "text-emerald-500",
    bgColor: "bg-emerald-100 dark:bg-emerald-900/30",
    category: "analysis",
    oversightRole: "Compliance Officer",
    oversightRoleAr: "مسؤول الامتثال",
    autoApprovalThreshold: 85,
    dataSources: ["Frameworks", "Controls", "Assessments", "Evidence", "Mappings"],
    capabilities: ["Analyze Framework", "Identify Gaps", "Suggest Remediation", "Map Requirements", "Score Compliance", "Generate Compliance Report"],
    capabilitiesAr: ["تحليل الإطار", "تحديد الثغرات", "اقتراح المعالجة", "ربط المتطلبات", "تسجيل الامتثال", "إنشاء تقرير الامتثال"],
    scenarios: [
      {
        id: "gap-analysis",
        title: "NCA-ECC Gap Analysis",
        titleAr: "تحليل فجوات NCA-ECC",
        description: "Real-time gap analysis against NCA Essential Cybersecurity Controls",
        descriptionAr: "تحليل فجوات في الوقت الفعلي مقابل ضوابط الأمن السيبراني الأساسية لـ NCA",
        steps: [
          { type: "analysis", content: "Loading NCA-ECC framework (114 controls across 5 domains)...", contentAr: "تحميل إطار NCA-ECC (114 ضابط عبر 5 مجالات)...", status: "pending", confidence: 100 },
          { type: "analysis", content: "Mapping existing controls to NCA requirements...", contentAr: "ربط الضوابط الحالية بمتطلبات NCA...", status: "pending", details: "Governance: 18 controls, Defense: 42 controls, Resilience: 24 controls", detailsAr: "الحوكمة: 18 ضابط، الدفاع: 42 ضابط، المرونة: 24 ضابط" },
          { type: "finding", content: "Gap identified: 12 controls require implementation", contentAr: "تم تحديد فجوة: 12 ضابط تتطلب التنفيذ", status: "pending", confidence: 92, details: "Critical: 3, High: 5, Medium: 4", detailsAr: "حرج: 3، عالي: 5، متوسط: 4" },
          { type: "finding", content: "Gap identified: 8 controls partially implemented", contentAr: "تم تحديد فجوة: 8 ضوابط منفذة جزئياً", status: "pending", confidence: 88 },
          { type: "recommendation", content: "Priority remediation plan generated with 45-day timeline", contentAr: "تم إنشاء خطة معالجة ذات أولوية بجدول زمني 45 يوماً", status: "pending", confidence: 94 },
          { type: "approval", content: "Remediation plan requires Compliance Officer approval", contentAr: "تتطلب خطة المعالجة موافقة مسؤول الامتثال", status: "pending" }
        ]
      }
    ]
  },
  {
    id: "risk",
    code: "RISK_AGENT",
    name: "Risk Assessment Agent",
    nameAr: "وكيل تقييم المخاطر",
    description: "Analyzes and scores risks, identifies risk factors, and suggests mitigation strategies",
    descriptionAr: "يحلل المخاطر ويسجلها، ويحدد عوامل الخطر، ويقترح استراتيجيات التخفيف",
    icon: AlertTriangle,
    color: "text-orange-500",
    bgColor: "bg-orange-100 dark:bg-orange-900/30",
    category: "analysis",
    oversightRole: "Risk Manager",
    oversightRoleAr: "مدير المخاطر",
    autoApprovalThreshold: 80,
    dataSources: ["Risks", "Controls", "Incidents", "Threats", "Vulnerabilities"],
    capabilities: ["Analyze Risk", "Score Risk", "Identify Factors", "Suggest Mitigation", "Monitor Risk", "Generate Risk Report"],
    capabilitiesAr: ["تحليل المخاطر", "تسجيل المخاطر", "تحديد العوامل", "اقتراح التخفيف", "مراقبة المخاطر", "إنشاء تقرير المخاطر"],
    scenarios: [
      {
        id: "risk-assessment",
        title: "Third-Party Vendor Risk Assessment",
        titleAr: "تقييم مخاطر الطرف الثالث",
        description: "Comprehensive vendor risk assessment with automated scoring",
        descriptionAr: "تقييم شامل لمخاطر الموردين مع تسجيل تلقائي",
        steps: [
          { type: "analysis", content: "Analyzing vendor profile: CloudTech Solutions Ltd.", contentAr: "تحليل ملف المورد: CloudTech Solutions Ltd.", status: "pending", confidence: 96 },
          { type: "analysis", content: "Evaluating 47 risk factors across 6 categories...", contentAr: "تقييم 47 عامل خطر عبر 6 فئات...", status: "pending", details: "Security: 12, Financial: 8, Operational: 10, Legal: 7, Reputational: 5, Strategic: 5", detailsAr: "الأمان: 12، المالي: 8، التشغيلي: 10، القانوني: 7، السمعة: 5، الاستراتيجي: 5" },
          { type: "finding", content: "Risk Score: 72/100 (Medium-High Risk)", contentAr: "درجة المخاطر: 72/100 (مخاطر متوسطة-عالية)", status: "pending", confidence: 89, details: "Primary concern: Data handling practices", detailsAr: "القلق الرئيسي: ممارسات معالجة البيانات" },
          { type: "finding", content: "3 critical findings requiring immediate attention", contentAr: "3 نتائج حرجة تتطلب اهتماماً فورياً", status: "pending", confidence: 91 },
          { type: "recommendation", content: "Recommend: Enhanced monitoring + contractual safeguards", contentAr: "التوصية: مراقبة معززة + ضمانات تعاقدية", status: "pending", confidence: 87 },
          { type: "approval", content: "Risk acceptance decision requires Risk Manager approval", contentAr: "قرار قبول المخاطر يتطلب موافقة مدير المخاطر", status: "pending" }
        ]
      }
    ]
  },
  {
    id: "audit",
    code: "AUDIT_AGENT",
    name: "Audit Analysis Agent",
    nameAr: "وكيل تحليل التدقيق",
    description: "Analyzes audit trails, identifies patterns, and provides findings analysis",
    descriptionAr: "يحلل مسارات التدقيق، ويحدد الأنماط، ويقدم تحليل النتائج",
    icon: FileSearch,
    color: "text-blue-500",
    bgColor: "bg-blue-100 dark:bg-blue-900/30",
    category: "analysis",
    oversightRole: "Auditor",
    oversightRoleAr: "المدقق",
    autoApprovalThreshold: 75,
    dataSources: ["Audits", "Findings", "Evidence", "ActionPlans", "AuditTrail"],
    capabilities: ["Analyze Audit", "Identify Patterns", "Analyze Findings", "Suggest Improvements", "Track Remediation", "Generate Audit Report"],
    capabilitiesAr: ["تحليل التدقيق", "تحديد الأنماط", "تحليل النتائج", "اقتراح التحسينات", "تتبع المعالجة", "إنشاء تقرير التدقيق"],
    scenarios: [
      {
        id: "audit-findings",
        title: "Internal Audit Findings Analysis",
        titleAr: "تحليل نتائج التدقيق الداخلي",
        description: "Analyze audit findings and track remediation progress",
        descriptionAr: "تحليل نتائج التدقيق وتتبع تقدم المعالجة",
        steps: [
          { type: "analysis", content: "Loading Q4 2025 internal audit data (156 findings)...", contentAr: "تحميل بيانات التدقيق الداخلي للربع الرابع 2025 (156 نتيجة)...", status: "pending", confidence: 100 },
          { type: "analysis", content: "Categorizing findings by severity and department...", contentAr: "تصنيف النتائج حسب الخطورة والقسم...", status: "pending" },
          { type: "finding", content: "Pattern detected: 67% of findings relate to access control", contentAr: "تم كشف نمط: 67% من النتائج تتعلق بالتحكم في الوصول", status: "pending", confidence: 94, details: "Recurring issue across IT, Finance, and HR departments", detailsAr: "مشكلة متكررة في أقسام تقنية المعلومات والمالية والموارد البشرية" },
          { type: "finding", content: "23 overdue remediation items identified", contentAr: "تم تحديد 23 عنصر معالجة متأخر", status: "pending", confidence: 100 },
          { type: "recommendation", content: "Root cause analysis suggests training gap - recommend awareness program", contentAr: "يشير تحليل السبب الجذري إلى فجوة تدريبية - يوصى ببرنامج توعية", status: "pending", confidence: 86 },
          { type: "approval", content: "Audit report requires Auditor sign-off before distribution", contentAr: "يتطلب تقرير التدقيق توقيع المدقق قبل التوزيع", status: "pending" }
        ]
      }
    ]
  },
  {
    id: "policy",
    code: "POLICY_AGENT",
    name: "Policy Management Agent",
    nameAr: "وكيل إدارة السياسات",
    description: "Reviews policies, checks alignment with frameworks, and suggests improvements",
    descriptionAr: "يراجع السياسات، ويتحقق من التوافق مع الأطر، ويقترح التحسينات",
    icon: FileText,
    color: "text-indigo-500",
    bgColor: "bg-indigo-100 dark:bg-indigo-900/30",
    category: "analysis",
    oversightRole: "Policy Owner",
    oversightRoleAr: "مالك السياسة",
    autoApprovalThreshold: 80,
    dataSources: ["Policies", "Frameworks", "Standards", "Controls", "Regulations"],
    capabilities: ["Analyze Policy", "Check Alignment", "Suggest Improvements", "Identify Gaps", "Validate Compliance", "Generate Policy Report"],
    capabilitiesAr: ["تحليل السياسة", "فحص التوافق", "اقتراح التحسينات", "تحديد الثغرات", "التحقق من الامتثال", "إنشاء تقرير السياسة"],
    scenarios: [
      {
        id: "policy-review",
        title: "Information Security Policy Review",
        titleAr: "مراجعة سياسة أمن المعلومات",
        description: "Comprehensive policy review against PDPL and ISO 27001",
        descriptionAr: "مراجعة شاملة للسياسة مقابل PDPL وISO 27001",
        steps: [
          { type: "analysis", content: "Parsing Information Security Policy v2.3 (42 pages)...", contentAr: "تحليل سياسة أمن المعلومات الإصدار 2.3 (42 صفحة)...", status: "pending", confidence: 100 },
          { type: "analysis", content: "Cross-referencing with PDPL requirements (32 articles)...", contentAr: "المقارنة مع متطلبات PDPL (32 مادة)...", status: "pending" },
          { type: "finding", content: "Policy gap: Missing data subject rights section (PDPL Art. 4-8)", contentAr: "فجوة في السياسة: قسم حقوق أصحاب البيانات مفقود (PDPL المواد 4-8)", status: "pending", confidence: 97 },
          { type: "finding", content: "Outdated reference: ISO 27001:2013 should be updated to 2022", contentAr: "مرجع قديم: يجب تحديث ISO 27001:2013 إلى 2022", status: "pending", confidence: 100 },
          { type: "recommendation", content: "Generated draft amendments for 7 policy sections", contentAr: "تم إنشاء مسودة تعديلات لـ 7 أقسام من السياسة", status: "pending", confidence: 88 },
          { type: "approval", content: "Policy amendments require Policy Owner approval", contentAr: "تتطلب تعديلات السياسة موافقة مالك السياسة", status: "pending" }
        ]
      }
    ]
  },
  {
    id: "analytics",
    code: "ANALYTICS_AGENT",
    name: "Analytics & Insights Agent",
    nameAr: "وكيل التحليلات والرؤى",
    description: "Generates insights from GRC data, identifies trends, provides predictive analytics",
    descriptionAr: "يولد رؤى من بيانات GRC، ويحدد الاتجاهات، ويقدم تحليلات تنبؤية",
    icon: BarChart3,
    color: "text-cyan-500",
    bgColor: "bg-cyan-100 dark:bg-cyan-900/30",
    category: "analytics",
    oversightRole: "GRC Manager",
    oversightRoleAr: "مدير GRC",
    autoApprovalThreshold: 90,
    dataSources: ["AllModules", "Metrics", "KRIs", "KPIs", "HistoricalData"],
    capabilities: ["Generate Insights", "Identify Trends", "Predict Risks", "Analyze Metrics", "Create Dashboards", "Generate Analytics Report"],
    capabilitiesAr: ["إنشاء الرؤى", "تحديد الاتجاهات", "التنبؤ بالمخاطر", "تحليل المقاييس", "إنشاء لوحات المعلومات", "إنشاء تقرير التحليلات"],
    scenarios: [
      {
        id: "predictive-risk",
        title: "Predictive Risk Analysis",
        titleAr: "تحليل المخاطر التنبؤي",
        description: "AI-powered prediction of emerging compliance risks",
        descriptionAr: "تنبؤ مدعوم بالذكاء الاصطناعي للمخاطر الناشئة",
        steps: [
          { type: "analysis", content: "Analyzing 18 months of historical GRC data...", contentAr: "تحليل 18 شهراً من بيانات GRC التاريخية...", status: "pending", confidence: 96 },
          { type: "analysis", content: "Applying ML models to identify risk patterns...", contentAr: "تطبيق نماذج التعلم الآلي لتحديد أنماط المخاطر...", status: "pending", details: "Models: Time Series, Anomaly Detection, Clustering", detailsAr: "النماذج: السلاسل الزمنية، كشف الشذوذ، التجميع" },
          { type: "finding", content: "Trend detected: 34% increase in third-party risks Q1 2026", contentAr: "تم كشف اتجاه: زيادة 34% في مخاطر الطرف الثالث الربع الأول 2026", status: "pending", confidence: 82 },
          { type: "finding", content: "Prediction: 3 controls at risk of non-compliance within 60 days", contentAr: "تنبؤ: 3 ضوابط معرضة لخطر عدم الامتثال خلال 60 يوماً", status: "pending", confidence: 78, details: "AC-2, RA-5, CM-7 based on degradation patterns", detailsAr: "AC-2، RA-5، CM-7 بناءً على أنماط التدهور" },
          { type: "recommendation", content: "Proactive mitigation plan generated for identified risks", contentAr: "تم إنشاء خطة تخفيف استباقية للمخاطر المحددة", status: "pending", confidence: 85 },
          { type: "approval", content: "Predictive report ready for GRC Manager review", contentAr: "التقرير التنبؤي جاهز لمراجعة مدير GRC", status: "pending" }
        ]
      }
    ]
  },
  {
    id: "report",
    code: "REPORT_AGENT",
    name: "Report Generation Agent",
    nameAr: "وكيل إنشاء التقارير",
    description: "Generates comprehensive reports in natural language for various stakeholders",
    descriptionAr: "ينشئ تقارير شاملة باللغة الطبيعية لمختلف أصحاب المصلحة",
    icon: FileOutput,
    color: "text-purple-500",
    bgColor: "bg-purple-100 dark:bg-purple-900/30",
    category: "reporting",
    oversightRole: "Report Owner",
    oversightRoleAr: "مالك التقرير",
    autoApprovalThreshold: 85,
    dataSources: ["AllModules", "Templates", "UserPreferences", "BrandingAssets"],
    capabilities: ["Generate Report", "Summarize Data", "Create Executive Summary", "Format Document", "Localize Content", "Schedule Reports"],
    capabilitiesAr: ["إنشاء التقرير", "تلخيص البيانات", "إنشاء ملخص تنفيذي", "تنسيق المستند", "توطين المحتوى", "جدولة التقارير"],
    scenarios: [
      {
        id: "board-report",
        title: "Board-Level GRC Report Generation",
        titleAr: "إنشاء تقرير GRC على مستوى مجلس الإدارة",
        description: "Automated executive report with Arabic/English localization",
        descriptionAr: "تقرير تنفيذي آلي مع توطين عربي/إنجليزي",
        steps: [
          { type: "analysis", content: "Aggregating Q4 2025 GRC metrics from all modules...", contentAr: "تجميع مقاييس GRC للربع الرابع 2025 من جميع الوحدات...", status: "pending", confidence: 100 },
          { type: "analysis", content: "Generating executive summary (board-level language)...", contentAr: "إنشاء ملخص تنفيذي (لغة مستوى مجلس الإدارة)...", status: "pending" },
          { type: "finding", content: "Key highlight: Compliance score improved 12% YoY", contentAr: "أبرز النقاط: تحسنت درجة الامتثال بنسبة 12% على أساس سنوي", status: "pending", confidence: 100 },
          { type: "action", content: "Creating bilingual report (Arabic primary, English secondary)", contentAr: "إنشاء تقرير ثنائي اللغة (العربية أساسي، الإنجليزية ثانوي)", status: "pending" },
          { type: "recommendation", content: "Report ready: 24 pages, PDF + PowerPoint formats", contentAr: "التقرير جاهز: 24 صفحة، صيغ PDF + PowerPoint", status: "pending", confidence: 95 },
          { type: "approval", content: "Board report requires Report Owner approval before distribution", contentAr: "يتطلب تقرير مجلس الإدارة موافقة مالك التقرير قبل التوزيع", status: "pending" }
        ]
      }
    ]
  },
  {
    id: "diagnostic",
    code: "DIAGNOSTIC_AGENT",
    name: "System Diagnostic Agent",
    nameAr: "وكيل تشخيص النظام",
    description: "Monitors system health, analyzes errors, and provides diagnostic insights",
    descriptionAr: "يراقب صحة النظام، ويحلل الأخطاء، ويقدم رؤى تشخيصية",
    icon: Activity,
    color: "text-red-500",
    bgColor: "bg-red-100 dark:bg-red-900/30",
    category: "monitoring",
    oversightRole: "Platform Admin",
    oversightRoleAr: "مسؤول المنصة",
    autoApprovalThreshold: 80,
    dataSources: ["SystemLogs", "ErrorLogs", "PerformanceMetrics", "Alerts"],
    capabilities: ["Monitor Health", "Analyze Errors", "Diagnose Issues", "Suggest Fixes", "Track Performance", "Generate Health Report"],
    capabilitiesAr: ["مراقبة الصحة", "تحليل الأخطاء", "تشخيص المشاكل", "اقتراح الإصلاحات", "تتبع الأداء", "إنشاء تقرير الصحة"],
    scenarios: [
      {
        id: "system-health",
        title: "Real-time System Health Check",
        titleAr: "فحص صحة النظام في الوقت الفعلي",
        description: "Continuous monitoring with anomaly detection",
        descriptionAr: "مراقبة مستمرة مع كشف الشذوذ",
        steps: [
          { type: "analysis", content: "Scanning 47 system components and 12 integrations...", contentAr: "فحص 47 مكون نظام و12 تكامل...", status: "pending", confidence: 100 },
          { type: "finding", content: "Health Score: 94/100 - System Healthy", contentAr: "درجة الصحة: 94/100 - النظام صحي", status: "pending", confidence: 98, details: "API: 99.7% uptime, DB: 98.9% uptime, Cache: 100%", detailsAr: "API: وقت تشغيل 99.7%، قاعدة البيانات: وقت تشغيل 98.9%، الذاكرة المؤقتة: 100%" },
          { type: "finding", content: "Warning: Database query latency increased 15% (last 2 hours)", contentAr: "تحذير: زادت كمون استعلام قاعدة البيانات بنسبة 15% (آخر ساعتين)", status: "pending", confidence: 92 },
          { type: "recommendation", content: "Suggest: Index optimization for compliance_assessments table", contentAr: "اقتراح: تحسين الفهرس لجدول compliance_assessments", status: "pending", confidence: 87 },
          { type: "action", content: "Auto-scheduled: Performance optimization task created", contentAr: "مجدول تلقائياً: تم إنشاء مهمة تحسين الأداء", status: "pending" },
          { type: "approval", content: "Database changes require Platform Admin approval", contentAr: "تتطلب تغييرات قاعدة البيانات موافقة مسؤول المنصة", status: "pending" }
        ]
      }
    ]
  },
  {
    id: "support",
    code: "SUPPORT_AGENT",
    name: "Customer Support Agent",
    nameAr: "وكيل دعم العملاء",
    description: "Assists users during onboarding, answers questions, and provides guidance",
    descriptionAr: "يساعد المستخدمين أثناء الإعداد، ويجيب على الأسئلة، ويقدم التوجيه",
    icon: HeadphonesIcon,
    color: "text-pink-500",
    bgColor: "bg-pink-100 dark:bg-pink-900/30",
    category: "support",
    oversightRole: "Support Manager",
    oversightRoleAr: "مدير الدعم",
    autoApprovalThreshold: 90,
    dataSources: ["KnowledgeBase", "FAQs", "UserContext", "OnboardingStatus", "Tickets"],
    capabilities: ["Answer Questions", "Guide Onboarding", "Resolve Issues", "Escalate to Human", "Track Conversation", "Provide Knowledge"],
    capabilitiesAr: ["الإجابة على الأسئلة", "توجيه الإعداد", "حل المشاكل", "التصعيد للبشر", "تتبع المحادثة", "تقديم المعرفة"],
    scenarios: [
      {
        id: "onboarding-help",
        title: "New User Onboarding Assistance",
        titleAr: "مساعدة تهيئة المستخدم الجديد",
        description: "Guided onboarding with personalized recommendations",
        descriptionAr: "تهيئة موجهة مع توصيات مخصصة",
        steps: [
          { type: "analysis", content: "Welcome! Analyzing your role and organization profile...", contentAr: "مرحباً! تحليل دورك وملف منظمتك...", status: "pending", confidence: 100 },
          { type: "finding", content: "Detected: Compliance Officer role, Financial sector, 500+ employees", contentAr: "تم الكشف: دور مسؤول الامتثال، القطاع المالي، أكثر من 500 موظف", status: "pending", confidence: 95 },
          { type: "recommendation", content: "Recommended frameworks: SAMA CSF, NCA-ECC, PDPL", contentAr: "الأطر الموصى بها: SAMA CSF، NCA-ECC، PDPL", status: "pending", confidence: 92 },
          { type: "action", content: "Created personalized onboarding checklist (12 steps)", contentAr: "تم إنشاء قائمة تهيئة مخصصة (12 خطوة)", status: "pending" },
          { type: "action", content: "Scheduled: Demo session with GRC specialist", contentAr: "مجدول: جلسة عرض مع متخصص GRC", status: "pending" },
          { type: "approval", content: "Continue with guided setup or request human support?", contentAr: "المتابعة مع الإعداد الموجه أو طلب دعم بشري؟", status: "pending" }
        ]
      }
    ]
  },
  {
    id: "workflow",
    code: "WORKFLOW_AGENT",
    name: "Workflow Optimization Agent",
    nameAr: "وكيل تحسين سير العمل",
    description: "Optimizes workflow processes, identifies bottlenecks, and suggests improvements",
    descriptionAr: "يحسن عمليات سير العمل، ويحدد الاختناقات، ويقترح التحسينات",
    icon: GitBranch,
    color: "text-teal-500",
    bgColor: "bg-teal-100 dark:bg-teal-900/30",
    category: "automation",
    oversightRole: "Workflow Admin",
    oversightRoleAr: "مسؤول سير العمل",
    autoApprovalThreshold: 85,
    dataSources: ["Workflows", "Tasks", "SLAs", "ProcessMetrics", "UserWorkload"],
    capabilities: ["Optimize Workflow", "Identify Bottlenecks", "Suggest Improvements", "Automate Routing", "Manage Deadlines", "Escalate Overdue"],
    capabilitiesAr: ["تحسين سير العمل", "تحديد الاختناقات", "اقتراح التحسينات", "أتمتة التوجيه", "إدارة المواعيد النهائية", "تصعيد المتأخرات"],
    scenarios: [
      {
        id: "workflow-optimization",
        title: "Control Assessment Workflow Optimization",
        titleAr: "تحسين سير عمل تقييم الضوابط",
        description: "Identify and resolve workflow bottlenecks",
        descriptionAr: "تحديد وحل اختناقات سير العمل",
        steps: [
          { type: "analysis", content: "Analyzing control assessment workflow (last 90 days)...", contentAr: "تحليل سير عمل تقييم الضوابط (آخر 90 يوماً)...", status: "pending", confidence: 100 },
          { type: "finding", content: "Bottleneck detected: Evidence review stage (avg. 4.2 days)", contentAr: "تم كشف اختناق: مرحلة مراجعة الأدلة (متوسط 4.2 يوم)", status: "pending", confidence: 94, details: "3x longer than other stages", detailsAr: "أطول 3 مرات من المراحل الأخرى" },
          { type: "finding", content: "23 tasks overdue, 67% assigned to 2 users", contentAr: "23 مهمة متأخرة، 67% مخصصة لمستخدمين اثنين", status: "pending", confidence: 100 },
          { type: "recommendation", content: "Suggest: Load balancing - redistribute 15 tasks to available users", contentAr: "اقتراح: موازنة الحمل - إعادة توزيع 15 مهمة للمستخدمين المتاحين", status: "pending", confidence: 88 },
          { type: "action", content: "Auto-escalation rules created for SLA breaches", contentAr: "تم إنشاء قواعد تصعيد تلقائي لانتهاكات SLA", status: "pending" },
          { type: "approval", content: "Task redistribution requires Workflow Admin approval", contentAr: "تتطلب إعادة توزيع المهام موافقة مسؤول سير العمل", status: "pending" }
        ]
      }
    ]
  },
  {
    id: "evidence",
    code: "EVIDENCE_AGENT",
    name: "Evidence Collection Agent",
    nameAr: "وكيل جمع الأدلة",
    description: "Collects evidence from integrated systems, validates completeness, organizes packs",
    descriptionAr: "يجمع الأدلة من الأنظمة المتكاملة، ويتحقق من الاكتمال، وينظم الحزم",
    icon: FolderCheck,
    color: "text-amber-500",
    bgColor: "bg-amber-100 dark:bg-amber-900/30",
    category: "collection",
    oversightRole: "Evidence Owner",
    oversightRoleAr: "مالك الأدلة",
    autoApprovalThreshold: 90,
    dataSources: ["Integrations", "ERP", "IAM", "SIEM", "ITSM", "CloudSystems"],
    capabilities: ["Collect Evidence", "Validate Evidence", "Organize Evidence", "Link to Controls", "Track Expiry", "Generate Evidence Pack"],
    capabilitiesAr: ["جمع الأدلة", "التحقق من الأدلة", "تنظيم الأدلة", "الربط بالضوابط", "تتبع انتهاء الصلاحية", "إنشاء حزمة الأدلة"],
    scenarios: [
      {
        id: "auto-evidence",
        title: "Automated Evidence Collection",
        titleAr: "جمع الأدلة التلقائي",
        description: "Collect evidence from SAP, Azure AD, and ServiceNow",
        descriptionAr: "جمع الأدلة من SAP وAzure AD وServiceNow",
        steps: [
          { type: "analysis", content: "Connecting to 3 integrated systems...", contentAr: "الاتصال بـ 3 أنظمة متكاملة...", status: "pending", confidence: 100, details: "SAP S/4HANA, Azure AD, ServiceNow ITSM", detailsAr: "SAP S/4HANA، Azure AD، ServiceNow ITSM" },
          { type: "action", content: "Collecting user access reports from Azure AD...", contentAr: "جمع تقارير وصول المستخدمين من Azure AD...", status: "pending" },
          { type: "action", content: "Extracting change management tickets from ServiceNow...", contentAr: "استخراج تذاكر إدارة التغيير من ServiceNow...", status: "pending" },
          { type: "finding", content: "Collected 47 evidence artifacts, 3 require manual upload", contentAr: "تم جمع 47 قطعة أثبات، 3 تتطلب رفع يدوي", status: "pending", confidence: 96 },
          { type: "recommendation", content: "Evidence pack 94% complete, linked to 12 controls", contentAr: "حزمة الأدلة مكتملة بنسبة 94%، مرتبطة بـ 12 ضابط", status: "pending", confidence: 94 },
          { type: "approval", content: "Evidence pack requires Evidence Owner approval", contentAr: "تتطلب حزمة الأدلة موافقة مالك الأدلة", status: "pending" }
        ]
      }
    ]
  },
  {
    id: "email",
    code: "EMAIL_AGENT",
    name: "Email Classification Agent",
    nameAr: "وكيل تصنيف البريد الإلكتروني",
    description: "Classifies incoming emails, routes to appropriate teams, and drafts responses",
    descriptionAr: "يصنف رسائل البريد الواردة، ويوجهها للفرق المناسبة، ويصوغ الردود",
    icon: Mail,
    color: "text-sky-500",
    bgColor: "bg-sky-100 dark:bg-sky-900/30",
    category: "communication",
    oversightRole: "Email Admin",
    oversightRoleAr: "مسؤول البريد",
    autoApprovalThreshold: 85,
    dataSources: ["Mailboxes", "Templates", "RoutingRules", "PriorityMatrix"],
    capabilities: ["Classify Email", "Route Email", "Draft Response", "Extract Data", "Prioritize Email", "Track SLA"],
    capabilitiesAr: ["تصنيف البريد", "توجيه البريد", "صياغة الرد", "استخراج البيانات", "تحديد الأولوية", "تتبع SLA"],
    scenarios: [
      {
        id: "email-triage",
        title: "Regulatory Email Triage",
        titleAr: "فرز البريد التنظيمي",
        description: "Auto-classify and route regulatory correspondence",
        descriptionAr: "تصنيف وتوجيه المراسلات التنظيمية تلقائياً",
        steps: [
          { type: "analysis", content: "New email received from sama.gov.sa...", contentAr: "تم استلام بريد جديد من sama.gov.sa...", status: "pending", confidence: 100 },
          { type: "finding", content: "Classification: Official Regulatory Request (Priority: High)", contentAr: "التصنيف: طلب تنظيمي رسمي (الأولوية: عالية)", status: "pending", confidence: 97 },
          { type: "finding", content: "Extracted: Response deadline Jan 28, 2026 (8 days)", contentAr: "تم الاستخراج: موعد الرد 28 يناير 2026 (8 أيام)", status: "pending", confidence: 95 },
          { type: "action", content: "Routed to: Compliance Team + Legal Team", contentAr: "تم التوجيه إلى: فريق الامتثال + الفريق القانوني", status: "pending" },
          { type: "recommendation", content: "Draft response prepared based on similar past responses", contentAr: "تم إعداد مسودة رد بناءً على ردود مماثلة سابقة", status: "pending", confidence: 82 },
          { type: "approval", content: "Response draft requires review before sending", contentAr: "تتطلب مسودة الرد مراجعة قبل الإرسال", status: "pending" }
        ]
      }
    ]
  }
]

// Predefined chat conversations for each agent
const agentChats: Record<string, ChatMessage[]> = {
  shahin: [
    { role: "user", content: "What's our current compliance status?", contentAr: "ما هو وضع الامتثال الحالي لدينا؟", timestamp: new Date() },
    { role: "agent", content: "Based on the latest assessment, your overall compliance score is 87%. NCA-ECC: 92%, SAMA CSF: 84%, PDPL: 85%. I've identified 3 priority items that need attention.", contentAr: "بناءً على أحدث تقييم، درجة الامتثال الإجمالية هي 87%. NCA-ECC: 92%، SAMA CSF: 84%، PDPL: 85%. لقد حددت 3 عناصر ذات أولوية تحتاج إلى اهتمام.", timestamp: new Date() },
    { role: "user", content: "Can you delegate tasks to address those gaps?", contentAr: "هل يمكنك تفويض مهام لمعالجة تلك الفجوات؟", timestamp: new Date() },
    { role: "agent", content: "I'll coordinate with the Compliance Agent and Evidence Agent. Tasks have been created and assigned. Estimated completion: 5 business days. Would you like me to schedule a progress review?", contentAr: "سأنسق مع وكيل الامتثال ووكيل الأدلة. تم إنشاء المهام وتعيينها. الإكمال المتوقع: 5 أيام عمل. هل تريد مني جدولة مراجعة التقدم؟", timestamp: new Date() }
  ],
  compliance: [
    { role: "user", content: "Run a gap analysis against NCA-ECC", contentAr: "قم بإجراء تحليل فجوات مقابل NCA-ECC", timestamp: new Date() },
    { role: "agent", content: "Starting NCA-ECC gap analysis across 114 controls. Current implementation: 94 fully implemented, 12 partially implemented, 8 not implemented. Critical gaps found in: Network Security (2), Access Control (3), Incident Response (3).", contentAr: "بدء تحليل فجوات NCA-ECC عبر 114 ضابط. التنفيذ الحالي: 94 منفذ بالكامل، 12 منفذ جزئياً، 8 غير منفذ. الفجوات الحرجة في: أمن الشبكات (2)، التحكم في الوصول (3)، الاستجابة للحوادث (3).", timestamp: new Date() },
    { role: "user", content: "Generate a remediation plan", contentAr: "قم بإنشاء خطة معالجة", timestamp: new Date() },
    { role: "agent", content: "Remediation plan generated with 45-day timeline. Priority 1: Access Control gaps (Week 1-2). Priority 2: Network Security (Week 2-3). Priority 3: Incident Response (Week 3-4). Plan requires Compliance Officer approval before execution.", contentAr: "تم إنشاء خطة معالجة بجدول زمني 45 يوماً. الأولوية 1: فجوات التحكم في الوصول (الأسبوع 1-2). الأولوية 2: أمن الشبكات (الأسبوع 2-3). الأولوية 3: الاستجابة للحوادث (الأسبوع 3-4). تتطلب الخطة موافقة مسؤول الامتثال قبل التنفيذ.", timestamp: new Date() }
  ],
  risk: [
    { role: "user", content: "Assess risk for our new cloud vendor", contentAr: "قيّم المخاطر للمورد السحابي الجديد", timestamp: new Date() },
    { role: "agent", content: "Initiating third-party risk assessment for CloudTech Solutions. Analyzing 47 risk factors across security, financial, operational, legal, reputational, and strategic categories.", contentAr: "بدء تقييم مخاطر الطرف الثالث لـ CloudTech Solutions. تحليل 47 عامل خطر عبر الفئات الأمنية والمالية والتشغيلية والقانونية والسمعة والاستراتيجية.", timestamp: new Date() },
    { role: "user", content: "What's the risk score?", contentAr: "ما هي درجة المخاطر؟", timestamp: new Date() },
    { role: "agent", content: "Risk Score: 72/100 (Medium-High). Key concerns: 1) Data handling practices - unclear data residency, 2) No SOC 2 Type II certification, 3) Limited incident response SLAs. Recommendation: Proceed with enhanced monitoring and contractual safeguards. Risk acceptance requires Risk Manager approval.", contentAr: "درجة المخاطر: 72/100 (متوسطة-عالية). المخاوف الرئيسية: 1) ممارسات معالجة البيانات - موقع البيانات غير واضح، 2) لا يوجد شهادة SOC 2 Type II، 3) اتفاقيات SLA محدودة للاستجابة للحوادث. التوصية: المتابعة مع مراقبة معززة وضمانات تعاقدية. يتطلب قبول المخاطر موافقة مدير المخاطر.", timestamp: new Date() }
  ]
}

export function AIAgentsShowcase() {
  const [activeView, setActiveView] = useState<"grid" | "scenario" | "chat">("grid")
  const [selectedAgent, setSelectedAgent] = useState<AIAgent | null>(null)
  const [activeScenario, setActiveScenario] = useState<Scenario | null>(null)
  const [scenarioStep, setScenarioStep] = useState(0)
  const [isRunning, setIsRunning] = useState(false)
  const [chatMessages, setChatMessages] = useState<ChatMessage[]>([])
  const [userInput, setUserInput] = useState("")
  const [isTyping, setIsTyping] = useState(false)
  const { locale } = useLocale()
  const isArabic = locale === "ar"

  // Run scenario simulation
  const runScenario = useCallback(() => {
    if (!activeScenario || isRunning) return
    setIsRunning(true)
    setScenarioStep(0)
  }, [activeScenario, isRunning])

  // Advance scenario steps
  useEffect(() => {
    if (!isRunning || !activeScenario) return

    if (scenarioStep < activeScenario.steps.length) {
      const timer = setTimeout(() => {
        setScenarioStep(prev => prev + 1)
      }, 1500 + Math.random() * 1000)
      return () => clearTimeout(timer)
    } else {
      setIsRunning(false)
    }
  }, [isRunning, scenarioStep, activeScenario])

  // Handle agent selection
  const handleAgentSelect = (agent: AIAgent) => {
    setSelectedAgent(agent)
    setActiveScenario(agent.scenarios[0])
    setScenarioStep(0)
    setIsRunning(false)
    setChatMessages(agentChats[agent.id] || [])
    setActiveView("scenario")
  }

  // Handle chat submit
  const handleChatSubmit = () => {
    if (!userInput.trim() || !selectedAgent) return

    const newMessage: ChatMessage = {
      role: "user",
      content: userInput,
      contentAr: userInput,
      timestamp: new Date()
    }

    setChatMessages(prev => [...prev, newMessage])
    setUserInput("")
    setIsTyping(true)

    // Simulate agent response
    setTimeout(() => {
      const responses = [
        { en: "I understand your request. Let me analyze the data and provide recommendations. This will require human approval before any action is taken.", ar: "أفهم طلبك. دعني أحلل البيانات وأقدم التوصيات. سيتطلب هذا موافقة بشرية قبل اتخاذ أي إجراء." },
        { en: "Based on my analysis, I've identified 3 key areas that need attention. I'll prepare a detailed report for your review.", ar: "بناءً على تحليلي، حددت 3 مجالات رئيسية تحتاج إلى اهتمام. سأعد تقريراً مفصلاً لمراجعتك." },
        { en: "I've processed your request and created the necessary tasks. These have been assigned to the appropriate team members with deadlines.", ar: "لقد عالجت طلبك وأنشأت المهام اللازمة. تم تعيينها لأعضاء الفريق المناسبين مع المواعيد النهائية." }
      ]
      const response = responses[Math.floor(Math.random() * responses.length)]

      setChatMessages(prev => [...prev, {
        role: "agent",
        content: response.en,
        contentAr: response.ar,
        timestamp: new Date()
      }])
      setIsTyping(false)
    }, 1500 + Math.random() * 1000)
  }

  const getStepIcon = (type: string) => {
    switch (type) {
      case "analysis": return <BarChart3 className="w-4 h-4" />
      case "finding": return <AlertCircle className="w-4 h-4" />
      case "recommendation": return <TrendingUp className="w-4 h-4" />
      case "action": return <Zap className="w-4 h-4" />
      case "approval": return <Shield className="w-4 h-4" />
      default: return <CheckCircle className="w-4 h-4" />
    }
  }

  const getStepColor = (type: string) => {
    switch (type) {
      case "analysis": return "text-blue-500 bg-blue-100 dark:bg-blue-900/30"
      case "finding": return "text-amber-500 bg-amber-100 dark:bg-amber-900/30"
      case "recommendation": return "text-emerald-500 bg-emerald-100 dark:bg-emerald-900/30"
      case "action": return "text-violet-500 bg-violet-100 dark:bg-violet-900/30"
      case "approval": return "text-pink-500 bg-pink-100 dark:bg-pink-900/30"
      default: return "text-gray-500 bg-gray-100 dark:bg-gray-900/30"
    }
  }

  return (
    <section className="py-24 bg-gradient-to-b from-gray-50 to-white dark:from-gray-900 dark:to-gray-950">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-12"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-violet-100 dark:bg-violet-900/30 text-violet-700 dark:text-violet-300 text-sm font-medium mb-6">
            <Bot className="w-4 h-4" />
            {isArabic ? "12 وكيل ذكاء اصطناعي تفاعلي" : "12 Interactive AI Agents"}
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
            {isArabic ? "شاهد الذكاء الاصطناعي في العمل" : "See AI in Action"}
          </h2>
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-3xl mx-auto">
            {isArabic
              ? "استكشف سيناريوهات حقيقية وتفاعل مع الوكلاء - كل إجراء يتطلب موافقة بشرية"
              : "Explore real scenarios and interact with agents - every action requires human approval"}
          </p>
        </motion.div>

        {/* Main Content */}
        <div className="grid lg:grid-cols-12 gap-8">
          {/* Layer 1: Agent Selection (Left Sidebar) */}
          <div className="lg:col-span-3">
            <div className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 overflow-hidden sticky top-24">
              <div className="p-4 border-b border-gray-200 dark:border-gray-700">
                <h3 className="font-semibold text-gray-900 dark:text-white flex items-center gap-2">
                  <Users className="w-5 h-5 text-violet-500" />
                  {isArabic ? "فريق الذكاء الاصطناعي" : "AI Team"}
                </h3>
              </div>
              <div className="max-h-[600px] overflow-y-auto">
                {agents.map((agent) => (
                  <button
                    key={agent.id}
                    onClick={() => handleAgentSelect(agent)}
                    className={`w-full p-4 flex items-center gap-3 transition-all text-left hover:bg-gray-50 dark:hover:bg-gray-700/50 border-b border-gray-100 dark:border-gray-700/50 last:border-0 ${
                      selectedAgent?.id === agent.id ? "bg-violet-50 dark:bg-violet-900/20" : ""
                    }`}
                  >
                    <div className={`w-10 h-10 rounded-lg ${agent.bgColor} flex items-center justify-center flex-shrink-0`}>
                      <agent.icon className={`w-5 h-5 ${agent.color}`} />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="font-medium text-sm text-gray-900 dark:text-white truncate">
                        {isArabic ? agent.nameAr : agent.name}
                      </p>
                      <p className="text-xs text-gray-500 dark:text-gray-400 truncate">
                        {isArabic ? agent.oversightRoleAr : agent.oversightRole}
                      </p>
                    </div>
                    <ChevronRight className={`w-4 h-4 text-gray-400 transition-transform ${selectedAgent?.id === agent.id ? "rotate-90" : ""}`} />
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* Layer 2 & 3: Scenario + Chat (Right Side) */}
          <div className="lg:col-span-9">
            {selectedAgent ? (
              <div className="space-y-6">
                {/* Agent Header */}
                <div className={`${selectedAgent.bgColor} rounded-2xl p-6`}>
                  <div className="flex items-start justify-between">
                    <div className="flex items-center gap-4">
                      <div className="w-16 h-16 rounded-xl bg-white/90 dark:bg-gray-900/50 flex items-center justify-center">
                        <selectedAgent.icon className={`w-8 h-8 ${selectedAgent.color}`} />
                      </div>
                      <div>
                        <h3 className="text-xl font-bold text-gray-900 dark:text-white">
                          {isArabic ? selectedAgent.nameAr : selectedAgent.name}
                        </h3>
                        <p className="text-sm text-gray-600 dark:text-gray-400">
                          {isArabic ? selectedAgent.descriptionAr : selectedAgent.description}
                        </p>
                        <div className="flex items-center gap-4 mt-2">
                          <span className="inline-flex items-center gap-1 text-xs text-gray-600 dark:text-gray-400">
                            <Shield className="w-3 h-3" />
                            {isArabic ? selectedAgent.oversightRoleAr : selectedAgent.oversightRole}
                          </span>
                          <span className="inline-flex items-center gap-1 text-xs text-emerald-600">
                            <Check className="w-3 h-3" />
                            {selectedAgent.autoApprovalThreshold}% {isArabic ? "عتبة الموافقة" : "Confidence Threshold"}
                          </span>
                        </div>
                      </div>
                    </div>
                    <div className="flex gap-2">
                      <button
                        onClick={() => setActiveView("scenario")}
                        className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${
                          activeView === "scenario"
                            ? "bg-white dark:bg-gray-800 text-gray-900 dark:text-white shadow-sm"
                            : "text-gray-600 dark:text-gray-400 hover:bg-white/50 dark:hover:bg-gray-800/50"
                        }`}
                      >
                        <Play className="w-4 h-4 inline mr-1" />
                        {isArabic ? "السيناريو" : "Scenario"}
                      </button>
                      <button
                        onClick={() => setActiveView("chat")}
                        className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${
                          activeView === "chat"
                            ? "bg-white dark:bg-gray-800 text-gray-900 dark:text-white shadow-sm"
                            : "text-gray-600 dark:text-gray-400 hover:bg-white/50 dark:hover:bg-gray-800/50"
                        }`}
                      >
                        <MessageSquare className="w-4 h-4 inline mr-1" />
                        {isArabic ? "المحادثة" : "Chat"}
                      </button>
                    </div>
                  </div>
                </div>

                {/* Layer 2: Live Scenario Demo */}
                {activeView === "scenario" && activeScenario && (
                  <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 overflow-hidden"
                  >
                    <div className="p-4 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between">
                      <div>
                        <h4 className="font-semibold text-gray-900 dark:text-white">
                          {isArabic ? activeScenario.titleAr : activeScenario.title}
                        </h4>
                        <p className="text-sm text-gray-500 dark:text-gray-400">
                          {isArabic ? activeScenario.descriptionAr : activeScenario.description}
                        </p>
                      </div>
                      <Button
                        onClick={runScenario}
                        disabled={isRunning}
                        className="bg-violet-600 hover:bg-violet-700 text-white"
                      >
                        {isRunning ? (
                          <>
                            <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                            {isArabic ? "جاري التشغيل..." : "Running..."}
                          </>
                        ) : (
                          <>
                            <Play className="w-4 h-4 mr-2" />
                            {isArabic ? "تشغيل السيناريو" : "Run Scenario"}
                          </>
                        )}
                      </Button>
                    </div>
                    <div className="p-6 space-y-4">
                      {activeScenario.steps.map((step, index) => {
                        const isActive = index < scenarioStep
                        const isCurrent = index === scenarioStep - 1

                        return (
                          <motion.div
                            key={index}
                            initial={{ opacity: 0.5 }}
                            animate={{ opacity: isActive ? 1 : 0.5 }}
                            className={`flex gap-4 p-4 rounded-xl transition-all ${
                              isActive
                                ? "bg-gray-50 dark:bg-gray-700/50"
                                : "bg-gray-50/50 dark:bg-gray-800/50"
                            } ${isCurrent ? "ring-2 ring-violet-500" : ""}`}
                          >
                            <div className={`w-10 h-10 rounded-lg flex items-center justify-center flex-shrink-0 ${getStepColor(step.type)}`}>
                              {isActive && isCurrent ? (
                                <Loader2 className="w-5 h-5 animate-spin" />
                              ) : isActive ? (
                                <Check className="w-5 h-5" />
                              ) : (
                                getStepIcon(step.type)
                              )}
                            </div>
                            <div className="flex-1">
                              <div className="flex items-center justify-between">
                                <p className={`font-medium ${isActive ? "text-gray-900 dark:text-white" : "text-gray-500 dark:text-gray-400"}`}>
                                  {isArabic ? step.contentAr : step.content}
                                </p>
                                {step.confidence && isActive && (
                                  <span className="text-xs px-2 py-1 rounded-full bg-emerald-100 dark:bg-emerald-900/30 text-emerald-600">
                                    {step.confidence}% {isArabic ? "ثقة" : "confidence"}
                                  </span>
                                )}
                              </div>
                              {step.details && isActive && (
                                <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                                  {isArabic ? step.detailsAr : step.details}
                                </p>
                              )}
                              {step.type === "approval" && isActive && (
                                <div className="mt-3 flex gap-2">
                                  <Button size="sm" className="bg-emerald-600 hover:bg-emerald-700 text-white">
                                    <Check className="w-4 h-4 mr-1" />
                                    {isArabic ? "موافقة" : "Approve"}
                                  </Button>
                                  <Button size="sm" variant="outline">
                                    <Eye className="w-4 h-4 mr-1" />
                                    {isArabic ? "مراجعة" : "Review"}
                                  </Button>
                                </div>
                              )}
                            </div>
                          </motion.div>
                        )
                      })}
                    </div>
                  </motion.div>
                )}

                {/* Layer 3: Interactive Chat */}
                {activeView === "chat" && (
                  <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 overflow-hidden"
                  >
                    <div className="p-4 border-b border-gray-200 dark:border-gray-700">
                      <h4 className="font-semibold text-gray-900 dark:text-white flex items-center gap-2">
                        <MessageSquare className="w-5 h-5 text-violet-500" />
                        {isArabic ? "محادثة مع" : "Chat with"} {isArabic ? selectedAgent.nameAr : selectedAgent.name}
                      </h4>
                    </div>
                    <div className="h-[400px] overflow-y-auto p-4 space-y-4">
                      {chatMessages.map((msg, index) => (
                        <div
                          key={index}
                          className={`flex ${msg.role === "user" ? "justify-end" : "justify-start"}`}
                        >
                          <div className={`max-w-[80%] p-4 rounded-2xl ${
                            msg.role === "user"
                              ? "bg-violet-600 text-white rounded-br-none"
                              : "bg-gray-100 dark:bg-gray-700 text-gray-900 dark:text-white rounded-bl-none"
                          }`}>
                            <p className="text-sm">
                              {isArabic ? msg.contentAr : msg.content}
                            </p>
                          </div>
                        </div>
                      ))}
                      {isTyping && (
                        <div className="flex justify-start">
                          <div className="bg-gray-100 dark:bg-gray-700 p-4 rounded-2xl rounded-bl-none">
                            <div className="flex gap-1">
                              <span className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: "0ms" }} />
                              <span className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: "150ms" }} />
                              <span className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: "300ms" }} />
                            </div>
                          </div>
                        </div>
                      )}
                    </div>
                    <div className="p-4 border-t border-gray-200 dark:border-gray-700">
                      <div className="flex gap-2">
                        <input
                          type="text"
                          value={userInput}
                          onChange={(e) => setUserInput(e.target.value)}
                          onKeyPress={(e) => e.key === "Enter" && handleChatSubmit()}
                          placeholder={isArabic ? "اكتب رسالتك..." : "Type your message..."}
                          className="flex-1 px-4 py-3 rounded-xl border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-violet-500"
                        />
                        <Button
                          onClick={handleChatSubmit}
                          className="bg-violet-600 hover:bg-violet-700 text-white px-6"
                        >
                          <Send className="w-5 h-5" />
                        </Button>
                      </div>
                      <p className="text-xs text-gray-500 dark:text-gray-400 mt-2 text-center">
                        {isArabic
                          ? "هذا عرض توضيحي. في التطبيق الفعلي، يتطلب كل إجراء موافقة بشرية."
                          : "This is a demo. In the actual app, every action requires human approval."}
                      </p>
                    </div>
                  </motion.div>
                )}

                {/* Capabilities Grid */}
                <div className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 p-6">
                  <h4 className="font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                    <Zap className="w-5 h-5 text-violet-500" />
                    {isArabic ? "القدرات" : "Capabilities"}
                  </h4>
                  <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                    {(isArabic ? selectedAgent.capabilitiesAr : selectedAgent.capabilities).map((cap, i) => (
                      <div key={i} className="flex items-center gap-2 p-3 rounded-lg bg-gray-50 dark:bg-gray-700/50">
                        <CheckCircle className="w-4 h-4 text-emerald-500 flex-shrink-0" />
                        <span className="text-sm text-gray-700 dark:text-gray-300">{cap}</span>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Data Sources */}
                <div className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 p-6">
                  <h4 className="font-semibold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
                    <FolderCheck className="w-5 h-5 text-violet-500" />
                    {isArabic ? "مصادر البيانات" : "Data Sources"}
                  </h4>
                  <div className="flex flex-wrap gap-2">
                    {selectedAgent.dataSources.map((source, i) => (
                      <span key={i} className="px-3 py-1.5 rounded-lg bg-gray-100 dark:bg-gray-700 text-sm text-gray-700 dark:text-gray-300">
                        {source}
                      </span>
                    ))}
                  </div>
                </div>
              </div>
            ) : (
              /* Default State - No Agent Selected */
              <div className="bg-white dark:bg-gray-800 rounded-2xl border border-gray-200 dark:border-gray-700 p-12 text-center">
                <div className="w-20 h-20 rounded-full bg-violet-100 dark:bg-violet-900/30 flex items-center justify-center mx-auto mb-6">
                  <Bot className="w-10 h-10 text-violet-500" />
                </div>
                <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                  {isArabic ? "اختر وكيلاً للبدء" : "Select an Agent to Begin"}
                </h3>
                <p className="text-gray-600 dark:text-gray-400 max-w-md mx-auto">
                  {isArabic
                    ? "اختر أي وكيل من القائمة لمشاهدة السيناريوهات الحية والتفاعل عبر المحادثة"
                    : "Choose any agent from the list to see live scenarios and interact via chat"}
                </p>
              </div>
            )}
          </div>
        </div>

        {/* Stats Bar */}
        <motion.div
          className="mt-12 grid grid-cols-2 md:grid-cols-4 gap-6"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          {[
            { icon: Bot, value: "12", label: isArabic ? "وكيل ذكاء اصطناعي" : "AI Agents" },
            { icon: Zap, value: "72", label: isArabic ? "قدرة آلية" : "Capabilities" },
            { icon: Shield, value: "100%", label: isArabic ? "رقابة بشرية" : "Human Oversight" },
            { icon: Sparkles, value: "24/7", label: isArabic ? "متاح دائماً" : "Always Available" }
          ].map((stat, index) => (
            <div key={index} className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-4 text-center">
              <stat.icon className="w-6 h-6 text-violet-500 mx-auto mb-2" />
              <div className="text-2xl font-bold text-gray-900 dark:text-white">{stat.value}</div>
              <div className="text-sm text-gray-600 dark:text-gray-400">{stat.label}</div>
            </div>
          ))}
        </motion.div>

        {/* CTA */}
        <motion.div
          className="text-center mt-12"
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
        >
          <Link href="/trial">
            <Button className="bg-violet-600 hover:bg-violet-700 text-white px-8 py-6 text-lg font-semibold rounded-xl">
              <Bot className="w-5 h-5 mr-2" />
              {isArabic ? "جرب فريق الذكاء الاصطناعي" : "Try the AI Team"}
            </Button>
          </Link>
        </motion.div>
      </div>
    </section>
  )
}
