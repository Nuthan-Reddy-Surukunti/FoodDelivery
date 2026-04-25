import { useCallback, useEffect, useRef, useState } from 'react'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'
import { useNotification } from '../hooks/useNotification'
import api from '../services/api'

// Backend OrderStatus enum integers — must match OrderService.Domain.Enums.OrderStatus exactly
const ORDER_STATUS_INT = {
  PickedUp: 8,
  OutForDelivery: 9,
  Delivered: 10,
}

// What statuses a delivery agent can transition TO
const DELIVERY_STATUS_FLOW = {
  // After payment, assignment is PickupPending → agent should pick up
  PickupPending: {
    label: 'Awaiting Pickup',
    actionLabel: 'Mark Picked Up',
    nextStatusStr: 'PickedUp',
    nextStatusInt: ORDER_STATUS_INT.PickedUp,
    color: 'text-blue-600 bg-blue-50 border-blue-200',
  },
  // Order is assigned but OrderStatus is already OutForDelivery from backend payment flow
  OutForDelivery: {
    label: 'Out for Delivery',
    actionLabel: 'Mark Picked Up',
    nextStatusStr: 'PickedUp',
    nextStatusInt: ORDER_STATUS_INT.PickedUp,
    color: 'text-amber-600 bg-amber-50 border-amber-200',
  },
  PickedUp: {
    label: 'Picked Up',
    actionLabel: 'Mark Out for Delivery',
    nextStatusStr: 'OutForDelivery',
    nextStatusInt: ORDER_STATUS_INT.OutForDelivery,
    color: 'text-indigo-600 bg-indigo-50 border-indigo-200',
  },
  ReadyForDelivery: {
    label: 'Out for Delivery',
    actionLabel: 'Mark Delivered',
    nextStatusStr: 'Delivered',
    nextStatusInt: ORDER_STATUS_INT.Delivered,
    color: 'text-cyan-600 bg-cyan-50 border-cyan-200',
  },
  Delivered: {
    label: 'Delivered ✓',
    actionLabel: null,
    nextStatusStr: null,
    nextStatusInt: null,
    color: 'text-green-600 bg-green-50 border-green-200',
  },
}

// Map DeliveryStatus string → flow key
const DELIVERY_STATUS_TO_FLOW_KEY = {
  PickupPending: 'PickupPending',
  PickedUp: 'PickedUp',
  OutForDelivery: 'ReadyForDelivery',
  Delivered: 'Delivered',
}

// Map OrderStatus integer → flow key for display when assignment status not available
const ORDER_STATUS_INT_TO_FLOW_KEY = {
  4: 'PickupPending',   // Paid
  7: 'PickupPending',   // ReadyForPickup
  8: 'PickedUp',        // PickedUp
  9: 'ReadyForDelivery',// OutForDelivery
  10: 'Delivered',      // Delivered
}

const formatTime = (iso) => {
  if (!iso) return ''
  return new Date(iso).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true })
}

