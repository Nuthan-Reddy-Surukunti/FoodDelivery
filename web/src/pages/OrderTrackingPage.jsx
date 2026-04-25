import { useEffect, useMemo, useState } from 'react'
import { useParams } from 'react-router-dom'
import { OrderTracker } from '../components/organisms/OrderTracker'
import orderApi from '../services/orderApi'

// Map every possible backend OrderStatus to tracker step index
const STATUS_TO_INDEX = {
  // Step 0 — Placed
  checkoutstarted: 0,
  pending: 0,
  placed: 0,
  paid: 0,
  // Step 1 — Confirmed
  restaurantaccepted: 1,
  confirmed: 1,
  // Step 2 — Preparing
  preparing: 2,
  // Step 3 — Ready / Picked Up
  readyforpickup: 3,
  pickedup: 3,
  picked_up: 3,
  // Step 4 — Out for Delivery
  outfordelivery: 4,
  out_for_delivery: 4,
  // Step 5 — Delivered
  delivered: 5,
}

const STATUS_LABEL = {
  CheckoutStarted: 'Order Placed',
  Paid: 'Payment Confirmed',
  RestaurantAccepted: 'Restaurant Accepted',
  Preparing: 'Preparing Your Order',
  ReadyForPickup: 'Ready for Pickup',
  PickedUp: 'Picked Up by Agent',
  OutForDelivery: 'Out for Delivery',
  Delivered: 'Delivered 🎉',
  Cancelled: 'Order Cancelled',
  RestaurantRejected: 'Rejected by Restaurant',
  CancelRequestedByCustomer: 'Cancellation Requested',
}

const formatTime = (iso) => {
  if (!iso) return ''
  return new Date(iso).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true })
}

export const OrderTrackingPage = () => {
  const { orderId } = useParams()
  const [orderData, setOrderData] = useState(null)
  const [timeline, setTimeline] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!orderId) return
    let active = true

    const load = async () => {
      setLoading(true)
      setError('')
      try {
        const response = await orderApi.getOrderById(orderId)
        if (!active) return
        // Backend returns { order, timeline }
        const order = response?.order ?? response
        const tl = response?.timeline ?? []
        setOrderData(order)
        setTimeline(Array.isArray(tl) ? tl : [])
      } catch (err) {
        if (!active) return
        setError(err.response?.data?.message || err.message || 'Failed to load order tracking')
      } finally {
        if (active) setLoading(false)
      }
    }

    load()
    return () => { active = false }
  }, [orderId])

  const trackerIndex = useMemo(() => {
    const status = orderData?.orderStatus || orderData?.status || ''
    const key = status.replace(/[\s_]/g, '').toLowerCase()
    return STATUS_TO_INDEX[key] ?? 0
  }, [orderData])

  const statusLabel = STATUS_LABEL[orderData?.orderStatus] || orderData?.orderStatus || 'Pending'
  const isCancelled = ['Cancelled', 'RestaurantRejected', 'CancelRequestedByCustomer'].includes(orderData?.orderStatus)

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="mb-1 text-2xl font-bold">Track Order</h1>
      <p className="mb-5 text-sm text-on-background/60">#{orderId}</p>

      {loading && <p className="text-sm text-on-background/70">Loading tracking details...</p>}
      {error && <p className="text-sm text-error">{error}</p>}

      {!loading && !error && orderData && (
        <>
          {/* Current status banner */}
          <div className={`mb-6 rounded-2xl p-4 text-center ${isCancelled ? 'bg-red-50 border border-red-200' : 'bg-primary/5 border border-primary/20'}`}>
            <p className={`text-lg font-bold ${isCancelled ? 'text-red-600' : 'text-primary'}`}>
              {statusLabel}
            </p>
          </div>

          {/* Tracker stepper */}
          {!isCancelled && <OrderTracker currentStatusIndex={trackerIndex} />}

          {/* Order summary */}
          {orderData.items?.length > 0 && (
            <div className="mt-6 rounded-2xl border border-outline bg-surface p-4">
              <h2 className="mb-3 font-semibold">Order Summary</h2>
              <div className="space-y-1 text-sm">
                {orderData.items.map((item, idx) => (
                  <div key={item.orderItemId || idx} className="flex justify-between text-on-background/70">
                    <span>{item.quantity}× {item.menuItemName || item.name || 'Item'}</span>
                    <span>₹{item.subtotal || 0}</span>
                  </div>
                ))}
                <div className="flex justify-between font-semibold border-t border-outline mt-2 pt-2">
                  <span>Total</span>
                  <span>₹{orderData.total || orderData.totalAmount || 0}</span>
                </div>
              </div>
            </div>
          )}

          {/* Timeline */}
          {timeline.length > 0 && (
            <div className="mt-6">
              <h2 className="mb-3 font-semibold">Status Timeline</h2>
              <div className="relative space-y-4 pl-5 border-l-2 border-outline">
                {timeline.map((entry, idx) => (
                  <div key={entry.id || idx} className="relative">
                    <span className="absolute -left-[1.35rem] top-1 h-3 w-3 rounded-full bg-primary border-2 border-surface" />
                    <p className="font-medium text-sm">{STATUS_LABEL[entry.status] || entry.status}</p>
                    {entry.timestamp && (
                      <p className="text-xs text-on-background/50">{formatTime(entry.timestamp)}</p>
                    )}
                    {entry.note && (
                      <p className="text-xs text-on-background/60 mt-0.5">{entry.note}</p>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}
        </>
      )}
    </div>
  )
}
