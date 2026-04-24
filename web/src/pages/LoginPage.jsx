import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { Button } from '../components/atoms/Button'
import { Icon } from '../components/atoms/Icon'
import { useAuth } from '../context/AuthContext'
import { useFormValidation } from '../hooks/useFormValidation'

export const LoginPage = () => {
  const navigate = useNavigate()
  const { login, isLoading: authLoading, error: authError } = useAuth()
  const [submitError, setSubmitError] = useState(null)

  const form = useFormValidation(
    { email: '', password: '' },
    async (values) => {
      setSubmitError(null)
      try {
        await login(values.email, values.password)
        navigate('/')
      } catch (error) {
        setSubmitError(error.message || 'Login failed. Please try again.')
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
            Your cravings,
            <br />
            delivered fresh.
          </h2>
          <p className="font-body-lg text-body-lg text-on-primary drop-shadow-sm max-w-md">
            Experience the fastest way to bring your favorite local flavors straight to your door.
          </p>
        </div>
      </div>

      {/* Right Hemisphere: Login Form */}
      <div className="w-full md:w-1/2 lg:w-[45%] h-screen overflow-y-auto flex items-center justify-center p-container-padding bg-surface">
        <div className="w-full max-w-md space-y-stack-lg">
          {/* Mobile Brand Header */}
          <div className="md:hidden text-center mb-8">
            <h1 className="font-headline-md text-headline-md text-primary">CraveCloud</h1>
          </div>

          {/* Header */}
          <div className="space-y-stack-sm text-center md:text-left">
            <h2 className="font-display-xl text-display-xl text-on-background">Welcome back</h2>
            <p className="font-body-md text-body-md text-on-surface-variant">
              Sign in to continue feeding your cravings.
            </p>
          </div>

          {/* Error Alert */}
          {(submitError || authError) && (
            <div className="bg-error-container text-on-error-container p-4 rounded-[16px] flex items-center space-x-2">
              <Icon name="error" size={20} />
              <span className="font-body-md text-body-md">{submitError || authError}</span>
            </div>
          )}

          {/* Form */}
          <form onSubmit={form.handleSubmit} className="space-y-stack-md">
            {/* Email Input */}
            <div className="space-y-unit">
              <label className="font-label-md text-label-md text-on-surface ml-4 block" htmlFor="email">
                Email or Mobile Number
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-5 flex items-center pointer-events-none">
                  <Icon name="person" size={20} className="text-outline" />
                </div>
                <input
                  type="email"
                  id="email"
                  name="email"
                  placeholder="Enter your email or mobile"
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

            {/* Password Input */}
            <div className="space-y-unit pt-stack-sm">
              <div className="flex justify-between items-center ml-4 mr-4 block">
                <label className="font-label-md text-label-md text-on-surface" htmlFor="password">
                  Password
                </label>
                <a
                  href="#"
                  className="font-caption-sm text-caption-sm text-primary hover:text-surface-tint transition-colors"
                >
                  Forgot Password?
                </a>
              </div>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-5 flex items-center pointer-events-none">
                  <Icon name="lock" size={20} className="text-outline" />
                </div>
                <input
                  type="password"
                  id="password"
                  name="password"
                  placeholder="Enter your password"
                  className="w-full rounded-[16px] bg-surface-container-low border border-transparent focus:border-primary focus:bg-surface-container-lowest focus:ring-2 focus:ring-primary/20 py-4 pl-12 pr-6 font-body-md text-body-md text-on-surface placeholder:text-outline transition-all shadow-ambient"
                  value={form.values.password}
                  onChange={form.handleChange}
                  onBlur={form.handleBlur}
                  required
                />
              </div>
              {form.touched.password && form.errors.password && (
                <p className="hidden font-caption-sm text-caption-sm text-error ml-4 mt-1">
                  {form.errors.password}
                </p>
              )}
            </div>

            {/* Submit Button */}
            <div className="pt-stack-md">
              <button
                type="submit"
                disabled={authLoading}
                className="w-full rounded-[16px] bg-primary text-on-primary py-4 font-title-lg text-title-lg shadow-ambient hover:bg-surface-tint active:scale-95 transition-all duration-200 flex items-center justify-center space-x-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <span>{authLoading ? 'Signing in...' : 'Sign In'}</span>
                {!authLoading && <Icon name="arrow_forward" size={20} />}
              </button>
            </div>
          </form>

          {/* Sign Up Link */}
          <div className="text-center pt-stack-sm">
            <p className="font-body-md text-body-md text-on-surface-variant">
              Don't have an account?{' '}
              <Link to="/register" className="font-title-lg text-title-lg text-primary hover:text-surface-tint ml-1 transition-colors">
                Sign Up
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
