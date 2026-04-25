import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import authApi from '../services/authApi'

export const DeleteAccountPage = () => {
  const navigate = useNavigate()
  const { user, logout } = useAuth()
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState(null)
  const [success, setSuccess] = useState(false)

  const handlePasswordChange = (e) => {
    setPassword(e.target.value)
    setError(null)
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError(null)
    setIsLoading(true)

    if (!password.trim()) {
      setError('Password is required')
      setIsLoading(false)
      return
    }

    try {
      const result = await authApi.deleteAccount(user?.email, password)
      
      if (result.success) {
        setSuccess(true)
        // Logout after successful deletion
        logout()
        // Redirect to login after a short delay
        setTimeout(() => {
          navigate('/login', { replace: true })
        }, 2000)
      } else {
        setError(result.message || 'Failed to delete account')
      }
    } catch (err) {
      setError(err.message || 'An error occurred while deleting your account')
    } finally {
      setIsLoading(false)
    }
  }

  const handleCancel = () => {
    navigate('/profile')
  }

  if (success) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background px-4">
        <div className="w-full max-w-md">
          <div className="bg-surface rounded-2xl shadow-lg p-8 text-center">
            <div className="w-12 h-12 bg-success/10 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg className="w-6 h-6 text-success" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
              </svg>
            </div>
            <h2 className="text-headline-md font-bold text-on-background mb-2">Account Deleted</h2>
            <p className="text-body-md text-on-background/70 mb-6">
              Your account has been successfully deleted. You will be redirected to the login page shortly.
            </p>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-background px-4 py-8">
      <div className="w-full max-w-md">
        <div className="bg-surface rounded-2xl shadow-lg p-8">
          <h1 className="text-headline-md font-bold text-on-background mb-2 text-center">Delete Account</h1>
          <p className="text-body-md text-on-background/70 text-center mb-8">
            This action cannot be undone. Please enter your password to confirm deletion.
          </p>

          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Email Display (Read-only) */}
            <div>
              <label className="block text-label-md text-on-surface font-medium mb-2">Email</label>
              <div className="px-4 py-3 bg-surface-variant/30 rounded-lg border border-outline/20">
                <p className="text-body-md text-on-background font-medium">{user?.email || 'N/A'}</p>
              </div>
            </div>

            {/* Password Input */}
            <div>
              <label className="block text-label-md text-on-surface font-medium mb-2">Password</label>
              <div className="relative">
                <input
                  type={showPassword ? 'text' : 'password'}
                  name="password"
                  value={password}
                  onChange={handlePasswordChange}
                  placeholder="Enter your password"
                  className="w-full px-4 py-3 bg-surface border border-outline rounded-lg text-body-md text-on-background placeholder-on-surface/40 focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-colors"
                  disabled={isLoading}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-4 top-1/2 transform -translate-y-1/2 text-on-surface/60 hover:text-on-surface transition-colors"
                  tabIndex="-1"
                >
                  {showPassword ? (
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-4.803m5.596-3.856a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0z" />
                    </svg>
                  ) : (
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                    </svg>
                  )}
                </button>
              </div>
            </div>

            {/* Error Message */}
            {error && (
              <div className="p-4 bg-error/10 border border-error/30 rounded-lg">
                <p className="text-body-sm text-error font-medium">{error}</p>
              </div>
            )}

            {/* Warning Message */}
            <div className="p-4 bg-warning/10 border border-warning/30 rounded-lg">
              <p className="text-body-sm text-warning font-medium">
                ⚠️ Warning: Deleting your account is permanent. All your data, orders, and settings will be removed.
              </p>
            </div>

            {/* Buttons */}
            <div className="flex gap-3 pt-4">
              <button
                type="button"
                onClick={handleCancel}
                className="flex-1 px-4 py-3 bg-surface border border-outline rounded-lg text-body-md font-semibold text-on-background hover:bg-surface-variant transition-colors disabled:opacity-50"
                disabled={isLoading}
              >
                Cancel
              </button>
              <button
                type="submit"
                className="flex-1 px-4 py-3 bg-error hover:bg-error/90 rounded-lg text-body-md font-semibold text-white transition-colors disabled:opacity-50 flex items-center justify-center gap-2"
                disabled={isLoading || !password.trim()}
              >
                {isLoading ? (
                  <>
                    <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    Deleting...
                  </>
                ) : (
                  'Delete Account'
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
