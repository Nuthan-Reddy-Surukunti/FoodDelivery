import { useCallback, useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'
import { useNotification } from '../hooks/useNotification'
import catalogApi from '../services/catalogApi'
import orderApi from '../services/orderApi'

const STATUS_FLOW = {
  Paid: { label: 'New Order', actionLabel: 'Accept & Prepare', nextStatus: 'Preparing', color: 'text-blue-600 bg-blue-50 border-blue-200' },
  RestaurantAccepted: { label: 'Accepted', actionLabel: 'Mark Preparing', nextStatus: 'Preparing', color: 'text-indigo-600 bg-indigo-50 border-indigo-200' },
  Preparing: { label: 'Preparing', actionLabel: 'Mark Ready for Pickup', nextStatus: 'ReadyForPickup', color: 'text-amber-600 bg-amber-50 border-amber-200' },
  ReadyForPickup: { label: 'Ready for Pickup', actionLabel: null, nextStatus: null, color: 'text-green-600 bg-green-50 border-green-200' },
  PickedUp: { label: 'Picked Up', actionLabel: null, nextStatus: null, color: 'text-teal-600 bg-teal-50 border-teal-200' },
  OutForDelivery: { label: 'Out for Delivery', actionLabel: null, nextStatus: null, color: 'text-cyan-600 bg-cyan-50 border-cyan-200' },
  Delivered: { label: 'Delivered', actionLabel: null, nextStatus: null, color: 'text-gray-500 bg-gray-50 border-gray-200' },
  RestaurantRejected: { label: 'Rejected', actionLabel: null, nextStatus: null, color: 'text-red-600 bg-red-50 border-red-200' },
  Cancelled: { label: 'Cancelled', actionLabel: null, nextStatus: null, color: 'text-red-500 bg-red-50 border-red-200' },
}

const ACTIVE_STATUSES = ['Paid', 'RestaurantAccepted', 'Preparing', 'ReadyForPickup']

const formatTime = (iso) => {
  if (!iso) return ''
  const d = new Date(iso)
  return d.toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true })
}

export const OrderQueuePage = () => {
  const { showSuccess, showError } = useNotification()
  const [restaurant, setRestaurant] = useState(null)
  const [loadingRestaurant, setLoadingRestaurant] = useState(true)
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(false)
  const [actioning, setActioning] = useState(null)
  const [filter, setFilter] = useState('active') // 'active' | 'all'
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

  // Fetch orders for the restaurant
  const fetchOrders = useCallback(async (restaurantId, isActive) => {
    setLoading(true)
    try {
      const data = await orderApi.getOrdersByUser(null, isActive, { restaurantId })
      const arr = Array.isArray(data) ? data : (data?.items || data?.data || [])
      setOrders(arr)
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to fetch orders')
    } finally {
      setLoading(false)
    }
  }, [showError])

  useEffect(() => {
    if (!restaurant?.id) return
    const isActive = filter === 'active'
    fetchOrders(restaurant.id, isActive)

    // Auto-refresh every 30 seconds
    intervalRef.current = setInterval(() => fetchOrders(restaurant.id, isActive), 30000)
    return () => clearInterval(intervalRef.current)
  }, [restaurant?.id, filter, fetchOrders])

  // Update order status
  const handleAction = async (order, nextStatus) => {
    const id = order.orderId || order.id
    setActioning(id)
    try {
      await orderApi.updateOrderStatus(id, nextStatus)
      showSuccess(`Order marked as ${nextStatus}`)
      setOrders(prev => prev.map(o =>
        (o.orderId || o.id) === id ? { ...o, orderStatus: nextStatus } : o
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
      await orderApi.updateOrderStatus(id, 'RestaurantRejected')
      showSuccess('Order rejected')
      setOrders(prev => prev.map(o =>
        (o.orderId || o.id) === id ? { ...o, orderStatus: 'RestaurantRejected' } : o
      ))
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

  const visibleOrders = filter === 'active'
    ? orders.filter(o => ACTIVE_STATUSES.includes(o.orderStatus || o.status))
    : orders

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      {/* Header */}
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <div>
          <Link to="/partner/dashboard" className="text-sm text-on-background/60 hover:text-primary">← Dashboard</Link>
          <h1 className="text-2xl font-bold">Order Queue — {restaurant.name}</h1>
        </div>
        <div className="flex items-center gap-3">
          <div className="flex rounded-xl border border-outline overflow-hidden text-sm">
            <button
              onClick={() => setFilter('active')}
              className={`px-4 py-2 font-medium transition ${filter === 'active' ? 'bg-primary text-on-primary' : 'hover:bg-surface-dim'}`}
            >Active</button>
            <button
              onClick={() => setFilter('all')}
              className={`px-4 py-2 font-medium transition ${filter === 'all' ? 'bg-primary text-on-primary' : 'hover:bg-surface-dim'}`}
            >All</button>
          </div>
          <Button variant="secondary" onClick={() => fetchOrders(restaurant.id, filter === 'active')}>
            🔄 Refresh
          </Button>
        </div>
      </div>

      {/* Orders */}
      {loading ? (
        <p className="text-sm text-on-background/70">Loading orders...</p>
      ) : visibleOrders.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-outline p-10 text-center text-on-background/60">
          {filter === 'active' ? 'No active orders right now.' : 'No orders found.'}
        </div>
      ) : (
        <div className="space-y-4">
          {visibleOrders.map(order => {
            const id = order.orderId || order.id
            const status = order.orderStatus || order.status || 'Unknown'
            const flow = STATUS_FLOW[status]
            const isActioning = actioning === id
            const items = order.items || []
            const total = order.total || order.totalAmount || 0
            const addr = order.deliveryAddress
            const addrStr = addr
              ? `${addr.street || addr.addressLine1 || ''}, ${addr.city || ''}`
              : 'Address not available'

            return (
              <Card key={id} className="p-5">
                <div className="flex flex-wrap items-start justify-between gap-3 mb-4">
                  <div>
                    <p className="font-semibold text-sm text-on-background/60">Order #{id.split('-')[0].toUpperCase()}</p>
                    <p className="text-xs text-on-background/50 mt-0.5">{formatTime(order.createdAt)}</p>
                  </div>
                  <span className={`rounded-full border px-3 py-1 text-xs font-semibold ${flow?.color || 'text-gray-600 bg-gray-50 border-gray-200'}`}>
                    {flow?.label || status}
                  </span>
                </div>

                {/* Items */}
                {items.length > 0 && (
                  <div className="mb-4 rounded-xl bg-surface-dim p-3 space-y-1">
                    {items.map((item, idx) => (
                      <div key={item.orderItemId || idx} className="flex justify-between text-sm">
                        <span>{item.quantity}× {item.menuItemName || item.name || item.menuItemId?.split('-')[0]}</span>
                        <span className="text-on-background/70">₹{item.subtotal || item.unitPriceSnapshot * item.quantity || 0}</span>
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
                  {flow?.nextStatus && (
                    <Button
                      size="sm"
                      disabled={isActioning}
                      onClick={() => handleAction(order, flow.nextStatus)}
                    >
                      {isActioning ? 'Updating...' : flow.actionLabel}
                    </Button>
                  )}
                  {ACTIVE_STATUSES.slice(0, 2).includes(status) && (
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
