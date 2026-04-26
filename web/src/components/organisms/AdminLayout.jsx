import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'

const NAV_ITEMS = [
  { to: '/admin', icon: 'dashboard', label: 'Overview' },
  { to: '/admin/orders', icon: 'receipt_long', label: 'Orders' },
  { to: '/admin/restaurants', icon: 'storefront', label: 'Restaurants' },
  { to: '/admin/users', icon: 'group', label: 'Users' },
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
      <aside className="hidden md:flex bg-white h-screen w-64 border-r border-slate-200 shadow-sm flex-col py-6 z-20 shrink-0">
        {/* Logo */}
        <div className="px-6 mb-8 flex items-center gap-3">
          <div className="w-10 h-10 bg-primary rounded-xl flex items-center justify-center">
            <span className="text-white font-bold text-lg">QB</span>
          </div>
          <div>
            <h1 className="text-sm font-bold text-primary">QuickBite Admin</h1>
            <p className="text-[11px] text-slate-500">Platform Management</p>
          </div>
        </div>

        {/* Nav */}
        <nav className="flex-1 flex flex-col gap-1 px-3">
          {NAV_ITEMS.map(({ to, icon, label }) => {
            const isActive = location.pathname === to || (to !== '/admin' && location.pathname.startsWith(to))
            return (
              <Link
                key={to}
                to={to}
                className={`flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all duration-150 ${isActive ? 'bg-blue-50 text-primary' : 'text-slate-500 hover:text-primary hover:bg-slate-50'}`}
              >
                <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: isActive ? "'FILL' 1" : "'FILL' 0" }}>
                  {icon}
                </span>
                {label}
              </Link>
            )
          })}
        </nav>

        {/* User + Logout */}
        <div className="px-3 mt-6 border-t border-slate-200 pt-4">
          <div className="flex items-center gap-3 px-3 py-2 mb-2">
            <div className="w-8 h-8 rounded-full bg-primary text-white flex items-center justify-center text-sm font-semibold">
              {(user?.email?.[0] || 'A').toUpperCase()}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-on-surface truncate">{user?.email || 'Admin'}</p>
              <p className="text-xs text-slate-400">Admin</p>
            </div>
          </div>
          <button
            onClick={handleLogout}
            className="w-full flex items-center gap-3 px-3 py-2 text-sm text-slate-500 hover:text-red-600 hover:bg-red-50 rounded-xl transition-colors"
          >
            <span className="material-symbols-outlined text-xl">logout</span>
            Sign Out
          </button>
        </div>
      </aside>

      {/* ── Main content ── */}
      <main className="flex-1 flex flex-col h-full overflow-hidden relative">
        {/* Top header bar */}
        <header className="bg-white/80 backdrop-blur-md w-full border-b border-slate-200 flex justify-between items-center px-6 h-16 sticky top-0 z-40">
          <div className="flex items-center gap-4">
            {/* Mobile menu icon */}
            <div className="md:hidden">
              <span className="material-symbols-outlined text-slate-600 cursor-pointer">menu</span>
            </div>
            <h2 className="text-lg font-bold text-primary tracking-tight">QuickBite Admin</h2>
          </div>

          <div className="flex items-center gap-4">
            {searchPlaceholder && (
              <div className="hidden sm:flex items-center bg-slate-50 rounded-xl px-3 py-2 border border-slate-200 gap-2 w-56">
                <span className="material-symbols-outlined text-slate-400 text-lg">search</span>
                <input type="text" placeholder={searchPlaceholder} className="bg-transparent text-sm text-on-surface placeholder:text-slate-400 outline-none flex-1" />
              </div>
            )}
            <div className="flex items-center gap-2 text-slate-500">
              <span className="material-symbols-outlined cursor-pointer hover:text-primary transition-colors">notifications</span>
              <span className="material-symbols-outlined cursor-pointer hover:text-primary transition-colors">settings</span>
            </div>
            <div className="w-8 h-8 rounded-full bg-primary text-white flex items-center justify-center text-sm font-semibold">
              {(user?.email?.[0] || 'A').toUpperCase()}
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
