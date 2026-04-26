import { useState } from 'react'
import PropTypes from 'prop-types'
import { useLocation, Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { useCart } from '../../context/CartContext'

export const Layout = ({ children }) => {
  const { isAuthenticated, user, logout } = useAuth()
  const { totalItems } = useCart()
  const location = useLocation()
  const navigate = useNavigate()
  const [searchQuery, setSearchQuery] = useState('')

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

  const isHome = path === '/'
  const isExplore = path === '/explore' || path.startsWith('/restaurant') || path.startsWith('/search')
  const isOrders = path === '/orders' || path.startsWith('/track')

  const handleSearchSubmit = (e) => {
    e.preventDefault()
    const query = searchQuery.trim()
    if (!query) return
    navigate(`/search?q=${encodeURIComponent(query)}`)
  }

  return (
    <div className="min-h-screen bg-background text-on-background flex flex-col">
      {/* ── Horizon TopNavBar ── */}
      <nav className="bg-white/80 dark:bg-slate-900/80 backdrop-blur-md sticky top-0 w-full z-50 border-b border-slate-200 dark:border-slate-800 shadow-sm">
        <div className="flex justify-between items-center px-6 h-16 w-full max-w-7xl mx-auto">
          <div className="flex items-center gap-6 min-w-0">
            <Link to="/" className="text-xl font-bold tracking-tight text-slate-900 dark:text-white whitespace-nowrap">
              Horizon Food
            </Link>

            <form onSubmit={handleSearchSubmit} className="hidden md:flex relative">
              <span className="material-symbols-outlined absolute left-3 top-2 text-slate-400 text-lg">search</span>
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Search..."
                className="pl-10 pr-4 py-2 bg-slate-100 dark:bg-slate-800 border-transparent rounded-full text-sm focus:ring-2 focus:ring-primary outline-none w-64 text-slate-900 dark:text-white placeholder-slate-500"
              />
            </form>
          </div>

          {/* Center links */}
          <div className="hidden md:flex items-center gap-8">
            <Link
              to="/"
              className={`font-medium transition-colors duration-200 ${isHome ? 'text-primary border-b-2 border-primary pb-0.5' : 'text-slate-500 hover:text-primary'}`}
            >
              Home
            </Link>
            <Link
              to="/explore"
              className={`font-medium transition-colors duration-200 ${isExplore ? 'text-primary border-b-2 border-primary pb-0.5' : 'text-slate-500 hover:text-primary'}`}
            >
              Explore
            </Link>
            <Link
              to="/orders"
              className={`font-medium transition-colors duration-200 ${isOrders ? 'text-primary border-b-2 border-primary pb-0.5' : 'text-slate-500 hover:text-primary'}`}
            >
              Orders
            </Link>
          </div>

          {/* Right actions */}
          <div className="flex items-center gap-4 text-primary">
            {isCustomer && (
              <Link
                to="/cart"
                className="relative p-2 rounded-full hover:text-primary/80 transition-colors duration-200"
                aria-label="Cart"
              >
                <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: "'FILL' 0" }}>shopping_cart</span>
                {totalItems > 0 && (
                  <span className="absolute -top-0.5 -right-0.5 bg-primary text-white text-[10px] font-bold w-5 h-5 rounded-full flex items-center justify-center">
                    {totalItems}
                  </span>
                )}
              </Link>
            )}
            {isAuthenticated ? (
              <>
                <button className="p-2 rounded-full hover:text-primary/80 transition-colors duration-200" aria-label="Notifications">
                  <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: "'FILL' 0" }}>notifications</span>
                </button>
                <Link to="/profile" className="w-8 h-8 rounded-full border border-slate-200 bg-slate-100 text-slate-700 text-sm font-semibold flex items-center justify-center" aria-label="Profile">
                  {(user?.email?.[0] || 'U').toUpperCase()}
                </Link>
                <button
                  onClick={() => { logout(); navigate('/login') }}
                  className="p-2 rounded-full text-slate-500 hover:text-red-600 hover:bg-red-50 transition-colors duration-200"
                  aria-label="Logout"
                >
                  <span className="material-symbols-outlined text-xl">power_settings_new</span>
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

Layout.propTypes = {
  children: PropTypes.node,
}
