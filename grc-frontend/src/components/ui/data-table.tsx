"use client"

import * as React from "react"
import { cn } from "@/lib/utils"
import {
  ChevronUp,
  ChevronDown,
  ChevronsUpDown,
  ChevronLeft,
  ChevronRight,
  ChevronsLeft,
  ChevronsRight,
  Search,
  X,
  Filter,
  Download,
  MoreHorizontal,
  Check,
} from "lucide-react"
import { Button } from "./button"
import { Skeleton } from "./skeleton"

/**
 * Data Table Component - Design System 2.0
 *
 * Enterprise-grade data table with sorting, filtering, pagination, and row selection.
 *
 * @example
 * <DataTable
 *   data={risks}
 *   columns={[
 *     { key: 'name', header: 'Name', sortable: true },
 *     { key: 'status', header: 'Status', filterable: true },
 *   ]}
 *   onRowClick={(row) => console.log(row)}
 * />
 */

// Types
export type SortDirection = "asc" | "desc" | null

export interface Column<T> {
  /** Unique key for the column */
  key: keyof T | string
  /** Header text */
  header: string
  /** Header text (Arabic) */
  headerAr?: string
  /** Enable sorting */
  sortable?: boolean
  /** Enable filtering */
  filterable?: boolean
  /** Custom cell renderer */
  cell?: (row: T, index: number) => React.ReactNode
  /** Column width */
  width?: string | number
  /** Align content */
  align?: "start" | "center" | "end"
  /** Hide on mobile */
  hideOnMobile?: boolean
  /** Custom sort function */
  sortFn?: (a: T, b: T) => number
}

export interface DataTableProps<T> {
  /** Data array */
  data: T[]
  /** Column definitions */
  columns: Column<T>[]
  /** Loading state */
  isLoading?: boolean
  /** Enable row selection */
  selectable?: boolean
  /** Selected row IDs */
  selectedIds?: Set<string>
  /** Row ID key */
  idKey?: keyof T
  /** Callback when selection changes */
  onSelectionChange?: (ids: Set<string>) => void
  /** Callback when row is clicked */
  onRowClick?: (row: T) => void
  /** Enable pagination */
  pagination?: boolean
  /** Items per page */
  pageSize?: number
  /** Page size options */
  pageSizeOptions?: number[]
  /** Enable search */
  searchable?: boolean
  /** Search placeholder */
  searchPlaceholder?: string
  /** Enable export */
  exportable?: boolean
  /** Export filename */
  exportFilename?: string
  /** Empty state message */
  emptyMessage?: string
  /** Custom class name */
  className?: string
  /** Sticky header */
  stickyHeader?: boolean
  /** Max height for scrolling */
  maxHeight?: string | number
  /** Row actions renderer */
  rowActions?: (row: T) => React.ReactNode
}

// Sort Icon Component
const SortIcon = React.memo(function SortIcon({
  direction,
}: {
  direction: SortDirection
}) {
  if (direction === "asc") return <ChevronUp className="h-4 w-4" />
  if (direction === "desc") return <ChevronDown className="h-4 w-4" />
  return <ChevronsUpDown className="h-4 w-4 opacity-50" />
})

// Checkbox Component
const Checkbox = React.memo(function Checkbox({
  checked,
  indeterminate,
  onChange,
  className,
}: {
  checked: boolean
  indeterminate?: boolean
  onChange: (checked: boolean) => void
  className?: string
}) {
  const ref = React.useRef<HTMLInputElement>(null)

  React.useEffect(() => {
    if (ref.current) {
      ref.current.indeterminate = indeterminate || false
    }
  }, [indeterminate])

  return (
    <div className={cn("relative flex items-center justify-center", className)}>
      <input
        ref={ref}
        type="checkbox"
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
        className={cn(
          "h-4 w-4 rounded border border-input",
          "text-primary focus:ring-2 focus:ring-ring focus:ring-offset-2",
          "cursor-pointer"
        )}
      />
    </div>
  )
})

