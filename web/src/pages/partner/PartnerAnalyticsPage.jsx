import { useEffect, useState, useMemo } from 'react'
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts'
import { PartnerLayout } from '../../components/organisms/PartnerLayout'
import orderApi from '../../services/orderApi'
import catalogApi from '../../services/catalogApi'

const StatCard = ({ title, value, icon, trend, isCurrency }) => {
  const numValue = Number(value || 0)
  return (
    <div className="bg-surface-container-lowest p-6 rounded-2xl shadow-sm border border-slate-200">
      <div className="flex items-start justify-between mb-4">
        <div className="w-12 h-12 rounded-xl bg-primary/10 flex items-center justify-center text-primary">
          <span className="material-symbols-outlined">{icon}</span>
        </div>
        {trend && (
          <span className="bg-green-100 text-green-700 text-xs font-semibold px-2 py-1 rounded-full flex items-center gap-1">
            <span className="material-symbols-outlined text-[14px]">trending_up</span>
            {trend}%
          </span>
        )}
      </div>
      <h3 className="text-on-surface-variant text-sm font-medium mb-1">{title}</h3>
      <p className="text-2xl font-bold text-on-surface">
        {isCurrency ? `₹${numValue.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}` : numValue}
      </p>
    </div>
  )
}

export const PartnerAnalyticsPage = () => {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [stats, setStats] = useState(null)

  useEffect(() => {
    let active = true
    const fetchData = async () => {
      setLoading(true)
      setError('')
      try {
        const restaurant = await catalogApi.getMyRestaurant()
        if (!restaurant || !restaurant.id) {
           setError('No active restaurant found. Please ensure your restaurant is set up.')
           return
        }
        const data = await orderApi.getPartnerStats(restaurant.id)
        if (active) setStats(data)
      } catch (err) {
        if (active) {
          console.error('Analytics Fetch Error:', err)
          setError(err.response?.data?.message || err.message || 'Failed to load analytics')
        }
      } finally {
        if (active) setLoading(false)
      }
    }
    fetchData()
    return () => { active = false }
  }, [])

  // Format chart data
  const chartData = useMemo(() => {
    if (!stats?.dailyRevenue) return []
    return Object.entries(stats.dailyRevenue).map(([date, revenue]) => {
      const d = new Date(date)
      return {
        name: d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
        revenue: Number(revenue || 0),
      }
    })
  }, [stats])

  if (loading) {
    return (
      <PartnerLayout>
        <div className="p-8">
          <div className="h-8 w-48 bg-slate-200 rounded animate-pulse mb-8" />
          <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
            {[1, 2, 3, 4].map((i) => (
              <div key={i} className="h-32 bg-slate-200 rounded-2xl animate-pulse" />
            ))}
          </div>
          <div className="h-[400px] bg-slate-200 rounded-2xl animate-pulse" />
        </div>
      </PartnerLayout>
    )
  }

  if (error || !stats) {
    return (
      <PartnerLayout>
        <div className="p-8">
          <div className="bg-error-container text-on-error-container px-6 py-4 rounded-2xl shadow-sm border border-error/20">
            <div className="flex items-center gap-3">
              <span className="material-symbols-outlined text-error">error</span>
              <p className="font-medium">{error || 'No analytics data available yet.'}</p>
            </div>
          </div>
        </div>
      </PartnerLayout>
    )
  }

  return (
    <PartnerLayout>
      <div className="max-w-6xl mx-auto p-8">
        <div className="mb-8">
          <h1 className="text-2xl font-bold text-on-surface">Analytics Overview</h1>
          <p className="text-on-surface-variant text-sm mt-1">Track your restaurant's performance</p>
        </div>

        {/* Top Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <StatCard
            title="Today's Revenue"
            value={stats.todayRevenue}
            icon="payments"
            isCurrency
          />
          <StatCard
            title="Today's Orders"
            value={stats.todayOrders}
            icon="receipt_long"
          />
          <StatCard
            title="Total Revenue"
            value={stats.totalRevenue}
            icon="account_balance_wallet"
            isCurrency
          />
          <StatCard
            title="Total Orders"
            value={stats.totalOrders}
            icon="inventory_2"
          />
        </div>

        {/* Chart Section */}
        <div className="bg-surface-container-lowest p-6 rounded-2xl shadow-sm border border-slate-200">
          <div className="mb-6">
            <h2 className="text-lg font-bold text-on-surface">Revenue (Last 7 Days)</h2>
          </div>
          <div className="h-[400px] w-full">
            {chartData.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={chartData} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E2E8F0" />
                  <XAxis 
                    dataKey="name" 
                    axisLine={false}
                    tickLine={false}
                    tick={{ fill: '#64748B', fontSize: 12 }}
                    dy={10}
                  />
                  <YAxis 
                    axisLine={false}
                    tickLine={false}
                    tick={{ fill: '#64748B', fontSize: 12 }}
                    tickFormatter={(value) => `₹${value}`}
                    dx={-10}
                  />
                  <Tooltip 
                    cursor={{ fill: '#F1F5F9' }}
                    contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                    formatter={(value) => [`₹${Number(value).toFixed(2)}`, 'Revenue']}
                  />
                  <Bar dataKey="revenue" fill="#6366f1" radius={[4, 4, 0, 0]} maxBarSize={50} />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-full flex items-center justify-center flex-col text-slate-400">
                <span className="material-symbols-outlined text-4xl mb-2">bar_chart</span>
                <p>No revenue data for the last 7 days</p>
              </div>
            )}
          </div>
        </div>

        {/* Order Status Breakdown */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-8">
          <div className="bg-surface-container-lowest p-6 rounded-2xl shadow-sm border border-slate-200 flex items-center justify-between">
            <div>
              <p className="text-on-surface-variant text-sm font-medium mb-1">Pending Orders</p>
              <p className="text-2xl font-bold text-orange-600">{stats.pendingOrders}</p>
            </div>
            <div className="w-12 h-12 rounded-full bg-orange-100 flex items-center justify-center text-orange-600">
              <span className="material-symbols-outlined">pending_actions</span>
            </div>
          </div>
          <div className="bg-surface-container-lowest p-6 rounded-2xl shadow-sm border border-slate-200 flex items-center justify-between">
            <div>
              <p className="text-on-surface-variant text-sm font-medium mb-1">Preparing/Active</p>
              <p className="text-2xl font-bold text-blue-600">{stats.preparingOrders}</p>
            </div>
            <div className="w-12 h-12 rounded-full bg-blue-100 flex items-center justify-center text-blue-600">
              <span className="material-symbols-outlined">soup_kitchen</span>
            </div>
          </div>
          <div className="bg-surface-container-lowest p-6 rounded-2xl shadow-sm border border-slate-200 flex items-center justify-between">
            <div>
              <p className="text-on-surface-variant text-sm font-medium mb-1">Cancelled Orders</p>
              <p className="text-2xl font-bold text-red-600">{stats.cancelledOrders}</p>
            </div>
            <div className="w-12 h-12 rounded-full bg-red-100 flex items-center justify-center text-red-600">
              <span className="material-symbols-outlined">cancel</span>
            </div>
          </div>
        </div>
      </div>
    </PartnerLayout>
  )
}
