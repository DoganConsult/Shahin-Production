"use client"

import { motion } from "framer-motion"
import { Bot, Sparkles, Brain, Shield, CheckCircle, Zap, Globe } from "lucide-react"
import { Button } from "@/components/ui/button"
import Link from "next/link"
import { useLocale } from "@/components/providers/locale-provider"

// Configuration from environment variables (no hardcoding)
const getAIIntegrationsConfig = () => {
  return {
    azure: {
      enabled: process.env.NEXT_PUBLIC_AZURE_BOT_ENABLED === "true",
      name: process.env.NEXT_PUBLIC_AZURE_BOT_NAME || "Azure Bot",
      description: process.env.NEXT_PUBLIC_AZURE_BOT_DESCRIPTION || "Enterprise-grade conversational AI",
      icon: Bot,
      color: "blue",
      features: (process.env.NEXT_PUBLIC_AZURE_BOT_FEATURES || "Natural Language Processing,Multi-turn Conversations,Enterprise Security").split(",")
    },
    copilot: {
      enabled: process.env.NEXT_PUBLIC_COPILOT_ENABLED === "true",
      name: process.env.NEXT_PUBLIC_COPILOT_NAME || "Microsoft Copilot",
      description: process.env.NEXT_PUBLIC_COPILOT_DESCRIPTION || "AI-powered productivity assistant",
      icon: Sparkles,
      color: "cyan",
      features: (process.env.NEXT_PUBLIC_COPILOT_FEATURES || "Context-Aware Assistance,Document Analysis,Smart Suggestions").split(",")
    },
    claude: {
      enabled: process.env.NEXT_PUBLIC_CLAUDE_ENABLED === "true",
      name: process.env.NEXT_PUBLIC_CLAUDE_NAME || "Claude AI",
      description: process.env.NEXT_PUBLIC_CLAUDE_DESCRIPTION || "Advanced reasoning and analysis",
      icon: Brain,
      color: "emerald",
      features: (process.env.NEXT_PUBLIC_CLAUDE_FEATURES || "Deep Analysis,Complex Reasoning,Long Context").split(",")
    },
    shahin: {
      enabled: process.env.NEXT_PUBLIC_SHAHIN_AI_ENABLED !== "false",
      name: process.env.NEXT_PUBLIC_SHAHIN_AI_NAME || "Shahin AI",
      description: process.env.NEXT_PUBLIC_SHAHIN_AI_DESCRIPTION || "Specialized GRC AI assistant",
      icon: Shield,
      color: "violet",
      features: (process.env.NEXT_PUBLIC_SHAHIN_AI_FEATURES || "GRC Expertise,Compliance Analysis,Risk Assessment").split(",")
    }
  }
}

