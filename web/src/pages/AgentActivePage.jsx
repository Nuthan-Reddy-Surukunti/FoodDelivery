import { useCallback, useEffect, useRef, useState } from 'react'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'
import { useNotification } from '../hooks/useNotification'
import api from '../services/api'
import catalogApi from '../services/catalogApi'

// Backend OrderStatus enum integers
const STATUS_INT = { PickedUp: 8, OutForDelivery: 9, Delivered: 10 }

// Delivery flow keyed by DeliveryStatus string from the assignment
const FLOW = {
  PickupPending: {
    label: 'Awaiting Pickup',
    badge: 'text-blue-700 bg-blue-50 border-blue-200',
    action: 'Mark Picked Up',
    next: STATUS_INT.PickedUp,
  },
  OutForDelivery: {
    // Order gets OutForDelivery immediately on payment — agent still needs to pick it up
    label: 'Ready for Pickup',
    badge: 'text-amber-700 bg-amber-50 border-amber-200',
    action: 'Mark Picked Up',
    next: STATUS_INT.PickedUp,
  },
  PickedUp: {
    label: 'Picked Up',
    badge: 'text-indigo-700 bg-indigo-50 border-indigo-200',
    action: 'Mark Out for Delivery',
    next: STATUS_INT.OutForDelivery,
  },
  ReadyForDelivery: {
    label: 'Out for Delivery',
    badge: 'text-violet-700 bg-violet-50 border-violet-200',
    action: 'Mark Delivered',
    next: STATUS_INT.Delivered,
  },
  Delivered: {
    label: 'Delivered ✓',
    badge: 'text-green-700 bg-green-50 border-green-200',
    action: null,
    next: null,
  },
}

// Map integer OrderStatus → display flow key
const ORDER_INT_TO_FLOW = {
  4: 'PickupPending',
  7: 'PickupPending',
  8: 'PickedUp',
  9: 'ReadyForDelivery',
  10: 'Delivered',
}

const fmt = (iso) => iso
  ? new Date(iso).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true })
  : ''

const fmtDateTime = (iso) => iso
  ? new Date(iso).toLocaleString('en-IN', { dateStyle: 'short', timeStyle: 'short' })
  : ''

