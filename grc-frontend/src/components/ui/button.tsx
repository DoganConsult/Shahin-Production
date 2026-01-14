"use client"

import * as React from "react"
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

/**
 * Button Component - Design System 2.0
 *
 * A fully accessible, performance-optimized button component with multiple variants.
 * Supports loading states, icons, and ARIA labels.
 *
 * @example
 * <Button variant="primary" size="lg">Click me</Button>
 * <Button variant="destructive" isLoading>Deleting...</Button>
 * <Button variant="ghost" size="icon" aria-label="Settings"><SettingsIcon /></Button>
 */

const buttonVariants = cva(
  [
    // Base styles
    "inline-flex items-center justify-center gap-2",
    "whitespace-nowrap rounded-lg font-medium",
    "transition-all",
    // Focus styles (Accessibility)
    "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background",
    // Disabled styles
    "disabled:pointer-events-none disabled:opacity-50",
    // Active state
    "active:scale-[0.98]",
  ].join(" "),
  {
    variants: {
      variant: {
        // Primary - Main CTA
        default: [
          "bg-primary text-primary-foreground",
          "shadow-sm hover:bg-primary/90",
          "hover:shadow-md",
        ].join(" "),

        // Destructive - Delete, Remove actions
        destructive: [
          "bg-destructive text-destructive-foreground",
          "shadow-sm hover:bg-destructive/90",
        ].join(" "),

        // Outline - Secondary actions
        outline: [
          "border border-input bg-background",
          "shadow-sm hover:bg-accent hover:text-accent-foreground",
          "hover:border-accent",
        ].join(" "),

        // Secondary - Less prominent actions
        secondary: [
          "bg-secondary text-secondary-foreground",
          "shadow-sm hover:bg-secondary/80",
        ].join(" "),

        // Ghost - Minimal styling
        ghost: [
          "hover:bg-accent hover:text-accent-foreground",
        ].join(" "),

        // Link - Text-only button
        link: [
          "text-primary underline-offset-4 hover:underline",
          "p-0 h-auto",
        ].join(" "),

        // Gradient - Premium CTAs
        gradient: [
          "bg-gradient-to-r from-emerald-500 to-teal-600",
          "text-white shadow-lg",
          "hover:shadow-xl hover:scale-[1.02]",
          "hover:from-emerald-600 hover:to-teal-700",
        ].join(" "),

        // Success - Positive actions
        success: [
          "bg-emerald-600 text-white",
          "shadow-sm hover:bg-emerald-700",
        ].join(" "),

        // Warning - Caution actions
        warning: [
          "bg-amber-500 text-white",
          "shadow-sm hover:bg-amber-600",
        ].join(" "),

        // Info - Informational actions
        info: [
          "bg-sky-500 text-white",
          "shadow-sm hover:bg-sky-600",
        ].join(" "),
      },
      size: {
        xs: "h-7 px-2.5 text-xs rounded-md",
        sm: "h-8 px-3 text-sm rounded-md",
        default: "h-10 px-4 text-sm",
        lg: "h-11 px-6 text-base",
        xl: "h-12 px-8 text-base font-semibold",
        icon: "h-10 w-10 p-0",
        "icon-sm": "h-8 w-8 p-0 rounded-md",
        "icon-lg": "h-12 w-12 p-0",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  }
)

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  /** Shows a loading spinner and disables the button */
  isLoading?: boolean
  /** Icon to display before the button text */
  leftIcon?: React.ReactNode
  /** Icon to display after the button text */
  rightIcon?: React.ReactNode
  /** Accessible label for icon-only buttons (required when using size="icon") */
  "aria-label"?: string
}

/**
 * Loading Spinner Component
 * Memoized to prevent unnecessary re-renders
 */
const LoadingSpinner = React.memo(function LoadingSpinner({ className }: { className?: string }) {
  return (
    <svg
      className={cn("animate-spin", className)}
      viewBox="0 0 24 24"
      fill="none"
      aria-hidden="true"
    >
      <circle
        className="opacity-25"
        cx="12"
        cy="12"
        r="10"
        stroke="currentColor"
        strokeWidth="4"
      />
      <path
        className="opacity-75"
        fill="currentColor"
        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
      />
    </svg>
  )
})

/**
 * Button Component
 *
 * Performance optimized with React.memo and useMemo for className computation.
 * Fully accessible with proper ARIA attributes and focus management.
 */
const ButtonComponent = React.forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      className,
      variant,
      size,
      isLoading,
      leftIcon,
      rightIcon,
      children,
      disabled,
      type = "button",
      "aria-label": ariaLabel,
      ...props
    },
    ref
  ) => {
    // Memoize className computation for performance
    const computedClassName = React.useMemo(
      () => cn(buttonVariants({ variant, size }), className),
      [variant, size, className]
    )

    // Determine spinner size based on button size
    const spinnerSize = React.useMemo(() => {
      switch (size) {
        case "xs":
          return "h-3 w-3"
        case "sm":
        case "icon-sm":
          return "h-3.5 w-3.5"
        case "lg":
        case "xl":
        case "icon-lg":
          return "h-5 w-5"
        default:
          return "h-4 w-4"
      }
    }, [size])

    // Check if this is an icon-only button
    const isIconButton = size === "icon" || size === "icon-sm" || size === "icon-lg"

    return (
      <button
        className={computedClassName}
        ref={ref}
        type={type}
        disabled={disabled || isLoading}
        aria-label={ariaLabel}
        aria-busy={isLoading}
        aria-disabled={disabled || isLoading}
        {...props}
      >
        {isLoading ? (
          <>
            <LoadingSpinner className={spinnerSize} />
            {!isIconButton && <span className="sr-only">Loading</span>}
            {!isIconButton && children}
          </>
        ) : (
          <>
            {leftIcon && <span className="shrink-0">{leftIcon}</span>}
            {children}
            {rightIcon && <span className="shrink-0">{rightIcon}</span>}
          </>
        )}
      </button>
    )
  }
)

ButtonComponent.displayName = "Button"

// Wrap with React.memo for performance optimization
// Only re-renders when props actually change
const Button = React.memo(ButtonComponent)

export { Button, buttonVariants }
