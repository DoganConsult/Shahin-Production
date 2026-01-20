"use client"

import { useState, useEffect, useRef } from "react"
import { motion, AnimatePresence } from "framer-motion"
import {
  Bot,
  X,
  Send,
  Minimize2,
  Maximize2,
  Loader2,
  Sparkles,
  MessageSquare,
  ChevronDown,
  ChevronUp,
  Brain
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { useLocale } from "@/components/providers/locale-provider"

interface AIMessage {
  id: string
  role: "user" | "assistant"
  content: string
  timestamp: Date
  provider?: "azure" | "copilot" | "claude" | "shahin"
}

interface AIProvider {
  id: "azure" | "copilot" | "claude" | "shahin"
  name: { en: string; ar: string }
  icon: typeof Bot
  color: string
  enabled: boolean
}

// Configuration from environment variables (no hardcoding)
const getAIConfig = () => {
  const config = {
    enabled: process.env.NEXT_PUBLIC_AI_ASSISTANT_ENABLED === "true",
    apiEndpoint: process.env.NEXT_PUBLIC_AI_API_ENDPOINT || "/api/ai/chat",
    providers: {
      azure: {
        enabled: process.env.NEXT_PUBLIC_AZURE_BOT_ENABLED === "true",
        name: process.env.NEXT_PUBLIC_AZURE_BOT_NAME || "Azure Bot"
      },
      copilot: {
        enabled: process.env.NEXT_PUBLIC_COPILOT_ENABLED === "true",
        name: process.env.NEXT_PUBLIC_COPILOT_NAME || "Microsoft Copilot"
      },
      claude: {
        enabled: process.env.NEXT_PUBLIC_CLAUDE_ENABLED === "true",
        name: process.env.NEXT_PUBLIC_CLAUDE_NAME || "Claude AI"
      },
      shahin: {
        enabled: process.env.NEXT_PUBLIC_SHAHIN_AI_ENABLED !== "false", // Default enabled
        name: process.env.NEXT_PUBLIC_SHAHIN_AI_NAME || "Shahin AI"
      }
    },
    defaultProvider: (process.env.NEXT_PUBLIC_DEFAULT_AI_PROVIDER || "shahin") as "azure" | "copilot" | "claude" | "shahin",
    position: process.env.NEXT_PUBLIC_AI_ASSISTANT_POSITION || "bottom-right", // bottom-right, bottom-left, top-right, top-left
    initialHeight: parseInt(process.env.NEXT_PUBLIC_AI_ASSISTANT_HEIGHT || "600"),
    initialWidth: parseInt(process.env.NEXT_PUBLIC_AI_ASSISTANT_WIDTH || "400")
  }
  return config
}

const providers: AIProvider[] = [
  {
    id: "shahin",
    name: { en: "Shahin AI", ar: "شاهين الذكي" },
    icon: Bot,
    color: "violet",
    enabled: true
  },
  {
    id: "azure",
    name: { en: "Azure Bot", ar: "بوت Azure" },
    icon: Bot,
    color: "blue",
    enabled: false
  },
  {
    id: "copilot",
    name: { en: "Microsoft Copilot", ar: "Microsoft Copilot" },
    icon: Sparkles,
    color: "cyan",
    enabled: false
  },
  {
    id: "claude",
    name: { en: "Claude AI", ar: "Claude AI" },
    icon: Brain,
    color: "emerald",
    enabled: false
  }
]

export function FloatingAIAssistant() {
  const [isOpen, setIsOpen] = useState(false)
  const [isMinimized, setIsMinimized] = useState(false)
  const [messages, setMessages] = useState<AIMessage[]>([])
  const [input, setInput] = useState("")
  const [isLoading, setIsLoading] = useState(false)
  const [selectedProvider, setSelectedProvider] = useState<"azure" | "copilot" | "claude" | "shahin">("shahin")
  const messagesEndRef = useRef<HTMLDivElement>(null)
  const { locale } = useLocale()
  const isArabic = locale === "ar"
  const config = getAIConfig()

  // Filter enabled providers based on config (must be before hooks)
  const enabledProviders = providers.filter(p => {
    if (p.id === "shahin") return config.providers.shahin.enabled
    if (p.id === "azure") return config.providers.azure.enabled
    if (p.id === "copilot") return config.providers.copilot.enabled
    if (p.id === "claude") return config.providers.claude.enabled
    return false
  })

  // Set default provider from config
  useEffect(() => {
    if (enabledProviders.some(p => p.id === config.defaultProvider)) {
      setSelectedProvider(config.defaultProvider)
    } else if (enabledProviders.length > 0) {
      setSelectedProvider(enabledProviders[0].id as any)
    }
  }, [config.defaultProvider, enabledProviders])

  // Auto-scroll to bottom
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" })
  }, [messages])

  // Initialize with welcome message
  useEffect(() => {
    if (isOpen && messages.length === 0) {
      const welcomeMessage: AIMessage = {
        id: "welcome",
        role: "assistant",
        content: isArabic
          ? "مرحباً! أنا مساعدك الذكي. كيف يمكنني مساعدتك في عملية الإعداد؟"
          : "Hello! I'm your AI assistant. How can I help you with the onboarding process?",
        timestamp: new Date(),
        provider: selectedProvider
      }
      setMessages([welcomeMessage])
    }
  }, [isOpen, isArabic, selectedProvider, messages.length])

  // Check if AI assistant is enabled (after hooks)
  if (!config.enabled) {
    return null
  }

  const handleSend = async () => {
    if (!input.trim() || isLoading) return

    const userMessage: AIMessage = {
      id: Date.now().toString(),
      role: "user",
      content: input,
      timestamp: new Date()
    }

    setMessages(prev => [...prev, userMessage])
    setInput("")
    setIsLoading(true)

    try {
      // Call API endpoint (configurable)
      const response = await fetch(config.apiEndpoint, {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          message: input,
          provider: selectedProvider,
          context: {
            page: "onboarding",
            locale: locale,
            stage: window.location.pathname
          }
        })
      })

      const data = await response.json()

      const assistantMessage: AIMessage = {
        id: (Date.now() + 1).toString(),
        role: "assistant",
        content: data.message || (isArabic ? "عذراً، حدث خطأ. يرجى المحاولة مرة أخرى." : "Sorry, an error occurred. Please try again."),
        timestamp: new Date(),
        provider: selectedProvider
      }

      setMessages(prev => [...prev, assistantMessage])
    } catch (error) {
      console.error("AI Assistant error:", error)
      const errorMessage: AIMessage = {
        id: (Date.now() + 1).toString(),
        role: "assistant",
        content: isArabic
          ? "عذراً، لا يمكنني الاتصال بالخادم الآن. يرجى المحاولة لاحقاً."
          : "Sorry, I can't connect to the server right now. Please try again later.",
        timestamp: new Date(),
        provider: selectedProvider
      }
      setMessages(prev => [...prev, errorMessage])
    } finally {
      setIsLoading(false)
    }
  }

  const currentProvider = providers.find(p => p.id === selectedProvider)

  // Position classes based on config
  const positionClasses = {
    "bottom-right": "bottom-6 right-6",
    "bottom-left": "bottom-6 left-6",
    "top-right": "top-6 right-6",
    "top-left": "top-6 left-6"
  }

  return (
    <>
      {/* Floating Button */}
      {!isOpen && (
        <motion.button
          initial={{ scale: 0 }}
          animate={{ scale: 1 }}
          whileHover={{ scale: 1.1 }}
          whileTap={{ scale: 0.9 }}
          onClick={() => setIsOpen(true)}
          className={`fixed ${positionClasses[config.position as keyof typeof positionClasses]} z-50 w-16 h-16 rounded-full bg-gradient-to-br from-violet-600 to-purple-600 text-white shadow-2xl shadow-violet-500/50 flex items-center justify-center hover:shadow-violet-500/75 transition-all`}
          aria-label={isArabic ? "فتح المساعد الذكي" : "Open AI Assistant"}
        >
          <Bot className="w-7 h-7" />
          <span className="absolute -top-1 -right-1 w-4 h-4 bg-emerald-500 rounded-full border-2 border-white animate-pulse" />
        </motion.button>
      )}

      {/* Chat Window */}
      <AnimatePresence>
        {isOpen && (
          <motion.div
            initial={{ opacity: 0, scale: 0.8, y: 20 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.8, y: 20 }}
            className={`fixed ${positionClasses[config.position as keyof typeof positionClasses]} z-50 ${isMinimized ? "h-16" : ""}`}
            style={{
              width: isMinimized ? config.initialWidth : `${config.initialWidth}px`,
              height: isMinimized ? "64px" : `${config.initialHeight}px`,
              maxWidth: "calc(100vw - 3rem)",
              maxHeight: "calc(100vh - 3rem)"
            }}
          >
            <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 flex flex-col h-full overflow-hidden">
              {/* Header */}
              <div className="flex items-center justify-between p-4 border-b border-gray-200 dark:border-gray-700 bg-gradient-to-r from-violet-600 to-purple-600 text-white">
                <div className="flex items-center gap-3">
                  {currentProvider && (
                    <div className={`w-10 h-10 rounded-full bg-white/20 flex items-center justify-center`}>
                      <currentProvider.icon className="w-5 h-5" />
                    </div>
                  )}
                  <div>
                    <h3 className="font-semibold text-sm">
                      {isArabic ? currentProvider?.name.ar : currentProvider?.name.en}
                    </h3>
                    <p className="text-xs text-white/80">
                      {isArabic ? "مساعد ذكي" : "AI Assistant"}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  {/* Provider Selector (if multiple enabled) */}
                  {enabledProviders.length > 1 && !isMinimized && (
                    <select
                      value={selectedProvider}
                      onChange={(e) => setSelectedProvider(e.target.value as any)}
                      className="text-xs bg-white/20 text-white rounded-lg px-2 py-1 border border-white/30 focus:outline-none"
                    >
                      {enabledProviders.map(provider => (
                        <option key={provider.id} value={provider.id} className="bg-gray-800 text-white">
                          {isArabic ? provider.name.ar : provider.name.en}
                        </option>
                      ))}
                    </select>
                  )}
                  <button
                    onClick={() => setIsMinimized(!isMinimized)}
                    className="p-1.5 hover:bg-white/20 rounded-lg transition-colors"
                    aria-label={isMinimized ? (isArabic ? "تكبير" : "Maximize") : (isArabic ? "تصغير" : "Minimize")}
                  >
                    {isMinimized ? <Maximize2 className="w-4 h-4" /> : <Minimize2 className="w-4 h-4" />}
                  </button>
                  <button
                    onClick={() => {
                      setIsOpen(false)
                      setIsMinimized(false)
                    }}
                    className="p-1.5 hover:bg-white/20 rounded-lg transition-colors"
                    aria-label={isArabic ? "إغلاق" : "Close"}
                  >
                    <X className="w-4 h-4" />
                  </button>
                </div>
              </div>

              {/* Messages */}
              {!isMinimized && (
                <>
                  <div className="flex-1 overflow-y-auto p-4 space-y-4 bg-gray-50 dark:bg-gray-900/50">
                    {messages.map((msg) => (
                      <div
                        key={msg.id}
                        className={`flex ${msg.role === "user" ? "justify-end" : "justify-start"}`}
                      >
                        <div
                          className={`max-w-[80%] rounded-2xl px-4 py-2 ${
                            msg.role === "user"
                              ? "bg-violet-600 text-white rounded-br-none"
                              : "bg-white dark:bg-gray-800 text-gray-900 dark:text-white border border-gray-200 dark:border-gray-700 rounded-bl-none"
                          }`}
                        >
                          <p className="text-sm whitespace-pre-wrap">{msg.content}</p>
                          <p className="text-xs mt-1 opacity-70">
                            {msg.timestamp.toLocaleTimeString(isArabic ? "ar-SA" : "en-US", {
                              hour: "2-digit",
                              minute: "2-digit"
                            })}
                          </p>
                        </div>
                      </div>
                    ))}
                    {isLoading && (
                      <div className="flex justify-start">
                        <div className="bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-2xl rounded-bl-none px-4 py-2">
                          <Loader2 className="w-4 h-4 animate-spin text-violet-600" />
                        </div>
                      </div>
                    )}
                    <div ref={messagesEndRef} />
                  </div>

                  {/* Input */}
                  <div className="p-4 border-t border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
                    <div className="flex gap-2">
                      <input
                        type="text"
                        value={input}
                        onChange={(e) => setInput(e.target.value)}
                        onKeyPress={(e) => e.key === "Enter" && !e.shiftKey && handleSend()}
                        placeholder={isArabic ? "اكتب رسالتك..." : "Type your message..."}
                        className="flex-1 px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-violet-500"
                        disabled={isLoading}
                      />
                      <Button
                        onClick={handleSend}
                        disabled={!input.trim() || isLoading}
                        className="bg-violet-600 hover:bg-violet-700 text-white px-4"
                      >
                        {isLoading ? (
                          <Loader2 className="w-4 h-4 animate-spin" />
                        ) : (
                          <Send className="w-4 h-4" />
                        )}
                      </Button>
                    </div>
                    <p className="text-xs text-gray-500 dark:text-gray-400 mt-2 text-center">
                      {isArabic
                        ? "المساعد متاح في جميع مراحل الإعداد"
                        : "Assistant available across all onboarding stages"}
                    </p>
                  </div>
                </>
              )}
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  )
}
