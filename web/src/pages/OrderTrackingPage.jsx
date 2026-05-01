import { useEffect, useMemo, useRef, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import orderApi from '../services/orderApi'
import catalogApi from '../services/catalogApi'

// ── Status mappings ────────────────────────────────────────────────────────────

// Map status string/int → step index (0–3)
const STATUS_TO_STEP = {
  checkoutstarted: 0, 1: 0,
  paid: 0, 4: 0,
  restaurantaccepted: 1, 5: 1,
  preparing: 1, 6: 1,
  readyforpickup: 1, 7: 1,
  pickedup: 2, 8: 2,
  outfordelivery: 2, 9: 2,
  delivered: 3, 10: 3,
}

const STEPS = [
  { label: 'Order Placed', desc: 'QuickBites' },
  { label: 'Preparing your food', desc: 'Chef is packing your items' },
  { label: 'Out for Delivery', desc: 'Driver is on the way' },
  { label: 'Delivered', desc: 'Delivered to you' },
]

// Toast messages per step transition
const STEP_TOASTS = {
  1: { emoji: '🎉', message: 'Restaurant accepted your order!' },
  2: { emoji: '👨‍🍳', message: 'The restaurant is preparing your food!' },
  3: { emoji: '🛵', message: 'Delivery agent picked up your order!' },
  4: { emoji: '🎊', message: 'Order delivered! Enjoy your meal!' },
}

const STATUS_LABEL = {
  CheckoutStarted: 'Order Placed', Paid: 'Payment Confirmed',
  RestaurantAccepted: 'Restaurant Accepted', Preparing: 'Preparing Your Order',
  ReadyForPickup: 'Ready for Pickup', PickedUp: 'Picked Up by Agent',
  OutForDelivery: 'Out for Delivery', Delivered: 'Delivered 🎉',
  Cancelled: 'Order Cancelled', RestaurantRejected: 'Rejected by Restaurant',
  CancelRequestedByCustomer: 'Cancellation Requested',
}

const PAYMENT_METHOD_LABEL = { 
  1: 'Online Payment (Razorpay)', Online: 'Online Payment (Razorpay)',
  2: 'Cash on Delivery', CashOnDelivery: 'Cash on Delivery' 
}
const PAYMENT_METHOD_ICON  = { 
  1: 'account_balance_wallet', Online: 'account_balance_wallet',
  2: 'payments', CashOnDelivery: 'payments' 
}

const fmtTime = (iso) =>
  iso ? new Date(iso).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true }) : ''

const fmtDate = (iso) =>
  iso ? new Date(iso).toLocaleString('en-IN', { dateStyle: 'medium', timeStyle: 'short' }) : ''

const fmtEta = (iso) => {
  if (!iso) return ''
  const date = new Date(iso)
  const now = new Date()
  const isToday = date.toDateString() === now.toDateString()
  const time = date.toLocaleTimeString('en-IN', { hour: 'numeric', minute: '2-digit', hour12: true })
  if (isToday) return `Today, ${time}`
  const day = date.toLocaleDateString('en-IN', { month: 'short', day: 'numeric' })
  return `${day}, ${time}`
}

const isGenericItemName = (name) => /^item\s*\d+$/i.test((name || '').trim())

const resolveItemName = (item, menuNameMap = {}) => {
  const directCandidates = [item.menuItemName, item.name, item.itemName, item.title]
  const directName = directCandidates.find((value) => value && !isGenericItemName(value))
  if (directName) return directName

  const key = String(item.menuItemId || item.id || '')
  return menuNameMap[key] || 'Menu Item'
}

// ── Toast component ────────────────────────────────────────────────────────────
const StatusToast = ({ toast, onDismiss }) => {
  useEffect(() => {
    const t = setTimeout(onDismiss, 4000)
    return () => clearTimeout(t)
  }, [onDismiss])

  return (
    <div className="fixed top-6 left-1/2 -translate-x-1/2 z-50 animate-[slideDown_0.4s_ease-out]"
         style={{ animation: 'slideDown 0.4s ease-out' }}>
      <div className="flex items-center gap-3 bg-white border border-slate-200 shadow-xl rounded-2xl px-5 py-3.5 min-w-[280px]">
        <span className="text-2xl">{toast.emoji}</span>
        <p className="text-sm font-semibold text-on-surface">{toast.message}</p>
        <button onClick={onDismiss} className="ml-auto text-slate-400 hover:text-slate-600">
          <span className="material-symbols-outlined text-base">close</span>
        </button>
      </div>
    </div>
  )
}

