import { useState } from 'react'
import { useNavigate, useSearchParams, Link } from 'react-router-dom'
import { Icon } from '../components/atoms/Icon'
import { useFormValidation } from '../hooks/useFormValidation'
import { authApi } from '../services/authApi'
import { AuthHeroPanel } from './RegisterPage'

export const ResetPasswordPage = () => {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')
  const email = searchParams.get('email')
  const isOtpFlow = !!email && !token

  const [isLoading, setIsLoading] = useState(false)
  const [submitError, setSubmitError] = useState(null)
  const [successMessage, setSuccessMessage] = useState(null)
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)

  if (!token && !email) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center">
        <div className="text-center space-y-4 p-8">
          <div className="w-16 h-16 bg-rose-100 rounded-full flex items-center justify-center mx-auto">
            <Icon name="error" size={28} className="text-rose-500" />
          </div>
          <h1 className="text-2xl font-extrabold text-slate-900">Invalid Reset Link</h1>
          <p className="text-slate-500 text-sm">The reset link is expired or invalid. Please request a new one.</p>
          <Link to="/forgot-password" className="inline-flex items-center gap-2 btn-primary-gradient text-white px-6 py-3 rounded-xl text-sm font-semibold">
            Request New OTP <Icon name="arrow_forward" size={16} />
          </Link>
        </div>
      </div>
    )
  }

  const otpForm = useFormValidation(
    { otp: '', password: '', confirmPassword: '' },
    async (values) => {
      if (values.password !== values.confirmPassword) {
        otpForm.setErrors?.({ confirmPassword: 'Passwords do not match' })
        return
      }
      setSubmitError(null); setSuccessMessage(null); setIsLoading(true)
      try {
        await authApi.resetPasswordWithOtp(email, values.otp, values.password, values.confirmPassword)
        setSuccessMessage('Password reset successfully! Redirecting to login...')
        setTimeout(() => navigate('/login'), 2000)
      } catch (error) {
        setSubmitError(error.message || 'Failed to reset password. Please check your OTP and try again.')
      } finally { setIsLoading(false) }
    }
  )

  const tokenForm = useFormValidation(
    { password: '', confirmPassword: '' },
    async (values) => {
      if (values.password !== values.confirmPassword) {
        tokenForm.setErrors?.({ confirmPassword: 'Passwords do not match' })
        return
      }
      setSubmitError(null); setSuccessMessage(null); setIsLoading(true)
      try {
        await authApi.resetPassword(email, token, values.password)
        setSuccessMessage('Password reset successfully! Redirecting to login...')
        setTimeout(() => navigate('/login'), 2000)
      } catch (error) {
        setSubmitError(error.message || 'Failed to reset password. Please try again.')
      } finally { setIsLoading(false) }
    }
  )

  const form = isOtpFlow ? otpForm : tokenForm

  const inputClass = 'w-full input-premium py-3.5 pl-11 pr-11 text-sm rounded-xl placeholder:text-slate-400'

  return (
    <div className="min-h-screen flex font-sans antialiased overflow-hidden">
      <AuthHeroPanel
        title={<>Create a new <span className="text-yellow-400">password.</span></>}
        subtitle="Enter a strong password to secure your QuickBite account and get back to ordering."
        badge="Secure password reset"
      />

      <div className="w-full lg:w-[48%] h-screen overflow-hidden flex items-center justify-center bg-slate-50 relative">
        <div className="absolute top-0 right-0 w-64 h-64 bg-blue-100 rounded-full blur-3xl opacity-40 -translate-y-1/2 translate-x-1/2 animate-blob pointer-events-none" />

        <div className="w-full max-w-md px-8 py-12 relative z-10">
          <div className="lg:hidden text-center mb-8">
            <div className="inline-flex items-center gap-2">
              <span className="text-3xl">🍔</span>
              <span className="text-2xl font-extrabold text-primary">QuickBite</span>
            </div>
          </div>

          <div className="mb-8">
            <div className="w-16 h-16 bg-gradient-to-br from-primary to-indigo-600 rounded-2xl flex items-center justify-center mb-5 shadow-lg shadow-primary/20 animate-bounce-in">
              <Icon name="lock_reset" size={28} className="text-white" />
            </div>
            <h1 className="text-3xl font-extrabold text-slate-900 mb-1.5">Reset Password 🔑</h1>
            <p className="text-slate-500 text-sm">
              {isOtpFlow ? 'Enter the OTP from your email and set a new password.' : 'Set a new password for your account.'}
            </p>
          </div>

          {/* Alerts */}
          {successMessage && (
            <div className="bg-emerald-50 border border-emerald-200 text-emerald-700 p-4 rounded-2xl flex items-center gap-3 mb-6 animate-bounce-in">
              <div className="w-8 h-8 bg-emerald-100 rounded-full flex items-center justify-center flex-shrink-0">
                <Icon name="check_circle" size={16} />
              </div>
              <span className="text-sm font-medium">{successMessage}</span>
            </div>
          )}
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
              {/* OTP field (OTP flow only) */}
              {isOtpFlow && (
                <div className="space-y-1.5">
                  <label className="text-sm font-semibold text-slate-700 block" htmlFor="reset-otp">One-Time Password</label>
                  <div className="relative">
                    <Icon name="pin" size={17} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                    <input type="text" id="reset-otp" name="otp" placeholder="6-digit OTP" maxLength="6"
                      className="w-full input-premium py-3.5 pl-11 pr-4 text-sm rounded-xl placeholder:text-slate-400"
                      value={form.values.otp || ''} onChange={form.handleChange} onBlur={form.handleBlur} required />
                  </div>
                  <p className="text-xs text-slate-400">Check your email or AuthService console for the OTP.</p>
                </div>
              )}

              {/* New Password */}
              <div className="space-y-1.5">
                <label className="text-sm font-semibold text-slate-700 block" htmlFor="reset-password">New Password</label>
                <div className="relative">
                  <Icon name="lock" size={17} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                  <input type={showPassword ? 'text' : 'password'} id="reset-password" name="password" placeholder="At least 8 characters"
                    className={inputClass} value={form.values.password} onChange={form.handleChange} onBlur={form.handleBlur} required />
                  <button type="button" onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 hover:text-primary transition-colors">
                    <Icon name={showPassword ? 'visibility_off' : 'visibility'} size={17} />
                  </button>
                </div>
                {form.touched.password && form.errors.password && (
                  <p className="text-rose-500 text-xs">{form.errors.password}</p>
                )}
              </div>

              {/* Confirm Password */}
              <div className="space-y-1.5">
                <label className="text-sm font-semibold text-slate-700 block" htmlFor="reset-confirm">Confirm Password</label>
                <div className="relative">
                  <Icon name="lock" size={17} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                  <input type={showConfirmPassword ? 'text' : 'password'} id="reset-confirm" name="confirmPassword" placeholder="Repeat password"
                    className={inputClass} value={form.values.confirmPassword} onChange={form.handleChange} onBlur={form.handleBlur} required />
                  <button type="button" onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                    className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 hover:text-primary transition-colors">
                    <Icon name={showConfirmPassword ? 'visibility_off' : 'visibility'} size={17} />
                  </button>
                </div>
                {form.touched.confirmPassword && form.errors.confirmPassword && (
                  <p className="text-rose-500 text-xs">{form.errors.confirmPassword}</p>
                )}
              </div>

              <button
                id="reset-password-submit-btn"
                type="submit" disabled={isLoading}
                className="w-full btn-primary-gradient text-white py-4 rounded-xl text-sm font-bold flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isLoading ? (
                  <>
                    <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    Resetting...
                  </>
                ) : (
                  <>Reset Password <Icon name="arrow_forward" size={16} /></>
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

export default ResetPasswordPage
