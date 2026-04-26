import { useEffect, useState } from 'react'
import { AgentLayout } from '../components/organisms/AgentLayout'
import { useNotification } from '../hooks/useNotification'
import api from '../services/api'
import catalogApi from '../services/catalogApi'

const isToday = (iso) => {
  if (!iso) return false
  const d = new Date(iso)
  const now = new Date()
  return d.getDate() === now.getDate()
    && d.getMonth() === now.getMonth()
    && d.getFullYear() === now.getFullYear()
}

const fmtDate = (iso) => iso
  ? new Date(iso).toLocaleString('en-IN', { dateStyle: 'medium', timeStyle: 'short' })
  : '—'

export const AgentEarningsPage = () => {
  const { showError } = useNotification()
  const [deliveries, setDeliveries] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let active = true
    const load = async () => {
      setLoading(true)
      try {
        const res = await api.get('/gateway/orders/deliveries/assigned')
        const all = Array.isArray(res.data) ? res.data : (res.data?.items || [])

        const delivered = all.filter(a => {
          const s = a.currentStatus || a.CurrentStatus || ''
          return s === 'Delivered'
        })

        const enriched = await Promise.allSettled(
          delivered.map(async (assignment) => {
            const orderId = assignment.orderId || assignment.OrderId
            let order = null
            let restaurant = null
            try {
              const orderRes = await api.get(`/gateway/orders/${orderId}`)
              order = orderRes.data?.order ?? orderRes.data
              if (order?.restaurantId) {
                restaurant = await catalogApi.getRestaurantById(order.restaurantId).catch(() => null)
              }
            } catch { /* no-op */ }
            return { assignment, order, restaurant }
          })
        )

        if (!active) return
        setDeliveries(
          enriched
            .filter(r => r.status === 'fulfilled')
            .map(r => r.value)
        )
      } catch (err) {
        if (!active) return
        showError(err.response?.data?.message || 'Failed to load earnings')
      } finally {
        if (active) setLoading(false)
      }
    }
    load()
    return () => { active = false }
  }, [showError])

  const todayDeliveries = deliveries.filter(d => isToday(d.assignment.deliveredAt || d.assignment.DeliveredAt || d.order?.updatedAt))
  const totalAmount = deliveries.reduce((s, d) => s + Number(d.order?.total ?? d.order?.totalAmount ?? 0), 0)
  const todayAmount = todayDeliveries.reduce((s, d) => s + Number(d.order?.total ?? d.order?.totalAmount ?? 0), 0)

  const statCards = [
    { label: "Today's Deliveries", value: todayDeliveries.length, icon: 'local_shipping', color: 'text-blue-600 bg-blue-50' },
    { label: "Today's Value", value: `₹${todayAmount.toFixed(2)}`, icon: 'payments', color: 'text-emerald-600 bg-emerald-50' },
    { label: 'Total Deliveries', value: deliveries.length, icon: 'inventory_2', color: 'text-purple-600 bg-purple-50' },
    { label: 'Total COD Value', value: `₹${totalAmount.toFixed(2)}`, icon: 'account_balance_wallet', color: 'text-amber-600 bg-amber-50' },
  ]

  return (
    <AgentLayout title="Earnings">
      {loading ? (
        <div className="space-y-4">
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            {[1,2,3,4].map(i => <div key={i} className="h-28 bg-slate-200 animate-pulse rounded-xl" />)}
          </div>
          <div className="h-48 bg-slate-200 animate-pulse rounded-xl" />
        </div>
      ) : (
        <>
          {/* KPI cards */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            {statCards.map(({ label, value, icon, color }) => (
              <div key={label} className="bg-white rounded-xl p-5 border border-slate-100 shadow-sm relative overflow-hidden group hover:border-primary/30 transition-colors">
                <div className="absolute top-0 right-0 p-3 opacity-10 group-hover:opacity-20 transition-opacity">
                  <span className={`material-symbols-outlined text-5xl ${color.split(' ')[0]}`}>{icon}</span>
                </div>
                <p className="text-xs font-medium text-on-surface-variant mb-2">{label}</p>
                <p className="text-2xl font-bold text-on-surface">{value}</p>
              </div>
            ))}
          </div>

          {/* History table */}
          <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden">
            <div className="p-5 border-b border-slate-100 flex items-center gap-2">
              <span className="material-symbols-outlined text-primary">list_alt</span>
              <h2 className="text-lg font-semibold text-on-surface">Delivery History</h2>
            </div>

            {deliveries.length === 0 ? (
              <div className="py-16 text-center text-on-surface-variant">
                <p className="text-4xl mb-3">📦</p>
                <p className="text-lg font-semibold">No completed deliveries yet</p>
              </div>
            ) : (
              <div className="divide-y divide-slate-50">
                {deliveries.map(({ assignment, order, restaurant }) => {
                  const orderId = assignment.orderId || assignment.OrderId
                  const deliveredAt = assignment.deliveredAt || assignment.DeliveredAt || order?.updatedAt
                  const total = Number(order?.total ?? order?.totalAmount ?? 0)
                  const itemCount = order?.items?.length ?? 0
                  return (
                    <div
                      key={orderId}
                      className="flex flex-wrap items-center justify-between gap-3 px-5 py-4 hover:bg-slate-50 transition-colors"
                    >
                      <div className="flex items-center gap-4">
                        <div className="w-10 h-10 rounded-xl bg-slate-100 border border-slate-200 flex items-center justify-center flex-shrink-0">
                          <span className="material-symbols-outlined text-slate-500 text-lg" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
                        </div>
                        <div>
                          <div className="flex items-center gap-2 flex-wrap">
                            <span className="font-semibold text-sm text-on-surface">
                              #{String(orderId).split('-')[0].toUpperCase()}
                            </span>
                            {restaurant?.name && (
                              <span className="bg-slate-100 text-slate-600 px-2 py-0.5 rounded-full text-xs font-medium">
                                {restaurant.name}
                              </span>
                            )}
                          </div>
                          <p className="text-xs text-on-surface-variant mt-0.5">
                            {itemCount > 0 ? `${itemCount} item${itemCount > 1 ? 's' : ''} · ` : ''}{fmtDate(deliveredAt)}
                          </p>
                        </div>
                      </div>
                      <span className="font-bold text-emerald-600 text-sm">₹{total.toFixed(2)}</span>
                    </div>
                  )
                })}
              </div>
            )}
          </div>

          <p className="text-center text-xs text-on-surface-variant">
            Values shown are total order values (COD basis)
          </p>
        </>
      )}
    </AgentLayout>
  )
}
