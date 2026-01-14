"use client"

import * as React from "react"
import * as Dialog from "@radix-ui/react-dialog"
import { cn } from "@/lib/utils"
import {
  Bell,
  X,
  Check,
  CheckCheck,
  AlertTriangle,
  AlertCircle,
  Info,
  CheckCircle,
  Clock,
  Trash2,
  Settings,
  ExternalLink,
} from "lucide-react"
import { Button } from "./button"

/**
 * Notification Center Component - Design System 2.0
 *
 * Displays notifications with filtering by category.
 * Supports mark as read, dismiss, and action buttons.
 *
 * @example
 * <NotificationCenter
 *   notifications={notifications}
 *   onMarkAsRead={handleMarkAsRead}
 *   onDismiss={handleDismiss}
 * />
 */

// Types
export type NotificationType = "alert" | "warning" | "info" | "success" | "task"
export type NotificationPriority = "high" | "medium" | "low"

export interface Notification {
  id: string
  type: NotificationType
  priority?: NotificationPriority
  title: string
  titleAr?: string
  message: string
  messageAr?: string
  timestamp: Date | string
  read: boolean
  actionUrl?: string
  actionLabel?: string
  actionLabelAr?: string
  metadata?: Record<string, unknown>
}

export interface NotificationCenterProps {
  /** Array of notifications to display */
  notifications: Notification[]
  /** Callback when notification is marked as read */
  onMarkAsRead?: (id: string) => void
  /** Callback when all notifications are marked as read */
  onMarkAllAsRead?: () => void
  /** Callback when notification is dismissed */
  onDismiss?: (id: string) => void
  /** Callback when all notifications are cleared */
  onClearAll?: () => void
  /** Callback when settings is clicked */
  onSettingsClick?: () => void
  /** Custom class name */
  className?: string
  /** Show as a slide-over panel instead of dropdown */
  variant?: "dropdown" | "panel"
}

// Tab types
type TabType = "all" | "alerts" | "tasks" | "updates"

// Tab config
const tabs: { id: TabType; labelEn: string; labelAr: string }[] = [
  { id: "all", labelEn: "All", labelAr: "الكل" },
  { id: "alerts", labelEn: "Alerts", labelAr: "التنبيهات" },
  { id: "tasks", labelEn: "Tasks", labelAr: "المهام" },
  { id: "updates", labelEn: "Updates", labelAr: "التحديثات" },
]

// Icon mapping
const typeIcons: Record<NotificationType, React.ReactNode> = {
  alert: <AlertCircle className="h-4 w-4" />,
  warning: <AlertTriangle className="h-4 w-4" />,
  info: <Info className="h-4 w-4" />,
  success: <CheckCircle className="h-4 w-4" />,
  task: <Clock className="h-4 w-4" />,
}

// Color mapping
const typeColors: Record<NotificationType, string> = {
  alert: "text-red-500 bg-red-500/10",
  warning: "text-amber-500 bg-amber-500/10",
  info: "text-blue-500 bg-blue-500/10",
  success: "text-emerald-500 bg-emerald-500/10",
  task: "text-purple-500 bg-purple-500/10",
}

// Priority indicator
const priorityColors: Record<NotificationPriority, string> = {
  high: "bg-red-500",
  medium: "bg-amber-500",
  low: "bg-gray-400",
}

// Format relative time
function formatRelativeTime(date: Date | string, isArabic: boolean): string {
  const now = new Date()
  const then = new Date(date)
  const diffMs = now.getTime() - then.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMs / 3600000)
  const diffDays = Math.floor(diffMs / 86400000)

  if (diffMins < 1) return isArabic ? "الآن" : "Just now"
  if (diffMins < 60)
    return isArabic ? `منذ ${diffMins} دقيقة` : `${diffMins}m ago`
  if (diffHours < 24)
    return isArabic ? `منذ ${diffHours} ساعة` : `${diffHours}h ago`
  if (diffDays < 7)
    return isArabic ? `منذ ${diffDays} يوم` : `${diffDays}d ago`

  return then.toLocaleDateString(isArabic ? "ar-SA" : "en-US", {
    month: "short",
    day: "numeric",
  })
}

