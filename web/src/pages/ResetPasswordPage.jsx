import { useState } from 'react'
import { useNavigate, useSearchParams, Link } from 'react-router-dom'
import { Icon } from '../components/atoms/Icon'
import { useFormValidation } from '../hooks/useFormValidation'
import { authApi } from '../services/authApi'

export const ResetPasswordPage = () => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')
  const email = searchParams.get('email')

  const [isLoading, setIsLoading] = useState(false)
  const [submitError, setSubmitError] = useState(null)
  const [successMessage, setSuccessMessage] = useState(null)
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)

  // Redirect if no token
  if (!token || !email) {
    return (
      <div className="bg-background min-h-screen flex items-center justify-center">
        <div className="text-center space-y-stack-md">
          <h1 className="font-display-xl text-display-xl text-on-background">Invalid Reset Link</h1>
          <p className="font-body-md text-body-md text-on-surface-variant">
            The reset link is expired or invalid. Please request a new one.
          </p>
          <Link to="/forgot-password" className="inline-block bg-primary text-on-primary px-6 py-3 rounded-[16px] hover:bg-surface-tint transition-colors">
            Request New Link
          </Link>
        </div>
      </div>
    )
  }

  const form = useFormValidation(
    { password: '', confirmPassword: '' },
    async (values) => {
      if (values.password !== values.confirmPassword) {
        form.setErrors?.({ confirmPassword: 'Passwords do not match' })
        return
      }

      setSubmitError(null)
      setSuccessMessage(null)
      setIsLoading(true)
      try {
        await authApi.resetPassword(email, token, values.password)
        setSuccessMessage('Password reset successfully! Redirecting to login...')
        setTimeout(() => navigate('/login'), 2000)
      } catch (error) {
        setSubmitError(error.message || 'Failed to reset password. Please try again.')
      } finally {
        setIsLoading(false)
      }
    }
  )

  return (
    <div className="bg-background text-on-background min-h-screen flex flex-col md:flex-row antialiased overflow-hidden">
      {/* Left Hemisphere: Image */}
      <div className="hidden md:flex md:w-1/2 lg:w-[55%] relative h-screen bg-surface-container-highest">
        <div
          className="absolute inset-0 bg-cover bg-center"
          style={{
            backgroundImage:
              "url('https://lh3.googleusercontent.com/aida-public/AB6AXuDdjmT2L_2KIUrrbZxlduKbIzQ7MpYbPNULULA35xiJYwnM-H-2XWOHJlUfOeiBltg8pj8ZGY-rfQ8FIBVizFa5NF2uMc5fQ6k4dbGHslYwb25PY_ZZ-byNDB0N0JeCWyd_ZRrwK6DQ6vd5g0IFwyJ1enFCkZVU2hGTUaW7ft_PLYTLm-uPw6E2o0LU6ITwgRGJ3u4KH0BUOPgZsI2tZ9AZHKpIyQT88pbMxA_tOrFwy2ydHvvkmoMe2_b_QTfYdUpZEUCGyGh3CbZE')",
          }}
        />
        <div className="absolute inset-0 bg-gradient-to-t from-primary/90 via-surface-variant/20 to-transparent" />

        <div className="absolute top-8 left-8">
          <h1 className="font-headline-md text-headline-md text-primary bg-surface-container-lowest/90 px-4 py-2 rounded-[16px] shadow-ambient backdrop-blur-md">
            QuickBite
          </h1>
        </div>

        <div className="absolute bottom-16 left-8 right-8">
          <h2 className="font-display-xl text-display-xl text-on-primary drop-shadow-md mb-stack-sm">
            Create new password
          </h2>
          <p className="font-body-lg text-body-lg text-on-primary drop-shadow-sm max-w-md">
            Enter a strong password to secure your QuickBite account.
          </p>
        </div>
      </div>

      {/* Right Hemisphere: Reset Password Form */}
      <div className="w-full md:w-1/2 lg:w-[45%] h-screen overflow-y-auto flex items-center justify-center p-container-padding bg-surface">
        <div className="w-full max-w-md space-y-stack-lg">
          {/* Mobile Brand Header */}
          <div className="md:hidden text-center mb-8">
            <h1 className="font-headline-md text-headline-md text-primary">QuickBite</h1>
          </div>

          {/* Header */}
          <div className="space-y-stack-sm text-center md:text-left">
            <h2 className="font-display-xl text-display-xl text-on-background">Reset Password</h2>
            <p className="font-body-md text-body-md text-on-surface-variant">
              Enter a new password for your account.
            </p>
          </div>

          {/* Success Message */}
          {successMessage && (
            <div className="bg-tertiary-fixed text-on-tertiary-fixed p-4 rounded-[16px] flex items-center gap-2">
              <Icon name="check_circle" size={20} />
              <span className="font-body-md">{successMessage}</span>
            </div>
          )}

          {/* Error Alert */}
          {submitError && (
            <div className="bg-error-container text-on-error-container p-4 rounded-[16px] flex items-center space-x-2">
              <Icon name="error" size={20} />
              <span className="font-body-md text-body-md">{submitError}</span>
            </div>
          )}

          {/* Form */}
          <form onSubmit={form.handleSubmit} className="space-y-stack-md">
            {/* New Password */}
            <div className="space-y-unit">
              <label className="font-label-md text-label-md text-on-surface ml-4 block" htmlFor="password">
                New Password
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-5 flex items-center pointer-events-none">
                  <Icon name="lock" size={20} className="text-outline" />
                </div>
                <input
                  type={showPassword ? 'text' : 'password'}
                  id="password"
                  name="password"
                  placeholder="••••••••"
                  className="w-full rounded-[16px] bg-surface-container-low border border-transparent focus:border-primary focus:bg-surface-container-lowest focus:ring-2 focus:ring-primary/20 py-4 pl-12 pr-12 font-body-md text-body-md text-on-surface placeholder:text-outline transition-all shadow-ambient"
                  value={form.values.password}
                  onChange={form.handleChange}
                  onBlur={form.handleBlur}
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-primary transition-colors"
                >
                  <Icon name={showPassword ? 'visibility_off' : 'visibility'} size={20} />
                </button>
              </div>
              <p className="font-caption-sm text-caption-sm text-on-surface-variant px-2">
                Must be at least 8 characters.
              </p>
              {form.touched.password && form.errors.password && (
                <p className="font-caption-sm text-caption-sm text-error ml-4">{form.errors.password}</p>
              )}
            </div>

            {/* Confirm Password */}
            <div className="space-y-unit">
              <label className="font-label-md text-label-md text-on-surface ml-4 block" htmlFor="confirmPassword">
                Confirm Password
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-5 flex items-center pointer-events-none">
                  <Icon name="lock" size={20} className="text-outline" />
                </div>
                <input
                  type={showConfirmPassword ? 'text' : 'password'}
                  id="confirmPassword"
                  name="confirmPassword"
                  placeholder="••••••••"
                  className="w-full rounded-[16px] bg-surface-container-low border border-transparent focus:border-primary focus:bg-surface-container-lowest focus:ring-2 focus:ring-primary/20 py-4 pl-12 pr-12 font-body-md text-body-md text-on-surface placeholder:text-outline transition-all shadow-ambient"
                  value={form.values.confirmPassword}
                  onChange={form.handleChange}
                  onBlur={form.handleBlur}
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                  className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-primary transition-colors"
                >
                  <Icon name={showConfirmPassword ? 'visibility_off' : 'visibility'} size={20} />
                </button>
              </div>
              {form.touched.confirmPassword && form.errors.confirmPassword && (
                <p className="font-caption-sm text-caption-sm text-error ml-4">{form.errors.confirmPassword}</p>
              )}
            </div>

            {/* Submit Button */}
            <div className="pt-stack-md">
              <button
                type="submit"
                disabled={isLoading}
                className="w-full rounded-[16px] bg-primary text-on-primary py-4 font-title-lg text-title-lg shadow-ambient hover:bg-surface-tint active:scale-95 transition-all duration-200 flex items-center justify-center space-x-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <span>{isLoading ? 'Resetting...' : 'Reset Password'}</span>
                {!isLoading && <Icon name="arrow_forward" size={20} />}
              </button>
            </div>
          </form>

          {/* Back to Login */}
          <div className="text-center pt-stack-sm">
            <Link to="/login" className="font-body-md text-body-md text-primary hover:text-surface-tint transition-colors">
              Back to Login
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}
