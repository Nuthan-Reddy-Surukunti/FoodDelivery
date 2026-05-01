import { useEffect, useState, useMemo } from 'react'
import { Link } from 'react-router-dom'
import { AdminLayout } from '../components/organisms/AdminLayout'
import { KpiCard } from '../components/molecules/KpiCard'
import { OrdersChart } from '../components/molecules/OrdersChart'
import adminApi from '../services/adminApi'

// Compute percentage change between two values, capped and formatted
function computeTrend(current, previous) {
  if (!previous || previous === 0) return null
  const pct = ((current - previous) / previous) * 100
  const sign = pct >= 0 ? '+' : ''
  return `${sign}${pct.toFixed(1)}%`
}

function computeTrendDirection(current, previous) {
  if (!previous || previous === 0) return 'up'
  return current >= previous ? 'up' : 'down'
}

function getKpiValue(kpis, key, fallbackKeys = []) {
  const keys = [key, ...fallbackKeys]
  for (const k of keys) {
    if (kpis?.[k] !== undefined && kpis?.[k] !== null) {
      const val = Number(kpis[k])
      return val.toLocaleString()
    }
  }
  return '0'
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

  // Build real KPI card configs from live data
  const kpiCards = useMemo(() => {
    if (!kpis) return []
    return [
      {
        key: 'totalRevenue',
        label: 'Total Revenue',
        icon: 'payments',
        iconBg: 'bg-blue-50',
        iconColor: 'text-blue-600',
        value: loading ? '—' : `₹${Number(kpis.totalRevenue || 0).toLocaleString('en-IN')}`,
        trend: computeTrend(kpis.revenueToday, kpis.revenueYesterday),
        trendDirection: computeTrendDirection(kpis.revenueToday, kpis.revenueYesterday),
      },
      {
        key: 'ordersToday',
        label: "Today's Orders",
        icon: 'shopping_bag',
        iconBg: 'bg-blue-50',
        iconColor: 'text-blue-600',
        value: loading ? '—' : String(kpis.ordersToday ?? 0),
        trend: computeTrend(kpis.ordersToday, kpis.ordersYesterday),
        trendDirection: computeTrendDirection(kpis.ordersToday, kpis.ordersYesterday),
      },
      {
        key: 'activeRestaurants',
        label: 'Active Restaurants',
        icon: 'storefront',
        iconBg: 'bg-orange-50',
        iconColor: 'text-orange-600',
        value: loading ? '—' : String(kpis.activePartners ?? 0),
        trend: null,
        trendDirection: 'up',
      },
      {
        key: 'totalUsers',
        label: 'Total Users',
        icon: 'group',
        iconBg: 'bg-purple-50',
        iconColor: 'text-purple-600',
        value: loading ? '—' : String(kpis.totalUsers ?? kpis.pendingApprovals ?? 0),
        trend: null,
        trendDirection: 'up',
      },
    ]
  }, [kpis, loading])

  // Build chart data from backend daily order counts
  const chartData = useMemo(() => {
    if (!kpis?.dailyOrderCounts) return []
    return Object.entries(kpis.dailyOrderCounts).map(([dateStr, count]) => {
      const d = new Date(dateStr)
      return {
        day: d.toLocaleDateString('en-US', { weekday: 'short' }),
        orders: Number(count || 0),
      }
    })
  }, [kpis])

  return (
    <AdminLayout title="Dashboard" searchPlaceholder="Search...">
      {error && (
        <div className="bg-error-container text-on-error-container px-4 py-3 rounded-xl text-sm mb-6">{error}</div>
      )}

      {/* Page Header */}
      <div className="mb-6">
        <h1 className="font-headline-lg text-headline-lg text-on-surface">Overview</h1>
        <p className="font-body-md text-body-md text-on-surface-variant mt-1">Monitor key metrics and recent activity across the platform.</p>
      </div>

      {/* KPI Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-gutter mb-lg">
        {loading
          ? [1, 2, 3, 4].map(i => <div key={i} className="h-28 bg-slate-200 animate-pulse rounded-2xl" />)
          : kpiCards.map(cfg => (
            <KpiCard
              key={cfg.key}
              icon={cfg.icon}
              label={cfg.label}
              value={cfg.value}
              trend={cfg.trend}
              trendDirection={cfg.trendDirection}
              iconBg={cfg.iconBg}
              iconColor={cfg.iconColor}
            />
          ))
        }
      </div>


      {/* Chart and Recent Orders Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-gutter mb-lg">
        {/* Orders Chart */}
        <div className="lg:col-span-2">
          <OrdersChart data={chartData} title="Orders Over Time" />
        </div>

        {/* Recent Orders */}
        <div className="bg-surface-container-lowest rounded-xl border border-surface-variant shadow-sm flex flex-col overflow-hidden">
          <div className="p-6 border-b border-surface-variant flex justify-between items-center bg-surface-container-lowest">
            <h3 className="font-headline-md text-headline-md text-on-surface text-[20px] leading-tight">Recent Orders</h3>
            <Link to="/admin/orders" className="text-primary hover:text-primary-container transition-colors">
              <span className="material-symbols-outlined">more_vert</span>
            </Link>
          </div>

          {loading ? (
            <div className="p-6 space-y-4">
              {[1,2,3].map(i => <div key={i} className="h-12 bg-slate-200 animate-pulse rounded-lg" />)}
            </div>
          ) : recentOrders.length === 0 ? (
            <div className="py-8 text-center text-on-surface-variant text-sm">📋 No recent orders</div>
          ) : (
            <div className="divide-y divide-surface-variant flex-1 overflow-y-auto max-h-[400px]">
              {recentOrders.map((order) => {
                const status = order.orderStatus || order.status || ''
                const badgeClass = STATUS_BADGE[status] || 'bg-slate-100 text-slate-700'
                return (
                  <div key={order.orderId || order.id} className="p-4 hover:bg-surface-container transition-colors">
                    <div className="flex items-start justify-between gap-2">
                      <div>
                        <h4 className="font-semibold text-on-surface text-sm">#{String(order.orderId || order.id || '').slice(0, 8)}</h4>
                        <p className="text-xs text-on-surface-variant mt-0.5">{order.customerName || 'Customer'}</p>
                        <p className="text-xs text-on-surface-variant mt-1">{order.restaurantName || '—'}</p>
                      </div>
                      <span className={`${badgeClass} text-xs font-semibold px-2 py-1 rounded-full whitespace-nowrap`}>{status}</span>
                    </div>
                    <div className="mt-2 text-sm font-semibold text-on-surface">₹{Number(order.total || order.totalAmount || 0).toFixed(2)}</div>
                  </div>
                )
              })}
            </div>
          )}

          <div className="p-4 border-t border-surface-variant">
            <Link 
              to="/admin/orders" 
              className="text-primary font-label-md text-label-md flex items-center justify-center gap-1 hover:text-primary-container transition-colors w-full py-2"
            >
              View All Orders <span className="material-symbols-outlined text-[16px]">arrow_forward</span>
            </Link>
          </div>
        </div>
      </div>
    </AdminLayout>
  )
}
