"use client"

import * as React from "react"
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

const buttonVariants = cva(
  "inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-xl text-sm font-semibold ring-offset-white transition-all duration-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-accent-500 focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50",
  {
    variants: {
      variant: {
        default:
          "bg-primary-800 text-white hover:bg-primary-700 shadow-lg hover:shadow-xl hover:-translate-y-0.5",
        gradient:
          "bg-gradient-to-r from-accent-500 to-accent-400 text-primary-900 font-bold shadow-lg shadow-accent-500/30 hover:shadow-accent-500/50 hover:-translate-y-1",
        outline:
          "border-2 border-gray-200 dark:border-gray-700 bg-transparent hover:border-accent-500 hover:text-accent-600 dark:hover:text-accent-400",
        ghost:
          "hover:bg-gray-100 dark:hover:bg-gray-800 hover:text-accent-600",
        secondary:
          "bg-white/10 text-white border border-white/20 backdrop-blur-sm hover:bg-white/20 hover:-translate-y-0.5",
        link: "text-accent-500 underline-offset-4 hover:underline",
      },
      size: {
        default: "h-11 px-6 py-2",
        sm: "h-9 px-4 text-xs",
        lg: "h-12 px-8 text-base",
        xl: "h-14 px-10 text-lg",
        icon: "h-10 w-10",
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
  asChild?: boolean
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, ...props }, ref) => {
    return (
      <button
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        {...props}
      />
    )
  }
)
Button.displayName = "Button"

export { Button, buttonVariants }
