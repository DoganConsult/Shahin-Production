"use client"

import * as React from "react"
import * as Dialog from "@radix-ui/react-dialog"
import { cn } from "@/lib/utils"
import {
  Search,
  X,
  FileText,
  Shield,
  AlertTriangle,
  Settings,
  Users,
  BarChart3,
  Plus,
  Clock,
  ArrowRight,
  Command,
  Hash,
} from "lucide-react"

/**
 * Command Palette Component - Design System 2.0
 *
 * Global command palette for quick navigation and actions.
 * Triggered with ⌘K (Mac) or Ctrl+K (Windows/Linux).
 *
 * @example
 * <CommandPalette />
 *
 * @example
 * // With custom items
 * <CommandPalette
 *   customItems={[
 *     { id: 'custom', label: 'Custom Action', action: () => {} }
 *   ]}
 * />
 */

// Types
export interface CommandItem {
  id: string
  label: string
  labelAr?: string
  description?: string
  descriptionAr?: string
  icon?: React.ReactNode
  action: () => void
  category?: CommandCategory
  keywords?: string[]
  shortcut?: string
}

export type CommandCategory =
  | "navigation"
  | "actions"
  | "recent"
  | "settings"
  | "search"

export interface CommandPaletteProps {
  /** Custom command items to add */
  customItems?: CommandItem[]
  /** Callback when palette opens */
  onOpen?: () => void
  /** Callback when palette closes */
  onClose?: () => void
  /** Placeholder text for search */
  placeholder?: string
  /** Placeholder text for search (Arabic) */
  placeholderAr?: string
}

// Default navigation items
const defaultNavigationItems: CommandItem[] = [
  {
    id: "nav-dashboard",
    label: "Dashboard",
    labelAr: "لوحة التحكم",
    icon: <BarChart3 className="h-4 w-4" />,
    action: () => (window.location.href = "/dashboard"),
    category: "navigation",
    keywords: ["home", "overview", "main"],
  },
  {
    id: "nav-risks",
    label: "Risks",
    labelAr: "المخاطر",
    icon: <AlertTriangle className="h-4 w-4" />,
    action: () => (window.location.href = "/risks"),
    category: "navigation",
    keywords: ["risk", "threat", "vulnerability"],
  },
  {
    id: "nav-controls",
    label: "Controls",
    labelAr: "الضوابط",
    icon: <Shield className="h-4 w-4" />,
    action: () => (window.location.href = "/controls"),
    category: "navigation",
    keywords: ["control", "mitigation", "safeguard"],
  },
  {
    id: "nav-compliance",
    label: "Compliance",
    labelAr: "الامتثال",
    icon: <FileText className="h-4 w-4" />,
    action: () => (window.location.href = "/compliance"),
    category: "navigation",
    keywords: ["compliance", "regulation", "standard"],
  },
  {
    id: "nav-users",
    label: "Users",
    labelAr: "المستخدمين",
    icon: <Users className="h-4 w-4" />,
    action: () => (window.location.href = "/users"),
    category: "navigation",
    keywords: ["user", "team", "member"],
  },
  {
    id: "nav-settings",
    label: "Settings",
    labelAr: "الإعدادات",
    icon: <Settings className="h-4 w-4" />,
    action: () => (window.location.href = "/settings"),
    category: "navigation",
    keywords: ["settings", "preferences", "config"],
  },
]

