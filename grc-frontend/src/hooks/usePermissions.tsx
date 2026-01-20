"use client"

import React, { useState, useEffect } from 'react'
import PermissionApi from '@/lib/api/permissions'

/**
 * Hook for managing permissions
 * Provides permission checking and user permissions state
 */
export function usePermissions() {
  const [userPermissions, setUserPermissions] = useState<string[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Load user permissions on mount
  useEffect(() => {
    loadUserPermissions()
  }, [])

  const loadUserPermissions = async () => {
    try {
      setIsLoading(true)
      setError(null)
      const permissions = await PermissionApi.getUserPermissions()
      setUserPermissions(permissions)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load permissions')
    } finally {
      setIsLoading(false)
    }
  }

  const hasPermission = (permission: string): boolean => {
    return userPermissions.includes(permission)
  }

  const hasAnyPermission = (permissions: string[]): boolean => {
    return permissions.some(permission => userPermissions.includes(permission))
  }

  const hasAllPermissions = (permissions: string[]): boolean => {
    return permissions.every(permission => userPermissions.includes(permission))
  }

  const refreshPermissions = () => {
    loadUserPermissions()
  }

  return {
    userPermissions,
    isLoading,
    error,
    hasPermission,
    hasAnyPermission,
    hasAllPermissions,
    refreshPermissions
  }
}

/**
 * Hook for checking a specific permission
 * Returns boolean and loading state
 */
export function usePermission(permission: string) {
  const [hasPermission, setHasPermission] = useState(false)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    checkPermission()
  }, [permission])

  const checkPermission = async () => {
    try {
      setIsLoading(true)
      const result = await PermissionApi.checkPermission(permission)
      setHasPermission(result)
    } catch (err) {
      setHasPermission(false)
    } finally {
      setIsLoading(false)
    }
  }

  return { hasPermission, isLoading }
}

/**
 * Hook for checking multiple permissions
 */
export function usePermissionsCheck(permissions: string[], requireAll: boolean = false) {
  const [result, setResult] = useState(false)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    checkPermissions()
  }, [permissions, requireAll])

  const checkPermissions = async () => {
    try {
      setIsLoading(true)
      const hasPermissions = requireAll
        ? await PermissionApi.checkAllPermissions(permissions)
        : await PermissionApi.checkAnyPermission(permissions)
      setResult(hasPermissions)
    } catch (err) {
      setResult(false)
    } finally {
      setIsLoading(false)
    }
  }

  return { hasPermissions: result, isLoading }
}

/**
 * Permission-based component wrapper
 * Renders children only if user has required permissions
 */
export interface PermissionGuardProps {
  permissions: string[]
  requireAll?: boolean
  children: React.ReactNode
  fallback?: React.ReactNode
}

export function PermissionGuard({ 
  permissions, 
  requireAll = false, 
  children, 
  fallback = null 
}: PermissionGuardProps) {
  const { hasPermissions, isLoading } = usePermissionsCheck(permissions, requireAll)

  if (isLoading) {
    return <div className="animate-pulse">Checking permissions...</div>
  }

  if (!hasPermissions) {
    return <>{fallback}</>
  }

  return <>{children}</>
}

/**
 * Higher-order component for permission-based rendering
 */
export function withPermission<P extends object>(
  Component: React.ComponentType<P>,
  permissions: string[],
  requireAll: boolean = false,
  fallback?: React.ReactNode
) {
  return function PermissionWrapper(props: P) {
    return (
      <PermissionGuard permissions={permissions} requireAll={requireAll} fallback={fallback}>
        <Component {...props} />
      </PermissionGuard>
    )
  }
}

export default {
  usePermissions,
  usePermission,
  usePermissionsCheck,
  PermissionGuard,
  withPermission
}
