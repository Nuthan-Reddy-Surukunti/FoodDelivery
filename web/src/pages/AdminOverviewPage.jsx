import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Card } from '../components/atoms/Card'
import adminApi from '../services/adminApi'

const KPI_CONFIG = [
  { key: 'totalOrdersToday', label: "Today's Orders", icon: '📦', fallbackKeys: ['ordersToday', 'totalOrders'] },
  { key: 'totalActiveRestaurants', label: 'Active Restaurants', icon: '🍽️', fallbackKeys: ['activePartners', 'totalRestaurants'] },
  { key: 'pendingApprovals', label: 'Pending Approvals', icon: '⏳', fallbackKeys: ['pendingRestaurants'] },
  { key: 'totalRevenue', label: 'Total Revenue', icon: '💰', fallbackKeys: ['totalGmv', 'gmv'], prefix: '₹' },
]

function getKpiValue(kpis, config) {
  const keys = [config.key, ...(config.fallbackKeys || [])]
  for (const k of keys) {
    if (kpis[k] !== undefined && kpis[k] !== null) {
      const val = Number(kpis[k])
      return config.prefix ? `${config.prefix}${val.toLocaleString()}` : String(val)
    }
  }
  return '—'
}

const NAV_LINKS = [
  { to: '/admin/orders', icon: '📋', label: 'Orders', desc: 'View and manage all platform orders' },
  { to: '/admin/restaurants', icon: '🏪', label: 'Restaurants', desc: 'Approve, reject, and manage restaurants' },
  { to: '/admin/users', icon: '👥', label: 'Users & Analytics', desc: 'Platform user analytics and role breakdown' },
]

export const AdminOverviewPage = () => {
  const [kpis, setKpis] = useState(null)
  const [sales, setSales] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    const load = async () => {
      setLoading(true)
      setError('')
      try {
        const [kpiRes, salesRes] = await Promise.all([
          adminApi.getDashboardKpis(),
          adminApi.getSalesReport().catch(() => null),
        ])
        if (!active) return
        setKpis(kpiRes)
        setSales(salesRes)
      } catch (err) {
        if (!active) return
        setError(err.response?.data?.message || err.message || 'Failed to load dashboard')
      } finally {
        if (active) setLoading(false)
      }
    }
    load()
    return () => { active = false }
  }, [])

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-bold">Admin Overview</h1>

      {loading && <p className="text-sm text-on-background/70 mb-6">Loading dashboard...</p>}
      {error && <p className="mb-4 text-sm text-error">{error}</p>}

      {/* KPI Cards */}
      {kpis && (
        <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {KPI_CONFIG.map(cfg => (
            <Card key={cfg.key} className="p-4">
              <div className="flex items-center gap-2 mb-2">
                <span className="text-2xl">{cfg.icon}</span>
                <p className="text-xs text-on-background/60">{cfg.label}</p>
              </div>
              <p className="text-2xl font-bold">{getKpiValue(kpis, cfg)}</p>
            </Card>
          ))}
        </div>
      )}

      {/* Revenue snapshot */}
      {(sales || kpis) && (
        <Card className="mb-6 p-5">
          <h2 className="mb-4 text-lg font-semibold">Revenue Snapshot</h2>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <div>
              <p className="text-xs text-on-background/60">Total Revenue</p>
              <p className="text-xl font-bold mt-1">₹{Number(sales?.totalRevenue ?? kpis?.totalRevenue ?? 0).toLocaleString()}</p>
            </div>
            <div>
              <p className="text-xs text-on-background/60">Avg Order Value</p>
              <p className="text-xl font-bold mt-1">₹{Number(sales?.averageOrderValue ?? kpis?.avgOrderValue ?? 0).toLocaleString()}</p>
            </div>
            <div>
              <p className="text-xs text-on-background/60">Total Orders</p>
              <p className="text-xl font-bold mt-1">{Number(sales?.totalOrders ?? kpis?.totalOrders ?? 0).toLocaleString()}</p>
            </div>
          </div>
        </Card>
      )}

      {/* Navigation */}
      <h2 className="mb-3 text-lg font-semibold">Admin Sections</h2>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        {NAV_LINKS.map(link => (
          <Link
            key={link.to}
            to={link.to}
            className="block rounded-2xl border border-outline bg-surface p-5 transition hover:border-primary hover:shadow-md"
          >
            <div className="text-3xl mb-2">{link.icon}</div>
            <p className="font-semibold">{link.label}</p>
            <p className="mt-1 text-xs text-on-background/60">{link.desc}</p>
          </Link>
        ))}
      </div>
    </div>
  )
}
