import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useLogoutConfirmation } from '../hooks/useLogoutConfirmation'
import { authApi } from '../services/authApi'
import profileApi from '../services/profileApi'
import adminApi from '../services/adminApi'
import partnerApi from '../services/partnerApi'
import catalogApi from '../services/catalogApi'
import { PartnerLayout } from '../components/organisms/PartnerLayout'
import { AdminLayout } from '../components/organisms/AdminLayout'
import { AgentLayout } from '../components/organisms/AgentLayout'

const ROLE_THEMES = {
  Customer: {
    primary: 'from-primary to-rose-500',
    accent: 'text-primary',
    bg: 'bg-primary/10',
    icon: 'person',
    label: 'Foodie Explorer'
  },
  RestaurantPartner: {
    primary: 'from-primary to-rose-500',
    accent: 'text-primary',
    bg: 'bg-primary/10',
    icon: 'storefront',
    label: 'Restaurant Partner'
  },
  DeliveryAgent: {
    primary: 'from-primary to-rose-500',
    accent: 'text-primary',
    bg: 'bg-primary/10',
    icon: 'two_wheeler',
    label: 'Delivery Hero'
  },
  Admin: {
    primary: 'from-slate-700 to-slate-800',
    accent: 'text-slate-700',
    bg: 'bg-slate-100',
    icon: 'shield',
    label: 'Platform Admin'
  }
}

