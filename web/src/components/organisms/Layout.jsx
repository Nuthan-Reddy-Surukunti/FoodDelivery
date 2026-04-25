import { useLocation } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import { useCart } from '../../context/CartContext'
import { useTheme } from '../../context/ThemeContext'
import { Header } from './Header'
import { Footer } from './Footer'
import { NavBar } from './NavBar'

export const Layout = ({ children }) => {
  const { isAuthenticated, user, logout } = useAuth()
  const { totalItems } = useCart()
  const { isDark, toggleTheme } = useTheme()
  const location = useLocation()

  // Don't show header/footer on auth pages
  const isAuthPage = 
    location.pathname === '/login' || 
    location.pathname === '/register' ||
    location.pathname === '/forgot-password' ||
    location.pathname === '/reset-password' ||
    location.pathname === '/verify-email' ||
    location.pathname === '/verify-2fa'

  return (
    <div className="flex flex-col min-h-screen bg-background text-on-background">
      {!isAuthPage ? (
        <>
          <Header
            user={user}
            onLogout={logout}
            totalItems={totalItems}
            isDark={isDark}
            onToggleTheme={toggleTheme}
          />
          <div className="border-b border-outline bg-surface">
            <div className="mx-auto flex h-12 max-w-7xl items-center px-4">
              <NavBar links={getNavLinks(isAuthenticated, user)} />
            </div>
          </div>
        </>
      ) : null}
      
      <main className="flex-1">
        {children}
      </main>

      {!isAuthPage && <Footer />}
    </div>
  )
}

const getNavLinks = (isAuthenticated, user) => {
  if (!isAuthenticated) return []

  if (user?.role === 'Admin') {
    return [
      { to: '/admin', label: 'Overview' },
      { to: '/admin/orders', label: 'Orders' },
      { to: '/admin/restaurants', label: 'Restaurants' },
      { to: '/admin/users', label: 'Users' },
    ]
  }

  if (user?.role === 'RestaurantPartner') {
    return [
      { to: '/partner/dashboard', label: 'Dashboard' },
      { to: '/partner/menu', label: 'Menu' },
    ]
  }

  if (user?.role === 'DeliveryAgent') {
    return [
      { to: '/agent/active', label: 'Active Deliveries' },
      { to: '/agent/earnings', label: 'Earnings' },
    ]
  }

  return [
    { to: '/', label: 'Home' },
    { to: '/orders', label: 'My Orders' },
  ]
}
