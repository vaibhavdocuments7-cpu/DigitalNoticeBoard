import { Navigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import type { Role } from '../types'

export function ProtectedRoute({
  children,
  role,
}: {
  children: React.ReactNode
  role?: Role
}) {
  const { user, loading } = useAuth()

  if (loading) {
    return (
      <div className="page-center">
        <div className="spinner-border text-primary" aria-label="Loading" />
      </div>
    )
  }

  if (!user) {
    return <Navigate to="/login" replace />
  }

  if (role && user.role !== role) {
    return <Navigate to="/board" replace />
  }

  return children
}