// Default action items
const defaultActionItems: CommandItem[] = [
  {
    id: "action-new-risk",
    label: "Create New Risk",
    labelAr: "إنشاء خطر جديد",
    description: "Add a new risk to the register",
    descriptionAr: "إضافة خطر جديد إلى السجل",
    icon: <Plus className="h-4 w-4" />,
    action: () => (window.location.href = "/risks/new"),
    category: "actions",
    keywords: ["create", "new", "add", "risk"],
    shortcut: "N R",
  },
  {
    id: "action-new-control",
    label: "Create New Control",
    labelAr: "إنشاء ضابط جديد",
    description: "Add a new control measure",
    descriptionAr: "إضافة ضابط جديد",
    icon: <Plus className="h-4 w-4" />,
    action: () => (window.location.href = "/controls/new"),
    category: "actions",
    keywords: ["create", "new", "add", "control"],
    shortcut: "N C",
  },
  {
    id: "action-new-assessment",
    label: "Start Assessment",
    labelAr: "بدء تقييم",
    description: "Begin a new compliance assessment",
    descriptionAr: "بدء تقييم امتثال جديد",
    icon: <Plus className="h-4 w-4" />,
    action: () => (window.location.href = "/assessments/new"),
    category: "actions",
    keywords: ["create", "new", "assessment", "audit"],
    shortcut: "N A",
  },
]

// Category labels
const categoryLabels: Record<CommandCategory, { en: string; ar: string }> = {
  navigation: { en: "Navigation", ar: "التنقل" },
  actions: { en: "Actions", ar: "الإجراءات" },
  recent: { en: "Recent", ar: "الأخيرة" },
  settings: { en: "Settings", ar: "الإعدادات" },
  search: { en: "Search Results", ar: "نتائج البحث" },
}

// Command Item Component
const CommandItemComponent = React.memo(function CommandItemComponent({
  item,
  isSelected,
  onSelect,
  isArabic,
}: {
  item: CommandItem
  isSelected: boolean
  onSelect: () => void
  isArabic: boolean
}) {
  const label = isArabic && item.labelAr ? item.labelAr : item.label
  const description =
    isArabic && item.descriptionAr ? item.descriptionAr : item.description

  return (
    <button
      className={cn(
        "w-full flex items-center gap-3 px-4 py-3 text-sm transition-colors",
        "focus:outline-none",
        isSelected
          ? "bg-primary/10 text-primary"
          : "text-foreground hover:bg-muted"
      )}
      onClick={onSelect}
      role="option"
      aria-selected={isSelected}
    >
      {/* Icon */}
      {item.icon && (
        <span
          className={cn(
            "flex-shrink-0",
            isSelected ? "text-primary" : "text-muted-foreground"
          )}
        >
          {item.icon}
        </span>
      )}

      {/* Content */}
      <div className="flex-1 text-start">
        <div className="font-medium">{label}</div>
        {description && (
          <div className="text-xs text-muted-foreground mt-0.5">
            {description}
          </div>
        )}
      </div>

      {/* Shortcut */}
      {item.shortcut && (
        <div className="flex items-center gap-1">
          {item.shortcut.split(" ").map((key, i) => (
            <kbd
              key={i}
              className={cn(
                "px-1.5 py-0.5 text-xs font-mono rounded",
                "bg-muted border border-border",
                isSelected ? "border-primary/30" : ""
              )}
            >
              {key}
            </kbd>
          ))}
        </div>
      )}

      {/* Arrow indicator */}
      {isSelected && (
        <ArrowRight className="h-4 w-4 text-primary flex-shrink-0" />
      )}
    </button>
  )
})

