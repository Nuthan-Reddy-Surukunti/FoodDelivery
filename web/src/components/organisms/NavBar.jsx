import { Link, useLocation } from 'react-router-dom'

export const NavBar = ({ links = [] }) => {
  const location = useLocation()

  return (
    <nav className="hidden md:flex items-center gap-2">
      {links.map((link) => {
        const active = location.pathname === link.to
        return (
          <Link
            key={link.to}
            to={link.to}
            className={`rounded-lg px-3 py-2 text-sm font-medium transition ${active ? 'bg-primary text-on-primary' : 'text-on-background hover:bg-surface-dim'}`}
          >
            {link.label}
          </Link>
        )
      })}
    </nav>
  )
}

export default NavBar
