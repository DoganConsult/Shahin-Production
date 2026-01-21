"use client"

import { useEffect, useRef, useState } from "react"
import { motion } from "framer-motion"
import { BarChart3, RefreshCw, Maximize2, ExternalLink } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useTranslations } from "next-intl"

interface SupersetEmbedProps {
  dashboardId: string
  title?: string
  height?: string
  showToolbar?: boolean
  filters?: Record<string, string>
}

export function SupersetEmbed({
  dashboardId,
  title,
  height = "600px",
  showToolbar = true,
  filters = {},
}: SupersetEmbedProps) {
  const iframeRef = useRef<HTMLIFrameElement>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isFullscreen, setIsFullscreen] = useState(false)
  const t = useTranslations("dashboard")

  const displayTitle = title || t("analyticsBoard")
  const supersetUrl = process.env.NEXT_PUBLIC_SUPERSET_URL || "http://localhost:8088"

  // Build embed URL with filters
  const buildEmbedUrl = () => {
    const params = new URLSearchParams({
      standalone: "true",
      show_filters: "false",
      ...filters,
    })
    return `${supersetUrl}/superset/dashboard/${dashboardId}/?${params.toString()}`
  }

  const handleRefresh = () => {
    if (iframeRef.current) {
      setIsLoading(true)
      iframeRef.current.src = buildEmbedUrl()
    }
  }

  const handleFullscreen = () => {
    if (iframeRef.current) {
      if (iframeRef.current.requestFullscreen) {
        iframeRef.current.requestFullscreen()
        setIsFullscreen(true)
      }
    }
  }

  const handleOpenExternal = () => {
    window.open(`${supersetUrl}/superset/dashboard/${dashboardId}/`, "_blank")
  }

  useEffect(() => {
    document.addEventListener("fullscreenchange", () => {
      setIsFullscreen(!!document.fullscreenElement)
    })
  }, [])

  return (
    <Card className="overflow-hidden">
      {showToolbar && (
        <CardHeader className="flex flex-row items-center justify-between py-3 px-4 border-b">
          <CardTitle className="text-lg font-semibold flex items-center gap-2">
            <BarChart3 className="w-5 h-5 text-emerald-500" />
            {displayTitle}
          </CardTitle>
          <div className="flex items-center gap-2">
            <Button
              variant="ghost"
              size="sm"
              onClick={handleRefresh}
              className="h-8 w-8 p-0"
              title={t("refresh")}
            >
              <RefreshCw className={`w-4 h-4 ${isLoading ? "animate-spin" : ""}`} />
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={handleFullscreen}
              className="h-8 w-8 p-0"
              title={t("fullscreen")}
            >
              <Maximize2 className="w-4 h-4" />
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={handleOpenExternal}
              className="h-8 w-8 p-0"
              title={t("openNewWindow")}
            >
              <ExternalLink className="w-4 h-4" />
            </Button>
          </div>
        </CardHeader>
      )}
      <CardContent className="p-0 relative">
        {isLoading && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="absolute inset-0 flex items-center justify-center bg-gray-50 dark:bg-gray-800 z-10"
          >
            <div className="flex flex-col items-center gap-3">
              <div className="w-10 h-10 border-4 border-emerald-500 border-t-transparent rounded-full animate-spin" />
              <span className="text-sm text-gray-500">{t("loadingAnalytics")}</span>
            </div>
          </motion.div>
        )}
        <iframe
          ref={iframeRef}
          src={buildEmbedUrl()}
          width="100%"
          height={height}
          frameBorder="0"
          onLoad={() => setIsLoading(false)}
          className="w-full"
          title={displayTitle}
          allow="fullscreen"
        />
      </CardContent>
    </Card>
  )
}

// Grafana Embed Component
interface GrafanaEmbedProps {
  dashboardUid: string
  panelId?: number
  title?: string
  height?: string
  from?: string
  to?: string
  refresh?: string
}

export function GrafanaEmbed({
  dashboardUid,
  panelId,
  title,
  height = "400px",
  from = "now-24h",
  to = "now",
  refresh = "30s",
}: GrafanaEmbedProps) {
  const [isLoading, setIsLoading] = useState(true)
  const t = useTranslations("dashboard")
  const displayTitle = title || t("systemMonitoring")
  const grafanaUrl = process.env.NEXT_PUBLIC_GRAFANA_URL || "http://localhost:3030"

  const buildEmbedUrl = () => {
    const params = new URLSearchParams({
      orgId: "1",
      from,
      to,
      theme: "light",
      refresh,
    })

    if (panelId) {
      return `${grafanaUrl}/d-solo/${dashboardUid}?${params.toString()}&panelId=${panelId}`
    }
    return `${grafanaUrl}/d/${dashboardUid}?${params.toString()}&kiosk`
  }

  return (
    <Card className="overflow-hidden">
      <CardHeader className="py-3 px-4 border-b">
        <CardTitle className="text-lg font-semibold flex items-center gap-2">
          <BarChart3 className="w-5 h-5 text-blue-500" />
          {displayTitle}
        </CardTitle>
      </CardHeader>
      <CardContent className="p-0 relative">
        {isLoading && (
          <div className="absolute inset-0 flex items-center justify-center bg-gray-50 dark:bg-gray-800 z-10">
            <div className="w-8 h-8 border-4 border-blue-500 border-t-transparent rounded-full animate-spin" />
          </div>
        )}
        <iframe
          src={buildEmbedUrl()}
          width="100%"
          height={height}
          frameBorder="0"
          onLoad={() => setIsLoading(false)}
          className="w-full"
          title={displayTitle}
        />
      </CardContent>
    </Card>
  )
}

// Metabase Embed Component
interface MetabaseEmbedProps {
  questionId?: number
  dashboardId?: number
  title?: string
  height?: string
}

export function MetabaseEmbed({
  questionId,
  dashboardId,
  title,
  height = "500px",
}: MetabaseEmbedProps) {
  const [isLoading, setIsLoading] = useState(true)
  const t = useTranslations("dashboard")
  const displayTitle = title || t("analyticalReport")
  const metabaseUrl = process.env.NEXT_PUBLIC_METABASE_URL || "http://localhost:3033"

  const buildEmbedUrl = () => {
    if (dashboardId) {
      return `${metabaseUrl}/public/dashboard/${dashboardId}`
    }
    if (questionId) {
      return `${metabaseUrl}/public/question/${questionId}`
    }
    return metabaseUrl
  }

  return (
    <Card className="overflow-hidden">
      <CardHeader className="py-3 px-4 border-b">
        <CardTitle className="text-lg font-semibold">{displayTitle}</CardTitle>
      </CardHeader>
      <CardContent className="p-0 relative">
        {isLoading && (
          <div className="absolute inset-0 flex items-center justify-center bg-gray-50 dark:bg-gray-800 z-10">
            <div className="w-8 h-8 border-4 border-purple-500 border-t-transparent rounded-full animate-spin" />
          </div>
        )}
        <iframe
          src={buildEmbedUrl()}
          width="100%"
          height={height}
          frameBorder="0"
          onLoad={() => setIsLoading(false)}
          className="w-full"
          title={displayTitle}
        />
      </CardContent>
    </Card>
  )
}