// Notification Item Component
const NotificationItem = React.memo(function NotificationItem({
  notification,
  onMarkAsRead,
  onDismiss,
  isArabic,
}: {
  notification: Notification
  onMarkAsRead?: (id: string) => void
  onDismiss?: (id: string) => void
  isArabic: boolean
}) {
  const title =
    isArabic && notification.titleAr ? notification.titleAr : notification.title
  const message =
    isArabic && notification.messageAr
      ? notification.messageAr
      : notification.message
  const actionLabel =
    isArabic && notification.actionLabelAr
      ? notification.actionLabelAr
      : notification.actionLabel

  return (
    <div
      className={cn(
        "relative px-4 py-3 border-b border-border last:border-b-0",
        "transition-colors hover:bg-muted/50",
        !notification.read && "bg-primary/5"
      )}
    >
      {/* Unread indicator */}
      {!notification.read && (
        <div className="absolute top-4 start-1.5 w-2 h-2 rounded-full bg-primary" />
      )}

      <div className="flex gap-3">
        {/* Icon */}
        <div
          className={cn(
            "flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center",
            typeColors[notification.type]
          )}
        >
          {typeIcons[notification.type]}
        </div>

        {/* Content */}
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2">
            <div className="flex items-center gap-2">
              {/* Priority indicator */}
              {notification.priority && (
                <span
                  className={cn(
                    "w-1.5 h-1.5 rounded-full flex-shrink-0",
                    priorityColors[notification.priority]
                  )}
                  title={notification.priority}
                />
              )}
              <h4
                className={cn(
                  "text-sm font-medium text-foreground line-clamp-1",
                  !notification.read && "font-semibold"
                )}
              >
                {title}
              </h4>
            </div>
            <span className="text-xs text-muted-foreground whitespace-nowrap">
              {formatRelativeTime(notification.timestamp, isArabic)}
            </span>
          </div>

          <p className="text-sm text-muted-foreground mt-0.5 line-clamp-2">
            {message}
          </p>

          {/* Actions */}
          <div className="flex items-center gap-2 mt-2">
            {notification.actionUrl && (
              <a
                href={notification.actionUrl}
                className={cn(
                  "inline-flex items-center gap-1 text-xs font-medium",
                  "text-primary hover:text-primary/80 transition-colors"
                )}
              >
                {actionLabel || (isArabic ? "عرض" : "View")}
                <ExternalLink className="h-3 w-3" />
              </a>
            )}

            {!notification.read && onMarkAsRead && (
              <button
                onClick={() => onMarkAsRead(notification.id)}
                className={cn(
                  "inline-flex items-center gap-1 text-xs",
                  "text-muted-foreground hover:text-foreground transition-colors"
                )}
              >
                <Check className="h-3 w-3" />
                {isArabic ? "تحديد كمقروء" : "Mark read"}
              </button>
            )}

            {onDismiss && (
              <button
                onClick={() => onDismiss(notification.id)}
                className={cn(
                  "inline-flex items-center gap-1 text-xs",
                  "text-muted-foreground hover:text-destructive transition-colors"
                )}
              >
                <Trash2 className="h-3 w-3" />
                {isArabic ? "حذف" : "Dismiss"}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  )
})

// Notification Bell Button Component
export const NotificationBell = React.memo(function NotificationBell({
  unreadCount = 0,
  onClick,
  className,
}: {
  unreadCount?: number
  onClick?: () => void
  className?: string
}) {
  return (
    <button
      onClick={onClick}
      className={cn(
        "relative p-2 rounded-lg transition-colors",
        "text-muted-foreground hover:text-foreground hover:bg-muted",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
        className
      )}
      aria-label={`Notifications${unreadCount > 0 ? ` (${unreadCount} unread)` : ""}`}
    >
      <Bell className="h-5 w-5" />
      {unreadCount > 0 && (
        <span
          className={cn(
            "absolute -top-0.5 -end-0.5 flex items-center justify-center",
            "min-w-[18px] h-[18px] px-1 rounded-full",
            "bg-destructive text-destructive-foreground text-xs font-medium"
          )}
        >
          {unreadCount > 99 ? "99+" : unreadCount}
        </span>
      )}
    </button>
  )
})

// Main Component
function NotificationCenterComponent({
  notifications,
  onMarkAsRead,
  onMarkAllAsRead,
  onDismiss,
  onClearAll,
  onSettingsClick,
  className,
  variant = "dropdown",
}: NotificationCenterProps) {
  const [open, setOpen] = React.useState(false)
  const [activeTab, setActiveTab] = React.useState<TabType>("all")

  // Detect Arabic language
  const isArabic =
    typeof document !== "undefined" &&
    document.documentElement.lang === "ar"

  // Filter notifications by tab
  const filteredNotifications = React.useMemo(() => {
    switch (activeTab) {
      case "alerts":
        return notifications.filter(
          (n) => n.type === "alert" || n.type === "warning"
        )
      case "tasks":
        return notifications.filter((n) => n.type === "task")
      case "updates":
        return notifications.filter(
          (n) => n.type === "info" || n.type === "success"
        )
      default:
        return notifications
    }
  }, [notifications, activeTab])

  // Count unread notifications
  const unreadCount = React.useMemo(
    () => notifications.filter((n) => !n.read).length,
    [notifications]
  )

  // Count per tab
  const tabCounts = React.useMemo(() => {
    return {
      all: notifications.length,
      alerts: notifications.filter(
        (n) => n.type === "alert" || n.type === "warning"
      ).length,
      tasks: notifications.filter((n) => n.type === "task").length,
      updates: notifications.filter(
        (n) => n.type === "info" || n.type === "success"
      ).length,
    }
  }, [notifications])

  const content = (
    <div className={cn("flex flex-col h-full", className)}>
      {/* Header */}
      <div className="flex items-center justify-between px-4 py-3 border-b border-border">
        <div className="flex items-center gap-2">
          <Bell className="h-5 w-5 text-foreground" />
          <h2 className="font-semibold text-foreground">
            {isArabic ? "الإشعارات" : "Notifications"}
          </h2>
          {unreadCount > 0 && (
            <span className="px-2 py-0.5 text-xs font-medium rounded-full bg-primary/10 text-primary">
              {unreadCount}
            </span>
          )}
        </div>
        <div className="flex items-center gap-1">
          {onMarkAllAsRead && unreadCount > 0 && (
            <button
              onClick={onMarkAllAsRead}
              className={cn(
                "p-1.5 rounded text-muted-foreground hover:text-foreground",
                "hover:bg-muted transition-colors"
              )}
              title={isArabic ? "تحديد الكل كمقروء" : "Mark all as read"}
            >
              <CheckCheck className="h-4 w-4" />
            </button>
          )}
          {onSettingsClick && (
            <button
              onClick={onSettingsClick}
              className={cn(
                "p-1.5 rounded text-muted-foreground hover:text-foreground",
                "hover:bg-muted transition-colors"
              )}
              title={isArabic ? "الإعدادات" : "Settings"}
            >
              <Settings className="h-4 w-4" />
            </button>
          )}
          {variant === "panel" && (
            <Dialog.Close asChild>
              <button
                className={cn(
                  "p-1.5 rounded text-muted-foreground hover:text-foreground",
                  "hover:bg-muted transition-colors"
                )}
              >
                <X className="h-4 w-4" />
              </button>
            </Dialog.Close>
          )}
        </div>
      </div>

      {/* Tabs */}
      <div className="flex border-b border-border">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={cn(
              "flex-1 px-3 py-2 text-sm font-medium transition-colors",
              "border-b-2 -mb-px",
              activeTab === tab.id
                ? "border-primary text-primary"
                : "border-transparent text-muted-foreground hover:text-foreground"
            )}
          >
            {isArabic ? tab.labelAr : tab.labelEn}
            {tabCounts[tab.id] > 0 && (
              <span className="ms-1 text-xs text-muted-foreground">
                ({tabCounts[tab.id]})
              </span>
            )}
          </button>
        ))}
      </div>

      {/* Notifications List */}
      <div className="flex-1 overflow-y-auto">
        {filteredNotifications.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-12 px-4 text-center">
            <div className="w-12 h-12 rounded-full bg-muted flex items-center justify-center mb-3">
              <Bell className="h-6 w-6 text-muted-foreground" />
            </div>
            <p className="text-sm font-medium text-foreground">
              {isArabic ? "لا توجد إشعارات" : "No notifications"}
            </p>
            <p className="text-xs text-muted-foreground mt-1">
              {isArabic
                ? "ستظهر هنا عند وصولها"
                : "They'll appear here when you get them"}
            </p>
          </div>
        ) : (
          filteredNotifications.map((notification) => (
            <NotificationItem
              key={notification.id}
              notification={notification}
              onMarkAsRead={onMarkAsRead}
              onDismiss={onDismiss}
              isArabic={isArabic}
            />
          ))
        )}
      </div>

      {/* Footer */}
      {filteredNotifications.length > 0 && onClearAll && (
        <div className="px-4 py-3 border-t border-border">
          <Button
            variant="ghost"
            size="sm"
            className="w-full text-muted-foreground"
            onClick={onClearAll}
          >
            <Trash2 className="h-4 w-4 me-2" />
            {isArabic ? "مسح الكل" : "Clear all"}
          </Button>
        </div>
      )}
    </div>
  )

  if (variant === "dropdown") {
    return (
      <Dialog.Root open={open} onOpenChange={setOpen}>
        <Dialog.Trigger asChild>
          <NotificationBell unreadCount={unreadCount} />
        </Dialog.Trigger>
        <Dialog.Portal>
          <Dialog.Overlay className="fixed inset-0 z-40" onClick={() => setOpen(false)} />
          <Dialog.Content
            className={cn(
              "fixed z-50 w-[380px] max-h-[70vh]",
              "top-14 end-4",
              "bg-background rounded-xl shadow-2xl border border-border",
              "data-[state=open]:animate-in data-[state=closed]:animate-out",
              "data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0",
              "data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95",
              "duration-200"
            )}
          >
            {content}
          </Dialog.Content>
        </Dialog.Portal>
      </Dialog.Root>
    )
  }

  // Panel variant
  return (
    <Dialog.Root open={open} onOpenChange={setOpen}>
      <Dialog.Trigger asChild>
        <NotificationBell unreadCount={unreadCount} />
      </Dialog.Trigger>
      <Dialog.Portal>
        <Dialog.Overlay
          className={cn(
            "fixed inset-0 z-40 bg-black/50 backdrop-blur-sm",
            "data-[state=open]:animate-in data-[state=closed]:animate-out",
            "data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0"
          )}
        />
        <Dialog.Content
          className={cn(
            "fixed z-50 inset-y-0 end-0 w-full max-w-md",
            "bg-background shadow-2xl border-s border-border",
            "data-[state=open]:animate-in data-[state=closed]:animate-out",
            "data-[state=closed]:slide-out-to-right data-[state=open]:slide-in-from-right",
            "duration-300"
          )}
        >
          {content}
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  )
}

const NotificationCenter = React.memo(NotificationCenterComponent)

export { NotificationCenter }
export default NotificationCenter
