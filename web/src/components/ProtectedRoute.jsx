import { Navigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getRoleHomePath } from '../utils/authRoutes'

/**
 * ProtectedRoute - Redirects to login if not authenticated
 * Optionally checks for specific user roles
 */
export const ProtectedRoute = ({ children, requiredRole = null }) => {
  const { isAuthenticated, user } = useAuth()

  // Not authenticated - redirect to login
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  // Check role if specified
  if (requiredRole && user?.role !== requiredRole) {
    return <Navigate to={getRoleHomePath(user?.role)} replace />
  }

  return children
}

/**
 * PublicRoute - Redirects to home if already authenticated
 * Used for auth pages (login, register, etc) that shouldn't be accessible when logged in
 */
export const PublicRoute = ({ children }) => {
  const { isAuthenticated, user } = useAuth()

  if (isAuthenticated) {
    return <Navigate to={getRoleHomePath(user?.role)} replace />
  }

  return children
}
