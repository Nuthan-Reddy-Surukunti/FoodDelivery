import { Link } from 'react-router-dom'
import { Icon } from '../atoms/Icon'
import { Badge } from '../atoms/Badge'

export const Header = ({ user, totalItems = 0, onLogout, onToggleTheme, isDark }) => {
  return (
    <header className="sticky top-0 z-40 border-b border-outline bg-surface/95 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4">
        <Link to="/" className="text-xl font-bold text-primary">CraveCloud</Link>

        <div className="flex items-center gap-3">
          <button type="button" onClick={onToggleTheme} className="rounded-lg p-2 hover:bg-surface-dim" aria-label="Toggle theme">
            <Icon name={isDark ? 'light_mode' : 'dark_mode'} size={20} />
          </button>

          {user?.role === 'Customer' ? (
            <Link to="/cart" className="relative rounded-lg p-2 hover:bg-surface-dim" aria-label="Cart">
              <Icon name="shopping_cart" size={22} />
              {totalItems > 0 ? (
                <Badge variant="error" size="sm" className="absolute -right-1 -top-1">{totalItems}</Badge>
              ) : null}
            </Link>
          ) : null}

          {user ? (
            <>
              <Link to="/profile" className="rounded-lg p-2 hover:bg-surface-dim" aria-label="Profile">
                <Icon name="account_circle" size={22} />
              </Link>
              <button type="button" onClick={onLogout} className="rounded-lg p-2 hover:bg-surface-dim" aria-label="Logout">
                <Icon name="logout" size={20} />
              </button>
            </>
          ) : (
            <>
              <Link className="text-sm font-medium" to="/login">Login</Link>
              <Link className="rounded-xl bg-primary px-3 py-2 text-sm font-semibold text-on-primary" to="/register">Sign Up</Link>
            </>
          )}
        </div>
      </div>
    </header>
  )
}

export default Header
