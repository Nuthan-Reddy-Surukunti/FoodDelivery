import { useCallback, useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'
import { PartnerLayout } from '../components/organisms/PartnerLayout'
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
    return (
      <PartnerLayout title="Order Queue">
        <div className="space-y-4">
          {[1,2,3].map(i => <div key={i} className="h-36 bg-slate-200 animate-pulse rounded-xl" />)}
        </div>
      </PartnerLayout>
    )
  }

  if (!restaurant) {
    return (
      <PartnerLayout title="Order Queue">
        <div className="bg-amber-50 border border-amber-200 rounded-xl p-5 text-amber-800 text-sm">
          No restaurant found. <Link to="/partner/dashboard" className="text-primary font-semibold">Complete setup →</Link>
        </div>
      </PartnerLayout>
    )
  }

  return (
    <PartnerLayout title={`Order Queue — ${restaurant.name}`}>
      {/* Refresh header */}
      <div className="flex justify-end">
        <button
          onClick={fetchOrders}
          className="flex items-center gap-2 text-sm font-medium text-primary hover:text-primary-container transition-colors border border-primary/30 px-4 py-2 rounded-xl"
        >
          <span className="material-symbols-outlined text-base">refresh</span>
          Refresh
        </button>
      </div>

      {/* Orders */}
      {loading ? (
        <div className="space-y-4">
          {[1,2,3].map(i => <div key={i} className="h-36 bg-slate-200 animate-pulse rounded-xl" />)}
        </div>
      ) : orders.length === 0 ? (
        <div className="bg-white border border-dashed border-slate-300 rounded-xl p-12 text-center">
          <p className="text-4xl mb-3">📋</p>
          <p className="text-lg font-semibold text-on-surface">No active orders</p>
          <p className="text-sm text-on-surface-variant mt-1">Auto-refreshes every 30 seconds</p>
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map(order => {
            const id = order.orderId || order.id
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
              <div key={id} className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden hover:border-primary/30 transition-colors">
                <div className="flex flex-wrap items-center justify-between gap-3 p-5 border-b border-slate-100">
                  <div className="flex items-center gap-3">
                    <div className="bg-slate-100 h-11 w-11 rounded-lg flex items-center justify-center border border-slate-200 text-xs font-bold text-slate-500">
                      #{String(id).split('-')[0].slice(0, 3).toUpperCase()}
                    </div>
                    <div>
                      <p className="font-semibold text-on-surface text-sm">
                        {order.customerName || order.customerEmail || 'Customer'}
                      </p>
                      <p className="text-xs text-on-surface-variant mt-0.5">{formatTime(order.createdAt)}</p>
                    </div>
                  </div>
                  <span className={`rounded-full border px-3 py-1 text-xs font-semibold ${flow?.color || 'text-gray-600 bg-gray-50 border-gray-200'}`}>
                    {flow?.label || String(statusKey)}
                  </span>
                </div>

                <div className="p-5">
                  {/* Items */}
                  {items.length > 0 && (
                    <div className="mb-4 bg-slate-50 rounded-xl p-4 space-y-1.5">
                      {items.map((item, idx) => (
                        <div key={item.orderItemId || idx} className="flex justify-between text-sm">
                          <span className="text-on-surface">{item.quantity}× {item.menuItemName || item.name || item.menuItemId?.split('-')[0]}</span>
                          <span className="font-medium text-on-surface">₹{item.subtotal || (item.unitPriceSnapshot * item.quantity) || 0}</span>
                        </div>
                      ))}
                      <div className="border-t border-slate-200 mt-2 pt-2 flex justify-between font-semibold text-sm">
                        <span>Total</span><span>₹{total}</span>
                      </div>
                    </div>
                  )}

                  {/* Address */}
                  <p className="text-xs text-on-surface-variant mb-4 flex items-center gap-1">
                    <span className="material-symbols-outlined text-sm">location_on</span>{addrStr}
                  </p>

                  {/* Actions */}
                  <div className="flex flex-wrap gap-2">
                    {flow?.nextStatus != null && (
                      <button
                        disabled={isActioning}
                        onClick={() => handleAction(order, flow.nextStatus)}
                        className="bg-primary text-on-primary text-sm font-semibold px-5 py-2.5 rounded-xl hover:bg-primary-container transition-colors disabled:opacity-50"
                      >
                        {isActioning ? 'Updating...' : flow.actionLabel}
                      </button>
                    )}
                    {flow?.canReject && (
                      <button
                        disabled={isActioning}
                        onClick={() => handleReject(order)}
                        className="border border-red-300 text-red-600 text-sm font-semibold px-5 py-2.5 rounded-xl hover:bg-red-50 transition-colors disabled:opacity-50"
                      >
                        Reject
                      </button>
                    )}
                  </div>
                </div>
              </div>
            )
          })}
        </div>
      )}

      <p className="text-center text-xs text-on-surface-variant">Auto-refreshes every 30 seconds</p>
    </PartnerLayout>
  )
}