export function AIIntegrationsShowcase() {
  const { locale } = useLocale()
  const isArabic = locale === "ar"
  const config = getAIIntegrationsConfig()

  const integrations = [
    {
      id: "shahin",
      ...config.shahin,
      nameAr: "شاهين الذكي",
      descriptionAr: "مساعد GRC متخصص",
      featuresAr: ["خبرة GRC", "تحليل الامتثال", "تقييم المخاطر"]
    },
    {
      id: "azure",
      ...config.azure,
      nameAr: "بوت Azure",
      descriptionAr: "ذكاء اصطناعي محادثي على مستوى المؤسسات",
      featuresAr: ["معالجة اللغة الطبيعية", "محادثات متعددة الجولات", "أمان المؤسسات"]
    },
    {
      id: "copilot",
      ...config.copilot,
      nameAr: "Microsoft Copilot",
      descriptionAr: "مساعد إنتاجية مدعوم بالذكاء الاصطناعي",
      featuresAr: ["مساعدة واعية بالسياق", "تحليل المستندات", "اقتراحات ذكية"]
    },
    {
      id: "claude",
      ...config.claude,
      nameAr: "Claude AI",
      descriptionAr: "استدلال وتحليل متقدم",
      featuresAr: ["تحليل عميق", "استدلال معقد", "سياق طويل"]
    }
  ].filter(integration => integration.enabled)

  const getColorClasses = (color: string) => {
    const colors: Record<string, { bg: string; text: string; border: string; gradient: string }> = {
      blue: {
        bg: "bg-blue-100 dark:bg-blue-900/30",
        text: "text-blue-600 dark:text-blue-400",
        border: "border-blue-200 dark:border-blue-800",
        gradient: "from-blue-600 to-blue-500"
      },
      cyan: {
        bg: "bg-cyan-100 dark:bg-cyan-900/30",
        text: "text-cyan-600 dark:text-cyan-400",
        border: "border-cyan-200 dark:border-cyan-800",
        gradient: "from-cyan-600 to-cyan-500"
      },
      emerald: {
        bg: "bg-emerald-100 dark:bg-emerald-900/30",
        text: "text-emerald-600 dark:text-emerald-400",
        border: "border-emerald-200 dark:border-emerald-800",
        gradient: "from-emerald-600 to-emerald-500"
      },
      violet: {
        bg: "bg-violet-100 dark:bg-violet-900/30",
        text: "text-violet-600 dark:text-violet-400",
        border: "border-violet-200 dark:border-violet-800",
        gradient: "from-violet-600 to-violet-500"
      }
    }
    return colors[color] || colors.violet
  }

  return (
    <section className="py-24 bg-gradient-to-b from-gray-50 to-white dark:from-gray-900 dark:to-gray-950">
      <div className="container mx-auto px-6">
        {/* Section Header */}
        <motion.div
          className="text-center mb-16"
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-violet-100 dark:bg-violet-900/30 text-violet-700 dark:text-violet-300 text-sm font-medium mb-6">
            <Brain className="w-4 h-4" />
            {isArabic ? "تكاملات الذكاء الاصطناعي" : "AI Integrations"}
          </span>
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 dark:text-white mb-4">
            {isArabic
              ? "قوة متعددة من الذكاء الاصطناعي"
              : "Multiple AI Powers, One Platform"}
          </h2>
          <p className="text-lg text-gray-600 dark:text-gray-400 max-w-3xl mx-auto">
            {isArabic
              ? "استفد من أفضل منصات الذكاء الاصطناعي - Azure Bot و Microsoft Copilot و Claude AI - كلها متكاملة في منصة Shahin AI"
              : "Leverage the best AI platforms - Azure Bot, Microsoft Copilot, and Claude AI - all integrated into the Shahin AI platform"}
          </p>
        </motion.div>

        {/* Integrations Grid */}
        <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
          {integrations.map((integration, index) => {
            const colors = getColorClasses(integration.color)
            const Icon = integration.icon

            return (
              <motion.div
                key={integration.id}
                initial={{ opacity: 0, y: 30 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1 }}
                className={`bg-white dark:bg-gray-800 rounded-2xl border-2 ${colors.border} p-6 hover:shadow-xl transition-all duration-300`}
              >
                {/* Icon */}
                <div className={`w-16 h-16 ${colors.bg} rounded-xl flex items-center justify-center mb-4`}>
                  <Icon className={`w-8 h-8 ${colors.text}`} />
                </div>

                {/* Name */}
                <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
                  {isArabic ? integration.nameAr : integration.name}
                </h3>

                {/* Description */}
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-4">
                  {isArabic ? integration.descriptionAr : integration.description}
                </p>

                {/* Features */}
                <div className="space-y-2 mb-4">
                  {(isArabic ? integration.featuresAr : integration.features).map((feature, i) => (
                    <div key={i} className="flex items-center gap-2">
                      <CheckCircle className={`w-4 h-4 ${colors.text} flex-shrink-0`} />
                      <span className="text-sm text-gray-700 dark:text-gray-300">{feature}</span>
                    </div>
                  ))}
                </div>

                {/* Status Badge */}
                <div className={`inline-flex items-center gap-2 px-3 py-1 rounded-full ${colors.bg} ${colors.text} text-xs font-medium`}>
                  <div className={`w-2 h-2 rounded-full bg-${integration.color}-500 animate-pulse`} />
                  {isArabic ? "متاح" : "Available"}
                </div>
              </motion.div>
            )
          })}
        </div>

        {/* Unified Experience Section */}
        <motion.div
          className="bg-gradient-to-r from-violet-600 to-purple-600 rounded-3xl p-8 md:p-12 text-white"
          initial={{ opacity: 0, y: 30 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <div className="max-w-4xl mx-auto text-center">
            <Shield className="w-12 h-12 mx-auto mb-6 opacity-90" />
            <h3 className="text-2xl md:text-3xl font-bold mb-4">
              {isArabic
                ? "تجربة موحدة عبر جميع منصات الذكاء الاصطناعي"
                : "Unified Experience Across All AI Platforms"}
            </h3>
            <p className="text-lg text-white/90 mb-8">
              {isArabic
                ? "استخدم Azure Bot و Microsoft Copilot و Claude AI من واجهة واحدة. اختر المنصة الأنسب لمهمتك أو دع Shahin AI يختار تلقائياً."
                : "Use Azure Bot, Microsoft Copilot, and Claude AI from a single interface. Choose the best platform for your task or let Shahin AI choose automatically."}
            </p>

            <div className="grid md:grid-cols-3 gap-6 mb-8">
              {[
                {
                  icon: Zap,
                  title: isArabic ? "اختيار تلقائي" : "Auto Selection",
                  description: isArabic
                    ? "Shahin AI يختار أفضل منصة لكل مهمة"
                    : "Shahin AI selects the best platform for each task"
                },
                {
                  icon: Globe,
                  title: isArabic ? "متعدد اللغات" : "Multilingual",
                  description: isArabic
                    ? "دعم كامل للعربية والإنجليزية"
                    : "Full support for Arabic and English"
                },
                {
                  icon: Shield,
                  title: isArabic ? "آمن ومتوافق" : "Secure & Compliant",
                  description: isArabic
                    ? "جميع البيانات محمية ومتوافقة مع المعايير"
                    : "All data protected and compliant with standards"
                }
              ].map((feature, index) => (
                <div key={index} className="bg-white/10 backdrop-blur-sm rounded-xl p-6">
                  <feature.icon className="w-8 h-8 mx-auto mb-3" />
                  <h4 className="font-semibold mb-2">{feature.title}</h4>
                  <p className="text-sm text-white/80">{feature.description}</p>
                </div>
              ))}
            </div>

            <Link href="/trial">
              <Button className="bg-white text-violet-600 hover:bg-gray-100 px-8 py-6 text-lg font-semibold rounded-xl shadow-lg">
                {isArabic ? "جرب التكاملات الآن" : "Try Integrations Now"}
                <Zap className="w-5 h-5 ml-2" />
              </Button>
            </Link>
          </div>
        </motion.div>
      </div>
    </section>
  )
}
