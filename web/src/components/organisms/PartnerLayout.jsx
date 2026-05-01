import { Link, useLocation, useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'
import { useAuth } from '../../context/AuthContext'
import { useLogoutConfirmation } from '../../hooks/useLogoutConfirmation'
import { getHybridGreeting } from '../../utils/greetingUtils'
import { useNotification } from '../../hooks/useNotification'
import { NotificationCenter } from './NotificationCenter'

const NAV_ITEMS = [
  { to: '/partner/dashboard', icon: 'dashboard',       label: 'Dashboard',  color: 'text-emerald-400' },
  { to: '/partner/queue',     icon: 'receipt_long',    label: 'Orders',     color: 'text-orange-400' },
  { to: '/partner/menu',      icon: 'restaurant_menu', label: 'Menu',       color: 'text-purple-400' },
  { to: '/partner/analytics', icon: 'analytics',       label: 'Analytics',  color: 'text-sky-400' },
  { to: '/profile',           icon: 'person',          label: 'Profile',    color: 'text-slate-400' },
]

export const PartnerLayout = ({ children, title = '' }) => {
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
      {/* ── Premium Light Sidebar ── */}
      <aside className="hidden md:flex bg-white h-screen w-64 flex-col shrink-0 z-20 relative border-r border-slate-100">
        {/* Subtle decorative elements */}
        <div className="absolute top-0 right-0 w-32 h-32 bg-indigo-50 rounded-full blur-3xl opacity-60 pointer-events-none" />

        {/* Logo */}
        <div className="px-6 py-8 relative z-10">
          <div className="flex items-center gap-3">
            <img src="/quickbite-logo.png" alt="QuickBite Logo" className="w-11 h-11 object-contain" />
            <div>
              <p className="text-base font-extrabold text-slate-900 tracking-tight">QuickBite</p>
              <p className="text-[10px] text-primary font-bold uppercase tracking-wider">Partner Portal</p>
            </div>
          </div>
        </div>

        {/* Nav */}
        <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto no-scrollbar relative z-10">
          <p className="text-[10px] font-bold text-slate-500 uppercase tracking-widest px-3 mb-3">Restaurant Menu</p>
          {NAV_ITEMS.map(({ to, icon, label, color }) => {
            const isActive = location.pathname === to
            return (
              <Link
                key={to}
                to={to}
                className={`flex items-center gap-3 px-3 py-3 rounded-2xl text-sm font-semibold transition-all duration-300 group ${
                  isActive
                    ? 'bg-primary/10 text-primary shadow-sm border border-primary/10'
                    : 'text-slate-500 hover:text-slate-900 hover:bg-slate-50'
                }`}
              >
                <div className={`w-8 h-8 rounded-xl flex items-center justify-center transition-all ${
                  isActive ? 'bg-primary text-white shadow-lg shadow-primary/30' : 'bg-slate-100 group-hover:bg-slate-200'
                }`}>
                  <span
                    className={`material-symbols-outlined text-[18px] ${isActive ? 'text-white' : 'text-slate-400 group-hover:text-slate-600'}`}
                    style={{ fontVariationSettings: isActive ? "'FILL' 1" : "'FILL' 0" }}
                  >
                    {icon}
                  </span>
                </div>
                <span>{label}</span>
                {isActive && <span className="ml-auto w-1.5 h-1.5 rounded-full bg-emerald-400" />}
              </Link>
            )
          })}
        </nav>

        {/* Bottom Profile */}
        <div className="px-3 py-4 border-t border-slate-100 relative z-10">
          <div className="flex items-center gap-3 px-3 py-2.5 bg-slate-50 rounded-2xl">
            <div className="w-9 h-9 rounded-xl bg-gradient-to-br from-primary to-indigo-400 flex items-center justify-center text-white text-xs font-bold flex-shrink-0 shadow-md">
              {(user?.email?.[0] || 'P').toUpperCase()}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xs font-bold text-slate-900 truncate">{user?.name || user?.email || 'Partner'}</p>
              <p className="text-[10px] text-slate-400 font-medium uppercase">Admin</p>
            </div>
            <button onClick={handleLogout} className="text-slate-400 hover:text-rose-500 transition-colors p-1" aria-label="Sign out">
              <span className="material-symbols-outlined text-lg">logout</span>
            </button>
          </div>
        </div>
      </aside>

      {/* ── Main Content ── */}
      <main className="flex-1 flex flex-col h-full overflow-hidden">
        {/* Top Header */}
        <header className="bg-white w-full border-b border-slate-200 flex justify-between items-center px-6 h-16 sticky top-0 z-40 shadow-sm">
          <div className="flex items-center gap-3">
            <div className="md:hidden">
              <span className="material-symbols-outlined text-slate-600 cursor-pointer">menu</span>
            </div>
            <div>
              <h1 className="text-base font-bold text-slate-900">
                {title || <span className="text-gradient-primary">{greeting.main}</span>}
              </h1>
              <p className="text-[11px] text-slate-400 font-medium hidden sm:block">{greeting.sub}</p>
            </div>
          </div>
          <div className="flex items-center gap-2">
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
            <button className="p-2 rounded-xl text-slate-500 hover:text-primary hover:bg-slate-50 transition-all" aria-label="Help">
              <span className="material-symbols-outlined text-xl">help_outline</span>
            </button>
            <Link
              to="/profile"
              className="w-9 h-9 rounded-xl bg-gradient-to-br from-primary to-indigo-400 text-white text-xs font-bold flex items-center justify-center ml-1 hover:shadow-lg transition-all"
              aria-label="Profile"
            >
              {(user?.email?.[0] || 'P').toUpperCase()}
            </Link>
          </div>
        </header>

        {/* Scrollable content */}
        <div className="flex-1 overflow-y-auto bg-slate-50">
          <div className="max-w-7xl mx-auto p-6 space-y-6 pb-24 md:pb-8">
            {children}
          </div>
        </div>
      </main>

      {/* Mobile bottom nav */}
      <nav className="md:hidden bg-white fixed bottom-0 left-0 w-full z-50 flex justify-around items-center px-4 pb-2 pt-2 h-20 rounded-t-2xl border-t border-slate-200 shadow-lg">
        {NAV_ITEMS.map(({ to, icon, label }) => {
          const isActive = location.pathname === to
          return (
            <Link
              key={to}
              to={to}
              className={`flex flex-col items-center justify-center rounded-xl px-3 py-1.5 tap-highlight-transparent transition-transform active:scale-90 ${
                isActive ? 'text-emerald-600' : 'text-slate-400'
              }`}
            >
              <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: isActive ? "'FILL' 1" : "'FILL' 0" }}>{icon}</span>
              <span className="text-xs font-medium mt-0.5">{label}</span>
            </Link>
          )
        })}
      </nav>
    </div>
  )
}

PartnerLayout.propTypes = {
  children: PropTypes.node,
  title: PropTypes.string,
}
