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

      if (!response.data.success) {
        throw new Error(response.data.message || 'Login failed')
      }

      // Response format from backend varies based on flow
      // { success, message, token, refreshToken, role, userId, isTwoFactorRequired, tempToken }

      if (response.data.isTwoFactorRequired) {
        // 2FA or Email verification required
        return {
          success: true,
          isTwoFactorRequired: true,
          tempToken: response.data.tempToken || null, // Has TempToken if 2FA app, null if email verification
          userId: response.data.userId,
          role: response.data.role || 'customer',
        }
      } else if (response.data.token) {
        // Direct login successful
        return {
          success: true,
          token: response.data.token,
          user: {
            id: response.data.userId,
            email: response.data.email || email,
            name: response.data.fullName || response.data.name || email.split('@')[0], // Use fullName, or email prefix as fallback
            phone: response.data.mobileNumber || '',
            role: response.data.role || 'customer',
            isTwoFactorEnabled: response.data.isTwoFactorEnabled || false,
            ...response.data // Spread any additional fields from backend
          },
        }
      } else {
        throw new Error('Unexpected login response')
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
   * @param {string} role - User role (Customer, DeliveryAgent, RestaurantPartner, Admin)
   * @returns {Promise} Response with token and user data
   */
  register: async (fullName, email, mobileNumber, password, role = 'Customer') => {
    try {
      const response = await api.post('/gateway/auth/register', {
        fullName,
        email,
        mobileNumber,
        password,
        role,
      })

      if (response.data.success) {
        // Check if account is pending approval (RestaurantPartner/Admin)
        if (response.data.message.includes('pending approval')) {
          return {
            success: true,
            message: response.data.message,
            isPendingApproval: true,
            requiresEmailVerification: false
          }
        } else if (response.data.message.includes('verify your email')) {
          // Customer/DeliveryAgent - needs email verification
          return {
            success: true,
            message: response.data.message,
            isPendingApproval: false,
            requiresEmailVerification: true
          }
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
   * Approve a pending partner account
   * @param {string} userId - User ID
   * @returns {Promise} Response with approval status
   */
  approvePartnerAccount: async (userId) => {
    try {
      const response = await api.post('/gateway/auth/admin/restaurants/approve', {
        userId,
        notes: "Approved from Admin Dashboard"
      })
      return response.data
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Failed to approve partner'
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
        // Check if token and user data are returned (for login scenarios)
        if (response.data.token && response.data.userId) {
          return {
            success: true,
            token: response.data.token,
            user: {
              id: response.data.userId,
              email: response.data.email,
              name: response.data.fullName || response.data.email.split('@')[0],
              phone: response.data.mobileNumber || '',
              role: response.data.role || 'customer',
            },
            message: response.data.message || 'Email verified successfully',
          }
        } else {
          // Old response format - just success message
          return {
            success: true,
            message: response.data.message || 'Email verified successfully',
          }
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
            name: response.data.fullName || response.data.name || response.data.email.split('@')[0],
            phone: response.data.mobileNumber || '',
            role: response.data.role || 'customer',
            ...response.data // Spread any additional fields from backend
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

  /**
   * Update user profile
   * @param {string} userId - User ID
   * @param {string} fullName - Full name
   * @param {string} mobileNumber - Phone number
   * @returns {Promise} Response with updated user data
   */
  updateProfile: async (userId, fullName, mobileNumber) => {
    try {
      const response = await api.put('/gateway/auth/update-profile', {
        userId,
        fullName,
        mobileNumber,
      })

      if (response.data.success) {
        return {
          success: true,
          user: {
            id: response.data.userId,
            email: response.data.email,
            name: response.data.fullName || response.data.name,
            phone: response.data.mobileNumber,
            role: response.data.role,
          },
        }
      } else {
        throw new Error(response.data.message || 'Profile update failed')
      }
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Profile update failed'
      throw new Error(message)
    }
  },

  /**
   * Toggle two-factor authentication
   * @param {string} userId - User ID
   * @param {boolean} enable - Enable (true) or disable (false) 2FA
   * @returns {Promise} Response with updated status
   */
  toggleTwoFactor: async (userId, enable) => {
    try {
      const response = await api.put('/gateway/auth/toggle-2fa', {
        userId,
        enable,
      })

      if (response.data.success) {
        return {
          success: true,
          message: response.data.message,
          isTwoFactorEnabled: enable,
        }
      } else {
        throw new Error(response.data.message || '2FA toggle failed')
      }
    } catch (error) {
      const message = error.response?.data?.message || error.message || '2FA toggle failed'
      throw new Error(message)
    }
  },

  /**
   * Change user password
   * @param {string} currentPassword - Current password
   * @param {string} newPassword - New password
   * @param {string} confirmPassword - Confirm new password
   * @returns {Promise} Response with change status
   */
  changePassword: async (currentPassword, newPassword, confirmPassword) => {
    try {
      const response = await api.put('/gateway/auth/change-password', {
        currentPassword,
        newPassword,
        confirmPassword,
      })

      if (response.data.success) {
        return {
          success: true,
          message: response.data.message || 'Password changed successfully'
        }
      } else {
        throw new Error(response.data.message || 'Password change failed')
      }
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Password change failed'
      throw new Error(message)
    }
  },

  /**
   * Delete user account
   * @param {string} email - User email
   * @param {string} password - User password for confirmation
   * @returns {Promise} Response with deletion status
   */
  deleteAccount: async (email, password) => {
    try {
      const response = await api.delete('/gateway/auth/delete-account', {
        data: {
          email,
          password,
        }
      })

      if (response.data.success) {
        return {
          success: true,
          message: response.data.message || 'Account deleted successfully'
        }
      } else {
        throw new Error(response.data.message || 'Account deletion failed')
      }
    } catch (error) {
      const message = error.response?.data?.message || error.message || 'Account deletion failed'
      throw new Error(message)
    }
  },
}

export default authApi
