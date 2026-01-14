"use client"

import * as React from "react"
import { cn } from "@/lib/utils"
import { cva, type VariantProps } from "class-variance-authority"
import { AlertCircle, CheckCircle, Eye, EyeOff, Search, X } from "lucide-react"

/**
 * Input Component - Design System 2.0
 *
 * Accessible form input with variants and validation states.
 *
 * @example
 * <Input placeholder="Enter your name" />
 *
 * @example
 * <Input type="password" showPasswordToggle />
 *
 * @example
 * <Input
 *   label="Email"
 *   error="Please enter a valid email"
 *   leftIcon={<Mail />}
 * />
 */

const inputVariants = cva(
  [
    "flex w-full rounded-lg border bg-background px-3 py-2 text-sm",
    "ring-offset-background transition-colors duration-200",
    "file:border-0 file:bg-transparent file:text-sm file:font-medium",
    "placeholder:text-muted-foreground",
    "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
    "disabled:cursor-not-allowed disabled:opacity-50",
  ],
  {
    variants: {
      variant: {
        default: "border-input",
        error: "border-destructive focus-visible:ring-destructive",
        success: "border-emerald-500 focus-visible:ring-emerald-500",
      },
      inputSize: {
        sm: "h-8 text-xs px-2",
        md: "h-10",
        lg: "h-12 text-base px-4",
      },
    },
    defaultVariants: {
      variant: "default",
      inputSize: "md",
    },
  }
)

export interface InputProps
  extends Omit<React.InputHTMLAttributes<HTMLInputElement>, "size">,
    VariantProps<typeof inputVariants> {
  /** Label text */
  label?: string
  /** Helper text */
  helperText?: string
  /** Error message */
  error?: string
  /** Success message */
  success?: string
  /** Left icon */
  leftIcon?: React.ReactNode
  /** Right icon */
  rightIcon?: React.ReactNode
  /** Show password toggle for password inputs */
  showPasswordToggle?: boolean
  /** Show clear button */
  clearable?: boolean
  /** Callback when clear is clicked */
  onClear?: () => void
  /** Full width */
  fullWidth?: boolean
  /** Container class name */
  containerClassName?: string
}

const InputComponent = React.forwardRef<HTMLInputElement, InputProps>(
  (
    {
      className,
      containerClassName,
      type = "text",
      variant,
      inputSize,
      label,
      helperText,
      error,
      success,
      leftIcon,
      rightIcon,
      showPasswordToggle,
      clearable,
      onClear,
      fullWidth = true,
      disabled,
      value,
      ...props
    },
    ref
  ) => {
    const [showPassword, setShowPassword] = React.useState(false)
    const inputId = React.useId()

    // Determine variant based on error/success
    const computedVariant = error ? "error" : success ? "success" : variant

    // Password type handling
    const computedType =
      type === "password" && showPassword ? "text" : type

    // Has value for clear button
    const hasValue = value !== undefined && value !== ""

    return (
      <div
        className={cn(
          "flex flex-col gap-1.5",
          fullWidth && "w-full",
          containerClassName
        )}
      >
        {/* Label */}
        {label && (
          <label
            htmlFor={inputId}
            className={cn(
              "text-sm font-medium text-foreground",
              disabled && "opacity-50"
            )}
          >
            {label}
          </label>
        )}

        {/* Input wrapper */}
        <div className="relative">
          {/* Left icon */}
          {leftIcon && (
            <div className="absolute start-3 top-1/2 -translate-y-1/2 text-muted-foreground">
              {leftIcon}
            </div>
          )}

          {/* Input */}
          <input
            id={inputId}
            ref={ref}
            type={computedType}
            disabled={disabled}
            value={value}
            className={cn(
              inputVariants({ variant: computedVariant, inputSize }),
              leftIcon && "ps-10",
              (rightIcon || showPasswordToggle || (clearable && hasValue)) &&
                "pe-10",
              className
            )}
            aria-invalid={!!error}
            aria-describedby={
              error
                ? `${inputId}-error`
                : helperText
                  ? `${inputId}-helper`
                  : undefined
            }
            {...props}
          />

          {/* Right side icons */}
          <div className="absolute end-3 top-1/2 -translate-y-1/2 flex items-center gap-1">
            {/* Clear button */}
            {clearable && hasValue && !disabled && (
              <button
                type="button"
                onClick={onClear}
                className="text-muted-foreground hover:text-foreground transition-colors p-0.5"
                aria-label="Clear input"
              >
                <X className="h-4 w-4" />
              </button>
            )}

            {/* Password toggle */}
            {type === "password" && showPasswordToggle && (
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="text-muted-foreground hover:text-foreground transition-colors p-0.5"
                aria-label={showPassword ? "Hide password" : "Show password"}
              >
                {showPassword ? (
                  <EyeOff className="h-4 w-4" />
                ) : (
                  <Eye className="h-4 w-4" />
                )}
              </button>
            )}

            {/* Right icon */}
            {rightIcon && (
              <span className="text-muted-foreground">{rightIcon}</span>
            )}

            {/* Validation icon */}
            {error && <AlertCircle className="h-4 w-4 text-destructive" />}
            {success && !error && (
              <CheckCircle className="h-4 w-4 text-emerald-500" />
            )}
          </div>
        </div>

        {/* Error message */}
        {error && (
          <p
            id={`${inputId}-error`}
            className="text-xs text-destructive"
            role="alert"
          >
            {error}
          </p>
        )}

        {/* Success message */}
        {success && !error && (
          <p className="text-xs text-emerald-600">{success}</p>
        )}

        {/* Helper text */}
        {helperText && !error && !success && (
          <p
            id={`${inputId}-helper`}
            className="text-xs text-muted-foreground"
          >
            {helperText}
          </p>
        )}
      </div>
    )
  }
)
InputComponent.displayName = "Input"

