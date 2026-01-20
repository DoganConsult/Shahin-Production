/**
 * Permission API for frontend
 * Integrates with backend permission service
 */

import { api } from './client'

export interface Permission {
  key: string
  category: string
  description: string
}

export interface PermissionCategory {
  name: string
  description: string
}

export class PermissionApi {
  /**
   * Check if current user has a specific permission
   */
  static async checkPermission(permission: string): Promise<boolean> {
    const response = await api.get<{ hasPermission: boolean }>(`/api/permission/check?permission=${permission}`)
    return response.hasPermission
  }

  /**
   * Check if current user has any of the specified permissions
   */
  static async checkAnyPermission(permissions: string[]): Promise<boolean> {
    const response = await api.post<{ hasPermission: boolean }>('/api/permission/check-any', permissions)
    return response.hasPermission
  }

  /**
   * Check if current user has all specified permissions
   */
  static async checkAllPermissions(permissions: string[]): Promise<boolean> {
    const response = await api.post<{ hasPermission: boolean }>('/api/permission/check-all', permissions)
    return response.hasPermission
  }

  /**
   * Get all permissions for current user
   */
  static async getUserPermissions(): Promise<string[]> {
    const response = await api.get<{ permissions: string[] }>('/api/permission/my-permissions')
    return response.permissions
  }

  /**
   * Get available permission categories
   */
  static async getPermissionCategories(): Promise<PermissionCategory[]> {
    const response = await api.get<PermissionCategory[]>('/api/permission/categories')
    return response
  }

  /**
   * Get all available permissions (admin only)
   */
  static async getAllPermissions(): Promise<Permission[]> {
    const response = await api.get<Permission[]>('/api/permission/all')
    return response
  }

  // Permission constants for easy reference
  static readonly PERMISSIONS = {
    // Platform permissions
    PLATFORM_ADMIN: 'platform.admin',
    PLATFORM_USERS_CREATE: 'platform.users.create',
    PLATFORM_USERS_READ: 'platform.users.read',
    PLATFORM_USERS_UPDATE: 'platform.users.update',
    PLATFORM_USERS_DELETE: 'platform.users.delete',
    PLATFORM_TENANTS_CREATE: 'platform.tenants.create',
    PLATFORM_TENANTS_READ: 'platform.tenants.read',
    PLATFORM_TENANTS_UPDATE: 'platform.tenants.update',
    PLATFORM_TENANTS_DELETE: 'platform.tenants.delete',
    PLATFORM_SETTINGS_READ: 'platform.settings.read',
    PLATFORM_SETTINGS_UPDATE: 'platform.settings.update',

    // Tenant permissions
    TENANT_VIEW: 'tenant.view',
    TENANT_UPDATE: 'tenant.update',
    TENANT_USERS_CREATE: 'tenant.users.create',
    TENANT_USERS_READ: 'tenant.users.read',
    TENANT_USERS_UPDATE: 'tenant.users.update',
    TENANT_USERS_DELETE: 'tenant.users.delete',

    // Onboarding permissions
    ONBOARDING_VIEW: 'onboarding.view',
    ONBOARDING_UPDATE: 'onboarding.update',
    ONBOARDING_ADMIN: 'onboarding.admin',

    // GRC permissions
    GRC_CONTROLS_READ: 'grc.controls.read',
    GRC_CONTROLS_UPDATE: 'grc.controls.update',
    GRC_RISKS_READ: 'grc.risks.read',
    GRC_RISKS_UPDATE: 'grc.risks.update',
    GRC_COMPLIANCE_READ: 'grc.compliance.read',
    GRC_COMPLIANCE_UPDATE: 'grc.compliance.update',

    // Basic permissions
    PROFILE_VIEW: 'profile.view',
    PROFILE_UPDATE: 'profile.update',
    DASHBOARD_VIEW: 'dashboard.view'
  }

  // Permission groups
  static readonly GROUPS = {
    PLATFORM_ADMIN: [
      this.PERMISSIONS.PLATFORM_ADMIN
    ],
    TENANT_ADMIN: [
      this.PERMISSIONS.TENANT_VIEW,
      this.PERMISSIONS.TENANT_UPDATE,
      this.PERMISSIONS.TENANT_USERS_READ,
      this.PERMISSIONS.TENANT_USERS_UPDATE
    ],
    ONBOARDING: [
      this.PERMISSIONS.ONBOARDING_VIEW,
      this.PERMISSIONS.ONBOARDING_UPDATE
    ],
    GRC_READ: [
      this.PERMISSIONS.GRC_CONTROLS_READ,
      this.PERMISSIONS.GRC_RISKS_READ,
      this.PERMISSIONS.GRC_COMPLIANCE_READ
    ],
    GRC_WRITE: [
      this.PERMISSIONS.GRC_CONTROLS_UPDATE,
      this.PERMISSIONS.GRC_RISKS_UPDATE,
      this.PERMISSIONS.GRC_COMPLIANCE_UPDATE
    ],
    BASIC: [
      this.PERMISSIONS.PROFILE_VIEW,
      this.PERMISSIONS.PROFILE_UPDATE,
      this.PERMISSIONS.DASHBOARD_VIEW
    ]
  }
}

export default PermissionApi
