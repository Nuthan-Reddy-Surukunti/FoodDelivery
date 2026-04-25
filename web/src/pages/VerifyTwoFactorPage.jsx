import { useState } from 'react'
import { useNavigate, useLocation, Link } from 'react-router-dom'
import { Icon } from '../components/atoms/Icon'
import { useFormValidation } from '../hooks/useFormValidation'
import { useAuth } from '../context/AuthContext'
import { authApi } from '../services/authApi'

export const VerifyTwoFactorPage = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const { login } = useAuth()

  // Get temp credentials from login redirect
  const tempToken = location.state?.tempToken
  const userId = location.state?.userId
  const email = location.state?.email

  const [isLoading, setIsLoading] = useState(false)
  const [submitError, setSubmitError] = useState(null)

  // Redirect if no temp token
  if (!tempToken || !userId) {
    return (
      <div className="bg-background min-h-screen flex items-center justify-center">
        <div className="text-center space-y-stack-md">
          <h1 className="font-display-xl text-display-xl text-on-background">Invalid Session</h1>
          <p className="font-body-md text-body-md text-on-surface-variant">
            Your session has expired. Please login again.
          </p>
          <Link to="/login" className="inline-block bg-primary text-on-primary px-6 py-3 rounded-[16px] hover:bg-surface-tint transition-colors">
            Back to Login
          </Link>
        </div>
      </div>
    )
  }

  const form = useFormValidation(
    { otp: '' },
    async (values) => {
      setSubmitError(null)
      setIsLoading(true)
      try {
        await authApi.verifyTwoFactor(tempToken, values.otp)
        // Complete login after 2FA verification
        navigate('/')
      } catch (error) {
        setSubmitError(error.message || 'Invalid OTP. Please try again.')
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
            CraveCloud
          </h1>
        </div>

        <div className="absolute bottom-16 left-8 right-8">
          <h2 className="font-display-xl text-display-xl text-on-primary drop-shadow-md mb-stack-sm">
            Two-Factor Authentication
          </h2>
          <p className="font-body-lg text-body-lg text-on-primary drop-shadow-sm max-w-md">
            Enter the code from your authenticator app to complete login.
          </p>
        </div>
      </div>

      {/* Right Hemisphere: 2FA Form */}
      <div className="w-full md:w-1/2 lg:w-[45%] h-screen overflow-y-auto flex items-center justify-center p-container-padding bg-surface">
        <div className="w-full max-w-md space-y-stack-lg">
          {/* Mobile Brand Header */}
          <div className="md:hidden text-center mb-8">
            <h1 className="font-headline-md text-headline-md text-primary">CraveCloud</h1>
          </div>

          {/* Header */}
          <div className="space-y-stack-sm text-center md:text-left">
            <h2 className="font-display-xl text-display-xl text-on-background">Two-Factor Code</h2>
            <p className="font-body-md text-body-md text-on-surface-variant">
              Enter the 6-digit code from your authenticator app
            </p>
          </div>

          {/* Info Card */}
          <div className="bg-primary-fixed/50 border border-primary/20 rounded-[16px] p-4 flex items-start gap-3">
            <Icon name="info" size={20} className="text-primary mt-1 flex-shrink-0" />
            <p className="font-body-md text-body-md text-on-background">
              Check your authenticator app (Google Authenticator, Microsoft Authenticator, etc.)
            </p>
          </div>

          {/* Error Alert */}
          {submitError && (
            <div className="bg-error-container text-on-error-container p-4 rounded-[16px] flex items-center space-x-2">
              <Icon name="error" size={20} />
              <span className="font-body-md text-body-md">{submitError}</span>
            </div>
          )}

          {/* Form */}
          <form onSubmit={form.handleSubmit} className="space-y-stack-md">
            {/* OTP Input */}
            <div className="space-y-unit">
              <label className="font-label-md text-label-md text-on-surface ml-4 block" htmlFor="otp">
                Authentication Code
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-5 flex items-center pointer-events-none">
                  <Icon name="security" size={20} className="text-outline" />
                </div>
                <input
                  type="text"
                  id="otp"
                  name="otp"
                  placeholder="000000"
                  maxLength="6"
                  className="w-full rounded-[16px] bg-surface-container-low border border-transparent focus:border-primary focus:bg-surface-container-lowest focus:ring-2 focus:ring-primary/20 py-4 pl-12 pr-6 font-body-md text-body-md text-on-surface placeholder:text-outline transition-all shadow-ambient text-center tracking-widest text-2xl"
                  value={form.values.otp}
                  onChange={(e) => form.handleChange({ ...e, target: { ...e.target, value: e.target.value.replace(/\D/g, '') } })}
                  onBlur={form.handleBlur}
                  required
                  autoFocus
                />
              </div>
            </div>

            {/* Submit Button */}
            <div className="pt-stack-md">
              <button
                type="submit"
                disabled={isLoading || form.values.otp.length !== 6}
                className="w-full rounded-[16px] bg-primary text-on-primary py-4 font-title-lg text-title-lg shadow-ambient hover:bg-surface-tint active:scale-95 transition-all duration-200 flex items-center justify-center space-x-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <span>{isLoading ? 'Verifying...' : 'Verify & Login'}</span>
                {!isLoading && <Icon name="arrow_forward" size={20} />}
              </button>
            </div>
          </form>

          {/* Support Link */}
          <div className="text-center pt-stack-sm">
            <p className="font-body-md text-body-md text-on-surface-variant">
              Don't have an authenticator app?{' '}
              <a href="#" className="font-title-lg text-title-lg text-primary hover:text-surface-tint transition-colors">
                Learn more
              </a>
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
