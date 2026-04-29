import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { authApi } from '../services/authApi'
import { PartnerLayout } from '../components/organisms/PartnerLayout'
import { AdminLayout } from '../components/organisms/AdminLayout'
import { AgentLayout } from '../components/organisms/AgentLayout'

export const ProfilePage = () => {
  const navigate = useNavigate()
  const { user, logout, token, setAuthUser } = useAuth()
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
      const response = await authApi.updateProfile(user?.id, editForm.fullName, editForm.phone)
      
      if (response.success) {
        // Sync the updated user data to AuthContext so other components reflect the change
        setAuthUser(response.user, token)
        setMessage({ type: 'success', text: 'Profile updated successfully!' })
        setIsEditing(false)
      } else {
        throw new Error(response.message || 'Failed to update profile')
      }
      
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

  const content = (
    <div className="relative max-w-4xl mx-auto px-6 py-10">
      {/* ── Profile Hero Header ── */}
      <div className="mb-10 flex flex-col sm:flex-row items-center sm:items-end gap-5 p-6 bg-white rounded-2xl border border-slate-100 shadow-sm">
        {/* Avatar */}
        <div className="w-20 h-20 rounded-2xl bg-gradient-to-br from-primary to-indigo-600 flex items-center justify-center text-4xl font-black text-white shadow-lg shadow-primary/25 flex-shrink-0">
          {user?.name?.charAt(0)?.toUpperCase() || 'U'}
        </div>
        <div className="flex-1 text-center sm:text-left">
          <h1 className="text-2xl font-extrabold text-slate-900">{user?.name || 'Your Profile'}</h1>
          <p className="text-slate-500 text-sm mt-0.5">{user?.email}</p>
          <span className="inline-flex items-center gap-1.5 mt-2 bg-primary/10 text-primary text-xs font-bold px-3 py-1 rounded-full">
            <span className="material-symbols-outlined text-xs" style={{ fontVariationSettings: "'FILL' 1" }}>verified</span>
            {user?.role || 'Customer'}
          </span>
        </div>
        <button
          onClick={() => setIsEditing(!isEditing)}
          className="btn-primary-gradient text-white px-5 py-2.5 rounded-xl text-sm font-semibold flex items-center gap-2 flex-shrink-0"
        >
          <span className="material-symbols-outlined text-lg">{isEditing ? 'close' : 'edit_square'}</span>
          {isEditing ? 'Cancel' : 'Edit Profile'}
        </button>
      </div>

      {/* Message Alert */}
      {message && (
        <div className={`mb-8 p-4 rounded-xl flex items-center gap-3 border shadow-sm ${
          message.type === 'success' 
            ? 'bg-emerald-50 border-emerald-100 text-emerald-700' 
            : 'bg-rose-50 border-rose-100 text-rose-700'
        }`}>
          <span className="material-symbols-outlined text-xl">
            {message.type === 'success' ? 'check_circle' : 'error'}
          </span>
          <span className="text-sm font-semibold">{message.text}</span>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* ── Left Sidebar: Profile Summary ── */}
        <div className="lg:col-span-1 space-y-6">
          <div className="bg-white rounded-2xl p-8 text-center border border-slate-200 shadow-sm overflow-hidden relative">
            <div className="absolute top-0 left-0 w-full h-24 bg-gradient-to-br from-primary/15 to-primary/5" />
            
            <div className="relative mb-6">
              <div className="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-primary to-primary-container p-1 shadow-sm">
                <div className="w-full h-full rounded-full bg-white flex items-center justify-center text-3xl font-black text-primary">
                  {user?.name?.charAt(0)?.toUpperCase() || 'U'}
                </div>
              </div>
              <div className="absolute -bottom-1 -right-1 w-8 h-8 rounded-full bg-emerald-500 border-4 border-white shadow-sm flex items-center justify-center">
                <span className="material-symbols-outlined text-white text-[14px]">verified</span>
              </div>
            </div>

            <h2 className="text-2xl font-bold text-slate-900 mb-1">{user?.name || 'User'}</h2>
            <p className="text-sm font-semibold text-primary/90 uppercase tracking-wide mb-6">
              {user?.role || 'Customer'}
            </p>

            <button
              onClick={() => setIsEditing(!isEditing)}
              className="w-full py-3 rounded-xl bg-primary text-white text-sm font-semibold shadow-sm hover:bg-primary-container active:scale-95 transition-all flex items-center justify-center gap-2"
            >
              <span className="material-symbols-outlined text-lg">{isEditing ? 'close' : 'edit_square'}</span>
              {isEditing ? 'Cancel Editing' : 'Edit Profile'}
            </button>
          </div>

          {/* Quick Stats/Badges */}
          <div className="bg-white rounded-2xl p-6 border border-slate-200 shadow-sm">
            <h3 className="text-sm font-bold text-slate-400 uppercase tracking-widest mb-4">Account Stats</h3>
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <span className="material-symbols-outlined text-slate-400">calendar_today</span>
                  <span className="text-sm text-slate-600 font-medium">Joined</span>
                </div>
                <span className="text-sm font-bold text-slate-900">Oct 2024</span>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <span className="material-symbols-outlined text-slate-400">shopping_bag</span>
                  <span className="text-sm text-slate-600 font-medium">Total Orders</span>
                </div>
                <span className="text-sm font-bold text-slate-900">12</span>
              </div>
            </div>
          </div>
        </div>

        {/* ── Main Content: Details & Security ── */}
        <div className="lg:col-span-2 space-y-8">
          
          {/* Personal Information */}
          <section className="bg-white rounded-2xl p-8 border border-slate-200 shadow-sm">
            <h3 className="text-xl font-bold text-slate-900 mb-8 flex items-center gap-3">
              <span className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center">
                <span className="material-symbols-outlined text-primary">person</span>
              </span>
              Personal Information
            </h3>

            {!isEditing ? (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                <div className="space-y-1">
                  <label className="text-xs font-semibold text-slate-400 uppercase tracking-wide">Full Name</label>
                  <p className="text-base font-semibold text-slate-900">{user?.name || 'Not set'}</p>
                </div>
                <div className="space-y-1">
                  <label className="text-xs font-semibold text-slate-400 uppercase tracking-wide">Email Address</label>
                  <p className="text-base font-semibold text-slate-900">{user?.email || 'Not set'}</p>
                </div>
                <div className="space-y-1">
                  <label className="text-xs font-semibold text-slate-400 uppercase tracking-wide">Phone Number</label>
                  <p className="text-base font-semibold text-slate-900">{user?.phone || 'Not set'}</p>
                </div>
                <div className="space-y-1">
                  <label className="text-xs font-semibold text-slate-400 uppercase tracking-wide">Service Zone</label>
                  <p className="text-base font-semibold text-slate-900">Downtown Bangalore</p>
                </div>
              </div>
            ) : (
              <div className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div className="space-y-2">
                    <label className="text-sm font-bold text-slate-700">Full Name</label>
                    <input
                      type="text"
                      name="fullName"
                      value={editForm.fullName}
                      onChange={handleEditChange}
                      className="w-full px-5 py-3.5 rounded-xl bg-white border border-slate-300 focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none transition-all font-medium"
                    />
                  </div>
                  <div className="space-y-2">
                    <label className="text-sm font-bold text-slate-700">Phone Number</label>
                    <input
                      type="tel"
                      name="phone"
                      value={editForm.phone}
                      onChange={handleEditChange}
                      className="w-full px-5 py-3.5 rounded-xl bg-white border border-slate-300 focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none transition-all font-medium"
                    />
                  </div>
                </div>
                <div className="flex gap-4 pt-4">
                  <button
                    onClick={handleSaveProfile}
                    disabled={isSaving}
                    className="flex-1 py-4 rounded-xl bg-primary text-white font-semibold shadow-sm hover:bg-primary-container transition-all disabled:opacity-50"
                  >
                    {isSaving ? 'Saving Changes...' : 'Save Profile'}
                  </button>
                  <button
                    onClick={() => setIsEditing(false)}
                    className="px-8 py-4 rounded-xl bg-slate-100 text-slate-700 font-semibold hover:bg-slate-200 transition-all"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            )}
          </section>

          {/* Security & Settings */}
          <section className="bg-white rounded-2xl p-8 border border-slate-200 shadow-sm">
            <h3 className="text-xl font-bold text-slate-900 mb-8 flex items-center gap-3">
              <span className="w-10 h-10 rounded-xl bg-emerald-100 flex items-center justify-center">
                <span className="material-symbols-outlined text-emerald-600">security</span>
              </span>
              Security & Authentication
            </h3>

            <div className="space-y-4">
              {/* 2FA Card */}
              <div className="p-6 rounded-xl bg-white border border-slate-200 flex items-center justify-between hover:border-primary/30 transition-colors">
                <div className="flex gap-4 min-w-0">
                  <div className={`w-12 h-12 rounded-2xl flex items-center justify-center shrink-0 ${twoFactorEnabled ? 'bg-emerald-100 text-emerald-600' : 'bg-slate-100 text-slate-400'}`}>
                    <span className="material-symbols-outlined">{twoFactorEnabled ? 'lock' : 'lock_open'}</span>
                  </div>
                  <div>
                    <h4 className="text-sm font-bold text-slate-900">Two-Factor Authentication</h4>
                    <p className="text-xs text-slate-500 mt-1 leading-relaxed max-w-sm">
                      Enhance your account security with email verification codes during login.
                    </p>
                  </div>
                </div>
                <button
                  onClick={handleToggle2FA}
                  disabled={isTogglingTwoFactor}
                  className={`relative w-14 h-7 rounded-full shrink-0 transition-all ${twoFactorEnabled ? 'bg-emerald-500' : 'bg-slate-300'}`}
                >
                  <div className={`absolute top-1 left-1 w-5 h-5 bg-white rounded-full transition-transform ${twoFactorEnabled ? 'translate-x-7' : ''}`} />
                </button>
              </div>

              {/* Password Change Card */}
              <div className="p-6 rounded-xl bg-white border border-slate-200 flex items-center justify-between hover:border-primary/30 transition-colors group">
                <div className="flex gap-4">
                  <div className="w-12 h-12 rounded-2xl bg-indigo-100 text-indigo-600 flex items-center justify-center">
                    <span className="material-symbols-outlined">key</span>
                  </div>
                  <div>
                    <h4 className="text-sm font-bold text-slate-900">Account Password</h4>
                    <p className="text-xs text-slate-500 mt-1">Last changed 3 months ago</p>
                  </div>
                </div>
                <button
                  onClick={() => navigate('/change-password')}
                  className="px-6 py-2.5 rounded-lg border border-indigo-200 text-indigo-700 text-xs font-semibold hover:bg-indigo-50 transition-colors"
                >
                  Change
                </button>
              </div>
            </div>
          </section>

          {/* Quick Actions Footer */}
          <div className="flex flex-wrap gap-4">
            <button
              onClick={() => navigate('/addresses')}
              className="flex-1 min-w-[200px] p-4 bg-white rounded-xl border border-slate-200 flex items-center gap-3 hover:border-primary/30 transition-all group"
            >
              <div className="w-10 h-10 rounded-xl bg-orange-100 text-orange-600 flex items-center justify-center group-hover:scale-110 transition-transform">
                <span className="material-symbols-outlined">location_on</span>
              </div>
              <span className="text-sm font-bold text-slate-700">Manage Addresses</span>
            </button>

            <button
              onClick={() => logout()}
              className="p-4 rounded-xl bg-rose-50 text-rose-600 border border-rose-100 flex items-center gap-3 hover:bg-rose-100 transition-all group"
            >
              <span className="material-symbols-outlined group-hover:rotate-12 transition-transform">logout</span>
              <span className="text-sm font-bold">Logout</span>
            </button>
            
            <button
              onClick={() => navigate('/delete-account')}
              className="p-4 rounded-xl bg-slate-100 text-slate-500 border border-slate-200 flex items-center gap-2 hover:bg-slate-200 transition-all"
            >
              <span className="material-symbols-outlined text-[18px]">delete_forever</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  )

  if (user?.role === 'RestaurantPartner') {
    return <PartnerLayout title="Account Settings">{content}</PartnerLayout>
  }
  
  if (user?.role === 'Admin') {
    return <AdminLayout title="Account Settings">{content}</AdminLayout>
  }
  
  if (user?.role === 'DeliveryAgent') {
    return <AgentLayout title="Account Settings">{content}</AgentLayout>
  }

  return (
    <div className="min-h-screen bg-slate-50">
      {content}
    </div>
  )
}
