import { useCallback, useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'
import { useNotification } from '../hooks/useNotification'
import catalogApi from '../services/catalogApi'
import api from '../services/api'

// Backend OrderStatus enum integers — must match OrderService.Domain.Enums.OrderStatus exactly
const ORDER_STATUS_INT = {
  Preparing: 6,
  ReadyForPickup: 7,
  RestaurantRejected: 13,
}

// Keyed by integer AND string for resilience — backend may serialize either way
const STATUS_FLOW = {
  // Integer keys
  4:  { label: 'New Order (Paid)',     actionLabel: 'Accept & Prepare',       nextStatus: ORDER_STATUS_INT.Preparing,      color: 'text-blue-600 bg-blue-50 border-blue-200',   canReject: true  },
  5:  { label: 'Accepted',            actionLabel: 'Mark Preparing',          nextStatus: ORDER_STATUS_INT.Preparing,      color: 'text-indigo-600 bg-indigo-50 border-indigo-200', canReject: true },
  6:  { label: 'Preparing',           actionLabel: 'Mark Ready for Pickup',   nextStatus: ORDER_STATUS_INT.ReadyForPickup, color: 'text-amber-600 bg-amber-50 border-amber-200',   canReject: false },
  7:  { label: 'Ready for Pickup',    actionLabel: null,                      nextStatus: null,                            color: 'text-green-600 bg-green-50 border-green-200',  canReject: false },
  8:  { label: 'Picked Up',           actionLabel: null,                      nextStatus: null,                            color: 'text-teal-600 bg-teal-50 border-teal-200',     canReject: false },
  9:  { label: 'Out for Delivery',    actionLabel: null,                      nextStatus: null,                            color: 'text-cyan-600 bg-cyan-50 border-cyan-200',     canReject: false },
  10: { label: 'Delivered',           actionLabel: null,                      nextStatus: null,                            color: 'text-gray-500 bg-gray-50 border-gray-200',    canReject: false },
  13: { label: 'Rejected',            actionLabel: null,                      nextStatus: null,                            color: 'text-red-600 bg-red-50 border-red-200',        canReject: false },
  // String-name fallbacks
  Paid:              { label: 'New Order',        actionLabel: 'Accept & Prepare',     nextStatus: ORDER_STATUS_INT.Preparing,      color: 'text-blue-600 bg-blue-50 border-blue-200',  canReject: true  },
  RestaurantAccepted:{ label: 'Accepted',         actionLabel: 'Mark Preparing',       nextStatus: ORDER_STATUS_INT.Preparing,      color: 'text-indigo-600 bg-indigo-50 border-indigo-200', canReject: true },
  Preparing:         { label: 'Preparing',        actionLabel: 'Mark Ready for Pickup',nextStatus: ORDER_STATUS_INT.ReadyForPickup, color: 'text-amber-600 bg-amber-50 border-amber-200',   canReject: false },
  ReadyForPickup:    { label: 'Ready for Pickup', actionLabel: null,                   nextStatus: null,                            color: 'text-green-600 bg-green-50 border-green-200',  canReject: false },
}

const formatTime = (iso) => {
  if (!iso) return ''
  return new Date(iso).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true })
}

