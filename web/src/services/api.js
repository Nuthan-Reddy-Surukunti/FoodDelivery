import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'

/**
 * Axios instance configured for API communication
 * Handles JWT token injection and error handling
 */
const api = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
})

/**
 * Request interceptor
 */
api.interceptors.request.use(
  (config) => {
    return config
  },
  (error) => Promise.reject(error)
)

/**
 * Response interceptor: Handle 401 Unauthorized
 * Clear token and redirect to login on unauthorized response
 */
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Session is invalid or expired
      localStorage.removeItem('user')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default api
