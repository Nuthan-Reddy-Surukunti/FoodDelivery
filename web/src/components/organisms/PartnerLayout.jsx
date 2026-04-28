import { Link, useLocation, useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'
import { useAuth } from '../../context/AuthContext'

const NAV_ITEMS = [
  { to: '/partner/dashboard', icon: 'dashboard', label: 'Dashboard' },
  { to: '/partner/queue', icon: 'receipt_long', label: 'Orders' },
  { to: '/partner/menu', icon: 'restaurant_menu', label: 'Menu' },
  { to: '/partner/analytics', icon: 'analytics', label: 'Analytics' },
  { to: '/profile', icon: 'person', label: 'Profile' },
]

const PLACEHOLDER_ITEMS = []

export const PartnerLayout = ({ children, title = '' }) => {
  const location = useLocation()
  const navigate = useNavigate()
  const { user, logout } = useAuth()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="bg-background text-on-background h-screen w-full overflow-hidden flex font-sans">
      {/* ── Left sidebar ── */}
      <aside className="hidden md:flex bg-white dark:bg-slate-900 h-screen w-64 border-r border-slate-200 dark:border-slate-800 shadow-sm flex-col py-6 z-20 shrink-0">
        {/* Logo */}
        <div className="px-6 mb-8 flex items-center gap-3">
          <div className="w-10 h-10 bg-primary rounded-xl flex items-center justify-center">
            <span className="material-symbols-outlined text-white text-xl">storefront</span>
          </div>
          <div>
            <h1 className="text-sm font-bold text-primary">QuickBite Partner</h1>
            <p className="text-[11px] text-slate-500">Restaurant Dashboard</p>
          </div>
        </div>

        {/* Nav */}
        <nav className="flex-1 flex flex-col gap-2 px-3">
          {NAV_ITEMS.map(({ to, icon, label }) => {
            const isActive = location.pathname === to
            return (
              <Link
                key={to}
                to={to}
                className={`flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-all duration-150 ${isActive ? 'text-blue-600 dark:text-blue-400 font-semibold border-r-2 border-blue-600 bg-blue-50/50 dark:bg-blue-900/10 opacity-80' : 'text-slate-500 dark:text-slate-400 hover:text-blue-600 dark:hover:text-blue-300 hover:bg-slate-50 dark:hover:bg-slate-800'}`}
              >
                <span className="material-symbols-outlined" style={{ fontVariationSettings: isActive ? "'FILL' 1" : "'FILL' 0" }}>
                  {icon}
                </span>
                {label}
              </Link>
            )
          })}

          {PLACEHOLDER_ITEMS.map(({ icon, label }) => (
            <button
              key={label}
              type="button"
              aria-disabled="true"
              title="Coming soon"
              className="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-slate-500 dark:text-slate-400 hover:text-blue-600 dark:hover:text-blue-300 hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors duration-200"
            >
              <span className="material-symbols-outlined">{icon}</span>
              {label}
            </button>
          ))}

          <button
            type="button"
            aria-disabled="true"
            title="Coming soon"
            className="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-slate-500 dark:text-slate-400 hover:text-blue-600 dark:hover:text-blue-300 hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors duration-200 mt-auto"
          >
            <span className="material-symbols-outlined">settings</span>
            Settings
          </button>
        </nav>

        <div className="px-3 mt-6 border-t border-slate-200 pt-4">
          <button
            onClick={handleLogout}
            className="w-full flex items-center gap-3 px-3 py-2 text-sm text-slate-500 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
          >
            <span className="material-symbols-outlined">logout</span>
            Sign Out
          </button>
        </div>
      </aside>

      {/* ── Main content ── */}
      <main className="flex-1 flex flex-col h-full overflow-hidden relative">
        {/* Top bar */}
        <header className="bg-white/80 dark:bg-slate-950/80 backdrop-blur-md w-full border-b border-slate-200 dark:border-slate-800 flex justify-between items-center px-6 py-3 h-16 sticky top-0 z-40">
          <div className="flex items-center gap-4">
            <div className="md:hidden">
              <span className="material-symbols-outlined text-slate-600 cursor-pointer hover:text-blue-500 transition-colors">menu</span>
            </div>
            <h2 className="text-lg font-black text-blue-700 dark:text-blue-400 tracking-tight">QuickBite</h2>
          </div>
          <div className="flex items-center gap-4">
            <div className="hidden sm:flex items-center bg-slate-100 rounded-full p-1 border border-slate-200/70">
              <button className="px-4 py-1.5 rounded-full bg-primary text-white text-sm font-medium shadow-sm transition-transform active:scale-95 duration-100">Go Online</button>
              <button className="px-4 py-1.5 rounded-full text-slate-500 hover:text-primary transition-colors text-sm font-medium">Go Offline</button>
            </div>
            <div className="flex items-center gap-2 text-slate-600 dark:text-slate-400">
              <span className="material-symbols-outlined hover:text-blue-500 dark:hover:text-blue-300 cursor-pointer">notifications</span>
              <span className="material-symbols-outlined hover:text-blue-500 dark:hover:text-blue-300 cursor-pointer">help_outline</span>
            </div>
            <Link 
              to="/profile" 
              className="w-8 h-8 rounded-full border border-slate-200 bg-slate-100 text-slate-700 flex items-center justify-center text-sm font-semibold hover:border-primary transition-colors"
              aria-label="View Profile"
            >
              {(user?.email?.[0] || 'P').toUpperCase()}
            </Link>
          </div>
        </header>

        {/* Scrollable content area */}
        <div className="flex-1 overflow-y-auto p-6 scroll-smooth">
          <div className="max-w-7xl mx-auto space-y-6 pb-24 md:pb-8">
            {title && <h2 className="text-[28px] font-bold text-on-background tracking-tight">{title}</h2>}
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
              className={`flex flex-col items-center justify-center rounded-xl px-3 py-1.5 tap-highlight-transparent transition-transform active:scale-90 ${isActive ? 'text-primary' : 'text-slate-400'}`}
            >
              <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: isActive ? "'FILL' 1" : "'FILL' 0" }}>{icon}</span>
              <span className="text-[11px] font-medium mt-0.5">{label}</span>
            </Link>
          )
        })}

        <button
          type="button"
          aria-disabled="true"
          title="Coming soon"
          className="flex flex-col items-center justify-center rounded-xl px-3 py-1.5 tap-highlight-transparent transition-transform active:scale-90 text-slate-400"
        >
          <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: "'FILL' 0" }}>bar_chart</span>
          <span className="text-[11px] font-medium mt-0.5">Analytics</span>
        </button>
      </nav>
    </div>
  )
}

PartnerLayout.propTypes = {
  children: PropTypes.node,
  title: PropTypes.string,
}
