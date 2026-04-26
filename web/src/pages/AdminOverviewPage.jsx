import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { AdminLayout } from '../components/organisms/AdminLayout'
import adminApi from '../services/adminApi'

const KPI_CONFIG = [
  { key: 'totalOrdersToday', fallbackKeys: ['ordersToday', 'totalOrders'], label: "Today's Orders", icon: 'receipt_long', color: 'text-blue-600', bg: 'bg-blue-50' },
  { key: 'totalRevenue', fallbackKeys: ['totalGmv', 'gmv'], label: 'Total Revenue', icon: 'payments', color: 'text-emerald-600', bg: 'bg-emerald-50', prefix: '₹' },
  { key: 'totalActiveRestaurants', fallbackKeys: ['activePartners', 'totalRestaurants'], label: 'Active Restaurants', icon: 'storefront', color: 'text-purple-600', bg: 'bg-purple-50' },
  { key: 'pendingApprovals', fallbackKeys: ['pendingRestaurants'], label: 'Pending Approvals', icon: 'pending', color: 'text-amber-600', bg: 'bg-amber-50' },
]

function getKpiValue(kpis, cfg) {
  const keys = [cfg.key, ...(cfg.fallbackKeys || [])]
  for (const k of keys) {
    if (kpis?.[k] !== undefined && kpis?.[k] !== null) {
      const val = Number(kpis[k])
      return cfg.prefix ? `${cfg.prefix}${val.toLocaleString()}` : val.toLocaleString()
    }
  }
  return '—'
}

const STATUS_BADGE = {
  Delivered: 'bg-emerald-100 text-emerald-800',
  Cancelled: 'bg-red-100 text-red-800',
  Preparing: 'bg-blue-100 text-blue-800',
  OutForDelivery: 'bg-purple-100 text-purple-800',
  RestaurantAccepted: 'bg-teal-100 text-teal-800',
  Paid: 'bg-sky-100 text-sky-800',
  CheckoutStarted: 'bg-amber-100 text-amber-800',
}

const fmtDate = (iso) => iso ? new Date(iso).toLocaleDateString('en-IN', { dateStyle: 'medium' }) : ''

export const AdminOverviewPage = () => {
  const [kpis, setKpis] = useState(null)
  const [recentOrders, setRecentOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    const load = async () => {
      setLoading(true)
      setError('')
      try {
        const [kpiRes, ordersRes] = await Promise.all([
          adminApi.getDashboardKpis(),
          adminApi.getOrders().catch(() => null),
        ])
        if (!active) return
        setKpis(kpiRes)
        const raw = ordersRes?.items || ordersRes?.data || (Array.isArray(ordersRes) ? ordersRes : [])
        setRecentOrders(raw.slice(0, 6))
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
    <AdminLayout title="Overview" searchPlaceholder="Search anything...">
      {error && (
        <div className="bg-error-container text-on-error-container px-4 py-3 rounded-xl text-sm">{error}</div>
      )}

      {/* KPI Bento Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {KPI_CONFIG.map(cfg => (
          <div
            key={cfg.key}
            className="bg-white rounded-xl p-6 border border-slate-100 shadow-sm relative overflow-hidden group hover:border-primary/30 transition-colors"
          >
            {/* Decorative icon */}
            <div className="absolute top-0 right-0 p-4 opacity-10 group-hover:opacity-20 transition-opacity">
              <span className={`material-symbols-outlined text-5xl ${cfg.color}`}>{cfg.icon}</span>
            </div>
            <p className="text-sm font-medium text-on-surface-variant mb-2">{cfg.label}</p>
            <div className="flex items-baseline gap-2">
              {loading ? (
                <div className="h-8 w-20 bg-slate-200 animate-pulse rounded-lg" />
              ) : (
                <h3 className="text-3xl font-bold text-on-surface">{getKpiValue(kpis, cfg)}</h3>
              )}
            </div>
          </div>
        ))}
      </div>

      {/* Recent orders table */}
      <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden">
        <div className="p-5 border-b border-slate-100 flex justify-between items-center">
          <h3 className="text-lg font-semibold text-on-surface flex items-center gap-2">
            <span className="material-symbols-outlined text-primary">list_alt</span>
            Recent Orders
          </h3>
          <Link to="/admin/orders" className="text-sm font-medium text-primary hover:text-primary-container flex items-center gap-1 transition-colors">
            View All <span className="material-symbols-outlined text-base">chevron_right</span>
          </Link>
        </div>

        {loading ? (
          <div className="p-6 space-y-3">
            {[1,2,3,4].map(i => <div key={i} className="h-14 bg-slate-100 animate-pulse rounded-xl" />)}
          </div>
        ) : recentOrders.length === 0 ? (
          <div className="py-12 text-center text-on-surface-variant text-sm">No orders yet</div>
        ) : (
          <div className="divide-y divide-slate-50">
            {recentOrders.map((order) => {
              const status = order.orderStatus || order.status || ''
              const badgeClass = STATUS_BADGE[status] || 'bg-slate-100 text-slate-700'
              return (
                <div key={order.orderId || order.id} className="p-5 hover:bg-slate-50 transition-colors flex flex-col md:flex-row md:items-center justify-between gap-3">
                  <div className="flex items-start gap-4 flex-1">
                    <div className="bg-slate-100 h-11 w-11 rounded-lg flex items-center justify-center shrink-0 border border-slate-200">
                      <span className="text-xs font-bold text-on-surface-variant">#{String(order.orderId || order.id || '').split('-')[0].slice(0, 4).toUpperCase()}</span>
                    </div>
                    <div>
                      <h4 className="font-semibold text-on-surface text-sm">{order.customerName || order.customerEmail || 'Customer'}</h4>
                      <p className="text-xs text-on-surface-variant mt-0.5">{order.restaurantName || '—'} · {fmtDate(order.createdAt)}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className={`${badgeClass} text-xs font-semibold px-2.5 py-1 rounded-full`}>{status}</span>
                    <span className="text-sm font-semibold text-on-surface">₹{Number(order.total || order.totalAmount || 0).toFixed(2)}</span>
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>

      {/* Quick nav cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        {[
          { to: '/admin/orders', icon: 'receipt_long', label: 'Manage Orders', color: 'bg-blue-50 text-blue-600' },
          { to: '/admin/restaurants', icon: 'storefront', label: 'Manage Restaurants', color: 'bg-purple-50 text-purple-600' },
          { to: '/admin/users', icon: 'group', label: 'User Analytics', color: 'bg-emerald-50 text-emerald-600' },
        ].map(({ to, icon, label, color }) => (
          <Link
            key={to}
            to={to}
            className="bg-white border border-slate-100 rounded-xl p-5 hover:border-primary/30 hover:shadow-md transition-all flex items-center gap-4 group"
          >
            <div className={`w-12 h-12 rounded-xl ${color} flex items-center justify-center flex-shrink-0`}>
              <span className="material-symbols-outlined text-xl">{icon}</span>
            </div>
            <span className="font-semibold text-on-surface group-hover:text-primary transition-colors">{label}</span>
            <span className="material-symbols-outlined text-slate-400 ml-auto">arrow_forward</span>
          </Link>
        ))}
      </div>
    </AdminLayout>
  )
}
