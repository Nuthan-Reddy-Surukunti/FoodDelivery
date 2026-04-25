import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { Icon } from '../components/atoms/Icon'
import { useAuth } from '../context/AuthContext'
import { useFormValidation } from '../hooks/useFormValidation'

export const RegisterPage = () => {
  const navigate = useNavigate()
  const { register, isLoading: authLoading, error: authError } = useAuth()
  const [submitError, setSubmitError] = useState(null)
  const [showPassword, setShowPassword] = useState(false)
  const [isSuccess, setIsSuccess] = useState(false)
  const [successMessage, setSuccessMessage] = useState(null)

  const form = useFormValidation(
    { fullName: '', email: '', mobileNumber: '', password: '', role: 'Customer', terms: false },
    async (values) => {
      setSubmitError(null)
      setIsSuccess(false)
      try {
        const result = await register(values.fullName, values.email, values.mobileNumber, values.password, values.role)
        
        // Check if account is pending approval (RestaurantPartner/Admin)
        if (result.isPendingApproval) {
          setSuccessMessage(result.message)
          setIsSuccess(true)
          setTimeout(() => navigate('/login'), 4000)
        } else if (result.requiresEmailVerification) {
          // Customer/DeliveryAgent needs email verification
          navigate('/verify-email', {
            state: {
              email: values.email,
              isAfterRegistration: true,
              role: values.role
            }
          })
        }
      } catch (error) {
        setSubmitError(error.message || 'Registration failed. Please try again.')
      }
    }
  )

  return (
    <div className="bg-background min-h-screen flex font-body-md text-on-background selection:bg-primary-fixed selection:text-on-surface overflow-x-hidden">
      {/* Left Side: Form */}
      <div className="w-full lg:w-1/2 flex flex-col justify-center px-container-padding py-12 md:px-16 lg:px-24 xl:px-32 relative z-10 bg-surface">
        {/* Subtle background glow */}
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[120%] h-[120%] bg-primary-fixed opacity-40 blur-[100px] -z-10 rounded-full pointer-events-none" />

        <div className="max-w-md w-full mx-auto space-y-stack-lg">
          {/* Header */}
          <div className="space-y-stack-sm text-center lg:text-left">
            <h1 className="font-display-xl text-display-xl text-primary tracking-tight">QuickBite</h1>
            <h2 className="font-headline-md text-headline-md text-on-surface">Join the Feast</h2>
            <p className="font-body-lg text-body-lg text-on-surface-variant">
              Create an account to get your favorite meals delivered fast.
            </p>
          </div>

          {/* Error Alert */}
          {(submitError || authError) && (
            <div className="bg-error-container text-on-error-container p-4 rounded-[16px] flex items-center gap-2">
              <Icon name="error" size={20} />
              <span className="font-body-md">{submitError || authError}</span>
            </div>
          )}

          {/* Success Alert - Pending Approval */}
          {isSuccess && (
            <div className="bg-tertiary-fixed text-on-tertiary-fixed p-4 rounded-[16px] flex items-center gap-2">
              <Icon name="check_circle" size={20} />
              <span className="font-body-md">{successMessage}</span>
            </div>
          )}

          {/* Form Card */}
          <form
            onSubmit={form.handleSubmit}
            className="space-y-stack-md bg-surface p-8 rounded-xl shadow-[0_18px_40px_rgba(112,144,176,0.12)] border border-surface-container-highest"
          >
            {/* Full Name */}
            <div className="space-y-stack-sm">
              <label className="block font-label-md text-label-md text-on-surface" htmlFor="fullName">
                Full Name
              </label>
              <div className="relative flex items-center">
                <Icon name="person" size={20} className="absolute left-4 text-on-surface-variant" />
                <input
                  type="text"
                  id="fullName"
                  name="fullName"
                  placeholder="e.g. Jane Doe"
                  className="w-full bg-surface-container-low text-on-surface font-body-md text-body-md rounded-xl py-3 pl-12 pr-4 border border-outline outline-none focus:ring-2 focus:ring-primary focus:border-primary transition-colors placeholder:text-on-surface-variant"
                  value={form.values.fullName}
                  onChange={form.handleChange}
                  onBlur={form.handleBlur}
                  required
                />
              </div>
              {form.touched.fullName && form.errors.fullName && (
                <p className="text-error text-caption-sm font-medium">{form.errors.fullName}</p>
              )}
            </div>

            {/* Email */}
            <div className="space-y-stack-sm">
              <label className="block font-label-md text-label-md text-on-surface" htmlFor="email">
                Email Address
              </label>
              <div className="relative flex items-center">
                <Icon name="mail" size={20} className="absolute left-4 text-on-surface-variant" />
                <input
                  type="email"
                  id="email"
                  name="email"
                  placeholder="jane@example.com"
                  className="w-full bg-surface-container-low text-on-surface font-body-md text-body-md rounded-xl py-3 pl-12 pr-4 border border-outline outline-none focus:ring-2 focus:ring-primary focus:border-primary transition-colors placeholder:text-on-surface-variant"
                  value={form.values.email}
                  onChange={form.handleChange}
                  onBlur={form.handleBlur}
                  required
                />
              </div>
              {form.touched.email && form.errors.email && (
                <p className="text-error text-caption-sm font-medium">{form.errors.email}</p>
              )}
            </div>

            {/* Phone Number */}
            <div className="space-y-stack-sm">
              <label className="block font-label-md text-label-md text-on-surface" htmlFor="mobileNumber">
                Phone Number
              </label>
              <div className="relative flex items-center">
                <Icon name="call" size={20} className="absolute left-4 text-on-surface-variant" />
                <input
                  type="tel"
                  id="mobileNumber"
                  name="mobileNumber"
                  placeholder="(555) 000-0000"
                  className="w-full bg-surface-container-low text-on-surface font-body-md text-body-md rounded-xl py-3 pl-12 pr-4 border border-outline outline-none focus:ring-2 focus:ring-primary focus:border-primary transition-colors placeholder:text-on-surface-variant"
                  value={form.values.mobileNumber}
                  onChange={form.handleChange}
                  onBlur={form.handleBlur}
                  required
                />
              </div>
              {form.touched.mobileNumber && form.errors.mobileNumber && (
                <p className="text-error text-caption-sm font-medium">{form.errors.mobileNumber}</p>
              )}
            </div>
            {/* Role Selection */}
            <div className="space-y-stack-sm">
              <label className="block font-label-md text-label-md text-on-surface" htmlFor="role">
                Account Type
              </label>
              <select
                id="role"
                name="role"
                value={form.values.role}
                onChange={form.handleChange}
                onBlur={form.handleBlur}
                className="w-full bg-surface-container-low text-on-surface font-body-md text-body-md rounded-xl py-3 px-4 border border-outline outline-none focus:ring-2 focus:ring-primary focus:border-primary transition-colors"
              >
                <option value="Customer">Customer - Order Food</option>
                <option value="RestaurantPartner">Restaurant Partner - Manage Menu</option>
                <option value="DeliveryAgent">Delivery Agent - Make Deliveries</option>
              </select>
            </div>
            {/* Password */}
            <div className="space-y-stack-sm">
              <label className="block font-label-md text-label-md text-on-surface" htmlFor="password">
                Password
              </label>
              <div className="relative flex items-center">
                <Icon name="lock" size={20} className="absolute left-4 text-on-surface-variant" />
                <input
                  type={showPassword ? 'text' : 'password'}
                  id="password"
                  name="password"
                  placeholder="••••••••"
                  className="w-full bg-surface-container-low text-on-surface font-body-md text-body-md rounded-xl py-3 pl-12 pr-12 border border-outline outline-none focus:ring-2 focus:ring-primary focus:border-primary transition-colors placeholder:text-on-surface-variant"
                  value={form.values.password}
                  onChange={form.handleChange}
                  onBlur={form.handleBlur}
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-4 text-on-surface-variant hover:text-primary transition-colors focus:outline-none"
                  aria-label="Toggle password visibility"
                >
                  <Icon name={showPassword ? 'visibility_off' : 'visibility'} size={20} />
                </button>
              </div>
              <p className="font-caption-sm text-caption-sm text-on-surface-variant px-2">
                Must be at least 8 characters.
              </p>
              {form.touched.password && form.errors.password && (
                <p className="text-error text-caption-sm font-medium">{form.errors.password}</p>
              )}
            </div>

            {/* Terms Checkbox */}
            <div className="flex items-start pt-2">
              <div className="flex h-5 items-center">
                <input
                  type="checkbox"
                  id="terms"
                  name="terms"
                  checked={form.values.terms}
                  onChange={form.handleChange}
                  className="h-5 w-5 rounded border-outline-variant bg-surface-container-lowest text-primary focus:ring-2 focus:ring-primary focus:ring-offset-2 focus:ring-offset-background cursor-pointer"
                  required
                />
              </div>
              <div className="ml-3 font-body-md text-body-md text-on-surface-variant">
                <label className="cursor-pointer" htmlFor="terms">
                  I agree to the{' '}
                  <a href="#" className="text-primary hover:underline font-medium">
                    Terms of Service
                  </a>{' '}
                  and{' '}
                  <a href="#" className="text-primary hover:underline font-medium">
                    Privacy Policy
                  </a>
                  .
                </label>
              </div>
            </div>

            {/* Submit Button */}
            <div className="pt-4">
              <button
                type="submit"
                disabled={authLoading}
                className="w-full bg-primary text-on-primary font-title-lg text-title-lg rounded-xl py-4 shadow-[0_8px_16px_rgba(25,120,229,0.24)] hover:bg-surface-tint hover:shadow-[0_12px_24px_rgba(25,120,229,0.32)] active:scale-95 transition-all duration-200 flex justify-center items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <span>{authLoading ? 'Creating Account...' : 'Create Account'}</span>
                {!authLoading && <Icon name="arrow_forward" size={20} />}
              </button>
            </div>
          </form>

          {/* Login Link */}
          <div className="text-center">
            <p className="font-body-md text-body-md text-on-surface-variant">
              Already have an account?{' '}
              <Link to="/login" className="text-primary font-bold hover:underline transition-all">
                Sign In
              </Link>
            </p>
          </div>
        </div>
      </div>

      {/* Right Side: Image */}
      <div className="hidden lg:block lg:w-1/2 relative bg-surface-variant overflow-hidden">
        {/* Main Image */}
        <div
          className="absolute inset-0 bg-cover bg-center transition-transform duration-[10s] hover:scale-105"
          style={{
            backgroundImage:
              "url('https://lh3.googleusercontent.com/aida-public/AB6AXuAMvJjZT9ymg1tGBKS2LlE5zFBzPYYqj2vv8h91KH-Sn7dlVLy56ByiCKqxY76YoICwJQg0F_UTK2uwex-wMlFi1A7s5IG2T_3OJay9dorF_z4wJUqKDKhxg_mJLjN5LaWu-E0JCFu5dpYrrnCXrKT220YkcHKXL_6aDHNyYcvoIvh5aFIy17K8-Yk-6tEYuCNXCfOE8-_aWO4GZa_hByxb1XZdgienBISwVsoPBF-HnuabIXUWX4W7ZNZwywW72-NFFeD-6ltQRGbL')",
          }}
        />
        {/* Gradient Overlay */}
        <div className="absolute inset-0 bg-gradient-to-tr from-background/40 via-transparent to-transparent" />

        {/* Decorative Card */}
        <div className="absolute bottom-12 left-12 right-12 bg-white/10 backdrop-blur-md border border-white/20 p-6 rounded-xl shadow-[0_18px_40px_rgba(112,144,176,0.12)]">
          <div className="flex items-center gap-4 text-on-primary">
            <div className="bg-primary/20 p-3 rounded-xl backdrop-blur-sm">
              <Icon name="restaurant" size={24} />
            </div>
            <div>
              <h3 className="font-title-lg text-title-lg">Curated Culinary Experiences</h3>
              <p className="font-body-md text-body-md opacity-90">
                Discover the best local flavors delivered to your door.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
