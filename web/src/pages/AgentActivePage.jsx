import { useCallback, useEffect, useRef, useState } from 'react'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'
import { useNotification } from '../hooks/useNotification'
import api from '../services/api'

const DELIVERY_STATUS_FLOW = {
  PickupPending: {
    label: 'Awaiting Pickup',
    actionLabel: 'Mark Picked Up',
    nextStatus: 'PickedUp',
    color: 'text-blue-600 bg-blue-50 border-blue-200',
  },
  PickedUp: {
    label: 'Picked Up',
    actionLabel: 'Mark Out for Delivery',
    nextStatus: 'OutForDelivery',
    color: 'text-amber-600 bg-amber-50 border-amber-200',
  },
  OutForDelivery: {
    label: 'Out for Delivery',
    actionLabel: 'Mark Delivered',
    nextStatus: 'Delivered',
    color: 'text-indigo-600 bg-indigo-50 border-indigo-200',
  },
  Delivered: {
    label: 'Delivered ✓',
    actionLabel: null,
    nextStatus: null,
    color: 'text-green-600 bg-green-50 border-green-200',
  },
}

const formatTime = (iso) => {
  if (!iso) return ''
  return new Date(iso).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true })
}

export const AgentActivePage = () => {
  const { showSuccess, showError } = useNotification()
  const [deliveries, setDeliveries] = useState([])
  const [loading, setLoading] = useState(true)
  const [actioning, setActioning] = useState(null)
  const intervalRef = useRef(null)

  const fetchDeliveries = useCallback(async () => {
    setLoading(true)
    try {
      const response = await api.get('/gateway/orders/deliveries/assigned')
      const arr = Array.isArray(response.data) ? response.data : (response.data?.items || [])
      setDeliveries(arr)
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to load deliveries')
    } finally {
      setLoading(false)
    }
  }, [showError])

  useEffect(() => {
    fetchDeliveries()
    intervalRef.current = setInterval(fetchDeliveries, 30000)
    return () => clearInterval(intervalRef.current)
  }, [fetchDeliveries])

  const handleAdvance = async (delivery, nextStatus) => {
    const orderId = delivery.orderId || delivery.id
    setActioning(orderId)
    try {
      await api.put(`/gateway/orders/${orderId}/status`, {
        orderId,
        targetStatus: nextStatus,
      })
      showSuccess(`Marked as ${nextStatus}`)
      setDeliveries(prev =>
        prev.map(d => (d.orderId || d.id) === orderId
          ? { ...d, deliveryAssignment: { ...d.deliveryAssignment, currentStatus: nextStatus }, orderStatus: nextStatus }
          : d
        )
      )
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to update delivery status')
    } finally {
      setActioning(null)
    }
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">Active Deliveries</h1>
        <Button variant="secondary" onClick={fetchDeliveries}>🔄 Refresh</Button>
      </div>

      {loading ? (
        <p className="text-sm text-on-background/70">Loading deliveries...</p>
      ) : deliveries.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-outline p-10 text-center text-on-background/60">
          No active deliveries assigned to you right now.
        </div>
      ) : (
        <div className="space-y-4">
          {deliveries.map(delivery => {
            const orderId = delivery.orderId || delivery.id
            const assignment = delivery.deliveryAssignment
            const deliveryStatus = assignment?.currentStatus || delivery.orderStatus || 'PickupPending'
            const flow = DELIVERY_STATUS_FLOW[deliveryStatus] || DELIVERY_STATUS_FLOW.PickupPending
            const isActioning = actioning === orderId
            const addr = delivery.deliveryAddress
            const addrStr = addr
              ? `${addr.street || addr.addressLine1 || ''}, ${addr.city || ''} - ${addr.pincode || addr.postalCode || ''}`
              : 'Address unavailable'
            const items = delivery.items || []
            const total = delivery.total || delivery.totalAmount || 0

            return (
              <Card key={orderId} className="p-5">
                <div className="mb-4 flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="font-semibold">Order #{orderId.split('-')[0].toUpperCase()}</p>
                    <p className="text-xs text-on-background/50 mt-0.5">{formatTime(delivery.createdAt)}</p>
                  </div>
                  <span className={`rounded-full border px-3 py-1 text-xs font-semibold ${flow.color}`}>
                    {flow.label}
                  </span>
                </div>

                {/* Delivery address */}
                <div className="mb-4 rounded-xl bg-surface-dim p-3">
                  <p className="text-xs font-semibold text-on-background/60 mb-1">📍 Deliver To</p>
                  <p className="text-sm">{addrStr}</p>
                </div>

                {/* Order items summary */}
                {items.length > 0 && (
                  <div className="mb-4 text-sm space-y-1">
                    {items.map((item, idx) => (
                      <div key={item.orderItemId || idx} className="flex justify-between text-on-background/70">
                        <span>{item.quantity}× {item.menuItemName || item.name || 'Item'}</span>
                        <span>₹{item.subtotal || 0}</span>
                      </div>
                    ))}
                    <div className="flex justify-between font-semibold border-t border-outline pt-2 mt-2">
                      <span>Total (COD)</span>
                      <span>₹{total}</span>
                    </div>
                  </div>
                )}

                {/* Action button */}
                {flow.nextStatus && (
                  <Button
                    disabled={isActioning}
                    onClick={() => handleAdvance(delivery, flow.nextStatus)}
                  >
                    {isActioning ? 'Updating...' : flow.actionLabel}
                  </Button>
                )}
              </Card>
            )
          })}
        </div>
      )}

      <p className="mt-4 text-center text-xs text-on-background/50">Auto-refreshes every 30 seconds</p>
    </div>
  )
}