const Input = React.memo(InputComponent)

// Search Input variant
interface SearchInputProps extends Omit<InputProps, "leftIcon" | "type"> {
  onSearch?: (value: string) => void
}

const SearchInputComponent = React.forwardRef<HTMLInputElement, SearchInputProps>(
  ({ onSearch, onKeyDown, ...props }, ref) => {
    const handleKeyDown = React.useCallback(
      (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter" && onSearch) {
          onSearch(e.currentTarget.value)
        }
        onKeyDown?.(e)
      },
      [onSearch, onKeyDown]
    )

    return (
      <Input
        ref={ref}
        type="search"
        leftIcon={<Search className="h-4 w-4" />}
        clearable
        onKeyDown={handleKeyDown}
        {...props}
      />
    )
  }
)
SearchInputComponent.displayName = "SearchInput"

const SearchInput = React.memo(SearchInputComponent)

// Textarea component
interface TextareaProps
  extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  /** Label text */
  label?: string
  /** Helper text */
  helperText?: string
  /** Error message */
  error?: string
  /** Container class name */
  containerClassName?: string
}

const TextareaComponent = React.forwardRef<HTMLTextAreaElement, TextareaProps>(
  (
    { className, containerClassName, label, helperText, error, disabled, ...props },
    ref
  ) => {
    const textareaId = React.useId()

    return (
      <div className={cn("flex flex-col gap-1.5 w-full", containerClassName)}>
        {label && (
          <label
            htmlFor={textareaId}
            className={cn(
              "text-sm font-medium text-foreground",
              disabled && "opacity-50"
            )}
          >
            {label}
          </label>
        )}

        <textarea
          id={textareaId}
          ref={ref}
          disabled={disabled}
          className={cn(
            "flex min-h-[80px] w-full rounded-lg border bg-background px-3 py-2 text-sm",
            "ring-offset-background transition-colors",
            "placeholder:text-muted-foreground",
            "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
            "disabled:cursor-not-allowed disabled:opacity-50",
            error ? "border-destructive" : "border-input",
            className
          )}
          aria-invalid={!!error}
          aria-describedby={error ? `${textareaId}-error` : undefined}
          {...props}
        />

        {error && (
          <p
            id={`${textareaId}-error`}
            className="text-xs text-destructive"
            role="alert"
          >
            {error}
          </p>
        )}

        {helperText && !error && (
          <p className="text-xs text-muted-foreground">{helperText}</p>
        )}
      </div>
    )
  }
)
TextareaComponent.displayName = "Textarea"

const Textarea = React.memo(TextareaComponent)

export { Input, SearchInput, Textarea, inputVariants }
