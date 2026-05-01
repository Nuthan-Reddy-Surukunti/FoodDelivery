import PropTypes from 'prop-types'
import { useLocation, Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { useCart } from '../../context/CartContext'
import { useLogoutConfirmation } from '../../hooks/useLogoutConfirmation'
import { useNotification } from '../../hooks/useNotification'
import { AiChatWidget } from './AiChatWidget'
import { NotificationCenter } from './NotificationCenter'

export const Layout = ({ children }) => {
  const { isAuthenticated, user, logout } = useAuth()
  const { totalItems } = useCart()
  const { confirmLogout } = useLogoutConfirmation()
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
  const location = useLocation()
  const navigate = useNavigate()

  const handleLogout = () => {
    confirmLogout(async () => {
      await logout()
      navigate('/login')
    })
  }

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

  const isHome = path === '/'
  const isExplore = path === '/explore' || path.startsWith('/restaurant') || path.startsWith('/search')
  const isOrders = path === '/orders' || path.startsWith('/track')
  const isAddresses = path === '/addresses'
  const isHelp = path === '/help'

  const navLinkClass = (active) =>
    `relative font-semibold text-sm transition-all duration-200 pb-0.5 ${
      active
        ? 'text-primary after:absolute after:bottom-0 after:left-0 after:right-0 after:h-0.5 after:bg-primary after:rounded-full'
        : 'text-slate-600 hover:text-primary'
    }`

  return (
    <div className="min-h-screen bg-background text-on-background flex flex-col">
      {/* ── Soft Ambient Mesh Background ── */}
      <div className="fixed inset-0 pointer-events-none z-0 overflow-hidden">
        <div className="absolute top-[-10%] left-[-10%] w-[40%] h-[40%] bg-blue-400/10 rounded-full blur-[120px]" />
        <div className="absolute top-[20%] right-[-10%] w-[35%] h-[35%] bg-purple-400/10 rounded-full blur-[120px]" />
        <div className="absolute bottom-[-10%] left-[20%] w-[40%] h-[40%] bg-emerald-400/10 rounded-full blur-[120px]" />
      </div>

      <nav className="bg-white/80 backdrop-blur-xl border-b border-slate-200 sticky top-0 z-40 shadow-sm relative">
        <div className="max-w-7xl mx-auto px-6 h-16 flex items-center justify-between">

          {/* Brand */}
          <Link to="/" className="flex items-center gap-2 group">
            <img src="/quickbite-logo.png" alt="QuickBite Logo" className="w-11 h-11 object-contain group-hover:scale-105 transition-transform" />
            <span className="text-xl font-extrabold bg-gradient-to-r from-slate-900 to-slate-700 bg-clip-text text-transparent tracking-tight">
              QuickBite
            </span>
          </Link>

          {/* Center Nav Links */}
          <div className="hidden md:flex items-center gap-8">
            <Link to="/" className={navLinkClass(isHome)}>Home</Link>
            <Link to="/explore" className={navLinkClass(isExplore)}>Explore</Link>
            <Link to="/orders" className={navLinkClass(isOrders)}>Orders</Link>
            {isCustomer && (
              <Link to="/addresses" className={navLinkClass(isAddresses)}>Addresses</Link>
            )}
            <Link to="/help" className={navLinkClass(isHelp)}>Help</Link>
          </div>

          {/* Right Actions */}
          <div className="flex items-center gap-2">
            {isCustomer && (
              <Link
                to="/cart"
                className="relative p-2.5 rounded-xl hover:bg-slate-100/80 text-slate-700 hover:text-primary transition-all"
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
                <div className="relative">
                  <button
                    type="button"
                    onClick={() => toggleCenter(user?.role)}
                    className="relative p-2.5 rounded-xl hover:bg-slate-100/80 text-slate-700 hover:text-primary transition-all"
                    aria-label="Notifications"
                  >
                    <span className="material-symbols-outlined text-xl" style={{ fontVariationSettings: "'FILL' 0" }}>notifications</span>
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
                <Link
                  to="/profile"
                  className="w-9 h-9 rounded-xl bg-gradient-to-br from-primary to-amber-400 text-white text-sm font-bold flex items-center justify-center shadow-sm hover:shadow-md hover:scale-105 transition-all"
                  aria-label="Profile"
                >
                  {(user?.name?.[0] || user?.email?.[0] || 'U').toUpperCase()}
                </Link>
                <button
                  onClick={handleLogout}
                  className="p-2.5 rounded-xl text-slate-500 hover:text-rose-600 hover:bg-rose-50/80 transition-all"
                  aria-label="Logout"
                >
                  <span className="material-symbols-outlined text-xl">logout</span>
                </button>
              </>
            ) : (
              <>
                <Link to="/login" className="text-sm font-semibold text-slate-600 hover:text-primary transition-colors px-3 py-2 rounded-lg hover:bg-slate-50/80">
                  Login
                </Link>
                <Link to="/register" className="bg-gradient-to-r from-primary to-amber-400 text-white rounded-xl px-5 py-2 text-sm font-bold hover:shadow-lg hover:shadow-primary/20 transition-all active:scale-95">
                  Sign Up
                </Link>
              </>
            )}
          </div>
        </div>
      </nav>

      <main className="flex-1 relative z-10">
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
