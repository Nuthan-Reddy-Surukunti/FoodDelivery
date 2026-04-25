import React, { createContext, useState, useCallback, useEffect } from 'react'
import authApi from '../services/authApi'

export const AuthContext = createContext()

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null)
  const [token, setToken] = useState(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState(null)

  // Load token from localStorage on mount
  useEffect(() => {
    const savedToken = localStorage.getItem('token')
    const savedUser = localStorage.getItem('user')
    
    if (savedToken) {
      setToken(savedToken)
      if (savedUser) {
        setUser(JSON.parse(savedUser))
      }
    }
  }, [])

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
        setToken(response.token)
        localStorage.setItem('token', response.token)
        localStorage.setItem('user', JSON.stringify(response.user))
        
        return {
          status: 'SUCCESS',
          user: response.user,
          token: response.token
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

  const logout = useCallback(() => {
    setUser(null)
    setToken(null)
    authApi.logout()
  }, [])

  const setAuthUser = useCallback((userData, authToken) => {
    setUser(userData)
    setToken(authToken)
    localStorage.setItem('token', authToken)
    localStorage.setItem('user', JSON.stringify(userData))
  }, [])

  const value = {
    user,
    token,
    isLoading,
    error,
    login,
    register,
    logout,
    setAuthUser,
    isAuthenticated: !!token
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