// Main Component
function DataTableComponent<T extends Record<string, unknown>>({
  data,
  columns,
  isLoading = false,
  selectable = false,
  selectedIds = new Set(),
  idKey = "id" as keyof T,
  onSelectionChange,
  onRowClick,
  pagination = true,
  pageSize: initialPageSize = 10,
  pageSizeOptions = [10, 25, 50, 100],
  searchable = true,
  searchPlaceholder = "Search...",
  exportable = false,
  exportFilename = "export",
  emptyMessage = "No data available",
  className,
  stickyHeader = false,
  maxHeight,
  rowActions,
}: DataTableProps<T>) {
  // State
  const [search, setSearch] = React.useState("")
  const [sortKey, setSortKey] = React.useState<string | null>(null)
  const [sortDirection, setSortDirection] = React.useState<SortDirection>(null)
  const [currentPage, setCurrentPage] = React.useState(1)
  const [pageSize, setPageSize] = React.useState(initialPageSize)
  const [columnFilters, setColumnFilters] = React.useState<
    Record<string, string>
  >({})

  // Detect Arabic
  const isArabic =
    typeof document !== "undefined" && document.documentElement.lang === "ar"

  // Filter data
  const filteredData = React.useMemo(() => {
    let result = [...data]

    // Apply search
    if (search.trim()) {
      const searchLower = search.toLowerCase()
      result = result.filter((row) =>
        columns.some((col) => {
          const value = row[col.key as keyof T]
          return String(value || "")
            .toLowerCase()
            .includes(searchLower)
        })
      )
    }

    // Apply column filters
    Object.entries(columnFilters).forEach(([key, filterValue]) => {
      if (filterValue) {
        result = result.filter((row) => {
          const value = row[key as keyof T]
          return String(value || "")
            .toLowerCase()
            .includes(filterValue.toLowerCase())
        })
      }
    })

    return result
  }, [data, search, columnFilters, columns])

  // Sort data
  const sortedData = React.useMemo(() => {
    if (!sortKey || !sortDirection) return filteredData

    const column = columns.find((c) => c.key === sortKey)
    if (!column) return filteredData

    return [...filteredData].sort((a, b) => {
      if (column.sortFn) {
        const result = column.sortFn(a, b)
        return sortDirection === "desc" ? -result : result
      }

      const aVal = a[sortKey as keyof T]
      const bVal = b[sortKey as keyof T]

      if (aVal === bVal) return 0
      if (aVal === null || aVal === undefined) return 1
      if (bVal === null || bVal === undefined) return -1

      const comparison = String(aVal).localeCompare(String(bVal))
      return sortDirection === "desc" ? -comparison : comparison
    })
  }, [filteredData, sortKey, sortDirection, columns])

  // Paginate data
  const paginatedData = React.useMemo(() => {
    if (!pagination) return sortedData
    const start = (currentPage - 1) * pageSize
    return sortedData.slice(start, start + pageSize)
  }, [sortedData, currentPage, pageSize, pagination])

  // Pagination info
  const totalPages = Math.ceil(sortedData.length / pageSize)
  const startItem = (currentPage - 1) * pageSize + 1
  const endItem = Math.min(currentPage * pageSize, sortedData.length)

  // Reset to page 1 when data changes
  React.useEffect(() => {
    setCurrentPage(1)
  }, [search, columnFilters, data.length])

  // Handle sort
  const handleSort = React.useCallback((key: string) => {
    setSortKey((prevKey) => {
      if (prevKey !== key) {
        setSortDirection("asc")
        return key
      }
      // Same key - cycle through directions
      setSortDirection((prevDir) => {
        if (prevDir === "asc") return "desc"
        if (prevDir === "desc") return null
        return "asc"
      })
      // Keep the same key unless we're clearing sort
      return prevKey
    })
  }, [])

  // Handle select all
  const handleSelectAll = React.useCallback(
    (checked: boolean) => {
      if (!onSelectionChange) return
      if (checked) {
        const allIds = new Set(
          paginatedData.map((row) => String(row[idKey]))
        )
        onSelectionChange(allIds)
      } else {
        onSelectionChange(new Set())
      }
    },
    [paginatedData, idKey, onSelectionChange]
  )

  // Handle row select
  const handleRowSelect = React.useCallback(
    (id: string, checked: boolean) => {
      if (!onSelectionChange) return
      const newIds = new Set(selectedIds)
      if (checked) {
        newIds.add(id)
      } else {
        newIds.delete(id)
      }
      onSelectionChange(newIds)
    },
    [selectedIds, onSelectionChange]
  )

  // Export to CSV
  const handleExport = React.useCallback(() => {
    const headers = columns.map((c) => c.header).join(",")
    const rows = sortedData.map((row) =>
      columns
        .map((col) => {
          const value = row[col.key as keyof T]
          const str = String(value || "")
          // Escape quotes and wrap in quotes if contains comma
          if (str.includes(",") || str.includes('"')) {
            return `"${str.replace(/"/g, '""')}"`
          }
          return str
        })
        .join(",")
    )
    const csv = [headers, ...rows].join("\n")
    const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" })
    const link = document.createElement("a")
    link.href = URL.createObjectURL(blob)
    link.download = `${exportFilename}-${new Date().toISOString().split("T")[0]}.csv`
    link.click()
  }, [sortedData, columns, exportFilename])

  // Selection state
  const allSelected =
    paginatedData.length > 0 &&
    paginatedData.every((row) => selectedIds.has(String(row[idKey])))
  const someSelected =
    paginatedData.some((row) => selectedIds.has(String(row[idKey]))) &&
    !allSelected

  return (
    <div className={cn("w-full", className)}>
      {/* Toolbar */}
      {(searchable || exportable) && (
        <div className="flex items-center justify-between gap-4 mb-4">
          {/* Search */}
          {searchable && (
            <div className="relative flex-1 max-w-sm">
              <Search className="absolute start-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <input
                type="text"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder={searchPlaceholder}
                className={cn(
                  "w-full ps-9 pe-9 py-2 text-sm rounded-lg",
                  "border border-input bg-background",
                  "focus:outline-none focus:ring-2 focus:ring-ring"
                )}
              />
              {search && (
                <button
                  onClick={() => setSearch("")}
                  className="absolute end-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                >
                  <X className="h-4 w-4" />
                </button>
              )}
            </div>
          )}

          {/* Actions */}
          <div className="flex items-center gap-2">
            {exportable && (
              <Button variant="outline" size="sm" onClick={handleExport}>
                <Download className="h-4 w-4 me-2" />
                {isArabic ? "تصدير" : "Export"}
              </Button>
            )}
          </div>
        </div>
      )}

      {/* Table */}
      <div
        className={cn(
          "relative rounded-lg border border-border overflow-hidden",
          maxHeight && "overflow-auto"
        )}
        style={{ maxHeight }}
      >
        <table className="w-full text-sm">
          <thead
            className={cn(
              "bg-muted/50 border-b border-border",
              stickyHeader && "sticky top-0 z-10"
            )}
          >
            <tr>
              {/* Selection column */}
              {selectable && (
                <th className="w-12 px-4 py-3">
                  <Checkbox
                    checked={allSelected}
                    indeterminate={someSelected}
                    onChange={handleSelectAll}
                  />
                </th>
              )}

              {/* Data columns */}
              {columns.map((column) => (
                <th
                  key={String(column.key)}
                  className={cn(
                    "px-4 py-3 text-start font-medium text-muted-foreground",
                    column.hideOnMobile && "hidden md:table-cell",
                    column.sortable && "cursor-pointer select-none hover:text-foreground",
                    column.align === "center" && "text-center",
                    column.align === "end" && "text-end"
                  )}
                  style={{ width: column.width }}
                  onClick={
                    column.sortable
                      ? () => handleSort(String(column.key))
                      : undefined
                  }
                >
                  <div
                    className={cn(
                      "flex items-center gap-1",
                      column.align === "center" && "justify-center",
                      column.align === "end" && "justify-end"
                    )}
                  >
                    {isArabic && column.headerAr ? column.headerAr : column.header}
                    {column.sortable && (
                      <SortIcon
                        direction={
                          sortKey === column.key ? sortDirection : null
                        }
                      />
                    )}
                  </div>
                </th>
              ))}

              {/* Actions column */}
              {rowActions && (
                <th className="w-12 px-4 py-3 text-end">
                  <span className="sr-only">Actions</span>
                </th>
              )}
            </tr>
          </thead>

          <tbody className="divide-y divide-border">
            {isLoading ? (
              // Loading skeleton
              Array.from({ length: pageSize }).map((_, i) => (
                <tr key={i}>
                  {selectable && (
                    <td className="px-4 py-3">
                      <Skeleton className="h-4 w-4" />
                    </td>
                  )}
                  {columns.map((col) => (
                    <td
                      key={String(col.key)}
                      className={cn(
                        "px-4 py-3",
                        col.hideOnMobile && "hidden md:table-cell"
                      )}
                    >
                      <Skeleton className="h-4 w-full max-w-[200px]" />
                    </td>
                  ))}
                  {rowActions && (
                    <td className="px-4 py-3">
                      <Skeleton className="h-4 w-4 ms-auto" />
                    </td>
                  )}
                </tr>
              ))
            ) : paginatedData.length === 0 ? (
              // Empty state
              <tr>
                <td
                  colSpan={
                    columns.length + (selectable ? 1 : 0) + (rowActions ? 1 : 0)
                  }
                  className="px-4 py-12 text-center text-muted-foreground"
                >
                  {emptyMessage}
                </td>
              </tr>
            ) : (
              // Data rows
              paginatedData.map((row, index) => {
                const rowId = String(row[idKey])
                const isSelected = selectedIds.has(rowId)

                return (
                  <tr
                    key={rowId}
                    className={cn(
                      "transition-colors",
                      isSelected && "bg-primary/5",
                      onRowClick && "cursor-pointer hover:bg-muted/50"
                    )}
                    onClick={
                      onRowClick ? () => onRowClick(row) : undefined
                    }
                  >
                    {/* Selection */}
                    {selectable && (
                      <td
                        className="px-4 py-3"
                        onClick={(e) => e.stopPropagation()}
                      >
                        <Checkbox
                          checked={isSelected}
                          onChange={(checked) =>
                            handleRowSelect(rowId, checked)
                          }
                        />
                      </td>
                    )}

                    {/* Data cells */}
                    {columns.map((column) => (
                      <td
                        key={String(column.key)}
                        className={cn(
                          "px-4 py-3",
                          column.hideOnMobile && "hidden md:table-cell",
                          column.align === "center" && "text-center",
                          column.align === "end" && "text-end"
                        )}
                      >
                        {column.cell
                          ? column.cell(row, index)
                          : String(row[column.key as keyof T] ?? "")}
                      </td>
                    ))}

                    {/* Actions */}
                    {rowActions && (
                      <td
                        className="px-4 py-3 text-end"
                        onClick={(e) => e.stopPropagation()}
                      >
                        {rowActions(row)}
                      </td>
                    )}
                  </tr>
                )
              })
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {pagination && !isLoading && sortedData.length > 0 && (
        <div className="flex items-center justify-between gap-4 mt-4 text-sm">
          {/* Info */}
          <div className="text-muted-foreground">
            {isArabic
              ? `عرض ${startItem} إلى ${endItem} من ${sortedData.length}`
              : `Showing ${startItem} to ${endItem} of ${sortedData.length}`}
          </div>

          {/* Controls */}
          <div className="flex items-center gap-2">
            {/* Page size selector */}
            <select
              value={pageSize}
              onChange={(e) => {
                setPageSize(Number(e.target.value))
                setCurrentPage(1)
              }}
              className={cn(
                "px-2 py-1 rounded border border-input bg-background",
                "text-sm focus:outline-none focus:ring-2 focus:ring-ring"
              )}
            >
              {pageSizeOptions.map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>

            {/* Navigation */}
            <div className="flex items-center gap-1">
              <Button
                variant="outline"
                size="icon-sm"
                onClick={() => setCurrentPage(1)}
                disabled={currentPage === 1}
              >
                <ChevronsLeft className="h-4 w-4" />
              </Button>
              <Button
                variant="outline"
                size="icon-sm"
                onClick={() => setCurrentPage((p) => p - 1)}
                disabled={currentPage === 1}
              >
                <ChevronLeft className="h-4 w-4" />
              </Button>

              <span className="px-3 text-muted-foreground">
                {currentPage} / {totalPages}
              </span>

              <Button
                variant="outline"
                size="icon-sm"
                onClick={() => setCurrentPage((p) => p + 1)}
                disabled={currentPage === totalPages}
              >
                <ChevronRight className="h-4 w-4" />
              </Button>
              <Button
                variant="outline"
                size="icon-sm"
                onClick={() => setCurrentPage(totalPages)}
                disabled={currentPage === totalPages}
              >
                <ChevronsRight className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

const DataTable = React.memo(DataTableComponent) as typeof DataTableComponent

export { DataTable }
export default DataTable
