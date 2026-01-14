"use client"

import * as React from "react"
import { cn } from "@/lib/utils"

/**
 * Skeleton Component - Design System 2.0
 *
 * Loading placeholder component with shimmer animation.
 * Use to indicate content is loading.
 *
 * @example
 * <Skeleton className="h-4 w-[200px]" />
 * <Skeleton variant="circular" className="h-12 w-12" />
 * <Skeleton variant="text" lines={3} />
 */

export interface SkeletonProps extends React.HTMLAttributes<HTMLDivElement> {
  /** Shape variant */
  variant?: "rectangular" | "circular" | "text"
  /** Enable shimmer animation */
  animate?: boolean
  /** Number of text lines (only for variant="text") */
  lines?: number
}

const SkeletonComponent = React.forwardRef<HTMLDivElement, SkeletonProps>(
  ({ className, variant = "rectangular", animate = true, lines = 1, ...props }, ref) => {
    // Must call hooks before any conditional returns
    const variantClasses = React.useMemo(() => {
      switch (variant) {
        case "circular":
          return "rounded-full"
        case "text":
          return "h-4 rounded-md"
        default:
          return "rounded-lg"
      }
    }, [variant])

    // For text variant with multiple lines
    if (variant === "text" && lines > 1) {
      return (
        <div ref={ref} className={cn("space-y-2", className)} {...props}>
          {Array.from({ length: lines }).map((_, i) => (
            <div
              key={i}
              className={cn(
                "h-4 rounded-md bg-muted",
                animate && "animate-shimmer",
                // Last line is shorter
                i === lines - 1 && "w-3/4"
              )}
            />
          ))}
        </div>
      )
    }

    return (
      <div
        ref={ref}
        className={cn(
          "bg-muted",
          variantClasses,
          animate && "animate-shimmer",
          className
        )}
        aria-hidden="true"
        {...props}
      />
    )
  }
)

SkeletonComponent.displayName = "Skeleton"

const Skeleton = React.memo(SkeletonComponent)

/**
 * SkeletonCard - Pre-built card skeleton
 */
const SkeletonCardComponent = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => (
  <div
    ref={ref}
    className={cn("rounded-xl border bg-card p-6 space-y-4", className)}
    {...props}
  >
    <div className="flex items-center space-x-4">
      <Skeleton variant="circular" className="h-12 w-12" />
      <div className="space-y-2 flex-1">
        <Skeleton className="h-4 w-1/2" />
        <Skeleton className="h-3 w-1/3" />
      </div>
    </div>
    <Skeleton variant="text" lines={3} />
    <div className="flex justify-end gap-2">
      <Skeleton className="h-9 w-20" />
      <Skeleton className="h-9 w-20" />
    </div>
  </div>
))

SkeletonCardComponent.displayName = "SkeletonCard"

const SkeletonCard = React.memo(SkeletonCardComponent)

/**
 * SkeletonTable - Pre-built table skeleton
 */
interface SkeletonTableProps extends React.HTMLAttributes<HTMLDivElement> {
  rows?: number
  columns?: number
}

const SkeletonTableComponent = React.forwardRef<HTMLDivElement, SkeletonTableProps>(
  ({ className, rows = 5, columns = 4, ...props }, ref) => (
    <div
      ref={ref}
      className={cn("rounded-xl border bg-card overflow-hidden", className)}
      {...props}
    >
      {/* Header */}
      <div className="border-b bg-muted/50 p-4">
        <div className="flex gap-4">
          {Array.from({ length: columns }).map((_, i) => (
            <Skeleton key={i} className="h-4 flex-1" />
          ))}
        </div>
      </div>
      {/* Rows */}
      {Array.from({ length: rows }).map((_, rowIndex) => (
        <div key={rowIndex} className="border-b last:border-0 p-4">
          <div className="flex gap-4 items-center">
            {Array.from({ length: columns }).map((_, colIndex) => (
              <Skeleton
                key={colIndex}
                className={cn(
                  "h-4 flex-1",
                  colIndex === 0 && "w-1/4 flex-none"
                )}
              />
            ))}
          </div>
        </div>
      ))}
    </div>
  )
)

SkeletonTableComponent.displayName = "SkeletonTable"

const SkeletonTable = React.memo(SkeletonTableComponent)

/**
 * SkeletonAvatar - Pre-built avatar skeleton
 */
interface SkeletonAvatarProps extends React.HTMLAttributes<HTMLDivElement> {
  size?: "sm" | "default" | "lg" | "xl"
}

const SkeletonAvatarComponent = React.forwardRef<HTMLDivElement, SkeletonAvatarProps>(
  ({ className, size = "default", ...props }, ref) => {
    const sizeClasses = React.useMemo(() => {
      switch (size) {
        case "sm":
          return "h-8 w-8"
        case "lg":
          return "h-14 w-14"
        case "xl":
          return "h-20 w-20"
        default:
          return "h-10 w-10"
      }
    }, [size])

    return (
      <Skeleton
        ref={ref}
        variant="circular"
        className={cn(sizeClasses, className)}
        {...props}
      />
    )
  }
)

SkeletonAvatarComponent.displayName = "SkeletonAvatar"

const SkeletonAvatar = React.memo(SkeletonAvatarComponent)

export { Skeleton, SkeletonCard, SkeletonTable, SkeletonAvatar }