// Main Component
function CommandPaletteComponent({
  customItems = [],
  onOpen,
  onClose,
  placeholder = "Search commands...",
  placeholderAr = "ابحث عن الأوامر...",
}: CommandPaletteProps) {
  const [open, setOpen] = React.useState(false)
  const [search, setSearch] = React.useState("")
  const [selectedIndex, setSelectedIndex] = React.useState(0)
  const inputRef = React.useRef<HTMLInputElement>(null)

  // Detect Arabic language
  const isArabic =
    typeof document !== "undefined" &&
    document.documentElement.lang === "ar"

  // All items
  const allItems = React.useMemo(() => {
    return [...defaultNavigationItems, ...defaultActionItems, ...customItems]
  }, [customItems])

  // Filtered items based on search
  const filteredItems = React.useMemo(() => {
    if (!search.trim()) {
      return allItems
    }

    const searchLower = search.toLowerCase()
    return allItems.filter((item) => {
      const labelMatch = item.label.toLowerCase().includes(searchLower)
      const labelArMatch = item.labelAr?.toLowerCase().includes(searchLower)
      const descMatch = item.description?.toLowerCase().includes(searchLower)
      const keywordsMatch = item.keywords?.some((k) =>
        k.toLowerCase().includes(searchLower)
      )
      return labelMatch || labelArMatch || descMatch || keywordsMatch
    })
  }, [search, allItems])

  // Group items by category
  const groupedItems = React.useMemo(() => {
    const groups: Record<CommandCategory, CommandItem[]> = {
      navigation: [],
      actions: [],
      recent: [],
      settings: [],
      search: [],
    }

    filteredItems.forEach((item) => {
      const category = item.category || "navigation"
      groups[category].push(item)
    })

    return groups
  }, [filteredItems])

  // Handle keyboard shortcut
  React.useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // ⌘K or Ctrl+K to open
      if ((e.metaKey || e.ctrlKey) && e.key === "k") {
        e.preventDefault()
        setOpen((prev) => !prev)
      }
    }

    document.addEventListener("keydown", handleKeyDown)
    return () => document.removeEventListener("keydown", handleKeyDown)
  }, [])

  // Handle open/close callbacks
  React.useEffect(() => {
    if (open) {
      onOpen?.()
      // Reset state when opening
      setSearch("")
      setSelectedIndex(0)
      // Focus input after a short delay
      setTimeout(() => inputRef.current?.focus(), 50)
    } else {
      onClose?.()
    }
  }, [open, onOpen, onClose])

  // Handle keyboard navigation
  const handleKeyDown = React.useCallback(
    (e: React.KeyboardEvent) => {
      switch (e.key) {
        case "ArrowDown":
          e.preventDefault()
          setSelectedIndex((prev) =>
            prev < filteredItems.length - 1 ? prev + 1 : 0
          )
          break
        case "ArrowUp":
          e.preventDefault()
          setSelectedIndex((prev) =>
            prev > 0 ? prev - 1 : filteredItems.length - 1
          )
          break
        case "Enter":
          e.preventDefault()
          if (filteredItems[selectedIndex]) {
            filteredItems[selectedIndex].action()
            setOpen(false)
          }
          break
        case "Escape":
          e.preventDefault()
          setOpen(false)
          break
      }
    },
    [filteredItems, selectedIndex]
  )

  // Reset selected index when filtered items change
  React.useEffect(() => {
    setSelectedIndex(0)
  }, [search])

  // Calculate flat index for items
  const getFlatIndex = React.useCallback(
    (category: CommandCategory, itemIndex: number): number => {
      let flatIndex = 0
      const categories: CommandCategory[] = [
        "recent",
        "actions",
        "navigation",
        "settings",
        "search",
      ]

      for (const cat of categories) {
        if (cat === category) {
          return flatIndex + itemIndex
        }
        flatIndex += groupedItems[cat].length
      }
      return flatIndex
    },
    [groupedItems]
  )

  return (
    <Dialog.Root open={open} onOpenChange={setOpen}>
      <Dialog.Portal>
        <Dialog.Overlay
          className={cn(
            "fixed inset-0 z-50 bg-black/50 backdrop-blur-sm",
            "data-[state=open]:animate-in data-[state=closed]:animate-out",
            "data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0"
          )}
        />
        <Dialog.Content
          className={cn(
            "fixed left-[50%] top-[20%] z-50 w-full max-w-lg translate-x-[-50%]",
            "bg-background rounded-xl shadow-2xl border border-border",
            "data-[state=open]:animate-in data-[state=closed]:animate-out",
            "data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0",
            "data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95",
            "data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-[48%]",
            "data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-[48%]",
            "duration-200"
          )}
          onKeyDown={handleKeyDown}
        >
          {/* Search Input */}
          <div className="flex items-center gap-3 px-4 py-3 border-b border-border">
            <Search className="h-5 w-5 text-muted-foreground flex-shrink-0" />
            <input
              ref={inputRef}
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder={isArabic ? placeholderAr : placeholder}
              className={cn(
                "flex-1 bg-transparent text-foreground placeholder:text-muted-foreground",
                "outline-none text-sm"
              )}
              aria-label={isArabic ? "بحث" : "Search"}
            />
            <kbd className="hidden sm:inline-flex items-center gap-1 px-2 py-1 text-xs font-mono text-muted-foreground bg-muted rounded border border-border">
              <Command className="h-3 w-3" />K
            </kbd>
            <Dialog.Close asChild>
              <button
                className="p-1 rounded hover:bg-muted text-muted-foreground hover:text-foreground transition-colors"
                aria-label={isArabic ? "إغلاق" : "Close"}
              >
                <X className="h-4 w-4" />
              </button>
            </Dialog.Close>
          </div>

          {/* Results */}
          <div
            className="max-h-[60vh] overflow-y-auto py-2"
            role="listbox"
            aria-label={isArabic ? "الأوامر" : "Commands"}
          >
            {filteredItems.length === 0 ? (
              <div className="px-4 py-8 text-center text-muted-foreground">
                <Hash className="h-8 w-8 mx-auto mb-2 opacity-50" />
                <p className="text-sm">
                  {isArabic
                    ? "لم يتم العثور على نتائج"
                    : "No results found"}
                </p>
                <p className="text-xs mt-1">
                  {isArabic
                    ? "جرب كلمات بحث مختلفة"
                    : "Try different search terms"}
                </p>
              </div>
            ) : (
              <>
                {/* Render grouped items */}
                {(
                  [
                    "recent",
                    "actions",
                    "navigation",
                    "settings",
                    "search",
                  ] as CommandCategory[]
                ).map((category) => {
                  const items = groupedItems[category]
                  if (items.length === 0) return null

                  return (
                    <div key={category}>
                      {/* Category Header */}
                      <div className="px-4 py-2 text-xs font-medium text-muted-foreground uppercase tracking-wider">
                        {isArabic
                          ? categoryLabels[category].ar
                          : categoryLabels[category].en}
                      </div>

                      {/* Items */}
                      {items.map((item, itemIndex) => {
                        const flatIndex = getFlatIndex(category, itemIndex)
                        return (
                          <CommandItemComponent
                            key={item.id}
                            item={item}
                            isSelected={selectedIndex === flatIndex}
                            onSelect={() => {
                              item.action()
                              setOpen(false)
                            }}
                            isArabic={isArabic}
                          />
                        )
                      })}
                    </div>
                  )
                })}
              </>
            )}
          </div>

          {/* Footer */}
          <div className="flex items-center justify-between px-4 py-2 border-t border-border bg-muted/30 text-xs text-muted-foreground">
            <div className="flex items-center gap-4">
              <span className="flex items-center gap-1">
                <kbd className="px-1 py-0.5 bg-background rounded border border-border">
                  ↑↓
                </kbd>
                {isArabic ? "للتنقل" : "Navigate"}
              </span>
              <span className="flex items-center gap-1">
                <kbd className="px-1 py-0.5 bg-background rounded border border-border">
                  ↵
                </kbd>
                {isArabic ? "للتحديد" : "Select"}
              </span>
              <span className="flex items-center gap-1">
                <kbd className="px-1.5 py-0.5 bg-background rounded border border-border">
                  Esc
                </kbd>
                {isArabic ? "للإغلاق" : "Close"}
              </span>
            </div>
          </div>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  )
}

const CommandPalette = React.memo(CommandPaletteComponent)

/**
 * Hook to programmatically open the command palette
 */
export function useCommandPalette() {
  const [isOpen, setIsOpen] = React.useState(false)

  const open = React.useCallback(() => {
    // Dispatch keyboard event to trigger command palette
    const event = new KeyboardEvent("keydown", {
      key: "k",
      metaKey: true,
      bubbles: true,
    })
    document.dispatchEvent(event)
  }, [])

  return { isOpen, open }
}

export { CommandPalette }
export default CommandPalette
