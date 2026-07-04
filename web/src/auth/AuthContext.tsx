import { createContext, useContext, useState, type ReactNode } from 'react'
import type { AuthResponse } from '../api/auth'

interface AuthUser {
  userId: string
  username: string
}

interface AuthContextValue {
  user: AuthUser | null
  setSession: (auth: AuthResponse) => void
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

const TOKEN_KEY = 'questlog_token'
const USER_KEY = 'questlog_user'

function loadUser(): AuthUser | null {
  const raw = localStorage.getItem(USER_KEY)
  return raw ? (JSON.parse(raw) as AuthUser) : null
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(loadUser())

  const setSession = (auth: AuthResponse) => {
    localStorage.setItem(TOKEN_KEY, auth.token)
    const nextUser = { userId: auth.userId, username: auth.username }
    localStorage.setItem(USER_KEY, JSON.stringify(nextUser))
    setUser(nextUser)
  }

  const logout = () => {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, setSession, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
