"use client"

import * as React from "react"
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

/**
 * Badge Component - Design System 2.0
 *
 * A versatile badge component for status indicators, tags, and labels.
 * Performance optimized with React.memo.
 *
 * @example
 * <Badge variant="success">Active</Badge>
 * <Badge variant="warning" dot>Pending</Badge>
 * <Badge variant="critical" removable onRemove={() => {}}>Alert</Badge>
 */

const badgeVariants = cva(
  [
    "inline-flex items-center gap-1.5 font-medium",
    "transition-colors duration-150",
    "focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2",
  ].join(" "),
  {
    variants: {
      variant: {
        // Primary
        default: "bg-primary text-primary-foreground shadow-sm hover:bg-primary/90",
        // Secondary
        secondary: "bg-secondary text-secondary-foreground hover:bg-secondary/80",
        // Destructive
        destructive: "bg-destructive text-destructive-foreground shadow-sm hover:bg-destructive/90",
        // Outline
        outline: "border border-input bg-background text-foreground hover:bg-accent",
        // Success - Green
        success: [
          "bg-emerald-100 text-emerald-800 border border-emerald-200",
          "dark:bg-emerald-900/30 dark:text-emerald-400 dark:border-emerald-800",
        ].join(" "),
        // Warning - Amber/Orange
        warning: [
          "bg-amber-100 text-amber-800 border border-amber-200",
          "dark:bg-amber-900/30 dark:text-amber-400 dark:border-amber-800",
        ].join(" "),
        // Info - Blue
        info: [
          "bg-sky-100 text-sky-800 border border-sky-200",
          "dark:bg-sky-900/30 dark:text-sky-400 dark:border-sky-800",
        ].join(" "),
        // Critical - Red
        critical: [
          "bg-red-100 text-red-800 border border-red-200",
          "dark:bg-red-900/30 dark:text-red-400 dark:border-red-800",
        ].join(" "),
        // Purple
        purple: [
          "bg-purple-100 text-purple-800 border border-purple-200",
          "dark:bg-purple-900/30 dark:text-purple-400 dark:border-purple-800",
        ].join(" "),
        // Neutral
        neutral: [
          "bg-gray-100 text-gray-800 border border-gray-200",
          "dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700",
        ].join(" "),
      },
      size: {
        sm: "text-xs px-2 py-0.5 rounded-md",
        default: "text-xs px-2.5 py-0.5 rounded-full",
        lg: "text-sm px-3 py-1 rounded-full",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  }
)

export interface BadgeProps
  extends React.HTMLAttributes<HTMLSpanElement>,
    VariantProps<typeof badgeVariants> {
  /** Shows a status dot before the badge text */
  dot?: boolean
  /** Color for the status dot (defaults to variant color) */
  dotColor?: "success" | "warning" | "error" | "info" | "neutral"
  /** Makes the badge removable with an X button */
  removable?: boolean
  /** Callback when the remove button is clicked */
  onRemove?: () => void
  /** Icon to display before the badge text */
  icon?: React.ReactNode
}

/**
 * Status Dot Component
 */
const StatusDot = React.memo(function StatusDot({
  color,
  variant,
}: {
  color?: BadgeProps["dotColor"]
  variant?: BadgeProps["variant"]
}) {
  const dotColorClass = React.useMemo(() => {
    if (color) {
      switch (color) {
        case "success":
          return "bg-emerald-500"
        case "warning":
          return "bg-amber-500"
        case "error":
          return "bg-red-500"
        case "info":
          return "bg-sky-500"
        case "neutral":
          return "bg-gray-400"
      }
    }

    // Default to variant-based color
    switch (variant) {
      case "success":
        return "bg-emerald-500"
      case "warning":
        return "bg-amber-500"
      case "critical":
      case "destructive":
        return "bg-red-500"
      case "info":
        return "bg-sky-500"
      case "purple":
        return "bg-purple-500"
      default:
        return "bg-current"
    }
  }, [color, variant])

  return (
    <span
      className={cn("h-1.5 w-1.5 rounded-full shrink-0", dotColorClass)}
      aria-hidden="true"
    />
  )
})

/**
 * Remove Button Component
 */
const RemoveButton = React.memo(function RemoveButton({
  onClick,
  label,
}: {
  onClick?: () => void
  label: string
}) {
  return (
    <button
      type="button"
      onClick={(e) => {
        e.stopPropagation()
        onClick?.()
      }}
      className={cn(
        "ml-0.5 -mr-1 h-4 w-4 rounded-full",
        "inline-flex items-center justify-center",
        "hover:bg-black/10 dark:hover:bg-white/20",
        "focus:outline-none focus:ring-1 focus:ring-current",
        "transition-colors"
      )}
      aria-label={`Remove ${label}`}
    >
      <svg
        className="h-3 w-3"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
        strokeWidth={2}
        aria-hidden="true"
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          d="M6 18L18 6M6 6l12 12"
        />
      </svg>
    </button>
  )
})

/**
 * Badge Component
 */
const BadgeComponent = React.forwardRef<HTMLSpanElement, BadgeProps>(
  (
    {
      className,
      variant,
      size,
      dot = false,
      dotColor,
      removable = false,
      onRemove,
      icon,
      children,
      ...props
    },
    ref
  ) => {
    // Memoize className computation
    const computedClassName = React.useMemo(
      () => cn(badgeVariants({ variant, size }), className),
      [variant, size, className]
    )

    // Get text content for aria-label
    const textContent =
      typeof children === "string"
        ? children
        : "badge"

    return (
      <span ref={ref} className={computedClassName} {...props}>
        {dot && <StatusDot color={dotColor} variant={variant} />}
        {icon && <span className="shrink-0">{icon}</span>}
        {children}
        {removable && <RemoveButton onClick={onRemove} label={textContent} />}
      </span>
    )
  }
)

BadgeComponent.displayName = "Badge"

// Wrap with React.memo for performance optimization
const Badge = React.memo(BadgeComponent)

export { Badge, StatusDot, badgeVariants }
