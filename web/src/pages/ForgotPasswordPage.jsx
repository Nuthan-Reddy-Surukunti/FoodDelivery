import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { Icon } from '../components/atoms/Icon'
import { useFormValidation } from '../hooks/useFormValidation'
import { authApi } from '../services/authApi'

export const ForgotPasswordPage = () => {
  const navigate = useNavigate()
  const [isLoading, setIsLoading] = useState(false)
  const [submitError, setSubmitError] = useState(null)
  const [successMessage, setSuccessMessage] = useState(null)

  const form = useFormValidation(
    { email: '' },
    async (values) => {
      setSubmitError(null)
      setSuccessMessage(null)
      setIsLoading(true)
      try {
        const response = await authApi.forgotPassword(values.email)
        setSuccessMessage('Password reset link has been sent to your email. Check your inbox and spam folder.')
        form.resetForm?.()
        setTimeout(() => navigate('/login'), 3000)
      } catch (error) {
        setSubmitError(error.message || 'Failed to send reset link. Please try again.')
      } finally {
        setIsLoading(false)
      }
    }
  )

  return (
    <div className="bg-background text-on-background min-h-screen flex flex-col md:flex-row antialiased overflow-hidden">
      {/* Left Hemisphere: Image */}
      <div className="hidden md:flex md:w-1/2 lg:w-[55%] relative h-screen bg-surface-container-highest">
        {/* Background Image */}
        <div
          className="absolute inset-0 bg-cover bg-center"
          style={{
            backgroundImage:
              "url('https://lh3.googleusercontent.com/aida-public/AB6AXuDdjmT2L_2KIUrrbZxlduKbIzQ7MpYbPNULULA35xiJYwnM-H-2XWOHJlUfOeiBltg8pj8ZGY-rfQ8FIBVizFa5NF2uMc5fQ6k4dbGHslYwb25PY_ZZ-byNDB0N0JeCWyd_ZRrwK6DQ6vd5g0IFwyJ1enFCkZVU2hGTUaW7ft_PLYTLm-uPw6E2o0LU6ITwgRGJ3u4KH0BUOPgZsI2tZ9AZHKpIyQT88pbMxA_tOrFwy2ydHvvkmoMe2_b_QTfYdUpZEUCGyGh3CbZE')",
          }}
        />
        {/* Gradient overlay */}
        <div className="absolute inset-0 bg-gradient-to-t from-primary/90 via-surface-variant/20 to-transparent" />

        {/* Brand Badge */}
        <div className="absolute top-8 left-8">
          <h1 className="font-headline-md text-headline-md text-primary bg-surface-container-lowest/90 px-4 py-2 rounded-[16px] shadow-ambient backdrop-blur-md">
            CraveCloud
          </h1>
        </div>

        {/* Sensorial Text */}
        <div className="absolute bottom-16 left-8 right-8">
          <h2 className="font-display-xl text-display-xl text-on-primary drop-shadow-md mb-stack-sm">
            Reset your access
          </h2>
          <p className="font-body-lg text-body-lg text-on-primary drop-shadow-sm max-w-md">
            We'll send you a link to reset your password and get back to your food ordering.
          </p>
        </div>
      </div>

      {/* Right Hemisphere: Forgot Password Form */}
      <div className="w-full md:w-1/2 lg:w-[45%] h-screen overflow-y-auto flex items-center justify-center p-container-padding bg-surface">
        <div className="w-full max-w-md space-y-stack-lg">
          {/* Mobile Brand Header */}
          <div className="md:hidden text-center mb-8">
            <h1 className="font-headline-md text-headline-md text-primary">CraveCloud</h1>
          </div>

          {/* Header */}
          <div className="space-y-stack-sm text-center md:text-left">
            <h2 className="font-display-xl text-display-xl text-on-background">Forgot Password?</h2>
            <p className="font-body-md text-body-md text-on-surface-variant">
              Enter your email and we'll send you a link to reset your password.
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
            {/* Email Input */}
            <div className="space-y-unit">
              <label className="font-label-md text-label-md text-on-surface ml-4 block" htmlFor="email">
                Email Address
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-5 flex items-center pointer-events-none">
                  <Icon name="mail" size={20} className="text-outline" />
                </div>
                <input
                  type="email"
                  id="email"
                  name="email"
                  placeholder="Enter your email"
                  className="w-full rounded-[16px] bg-surface-container-low border border-transparent focus:border-primary focus:bg-surface-container-lowest focus:ring-2 focus:ring-primary/20 py-4 pl-12 pr-6 font-body-md text-body-md text-on-surface placeholder:text-outline transition-all shadow-ambient"
                  value={form.values.email}
                  onChange={form.handleChange}
                  onBlur={form.handleBlur}
                  required
                />
              </div>
              {form.touched.email && form.errors.email && (
                <p className="hidden font-caption-sm text-caption-sm text-error ml-4 mt-1">
                  {form.errors.email}
                </p>
              )}
            </div>

            {/* Submit Button */}
            <div className="pt-stack-md">
              <button
                type="submit"
                disabled={isLoading}
                className="w-full rounded-[16px] bg-primary text-on-primary py-4 font-title-lg text-title-lg shadow-ambient hover:bg-surface-tint active:scale-95 transition-all duration-200 flex items-center justify-center space-x-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <span>{isLoading ? 'Sending...' : 'Send Reset Link'}</span>
                {!isLoading && <Icon name="arrow_forward" size={20} />}
              </button>
            </div>
          </form>

          {/* Back to Login */}
          <div className="text-center pt-stack-sm">
            <p className="font-body-md text-body-md text-on-surface-variant">
              Remember your password?{' '}
              <Link to="/login" className="font-title-lg text-title-lg text-primary hover:text-surface-tint ml-1 transition-colors">
                Sign In
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