export const ProfilePage = () => {
  const navigate = useNavigate()
  const { user, logout, token, setAuthUser } = useAuth()
  const { confirmLogout } = useLogoutConfirmation()
  const [isEditing, setIsEditing] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [twoFactorEnabled, setTwoFactorEnabled] = useState(user?.isTwoFactorEnabled || false)
  const [isTogglingTwoFactor, setIsTogglingTwoFactor] = useState(false)
  const [stats, setStats] = useState(null)
  const [count,setCount] = useState(0)
  const [loadingStats, setLoadingStats] = useState(true)
  const [editForm, setEditForm] = useState({
    fullName: user?.name || '',
    phone: user?.phone || '',
  })
  const [message, setMessage] = useState(null)

  const theme = ROLE_THEMES[user?.role] || ROLE_THEMES.Customer

  useEffect(() => {
    const fetchStats = async () => {
      try {
        setLoadingStats(true)
        let data = await profileApi.getProfileStats()

        // Supplement data for specialized roles
        try {
          if (user?.role === 'RestaurantPartner') {
            const restaurant = await catalogApi.getMyRestaurant()
            if (restaurant) {
              const partnerStats = await partnerApi.getDashboardStats(restaurant.id)
              data = { 
                ...data, 
                lifetimeRevenue: partnerStats?.totalRevenue || 0,
                averageRating: restaurant?.rating || 0,
                menuItemsCount: restaurant?.menuItems?.length || 0,
                restaurantName: restaurant?.name
              }
            }
          } else if (user?.role === 'Admin') {
            const adminKpis = await adminApi.getDashboardKpis()
            data = {
              ...data,
              systemUsersCount: adminKpis?.totalUsers || 0,
              systemRevenue: adminKpis?.totalRevenue || 0
            }
          }
        } catch (supplementalErr) {
          console.warn("Failed to fetch supplemental profile stats", supplementalErr)
        }

        setStats(data)
      } catch (err) {
        console.error('Failed to fetch base profile stats', err)
      } finally {
        setLoadingStats(false)
      }
    }
    fetchStats()
  }, [user?.role])

  const handleLogout = () => {
    confirmLogout(async () => {
      await logout()
      navigate('/login')
    })
  }

  const handleEditChange = (e) => {
    const { name, value } = e.target
    setEditForm(prev => ({ ...prev, [name]: value }))
    setFieldError('')
  }

  const handleSaveProfile = async () => {
    try {
      setIsSaving(true)
      const response = await authApi.updateProfile(user?.id, editForm.fullName, editForm.phone)
      
      if (response.success) {
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
        text: newState ? '2FA enabled! You will now receive an email OTP during login.' : '2FA disabled successfully'
      })
      setTimeout(() => setMessage(null), 4000)
    } catch (error) {
      setMessage({ type: 'error', text: error.message || 'Failed to update 2FA settings' })
    } finally {
      setIsTogglingTwoFactor(false)
    }
  }

  const statCards = () => {
    if (loadingStats) {
      return [1, 2, 3].map(i => <div key={i} className="h-20 bg-slate-100 animate-pulse rounded-2xl" />)
    }

    const items = []
    if (user?.role === 'Customer') {
      items.push({ label: 'Total Orders', value: stats?.totalOrders ?? 0, icon: 'shopping_bag' })
      items.push({ label: 'Active Orders', value: stats?.activeOrders ?? 0, icon: 'package_2' })
      items.push({ label: 'Addresses', value: stats?.savedAddressesCount ?? 0, icon: 'location_on' })
    } else if (user?.role === 'RestaurantPartner') {
      items.push({ label: 'Revenue', value: `₹${(stats?.lifetimeRevenue ?? 0).toLocaleString()}`, icon: 'payments' })
      items.push({ label: 'Rating', value: `⭐ ${stats?.averageRating?.toFixed(1) ?? 'N/A'}`, icon: 'star' })
      items.push({ label: 'Menu Items', value: stats?.menuItemsCount ?? 0, icon: 'restaurant_menu' })
    } else if (user?.role === 'DeliveryAgent') {
      items.push({ label: 'Earnings', value: `₹${(stats?.totalEarnings ?? 0).toLocaleString()}`, icon: 'account_balance_wallet' })
      items.push({ label: 'Deliveries', value: stats?.deliveriesCompleted ?? 0, icon: 'task_alt' })
      items.push({ label: 'Status', value: stats?.currentStatus ?? 'Active', icon: 'online_prediction' })
    } else if (user?.role === 'Admin') {
      items.push({ label: 'Total Users', value: (stats?.systemUsersCount ?? 0).toLocaleString(), icon: 'group' })
      items.push({ label: 'Platform GMV', value: `₹${(stats?.systemRevenue ?? 0).toLocaleString()}`, icon: 'trending_up' })
      items.push({ label: 'Security', value: 'Active', icon: 'verified_user' })
    }

    return items.map((item, idx) => (
      <div key={idx} className="bg-white/50 backdrop-blur-sm p-4 rounded-xl border border-white/40 shadow-sm flex items-center gap-4 hover-lift">
        <div className={`w-10 h-10 rounded-xl ${theme.bg} ${theme.accent} flex items-center justify-center`}>
          <span className="material-symbols-outlined text-[18px]">{item.icon}</span>
        </div>
        <div>
          <p className="text-[10px] font-bold text-slate-400 uppercase tracking-wider">{item.label}</p>
          <p className="text-base font-bold text-slate-900 leading-tight">{item.value}</p>
        </div>
      </div>
    ))
  }

  const content = (
    <div className="relative max-w-5xl mx-auto px-4 py-8 md:py-12 animate-fade-in">
      {/* ── Background Decorative Blobs ── */}
      <div className={`absolute top-[-10%] right-[-5%] w-[250px] h-[250px] rounded-full bg-gradient-to-br ${theme.primary} blur-[120px] opacity-10 pointer-events-none`} />
      <div className={`absolute bottom-[5%] left-[-5%] w-[200px] h-[200px] rounded-full bg-gradient-to-tr ${theme.primary} blur-[100px] opacity-10 pointer-events-none`} />

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
        {/* ── Left Profile Card ── */}
        <div className="lg:col-span-4 space-y-6">
          <div className="glass-panel-premium rounded-3xl p-6 text-center relative overflow-hidden group border border-slate-100">
            <div className={`absolute top-0 left-0 w-full h-28 bg-gradient-to-br ${theme.primary} opacity-10 group-hover:opacity-20 transition-opacity`} />
            
            <div className="relative mb-5 mt-4">
              <div className={`w-24 h-24 mx-auto rounded-2xl bg-gradient-to-br ${theme.primary} p-[2px] shadow-lg shadow-primary/10 rotate-3 group-hover:rotate-0 transition-transform duration-500`}>
                <div className="w-full h-full rounded-[0.9rem] bg-white flex items-center justify-center text-3xl font-black text-slate-800">
                  {user?.name?.charAt(0)?.toUpperCase() || 'U'}
                </div>
              </div>
              <div className="absolute -bottom-1 right-1/4 translate-x-1/2 w-7 h-7 rounded-full bg-white border border-slate-100 shadow-md flex items-center justify-center text-emerald-500">
                <span className="material-symbols-outlined text-[16px] filled">verified</span>
              </div>
            </div>

            <h2 className="text-lg font-bold text-slate-800 tracking-tight mb-0.5">{user?.name || 'User'}</h2>
            <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-5">
              {theme.label}
            </p>

            <div className="flex flex-col gap-2.5">
              <button
                onClick={() => setIsEditing(!isEditing)}
                className={`w-full py-3 rounded-xl bg-gradient-to-r ${theme.primary} text-white text-sm font-semibold shadow-md hover:shadow-lg transition-all flex items-center justify-center gap-2`}
              >
                <span className="material-symbols-outlined text-[18px]">{isEditing ? 'close' : 'edit_square'}</span>
                {isEditing ? 'Cancel Editing' : 'Edit Profile'}
              </button>
              
              <button
                onClick={handleLogout}
                className="w-full py-3 rounded-xl bg-white border border-slate-200 text-slate-500 text-sm font-semibold hover:bg-slate-50 transition-all flex items-center justify-center gap-2"
              >
                <span className="material-symbols-outlined text-[18px]">logout</span>
                Sign Out
              </button>
            </div>
          </div>

          {/* Role Stats Grid */}
          <div className="grid grid-cols-1 gap-3">
            {statCards()}
          </div>
        </div>

        {/* ── Right Content ── */}
        <div className="lg:col-span-8 space-y-6">
          {message && (
            <div className={`p-4 rounded-xl flex items-center gap-3 border animate-bounce-in ${
              message.type === 'success' ? 'bg-emerald-50 border-emerald-100 text-emerald-700' : 'bg-rose-50 border-rose-100 text-rose-700'
            }`}>
              <span className="material-symbols-outlined text-[18px]">{message.type === 'success' ? 'check_circle' : 'error'}</span>
              <span className="text-sm font-semibold">{message.text}</span>
            </div>
          )}

          {/* Personal Info */}
          <div className="glass-panel-premium rounded-3xl p-6 md:p-8 relative z-10 border border-slate-100">
            <h3 className="text-lg font-bold text-slate-800 mb-6 flex items-center gap-3">
              <div className={`w-10 h-10 rounded-xl ${theme.bg} ${theme.accent} flex items-center justify-center shadow-inner`}>
                <span className="material-symbols-outlined text-[20px]">account_circle</span>
              </div>
              Personal Details
            </h3>

            {!isEditing ? (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-6">
                {[
                  { label: 'Display Name', value: user?.name, icon: 'badge' },
                  { label: 'Email Address', value: user?.email, icon: 'mail' },
                  { label: 'Phone Number', value: user?.phone || 'Not provided', icon: 'call' },
                  { label: 'Member Since', value: user?.createdAt ? new Date(user.createdAt).toLocaleDateString('en-IN', { month: 'long', year: 'numeric' }) : 'Recently', icon: 'calendar_today' },
                ].map((item, i) => (
                  <div key={i} className="group cursor-default bg-slate-50/50 p-4 rounded-xl border border-transparent hover:border-slate-100 transition-colors">
                    <label className="text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-1.5 flex items-center gap-1.5">
                      <span className="material-symbols-outlined text-[14px]">{item.icon}</span>
                      {item.label}
                    </label>
                    <p className="text-sm font-semibold text-slate-700 group-hover:text-primary transition-colors">{item.value}</p>
                  </div>
                ))}
              </div>
            ) : (
              <div className="space-y-5 animate-fade-in">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                  <div className="space-y-1.5">
                    <label className="text-[11px] font-bold text-slate-500 ml-1 uppercase tracking-wider">Full Name</label>
                    <input
                      type="text"
                      name="fullName"
                      value={editForm.fullName}
                      onChange={handleEditChange}
                      className="w-full px-4 py-3 rounded-xl bg-white/80 border border-slate-200 focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none transition-all text-sm font-medium text-slate-700"
                    />
                  </div>
                  <div className="space-y-1.5">
                    <label className="text-[11px] font-bold text-slate-500 ml-1 uppercase tracking-wider">Phone Number</label>
                    <input
                      type="tel"
                      name="phone"
                      value={editForm.phone}
                      onChange={handleEditChange}
                      className="w-full px-4 py-3 rounded-xl bg-white/80 border border-slate-200 focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none transition-all text-sm font-medium text-slate-700"
                    />
                  </div>
                </div>
                <div className="flex gap-3 pt-3">
                  <button
                    onClick={handleSaveProfile}
                    disabled={isSaving}
                    className={`flex-1 py-3 rounded-xl bg-gradient-to-r ${theme.primary} text-white font-semibold text-sm shadow-md hover:shadow-lg transition-all disabled:opacity-50`}
                  >
                    {isSaving ? 'Updating...' : 'Save Changes'}
                  </button>
                  <button
                    onClick={() => setIsEditing(false)}
                    className="px-8 py-3 rounded-xl bg-slate-100 text-slate-600 font-semibold text-sm hover:bg-slate-200 transition-all"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            )}
          </div>

          {/* Security Area */}
          <div className="glass-panel-premium rounded-3xl p-6 md:p-8 border border-slate-100">
            <h3 className="text-lg font-bold text-slate-800 mb-6 flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-slate-100 text-slate-500 flex items-center justify-center shadow-inner">
                <span className="material-symbols-outlined text-[20px]">verified_user</span>
              </div>
              Account Security
            </h3>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* 2FA Toggle */}
              <div className="p-5 rounded-2xl bg-white border border-slate-100 flex items-center justify-between group hover:border-slate-200 transition-all shadow-sm hover:shadow-md">
                <div className="flex gap-3">
                  <div className={`w-10 h-10 rounded-xl flex items-center justify-center shrink-0 ${twoFactorEnabled ? 'bg-emerald-500 text-white shadow-md' : 'bg-slate-100 text-slate-400'}`}>
                    <span className="material-symbols-outlined text-[18px]">{twoFactorEnabled ? 'shield_lock' : 'security'}</span>
                  </div>
                  <div>
                    <h4 className="text-sm font-semibold text-slate-700">Two-Factor Auth</h4>
                    <p className="text-[10px] text-slate-400 font-bold uppercase tracking-wider mt-0.5">{twoFactorEnabled ? 'Enabled' : 'Disabled'}</p>
                  </div>
                </div>
                <button
                  onClick={handleToggle2FA}
                  disabled={isTogglingTwoFactor}
                  className={`relative w-11 h-6 rounded-full shrink-0 transition-all ${twoFactorEnabled ? 'bg-emerald-500' : 'bg-slate-200'}`}
                >
                  <div className={`absolute top-1 left-1 w-4 h-4 bg-white rounded-full transition-transform ${twoFactorEnabled ? 'translate-x-5' : ''}`} />
                </button>
              </div>

              {/* Password Change */}
              <button
                onClick={() => navigate('/change-password')}
                className="p-5 rounded-2xl bg-white border border-slate-100 flex items-center justify-between group hover:border-slate-200 transition-all text-left shadow-sm hover:shadow-md"
              >
                <div className="flex gap-3">
                  <div className="w-10 h-10 rounded-xl bg-slate-50 text-slate-500 flex items-center justify-center group-hover:scale-105 transition-transform border border-slate-100">
                    <span className="material-symbols-outlined text-[18px]">password</span>
                  </div>
                  <div>
                    <h4 className="text-sm font-semibold text-slate-700">Change Password</h4>
                    <p className="text-[10px] text-slate-400 font-bold uppercase tracking-wider mt-0.5">Manage Security</p>
                  </div>
                </div>
                <span className="material-symbols-outlined text-slate-300 group-hover:text-primary transition-colors text-[18px]">arrow_forward_ios</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  )

  if (user?.role === 'RestaurantPartner') return <PartnerLayout title="Profile Settings">{content}</PartnerLayout>
  if (user?.role === 'Admin') return <AdminLayout title="System Profile">{content}</AdminLayout>
  if (user?.role === 'DeliveryAgent') return <AgentLayout title="Agent Identity">{content}</AgentLayout>

  return <div className="min-h-screen bg-slate-50">{content}</div>
}

