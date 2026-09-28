import { createContext, use, useCallback, useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { authApi } from '../lib/authApi'
import { ApiError } from '../lib/apiClient'
import type { AuthUser, LoginPayload, RegisterPayload } from '../types/auth'

interface AuthContextValue {
  user: AuthUser | null
  isLoadingSession: boolean
  login: (payload: LoginPayload) => Promise<void>
  register: (payload: RegisterPayload) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null)
  const [isLoadingSession, setIsLoadingSession] = useState(true)

  useEffect(() => {
    let isMounted = true

    authApi
      .me()
      .then((profile) => {
        if (isMounted) setUser(profile)
      })
      .catch((error: unknown) => {
        if (isMounted && !(error instanceof ApiError && error.status === 401)) {
          console.error('Failed to load session', error)
        }
      })
      .finally(() => {
        if (isMounted) setIsLoadingSession(false)
      })

    return () => {
      isMounted = false
    }
  }, [])

  const login = useCallback(async (payload: LoginPayload) => {
    const profile = await authApi.login(payload)
    setUser(profile)
  }, [])

  const register = useCallback(async (payload: RegisterPayload) => {
    const profile = await authApi.register(payload)
    setUser(profile)
  }, [])

  const logout = useCallback(async () => {
    await authApi.logout()
    setUser(null)
  }, [])

  return (
    <AuthContext value={{ user, isLoadingSession, login, register, logout }}>
      {children}
    </AuthContext>
  )
}

export function useAuth(): AuthContextValue {
  const context = use(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
