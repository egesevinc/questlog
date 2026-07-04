import { createContext, useContext, useState, type ReactNode } from 'react'
import type { AuthResponse } from '../api/auth'
import { TOKEN_KEY, USER_KEY, clearSession } from './storage'

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

function loadUser(): AuthUser | null {
  const raw = localStorage.getItem(USER_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as AuthUser
  } catch {
    // Corrupt/legacy value — drop it rather than crashing the whole app.
    clearSession()
    return null
  }
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
    clearSession()
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
