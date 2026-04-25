import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Icon } from '../components/atoms/Icon'
import { useFormValidation } from '../hooks/useFormValidation'
import { authApi } from '../services/authApi'

export const ChangePasswordPage = () => {
  const navigate = useNavigate()
  const [isLoading, setIsLoading] = useState(false)
  const [submitError, setSubmitError] = useState(null)
  const [successMessage, setSuccessMessage] = useState(null)
  const [showCurrentPassword, setShowCurrentPassword] = useState(false)
  const [showNewPassword, setShowNewPassword] = useState(false)
  const [showConfirmPassword, setShowConfirmPassword] = useState(false)

  const form = useFormValidation(
    { currentPassword: '', newPassword: '', confirmPassword: '' },
    async (values) => {
      setSubmitError(null)
      setSuccessMessage(null)

      // Validate password match
      if (values.newPassword !== values.confirmPassword) {
        form.setErrors?.({ confirmPassword: 'Passwords do not match' })
        return
      }

      // Validate password length
      if (values.newPassword.length < 8) {
        form.setErrors?.({ newPassword: 'Password must be at least 8 characters' })
        return
      }

      // Validate new password is different from current
      if (values.currentPassword === values.newPassword) {
        form.setErrors?.({ newPassword: 'New password must be different from current password' })
        return
      }

      setIsLoading(true)
      try {
        await authApi.changePassword(values.currentPassword, values.newPassword, values.confirmPassword)
        setSuccessMessage('Password changed successfully! Redirecting to profile...')
        setTimeout(() => navigate('/profile'), 2000)
      } catch (error) {
        setSubmitError(error.message || 'Failed to change password. Please try again.')
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
              'url("https://images.unsplash.com/photo-1633356122544-f134324ef6e6?w=1000&h=1200&fit=crop")',
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
            Keep your account secure
          </h2>
          <p className="font-body-lg text-body-lg text-on-primary drop-shadow-sm max-w-md">
            Change your password regularly to maintain the security of your QuickBite account.
          </p>
        </div>
      </div>

      {/* Right Hemisphere: Change Password Form */}
      <div className="w-full md:w-1/2 lg:w-[45%] h-screen overflow-y-auto flex items-center justify-center p-container-padding bg-surface">
        <div className="w-full max-w-md space-y-stack-lg">
          {/* Mobile Brand Header */}
          <div className="md:hidden text-center mb-8">
            <h1 className="font-headline-md text-headline-md text-primary">QuickBite</h1>
          </div>

          {/* Header */}
          <div className="space-y-stack-sm text-center md:text-left">
            <h2 className="font-display-xl text-display-xl text-on-background">Change Password</h2>
            <p className="font-body-md text-body-md text-on-surface-variant">
              Update your password to keep your account secure.
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
            {/* Current Password */}
            <div className="space-y-unit">
              <label className="font-label-md text-label-md text-on-surface ml-4 block" htmlFor="currentPassword">
                Current Password
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-5 flex items-center pointer-events-none">
                  <Icon name="lock" size={20} className="text-outline" />
                </div>
                <input
                  type={showCurrentPassword ? 'text' : 'password'}
                  id="currentPassword"
                  name="currentPassword"
                  placeholder="••••••••"
                  className="w-full rounded-[16px] bg-surface-container-low border border-transparent focus:border-primary focus:bg-surface-container-lowest focus:ring-2 focus:ring-primary/20 py-4 pl-12 pr-12 font-body-md text-body-md text-on-surface placeholder:text-outline transition-all shadow-ambient"
                  value={form.values.currentPassword}
                  onChange={form.handleChange}
                  onBlur={form.handleBlur}
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowCurrentPassword(!showCurrentPassword)}
                  className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-primary transition-colors"
                >
                  <Icon name={showCurrentPassword ? 'visibility_off' : 'visibility'} size={20} />
                </button>
              </div>
              {form.touched.currentPassword && form.errors.currentPassword && (
                <p className="font-caption-sm text-caption-sm text-error ml-4">{form.errors.currentPassword}</p>
              )}
            </div>

            {/* New Password */}
            <div className="space-y-unit">
              <label className="font-label-md text-label-md text-on-surface ml-4 block" htmlFor="newPassword">
                New Password
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-5 flex items-center pointer-events-none">
                  <Icon name="lock" size={20} className="text-outline" />
                </div>
                <input
                  type={showNewPassword ? 'text' : 'password'}
                  id="newPassword"
                  name="newPassword"
                  placeholder="••••••••"
                  className="w-full rounded-[16px] bg-surface-container-low border border-transparent focus:border-primary focus:bg-surface-container-lowest focus:ring-2 focus:ring-primary/20 py-4 pl-12 pr-12 font-body-md text-body-md text-on-surface placeholder:text-outline transition-all shadow-ambient"
                  value={form.values.newPassword}
                  onChange={form.handleChange}
                  onBlur={form.handleBlur}
                  required
                />
                <button
                  type="button"
                  onClick={() => setShowNewPassword(!showNewPassword)}
                  className="absolute right-4 top-1/2 -translate-y-1/2 text-outline hover:text-primary transition-colors"
                >
                  <Icon name={showNewPassword ? 'visibility_off' : 'visibility'} size={20} />
                </button>
              </div>
              <p className="font-caption-sm text-caption-sm text-on-surface-variant px-2">
                Must be at least 8 characters.
              </p>
              {form.touched.newPassword && form.errors.newPassword && (
                <p className="font-caption-sm text-caption-sm text-error ml-4">{form.errors.newPassword}</p>
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
                <span>{isLoading ? 'Changing...' : 'Change Password'}</span>
                {!isLoading && <Icon name="arrow_forward" size={20} />}
              </button>
            </div>
          </form>

          {/* Back to Profile */}
          <div className="text-center pt-stack-sm">
            <p className="font-body-md text-body-md text-on-surface-variant">
              <button
                onClick={() => navigate('/profile')}
                className="font-title-lg text-title-lg text-primary hover:text-surface-tint ml-1 transition-colors bg-none border-none cursor-pointer p-0"
              >
                Back to Profile
              </button>
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
