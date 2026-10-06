import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { apiRequest } from '../api'
import type { LoginResponse, User } from '../types'
import { AuthContext } from './authContext'

export function AuthProvider({ children }: { children: ReactNode }) {
  const storedToken = localStorage.getItem('notice-board-token')
  const [user, setUser] = useState<User | null>(() => {
    const savedUser = localStorage.getItem('notice-board-user')
    return savedUser ? (JSON.parse(savedUser) as User) : null
  })
  const [loading, setLoading] = useState(Boolean(storedToken))

  const logout = useCallback(() => {
    localStorage.removeItem('notice-board-token')
    localStorage.removeItem('notice-board-user')
    setUser(null)
  }, [])

  useEffect(() => {
    if (!storedToken) {
      return
    }

    apiRequest<User>('/auth/me')
      .then((currentUser) => {
        setUser(currentUser)
        localStorage.setItem('notice-board-user', JSON.stringify(currentUser))
      })
      .catch(logout)
      .finally(() => setLoading(false))
  }, [logout, storedToken])

  const login = useCallback(async (username: string, password: string) => {
    const response = await apiRequest<LoginResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ username, password }),
    })
    localStorage.setItem('notice-board-token', response.token)
    localStorage.setItem('notice-board-user', JSON.stringify(response.user))
    setUser(response.user)
    return response.user
  }, [])

  const value = useMemo(
    () => ({ user, loading, login, logout }),
    [user, loading, login, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