export const OrderQueuePage = () => {
  const { showSuccess, showError } = useNotification()
  const [restaurant, setRestaurant] = useState(null)
  const [loadingRestaurant, setLoadingRestaurant] = useState(true)
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(false)
  const [actioning, setActioning] = useState(null)
  const intervalRef = useRef(null)

  // Load partner's restaurant
  useEffect(() => {
    let active = true
    const load = async () => {
      setLoadingRestaurant(true)
      try {
        const r = await catalogApi.getMyRestaurant()
        if (!active) return
        setRestaurant(r)
      } catch {
        if (!active) return
      } finally {
        if (active) setLoadingRestaurant(false)
      }
    }
    load()
    return () => { active = false }
  }, [])

  // Fetch orders via the partner-specific queue endpoint
  const fetchOrders = useCallback(async () => {
    setLoading(true)
    try {
      const data = await api.get('/gateway/orders/queue')
      const arr = Array.isArray(data.data) ? data.data : (data.data?.items || [])
      setOrders(arr)
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to fetch orders')
    } finally {
      setLoading(false)
    }
  }, [showError])

  useEffect(() => {
    if (!restaurant?.id) return
    fetchOrders()
    intervalRef.current = setInterval(() => fetchOrders(), 30000)
    return () => clearInterval(intervalRef.current)
  }, [restaurant?.id, fetchOrders])

  const handleAction = async (order, nextStatusInt) => {
    const id = order.orderId || order.id
    setActioning(id)
    try {
      // targetStatus MUST be an integer matching the enum value
      await api.put(`/gateway/orders/${id}/status`, { orderId: id, targetStatus: nextStatusInt })
      showSuccess('Order status updated')
      setOrders(prev => prev.map(o =>
        (o.orderId || o.id) === id ? { ...o, orderStatus: nextStatusInt } : o
      ))
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to update order status')
    } finally {
      setActioning(null)
    }
  }

  const handleReject = async (order) => {
    if (!window.confirm('Reject this order?')) return
    const id = order.orderId || order.id
    setActioning(id)
    try {
      await api.put(`/gateway/orders/${id}/status`, { orderId: id, targetStatus: ORDER_STATUS_INT.RestaurantRejected })
      showSuccess('Order rejected')
      setOrders(prev => prev.filter(o => (o.orderId || o.id) !== id))
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to reject order')
    } finally {
      setActioning(null)
    }
  }

  // ── Guards ───────────────────────────────────────────────────────────────────

  if (loadingRestaurant) {
    return <div className="mx-auto max-w-5xl px-4 py-8"><p className="text-sm text-on-background/70">Loading order queue...</p></div>
  }

  if (!restaurant) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-8">
        <Card className="p-5">
          <p className="text-on-background/70">No restaurant found. <Link to="/partner/dashboard" className="text-primary font-semibold">Complete setup →</Link></p>
        </Card>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      {/* Header */}
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <div>
          <Link to="/partner/dashboard" className="text-sm text-on-background/60 hover:text-primary">← Dashboard</Link>
          <h1 className="text-2xl font-bold">Order Queue — {restaurant.name}</h1>
        </div>
        <Button variant="secondary" onClick={fetchOrders}>🔄 Refresh</Button>
      </div>

      {/* Orders */}
      {loading ? (
        <p className="text-sm text-on-background/70">Loading orders...</p>
      ) : orders.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-outline p-10 text-center text-on-background/60">
          No active orders right now.
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map(order => {
            const id = order.orderId || order.id
            // orderStatus is an integer enum from the backend
            const statusKey = order.orderStatus ?? order.status ?? 0
            const flow = STATUS_FLOW[statusKey]
            const isActioning = actioning === id
            const items = order.items || []
            const total = order.total || order.totalAmount || 0
            const addr = order.deliveryAddress
            const addrStr = addr
              ? [addr.street, addr.city, addr.pincode].filter(Boolean).join(', ')
              : 'Address not available'

            return (
              <Card key={id} className="p-5">
                <div className="flex flex-wrap items-start justify-between gap-3 mb-4">
                  <div>
                    <p className="font-semibold text-sm text-on-background/60">Order #{String(id).split('-')[0].toUpperCase()}</p>
                    <p className="text-xs text-on-background/50 mt-0.5">{formatTime(order.createdAt)}</p>
                  </div>
                  <span className={`rounded-full border px-3 py-1 text-xs font-semibold ${flow?.color || 'text-gray-600 bg-gray-50 border-gray-200'}`}>
                    {flow?.label || String(statusKey)}
                  </span>
                </div>

                {/* Items */}
                {items.length > 0 && (
                  <div className="mb-4 rounded-xl bg-surface-dim p-3 space-y-1">
                    {items.map((item, idx) => (
                      <div key={item.orderItemId || idx} className="flex justify-between text-sm">
                        <span>{item.quantity}× {item.menuItemName || item.name || item.menuItemId?.split('-')[0]}</span>
                        <span className="text-on-background/70">₹{item.subtotal || (item.unitPriceSnapshot * item.quantity) || 0}</span>
                      </div>
                    ))}
                    <div className="border-t border-outline mt-2 pt-2 flex justify-between font-semibold text-sm">
                      <span>Total</span>
                      <span>₹{total}</span>
                    </div>
                  </div>
                )}

                {/* Address */}
                <p className="text-xs text-on-background/60 mb-4">📍 {addrStr}</p>

                {/* Actions */}
                <div className="flex flex-wrap gap-2">
                  {flow?.nextStatus != null && (
                    <Button size="sm" disabled={isActioning} onClick={() => handleAction(order, flow.nextStatus)}>
                      {isActioning ? 'Updating...' : flow.actionLabel}
                    </Button>
                  )}
                  {flow?.canReject && (
                    <Button size="sm" variant="tertiary" disabled={isActioning} onClick={() => handleReject(order)}>
                      Reject
                    </Button>
                  )}
                </div>
              </Card>
            )
          })}
        </div>
      )}

      <p className="mt-4 text-center text-xs text-on-background/50">Auto-refreshes every 30 seconds</p>
    </div>
  )
}
