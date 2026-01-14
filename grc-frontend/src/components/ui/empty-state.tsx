"use client"

import * as React from "react"
import { cn } from "@/lib/utils"
import { Button, type ButtonProps } from "./button"

/**
 * EmptyState Component - Design System 2.0
 *
 * Display when there's no data or content to show.
 * Provides guidance and optional action buttons.
 *
 * @example
 * <EmptyState
 *   icon={<SearchIcon />}
 *   title="No results found"
 *   description="Try adjusting your search or filters"
 *   action={{ label: "Clear filters", onClick: () => {} }}
 * />
 */

export interface EmptyStateProps extends React.HTMLAttributes<HTMLDivElement> {
  /** Icon to display (typically an SVG icon component) */
  icon?: React.ReactNode
  /** Main title text */
  title: string
  /** Description text */
  description?: string
  /** Primary action button */
  action?: {
    label: string
    onClick: () => void
    variant?: ButtonProps["variant"]
  }
  /** Secondary action button */
  secondaryAction?: {
    label: string
    onClick: () => void
  }
  /** Size variant */
  size?: "sm" | "default" | "lg"
  /** Visual variant */
  variant?: "default" | "card" | "inline"
}

/**
 * Default Icons for common empty states
 */
const EmptyIcons = {
  noData: (
    <svg
      className="h-full w-full"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
      strokeWidth={1.5}
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M20.25 7.5l-.625 10.632a2.25 2.25 0 01-2.247 2.118H6.622a2.25 2.25 0 01-2.247-2.118L3.75 7.5M10 11.25h4M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125z"
      />
    </svg>
  ),
  noSearch: (
    <svg
      className="h-full w-full"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
      strokeWidth={1.5}
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z"
      />
    </svg>
  ),
  error: (
    <svg
      className="h-full w-full"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
      strokeWidth={1.5}
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z"
      />
    </svg>
  ),
  success: (
    <svg
      className="h-full w-full"
      fill="none"
      viewBox="0 0 24 24"
      stroke="currentColor"
      strokeWidth={1.5}
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
      />
    </svg>
  ),
}

const EmptyStateComponent = React.forwardRef<HTMLDivElement, EmptyStateProps>(
  (
    {
      className,
      icon,
      title,
      description,
      action,
      secondaryAction,
      size = "default",
      variant = "default",
      ...props
    },
    ref
  ) => {
    // Size classes
    const sizeClasses = React.useMemo(() => {
      switch (size) {
        case "sm":
          return {
            container: "py-6",
            icon: "h-10 w-10",
            title: "text-sm font-medium",
            description: "text-xs",
            gap: "gap-2",
          }
        case "lg":
          return {
            container: "py-16",
            icon: "h-16 w-16",
            title: "text-xl font-semibold",
            description: "text-base",
            gap: "gap-4",
          }
        default:
          return {
            container: "py-12",
            icon: "h-12 w-12",
            title: "text-base font-medium",
            description: "text-sm",
            gap: "gap-3",
          }
      }
    }, [size])

    // Variant classes
    const variantClasses = React.useMemo(() => {
      switch (variant) {
        case "card":
          return "bg-card border rounded-xl shadow-sm"
        case "inline":
          return "bg-transparent"
        default:
          return "bg-muted/30 rounded-xl"
      }
    }, [variant])

    return (
      <div
        ref={ref}
        className={cn(
          "flex flex-col items-center justify-center text-center",
          sizeClasses.container,
          sizeClasses.gap,
          variantClasses,
          "px-6",
          className
        )}
        role="status"
        aria-label={title}
        {...props}
      >
        {/* Icon */}
        {icon && (
          <div
            className={cn(
              sizeClasses.icon,
              "text-muted-foreground/60",
              "mb-2"
            )}
            aria-hidden="true"
          >
            {icon}
          </div>
        )}

        {/* Title */}
        <h3 className={cn(sizeClasses.title, "text-foreground")}>
          {title}
        </h3>

        {/* Description */}
        {description && (
          <p
            className={cn(
              sizeClasses.description,
              "text-muted-foreground max-w-sm"
            )}
          >
            {description}
          </p>
        )}

        {/* Actions */}
        {(action || secondaryAction) && (
          <div className="flex items-center gap-3 mt-4">
            {action && (
              <Button
                variant={action.variant || "default"}
                onClick={action.onClick}
                size={size === "sm" ? "sm" : "default"}
              >
                {action.label}
              </Button>
            )}
            {secondaryAction && (
              <Button
                variant="ghost"
                onClick={secondaryAction.onClick}
                size={size === "sm" ? "sm" : "default"}
              >
                {secondaryAction.label}
              </Button>
            )}
          </div>
        )}
      </div>
    )
  }
)

EmptyStateComponent.displayName = "EmptyState"

const EmptyState = React.memo(EmptyStateComponent)

/**
 * Pre-built empty state variants
 */

interface PresetEmptyStateProps extends Omit<EmptyStateProps, "icon" | "title"> {
  title?: string
}

// No Data Empty State
const NoDataComponent = React.forwardRef<HTMLDivElement, PresetEmptyStateProps>(
  ({ title = "No data available", description = "There is no data to display at this time.", ...props }, ref) => (
    <EmptyState
      ref={ref}
      icon={EmptyIcons.noData}
      title={title}
      description={description}
      {...props}
    />
  )
)
NoDataComponent.displayName = "NoData"
const NoData = React.memo(NoDataComponent)

// No Search Results Empty State
const NoResultsComponent = React.forwardRef<HTMLDivElement, PresetEmptyStateProps>(
  ({ title = "No results found", description = "Try adjusting your search or filter criteria.", ...props }, ref) => (
    <EmptyState
      ref={ref}
      icon={EmptyIcons.noSearch}
      title={title}
      description={description}
      {...props}
    />
  )
)
NoResultsComponent.displayName = "NoResults"
const NoResults = React.memo(NoResultsComponent)

// Error Empty State
const ErrorStateComponent = React.forwardRef<HTMLDivElement, PresetEmptyStateProps>(
  ({ title = "Something went wrong", description = "An error occurred while loading data. Please try again.", ...props }, ref) => (
    <EmptyState
      ref={ref}
      icon={EmptyIcons.error}
      title={title}
      description={description}
      {...props}
    />
  )
)
ErrorStateComponent.displayName = "ErrorState"
const ErrorState = React.memo(ErrorStateComponent)

// Success Empty State
const SuccessStateComponent = React.forwardRef<HTMLDivElement, PresetEmptyStateProps>(
  ({ title = "All done!", description = "You have completed all tasks.", ...props }, ref) => (
    <EmptyState
      ref={ref}
      icon={EmptyIcons.success}
      title={title}
      description={description}
      {...props}
    />
  )
)
SuccessStateComponent.displayName = "SuccessState"
const SuccessState = React.memo(SuccessStateComponent)

export {
  EmptyState,
  EmptyIcons,
  NoData,
  NoResults,
  ErrorState,
  SuccessState,
}