export const AgentActivePage = () => {
  const { showSuccess, showError } = useNotification()
  const [deliveries, setDeliveries] = useState([])  // enriched with full order data
  const [loading, setLoading] = useState(true)
  const [actioning, setActioning] = useState(null)
  const intervalRef = useRef(null)

  const fetchDeliveries = useCallback(async () => {
    setLoading(true)
    try {
      // Step 1: Get assigned delivery assignments (has OrderId but no address/items)
      const assignmentsRes = await api.get('/gateway/orders/deliveries/assigned')
      const assignments = Array.isArray(assignmentsRes.data)
        ? assignmentsRes.data
        : (assignmentsRes.data?.items || [])

      if (assignments.length === 0) {
        setDeliveries([])
        return
      }

      // Step 2: Fetch full order detail for each assignment to get address + items
      const enriched = await Promise.allSettled(
        assignments.map(async (assignment) => {
          const orderId = assignment.orderId || assignment.OrderId
          try {
            const orderRes = await api.get(`/gateway/orders/${orderId}`)
            const order = orderRes.data?.order ?? orderRes.data
            return { assignment, order }
          } catch {
            // If order fetch fails, still show the assignment with limited data
            return { assignment, order: null }
          }
        })
      )

      const results = enriched
        .filter(r => r.status === 'fulfilled')
        .map(r => r.value)

      setDeliveries(results)
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

  const handleAdvance = async (assignment, nextStatusStr, nextStatusInt) => {
    const orderId = assignment.orderId || assignment.OrderId
    setActioning(orderId)
    try {
      // Send TargetStatus as integer — backend expects OrderStatus enum value
      await api.put(`/gateway/orders/${orderId}/status`, {
        orderId,
        targetStatus: nextStatusInt,
      })
      showSuccess(`Marked as ${nextStatusStr}`)

      // Optimistically update local state
      setDeliveries(prev => prev.map(d => {
        const dId = d.assignment.orderId || d.assignment.OrderId
        if (dId !== orderId) return d
        return {
          ...d,
          assignment: { ...d.assignment, currentStatus: nextStatusStr, CurrentStatus: nextStatusStr },
          order: d.order ? { ...d.order, orderStatus: nextStatusInt } : d.order,
        }
      }))
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
        <div className="space-y-4">
          {[1, 2].map(i => (
            <div key={i} className="h-48 rounded-2xl border border-outline bg-surface-dim animate-pulse" />
          ))}
        </div>
      ) : deliveries.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-outline p-10 text-center text-on-background/60">
          No active deliveries assigned to you right now.
        </div>
      ) : (
        <div className="space-y-4">
          {deliveries.map(({ assignment, order }) => {
            const orderId = assignment.orderId || assignment.OrderId
            const isActioning = actioning === orderId

            // Determine current status from delivery assignment first, fallback to order status
            const deliveryStatusRaw = assignment.currentStatus || assignment.CurrentStatus || ''
            const orderStatusInt = typeof order?.orderStatus === 'number' ? order.orderStatus : null

            const flowKey = DELIVERY_STATUS_TO_FLOW_KEY[deliveryStatusRaw]
              || (orderStatusInt !== null ? ORDER_STATUS_INT_TO_FLOW_KEY[orderStatusInt] : null)
              || 'PickupPending'

            const flow = DELIVERY_STATUS_FLOW[flowKey]

            // Address from full order response
            const addr = order?.deliveryAddress
            const addrStr = addr
              ? [addr.street, addr.city, addr.pincode].filter(Boolean).join(', ')
              : 'Address not available'

            // Items from full order
            const items = order?.items || []
            const total = order?.total ?? order?.totalAmount ?? 0

            return (
              <Card key={orderId} className="p-5">
                {/* Header */}
                <div className="mb-4 flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="font-semibold">
                      Order #{(orderId || '').split('-')[0].toUpperCase()}
                    </p>
                    <p className="text-xs text-on-background/50 mt-0.5">
                      Assigned: {formatTime(assignment.assignedAt || assignment.AssignedAt)}
                    </p>
                  </div>
                  <span className={`rounded-full border px-3 py-1 text-xs font-semibold ${flow.color}`}>
                    {flow.label}
                  </span>
                </div>

                {/* Delivery Address */}
                <div className="mb-4 rounded-xl bg-surface-dim p-3">
                  <p className="text-xs font-semibold text-on-background/60 mb-1">📍 Deliver To</p>
                  <p className="text-sm">{addrStr}</p>
                </div>

                {/* Order Items */}
                {items.length > 0 ? (
                  <div className="mb-4 space-y-1 text-sm">
                    {items.map((item, idx) => (
                      <div key={item.orderItemId || idx} className="flex justify-between text-on-background/70">
                        <span>{item.quantity}× {item.menuItemName || item.name || 'Item'}</span>
                        <span>₹{item.subtotal ?? (item.unitPriceSnapshot * item.quantity) ?? 0}</span>
                      </div>
                    ))}
                    <div className="flex justify-between font-semibold border-t border-outline pt-2 mt-2">
                      <span>Total (COD)</span>
                      <span>₹{total}</span>
                    </div>
                  </div>
                ) : (
                  <p className="mb-4 text-xs text-on-background/50">Order details loading...</p>
                )}

                {/* Action button */}
                {flow.nextStatusStr && (
                  <Button
                    disabled={isActioning}
                    onClick={() => handleAdvance(assignment, flow.nextStatusStr, flow.nextStatusInt)}
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
