"use client"

import * as React from "react"
import { cva, type VariantProps } from "class-variance-authority"
import { cn } from "@/lib/utils"

/**
 * Card Component - Design System 2.0
 *
 * A flexible card component with multiple variants for different use cases.
 * Performance optimized with React.memo.
 *
 * @example
 * <Card variant="elevated" hover>
 *   <CardHeader>
 *     <CardTitle>Card Title</CardTitle>
 *     <CardDescription>Card description</CardDescription>
 *   </CardHeader>
 *   <CardContent>Content here</CardContent>
 *   <CardFooter>Footer actions</CardFooter>
 * </Card>
 */

const cardVariants = cva(
  "rounded-xl border bg-card text-card-foreground",
  {
    variants: {
      variant: {
        default: "shadow-sm",
        elevated: "shadow-md border-0",
        outline: "border-2 shadow-none",
        ghost: "border-0 shadow-none bg-transparent",
        glass: "backdrop-blur-md bg-white/80 dark:bg-gray-900/80 border-white/20",
        interactive: [
          "shadow-sm cursor-pointer",
          "transition-all duration-200",
          "hover:shadow-lg hover:border-primary/20 hover:-translate-y-1",
          "active:scale-[0.99]",
        ].join(" "),
      },
      padding: {
        none: "",
        sm: "p-4",
        default: "p-6",
        lg: "p-8",
      },
    },
    defaultVariants: {
      variant: "default",
      padding: "none", // Let CardContent/CardHeader handle padding by default
    },
  }
)

export interface CardProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof cardVariants> {
  /** @deprecated Use variant="interactive" instead */
  hover?: boolean
  /** Makes the card focusable and adds focus styles */
  focusable?: boolean
  /** ARIA role for the card */
  role?: string
}

const CardComponent = React.forwardRef<HTMLDivElement, CardProps>(
  ({ className, variant, padding, hover = false, focusable = false, role, ...props }, ref) => {
    // Memoize className computation
    const computedClassName = React.useMemo(
      () =>
        cn(
          cardVariants({ variant: hover ? "interactive" : variant, padding }),
          focusable && "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
          className
        ),
      [variant, padding, hover, focusable, className]
    )

    return (
      <div
        ref={ref}
        className={computedClassName}
        role={role}
        tabIndex={focusable ? 0 : undefined}
        {...props}
      />
    )
  }
)
CardComponent.displayName = "Card"

const Card = React.memo(CardComponent)

// CardHeader
export interface CardHeaderProps extends React.HTMLAttributes<HTMLDivElement> {
  /** Adds a bottom border to separate header from content */
  bordered?: boolean
}

const CardHeaderComponent = React.forwardRef<HTMLDivElement, CardHeaderProps>(
  ({ className, bordered = false, ...props }, ref) => {
    const computedClassName = React.useMemo(
      () =>
        cn(
          "flex flex-col space-y-1.5 p-6",
          bordered && "border-b",
          className
        ),
      [bordered, className]
    )

    return <div ref={ref} className={computedClassName} {...props} />
  }
)
CardHeaderComponent.displayName = "CardHeader"

const CardHeader = React.memo(CardHeaderComponent)

// CardTitle
export interface CardTitleProps extends React.HTMLAttributes<HTMLHeadingElement> {
  /** Heading level for accessibility (h1-h6) */
  as?: "h1" | "h2" | "h3" | "h4" | "h5" | "h6"
}

const CardTitleComponent = React.forwardRef<HTMLHeadingElement, CardTitleProps>(
  ({ className, as: Component = "h3", ...props }, ref) => {
    const computedClassName = React.useMemo(
      () => cn("text-lg font-semibold leading-none tracking-tight", className),
      [className]
    )

    return <Component ref={ref} className={computedClassName} {...props} />
  }
)
CardTitleComponent.displayName = "CardTitle"

const CardTitle = React.memo(CardTitleComponent)

// CardDescription
const CardDescriptionComponent = React.forwardRef<
  HTMLParagraphElement,
  React.HTMLAttributes<HTMLParagraphElement>
>(({ className, ...props }, ref) => {
  const computedClassName = React.useMemo(
    () => cn("text-sm text-muted-foreground", className),
    [className]
  )

  return <p ref={ref} className={computedClassName} {...props} />
})
CardDescriptionComponent.displayName = "CardDescription"

const CardDescription = React.memo(CardDescriptionComponent)

// CardContent
export interface CardContentProps extends React.HTMLAttributes<HTMLDivElement> {
  /** Removes default padding */
  noPadding?: boolean
}

const CardContentComponent = React.forwardRef<HTMLDivElement, CardContentProps>(
  ({ className, noPadding = false, ...props }, ref) => {
    const computedClassName = React.useMemo(
      () => cn(noPadding ? "" : "p-6 pt-0", className),
      [noPadding, className]
    )

    return <div ref={ref} className={computedClassName} {...props} />
  }
)
CardContentComponent.displayName = "CardContent"

const CardContent = React.memo(CardContentComponent)

// CardFooter
export interface CardFooterProps extends React.HTMLAttributes<HTMLDivElement> {
  /** Adds a top border to separate footer from content */
  bordered?: boolean
  /** Aligns footer content to the end (right in LTR) */
  justify?: "start" | "center" | "end" | "between"
}

const CardFooterComponent = React.forwardRef<HTMLDivElement, CardFooterProps>(
  ({ className, bordered = false, justify = "start", ...props }, ref) => {
    const computedClassName = React.useMemo(
      () =>
        cn(
          "flex items-center gap-2 p-6 pt-0",
          bordered && "border-t pt-6",
          {
            "justify-start": justify === "start",
            "justify-center": justify === "center",
            "justify-end": justify === "end",
            "justify-between": justify === "between",
          },
          className
        ),
      [bordered, justify, className]
    )

    return <div ref={ref} className={computedClassName} {...props} />
  }
)
CardFooterComponent.displayName = "CardFooter"

const CardFooter = React.memo(CardFooterComponent)

// CardImage - New component for card with image
export interface CardImageProps extends React.ImgHTMLAttributes<HTMLImageElement> {
  /** Position of the image in the card */
  position?: "top" | "bottom"
}

const CardImageComponent = React.forwardRef<HTMLImageElement, CardImageProps>(
  ({ className, position = "top", alt = "", ...props }, ref) => {
    const computedClassName = React.useMemo(
      () =>
        cn(
          "w-full object-cover",
          position === "top" ? "rounded-t-xl" : "rounded-b-xl",
          className
        ),
      [position, className]
    )

    return <img ref={ref} className={computedClassName} alt={alt} {...props} />
  }
)
CardImageComponent.displayName = "CardImage"

const CardImage = React.memo(CardImageComponent)

export {
  Card,
  CardHeader,
  CardFooter,
  CardTitle,
  CardDescription,
  CardContent,
  CardImage,
  cardVariants,
}
