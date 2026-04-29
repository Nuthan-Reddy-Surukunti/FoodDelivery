import PropTypes from 'prop-types'
import { useLocation, Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { useCart } from '../../context/CartContext'
import { AiChatWidget } from './AiChatWidget'

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
  const isProfileRelated = ['/profile', '/change-password', '/delete-account'].includes(location.pathname)
  const isNotCustomer = user?.role && !['Customer', 'customer'].includes(user.role)

  const useCustomLayout = isAdminPage || isPartnerPage || isAgentPage || (isProfileRelated && isNotCustomer)

  if (isAuthPage || useCustomLayout) {
    return (
      <div className="min-h-screen bg-background text-on-background">
        {children}
      </div>
    )
  }

  const isCustomer = user?.role === 'Customer' || user?.role === 'customer' || (!user?.role && isAuthenticated)
  const path = location.pathname

  const isHome    = path === '/'
  const isExplore = path === '/explore' || path.startsWith('/restaurant') || path.startsWith('/search')
  const isOrders  = path === '/orders' || path.startsWith('/track')

  const navLinkClass = (active) =>
    `relative font-semibold text-sm transition-all duration-200 pb-0.5 ${
      active
        ? 'text-primary after:absolute after:bottom-0 after:left-0 after:right-0 after:h-0.5 after:bg-primary after:rounded-full'
        : 'text-slate-600 hover:text-primary'
    }`

  return (
    <div className="min-h-screen bg-background text-on-background flex flex-col">
      {/* ── TopNavBar ── */}
      <nav className="bg-white/95 backdrop-blur-md sticky top-0 w-full z-50 border-b border-slate-100 shadow-sm">
        <div className="flex justify-between items-center px-6 h-16 w-full max-w-7xl mx-auto">

          {/* Brand */}
          <Link to="/" className="flex items-center gap-2.5 group" aria-label="QuickBite home">
            <div className="w-8 h-8 bg-gradient-to-br from-primary to-indigo-600 rounded-xl flex items-center justify-center shadow-sm group-hover:scale-110 transition-transform">
              <span className="text-base">🍔</span>
            </div>
            <span className="text-lg font-extrabold tracking-tight text-slate-900">
              Quick<span className="text-primary">Bite</span>
            </span>
          </Link>

          {/* Center Nav Links */}
          <div className="hidden md:flex items-center gap-8">
            <Link to="/"        className={navLinkClass(isHome)}>Home</Link>
            <Link to="/explore" className={navLinkClass(isExplore)}>Explore</Link>
            <Link to="/orders"  className={navLinkClass(isOrders)}>Orders</Link>
          </div>

          {/* Right Actions */}
          <div className="flex items-center gap-2">
            {isCustomer && (
              <Link
                to="/cart"
                className="relative p-2.5 rounded-xl hover:bg-slate-100 text-slate-700 hover:text-primary transition-all"
                aria-label="Cart"
              >
                <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: "'FILL' 0" }}>shopping_cart</span>
                {totalItems > 0 && (
                  <span className="absolute -top-0.5 -right-0.5 bg-primary text-white text-[10px] font-bold w-5 h-5 rounded-full flex items-center justify-center shadow-sm">
                    {totalItems}
                  </span>
                )}
              </Link>
            )}

            {isAuthenticated ? (
              <>
                <button className="p-2.5 rounded-xl hover:bg-slate-100 text-slate-700 hover:text-primary transition-all" aria-label="Notifications">
                  <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: "'FILL' 0" }}>notifications</span>
                </button>
                <Link
                  to="/profile"
                  className="w-9 h-9 rounded-xl bg-gradient-to-br from-primary to-indigo-600 text-white text-sm font-bold flex items-center justify-center shadow-sm hover:shadow-md hover:scale-105 transition-all"
                  aria-label="Profile"
                >
                  {(user?.name?.[0] || user?.email?.[0] || 'U').toUpperCase()}
                </Link>
                <button
                  onClick={() => { logout(); navigate('/login') }}
                  className="p-2.5 rounded-xl text-slate-500 hover:text-rose-600 hover:bg-rose-50 transition-all"
                  aria-label="Logout"
                >
                  <span className="material-symbols-outlined text-xl">power_settings_new</span>
                </button>
              </>
            ) : (
              <>
                <Link to="/login" className="text-sm font-semibold text-slate-600 hover:text-primary transition-colors px-3 py-2 rounded-lg hover:bg-slate-50">
                  Login
                </Link>
                <Link to="/register" className="bg-gradient-to-r from-primary to-indigo-600 text-white rounded-xl px-5 py-2 text-sm font-bold hover:shadow-lg hover:shadow-primary/20 transition-all active:scale-95">
                  Sign Up
                </Link>
              </>
            )}
          </div>
        </div>
      </nav>

      <main className="flex-1">
        {children}
      </main>

      {/* AI Chat Widget */}
      {(isCustomer || (!isAuthenticated && !isAuthPage)) && (
        <AiChatWidget />
      )}
    </div>
  )
}

Layout.propTypes = {
  children: PropTypes.node,
}
