import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Icon } from '../components/atoms/Icon'
import { useAuth } from '../context/AuthContext'
import { authApi } from '../services/authApi'

export const ProfilePage = () => {
  const navigate = useNavigate()
  const { user, logout } = useAuth()
  const [isEditing, setIsEditing] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [twoFactorEnabled, setTwoFactorEnabled] = useState(user?.isTwoFactorEnabled || false)
  const [isTogglingTwoFactor, setIsTogglingTwoFactor] = useState(false)
  const [editForm, setEditForm] = useState({
    fullName: user?.name || '',
    phone: user?.phone || '',
  })
  const [message, setMessage] = useState(null)

  const handleEditChange = (e) => {
    const { name, value } = e.target
    setEditForm(prev => ({ ...prev, [name]: value }))
  }

  const handleSaveProfile = async () => {
    try {
      setIsSaving(true)
      await authApi.updateProfile(user?.id, editForm.fullName, editForm.phone)
      setMessage({ type: 'success', text: 'Profile updated successfully!' })
      setIsEditing(false)
      setTimeout(() => setMessage(null), 3000)
    } catch (error) {
      setMessage({ type: 'error', text: error.message || 'Failed to update profile' })
    } finally {
      setIsSaving(false)
    }
  }

  const handleToggle2FA = async () => {
    try {
      setIsTogglingTwoFactor(true)
      const newState = !twoFactorEnabled
      await authApi.toggleTwoFactor(user?.id, newState)
      setTwoFactorEnabled(newState)
      setMessage({
        type: 'success',
        text: newState ? '2FA enabled! Next time you login, you will receive an email OTP to verify.' : '2FA disabled successfully'
      })
      setTimeout(() => setMessage(null), 4000)
    } catch (error) {
      setMessage({ type: 'error', text: error.message || 'Failed to update 2FA settings' })
    } finally {
      setIsTogglingTwoFactor(false)
    }
  }

  return (
    <div className="min-h-screen bg-background">
      <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        {/* Page Header */}
        <div className="mb-8">
          <h1 className="text-display-md font-bold text-on-background mb-2">My Profile</h1>
          <p className="text-body-md text-on-surface-variant">Manage your account settings and preferences</p>
        </div>

        {/* Message Alert */}
        {message && (
          <div className={`mb-6 p-4 rounded-[16px] flex items-center gap-3 ${
            message.type === 'success' 
              ? 'bg-tertiary-fixed text-on-tertiary-fixed' 
              : 'bg-error-container text-on-error-container'
          }`}>
            <Icon name={message.type === 'success' ? 'check_circle' : 'error'} size={20} />
            <span className="font-body-md">{message.text}</span>
          </div>
        )}

        {/* Profile Card */}
        <div className="bg-surface rounded-[20px] p-6 shadow-ambient border border-outline mb-6">
          {/* Profile Header */}
          <div className="flex items-center gap-4 mb-8 pb-6 border-b border-outline">
            <div className="w-16 h-16 bg-primary rounded-full flex items-center justify-center text-on-primary text-display-sm font-bold">
              {user?.name?.charAt(0)?.toUpperCase() || 'U'}
            </div>
            <div>
              <h2 className="text-headline-md font-bold text-on-background">{user?.name || 'User'}</h2>
              <p className="text-body-md text-on-surface-variant">{user?.role || 'Customer'}</p>
            </div>
            <button
              onClick={() => setIsEditing(!isEditing)}
              className="ml-auto px-4 py-2 rounded-[12px] bg-primary text-on-primary font-body-md hover:bg-surface-tint transition-colors"
            >
              {isEditing ? 'Cancel' : 'Edit Profile'}
            </button>
          </div>

          {/* Profile Details - View Mode */}
          {!isEditing && (
            <div className="space-y-4 mb-8">
              <div className="flex items-start gap-4">
                <Icon name="person" size={24} className="text-primary mt-1" />
                <div>
                  <p className="text-label-md text-on-surface-variant uppercase">Full Name</p>
                  <p className="text-body-md text-on-background font-medium">{user?.name || 'Not set'}</p>
                </div>
              </div>
              <div className="flex items-start gap-4">
                <Icon name="mail" size={24} className="text-primary mt-1" />
                <div>
                  <p className="text-label-md text-on-surface-variant uppercase">Email</p>
                  <p className="text-body-md text-on-background font-medium">{user?.email || 'Not set'}</p>
                </div>
              </div>
              <div className="flex items-start gap-4">
                <Icon name="call" size={24} className="text-primary mt-1" />
                <div>
                  <p className="text-label-md text-on-surface-variant uppercase">Phone</p>
                  <p className="text-body-md text-on-background font-medium">{user?.phone || 'Not set'}</p>
                </div>
              </div>
              <div className="flex items-start gap-4">
                <Icon name="tag" size={24} className="text-primary mt-1" />
                <div>
                  <p className="text-label-md text-on-surface-variant uppercase">Account Type</p>
                  <p className="text-body-md text-on-background font-medium">{user?.role || 'Customer'}</p>
                </div>
              </div>
            </div>
          )}

          {/* Profile Details - Edit Mode */}
          {isEditing && (
            <div className="space-y-4 mb-8">
              <div>
                <label className="block text-label-md text-on-surface font-medium mb-2">Full Name</label>
                <input
                  type="text"
                  name="fullName"
                  value={editForm.fullName}
                  onChange={handleEditChange}
                  className="w-full px-4 py-3 rounded-[12px] bg-surface-container-low border border-outline focus:border-primary focus:ring-2 focus:ring-primary/20 text-body-md"
                />
              </div>
              <div>
                <label className="block text-label-md text-on-surface font-medium mb-2">Phone</label>
                <input
                  type="tel"
                  name="phone"
                  value={editForm.phone}
                  onChange={handleEditChange}
                  className="w-full px-4 py-3 rounded-[12px] bg-surface-container-low border border-outline focus:border-primary focus:ring-2 focus:ring-primary/20 text-body-md"
                />
              </div>
              <div className="flex gap-3 pt-4">
                <button
                  onClick={handleSaveProfile}
                  disabled={isSaving}
                  className="flex-1 px-4 py-3 rounded-[12px] bg-primary text-on-primary font-body-md hover:bg-surface-tint transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isSaving ? 'Saving...' : 'Save Changes'}
                </button>
                <button
                  onClick={() => setIsEditing(false)}
                  disabled={isSaving}
                  className="flex-1 px-4 py-3 rounded-[12px] bg-surface-container text-on-surface font-body-md border border-outline hover:bg-surface-container-high transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Cancel
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Security Settings */}
        <div className="bg-surface rounded-[20px] p-6 shadow-ambient border border-outline mb-6">
          <h3 className="text-headline-sm font-bold text-on-background mb-6 flex items-center gap-2">
            <Icon name="security" size={24} className="text-primary" />
            Security Settings
          </h3>

          {/* 2FA Toggle */}
          <div className="flex items-center justify-between p-4 bg-surface-container rounded-[12px] mb-4">
            <div className="flex-1">
              <p className="text-body-md font-medium text-on-background">Two-Factor Authentication</p>
              <p className="text-caption-md text-on-surface-variant mt-1">
                {twoFactorEnabled 
                  ? 'When you login, you will receive an email OTP to verify your identity'
                  : 'Add an extra layer of security to your account by receiving an OTP via email during login'
                }
              </p>
            </div>
            <button
              onClick={handleToggle2FA}
              disabled={isTogglingTwoFactor}
              className={`relative ml-4 flex-shrink-0 w-12 h-6 rounded-full transition-colors ${
                twoFactorEnabled ? 'bg-success' : 'bg-outline'
              } disabled:opacity-50`}
            >
              <div
                className={`absolute top-1 left-1 w-4 h-4 bg-white rounded-full transition-transform ${
                  twoFactorEnabled ? 'translate-x-6' : ''
                }`}
              />
            </button>
          </div>

          {/* Password Change */}
          <div className="flex items-center justify-between p-4 bg-surface-container rounded-[12px] mt-4">
            <div>
              <p className="text-body-md font-medium text-on-background">Change Password</p>
              <p className="text-caption-md text-on-surface-variant mt-1">Update your password regularly to keep your account secure</p>
            </div>
            <button 
              onClick={() => navigate('/change-password')}
              className="px-4 py-2 rounded-[12px] bg-outline text-on-background font-body-md hover:bg-outline/80 transition-colors">
              Change
            </button>
          </div>
        </div>

        {/* Account Actions */}
        <div className="bg-surface rounded-[20px] p-6 shadow-ambient border border-outline">
          <h3 className="text-headline-sm font-bold text-on-background mb-4">Account Actions</h3>
          <div className="space-y-3">
            <button
              onClick={() => navigate('/delete-account')}
              className="w-full px-4 py-3 rounded-[12px] bg-error-container text-on-error-container font-body-md hover:opacity-90 transition-opacity flex items-center gap-2"
            >
              <Icon name="delete_account" size={20} />
              Delete Account
            </button>
            <button
              onClick={() => {
                logout()
              }}
              className="w-full px-4 py-3 rounded-[12px] bg-outline text-on-background font-body-md hover:bg-outline/80 transition-colors flex items-center justify-center gap-2"
            >
              <Icon name="logout" size={20} />
              Logout
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
