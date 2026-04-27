import { Link, useLocation, useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'
import { useAuth } from '../../context/AuthContext'

const NAV_ITEMS = [
  { to: '/agent/active', icon: 'local_shipping', label: 'Deliveries' },
  { to: '/agent/earnings', icon: 'payments', label: 'Earnings' },
  { to: '/profile', icon: 'person', label: 'Profile' },
]

const PLACEHOLDER_ITEMS = [
  { icon: 'map', label: 'Map' },
]

export const AgentLayout = ({ children, title = '' }) => {
  const location = useLocation()
  const navigate = useNavigate()
  const { user, logout } = useAuth()

  return (
    <div className="bg-background text-on-background h-screen w-full overflow-hidden flex font-sans">
      {/* ── Left sidebar ── */}
      <aside className="hidden md:flex bg-white dark:bg-slate-900 h-screen w-64 border-r border-slate-200 dark:border-slate-800 shadow-sm flex-col py-6 z-20 shrink-0">
        <div className="px-6 mb-8 flex items-center gap-3">
          <Link 
            to="/profile" 
            className="w-8 h-8 rounded-full border border-slate-200 bg-slate-100 text-slate-700 flex items-center justify-center text-xs font-semibold hover:border-primary transition-colors"
            aria-label="View Profile"
          >
            {(user?.email?.[0] || 'A').toUpperCase()}
          </Link>
          <div>
            <h1 className="text-sm font-black text-blue-700 dark:text-blue-400 tracking-tight">QuickBite</h1>
            <p className="text-[11px] text-slate-500">Delivery Dashboard</p>
          </div>
        </div>

        <nav className="flex-1 flex flex-col gap-2 px-3">
          {NAV_ITEMS.map(({ to, icon, label }) => {
            const isActive = location.pathname === to
            return (
              <Link
                key={to}
                to={to}
                className={`flex items-center gap-3 px-3 py-2 rounded-lg text-sm transition-all ${isActive ? 'text-blue-600 dark:text-blue-400 bg-blue-50/60 dark:bg-blue-900/20 font-semibold' : 'text-slate-500 dark:text-slate-400 hover:text-primary hover:bg-slate-50 dark:hover:bg-slate-800'}`}
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
              className="flex items-center gap-3 px-3 py-2 rounded-lg text-sm text-slate-500 dark:text-slate-400 hover:text-primary hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors"
            >
              <span className="material-symbols-outlined">{icon}</span>
              {label}
            </button>
          ))}
        </nav>

        <div className="px-3 mt-6 border-t border-slate-200 pt-4">
          <button
            onClick={() => { logout(); navigate('/login') }}
            className="w-full flex items-center gap-3 px-3 py-2 text-sm text-slate-500 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
          >
            <span className="material-symbols-outlined">logout</span>
            Sign Out
          </button>
        </div>
      </aside>

      {/* ── Main content ── */}
      <main className="flex-1 flex flex-col h-full overflow-hidden">
        <header className="bg-white/80 dark:bg-slate-950/80 backdrop-blur-md w-full border-b border-slate-200 dark:border-slate-800 flex justify-between items-center px-6 py-3 h-16 sticky top-0 z-40">
          <div className="flex items-center gap-3">
            <Link 
              to="/profile" 
              className="w-8 h-8 rounded-full border border-slate-200 bg-slate-100 text-slate-700 flex items-center justify-center text-xs font-semibold hover:border-primary transition-colors"
              aria-label="View Profile"
            >
              {(user?.email?.[0] || 'A').toUpperCase()}
            </Link>
            <h2 className="text-lg font-black text-blue-700 dark:text-blue-400 tracking-tight">QuickBite</h2>
          </div>
          <div className="flex items-center gap-4">
            <button className="bg-primary text-on-primary text-sm font-medium px-4 py-2 rounded-full flex items-center gap-2 shadow-sm active:scale-95 transition-transform">
              <span className="w-2 h-2 rounded-full bg-green-400" />
              Online
            </button>
            <button className="text-slate-600 dark:text-slate-400 hover:text-blue-500 dark:hover:text-blue-300 flex items-center justify-center" aria-label="Help">
              <span className="material-symbols-outlined" style={{ fontVariationSettings: "'FILL' 0" }}>help_outline</span>
            </button>
          </div>
        </header>

        <div className="flex-1 overflow-y-auto p-6">
          <div className="max-w-7xl mx-auto space-y-6 pb-24 md:pb-8">
            {title && <h2 className="text-[28px] font-bold text-on-background tracking-tight">{title}</h2>}
            {children}
          </div>
        </div>
      </main>

      {/* Mobile bottom nav */}
      <nav className="md:hidden bg-white dark:bg-slate-900 fixed bottom-0 left-0 w-full z-50 flex justify-around items-center px-4 pb-2 pt-2 h-20 rounded-t-2xl border-t border-slate-200 dark:border-slate-800 shadow-[0_-4px_6px_-1px_rgba(0,0,0,0.1)]">
        {NAV_ITEMS.map(({ to, icon, label }) => {
          const isActive = location.pathname === to
          return (
            <Link
              key={to}
              to={to}
              className={`flex flex-col items-center justify-center rounded-xl px-3 py-1.5 transition-transform active:scale-90 w-16 ${isActive ? 'text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/30' : 'text-slate-400 dark:text-slate-500'}`}
            >
              <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: isActive ? "'FILL' 1" : "'FILL' 0" }}>{icon}</span>
              <span className="text-[11px] font-medium mt-0.5">{label}</span>
            </Link>
          )
        })}

        {PLACEHOLDER_ITEMS.map(({ icon, label }) => (
          <button
            key={label}
            type="button"
            aria-disabled="true"
            title="Coming soon"
            className="flex flex-col items-center justify-center text-slate-400 dark:text-slate-500 px-3 py-1.5 rounded-xl w-16 transition-transform active:scale-90"
          >
            <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: "'FILL' 0" }}>{icon}</span>
            <span className="text-[11px] font-medium mt-0.5">{label}</span>
          </button>
        ))}
      </nav>
    </div>
  )
}

AgentLayout.propTypes = {
  children: PropTypes.node,
  title: PropTypes.string,
}
