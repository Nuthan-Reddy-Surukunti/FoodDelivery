import { useState } from 'react'
import { useNavigate, useLocation, Link } from 'react-router-dom'
import { Icon } from '../components/atoms/Icon'
import { useFormValidation } from '../hooks/useFormValidation'
import { useAuth } from '../context/AuthContext'
import { authApi } from '../services/authApi'
import { getRoleHomePath } from '../utils/authRoutes'
import { AuthHeroPanel } from './RegisterPage'

export const VerifyTwoFactorPage = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const { setAuthUser } = useAuth()

  const tempToken = location.state?.tempToken
  const userId = location.state?.userId
  const email = location.state?.email

  const [isLoading, setIsLoading] = useState(false)
  const [submitError, setSubmitError] = useState(null)

  if (!tempToken || !userId) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center">
        <div className="text-center space-y-4 p-8">
          <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center mx-auto">
            <Icon name="warning" size={28} className="text-amber-500" />
          </div>
          <h1 className="text-2xl font-extrabold text-slate-900">Invalid Session</h1>
          <p className="text-slate-500 text-sm">Your session has expired. Please login again.</p>
          <Link to="/login" className="inline-flex items-center gap-2 btn-primary-gradient text-white px-6 py-3 rounded-xl text-sm font-semibold">
            Back to Login <Icon name="arrow_forward" size={16} />
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
        const result = await authApi.verifyTwoFactor(tempToken, values.otp)
        if (result.token && result.user) {
          setAuthUser(result.user, result.token)
          navigate(getRoleHomePath(result.user?.role), { replace: true })
        }
      } catch (error) {
        setSubmitError(error.message || 'Invalid OTP. Please try again.')
      } finally {
        setIsLoading(false)
      }
    }
  )

  return (
    <div className="min-h-screen flex font-sans antialiased overflow-hidden">
      <AuthHeroPanel
        title={<>Two-factor <span className="text-emerald-400">security.</span></>}
        subtitle="Enter the 6-digit code from your email or authenticator app to complete login."
        badge="Your account is protected"
      />

      <div className="w-full lg:w-[48%] h-screen overflow-hidden flex items-center justify-center bg-slate-50 relative">
        <div className="absolute top-0 right-0 w-64 h-64 bg-indigo-100 rounded-full blur-3xl opacity-40 -translate-y-1/2 translate-x-1/2 animate-blob pointer-events-none" />

        <div className="w-full max-w-md px-8 py-12 relative z-10">
          <div className="lg:hidden text-center mb-8">
            <div className="inline-flex items-center gap-2">
              <img src="/quickbite-logo-glow.png" alt="QuickBite Logo" className="w-10 h-10 object-contain animate-logo-shimmer" />
              <span className="text-2xl font-extrabold text-primary">QuickBite</span>
            </div>
          </div>

          {/* Icon + Header */}
          <div className="mb-8">
            <div className="w-16 h-16 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-2xl flex items-center justify-center mb-5 shadow-lg shadow-indigo-200 animate-bounce-in">
              <Icon name="shield_lock" size={28} className="text-white" />
            </div>
            <h1 className="text-3xl font-extrabold text-slate-900 mb-1.5">Two-Factor Code 🔐</h1>
            <p className="text-slate-500 text-sm">
              {email ? <>Sent to <strong className="text-slate-700">{email}</strong></> : 'Enter the code from your authenticator app'}
            </p>
          </div>

          {/* Info note */}
          <div className="bg-blue-50 border border-blue-100 rounded-2xl p-4 flex items-start gap-3 mb-6">
            <div className="w-8 h-8 bg-blue-100 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5">
              <Icon name="info" size={15} className="text-blue-600" />
            </div>
            <p className="text-sm text-blue-700">Check your email or authenticator app (Google Authenticator, Microsoft Authenticator, etc.)</p>
          </div>

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
              <div className="space-y-2">
                <label className="text-sm font-semibold text-slate-700 block" htmlFor="tfa-otp">Authentication Code</label>
                <div className="relative">
                  <Icon name="security" size={17} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                  <input
                    type="text"
                    id="tfa-otp"
                    name="otp"
                    placeholder="000000"
                    maxLength="6"
                    autoFocus
                    className="w-full input-premium py-4 pl-11 pr-4 text-2xl text-center tracking-[0.5em] font-bold rounded-xl placeholder:text-slate-300 placeholder:text-lg placeholder:tracking-normal"
                    value={form.values.otp}
                    onChange={(e) => {
                      const filtered = e.target.value.replace(/\D/g, '')
                      form.handleChange({ target: { name: 'otp', value: filtered } })
                    }}
                    onBlur={form.handleBlur}
                    required
                  />
                </div>
                <p className="text-xs text-slate-400 text-center">Enter your 6-digit authentication code</p>
              </div>

              <button
                id="verify-2fa-submit-btn"
                type="submit"
                disabled={isLoading || form.values.otp.length !== 6}
                className="w-full btn-primary-gradient text-white py-4 rounded-xl text-sm font-bold flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isLoading ? (
                  <>
                    <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    Verifying...
                  </>
                ) : (
                  <>Verify &amp; Login <Icon name="arrow_forward" size={16} /></>
                )}
              </button>
            </form>
          </div>

          <div className="text-center mt-6">
            <Link to="/login" className="inline-flex items-center gap-1.5 text-sm text-slate-500 hover:text-primary transition-colors font-medium">
              <Icon name="arrow_back" size={16} />
              Back to Login
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}
