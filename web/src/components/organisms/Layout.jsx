import { useLocation, Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { useCart } from '../../context/CartContext'

export const Layout = ({ children }) => {
  const { isAuthenticated, user, logout } = useAuth()
  const { totalItems } = useCart()
  const location = useLocation()
  const navigate = useNavigate()

  const isAuthPage =
    ['/login', '/register', '/forgot-password', '/reset-password', '/verify-email', '/verify-2fa']
      .includes(location.pathname)

  const isAdminPage = location.pathname.startsWith('/admin')
  const isPartnerPage = location.pathname.startsWith('/partner')
  const isAgentPage = location.pathname.startsWith('/agent')

  // Admin and Partner use their own built-in sidebar layouts
  // Agent uses mobile shell with bottom nav
  // These pages should NOT have the global top nav
  const useCustomLayout = isAdminPage || isPartnerPage || isAgentPage

  if (isAuthPage || useCustomLayout) {
    return (
      <div className="min-h-screen bg-background text-on-background">
        {children}
      </div>
    )
  }

  // Customer top nav
  const isCustomer = user?.role === 'Customer' || user?.role === 'customer' || (!user?.role && isAuthenticated)
  const path = location.pathname

  return (
    <div className="min-h-screen bg-background text-on-background flex flex-col">
      {/* ── Horizon TopNavBar ── */}
      <nav className="bg-white/80 dark:bg-slate-900/80 backdrop-blur-md sticky top-0 w-full z-50 border-b border-slate-200 dark:border-slate-800">
        <div className="flex justify-between items-center px-6 h-16 w-full max-w-7xl mx-auto">
          {/* Logo */}
          <Link to="/" className="text-xl font-bold tracking-tight text-on-surface">
            QuickBite
          </Link>

          {/* Center links */}
          <div className="hidden md:flex items-center gap-6 text-sm">
            <Link
              to="/"
              className={`font-medium transition-colors duration-200 ${path === '/' ? 'text-primary border-b-2 border-primary pb-0.5' : 'text-slate-500 hover:text-primary'}`}
            >
              Home
            </Link>
            <Link
              to="/explore"
              className={`font-medium transition-colors duration-200 ${path === '/explore' ? 'text-primary border-b-2 border-primary pb-0.5' : 'text-slate-500 hover:text-primary'}`}
            >
              Explore
            </Link>
            <Link
              to="/orders"
              className={`font-medium transition-colors duration-200 ${path === '/orders' ? 'text-primary border-b-2 border-primary pb-0.5' : 'text-slate-500 hover:text-primary'}`}
            >
              Orders
            </Link>
          </div>

          {/* Right actions */}
          <div className="flex items-center gap-2 text-on-surface">
            {isCustomer && (
              <Link
                to="/cart"
                className="relative p-2 rounded-full hover:bg-slate-100 transition-colors"
                aria-label="Cart"
              >
                <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: "'FILL' 0" }}>shopping_cart</span>
                {totalItems > 0 && (
                  <span className="absolute -top-0.5 -right-0.5 bg-primary text-white text-xs font-bold w-5 h-5 rounded-full flex items-center justify-center">
                    {totalItems}
                  </span>
                )}
              </Link>
            )}
            {isAuthenticated ? (
              <>
                <Link to="/profile" className="p-2 rounded-full hover:bg-slate-100 transition-colors" aria-label="Profile">
                  <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: "'FILL' 0" }}>account_circle</span>
                </Link>
                <button
                  onClick={() => { logout(); navigate('/login') }}
                  className="p-2 rounded-full hover:bg-slate-100 transition-colors text-slate-500"
                  aria-label="Logout"
                >
                  <span className="material-symbols-outlined text-xl">logout</span>
                </button>
              </>
            ) : (
              <>
                <Link to="/login" className="text-sm font-medium text-slate-600 hover:text-primary transition-colors px-3 py-2">Login</Link>
                <Link to="/register" className="bg-primary text-on-primary rounded-full px-5 py-2 text-sm font-semibold hover:bg-primary-container transition-colors">Sign Up</Link>
              </>
            )}
          </div>
        </div>
      </nav>

      <main className="flex-1">
        {children}
      </main>
    </div>
  )
}
