import { useEffect, useState } from 'react'
import { Card } from '../components/atoms/Card'
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

        // Filter to delivered-only assignments
        const delivered = all.filter(a => {
          const s = a.currentStatus || a.CurrentStatus || ''
          return s === 'Delivered'
        })

        // Enrich each with order + restaurant info
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

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-bold">Earnings</h1>

      {loading ? (
        <div className="space-y-4">
          {[1, 2, 3, 4].map(i => (
            <div key={i} className="h-16 animate-pulse rounded-2xl border border-outline bg-surface-dim" />
          ))}
        </div>
      ) : (
        <>
          {/* Summary cards */}
          <div className="mb-6 grid grid-cols-2 gap-4 sm:grid-cols-4">
            {[
              { label: "Today's Deliveries", value: todayDeliveries.length },
              { label: "Today's Value", value: `₹${todayAmount.toFixed(2)}` },
              { label: 'Total Deliveries', value: deliveries.length },
              { label: 'Total Value', value: `₹${totalAmount.toFixed(2)}` },
            ].map(c => (
              <Card key={c.label} className="p-4 text-center">
                <p className="text-xs text-on-background/60">{c.label}</p>
                <p className="mt-2 text-xl font-bold">{c.value}</p>
              </Card>
            ))}
          </div>

          {/* History table */}
          <Card className="p-5">
            <h2 className="mb-4 text-lg font-semibold">Delivery History</h2>
            {deliveries.length === 0 ? (
              <div className="py-8 text-center text-on-background/50">
                <p className="text-3xl mb-2">📦</p>
                <p>No completed deliveries yet.</p>
              </div>
            ) : (
              <div className="space-y-3">
                {deliveries.map(({ assignment, order, restaurant }) => {
                  const orderId = assignment.orderId || assignment.OrderId
                  const deliveredAt = assignment.deliveredAt || assignment.DeliveredAt || order?.updatedAt
                  const total = Number(order?.total ?? order?.totalAmount ?? 0)
                  const itemCount = order?.items?.length ?? 0
                  return (
                    <div
                      key={orderId}
                      className="flex flex-wrap items-start justify-between gap-3 rounded-xl border border-outline px-4 py-3"
                    >
                      <div className="min-w-0">
                        <div className="flex items-center gap-2">
                          <span className="font-semibold text-sm">
                            #{String(orderId).split('-')[0].toUpperCase()}
                          </span>
                          {restaurant?.name && (
                            <span className="rounded-full bg-surface-dim px-2 py-0.5 text-xs text-on-background/70">
                              {restaurant.name}
                            </span>
                          )}
                        </div>
                        <p className="mt-0.5 text-xs text-on-background/50">
                          {itemCount > 0 ? `${itemCount} item${itemCount > 1 ? 's' : ''}` : ''}{' · '}
                          {fmtDate(deliveredAt)}
                        </p>
                      </div>
                      <span className="font-bold text-green-600">₹{total.toFixed(2)}</span>
                    </div>
                  )
                })}
              </div>
            )}
          </Card>

          <p className="mt-4 text-center text-xs text-on-background/50">
            Values shown are total order values (COD basis)
          </p>
        </>
      )}
    </div>
  )
}
