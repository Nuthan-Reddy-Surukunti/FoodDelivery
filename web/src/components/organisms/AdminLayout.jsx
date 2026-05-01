import { Link, useLocation, useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'
import { useAuth } from '../../context/AuthContext'
import { useLogoutConfirmation } from '../../hooks/useLogoutConfirmation'
import { getHybridGreeting } from '../../utils/greetingUtils'
import { useNotification } from '../../hooks/useNotification'
import { NotificationCenter } from './NotificationCenter'

const NAV_ITEMS = [
  { to: '/admin', icon: 'dashboard', label: 'Overview', color: 'text-blue-400' },
  { to: '/admin/restaurants', icon: 'restaurant', label: 'Restaurants', color: 'text-orange-400' },
  { to: '/admin/orders', icon: 'shopping_cart', label: 'Orders', color: 'text-purple-400' },
  { to: '/admin/users', icon: 'group', label: 'Users', color: 'text-emerald-400' },
  { to: '/profile', icon: 'person', label: 'Profile', color: 'text-sky-400' },
]

export const AdminLayout = ({ children, title = 'Admin', searchPlaceholder = '' }) => {
  const location = useLocation()
  const navigate = useNavigate()
  const { user, logout } = useAuth()
  const { confirmLogout } = useLogoutConfirmation()
  const greeting = getHybridGreeting(user?.role, user?.name || user?.email)
  const {
    inboxItems,
    unreadCount,
    isCenterOpen,
    toggleCenter,
    closeCenter,
    markAllRead,
    markRead,
    refreshInbox,
  } = useNotification()

  const handleLogout = () => {
    confirmLogout(async () => {
      await logout()
      navigate('/login')
    })
  }

  return (
    <div className="bg-slate-50 text-slate-900 h-screen w-full overflow-hidden flex font-sans">
      {/* ── Dark Sidebar ── */}
      <aside className="hidden md:flex bg-[#0f172a] h-screen w-64 flex-col shrink-0 z-20 relative">
        {/* Subtle gradient overlay */}
        <div className="absolute inset-0 bg-gradient-to-b from-[#1e293b]/50 to-transparent pointer-events-none" />

        {/* Logo */}
        <div className="px-6 py-6 border-b border-white/5 relative z-10">
          <div className="flex items-center gap-3">
            <img src="/quickbite-logo-white.png" alt="QuickBite Logo" className="w-9 h-9 object-contain logo-glow-white" />
            <div>
              <p className="text-sm font-extrabold text-white tracking-tight">QuickBite</p>
              <p className="text-[11px] text-slate-400 font-medium">Admin Portal</p>
            </div>
          </div>
        </div>

        {/* Nav */}
        <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto no-scrollbar relative z-10">
          <p className="text-[10px] font-bold text-slate-500 uppercase tracking-widest px-3 mb-3">Main Menu</p>
          {NAV_ITEMS.map(({ to, icon, label, color }) => {
            const isActive = location.pathname === to || (to !== '/admin' && location.pathname.startsWith(to))
            return (
              <Link
                key={to}
                to={to}
                className={`flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all duration-200 group ${
                  isActive
                    ? 'bg-white/10 text-white shadow-sm border border-white/10'
                    : 'text-slate-400 hover:text-white hover:bg-white/5'
                }`}
              >
                <div className={`w-8 h-8 rounded-lg flex items-center justify-center transition-colors ${
                  isActive ? 'bg-primary/20' : 'bg-white/5 group-hover:bg-white/10'
                }`}>
                  <span
                    className={`material-symbols-outlined text-[18px] ${isActive ? color : 'text-slate-400 group-hover:text-slate-200'}`}
                    style={{ fontVariationSettings: isActive ? "'FILL' 1" : "'FILL' 0" }}
                  >
                    {icon}
                  </span>
                </div>
                <span>{label}</span>
                {isActive && (
                  <span className="ml-auto w-1.5 h-1.5 rounded-full bg-primary" />
                )}
              </Link>
            )
          })}
        </nav>

        {/* Bottom section */}
        <div className="px-3 py-4 border-t border-white/5 relative z-10 space-y-2">
          <button className="w-full bg-gradient-to-r from-primary to-indigo-600 text-white py-2.5 px-4 rounded-xl text-sm font-semibold hover:shadow-lg hover:shadow-primary/25 transition-all active:scale-95">
            Generate Reports
          </button>
          <div className="flex items-center gap-3 px-3 py-2.5">
            <div className="w-8 h-8 rounded-full bg-gradient-to-br from-primary to-indigo-500 flex items-center justify-center text-white text-xs font-bold flex-shrink-0">
              {(user?.email?.[0] || 'A').toUpperCase()}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xs font-semibold text-white truncate">{user?.name || user?.email || 'Admin'}</p>
              <p className="text-[10px] text-slate-400">Administrator</p>
            </div>
            <button
              onClick={handleLogout}
              className="text-slate-400 hover:text-rose-400 transition-colors"
              aria-label="Sign out"
            >
              <span className="material-symbols-outlined text-lg">logout</span>
            </button>
          </div>
        </div>
      </aside>

      {/* ── Main Content ── */}
      <main className="flex-1 flex flex-col h-full overflow-hidden">
        {/* Top Header */}
        <header className="bg-white w-full border-b border-slate-200 flex justify-between items-center px-8 h-16 sticky top-0 z-40 shadow-sm">
          <div className="flex items-center gap-4">
            <div className="md:hidden">
              <span className="material-symbols-outlined text-slate-600 cursor-pointer">menu</span>
            </div>
            <div>
              <h1 className="text-base font-bold text-slate-900">{title || greeting.main}</h1>
              <p className="text-[11px] text-slate-400 font-medium hidden sm:block">
                {title && title !== 'Admin' ? 'QuickBite Admin' : greeting.sub}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            {searchPlaceholder && (
              <div className="relative hidden md:block">
                <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-[18px]">search</span>
                <input
                  type="text"
                  placeholder={searchPlaceholder}
                  className="pl-9 pr-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-sm w-56 placeholder:text-slate-400 outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all"
                />
              </div>
            )}
            <div className="flex items-center gap-1">
              <div className="relative">
                <button
                  type="button"
                  onClick={() => toggleCenter(user?.role)}
                  className="relative p-2 rounded-xl text-slate-500 hover:text-primary hover:bg-slate-50 transition-all"
                  aria-label="Notifications"
                >
                  <span className="material-symbols-outlined text-xl">notifications</span>
                  {unreadCount > 0 && (
                    <span className="absolute -top-0.5 -right-0.5 bg-primary text-white text-[10px] font-bold w-5 h-5 rounded-full flex items-center justify-center shadow-sm">
                      {unreadCount}
                    </span>
                  )}
                </button>
                <NotificationCenter
                  isOpen={isCenterOpen}
                  items={inboxItems}
                  unreadCount={unreadCount}
                  onClose={closeCenter}
                  onMarkAllRead={markAllRead}
                  onRefresh={() => refreshInbox(user?.role)}
                  onMarkRead={markRead}
                />
              </div>
              <button className="p-2 rounded-xl text-slate-500 hover:text-primary hover:bg-slate-50 transition-all" aria-label="Settings">
                <span className="material-symbols-outlined text-xl">settings</span>
              </button>
              <Link
                to="/profile"
                className="w-8 h-8 rounded-xl bg-gradient-to-br from-primary to-indigo-600 text-white text-xs font-bold flex items-center justify-center ml-1 hover:shadow-md transition-all"
                aria-label="Profile"
              >
                {(user?.email?.[0] || 'A').toUpperCase()}
              </Link>
            </div>
          </div>
        </header>

        {/* Scrollable content */}
        <div className="flex-1 overflow-y-auto bg-slate-50">
          <div className="max-w-7xl mx-auto p-8 space-y-6 pb-24 md:pb-8">
            {children}
          </div>
        </div>
      </main>
    </div>
  )
}

AdminLayout.propTypes = {
  children: PropTypes.node,
  title: PropTypes.string,
  searchPlaceholder: PropTypes.string,
}
