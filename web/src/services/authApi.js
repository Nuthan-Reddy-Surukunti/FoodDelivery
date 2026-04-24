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
   * Logout user (client-side only, backend doesn't need logout endpoint)
   */
  logout: () => {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  },
}

export default authApi
