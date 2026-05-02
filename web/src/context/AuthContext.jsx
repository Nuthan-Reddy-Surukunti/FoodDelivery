import React, { createContext, useState, useCallback, useEffect } from 'react'
import authApi from '../services/authApi'

export const AuthContext = createContext()

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(() => {
    try {
      const savedUser = localStorage.getItem('user')
      return savedUser ? JSON.parse(savedUser) : null
    } catch (e) {
      console.error('Failed to parse user from localStorage', e)
      return null
    }
  })
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState(null)

  const login = useCallback(async (email, password) => {
    setIsLoading(true)
    setError(null)
    try {
      const response = await authApi.login(email, password)
      
      // Handle different login response scenarios
      if (response.isTwoFactorRequired && response.tempToken) {
        // 2FA required with tempToken - means 2FA authenticator app
        return {
          status: 'VERIFY_2FA',
          tempToken: response.tempToken,
          userId: response.userId,
          email: email,
          role: response.role
        }
      } else if (response.isTwoFactorRequired && !response.tempToken) {
        // Email verification required (RestaurantPartner/Admin first login after approval)
        return {
          status: 'VERIFY_EMAIL',
          userId: response.userId,
          email: email,
          role: response.role
        }
      } else if (response.token) {
        // Direct login successful
        setUser(response.user)
        localStorage.setItem('user', JSON.stringify(response.user))
        
        return {
          status: 'SUCCESS',
          user: response.user
        }
      }
      
      throw new Error('Unexpected login response')
    } catch (err) {
      const errorMessage = err.message || 'Login failed'
      setError(errorMessage)
      throw new Error(errorMessage)
    } finally {
      setIsLoading(false)
    }
  }, [])

  const googleLogin = useCallback(async (idToken) => {
    setIsLoading(true)
    setError(null)
    try {
      const response = await authApi.googleLogin(idToken)

      if (response.isTwoFactorRequired && response.tempToken) {
        return {
          status: 'VERIFY_2FA',
          tempToken: response.tempToken,
          userId: response.userId,
          role: response.role
        }
      } else if (response.token) {
        setUser(response.user)
        localStorage.setItem('user', JSON.stringify(response.user))
        
        return {
          status: 'SUCCESS',
          user: response.user
        }
      }
      
      throw new Error('Unexpected login response')
    } catch (err) {
      const errorMessage = err.message || 'Google Login failed'
      setError(errorMessage)
      throw new Error(errorMessage)
    } finally {
      setIsLoading(false)
    }
  }, [])

  const register = useCallback(async (fullName, email, mobileNumber, password, role = 'Customer') => {
    setIsLoading(true)
    setError(null)
    try {
      const response = await authApi.register(fullName, email, mobileNumber, password, role)
      
      return {
        success: response.success,
        message: response.message,
        isPendingApproval: response.isPendingApproval,
        requiresEmailVerification: response.requiresEmailVerification
      }
    } catch (err) {
      const errorMessage = err.message || 'Registration failed'
      setError(errorMessage)
      throw new Error(errorMessage)
    } finally {
      setIsLoading(false)
    }
  }, [])

  const logout = useCallback(async () => {
    try {
      await authApi.logout()
    } catch (err) {
      console.error('Logout error:', err)
    } finally {
      setUser(null)
      localStorage.removeItem('user')
    }
  }, [])

  const setAuthUser = useCallback((userData) => {
    setUser(userData)
    localStorage.setItem('user', JSON.stringify(userData))
  }, [])

  const value = {
    user,
    isLoading,
    error,
    login,
    googleLogin,
    register,
    logout,
    setAuthUser,
    isAuthenticated: !!user
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => {
  const context = React.useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return context
}
