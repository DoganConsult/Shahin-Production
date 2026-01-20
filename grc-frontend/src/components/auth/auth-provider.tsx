"use client"

import { createContext, useContext, useEffect, useState } from 'react'
import { getAccessToken, getUserInfo, signOut } from '@/lib/api/oidc-client'
import { OIDCUser } from '@/lib/api/oidc-client'

interface AuthContextType {
  user: OIDCUser | null
  isLoading: boolean
  isAuthenticated: boolean
  signOut: () => void
  refreshUser: () => Promise<void>
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<OIDCUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const refreshUser = async () => {
    try {
      const token = await getAccessToken()
      if (token) {
        const userInfo = await getUserInfo()
        setUser(userInfo)
      } else {
        setUser(null)
      }
    } catch (error) {
      console.error('Failed to refresh user:', error)
      setUser(null)
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    refreshUser()
  }, [])

  const handleSignOut = () => {
    setUser(null)
    signOut()
  }

  const value: AuthContextType = {
    user,
    isLoading,
    isAuthenticated: !!user,
    signOut: handleSignOut,
    refreshUser,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
