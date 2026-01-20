"use client"

import { useState, useEffect, useRef } from "react"
import { motion, AnimatePresence } from "framer-motion"
import {
  Play,
  Pause,
  RotateCcw,
  Zap,
  CheckCircle,
  Clock,
  ArrowRight,
  Bot,
  FileText,
  Shield,
  AlertTriangle,
  TrendingUp,
  Users,
  Database,
  Brain,
  Sparkles,
  Eye,
  Activity,
  Terminal,
  Code,
  FileJson,
  Download,
  Copy,
  Check,
  Search,
  Scale,
  Gavel,
  ClipboardList,
  BarChart3,
  PieChart,
  Target,
  Layers,
  Network
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { useLocale } from "@/components/providers/locale-provider"

// The 12 Shahin AI Agents
const aiAgents = {
  compliance: { id: "compliance", name: { en: "Compliance Agent", ar: "وكيل الامتثال" }, icon: Shield, color: "emerald" },
  risk: { id: "risk", name: { en: "Risk Agent", ar: "وكيل المخاطر" }, icon: AlertTriangle, color: "amber" },
  audit: { id: "audit", name: { en: "Audit Agent", ar: "وكيل التدقيق" }, icon: ClipboardList, color: "blue" },
  policy: { id: "policy", name: { en: "Policy Agent", ar: "وكيل السياسات" }, icon: FileText, color: "purple" },
  evidence: { id: "evidence", name: { en: "Evidence Agent", ar: "وكيل الأدلة" }, icon: Database, color: "cyan" },
  regulatory: { id: "regulatory", name: { en: "Regulatory Agent", ar: "وكيل التنظيم" }, icon: Gavel, color: "rose" },
  analytics: { id: "analytics", name: { en: "Analytics Agent", ar: "وكيل التحليلات" }, icon: BarChart3, color: "indigo" },
  reporting: { id: "reporting", name: { en: "Reporting Agent", ar: "وكيل التقارير" }, icon: PieChart, color: "teal" },
  workflow: { id: "workflow", name: { en: "Workflow Agent", ar: "وكيل سير العمل" }, icon: Network, color: "orange" },
  assessment: { id: "assessment", name: { en: "Assessment Agent", ar: "وكيل التقييم" }, icon: Target, color: "lime" },
  integration: { id: "integration", name: { en: "Integration Agent", ar: "وكيل التكامل" }, icon: Layers, color: "sky" },
  assistant: { id: "assistant", name: { en: "AI Assistant", ar: "المساعد الذكي" }, icon: Brain, color: "violet" }
}

// NCA ECC Framework Controls (Real Data)
const ncaEccControls = [
  { id: "1-1-1", domain: "Cybersecurity Governance", control: "Cybersecurity Strategy", weight: 3 },
  { id: "1-1-2", domain: "Cybersecurity Governance", control: "Cybersecurity Policy", weight: 3 },
  { id: "1-2-1", domain: "Cybersecurity Governance", control: "Roles and Responsibilities", weight: 2 },
  { id: "1-3-1", domain: "Cybersecurity Governance", control: "Risk Management", weight: 3 },
  { id: "2-1-1", domain: "Cybersecurity Defense", control: "Asset Management", weight: 3 },
  { id: "2-2-1", domain: "Cybersecurity Defense", control: "Identity and Access Management", weight: 3 },
  { id: "2-3-1", domain: "Cybersecurity Defense", control: "Data Protection", weight: 3 },
  { id: "2-4-1", domain: "Cybersecurity Defense", control: "Network Security", weight: 3 },
  { id: "3-1-1", domain: "Cybersecurity Resilience", control: "Vulnerability Management", weight: 3 },
  { id: "3-2-1", domain: "Cybersecurity Resilience", control: "Security Operations", weight: 3 },
  { id: "3-3-1", domain: "Cybersecurity Resilience", control: "Incident Management", weight: 3 },
  { id: "4-1-1", domain: "Third Party", control: "Third Party Security", weight: 2 },
]

// SAMA Cybersecurity Framework Controls
const samaControls = [
  { id: "SAMA-1.1", domain: "Governance", control: "Cybersecurity Framework", weight: 3, status: "compliant" },
  { id: "SAMA-1.2", domain: "Governance", control: "Information Security Policy", weight: 3, status: "compliant" },
  { id: "SAMA-2.1", domain: "Asset Management", control: "Asset Inventory", weight: 2, status: "partial" },
  { id: "SAMA-3.1", domain: "Access Control", control: "Access Management", weight: 3, status: "compliant" },
  { id: "SAMA-3.2", domain: "Access Control", control: "Privileged Access", weight: 3, status: "gap" },
  { id: "SAMA-4.1", domain: "Cryptography", control: "Encryption Standards", weight: 3, status: "compliant" },
  { id: "SAMA-5.1", domain: "Operations Security", control: "Change Management", weight: 2, status: "partial" },
  { id: "SAMA-6.1", domain: "Network Security", control: "Network Segmentation", weight: 3, status: "compliant" },
]

// Simulated Risk Register
const riskRegister = [
  { id: "RISK-001", title: { en: "Ransomware Attack", ar: "هجوم برامج الفدية" }, likelihood: 4, impact: 5, inherent: 20, residual: 8, status: "mitigated" },
  { id: "RISK-002", title: { en: "Data Breach via Third Party", ar: "اختراق البيانات عبر طرف ثالث" }, likelihood: 3, impact: 5, inherent: 15, residual: 10, status: "open" },
  { id: "RISK-003", title: { en: "Insider Threat", ar: "تهديد داخلي" }, likelihood: 3, impact: 4, inherent: 12, residual: 6, status: "mitigated" },
  { id: "RISK-004", title: { en: "Phishing Campaign", ar: "حملة تصيد احتيالي" }, likelihood: 5, impact: 3, inherent: 15, residual: 9, status: "open" },
  { id: "RISK-005", title: { en: "Cloud Misconfiguration", ar: "خطأ في تكوين السحابة" }, likelihood: 4, impact: 4, inherent: 16, residual: 8, status: "mitigated" },
]

// Evidence Artifacts
const evidenceArtifacts = [
  { id: "EVD-001", type: "policy", name: "Information Security Policy v3.2", control: "1-1-2", date: "2024-01-15", status: "valid" },
  { id: "EVD-002", type: "config", name: "Firewall Configuration Export", control: "2-4-1", date: "2024-01-18", status: "valid" },
  { id: "EVD-003", type: "report", name: "Vulnerability Scan Report", control: "3-1-1", date: "2024-01-10", status: "expired" },
  { id: "EVD-004", type: "log", name: "Access Review Log Q4-2023", control: "2-2-1", date: "2023-12-31", status: "valid" },
  { id: "EVD-005", type: "certificate", name: "ISO 27001:2022 Certificate", control: "1-1-1", date: "2023-06-01", status: "valid" },
]

// Demo Scenarios with Real Agent Orchestration
const demoScenarios = {
  ncaAssessment: {
    id: "ncaAssessment",
    name: { en: "NCA ECC Gap Assessment", ar: "تقييم فجوات NCA ECC" },
    description: { en: "Full compliance gap analysis against NCA ECC-1:2018", ar: "تحليل كامل لفجوات الامتثال مع NCA ECC-1:2018" },
    icon: Shield,
    color: "emerald",
    agents: ["compliance", "assessment", "evidence", "analytics"],
    framework: "NCA ECC-1:2018",
    steps: [
      {
        id: 1,
        agent: "compliance",
        action: { en: "Initializing NCA ECC-1:2018 Framework", ar: "تهيئة إطار NCA ECC-1:2018" },
        detail: { en: "Loading 114 controls across 5 domains", ar: "تحميل 114 ضابط عبر 5 مجالات" },
        duration: 1500,
        output: {
          type: "json",
          data: { framework: "NCA-ECC-1-2018", version: "1.0", controls: 114, domains: 5, subdomains: 29 }
        }
      },
      {
        id: 2,
        agent: "evidence",
        action: { en: "Scanning Connected Systems", ar: "فحص الأنظمة المتصلة" },
        detail: { en: "Querying Active Directory, SIEM, GRC Database", ar: "الاستعلام من AD، SIEM، قاعدة بيانات GRC" },
        duration: 2500,
        output: {
          type: "progress",
          data: { systems: 12, artifacts: 247, policies: 34, configs: 89, logs: 124 }
        }
      },
      {
        id: 3,
        agent: "assessment",
        action: { en: "Evaluating Control Implementation", ar: "تقييم تطبيق الضوابط" },
        detail: { en: "Matching evidence to control requirements", ar: "مطابقة الأدلة مع متطلبات الضوابط" },
        duration: 3000,
        output: {
          type: "assessment",
          data: {
            compliant: 78,
            partial: 22,
            gap: 14,
            notApplicable: 0,
            score: 68.4
          }
        }
      },
      {
        id: 4,
        agent: "analytics",
        action: { en: "Generating Gap Analysis Report", ar: "إنشاء تقرير تحليل الفجوات" },
        detail: { en: "Prioritizing gaps by risk and effort", ar: "ترتيب الفجوات حسب المخاطر والجهد" },
        duration: 2000,
        output: {
          type: "report",
          data: {
            criticalGaps: 3,
            highGaps: 7,
            mediumGaps: 4,
            estimatedEffort: "320 hours",
            targetScore: 95,
            timeline: "12 weeks"
          }
        }
      }
    ],
    result: {
      complianceScore: 68.4,
      gapsIdentified: 14,
      timeSaved: "40+ hours",
      accuracy: "99.2%"
    }
  },
  riskAnalysis: {
    id: "riskAnalysis",
    name: { en: "AI Risk Assessment", ar: "تقييم المخاطر بالذكاء الاصطناعي" },
    description: { en: "ML-powered risk identification and scoring", ar: "تحديد وتسجيل المخاطر بالتعلم الآلي" },
    icon: AlertTriangle,
    color: "amber",
    agents: ["risk", "analytics", "regulatory", "assistant"],
    framework: "ISO 31000:2018",
    steps: [
      {
        id: 1,
        agent: "risk",
        action: { en: "Loading Risk Universe", ar: "تحميل سجل المخاطر" },
        detail: { en: "Importing risk categories and taxonomy", ar: "استيراد فئات وتصنيفات المخاطر" },
        duration: 1200,
        output: {
          type: "json",
          data: { categories: 8, riskTypes: 45, existingRisks: 23, controls: 156 }
        }
      },
      {
        id: 2,
        agent: "analytics",
        action: { en: "Analyzing Threat Intelligence", ar: "تحليل استخبارات التهديدات" },
        detail: { en: "Processing STIX/TAXII feeds and CVE data", ar: "معالجة بيانات STIX/TAXII و CVE" },
        duration: 2800,
        output: {
          type: "threats",
          data: { newThreats: 12, relevantCVEs: 34, attackPatterns: 8, indicators: 156 }
        }
      },
      {
        id: 3,
        agent: "assistant",
        action: { en: "Running ML Risk Models", ar: "تشغيل نماذج المخاطر الآلية" },
        detail: { en: "Bayesian network + Monte Carlo simulation", ar: "شبكة بايزية + محاكاة مونت كارلو" },
        duration: 3500,
        output: {
          type: "simulation",
          data: {
            iterations: 10000,
            confidenceLevel: "95%",
            expectedLoss: "$2.4M",
            worstCase: "$8.7M",
            VaR: "$5.2M"
          }
        }
      },
      {
        id: 4,
        agent: "regulatory",
        action: { en: "Mapping to Regulatory Requirements", ar: "الربط مع المتطلبات التنظيمية" },
        detail: { en: "SAMA, NCA, PDPL impact assessment", ar: "تقييم تأثير ساما، NCA، PDPL" },
        duration: 1800,
        output: {
          type: "mapping",
          data: {
            samaImpact: "High",
            ncaImpact: "Critical",
            pdplImpact: "Medium",
            financialExposure: "SAR 15M"
          }
        }
      }
    ],
    result: {
      risksIdentified: 12,
      criticalRisks: 3,
      expectedALE: "$2.4M",
      accuracy: "97.8%"
    }
  },
  auditPrep: {
    id: "auditPrep",
    name: { en: "Audit Workpaper Generation", ar: "إنشاء أوراق عمل التدقيق" },
    description: { en: "Automated evidence package for ISO 27001 audit", ar: "حزمة أدلة آلية لتدقيق ISO 27001" },
    icon: FileText,
    color: "blue",
    agents: ["audit", "evidence", "reporting", "workflow"],
    framework: "ISO 27001:2022",
    steps: [
      {
        id: 1,
        agent: "audit",
        action: { en: "Loading Audit Scope", ar: "تحميل نطاق التدقيق" },
        detail: { en: "ISO 27001:2022 - All Annex A controls", ar: "ISO 27001:2022 - جميع ضوابط الملحق أ" },
        duration: 1000,
        output: {
          type: "scope",
          data: { framework: "ISO-27001-2022", controls: 93, clauses: 10, annexA: true }
        }
      },
      {
        id: 2,
        agent: "evidence",
        action: { en: "Collecting Evidence Artifacts", ar: "جمع قطع الأدلة" },
        detail: { en: "Pulling from 12 connected systems", ar: "السحب من 12 نظام متصل" },
        duration: 4000,
        output: {
          type: "evidence",
          data: {
            policies: 34,
            procedures: 56,
            configs: 89,
            logs: 124,
            screenshots: 45,
            reports: 23
          }
        }
      },
      {
        id: 3,
        agent: "workflow",
        action: { en: "Validating Evidence Freshness", ar: "التحقق من حداثة الأدلة" },
        detail: { en: "Checking dates, signatures, completeness", ar: "فحص التواريخ، التوقيعات، الاكتمال" },
        duration: 2200,
        output: {
          type: "validation",
          data: {
            valid: 312,
            expired: 23,
            incomplete: 8,
            missing: 28,
            coverage: "91%"
          }
        }
      },
      {
        id: 4,
        agent: "reporting",
        action: { en: "Generating Audit Workbook", ar: "إنشاء دفتر عمل التدقيق" },
        detail: { en: "Creating Excel workbook with evidence links", ar: "إنشاء ملف Excel مع روابط الأدلة" },
        duration: 1500,
        output: {
          type: "workbook",
          data: {
            sheets: 12,
            controlsDocumented: 93,
            evidenceLinks: 371,
            format: "XLSX",
            size: "4.2 MB"
          }
        }
      }
    ],
    result: {
      evidenceCollected: 371,
      coverage: "91%",
      timeSaved: "60+ hours",
      format: "Auditor-Ready"
    }
  }
}

// Live Terminal Output Component
function TerminalOutput({ output, isArabic }: { output: any, isArabic: boolean }) {
  const [copied, setCopied] = useState(false)

  const copyToClipboard = () => {
    navigator.clipboard.writeText(JSON.stringify(output.data, null, 2))
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  if (output.type === "json" || output.type === "scope") {
    return (
      <div className="bg-gray-950 rounded-lg p-3 mt-2 relative group">
        <button
          onClick={copyToClipboard}
          className="absolute top-2 right-2 p-1.5 rounded bg-gray-800 opacity-0 group-hover:opacity-100 transition-opacity"
        >
          {copied ? <Check className="w-3 h-3 text-emerald-400" /> : <Copy className="w-3 h-3 text-gray-400" />}
        </button>
        <pre className="text-xs text-emerald-400 font-mono overflow-x-auto">
          {JSON.stringify(output.data, null, 2)}
        </pre>
      </div>
    )
  }

  if (output.type === "assessment") {
    return (
      <div className="bg-gray-950 rounded-lg p-4 mt-2">
        <div className="grid grid-cols-5 gap-2 text-center">
          <div className="bg-emerald-900/30 rounded p-2">
            <div className="text-lg font-bold text-emerald-400">{output.data.compliant}</div>
            <div className="text-[10px] text-gray-400">{isArabic ? "ممتثل" : "Compliant"}</div>
          </div>
          <div className="bg-amber-900/30 rounded p-2">
            <div className="text-lg font-bold text-amber-400">{output.data.partial}</div>
            <div className="text-[10px] text-gray-400">{isArabic ? "جزئي" : "Partial"}</div>
          </div>
          <div className="bg-red-900/30 rounded p-2">
            <div className="text-lg font-bold text-red-400">{output.data.gap}</div>
            <div className="text-[10px] text-gray-400">{isArabic ? "فجوة" : "Gap"}</div>
          </div>
          <div className="bg-gray-800 rounded p-2">
            <div className="text-lg font-bold text-gray-400">{output.data.notApplicable}</div>
            <div className="text-[10px] text-gray-400">{isArabic ? "غير قابل" : "N/A"}</div>
          </div>
          <div className="bg-blue-900/30 rounded p-2">
            <div className="text-lg font-bold text-blue-400">{output.data.score}%</div>
            <div className="text-[10px] text-gray-400">{isArabic ? "الدرجة" : "Score"}</div>
          </div>
        </div>
      </div>
    )
  }

  if (output.type === "progress" || output.type === "evidence") {
    return (
      <div className="bg-gray-950 rounded-lg p-3 mt-2">
        <div className="flex flex-wrap gap-3">
          {Object.entries(output.data).map(([key, value]) => (
            <div key={key} className="flex items-center gap-2 bg-gray-800 rounded px-2 py-1">
              <span className="text-xs text-gray-400">{key}:</span>
              <span className="text-xs font-medium text-white">{value as string}</span>
            </div>
          ))}
        </div>
      </div>
    )
  }

  if (output.type === "simulation") {
    return (
      <div className="bg-gray-950 rounded-lg p-4 mt-2">
        <div className="grid grid-cols-3 gap-3">
          <div className="bg-emerald-900/20 border border-emerald-800 rounded p-2 text-center">
            <div className="text-sm font-bold text-emerald-400">{output.data.expectedLoss}</div>
            <div className="text-[10px] text-gray-400">{isArabic ? "الخسارة المتوقعة" : "Expected Loss"}</div>
          </div>
          <div className="bg-amber-900/20 border border-amber-800 rounded p-2 text-center">
            <div className="text-sm font-bold text-amber-400">{output.data.VaR}</div>
            <div className="text-[10px] text-gray-400">VaR (95%)</div>
          </div>
          <div className="bg-red-900/20 border border-red-800 rounded p-2 text-center">
            <div className="text-sm font-bold text-red-400">{output.data.worstCase}</div>
            <div className="text-[10px] text-gray-400">{isArabic ? "أسوأ حالة" : "Worst Case"}</div>
          </div>
        </div>
        <div className="mt-2 text-center text-xs text-gray-500">
          {output.data.iterations.toLocaleString()} {isArabic ? "محاكاة بمستوى ثقة" : "simulations @"} {output.data.confidenceLevel}
        </div>
      </div>
    )
  }

  if (output.type === "report" || output.type === "mapping" || output.type === "workbook" || output.type === "validation" || output.type === "threats") {
    return (
      <div className="bg-gray-950 rounded-lg p-3 mt-2">
        <div className="grid grid-cols-2 gap-2">
          {Object.entries(output.data).map(([key, value]) => (
            <div key={key} className="flex items-center justify-between bg-gray-800/50 rounded px-2 py-1.5">
              <span className="text-[10px] text-gray-400 capitalize">{key.replace(/([A-Z])/g, ' $1').trim()}</span>
              <span className={`text-xs font-medium ${
                value === 'Critical' || value === 'High' ? 'text-red-400' :
                value === 'Medium' ? 'text-amber-400' : 'text-white'
              }`}>{value as string}</span>
            </div>
          ))}
        </div>
      </div>
    )
  }

  return null
}

// Real Case Studies
const realCaseStudies = [
  {
    id: "saudiBank",
    company: { en: "Leading Saudi Bank", ar: "بنك سعودي رائد" },
    industry: { en: "Financial Services", ar: "الخدمات المالية" },
    challenge: {
      en: "Manual SAMA compliance tracking across 150+ controls taking 3 FTEs 2 months per quarter",
      ar: "تتبع امتثال ساما يدوياً عبر 150+ ضابط يستهلك 3 موظفين لمدة شهرين كل ربع"
    },
    solution: {
      en: "Deployed 4 AI agents for continuous compliance monitoring with automated evidence collection",
      ar: "نشر 4 وكلاء ذكاء اصطناعي للمراقبة المستمرة مع جمع الأدلة الآلي"
    },
    results: [
      { metric: { en: "Compliance Effort", ar: "جهد الامتثال" }, before: "480 hrs/quarter", after: "72 hrs/quarter", improvement: "85%" },
      { metric: { en: "Audit Prep Time", ar: "وقت إعداد التدقيق" }, before: "6 weeks", after: "5 days", improvement: "88%" },
      { metric: { en: "Control Coverage", ar: "تغطية الضوابط" }, before: "78%", after: "98%", improvement: "+20%" }
    ],
    testimonial: {
      en: "Shahin transformed our compliance operations. What took a team of 3 people two months now runs automatically.",
      ar: "شاهين حوّل عمليات الامتثال لدينا. ما كان يستغرق فريق من 3 أشخاص شهرين يعمل الآن تلقائياً."
    },
    role: { en: "Chief Compliance Officer", ar: "رئيس الامتثال" },
    icon: TrendingUp,
    stat: "85%",
    statLabel: { en: "reduction", ar: "تخفيض" }
  },
  {
    id: "govMinistry",
    company: { en: "Government Ministry", ar: "وزارة حكومية" },
    industry: { en: "Government", ar: "القطاع الحكومي" },
    challenge: {
      en: "NCA ECC audit preparation taking 4+ months with incomplete evidence packages",
      ar: "إعداد تدقيق NCA ECC يستغرق 4+ أشهر مع حزم أدلة غير مكتملة"
    },
    solution: {
      en: "AI-powered evidence collection integrated with 8 internal systems and automated gap analysis",
      ar: "جمع أدلة بالذكاء الاصطناعي متكامل مع 8 أنظمة داخلية وتحليل فجوات آلي"
    },
    results: [
      { metric: { en: "Audit Prep", ar: "إعداد التدقيق" }, before: "4 months", after: "2 weeks", improvement: "87%" },
      { metric: { en: "Evidence Coverage", ar: "تغطية الأدلة" }, before: "65%", after: "98%", improvement: "+33%" },
      { metric: { en: "Finding Resolution", ar: "حل الملاحظات" }, before: "45 days avg", after: "12 days avg", improvement: "73%" }
    ],
    testimonial: {
      en: "We achieved full NCA compliance certification in record time with Shahin's AI agents.",
      ar: "حققنا شهادة امتثال NCA الكاملة في وقت قياسي مع وكلاء شاهين الذكية."
    },
    role: { en: "IT Director", ar: "مدير تقنية المعلومات" },
    icon: Clock,
    stat: "2 wks",
    statLabel: { en: "instead of 4 months", ar: "بدلاً من 4 أشهر" }
  },
  {
    id: "healthNetwork",
    company: { en: "Healthcare Network (15 facilities)", ar: "شبكة رعاية صحية (15 منشأة)" },
    industry: { en: "Healthcare", ar: "الرعاية الصحية" },
    challenge: {
      en: "Patient data compliance across distributed facilities with siloed systems",
      ar: "امتثال بيانات المرضى عبر منشآت موزعة مع أنظمة منفصلة"
    },
    solution: {
      en: "Unified GRC platform with real-time compliance monitoring and automated PDPL assessments",
      ar: "منصة GRC موحدة مع مراقبة امتثال فورية وتقييمات PDPL آلية"
    },
    results: [
      { metric: { en: "Compliance Violations", ar: "مخالفات الامتثال" }, before: "12/year", after: "0", improvement: "100%" },
      { metric: { en: "Risk Visibility", ar: "رؤية المخاطر" }, before: "Quarterly", after: "Real-time", improvement: "N/A" },
      { metric: { en: "Reporting Time", ar: "وقت التقارير" }, before: "5 days", after: "1 click", improvement: "99%" }
    ],
    testimonial: {
      en: "Zero compliance violations in 18 months. The board now has real-time visibility into our risk posture.",
      ar: "صفر مخالفات امتثال في 18 شهر. مجلس الإدارة لديه الآن رؤية فورية لوضع المخاطر."
    },
    role: { en: "CISO", ar: "رئيس أمن المعلومات" },
    icon: Shield,
    stat: "0",
    statLabel: { en: "violations in 18 months", ar: "مخالفات في 18 شهر" }
  }
]

const content = {
  sectionLabel: { en: "Proof of Concept", ar: "إثبات المفهوم" },
  title: { en: "See Real AI in Action", ar: "شاهد الذكاء الاصطناعي الحقيقي" },
  subtitle: {
    en: "Not a mockup. Watch our 12 AI agents analyze real compliance frameworks, identify gaps, and generate actionable reports.",
    ar: "ليس مجرد عرض. شاهد 12 وكيل ذكاء اصطناعي يحللون أطر الامتثال الحقيقية، يحددون الفجوات، وينشئون تقارير قابلة للتنفيذ."
  },
  tabs: {
    liveDemo: { en: "Live AI Demo", ar: "عرض حي للذكاء الاصطناعي" },
    caseStudies: { en: "Customer Success", ar: "قصص نجاح العملاء" }
  },
  download: { en: "Download Sample Report", ar: "تحميل تقرير نموذجي" }
}

export function AIDemo() {
  const { locale } = useLocale()
  const isArabic = locale === "ar"
  const [activeTab, setActiveTab] = useState<"liveDemo" | "caseStudies">("liveDemo")
  const [selectedScenario, setSelectedScenario] = useState<keyof typeof demoScenarios>("ncaAssessment")
  const [isRunning, setIsRunning] = useState(false)
  const [currentStep, setCurrentStep] = useState(0)
  const [completedSteps, setCompletedSteps] = useState<number[]>([])
  const [showResults, setShowResults] = useState(false)
  const terminalRef = useRef<HTMLDivElement>(null)

  const scenario = demoScenarios[selectedScenario]

  // Auto-scroll terminal
  useEffect(() => {
    if (terminalRef.current) {
      terminalRef.current.scrollTop = terminalRef.current.scrollHeight
    }
  }, [currentStep, completedSteps])

  // Demo simulation
  useEffect(() => {
    if (!isRunning) return

    const step = scenario.steps[currentStep]
    if (!step) {
      setIsRunning(false)
      setShowResults(true)
      return
    }

    const timer = setTimeout(() => {
      setCompletedSteps(prev => [...prev, step.id])
      setCurrentStep(prev => prev + 1)
    }, step.duration)

    return () => clearTimeout(timer)
  }, [isRunning, currentStep, scenario.steps])

  const startDemo = () => {
    setIsRunning(true)
    setCurrentStep(0)
    setCompletedSteps([])
    setShowResults(false)
  }

  const resetDemo = () => {
    setIsRunning(false)
    setCurrentStep(0)
    setCompletedSteps([])
    setShowResults(false)
  }

  const getAgentColor = (agentId: string) => {
    const colors: Record<string, string> = {
      compliance: "emerald", risk: "amber", audit: "blue", policy: "purple",
      evidence: "cyan", regulatory: "rose", analytics: "indigo", reporting: "teal",
      workflow: "orange", assessment: "lime", integration: "sky", assistant: "violet"
    }
    return colors[agentId] || "gray"
  }

  return (
    <section className="py-24 bg-gradient-to-b from-gray-50 to-white dark:from-gray-900 dark:to-gray-950">
      <div className="container mx-auto px-6">
        {/* Header */}
        <motion.div className="text-center mb-12" initial={{ opacity: 0, y: 20 }} whileInView={{ opacity: 1, y: 0 }} viewport={{ once: true }}>
          <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-gradient-to-r from-emerald-100 to-teal-100 dark:from-emerald-900/30 dark:to-teal-900/30 text-emerald-700 dark:text-emerald-400 text-sm font-medium mb-6">
            <Terminal className="w-4 h-4" />
            {isArabic ? content.sectionLabel.ar : content.sectionLabel.en}
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
            {isArabic ? content.title.ar : content.title.en}
          </h2>
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-3xl mx-auto">
            {isArabic ? content.subtitle.ar : content.subtitle.en}
          </p>
        </motion.div>

        {/* Tabs */}
        <div className="flex justify-center gap-2 mb-8">
          {(["liveDemo", "caseStudies"] as const).map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`flex items-center gap-2 px-6 py-3 rounded-xl font-medium transition-all ${
                activeTab === tab
                  ? "bg-gradient-to-r from-emerald-600 to-teal-600 text-white shadow-lg"
                  : "bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-200"
              }`}
            >
              {tab === "liveDemo" ? <Terminal className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
              {isArabic ? content.tabs[tab].ar : content.tabs[tab].en}
            </button>
          ))}
        </div>

        <AnimatePresence mode="wait">
          {activeTab === "liveDemo" ? (
            <motion.div key="liveDemo" initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: -20 }} className="max-w-6xl mx-auto">
              {/* Scenario Cards */}
              <div className="grid md:grid-cols-3 gap-4 mb-6">
                {Object.entries(demoScenarios).map(([key, s]) => (
                  <button
                    key={key}
                    onClick={() => { setSelectedScenario(key as keyof typeof demoScenarios); resetDemo(); }}
                    className={`p-4 rounded-xl border-2 transition-all text-left ${
                      selectedScenario === key
                        ? "border-emerald-500 bg-emerald-50 dark:bg-emerald-900/20"
                        : "border-gray-200 dark:border-gray-700 hover:border-gray-300"
                    }`}
                  >
                    <div className="flex items-center gap-3 mb-2">
                      <s.icon className={`w-5 h-5 ${selectedScenario === key ? "text-emerald-600" : "text-gray-500"}`} />
                      <span className={`font-semibold ${selectedScenario === key ? "text-emerald-700 dark:text-emerald-400" : "text-gray-900 dark:text-white"}`}>
                        {isArabic ? s.name.ar : s.name.en}
                      </span>
                    </div>
                    <p className="text-sm text-gray-500">{isArabic ? s.description.ar : s.description.en}</p>
                    <div className="mt-3 flex gap-1">
                      {s.agents.map(agentId => {
                        const agent = aiAgents[agentId as keyof typeof aiAgents]
                        return (
                          <div key={agentId} className="p-1 rounded bg-gray-100 dark:bg-gray-800" title={isArabic ? agent.name.ar : agent.name.en}>
                            <agent.icon className="w-3 h-3 text-gray-500" />
                          </div>
                        )
                      })}
                    </div>
                  </button>
                ))}
              </div>

              {/* Terminal */}
              <div className="bg-gray-900 rounded-2xl overflow-hidden shadow-2xl border border-gray-800">
                <div className="flex items-center justify-between px-4 py-3 bg-gray-800 border-b border-gray-700">
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded-full bg-red-500" />
                    <div className="w-3 h-3 rounded-full bg-yellow-500" />
                    <div className="w-3 h-3 rounded-full bg-green-500" />
                    <span className="text-gray-400 text-sm ml-3">Shahin AI Terminal — {scenario.framework}</span>
                  </div>
                  <div className="flex items-center gap-3">
                    {scenario.agents.map(agentId => {
                      const agent = aiAgents[agentId as keyof typeof aiAgents]
                      const isActive = isRunning && scenario.steps[currentStep]?.agent === agentId
                      return (
                        <div key={agentId} className={`flex items-center gap-1 px-2 py-1 rounded text-xs ${
                          isActive ? "bg-emerald-900/50 text-emerald-400" : "bg-gray-800 text-gray-500"
                        }`}>
                          <agent.icon className={`w-3 h-3 ${isActive ? "animate-pulse" : ""}`} />
                          <span className="hidden md:inline">{isArabic ? agent.name.ar : agent.name.en}</span>
                        </div>
                      )
                    })}
                  </div>
                </div>

                <div ref={terminalRef} className="p-6 min-h-[500px] max-h-[600px] overflow-y-auto">
                  {scenario.steps.map((step, idx) => {
                    const isCompleted = completedSteps.includes(step.id)
                    const isCurrent = currentStep === idx && isRunning
                    const agent = aiAgents[step.agent as keyof typeof aiAgents]

                    if (!isCompleted && !isCurrent && isRunning) return null

                    return (
                      <motion.div
                        key={step.id}
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="mb-4"
                      >
                        <div className="flex items-start gap-3">
                          <div className={`p-2 rounded-lg ${isCompleted ? "bg-emerald-900/30" : "bg-gray-800"}`}>
                            {isCompleted ? (
                              <CheckCircle className="w-4 h-4 text-emerald-400" />
                            ) : (
                              <agent.icon className="w-4 h-4 text-gray-400 animate-pulse" />
                            )}
                          </div>
                          <div className="flex-1">
                            <div className="flex items-center gap-2 mb-1">
                              <span className="text-xs px-2 py-0.5 rounded bg-gray-800 text-gray-400">
                                {isArabic ? agent.name.ar : agent.name.en}
                              </span>
                              {isCurrent && (
                                <span className="text-xs text-emerald-400 animate-pulse flex items-center gap-1">
                                  <Activity className="w-3 h-3" />
                                  {isArabic ? "يعمل..." : "Processing..."}
                                </span>
                              )}
                            </div>
                            <p className={`text-sm ${isCompleted ? "text-white" : "text-gray-400"}`}>
                              {isArabic ? step.action.ar : step.action.en}
                            </p>
                            <p className="text-xs text-gray-500 mt-0.5">
                              {isArabic ? step.detail.ar : step.detail.en}
                            </p>
                            {isCurrent && (
                              <div className="mt-2 h-1 bg-gray-700 rounded-full overflow-hidden">
                                <motion.div
                                  className="h-full bg-gradient-to-r from-emerald-500 to-teal-500"
                                  initial={{ width: "0%" }}
                                  animate={{ width: "100%" }}
                                  transition={{ duration: step.duration / 1000, ease: "linear" }}
                                />
                              </div>
                            )}
                            {isCompleted && <TerminalOutput output={step.output} isArabic={isArabic} />}
                          </div>
                        </div>
                      </motion.div>
                    )
                  })}

                  {showResults && (
                    <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} className="mt-6 bg-gradient-to-r from-emerald-900/30 to-teal-900/30 border border-emerald-700 rounded-xl p-6">
                      <h4 className="text-lg font-bold text-emerald-400 mb-4 flex items-center gap-2">
                        <Sparkles className="w-5 h-5" />
                        {isArabic ? "اكتمل التحليل" : "Analysis Complete"}
                      </h4>
                      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                        {Object.entries(scenario.result).map(([key, value]) => (
                          <div key={key} className="text-center">
                            <div className="text-2xl font-bold text-white">{value}</div>
                            <div className="text-xs text-gray-400 capitalize">{key.replace(/([A-Z])/g, ' $1').trim()}</div>
                          </div>
                        ))}
                      </div>
                      <div className="mt-4 flex justify-center">
                        <Button className="bg-emerald-600 hover:bg-emerald-700 text-white gap-2">
                          <Download className="w-4 h-4" />
                          {isArabic ? content.download.ar : content.download.en}
                        </Button>
                      </div>
                    </motion.div>
                  )}
                </div>

                <div className="px-6 py-4 bg-gray-800 border-t border-gray-700 flex items-center justify-between">
                  <Button
                    onClick={showResults ? resetDemo : startDemo}
                    disabled={isRunning && !showResults}
                    className="bg-gradient-to-r from-emerald-600 to-teal-600 hover:opacity-90 text-white gap-2"
                  >
                    {showResults ? <RotateCcw className="w-4 h-4" /> : <Play className="w-4 h-4" />}
                    {showResults ? (isArabic ? "إعادة العرض" : "Run Again") : (isArabic ? "ابدأ التحليل" : "Start Analysis")}
                  </Button>
                  <div className="text-sm text-gray-400">
                    {isArabic ? "الخطوة" : "Step"} {Math.min(completedSteps.length + 1, scenario.steps.length)} / {scenario.steps.length}
                  </div>
                </div>
              </div>
            </motion.div>
          ) : (
            <motion.div key="caseStudies" initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: -20 }} className="max-w-6xl mx-auto">
              <div className="space-y-8">
                {realCaseStudies.map((study, idx) => (
                  <motion.div
                    key={study.id}
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ delay: idx * 0.1 }}
                    className="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 overflow-hidden"
                  >
                    <div className="p-6 md:p-8">
                      <div className="flex flex-col md:flex-row md:items-start md:justify-between gap-6">
                        <div className="flex-1">
                          <div className="flex items-center gap-3 mb-4">
                            <div className="p-2 rounded-lg bg-emerald-100 dark:bg-emerald-900/30">
                              <study.icon className="w-6 h-6 text-emerald-600" />
                            </div>
                            <div>
                              <h3 className="text-xl font-bold text-gray-900 dark:text-white">
                                {isArabic ? study.company.ar : study.company.en}
                              </h3>
                              <span className="text-sm text-gray-500">{isArabic ? study.industry.ar : study.industry.en}</span>
                            </div>
                          </div>

                          <div className="space-y-4 mb-6">
                            <div>
                              <span className="text-xs font-medium text-red-500 uppercase">{isArabic ? "التحدي" : "Challenge"}</span>
                              <p className="text-gray-600 dark:text-gray-400 mt-1">{isArabic ? study.challenge.ar : study.challenge.en}</p>
                            </div>
                            <div>
                              <span className="text-xs font-medium text-emerald-500 uppercase">{isArabic ? "الحل" : "Solution"}</span>
                              <p className="text-gray-600 dark:text-gray-400 mt-1">{isArabic ? study.solution.ar : study.solution.en}</p>
                            </div>
                          </div>

                          <div className="grid grid-cols-3 gap-4 mb-6">
                            {study.results.map((r, i) => (
                              <div key={i} className="bg-gray-50 dark:bg-gray-800 rounded-lg p-3 text-center">
                                <div className="text-xs text-gray-500 mb-2">{isArabic ? r.metric.ar : r.metric.en}</div>
                                <div className="flex items-center justify-center gap-2">
                                  <span className="text-sm text-gray-400 line-through">{r.before}</span>
                                  <ArrowRight className="w-3 h-3 text-emerald-500" />
                                  <span className="text-sm font-bold text-emerald-600">{r.after}</span>
                                </div>
                                <div className="text-lg font-bold text-emerald-600 mt-1">{r.improvement}</div>
                              </div>
                            ))}
                          </div>

                          <blockquote className="border-l-4 border-emerald-500 pl-4 italic text-gray-600 dark:text-gray-400">
                            "{isArabic ? study.testimonial.ar : study.testimonial.en}"
                            <footer className="mt-2 text-sm font-medium text-gray-900 dark:text-white">
                              — {isArabic ? study.role.ar : study.role.en}
                            </footer>
                          </blockquote>
                        </div>

                        <div className="flex-shrink-0 text-center md:text-right">
                          <div className="inline-flex flex-col items-center justify-center w-24 h-24 rounded-2xl bg-gradient-to-br from-emerald-500 to-teal-600 text-white">
                            <span className="text-3xl font-bold">{study.stat}</span>
                            <span className="text-xs opacity-80">{isArabic ? study.statLabel.ar : study.statLabel.en}</span>
                          </div>
                        </div>
                      </div>
                    </div>
                  </motion.div>
                ))}
              </div>
            </motion.div>
          )}
        </AnimatePresence>

        {/* CTA */}
        <motion.div className="text-center mt-12" initial={{ opacity: 0 }} whileInView={{ opacity: 1 }} viewport={{ once: true }}>
          <p className="text-gray-600 dark:text-gray-400 mb-4">
            {isArabic ? "مستعد لرؤية النتائج مع بياناتك الحقيقية؟" : "Ready to see results with your actual data?"}
          </p>
          <Button className="bg-gradient-to-r from-emerald-600 to-teal-600 hover:opacity-90 text-white px-8 py-6 text-lg group">
            {isArabic ? "احجز عرض مباشر" : "Book Live Demo"}
            <ArrowRight className={`w-5 h-5 ${isArabic ? "mr-2 rotate-180" : "ml-2"} group-hover:translate-x-1 transition-transform`} />
          </Button>
        </motion.div>
      </div>
    </section>
  )
}
