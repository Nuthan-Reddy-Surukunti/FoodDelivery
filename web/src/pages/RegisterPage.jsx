import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { GoogleLogin } from '@react-oauth/google'
import { Icon } from '../components/atoms/Icon'
import { useAuth } from '../context/AuthContext'
import { useFormValidation } from '../hooks/useFormValidation'
import { getRoleHomePath } from '../utils/authRoutes'

/* Shared auth hero panel — reused across auth pages */
export const AuthHeroPanel = ({ title, subtitle, badge }) => (
  <div className="hidden lg:flex lg:w-[52%] relative h-screen overflow-hidden flex-col">
    <img src="/food_hero.png" alt="Delicious food" className="absolute inset-0 w-full h-full object-cover scale-105" />
    <div className="absolute inset-0 bg-gradient-to-t from-slate-950/92 via-slate-900/45 to-slate-800/20" />
    <div className="absolute inset-0 bg-gradient-to-r from-transparent to-slate-900/25" />

    {/* Floating food emojis */}
    <div className="absolute top-[22%] left-[12%] text-5xl animate-float opacity-55 pointer-events-none select-none">🌮</div>
    <div className="absolute top-[38%] right-[12%] text-4xl animate-float-delayed opacity-45 pointer-events-none select-none">🍱</div>
    <div className="absolute bottom-[30%] left-[18%] text-3xl animate-float-slow opacity-40 pointer-events-none select-none">🥘</div>

    {/* Brand */}
    <div className="absolute top-8 left-8 z-10">
      <div className="flex items-center gap-2.5 bg-white/10 backdrop-blur-md border border-white/20 px-4 py-2.5 rounded-2xl">
        <img src="/quickbite-logo-glow.png" alt="QuickBite Logo" className="w-8 h-8 object-contain animate-logo-shimmer" />
        <span className="text-lg font-extrabold text-white tracking-tight">QuickBite</span>
      </div>
    </div>

    {/* Bottom content */}
    <div className="absolute bottom-0 left-0 right-0 p-10 z-10">
      {badge && (
        <div className="inline-flex items-center gap-2 bg-white/10 backdrop-blur-sm px-3 py-1.5 rounded-full border border-white/20 mb-5">
          <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />
          <span className="text-xs font-semibold text-white/90">{badge}</span>
        </div>
      )}
      <h2 className="text-4xl font-extrabold text-white leading-tight mb-3 drop-shadow-lg">{title}</h2>
      <p className="text-white/70 text-base max-w-sm leading-relaxed">{subtitle}</p>
    </div>
  </div>
)

