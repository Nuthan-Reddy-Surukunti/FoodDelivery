import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { GoogleLogin } from '@react-oauth/google'
import { Icon } from '../components/atoms/Icon'
import { useAuth } from '../context/AuthContext'
import { useFormValidation } from '../hooks/useFormValidation'
import { getRoleHomePath } from '../utils/authRoutes'

export const LoginPage = () => {
  const navigate = useNavigate()
  const { login, googleLogin, isLoading: authLoading, error: authError } = useAuth()
  const [submitError, setSubmitError] = useState(null)
  const [showPassword, setShowPassword] = useState(false)

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
    { email: '', password: '' },
    async (values) => {
      setSubmitError(null)
      try {
        const result = await login(values.email, values.password)
        if (result.status === 'SUCCESS') {
          navigate(getRoleHomePath(result.user?.role), { replace: true })
        } else if (result.status === 'VERIFY_2FA') {
          navigate('/verify-2fa', { state: { tempToken: result.tempToken, userId: result.userId, email: result.email, role: result.role } })
        } else if (result.status === 'VERIFY_EMAIL') {
          navigate('/verify-email', { state: { email: result.email, userId: result.userId, isAfterLogin: true, role: result.role } })
        }
      } catch (error) {
        setSubmitError(error.message || 'Login failed. Please try again.')
      }
    }
  )

  return (
    <div className="min-h-screen flex font-sans antialiased overflow-hidden">
      {/* ── Left Panel: Hero Image ── */}
      <div className="hidden md:flex md:w-1/2 lg:w-[58%] relative h-screen overflow-hidden">
        {/* Background Image */}
        <img
          src="/food_hero.png"
          alt="Delicious food spread"
          className="absolute inset-0 w-full h-full object-cover scale-105"
        />
        {/* Dark overlay gradient */}
        <div className="absolute inset-0 bg-gradient-to-t from-slate-950/90 via-slate-900/40 to-slate-900/20" />
        <div className="absolute inset-0 bg-gradient-to-r from-transparent to-slate-950/30" />

        {/* Floating food emojis */}
        <div className="absolute top-1/4 left-[10%] text-5xl animate-float opacity-60 pointer-events-none select-none">🍕</div>
        <div className="absolute top-1/3 right-[15%] text-4xl animate-float-delayed opacity-50 pointer-events-none select-none">🍜</div>
        <div className="absolute bottom-1/3 left-[20%] text-3xl animate-float-slow opacity-40 pointer-events-none select-none">🍣</div>

        {/* Brand Badge */}
        <div className="absolute top-8 left-8 z-10">
          <div className="flex items-center gap-2.5 bg-white/10 backdrop-blur-md border border-white/20 px-4 py-2.5 rounded-2xl shadow-lg">
            <img src="/quickbite-logo-glow.png" alt="QuickBite Logo" className="w-8 h-8 object-contain animate-logo-shimmer" />
            <span className="text-lg font-extrabold text-white tracking-tight">QuickBite</span>
          </div>
        </div>

        {/* Bottom tagline */}
        <div className="absolute bottom-0 left-0 right-0 p-10 z-10">
          <div className="inline-flex items-center gap-2 bg-white/10 backdrop-blur-sm px-3 py-1.5 rounded-full border border-white/20 mb-5">
            <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />
            <span className="text-xs font-semibold text-white/90">200+ restaurants live near you</span>
          </div>
          <h2 className="text-4xl lg:text-5xl font-extrabold text-white leading-tight mb-3 drop-shadow-lg">
            Your cravings,<br />
            <span className="text-yellow-400">delivered fresh.</span>
          </h2>
          <p className="text-white/75 text-base max-w-md leading-relaxed">
            Experience the fastest way to bring your favorite local flavors straight to your door.
          </p>
          {/* Social proof */}
          <div className="flex items-center gap-4 mt-6">
            <div className="flex -space-x-2">
              {['🧑','👩','🧔','👩‍🦱'].map((e, i) => (
                <div key={i} className="w-8 h-8 rounded-full bg-white/20 border-2 border-white/40 flex items-center justify-center text-sm backdrop-blur-sm">{e}</div>
              ))}
            </div>
            <p className="text-white/70 text-sm"><span className="text-white font-bold">50K+</span> happy customers</p>
          </div>
        </div>
      </div>

      {/* ── Right Panel: Login Form ── */}
      <div className="w-full md:w-1/2 lg:w-[42%] h-screen overflow-hidden flex items-center justify-center bg-white relative">
        {/* Subtle animated background orbs */}
        <div className="absolute top-0 right-0 w-72 h-72 bg-blue-100 rounded-full blur-3xl opacity-60 -translate-y-1/2 translate-x-1/2 animate-blob pointer-events-none" />
        <div className="absolute bottom-0 left-0 w-64 h-64 bg-indigo-100 rounded-full blur-3xl opacity-50 translate-y-1/2 -translate-x-1/2 animate-blob-delay1 pointer-events-none" />

        <div className="w-full max-w-md px-8 py-12 relative z-10">
          {/* Mobile Brand */}
          <div className="md:hidden text-center mb-8">
            <div className="inline-flex items-center gap-2 mb-2">
              <img src="/quickbite-logo-glow.png" alt="QuickBite Logo" className="w-10 h-10 object-contain animate-logo-shimmer" />
              <span className="text-2xl font-extrabold text-primary">QuickBite</span>
            </div>
          </div>

          {/* Header */}
          <div className="mb-8">
            <h1 className="text-3xl font-extrabold text-slate-900 mb-1.5">Welcome back 👋</h1>
            <p className="text-slate-500 text-sm">Sign in to continue feeding your cravings.</p>
          </div>

          {/* Error Alert */}
          {(submitError || authError) && (
            <div className="bg-rose-50 border border-rose-200 text-rose-700 p-4 rounded-2xl flex items-center gap-3 mb-6 animate-scale-in">
              <div className="w-8 h-8 bg-rose-100 rounded-full flex items-center justify-center flex-shrink-0">
                <Icon name="error" size={16} />
              </div>
              <span className="text-sm font-medium">{submitError || authError}</span>
            </div>
          )}

          {/* Form Card */}
          <div className="bg-white rounded-3xl shadow-xl shadow-slate-200/80 border border-slate-100 p-8">
            <form onSubmit={form.handleSubmit} className="space-y-5">
              {/* Email */}
              <div className="space-y-1.5">
                <label className="text-sm font-semibold text-slate-700 block" htmlFor="login-email">
                  Email address
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                    <Icon name="person" size={18} className="text-slate-400" />
                  </div>
                  <input
                    type="email"
                    id="login-email"
                    name="email"
                    placeholder="you@example.com"
                    className="w-full input-premium py-3.5 pl-11 pr-4 text-sm text-slate-900 placeholder:text-slate-400 rounded-xl"
                    value={form.values.email}
                    onChange={form.handleChange}
                    onBlur={form.handleBlur}
                    required
                  />
                </div>
              </div>

              {/* Password */}
              <div className="space-y-1.5">
                <div className="flex justify-between items-center">
                  <label className="text-sm font-semibold text-slate-700" htmlFor="login-password">
                    Password
                  </label>
                  <Link to="/forgot-password" className="text-xs font-semibold text-primary hover:text-primary-container transition-colors">
                    Forgot Password?
                  </Link>
                </div>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                    <Icon name="lock" size={18} className="text-slate-400" />
                  </div>
                  <input
                    type={showPassword ? 'text' : 'password'}
                    id="login-password"
                    name="password"
                    placeholder="••••••••"
                    className="w-full input-premium py-3.5 pl-11 pr-11 text-sm text-slate-900 placeholder:text-slate-400 rounded-xl"
                    value={form.values.password}
                    onChange={form.handleChange}
                    onBlur={form.handleBlur}
                    required
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute inset-y-0 right-0 pr-4 flex items-center text-slate-400 hover:text-primary transition-colors"
                    aria-label="Toggle password visibility"
                  >
                    <Icon name={showPassword ? 'visibility_off' : 'visibility'} size={18} />
                  </button>
                </div>
              </div>

              {/* Submit */}
              <button
                id="login-submit-btn"
                type="submit"
                disabled={authLoading}
                className="w-full btn-primary-gradient text-white py-4 rounded-xl text-sm font-bold flex items-center justify-center gap-2 mt-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {authLoading ? (
                  <>
                    <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                    <span>Signing in...</span>
                  </>
                ) : (
                  <>
                    <span>Sign In</span>
                    <Icon name="arrow_forward" size={16} />
                  </>
                )}
              </button>
            </form>

            {/* OR Divider */}
            <div className="flex items-center gap-3 my-6">
              <div className="flex-1 h-px bg-slate-100" />
              <span className="text-[10px] text-slate-400 font-bold uppercase tracking-widest">Or continue with</span>
              <div className="flex-1 h-px bg-slate-100" />
            </div>

            {/* Google Login */}
            <div className="flex justify-center mb-2">
              <GoogleLogin
                onSuccess={handleGoogleSuccess}
                onError={() => setSubmitError('Google Login Failed')}
                theme="outline"
                shape="pill"
                size="large"
                width="340"
                text="signin_with"
              />
            </div>

            {/* Bottom Link */}
            <div className="mt-8 pt-6 border-t border-slate-50 flex flex-col items-center gap-4">
              <p className="text-xs text-slate-400 font-medium">New here?</p>
              <Link
                to="/register"
                id="go-to-register-link"
                className="w-full flex items-center justify-center gap-2 py-3.5 rounded-xl border-2 border-slate-200 text-slate-700 text-sm font-bold hover:border-primary hover:text-primary transition-all bg-white"
              >
                Create an account
                <Icon name="person_add" size={16} />
              </Link>
            </div>
          </div>

          <p className="text-center text-xs text-slate-400 mt-6">
            By continuing, you agree to our{' '}
            <a href="#" className="text-primary hover:underline">Terms</a> &amp;{' '}
            <a href="#" className="text-primary hover:underline">Privacy Policy</a>.
          </p>
        </div>
      </div>
    </div>
  )
}
