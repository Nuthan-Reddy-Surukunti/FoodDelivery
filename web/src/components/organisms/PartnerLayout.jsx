import { Link, useLocation, useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'
import { useAuth } from '../../context/AuthContext'

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

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="bg-slate-50 text-slate-900 h-screen w-full overflow-hidden flex font-sans">
      {/* ── Dark Green Sidebar ── */}
      <aside className="hidden md:flex bg-[#0d1f1a] h-screen w-64 flex-col shrink-0 z-20 relative">
        {/* Subtle gradient */}
        <div className="absolute inset-0 bg-gradient-to-b from-[#1a3d30]/50 to-transparent pointer-events-none" />

        {/* Logo */}
        <div className="px-6 py-6 border-b border-white/5 relative z-10">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 bg-gradient-to-br from-emerald-500 to-teal-600 rounded-xl flex items-center justify-center shadow-lg shadow-emerald-900/50">
              <span className="material-symbols-outlined text-white text-lg" style={{ fontVariationSettings: "'FILL' 1" }}>storefront</span>
            </div>
            <div>
              <p className="text-sm font-extrabold text-white tracking-tight">QuickBite</p>
              <p className="text-[11px] text-emerald-400/70 font-medium">Partner Portal</p>
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
                className={`flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all duration-200 group ${
                  isActive
                    ? 'bg-white/10 text-white shadow-sm border border-white/10'
                    : 'text-slate-400 hover:text-white hover:bg-white/5'
                }`}
              >
                <div className={`w-8 h-8 rounded-lg flex items-center justify-center transition-colors ${
                  isActive ? 'bg-emerald-500/20' : 'bg-white/5 group-hover:bg-white/10'
                }`}>
                  <span
                    className={`material-symbols-outlined text-[18px] ${isActive ? color : 'text-slate-400 group-hover:text-slate-200'}`}
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

        {/* Bottom */}
        <div className="px-3 py-4 border-t border-white/5 relative z-10">
          <div className="flex items-center gap-3 px-3 py-2.5">
            <div className="w-8 h-8 rounded-full bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center text-white text-xs font-bold flex-shrink-0">
              {(user?.email?.[0] || 'P').toUpperCase()}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xs font-semibold text-white truncate">{user?.name || user?.email || 'Partner'}</p>
              <p className="text-[10px] text-emerald-400/70">Restaurant Partner</p>
            </div>
            <button onClick={handleLogout} className="text-slate-400 hover:text-rose-400 transition-colors" aria-label="Sign out">
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
                {title || <span className="text-gradient-primary">Restaurant Dashboard</span>}
              </h1>
              <p className="text-[11px] text-slate-400 font-medium hidden sm:block">QuickBite Partner</p>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <button className="p-2 rounded-xl text-slate-500 hover:text-primary hover:bg-slate-50 transition-all" aria-label="Notifications">
              <span className="material-symbols-outlined text-xl">notifications</span>
            </button>
            <button className="p-2 rounded-xl text-slate-500 hover:text-primary hover:bg-slate-50 transition-all" aria-label="Help">
              <span className="material-symbols-outlined text-xl">help_outline</span>
            </button>
            <Link
              to="/profile"
              className="w-8 h-8 rounded-xl bg-gradient-to-br from-emerald-500 to-teal-600 text-white text-xs font-bold flex items-center justify-center ml-1 hover:shadow-md transition-all"
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