// ── Main component ─────────────────────────────────────────────────────────────
export const OrderTrackingPage = () => {
  const { orderId } = useParams()
  const [orderData, setOrderData] = useState(null)
  const [timeline, setTimeline] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [toast, setToast] = useState(null)
  const [menuNameMap, setMenuNameMap] = useState({})
  const prevStepRef = useRef(null)
  const intervalRef = useRef(null)
  const lastMenuFetchRestaurantRef = useRef(null)

  const resolveStep = (order) => {
    if (!order) return 0
    const raw = order.orderStatus || order.status || ''
    const key = typeof raw === 'number' ? raw : raw.replace(/[\s_]/g, '').toLowerCase()
    return STATUS_TO_STEP[key] ?? 0
  }

  const fetchOrderData = async (showErrors = false) => {
    if (!orderId) return
    try {
      const res = await orderApi.getOrderById(orderId)
      const order = res?.order ?? res
      const tl = res?.timeline ?? []
      setOrderData(order)
      setTimeline(Array.isArray(tl) ? tl : [])
      setError('')

      // Show toast if step advanced
      const newStep = resolveStep(order)
      if (prevStepRef.current !== null && newStep > prevStepRef.current && STEP_TOASTS[newStep]) {
        setToast(STEP_TOASTS[newStep])
      }
      prevStepRef.current = newStep
    } catch (err) {
      if (showErrors) {
        setError(err.response?.data?.message || err.message || 'Failed to load tracking')
      }
    }
  }

  useEffect(() => {
    if (!orderId) return
    let active = true
    const load = async () => {
      setLoading(true)
      await fetchOrderData(true)
      if (active) setLoading(false)
    }
    load()
    // Poll every 5 seconds while the page is open
    intervalRef.current = setInterval(() => { if (active) fetchOrderData(false) }, 5000)
    return () => {
      active = false
      if (intervalRef.current) clearInterval(intervalRef.current)
    }
  }, [orderId])

  useEffect(() => {
    const loadMenuNames = async () => {
      const restaurantId = orderData?.restaurantId
      const items = orderData?.items || []
      if (!restaurantId || items.length === 0) return

      const needsLookup = items.some((item) => {
        const candidate = [item.menuItemName, item.name, item.itemName, item.title]
          .find((value) => value && !isGenericItemName(value))
        return !candidate && item.menuItemId
      })

      if (!needsLookup) return
      if (lastMenuFetchRestaurantRef.current === restaurantId) return

      try {
        const menuRes = await catalogApi.getRestaurantMenu(restaurantId)
        const menu = Array.isArray(menuRes) ? menuRes : (menuRes?.items || menuRes?.data || [])
        const map = {}
        for (const menuItem of menu) {
          const id = String(menuItem.id || menuItem.menuItemId || '')
          if (id && menuItem.name) map[id] = menuItem.name
        }
        setMenuNameMap(map)
        lastMenuFetchRestaurantRef.current = restaurantId
      } catch {
        // Keep existing fallback labels if menu lookup is unavailable.
      }
    }

    loadMenuNames()
  }, [orderData?.restaurantId, orderData?.items])

  // Stop polling once delivered or cancelled
  const currentStep = useMemo(() => resolveStep(orderData), [orderData])
  useEffect(() => {
    const isDone = currentStep >= 4 ||
      ['RestaurantRejected', 'CancelRequestedByCustomer'].includes(orderData?.orderStatus)
    if (isDone && intervalRef.current) {
      clearInterval(intervalRef.current)
      intervalRef.current = null
    }
  }, [currentStep, orderData?.orderStatus])

  const isCancelled = ['Cancelled', 'RestaurantRejected', 'CancelRequestedByCustomer']
    .includes(orderData?.orderStatus)
  const statusLabel = STATUS_LABEL[orderData?.orderStatus] || orderData?.orderStatus || 'Pending'
  const paymentMethodInt = orderData?.payment?.paymentMethod ?? 3
  const paymentLabel = PAYMENT_METHOD_LABEL[paymentMethodInt] || 'Cash on Delivery'
  const paymentIcon  = PAYMENT_METHOD_ICON[paymentMethodInt]  || 'payments'
  const normalizedStatus = (orderData?.orderStatus || '').replace(/[\s_]/g, '').toLowerCase()
  const showDeliveryAgentCard = !isCancelled && currentStep >= 2
  const deliveryAssignment = orderData?.deliveryAssignment || {}
  const agentName = deliveryAssignment.agentName || 'Delivery agent pending'
  const agentPhone = deliveryAssignment.agentPhone || ''
  const agentVehicle = deliveryAssignment.vehicleInfo || deliveryAssignment.vehicleNumber || ''
  const agentRating = deliveryAssignment.rating || deliveryAssignment.agentRating || '4.9'
  const agentInitials = agentName
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join('')
    .toUpperCase() || 'DA'
  const agentAvatarEmoji = agentName.toLowerCase().includes('pending') ? '🛵' : '🧑‍✈️'

  const estimatedDelivery = orderData?.estimatedDeliveryAt
    || orderData?.estimatedDeliveryTime
    || (orderData?.createdAt ? new Date(new Date(orderData.createdAt).getTime() + 45 * 60000).toISOString() : null)
  const etaText = fmtEta(estimatedDelivery)

  const subtotal = Number(orderData?.subtotal || orderData?.total || 0)
  const deliveryFee = Number(orderData?.deliveryFee || orderData?.deliveryCharges || 0)
  const serviceFee = Number(orderData?.serviceFee || orderData?.platformFee || 0)
  const total = Number(orderData?.total || orderData?.totalAmount || (subtotal + deliveryFee + serviceFee))
  const activeLinePct = isCancelled ? 0 : Math.round((currentStep / (STEPS.length - 1)) * 100)
  const clampedLinePct = Math.min(activeLinePct, 100)

  return (
    <div className="bg-background min-h-screen">
      {/* Status change toast */}
      {toast && <StatusToast toast={toast} onDismiss={() => setToast(null)} />}

      <main className="pt-8 pb-16 px-6 max-w-7xl mx-auto w-full">
        {/* Header */}
        <div className="mb-8 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-[28px] font-bold text-on-background mb-1">
              Order #{String(orderId || '').split('-')[0].toUpperCase()}
            </h1>
            <p className="text-on-surface-variant text-sm flex items-center gap-1">
              <span className="material-symbols-outlined text-base">calendar_today</span>
              Estimated Delivery: {etaText || fmtDate(orderData?.createdAt || orderData?.placedAt)}
            </p>
          </div>
          <div className="flex items-center gap-2 bg-surface-container py-2 px-4 rounded-lg border border-slate-200">
            <span className="material-symbols-outlined text-primary text-lg" style={{ fontVariationSettings: "'FILL' 1" }}>
              local_shipping
            </span>
            <span className="text-sm font-medium text-on-surface">{paymentLabel}{paymentLabel.toLowerCase().includes('cash') ? ' (COD)' : ''}</span>
          </div>
        </div>

        {loading && (
          <div className="space-y-6">
            <div className="h-48 bg-slate-200 animate-pulse rounded-2xl" />
            <div className="h-32 bg-slate-200 animate-pulse rounded-2xl" />
          </div>
        )}
        {error && (
          <div className="bg-error-container text-on-error-container px-4 py-3 rounded-xl text-sm">{error}</div>
        )}

        {!loading && !error && orderData && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Left — tracking + agent */}
            <div className="lg:col-span-2 flex flex-col gap-6">

              {/* Cancelled banner */}
              {isCancelled && (
                <div className="bg-red-50 border border-red-200 rounded-2xl p-5 flex items-start gap-3">
                  <span className="material-symbols-outlined text-red-500 text-2xl mt-0.5">cancel</span>
                  <div className="flex-1">
                    <p className="font-semibold text-red-800 text-base">{statusLabel}</p>
                    {orderData?.orderStatus === 'RestaurantRejected' ? (
                      <p className="text-sm text-red-700/90 mt-1 leading-relaxed">
                        We apologize, but the restaurant is currently unable to fulfill your order.
                      </p>
                    ) : orderData?.orderStatus === 'CancelRequestedByCustomer' ? (
                      <p className="text-sm text-red-700/90 mt-1 leading-relaxed">
                        You requested to cancel this order.
                      </p>
                    ) : (
                      <p className="text-sm text-red-700/90 mt-1 leading-relaxed">
                        This order has been cancelled.
                      </p>
                    )}
                    
                    {/* Refund Information */}
                    {(paymentMethodInt === 1 || paymentMethodInt === 'Online') ? (
                       <div className="mt-3 bg-white/60 border border-red-100 rounded-lg p-3 flex items-start gap-2">
                         <span className="material-symbols-outlined text-red-500 text-sm mt-0.5">account_balance</span>
                         <p className="text-sm text-red-800">
                           <span className="font-semibold">Refund Initiated:</span> Your secure online payment of ₹{total.toFixed(2)} is being refunded. It will reflect in your original payment method within 3-5 business days.
                         </p>
                       </div>
                    ) : (
                       <p className="text-sm text-red-700/80 mt-2 italic">
                         * Since this was a Cash on Delivery order, no payment was deducted.
                       </p>
                    )}
                  </div>
                </div>
              )}

              <section className="bg-surface-container-lowest rounded-xl p-6 shadow-sm border border-outline-variant/30 relative overflow-hidden">
                <div className="absolute top-0 right-0 w-64 h-64 bg-primary/10 rounded-full blur-3xl -translate-y-1/2 translate-x-1/4 pointer-events-none" />
                <h2 className="text-headline-md font-semibold text-on-surface mb-6 relative z-10">Delivery Status</h2>

                <div className="relative z-10 pl-4">
                  <div className="absolute left-[11px] top-[12px] bottom-[12px] w-[2px] bg-surface-variant" />
                  <div
                    className={`absolute left-[11px] top-[12px] w-[2px] tracking-progress-line ${!isCancelled && currentStep < STEPS.length - 1 ? 'tracking-progress-animated' : ''}`}
                    style={{ height: `${clampedLinePct}%`, maxHeight: 'calc(100% - 24px)' }}
                  />

                  {STEPS.map((step, idx) => {
                    const done = (idx < currentStep || (currentStep === STEPS.length - 1 && idx === currentStep)) && !isCancelled
                    const active = idx === currentStep && !isCancelled && currentStep < STEPS.length - 1
                    const disabled = idx > currentStep || isCancelled
                    const tEntry = timeline.find((t) => {
                      const k = (t.status || '').replace(/[\s_]/g, '').toLowerCase()
                      return STATUS_TO_STEP[k] === idx
                    })

                    return (
                      <div key={step.label} className={`flex gap-4 relative ${idx < STEPS.length - 1 ? 'mb-6' : ''} ${disabled ? 'opacity-60' : ''}`}>
                        <div className={`relative w-6 h-6 rounded-full flex items-center justify-center shrink-0 z-10 shadow-[0_0_0_4px_theme(colors.surface-container-lowest)] ${done ? 'bg-primary' : active ? 'bg-white border-2 border-primary' : 'bg-surface-variant'}`}>
                          {done && (
                            <span className="material-symbols-outlined text-[14px] text-on-primary" style={{ fontVariationSettings: "'FILL' 1" }}>
                              check
                            </span>
                          )}
                          {active && (
                            <>
                              <span className="absolute inset-0 rounded-full tracking-active-ping" />
                              <span className="w-2 h-2 rounded-full bg-primary" />
                            </>
                          )}
                        </div>
                        <div className="-mt-1">
                          <h3 className={`text-sm font-medium ${active ? 'text-primary font-semibold' : 'text-on-surface'}`}>
                            {active && !isCancelled ? statusLabel : step.label}
                          </h3>
                          <p className="text-sm text-on-surface-variant mt-1">
                            {tEntry ? `${fmtTime(tEntry.timestamp || tEntry.occurredAt)} • ${step.desc}` : (idx === STEPS.length - 1 ? `Expected ${etaText || 'soon'}` : step.desc)}
                          </p>
                        </div>
                      </div>
                    )
                  })}
                </div>
              </section>

              {/* Delivery agent card */}
              {showDeliveryAgentCard ? (
                <section className="bg-surface-container-lowest rounded-2xl p-5 shadow-sm border border-outline-variant/30 flex flex-col gap-5">
                  <div className="flex items-center justify-between gap-4">
                    <div>
                      <p className="text-xs font-semibold uppercase tracking-wide text-primary">Delivery Agent</p>
                      <h3 className="text-lg font-semibold text-on-surface mt-1">Your driver details</h3>
                    </div>
                    <div className="rounded-full bg-primary/10 text-primary px-3 py-1 text-xs font-semibold">
                      {normalizedStatus === 'outfordelivery' ? 'On the way' : 'Assigned'}
                    </div>
                  </div>

                  <div className="flex flex-col md:flex-row gap-4 items-start md:items-center">
                    <div className="w-full md:w-40 h-28 rounded-2xl bg-gradient-to-br from-primary/15 to-primary/5 overflow-hidden shrink-0 flex items-center justify-center border border-primary/10">
                      <div className="flex flex-col items-center gap-2 text-primary">
                        <div className="w-14 h-14 rounded-full bg-white shadow-sm flex items-center justify-center text-sm font-bold">
                          <span className="text-xl" aria-hidden="true">{agentAvatarEmoji}</span>
                          <span className="sr-only">{agentInitials}</span>
                        </div>
                        <span className="material-symbols-outlined text-[28px]">delivery_dining</span>
                      </div>
                    </div>

                    <div className="flex-grow flex flex-col sm:flex-row items-start sm:items-center justify-between w-full gap-4">
                      <div className="flex items-center gap-4">
                        <div className="w-12 h-12 rounded-full bg-surface-variant overflow-hidden shrink-0 flex items-center justify-center">
                          <span className="material-symbols-outlined text-slate-500">person</span>
                        </div>
                        <div>
                          <h4 className="text-sm font-semibold text-on-surface">{agentName}</h4>
                          <p className="text-sm text-on-surface-variant">
                            {agentVehicle ? `${agentVehicle} • ` : ''}{agentRating} <span className="text-amber-500">★</span>
                          </p>
                          <p className="text-xs text-on-surface-variant mt-1">
                            {agentPhone ? (
                              <span className="flex flex-wrap items-center gap-2">
                                <span>Call</span>
                                <span className="bg-primary/10 text-primary font-semibold px-2.5 py-0.5 rounded-full border border-primary/20">
                                  {agentPhone}
                                </span>
                                <span>for delivery updates</span>
                              </span>
                            ) : 'Contact details will appear once the driver is assigned'}
                          </p>
                        </div>
                      </div>

                      <a
                        href={agentPhone ? `tel:${agentPhone}` : undefined}
                        className={`w-full sm:w-auto py-2 px-4 rounded-lg text-sm font-medium transition-colors flex items-center justify-center gap-1.5 ${agentPhone ? 'bg-primary text-on-primary hover:bg-primary-container' : 'bg-slate-200 text-slate-500 pointer-events-none'}`}
                      >
                        <span className="material-symbols-outlined text-[18px]">call</span>
                        Call
                      </a>
                    </div>
                  </div>
                </section>
              ) : (
                <section className="bg-surface-container-lowest rounded-2xl p-5 shadow-sm border border-outline-variant/30">
                  <div className="flex items-start gap-3">
                    <div className="w-10 h-10 rounded-full bg-surface-container flex items-center justify-center text-primary shrink-0">
                      <span className="material-symbols-outlined text-[20px]">schedule</span>
                    </div>
                    <div>
                      <h3 className="text-sm font-semibold text-on-surface">Delivery agent will appear after out for delivery</h3>
                      <p className="text-sm text-on-surface-variant mt-1">
                        We will show the assigned agent name, phone, and vehicle details once the order reaches out-for-delivery.
                      </p>
                    </div>
                  </div>
                </section>
              )}
            </div>

            {/* Right — order summary */}
            <div className="lg:col-span-1">
              <section className="bg-surface-container-lowest rounded-xl p-6 shadow-sm border border-outline-variant/30 sticky top-24">
                <h2 className="text-headline-md font-semibold text-on-surface mb-4">Order Summary</h2>

                {orderData.items?.length > 0 ? (
                  <div className="flex flex-col gap-4 mb-6">
                    {orderData.items.map((item, idx) => (
                      <div key={item.orderItemId || idx} className="flex justify-between items-start gap-2">
                        <div className="flex gap-2">
                          <div className="w-6 h-6 rounded bg-surface-container flex items-center justify-center shrink-0 text-xs font-semibold text-on-surface">
                            {item.quantity}
                          </div>
                          <div>
                            <h4 className="text-sm font-medium text-on-surface leading-tight">{resolveItemName(item, menuNameMap)}</h4>
                            {item.notes && <p className="text-xs text-on-surface-variant">{item.notes}</p>}
                          </div>
                        </div>
                        <span className="text-sm font-medium text-on-surface shrink-0">
                          ₹{item.subtotal || (item.quantity * item.unitPriceSnapshot) || 0}
                        </span>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-on-surface-variant mb-6">No items available</p>
                )}

                <div className="border-t border-outline-variant/30 pt-4 space-y-2 text-sm text-on-surface-variant">
                  <div className="flex justify-between">
                    <span>Subtotal</span>
                    <span>₹{subtotal.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Delivery Fee</span>
                    <span>₹{deliveryFee.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Service Fee</span>
                    <span>₹{serviceFee.toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between font-semibold text-[18px] text-on-surface pt-2 border-t border-outline-variant/30">
                    <span>Total {paymentLabel.toLowerCase().includes('cash') ? '(COD)' : ''}</span>
                    <span>₹{total.toFixed(2)}</span>
                  </div>
                </div>

                {orderData.deliveryAddress && (
                  <div className="mt-5 pt-5 border-t border-slate-200">
                    <p className="text-xs font-semibold uppercase tracking-wide text-on-surface-variant mb-2 flex items-center gap-1">
                      <span className="material-symbols-outlined text-sm">location_on</span>
                      Deliver To
                    </p>
                    <p className="text-sm text-on-surface">{orderData.deliveryAddress.street}</p>
                    <p className="text-sm text-on-surface-variant">
                      {orderData.deliveryAddress.city}
                      {orderData.deliveryAddress.pincode ? `, ${orderData.deliveryAddress.pincode}` : ''}
                    </p>
                  </div>
                )}

                <Link
                  to="/orders"
                  className="mt-5 flex items-center gap-1 text-sm text-primary font-medium hover:text-primary-container transition-colors"
                >
                  <span className="material-symbols-outlined text-base">arrow_back</span>
                  Back to My Orders
                </Link>
              </section>
            </div>
          </div>
        )}
      </main>

      <style>{`
        @keyframes slideDown {
          from { opacity: 0; transform: translate(-50%, -20px); }
          to   { opacity: 1; transform: translate(-50%, 0); }
        }

        @keyframes trackingFlow {
          from { background-position: 0 0; }
          to { background-position: 0 -140px; }
        }

        @keyframes activePing {
          0% { transform: scale(1); opacity: 0.55; }
          70% { transform: scale(1.8); opacity: 0; }
          100% { transform: scale(1.8); opacity: 0; }
        }

        .tracking-progress-line {
          border-radius: 9999px;
          background: linear-gradient(180deg, #1978e5 0%, #60a5fa 50%, #1978e5 100%);
          background-size: 100% 140px;
        }

        .tracking-progress-animated {
          animation: trackingFlow 1.2s linear infinite;
        }

        .tracking-active-ping {
          background: rgba(25, 120, 229, 0.25);
          animation: activePing 1.5s ease-out infinite;
        }
      `}</style>
    </div>
  )
}