export const RegisterPage = () => {
  const navigate = useNavigate()
  const { register, googleLogin, isLoading: authLoading, error: authError } = useAuth()
  const [submitError, setSubmitError] = useState(null)
  const [showPassword, setShowPassword] = useState(false)
  const [isSuccess, setIsSuccess] = useState(false)
  const [successMessage, setSuccessMessage] = useState(null)

  const handleGoogleSuccess = async (credentialResponse) => {
    setSubmitError(null)
    try {
      const result = await googleLogin(credentialResponse.credential)
      if (result.status === 'SUCCESS') {
        navigate(getRoleHomePath(result.user?.role), { replace: true })
      } else if (result.status === 'VERIFY_2FA') {
        navigate('/verify-2fa', { state: { tempToken: result.tempToken, userId: result.userId, role: result.role } })
      }
    } catch (error) {
      setSubmitError(error.message || 'Google Login failed. Please try again.')
    }
  }

  const form = useFormValidation(
    { fullName: '', email: '', mobileNumber: '', password: '', role: 'Customer', terms: false },
    async (values) => {
      setSubmitError(null)
      setIsSuccess(false)
      try {
        const result = await register(values.fullName, values.email, values.mobileNumber, values.password, values.role)
        if (result.isPendingApproval) {
          setSuccessMessage(result.message)
          setIsSuccess(true)
          setTimeout(() => navigate('/login'), 4000)
        } else if (result.requiresEmailVerification) {
          navigate('/verify-email', { state: { email: values.email, isAfterRegistration: true, role: values.role } })
        }
      } catch (error) {
        setSubmitError(error.message || 'Registration failed. Please try again.')
      }
    }
  )

  const ROLE_OPTIONS = [
    { value: 'Customer', label: 'Customer', desc: 'Order food', icon: '🛒' },
    { value: 'RestaurantPartner', label: 'Restaurant Partner', desc: 'Manage menu', icon: '🍽️' },
    { value: 'DeliveryAgent', label: 'Delivery Agent', desc: 'Make deliveries', icon: '🛵' },
  ]

  return (
    <div className="min-h-screen flex font-sans antialiased overflow-hidden">
      {/* Hero Panel */}
      <AuthHeroPanel
        title={<>Join <span className="text-orange-400">50K+</span> food lovers.</>}
        subtitle="Create an account to discover top restaurants and get your favorite meals delivered in minutes."
        badge="Fast delivery · Track live · 24/7 support"
      />

      {/* Form Panel */}
      <div className="w-full lg:w-[48%] h-screen overflow-y-auto flex items-start justify-center bg-slate-50 relative overflow-hidden">
        {/* Animated orbs */}
        <div className="absolute top-0 right-0 w-64 h-64 bg-blue-100 rounded-full blur-3xl opacity-50 -translate-y-1/2 translate-x-1/2 animate-blob pointer-events-none" />
        <div className="absolute bottom-0 left-0 w-56 h-56 bg-purple-100 rounded-full blur-3xl opacity-40 translate-y-1/2 -translate-x-1/2 animate-blob-delay2 pointer-events-none" />

        <div className="w-full max-w-lg px-8 py-10 relative z-10">
          {/* Mobile Brand */}
          <div className="lg:hidden text-center mb-6">
            <div className="inline-flex items-center gap-2">
              <img src="/quickbite-logo-glow.png" alt="QuickBite Logo" className="w-10 h-10 object-contain animate-logo-shimmer" />
              <span className="text-2xl font-extrabold text-primary">QuickBite</span>
            </div>
          </div>

          {/* Header */}
          <div className="mb-6">
            <h1 className="text-3xl font-extrabold text-slate-900 mb-1.5">Create your account 🎉</h1>
            <p className="text-slate-500 text-sm">Join us and start ordering in minutes.</p>
          </div>

          {/* Alerts */}
          {(submitError || authError) && (
            <div className="bg-rose-50 border border-rose-200 text-rose-700 p-4 rounded-2xl flex items-center gap-3 mb-5 animate-scale-in">
              <div className="w-8 h-8 bg-rose-100 rounded-full flex items-center justify-center flex-shrink-0">
                <Icon name="error" size={16} />
              </div>
              <span className="text-sm font-medium">{submitError || authError}</span>
            </div>
          )}
          {isSuccess && (
            <div className="bg-emerald-50 border border-emerald-200 text-emerald-700 p-4 rounded-2xl flex items-center gap-3 mb-5 animate-bounce-in">
              <div className="w-8 h-8 bg-emerald-100 rounded-full flex items-center justify-center flex-shrink-0">
                <Icon name="check_circle" size={16} />
              </div>
              <span className="text-sm font-medium">{successMessage}</span>
            </div>
          )}

          {/* Form Card */}
          <div className="bg-white rounded-3xl shadow-xl shadow-slate-200/80 border border-slate-100 p-7">
            <form onSubmit={form.handleSubmit} className="space-y-4">
              {/* Full Name */}
              <div className="space-y-1.5">
                <label className="text-sm font-semibold text-slate-700 block" htmlFor="reg-fullName">Full Name</label>
                <div className="relative">
                  <Icon name="person" size={17} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                  <input type="text" id="reg-fullName" name="fullName" placeholder="Jane Doe"
                    className="w-full input-premium py-3 pl-11 pr-4 text-sm rounded-xl placeholder:text-slate-400"
                    value={form.values.fullName} onChange={form.handleChange} onBlur={form.handleBlur} required />
                </div>
                {form.touched.fullName && form.errors.fullName && (
                  <p className="text-rose-500 text-xs mt-1">{form.errors.fullName}</p>
                )}
              </div>

              {/* Email */}
              <div className="space-y-1.5">
                <label className="text-sm font-semibold text-slate-700 block" htmlFor="reg-email">Email Address</label>
                <div className="relative">
                  <Icon name="mail" size={17} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                  <input type="email" id="reg-email" name="email" placeholder="jane@example.com"
                    className="w-full input-premium py-3 pl-11 pr-4 text-sm rounded-xl placeholder:text-slate-400"
                    value={form.values.email} onChange={form.handleChange} onBlur={form.handleBlur} required />
                </div>
                {form.touched.email && form.errors.email && (
                  <p className="text-rose-500 text-xs mt-1">{form.errors.email}</p>
                )}
              </div>

              {/* Phone */}
              <div className="space-y-1.5">
                <label className="text-sm font-semibold text-slate-700 block" htmlFor="reg-mobile">Phone Number</label>
                <div className="relative">
                  <Icon name="call" size={17} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                  <input type="tel" id="reg-mobile" name="mobileNumber" placeholder="+91 00000 00000"
                    className="w-full input-premium py-3 pl-11 pr-4 text-sm rounded-xl placeholder:text-slate-400"
                    value={form.values.mobileNumber} onChange={form.handleChange} onBlur={form.handleBlur} required />
                </div>
              </div>

              {/* Account Type — pill buttons */}
              <div className="space-y-2">
                <label className="text-sm font-semibold text-slate-700 block">Account Type</label>
                <div className="grid grid-cols-3 gap-2">
                  {ROLE_OPTIONS.map(({ value, label, desc, icon }) => (
                    <button
                      key={value}
                      type="button"
                      onClick={() => form.handleChange({ target: { name: 'role', value } })}
                      className={`flex flex-col items-center gap-1 p-3 rounded-xl border-2 text-center transition-all ${
                        form.values.role === value
                          ? 'border-primary bg-blue-50 text-primary'
                          : 'border-slate-200 bg-white text-slate-600 hover:border-slate-300'
                      }`}
                    >
                      <span className="text-xl">{icon}</span>
                      <span className="text-[11px] font-bold leading-tight">{label}</span>
                      <span className="text-[10px] text-slate-400 leading-tight hidden sm:block">{desc}</span>
                    </button>
                  ))}
                </div>
              </div>

              {/* Password */}
              <div className="space-y-1.5">
                <label className="text-sm font-semibold text-slate-700 block" htmlFor="reg-password">Password</label>
                <div className="relative">
                  <Icon name="lock" size={17} className="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                  <input
                    type={showPassword ? 'text' : 'password'}
                    id="reg-password" name="password" placeholder="At least 8 characters"
                    className="w-full input-premium py-3 pl-11 pr-11 text-sm rounded-xl placeholder:text-slate-400"
                    value={form.values.password} onChange={form.handleChange} onBlur={form.handleBlur} required
                  />
                  <button type="button" onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 hover:text-primary transition-colors">
                    <Icon name={showPassword ? 'visibility_off' : 'visibility'} size={17} />
                  </button>
                </div>
                {form.touched.password && form.errors.password && (
                  <p className="text-rose-500 text-xs mt-1">{form.errors.password}</p>
                )}
              </div>

              {/* Terms */}
              <div className="flex items-start gap-3 pt-1">
                <input type="checkbox" id="reg-terms" name="terms"
                  checked={form.values.terms} onChange={form.handleChange}
                  className="mt-0.5 h-4 w-4 rounded border-slate-300 text-primary focus:ring-primary cursor-pointer" required />
                <label htmlFor="reg-terms" className="text-xs text-slate-500 cursor-pointer leading-relaxed">
                  I agree to the{' '}
                  <a href="#" className="text-primary hover:underline font-semibold">Terms of Service</a>{' '}
                  and{' '}
                  <a href="#" className="text-primary hover:underline font-semibold">Privacy Policy</a>.
                </label>
              </div>

              {/* Submit */}
              <button
                id="register-submit-btn"
                type="submit"
                disabled={authLoading}
                className="w-full btn-primary-gradient text-white py-4 rounded-xl text-sm font-bold flex items-center justify-center gap-2 mt-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {authLoading ? (
                  <>
                    <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    Creating Account...
                  </>
                ) : (
                  <>Create Account <Icon name="arrow_forward" size={16} /></>
                )}
              </button>
            </form>

            {/* OR Divider */}
            <div className="flex items-center gap-3 my-6">
              <div className="flex-1 h-px bg-slate-100" />
              <span className="text-[10px] text-slate-400 font-bold uppercase tracking-widest">Or sign up with</span>
              <div className="flex-1 h-px bg-slate-100" />
            </div>

            {/* Google Login */}
            <div className="flex justify-center mt-2">
              <GoogleLogin
                onSuccess={handleGoogleSuccess}
                onError={() => setSubmitError('Google Login Failed')}
                theme="outline"
                shape="pill"
                size="large"
                width="340"
                text="signup_with"
              />
            </div>
          </div>

          <p className="text-center text-sm text-slate-500 mt-6">
            Already have an account?{' '}
            <Link to="/login" className="text-primary font-bold hover:underline">Sign In</Link>
          </p>
        </div>
      </div>
    </div>
  )
}
