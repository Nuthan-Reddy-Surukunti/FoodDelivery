import { Link, useLocation } from 'react-router-dom'
import { Button } from '../atoms/Button'
import { Icon } from '../atoms/Icon'
import { Badge } from '../atoms/Badge'
import { useAuth } from '../../context/AuthContext'
import { useCart } from '../../context/CartContext'
import { useTheme } from '../../context/ThemeContext'

export const Layout = ({ children }) => {
  const { isAuthenticated, user, logout } = useAuth()
  const { totalItems } = useCart()
  const { isDark, toggleTheme } = useTheme()
  const location = useLocation()

  // Don't show header/footer on auth pages
  const isAuthPage = location.pathname === '/login' || location.pathname === '/register'

  return (
    <div className="flex flex-col min-h-screen bg-background text-on-background">
      {!isAuthPage && <Header isAuthenticated={isAuthenticated} user={user} logout={logout} totalItems={totalItems} isDark={isDark} toggleTheme={toggleTheme} />}
      
      <main className="flex-1">
        {children}
      </main>

      {!isAuthPage && <Footer />}
    </div>
  )
}

// Header Component
const Header = ({ isAuthenticated, user, logout, totalItems, isDark, toggleTheme }) => {
  const location = useLocation()

  return (
    <header className="bg-surface border-b border-outline sticky top-0 z-40 shadow-sm">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2">
            <div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center text-on-primary font-bold">
              🍔
            </div>
            <span className="hidden sm:inline text-headline-md font-bold text-primary">CraveCloud</span>
          </Link>

          {/* Navigation */}
          <nav className="hidden md:flex items-center gap-1">
            {isAuthenticated && (
              <>
                <NavLink to="/" label="Home" active={location.pathname === '/'} />
                <NavLink to="/orders" label="Orders" active={location.pathname === '/orders'} />
              </>
            )}
          </nav>

          {/* Right Section */}
          <div className="flex items-center gap-4">
            {/* Theme Toggle */}
            <button
              onClick={toggleTheme}
              className="p-2 rounded-lg hover:bg-surface-dim transition-colors"
              title="Toggle theme"
            >
              <Icon name={isDark ? 'light_mode' : 'dark_mode'} size={20} />
            </button>

            {/* Cart Icon (Customer only) */}
            {isAuthenticated && user?.role === 'customer' && (
              <Link
                to="/cart"
                className="relative p-2 rounded-lg hover:bg-surface-dim transition-colors"
              >
                <Icon name="shopping_cart" size={24} />
                {totalItems > 0 && (
                  <Badge
                    variant="error"
                    size="sm"
                    className="absolute -top-1 -right-1 w-5 h-5 text-xs"
                  >
                    {totalItems}
                  </Badge>
                )}
              </Link>
            )}

            {/* Auth Section */}
            {isAuthenticated ? (
              <div className="flex items-center gap-3">
                <span className="hidden sm:inline text-body-md text-on-background">
                  {user?.name || 'User'}
                </span>
                <button
                  onClick={logout}
                  className="p-2 rounded-lg hover:bg-surface-dim transition-colors"
                >
                  <Icon name="logout" size={20} />
                </button>
              </div>
            ) : (
              <div className="flex gap-2">
                <Link to="/login">
                  <Button variant="secondary" size="sm">
                    Login
                  </Button>
                </Link>
                <Link to="/register">
                  <Button variant="primary" size="sm">
                    Sign Up
                  </Button>
                </Link>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  )
}

// NavLink Helper
const NavLink = ({ to, label, active }) => {
  return (
    <Link
      to={to}
      className={`px-4 py-2 rounded-lg transition-colors text-body-md font-medium ${
        active
          ? 'bg-primary text-on-primary'
          : 'text-on-background hover:bg-surface-dim'
      }`}
    >
      {label}
    </Link>
  )
}

// Footer Component
const Footer = () => {
  return (
    <footer className="bg-surface border-t border-outline mt-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8 mb-8">
          {/* About */}
          <div>
            <h3 className="text-title-lg font-bold mb-4 text-primary">CraveCloud</h3>
            <p className="text-body-md text-on-background/80">
              Your favorite food delivery and restaurant aggregation platform.
            </p>
          </div>

          {/* Quick Links */}
          <div>
            <h4 className="text-title-lg font-bold mb-4">Quick Links</h4>
            <ul className="space-y-2">
              <li>
                <Link to="/" className="text-body-md text-on-background/80 hover:text-primary">
                  Home
                </Link>
              </li>
              <li>
                <Link to="/orders" className="text-body-md text-on-background/80 hover:text-primary">
                  My Orders
                </Link>
              </li>
              <li>
                <a href="#" className="text-body-md text-on-background/80 hover:text-primary">
                  About Us
                </a>
              </li>
            </ul>
          </div>

          {/* Support */}
          <div>
            <h4 className="text-title-lg font-bold mb-4">Support</h4>
            <ul className="space-y-2">
              <li>
                <a href="#" className="text-body-md text-on-background/80 hover:text-primary">
                  Contact Us
                </a>
              </li>
              <li>
                <a href="#" className="text-body-md text-on-background/80 hover:text-primary">
                  FAQ
                </a>
              </li>
              <li>
                <a href="#" className="text-body-md text-on-background/80 hover:text-primary">
                  Terms & Conditions
                </a>
              </li>
            </ul>
          </div>

          {/* Legal */}
          <div>
            <h4 className="text-title-lg font-bold mb-4">Legal</h4>
            <ul className="space-y-2">
              <li>
                <a href="#" className="text-body-md text-on-background/80 hover:text-primary">
                  Privacy Policy
                </a>
              </li>
              <li>
                <a href="#" className="text-body-md text-on-background/80 hover:text-primary">
                  Refund Policy
                </a>
              </li>
              <li>
                <a href="#" className="text-body-md text-on-background/80 hover:text-primary">
                  Shipping Info
                </a>
              </li>
            </ul>
          </div>
        </div>

        {/* Copyright */}
        <div className="border-t border-outline pt-8">
          <p className="text-center text-body-md text-on-background/60">
            © 2024 CraveCloud. All rights reserved.
          </p>
        </div>
      </div>
    </footer>
  )
}
