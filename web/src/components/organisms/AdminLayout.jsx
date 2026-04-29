import { Link, useLocation, useNavigate } from 'react-router-dom'
import PropTypes from 'prop-types'
import { useAuth } from '../../context/AuthContext'

const NAV_ITEMS = [
  { to: '/admin', icon: 'dashboard', label: 'Overview' },
  { to: '/admin/restaurants', icon: 'restaurant', label: 'Restaurants' },
  { to: '/admin/orders', icon: 'shopping_cart', label: 'Orders' },
  { to: '/admin/users', icon: 'group', label: 'Users' },
  { to: '/profile', icon: 'person', label: 'Profile' },
]

export const AdminLayout = ({ children, title = 'Admin', searchPlaceholder = '' }) => {
  const location = useLocation()
  const navigate = useNavigate()
  const { user, logout } = useAuth()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="bg-background text-on-background h-screen w-full overflow-hidden flex font-sans">
      {/* ── Fixed Left Sidebar ── */}
      <aside className="hidden md:flex bg-white h-screen w-64 border-r border-slate-200 shadow-sm flex-col p-4 z-20 shrink-0">
        {/* Logo */}
        <div className="mb-8 px-4 flex flex-col gap-1">
          <div>
            <h1 className="text-xl font-bold tracking-tight text-slate-900">FoodDash Admin</h1>
            <p className="text-xs font-medium text-slate-500">Enterprise Control</p>
          </div>
        </div>

        {/* Nav */}
        <nav className="flex-1 flex flex-col gap-2">
          {NAV_ITEMS.map(({ to, icon, label }) => {
            const isActive = location.pathname === to || (to !== '/admin' && location.pathname.startsWith(to))
            return (
              <Link
                key={to}
                to={to}
                className={`flex items-center gap-3 px-4 py-3 rounded-lg text-sm transition-all duration-200 ease-in-out ${isActive ? 'bg-primary/10 text-primary font-semibold' : 'text-slate-600 font-medium hover:bg-slate-50 hover:text-slate-900'}`}
              >
                <span className="material-symbols-outlined" style={{ fontVariationSettings: isActive ? "'FILL' 1" : "'FILL' 0" }}>
                  {icon}
                </span>
                {label}
              </Link>
            )
          })}
        </nav>

        <div className="mt-auto">
          <button className="w-full bg-primary text-on-primary py-3 rounded-lg text-sm font-medium hover:bg-primary-container transition-colors shadow-sm mb-2">
            Generate Reports
          </button>
          <button
            onClick={handleLogout}
            className="w-full flex items-center justify-center gap-2 px-3 py-2 text-sm text-slate-500 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
          >
            <span className="material-symbols-outlined">logout</span>
            Sign Out
          </button>
        </div>
      </aside>

      {/* ── Main content ── */}
      <main className="flex-1 flex flex-col h-full overflow-hidden relative">
        {/* Top header bar */}
        <header className="bg-white/95 backdrop-blur-md w-full border-b border-slate-200 flex justify-between items-center px-8 h-16 sticky top-0 z-40">
          <div className="flex items-center gap-4">
            {/* Mobile menu icon */}
            <div className="md:hidden">
              <span className="material-symbols-outlined text-slate-600 cursor-pointer">menu</span>
            </div>
            <h2 className="text-lg font-bold text-slate-900">{title || 'Dashboard'}</h2>
          </div>

          <div className="flex items-center gap-6">
            {searchPlaceholder && (
              <div className="relative hidden md:block">
                <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-[20px]">search</span>
                <input type="text" placeholder={searchPlaceholder} className="pl-10 pr-4 py-2 bg-slate-100 rounded-full border-none focus:ring-2 focus:ring-primary text-sm w-64 placeholder:text-slate-400 outline-none" />
              </div>
            )}
            <div className="flex items-center gap-4 text-slate-500">
              <button className="hover:text-primary transition-colors cursor-pointer active:opacity-70" aria-label="Notifications">
                <span className="material-symbols-outlined">notifications</span>
              </button>
              <button className="hover:text-primary transition-colors cursor-pointer active:opacity-70" aria-label="Settings">
                <span className="material-symbols-outlined">settings</span>
              </button>
              <button className="hover:text-primary transition-colors cursor-pointer active:opacity-70" aria-label="Help">
                <span className="material-symbols-outlined">help</span>
              </button>
              <Link 
                to="/profile" 
                className="h-8 w-8 rounded-full bg-slate-100 border border-slate-200 text-slate-700 flex items-center justify-center text-sm font-semibold ml-2 hover:border-primary transition-colors"
                aria-label="View Profile"
              >
                {(user?.email?.[0] || 'A').toUpperCase()}
              </Link>
            </div>
          </div>
        </header>

        {/* Scrollable content */}
        <div className="flex-1 overflow-y-auto p-6 scroll-smooth">
          <div className="max-w-7xl mx-auto space-y-6 pb-24 md:pb-8">
            {/* Page title */}
            {title && (
              <div>
                <h2 className="text-[28px] font-bold text-on-background tracking-tight">{title}</h2>
              </div>
            )}
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
