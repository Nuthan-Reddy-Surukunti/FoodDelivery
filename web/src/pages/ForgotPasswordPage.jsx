import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { Icon } from '../components/atoms/Icon'
import { useFormValidation } from '../hooks/useFormValidation'
import { authApi } from '../services/authApi'
import { AuthHeroPanel } from './RegisterPage'

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
        await authApi.forgotPassword(values.email)
        setSuccessMessage('OTP sent! Check your email (or AuthService console) for your one-time password.')
        form.resetForm?.()
        setTimeout(() => navigate(`/reset-password?email=${encodeURIComponent(values.email)}`), 3500)
      } catch (error) {
        setSubmitError(error.message || 'Failed to send reset link. Please try again.')
      } finally {
        setIsLoading(false)
      }
    }
  )

  return (
    <div className="min-h-screen flex font-sans antialiased overflow-hidden">
      <AuthHeroPanel
        title={<>Reset your <span className="text-orange-400">access.</span></>}
        subtitle="We'll send you a secure OTP to get back to your food ordering journey."
        badge="Secure password reset"
      />

      <div className="w-full lg:w-[48%] h-screen overflow-hidden flex items-center justify-center bg-slate-50 relative">
        <div className="absolute top-0 right-0 w-64 h-64 bg-orange-100 rounded-full blur-3xl opacity-40 -translate-y-1/2 translate-x-1/2 animate-blob pointer-events-none" />
        <div className="absolute bottom-0 left-0 w-56 h-56 bg-blue-100 rounded-full blur-3xl opacity-35 translate-y-1/2 -translate-x-1/2 animate-blob-delay2 pointer-events-none" />

        <div className="w-full max-w-md px-8 py-12 relative z-10">
          {/* Mobile brand */}
          <div className="lg:hidden text-center mb-8">
            <div className="inline-flex items-center gap-2">
              <img src="/quickbite-logo-glow.png" alt="QuickBite Logo" className="w-10 h-10 object-contain animate-logo-shimmer" />
              <span className="text-2xl font-extrabold text-primary">QuickBite</span>
            </div>
          </div>

          {/* Icon + Header */}
          <div className="mb-8">
            <div className="w-16 h-16 bg-gradient-to-br from-primary to-indigo-500 rounded-2xl flex items-center justify-center mb-5 shadow-lg shadow-primary/20">
              <Icon name="lock_reset" size={28} className="text-white" />
            </div>
            <h1 className="text-3xl font-extrabold text-slate-900 mb-1.5">Forgot Password?</h1>
            <p className="text-slate-500 text-sm">Enter your email and we'll send you an OTP to reset your password.</p>
          </div>

          {/* Success */}
          {successMessage && (
            <div className="bg-emerald-50 border border-emerald-200 text-emerald-700 p-4 rounded-2xl flex items-center gap-3 mb-6 animate-bounce-in">
              <div className="w-8 h-8 bg-emerald-100 rounded-full flex items-center justify-center flex-shrink-0 animate-bounce-in">
                <Icon name="check_circle" size={16} />
              </div>
              <span className="text-sm font-medium">{successMessage}</span>
            </div>
          )}

          {/* Error */}
          {submitError && (
            <div className="bg-rose-50 border border-rose-200 text-rose-700 p-4 rounded-2xl flex items-center gap-3 mb-6 animate-scale-in">
              <div className="w-8 h-8 bg-rose-100 rounded-full flex items-center justify-center flex-shrink-0">
                <Icon name="error" size={16} />
              </div>
              <span className="text-sm font-medium">{submitError}</span>
            </div>
          )}

          <div className="bg-white rounded-3xl shadow-xl shadow-slate-200/80 border border-slate-100 p-8">
            <form onSubmit={form.handleSubmit} className="space-y-5">
              <div className="space-y-1.5">
                <label className="text-sm font-semibold text-slate-700 block" htmlFor="forgot-email">Email Address</label>
                <div className="relative">
                  <Icon name="mail" size={17} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                  <input
                    type="email" id="forgot-email" name="email" placeholder="you@example.com"
                    className="w-full input-premium py-3.5 pl-11 pr-4 text-sm rounded-xl placeholder:text-slate-400"
                    value={form.values.email} onChange={form.handleChange} onBlur={form.handleBlur} required
                  />
                </div>
              </div>

              <button
                id="forgot-password-submit-btn"
                type="submit" disabled={isLoading}
                className="w-full btn-primary-gradient text-white py-4 rounded-xl text-sm font-bold flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isLoading ? (
                  <>
                    <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    Sending OTP...
                  </>
                ) : (
                  <>Send Reset OTP <Icon name="send" size={16} /></>
                )}
              </button>
            </form>
          </div>

          <div className="text-center mt-6">
            <Link to="/login" className="inline-flex items-center gap-1.5 text-sm text-slate-500 hover:text-primary transition-colors font-medium">
              <Icon name="arrow_back" size={16} />
              Back to Sign In
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}

export default ForgotPasswordPage
