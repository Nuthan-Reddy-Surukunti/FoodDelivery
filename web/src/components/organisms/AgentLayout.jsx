import { Link, useLocation, useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'
import { useAuth } from '../../context/AuthContext'
import { getHybridGreeting } from '../../utils/greetingUtils'

const NAV_ITEMS = [
  { to: '/agent/active',   icon: 'local_shipping', label: 'Deliveries', color: 'text-sky-400' },
  { to: '/agent/earnings', icon: 'payments',       label: 'Earnings',   color: 'text-emerald-400' },
  { to: '/profile',        icon: 'person',         label: 'Profile',    color: 'text-slate-400' },
]

const PLACEHOLDER_ITEMS = [
  { icon: 'map', label: 'Map (Soon)' },
]

export const AgentLayout = ({ children, title = '' }) => {
  const location = useLocation()
  const navigate = useNavigate()
  const { user, logout } = useAuth()
  const greeting = getHybridGreeting(user?.role, user?.name || user?.email)

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="bg-slate-50 text-slate-900 h-screen w-full overflow-hidden flex font-sans">
      {/* ── Dark Teal Sidebar ── */}
      <aside className="hidden md:flex bg-[#0a1f2e] h-screen w-64 flex-col shrink-0 z-20 relative">
        {/* Subtle gradient */}
        <div className="absolute inset-0 bg-gradient-to-b from-[#0d2d40]/50 to-transparent pointer-events-none" />

        {/* Logo */}
        <div className="px-6 py-6 border-b border-white/5 relative z-10">
          <div className="flex items-center gap-3">
            <img src="/quickbite-logo-white.png" alt="QuickBite Logo" className="w-9 h-9 object-contain logo-glow-white" />
            <div>
              <p className="text-sm font-extrabold text-white tracking-tight">QuickBite</p>
              <p className="text-[11px] text-sky-400/70 font-medium">Agent Portal</p>
            </div>
          </div>
        </div>

        {/* Nav */}
        <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto no-scrollbar relative z-10">
          <p className="text-[10px] font-bold text-slate-500 uppercase tracking-widest px-3 mb-3">Delivery Menu</p>

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
                  isActive ? 'bg-sky-500/20' : 'bg-white/5 group-hover:bg-white/10'
                }`}>
                  <span
                    className={`material-symbols-outlined text-[18px] ${isActive ? color : 'text-slate-400 group-hover:text-slate-200'}`}
                    style={{ fontVariationSettings: isActive ? "'FILL' 1" : "'FILL' 0" }}
                  >
                    {icon}
                  </span>
                </div>
                <span>{label}</span>
                {isActive && <span className="ml-auto w-1.5 h-1.5 rounded-full bg-sky-400" />}
              </Link>
            )
          })}

          {PLACEHOLDER_ITEMS.map(({ icon, label }) => (
            <button
              key={label}
              type="button"
              disabled
              title="Coming soon"
              className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm text-slate-600 cursor-not-allowed opacity-60"
            >
              <div className="w-8 h-8 rounded-lg bg-white/5 flex items-center justify-center">
                <span className="material-symbols-outlined text-[18px] text-slate-600">{icon}</span>
              </div>
              <span>{label}</span>
            </button>
          ))}
        </nav>

        {/* Bottom */}
        <div className="px-3 py-4 border-t border-white/5 relative z-10">
          <div className="flex items-center gap-3 px-3 py-2.5">
            <div className="w-8 h-8 rounded-full bg-gradient-to-br from-sky-500 to-cyan-600 flex items-center justify-center text-white text-xs font-bold flex-shrink-0">
              {(user?.email?.[0] || 'A').toUpperCase()}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xs font-semibold text-white truncate">{user?.name || user?.email || 'Agent'}</p>
              <p className="text-[10px] text-sky-400/70">Delivery Agent</p>
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
              <h1 className="text-base font-bold text-slate-900">{title || greeting.main}</h1>
              <p className="text-[11px] text-slate-400 font-medium hidden sm:block">
                {title && title !== 'Delivery Dashboard' ? 'QuickBite Agent' : greeting.sub}
              </p>
            </div>
          </div>
          <div className="flex items-center gap-2">
            {/* Online status pill */}
            <div className="flex items-center gap-1.5 bg-emerald-50 border border-emerald-200 text-emerald-700 text-xs font-semibold px-3 py-1.5 rounded-full">
              <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
              Online
            </div>
            <button className="p-2 rounded-xl text-slate-500 hover:text-primary hover:bg-slate-50 transition-all" aria-label="Help">
              <span className="material-symbols-outlined text-xl">help_outline</span>
            </button>
            <Link
              to="/profile"
              className="w-8 h-8 rounded-xl bg-gradient-to-br from-sky-500 to-cyan-600 text-white text-xs font-bold flex items-center justify-center ml-1 hover:shadow-md transition-all"
              aria-label="Profile"
            >
              {(user?.email?.[0] || 'A').toUpperCase()}
            </Link>
          </div>
        </header>

        {/* Scrollable content */}
        <div className="flex-1 overflow-y-auto bg-slate-50">
          <div className="max-w-7xl mx-auto p-6 space-y-6 pb-24 md:pb-8">
            {title && <h2 className="text-2xl font-bold text-slate-900">{title}</h2>}
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
                isActive ? 'text-sky-600' : 'text-slate-400'
              }`}
            >
              <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: isActive ? "'FILL' 1" : "'FILL' 0" }}>{icon}</span>
              <span className="text-xs font-medium mt-0.5">{label}</span>
            </Link>
          )
        })}
        {PLACEHOLDER_ITEMS.map(({ icon, label }) => (
          <button
            key={label}
            type="button"
            disabled
            className="flex flex-col items-center justify-center text-slate-300 px-3 py-1.5 rounded-xl opacity-60"
          >
            <span className="material-symbols-outlined text-xl">{icon}</span>
            <span className="text-xs font-medium mt-0.5">{label.replace(' (Soon)', '')}</span>
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
