import api from './api'

/**
 * Authentication API service
 * Handles login, register, and other auth operations
 */
export const authApi = {
  /**
   * Login with email and password
   * @param {string} email - User email
   * @param {string} password - User password
   * @returns {Promise} Response with token, user, and role
   */
  login: async (email, password) => {
    try {
      const response = await api.post('/gateway/auth/login', {
        email,
        password,
      })

      // Response format from backend:
      // { success, message, token, refreshToken, role, userId, isTwoFactorRequired }
      if (response.data.success && response.data.token) {
        return {
          success: true,
          token: response.data.token,
          user: {
            id: response.data.userId,
            email,
            role: response.data.role || 'customer',
          },
          isTwoFactorRequired: response.data.isTwoFactorRequired || false,
        }
      } else {
        throw new Error(response.data.message || 'Login failed')
      }
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Login failed'
      throw new Error(message)
    }
  },

  /**
   * Register new user
   * @param {string} fullName - User full name
   * @param {string} email - User email
   * @param {string} mobileNumber - User phone number
   * @param {string} password - User password
   * @param {string} role - User role (customer, restaurant_partner, delivery_agent)
   * @returns {Promise} Response with token and user data
   */
  register: async (fullName, email, mobileNumber, password, role = 'customer') => {
    try {
      const response = await api.post('/gateway/auth/register', {
        fullName,
        email,
        mobileNumber,
        password,
        role,
      })

      if (response.data.success && response.data.token) {
        return {
          success: true,
          token: response.data.token,
          user: {
            id: response.data.userId,
            email,
            name: fullName,
            role: response.data.role || role,
          },
        }
      } else {
        throw new Error(response.data.message || 'Registration failed')
      }
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Registration failed'
      throw new Error(message)
    }
  },

  /**
   * Request password reset
   * @param {string} email - User email
   * @returns {Promise} Response with reset link info
   */
  forgotPassword: async (email) => {
    try {
      const response = await api.post('/gateway/auth/forgot-password', { email })

      if (response.data.success) {
        return {
          success: true,
          message: response.data.message || 'Password reset link sent to your email',
        }
      } else {
        throw new Error(response.data.message || 'Failed to send reset link')
      }
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Failed to send reset link'
      throw new Error(message)
    }
  },

  /**
   * Reset password with token
   * @param {string} email - User email
   * @param {string} token - Reset token from email link
   * @param {string} newPassword - New password
   * @returns {Promise} Response with success message
   */
  resetPassword: async (email, token, newPassword) => {
    try {
      const response = await api.post('/gateway/auth/reset-password', {
        email,
        token,
        newPassword,
      })

      if (response.data.success) {
        return {
          success: true,
          message: response.data.message || 'Password reset successfully',
        }
      } else {
        throw new Error(response.data.message || 'Failed to reset password')
      }
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Failed to reset password'
      throw new Error(message)
    }
  },

  /**
   * Verify email with OTP
   * @param {string} email - User email
   * @param {string} otp - One-time password
   * @returns {Promise} Response with verification status
   */
  verifyEmail: async (email, otp) => {
    try {
      const response = await api.post('/gateway/auth/verify-email', {
        email,
        otp,
      })

      if (response.data.success) {
        return {
          success: true,
          message: response.data.message || 'Email verified successfully',
        }
      } else {
        throw new Error(response.data.message || 'Email verification failed')
      }
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Email verification failed'
      throw new Error(message)
    }
  },

  /**
   * Verify two-factor authentication OTP
   * @param {string} tempToken - Temporary token from login response
   * @param {string} otp - Two-factor OTP
   * @returns {Promise} Response with final auth token
   */
  verifyTwoFactor: async (tempToken, otp) => {
    try {
      const response = await api.post('/gateway/auth/verify-2fa', {
        tempToken,
        otp,
      })

      if (response.data.success && response.data.token) {
        return {
          success: true,
          token: response.data.token,
          user: {
            id: response.data.userId,
            email: response.data.email,
            role: response.data.role || 'customer',
          },
        }
      } else {
        throw new Error(response.data.message || '2FA verification failed')
      }
    } catch (error) {
      const message = error.response?.data?.message || error.message || '2FA verification failed'
      throw new Error(message)
    }
  },

  /**
   * Logout user (client-side only)
   */
  logout: () => {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  },
}

export default authApi