export const AgentActivePage = () => {
  const { showSuccess, showError } = useNotification()
  const [deliveries, setDeliveries] = useState([])
  const [loading, setLoading] = useState(true)
  const [actioning, setActioning] = useState(null)
  const intervalRef = useRef(null)

  const fetchDeliveries = useCallback(async () => {
    setLoading(true)
    try {
      // 1. Get all assigned delivery assignments
      const assignmentsRes = await api.get('/gateway/orders/deliveries/assigned')
      const assignments = Array.isArray(assignmentsRes.data)
        ? assignmentsRes.data
        : (assignmentsRes.data?.items || [])

      if (assignments.length === 0) {
        setDeliveries([])
        return
      }

      // 2. For each assignment, fetch full order + restaurant details in parallel
      const enriched = await Promise.allSettled(
        assignments.map(async (assignment) => {
          const orderId = assignment.orderId || assignment.OrderId

          const [orderResult, restaurantResult] = await Promise.allSettled([
            api.get(`/gateway/orders/${orderId}`),
            // We need restaurantId first — try to get it from the order fetch
          ])

          const orderData = orderResult.status === 'fulfilled'
            ? (orderResult.value.data?.order ?? orderResult.value.data)
            : null

          // Now fetch restaurant using restaurantId from the order
          let restaurantData = null
          if (orderData?.restaurantId) {
            const restResult = await catalogApi.getRestaurantById(orderData.restaurantId)
              .catch(() => null)
            restaurantData = restResult
          }

          return { assignment, order: orderData, restaurant: restaurantData }
        })
      )

      setDeliveries(
        enriched
          .filter(r => r.status === 'fulfilled')
          .map(r => r.value)
      )
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

  const handleAdvance = async (orderId, assignment, nextInt, nextLabel) => {
    setActioning(orderId)
    try {
      await api.put(`/gateway/orders/${orderId}/status`, {
        orderId,
        targetStatus: nextInt,
      })
      showSuccess(`✓ ${nextLabel}`)
      // Map nextInt back to a delivery status string for optimistic update
      const newDeliveryStatus = nextInt === STATUS_INT.PickedUp ? 'PickedUp'
        : nextInt === STATUS_INT.OutForDelivery ? 'ReadyForDelivery'
        : 'Delivered'
      setDeliveries(prev => prev.map(d => {
        const dId = d.assignment.orderId || d.assignment.OrderId
        if (dId !== orderId) return d
        return {
          ...d,
          assignment: {
            ...d.assignment,
            currentStatus: newDeliveryStatus,
            CurrentStatus: newDeliveryStatus,
          },
          order: d.order ? { ...d.order, orderStatus: nextInt } : d.order,
        }
      }))
    } catch (err) {
      showError(err.response?.data?.message || 'Status update failed')
    } finally {
      setActioning(null)
    }
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">My Deliveries</h1>
        <Button variant="secondary" onClick={fetchDeliveries} disabled={loading}>
          🔄 Refresh
        </Button>
      </div>

      {loading ? (
        <div className="space-y-4">
          {[1, 2].map(i => (
            <div key={i} className="h-64 animate-pulse rounded-2xl border border-outline bg-surface-dim" />
          ))}
        </div>
      ) : deliveries.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-outline p-12 text-center">
          <p className="text-3xl mb-2">🛵</p>
          <p className="font-semibold">No active deliveries</p>
          <p className="text-sm text-on-background/60 mt-1">You'll see new assignments here as they arrive.</p>
        </div>
      ) : (
        <div className="space-y-5">
          {deliveries.map(({ assignment, order, restaurant }) => {
            const orderId = assignment.orderId || assignment.OrderId
            const isActioning = actioning === orderId

            // Resolve flow
            const deliveryStatusRaw = assignment.currentStatus || assignment.CurrentStatus || ''
            const orderStatusInt = typeof order?.orderStatus === 'number' ? order.orderStatus : null
            const flowKey = FLOW[deliveryStatusRaw]
              ? deliveryStatusRaw
              : (orderStatusInt !== null ? ORDER_INT_TO_FLOW[orderStatusInt] : 'PickupPending')
            const flow = FLOW[flowKey] || FLOW.PickupPending

            // Build item name lookup from restaurant menu
            const menuItemMap = {}
            if (restaurant?.menuItems) {
              restaurant.menuItems.forEach(mi => {
                menuItemMap[mi.id?.toLowerCase()] = mi
              })
            }

            // Delivery address
            const addr = order?.deliveryAddress
            const addressLines = addr
              ? [addr.street, addr.city, addr.pincode].filter(Boolean)
              : []

            // Order items enriched with names
            const items = (order?.items || []).map(item => {
              const menuItem = menuItemMap[item.menuItemId?.toLowerCase()]
              return {
                ...item,
                resolvedName: menuItem?.name || `Item`,
                isVeg: menuItem?.isVeg ?? null,
              }
            })

            const total = order?.total ?? order?.totalAmount ?? 0

            return (
              <Card key={orderId} className="overflow-hidden p-0">
                {/* ── Status banner ── */}
                <div className={`px-5 py-3 flex items-center justify-between ${flow.badge} border-b border-inherit`}>
                  <span className="font-semibold text-sm">{flow.label}</span>
                  <span className="text-xs opacity-70">
                    Assigned {fmt(assignment.assignedAt || assignment.AssignedAt)}
                  </span>
                </div>

                <div className="p-5 space-y-5">
                  {/* ── Pick Up From ── */}
                  {restaurant ? (
                    <section>
                      <p className="text-xs font-semibold uppercase tracking-wide text-on-background/50 mb-2">
                        🏪 Pick Up From
                      </p>
                      <p className="font-bold text-base">{restaurant.name}</p>
                      {restaurant.address && (
                        <p className="text-sm text-on-background/70 mt-0.5">
                          {restaurant.address}{restaurant.city ? `, ${restaurant.city}` : ''}
                        </p>
                      )}
                      <div className="mt-2 flex flex-wrap gap-3">
                        {restaurant.contactPhone && (
                          <a
                            href={`tel:${restaurant.contactPhone}`}
                            className="inline-flex items-center gap-1.5 rounded-lg bg-surface-dim border border-outline px-3 py-1.5 text-sm font-medium hover:border-primary transition"
                          >
                            📞 {restaurant.contactPhone}
                          </a>
                        )}
                      </div>
                    </section>
                  ) : (
                    <section>
                      <p className="text-xs font-semibold uppercase tracking-wide text-on-background/50 mb-1">🏪 Pick Up From</p>
                      <p className="text-sm text-on-background/50">Restaurant info not available</p>
                    </section>
                  )}

                  <div className="border-t border-outline" />

                  {/* ── Order Items ── */}
                  <section>
                    <p className="text-xs font-semibold uppercase tracking-wide text-on-background/50 mb-2">
                      🧾 Order Items
                    </p>
                    <div className="space-y-2">
                      {items.length > 0 ? items.map((item, idx) => (
                        <div key={item.orderItemId || idx} className="flex items-center justify-between gap-2 text-sm">
                          <div className="flex items-center gap-2 min-w-0">
                            {/* Veg / Non-veg indicator */}
                            {item.isVeg !== null && (
                              <span
                                className={`shrink-0 h-3.5 w-3.5 rounded-sm border-2 flex items-center justify-center ${item.isVeg ? 'border-green-600' : 'border-red-600'}`}
                              >
                                <span className={`h-1.5 w-1.5 rounded-full ${item.isVeg ? 'bg-green-600' : 'bg-red-600'}`} />
                              </span>
                            )}
                            <span className="font-medium">{item.quantity}×</span>
                            <span className="truncate">{item.resolvedName}</span>
                          </div>
                          <span className="shrink-0 text-on-background/60">
                            ₹{(item.subtotal ?? item.unitPriceSnapshot * item.quantity ?? 0).toFixed(2)}
                          </span>
                        </div>
                      )) : (
                        <p className="text-sm text-on-background/50">Items unavailable</p>
                      )}
                    </div>

                    {/* COD Total — big and prominent */}
                    <div className="mt-3 flex items-center justify-between rounded-xl bg-surface-dim px-4 py-3 border border-outline">
                      <div>
                        <p className="text-xs text-on-background/60">Collect on Delivery (COD)</p>
                        <p className="text-2xl font-bold mt-0.5">₹{Number(total).toFixed(2)}</p>
                      </div>
                      <span className="text-3xl">💵</span>
                    </div>
                  </section>

                  <div className="border-t border-outline" />

                  {/* ── Deliver To ── */}
                  <section>
                    <p className="text-xs font-semibold uppercase tracking-wide text-on-background/50 mb-2">
                      📍 Deliver To
                    </p>
                    {addressLines.length > 0 ? (
                      <>
                        <p className="font-medium">{addressLines[0]}</p>
                        {addressLines.slice(1).map((line, i) => (
                          <p key={i} className="text-sm text-on-background/70">{line}</p>
                        ))}
                        {addr?.latitude && addr?.longitude && (
                          <a
                            href={`https://maps.google.com/?q=${addr.latitude},${addr.longitude}`}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="mt-2 inline-flex items-center gap-1.5 rounded-lg bg-surface-dim border border-outline px-3 py-1.5 text-sm font-medium hover:border-primary transition"
                          >
                            🗺️ Open in Maps
                          </a>
                        )}
                      </>
                    ) : (
                      <p className="text-sm text-on-background/50">Address not available</p>
                    )}
                  </section>

                  {/* ── Notes ── */}
                  {items.some(i => i.customizationNotes) && (
                    <>
                      <div className="border-t border-outline" />
                      <section>
                        <p className="text-xs font-semibold uppercase tracking-wide text-on-background/50 mb-1">
                          📝 Notes
                        </p>
                        {items.filter(i => i.customizationNotes).map((item, idx) => (
                          <p key={idx} className="text-sm text-on-background/70">
                            {item.resolvedName}: {item.customizationNotes}
                          </p>
                        ))}
                      </section>
                    </>
                  )}

                  {/* ── Action button ── */}
                  {flow.next !== null && (
                    <Button
                      className="w-full"
                      disabled={isActioning}
                      onClick={() => handleAdvance(orderId, assignment, flow.next, flow.action)}
                    >
                      {isActioning ? 'Updating...' : flow.action}
                    </Button>
                  )}

                  {/* Order timestamp footer */}
                  <p className="text-center text-xs text-on-background/40">
                    Order placed {fmtDateTime(order?.createdAt)}
                    {' · '}
                    #{String(orderId).split('-')[0].toUpperCase()}
                  </p>
                </div>
              </Card>
            )
          })}
        </div>
      )}

      <p className="mt-6 text-center text-xs text-on-background/40">Auto-refreshes every 30 seconds</p>
    </div>
  )
}
