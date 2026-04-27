import { useCallback, useEffect, useRef, useState } from 'react'
import { AgentLayout } from '../components/organisms/AgentLayout'
import { useNotification } from '../hooks/useNotification'
import api from '../services/api'
import catalogApi from '../services/catalogApi'

// Backend OrderStatus enum integers — must match OrderService.Domain.Enums.OrderStatus exactly
const STATUS_INT = { ReadyForPickup: 7, PickedUp: 8, OutForDelivery: 9, Delivered: 10 }

// Delivery flow keyed by order status integer (source of truth from backend)
const FLOW = {
  // Order is ready — agent must pick up from restaurant
  [STATUS_INT.ReadyForPickup]: {
    label: 'Ready for Pickup',
    badge: 'text-amber-700 bg-amber-50 border-amber-200',
    action: 'Mark Picked Up',
    next: STATUS_INT.PickedUp,
  },
  // Agent has picked up the order from restaurant
  [STATUS_INT.PickedUp]: {
    label: 'Picked Up ✓',
    badge: 'text-indigo-700 bg-indigo-50 border-indigo-200',
    action: 'Mark Out for Delivery',
    next: STATUS_INT.OutForDelivery,
  },
  // Agent is en route to customer
  [STATUS_INT.OutForDelivery]: {
    label: 'Out for Delivery',
    badge: 'text-violet-700 bg-violet-50 border-violet-200',
    action: 'Mark Delivered',
    next: STATUS_INT.Delivered,
  },
  // Terminal state
  [STATUS_INT.Delivered]: {
    label: 'Delivered ✓',
    badge: 'text-green-700 bg-green-50 border-green-200',
    action: null,
    next: null,
  },
  
  // String fallbacks because backend serializes enums as strings
  ReadyForPickup: {
    label: 'Ready for Pickup',
    badge: 'text-amber-700 bg-amber-50 border-amber-200',
    action: 'Mark Picked Up',
    next: STATUS_INT.PickedUp,
  },
  PickedUp: {
    label: 'Picked Up ✓',
    badge: 'text-indigo-700 bg-indigo-50 border-indigo-200',
    action: 'Mark Out for Delivery',
    next: STATUS_INT.OutForDelivery,
  },
  OutForDelivery: {
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

// Fallback for unexpected states
const DEFAULT_FLOW = {
  label: 'Awaiting Pickup',
  badge: 'text-blue-700 bg-blue-50 border-blue-200',
  action: null,
  next: null,
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

  const handleAdvance = async (orderId, nextInt, nextLabel) => {
    setActioning(orderId)
    try {
      await api.put(`/gateway/orders/${orderId}/status`, {
        orderId,
        targetStatus: nextInt,
      })
      showSuccess(`✓ ${nextLabel}`)
      // Optimistic update: set orderStatus to the new integer so the FLOW map resolves instantly
      setDeliveries(prev => prev.map(d => {
        const dId = d.assignment.orderId || d.assignment.OrderId
        if (dId !== orderId) return d
        return {
          ...d,
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
    <AgentLayout title="Active Deliveries">
      {/* Refresh */}
      <div className="flex justify-end">
        <button
          onClick={fetchDeliveries}
          disabled={loading}
          className="flex items-center gap-2 text-sm font-medium text-primary border border-primary/30 px-4 py-2 rounded-xl hover:bg-primary/5 transition-colors disabled:opacity-50"
        >
          <span className="material-symbols-outlined text-base">refresh</span>
          Refresh
        </button>
      </div>

      {loading ? (
        <div className="space-y-4">
          {[1, 2].map(i => (
            <div key={i} className="h-64 animate-pulse rounded-xl bg-slate-200" />
          ))}
        </div>
      ) : deliveries.length === 0 ? (
        <div className="bg-white border border-dashed border-slate-300 rounded-xl p-12 text-center">
          <p className="text-5xl mb-3">🛵</p>
          <p className="text-lg font-semibold text-on-surface">No active deliveries</p>
          <p className="text-sm text-on-surface-variant mt-1">You'll see new assignments here as they arrive.</p>
        </div>
      ) : (
        <div className="space-y-5">
          {deliveries.map(({ assignment, order, restaurant }) => {
            const orderId = assignment.orderId || assignment.OrderId
            const isActioning = actioning === orderId

            // Resolve flow using orderStatus integer or string as the single source of truth
            const statusKey = order?.orderStatus
            const flow = (statusKey !== null && statusKey !== undefined ? FLOW[statusKey] : null) || DEFAULT_FLOW

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
              <div key={orderId} className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden hover:border-primary/30 transition-colors">
                {/* ── Status banner ── */}
                <div className={`px-5 py-3 flex items-center justify-between border-b ${flow.badge}`}>
                  <span className="font-semibold text-sm">{flow.label}</span>
                  <span className="text-xs opacity-70">
                    Assigned {fmt(assignment.assignedAt || assignment.AssignedAt)}
                  </span>
                </div>

                <div className="p-5 space-y-5">
                  {/* ── Pick Up From ── */}
                  {restaurant ? (
                    <section>
                      <p className="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-2 flex items-center gap-1">
                        <span className="material-symbols-outlined text-sm">storefront</span> Pick Up From
                      </p>
                      <p className="font-bold text-base text-on-surface">{restaurant.name}</p>
                      {restaurant.address && (
                        <p className="text-sm text-on-surface-variant mt-0.5">
                          {restaurant.address}{restaurant.city ? `, ${restaurant.city}` : ''}
                        </p>
                      )}
                      {restaurant.contactPhone && (
                        <a
                          href={`tel:${restaurant.contactPhone}`}
                          className="mt-2 inline-flex items-center gap-1.5 bg-slate-50 border border-slate-200 rounded-lg px-3 py-1.5 text-sm font-medium hover:border-primary transition"
                        >
                          <span className="material-symbols-outlined text-sm">call</span>
                          {restaurant.contactPhone}
                        </a>
                      )}
                    </section>
                  ) : (
                    <section>
                      <p className="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-1">Pick Up From</p>
                      <p className="text-sm text-on-surface-variant">Restaurant info not available</p>
                    </section>
                  )}

                  <div className="border-t border-slate-100" />

                  {/* ── Order Items ── */}
                  <section>
                    <p className="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-2 flex items-center gap-1">
                      <span className="material-symbols-outlined text-sm">receipt_long</span> Order Items
                    </p>
                    <div className="bg-slate-50 rounded-xl p-4 space-y-2">
                      {items.length > 0 ? items.map((item, idx) => (
                        <div key={item.orderItemId || idx} className="flex items-center justify-between gap-2 text-sm">
                          <div className="flex items-center gap-2 min-w-0">
                            {item.isVeg !== null && (
                              <span className={`shrink-0 h-3.5 w-3.5 rounded-sm border-2 flex items-center justify-center ${item.isVeg ? 'border-green-600' : 'border-red-600'}`}>
                                <span className={`h-1.5 w-1.5 rounded-full ${item.isVeg ? 'bg-green-600' : 'bg-red-600'}`} />
                              </span>
                            )}
                            <span className="font-medium text-on-surface">{item.quantity}×</span>
                            <span className="truncate text-on-surface">{item.resolvedName}</span>
                          </div>
                          <span className="shrink-0 font-medium text-on-surface">
                            ₹{(item.subtotal ?? item.unitPriceSnapshot * item.quantity ?? 0).toFixed(2)}
                          </span>
                        </div>
                      )) : (
                        <p className="text-sm text-on-surface-variant">Items unavailable</p>
                      )}
                    </div>

                    {/* Payment Status Display */}
                    {(() => {
                      const method = order?.payment?.paymentMethod || 'CashOnDelivery'
                      const isCod = method === 'CashOnDelivery' || method === 3
                      const paymentLabel = isCod 
                        ? 'Collect on Delivery (COD)' 
                        : (method === 'Card' || method === 2 ? 'Paid Online (Card)' : 'Paid Online (Wallet)')
                      
                      const paymentIcon = isCod ? 'payments' : 'credit_score'
                      const bgClass = isCod ? 'bg-primary/5 border-primary/20' : 'bg-green-50 border-green-200'
                      const textClass = isCod ? 'text-primary' : 'text-green-700'
                      const titleClass = isCod ? 'text-primary/80' : 'text-green-700/80'

                      return (
                        <div className={`mt-3 flex items-center justify-between rounded-xl border px-4 py-3 ${bgClass}`}>
                          <div>
                            <p className={`text-xs font-medium ${titleClass}`}>{paymentLabel}</p>
                            <p className="text-2xl font-bold text-on-surface mt-0.5">₹{Number(total).toFixed(2)}</p>
                          </div>
                          <span className={`material-symbols-outlined text-3xl ${textClass}`} style={{ fontVariationSettings: "'FILL' 1" }}>
                            {paymentIcon}
                          </span>
                        </div>
                      )
                    })()}
                  </section>

                  <div className="border-t border-slate-100" />

                  {/* ── Deliver To ── */}
                  <section>
                    <p className="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-2 flex items-center gap-1">
                      <span className="material-symbols-outlined text-sm">location_on</span> Deliver To
                    </p>
                    {addressLines.length > 0 ? (
                      <>
                        <p className="font-semibold text-on-surface">{addressLines[0]}</p>
                        {addressLines.slice(1).map((line, i) => (
                          <p key={i} className="text-sm text-on-surface-variant">{line}</p>
                        ))}
                        {addr?.latitude && addr?.longitude && (
                          <a
                            href={`https://maps.google.com/?q=${addr.latitude},${addr.longitude}`}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="mt-2 inline-flex items-center gap-1.5 bg-slate-50 border border-slate-200 rounded-lg px-3 py-1.5 text-sm font-medium hover:border-primary transition"
                          >
                            <span className="material-symbols-outlined text-sm">map</span>
                            Open in Maps
                          </a>
                        )}
                      </>
                    ) : (
                      <p className="text-sm text-on-surface-variant">Address not available</p>
                    )}
                  </section>

                  {/* ── Notes ── */}
                  {items.some(i => i.customizationNotes) && (
                    <>
                      <div className="border-t border-slate-100" />
                      <section>
                        <p className="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-1 flex items-center gap-1">
                          <span className="material-symbols-outlined text-sm">edit_note</span> Notes
                        </p>
                        {items.filter(i => i.customizationNotes).map((item, idx) => (
                          <p key={idx} className="text-sm text-on-surface-variant">
                            {item.resolvedName}: {item.customizationNotes}
                          </p>
                        ))}
                      </section>
                    </>
                  )}

                  {/* ── Action button ── */}
                  {flow.next !== null && (
                    <button
                      disabled={isActioning}
                      onClick={() => handleAdvance(orderId, flow.next, flow.action)}
                      className="w-full bg-primary text-on-primary py-3 rounded-xl text-sm font-semibold hover:bg-primary-container transition-colors disabled:opacity-50 flex items-center justify-center gap-2"
                    >
                      <span className="material-symbols-outlined text-base">check_circle</span>
                      {isActioning ? 'Updating...' : flow.action}
                    </button>
                  )}

                  {/* Order footer */}
                  <p className="text-center text-xs text-slate-400">
                    Order placed {fmtDateTime(order?.createdAt)} · #{String(orderId).split('-')[0].toUpperCase()}
                  </p>
                </div>
              </div>
            )
          })}
        </div>
      )}

      <p className="text-center text-xs text-slate-400">Auto-refreshes every 30 seconds</p>
    </AgentLayout>
  )
}
