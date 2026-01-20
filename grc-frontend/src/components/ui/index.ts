/**
 * UI Components Index - Design System 2.0
 *
 * Central export for all UI components.
 * Import like: import { Button, Card, Badge } from "@/components/ui"
 */

// Core Components
export { Button, buttonVariants } from "./button"
export { Card, CardHeader, CardTitle, CardDescription, CardContent, CardFooter, CardImage, cardVariants } from "./card"
export { Badge, StatusDot, badgeVariants } from "./badge"

// Form Components
export { Input, SearchInput, Textarea, inputVariants } from "./input"
export {
  Select,
  SelectGroup,
  SelectValue,
  SelectTrigger,
  SelectContent,
  SelectLabel,
  SelectItem,
  SelectSeparator,
  SimpleSelect,
} from "./select"
export { RoleSelect } from "./role-select"
export { SectorSelect } from "./sector-select"
export { CountrySelect } from "./country-select"
export { CitySelect } from "./city-select"

// Feedback Components
export { Skeleton, SkeletonCard, SkeletonTable, SkeletonAvatar } from "./skeleton"
export { EmptyState, NoData, NoResults, ErrorState, SuccessState } from "./empty-state"

// Modal & Overlay Components
export {
  Dialog,
  DialogPortal,
  DialogOverlay,
  DialogClose,
  DialogTrigger,
  DialogContent,
  DialogHeader,
  DialogFooter,
  DialogTitle,
  DialogDescription,
  AlertDialog,
} from "./dialog"

// Navigation & Global Components
export { CommandPalette, useCommandPalette } from "./command-palette"
export { NotificationCenter, NotificationBell } from "./notification-center"

// Data Display Components
export { DataTable } from "./data-table"

// Animated & Catchy UI Components
export {
  AnimatedBlobs,
  FloatingIcons,
  HeroSection,
  FeatureCard,
  StatCard,
  GradientButton,
  GradientBorderCard,
  LoaderDots,
  LoaderSpinner,
  SuccessCheckmark,
} from "./animated-hero"

// Premium Enterprise Effects
export {
  CustomCursorProvider,
  TiltCard,
  MagneticButton,
  TextReveal,
  CharReveal,
  ParallaxLayer,
  ParallaxContainer,
  SmoothScrollLink,
  AnimatedCounter,
  LogoCarousel,
  ScrollProgress,
  SpotlightCard,
  GradientText,
  StaggerContainer,
  GlowCard,
} from "./premium-effects"

// Premium Page Sections
export {
  PremiumHero,
  PremiumFeatures,
  PremiumStats,
  PremiumCta,
} from "./premium-hero"

// Re-export types
export type { CommandItem, CommandCategory, CommandPaletteProps } from "./command-palette"
export type { Notification, NotificationType, NotificationPriority, NotificationCenterProps } from "./notification-center"
export type { Column, DataTableProps, SortDirection } from "./data-table"
