import { useEffect, useMemo, useRef, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import orderApi from '../services/orderApi'

// ── Status mappings ────────────────────────────────────────────────────────────

// Map status string/int → step index (0–4)
const STATUS_TO_STEP = {
  checkoutstarted: 0, 1: 0,
  paid: 0, 4: 0,
  restaurantaccepted: 1, 5: 1,
  preparing: 2, 6: 2,
  readyforpickup: 2, 7: 2,
  pickedup: 3, 8: 3,
  outfordelivery: 3, 9: 3,
  delivered: 4, 10: 4,
}

const STEPS = [
  { label: 'Order Placed',   icon: 'receipt_long',    desc: 'We have received your order' },
  { label: 'Confirmed',      icon: 'thumb_up',         desc: 'Restaurant accepted your order' },
  { label: 'Preparing',      icon: 'skillet',          desc: 'Your food is being prepared' },
  { label: 'On the Way',     icon: 'delivery_dining',  desc: 'Your delivery agent is en route' },
  { label: 'Delivered',      icon: 'home',             desc: 'Enjoy your meal!' },
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

const PAYMENT_METHOD_LABEL = { 1: 'Digital Wallet', 2: 'Credit / Debit Card', 3: 'Cash on Delivery' }
const PAYMENT_METHOD_ICON  = { 1: 'account_balance_wallet', 2: 'credit_card', 3: 'payments' }

const fmtTime = (iso) =>
  iso ? new Date(iso).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true }) : ''

const fmtDate = (iso) =>
  iso ? new Date(iso).toLocaleString('en-IN', { dateStyle: 'medium', timeStyle: 'short' }) : ''

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
  const prevStepRef = useRef(null)
  const intervalRef = useRef(null)

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

  // Progress percentage for the horizontal bar
  const progressPct = isCancelled ? 0 : Math.round((currentStep / (STEPS.length - 1)) * 100)

  return (
    <div className="bg-background min-h-screen">
      {/* Status change toast */}
      {toast && <StatusToast toast={toast} onDismiss={() => setToast(null)} />}

      <main className="pt-8 pb-16 px-6 max-w-5xl mx-auto w-full">
        {/* Header */}
        <div className="mb-8 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-[28px] font-bold text-on-background mb-1">
              Order Tracking
            </h1>
            <p className="text-on-surface-variant text-sm flex items-center gap-1">
              <span className="material-symbols-outlined text-base">tag</span>
              #{String(orderId || '').split('-')[0].toUpperCase()}
              {orderData && (
                <span className="ml-2 text-slate-400">· {fmtDate(orderData.createdAt || orderData.placedAt)}</span>
              )}
            </p>
          </div>
          <div className="flex items-center gap-2 bg-white py-2 px-4 rounded-xl border border-slate-200 shadow-sm">
            <span className="material-symbols-outlined text-primary text-lg" style={{ fontVariationSettings: "'FILL' 1" }}>
              {paymentIcon}
            </span>
            <span className="text-sm font-medium text-on-surface">{paymentLabel}</span>
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
                <div className="bg-red-50 border border-red-200 rounded-2xl p-5 flex items-center gap-3">
                  <span className="material-symbols-outlined text-red-500 text-2xl">cancel</span>
                  <div>
                    <p className="font-semibold text-red-800">{statusLabel}</p>
                    <p className="text-sm text-red-600/80">This order has been cancelled or rejected.</p>
                  </div>
                </div>
              )}

              {/* ── Zomato-style progress tracker ── */}
              <section className="bg-white rounded-2xl shadow-sm border border-slate-100 overflow-hidden">
                {/* Coloured header strip */}
                <div className={`px-6 py-4 flex items-center gap-3 ${isCancelled ? 'bg-red-50' : 'bg-gradient-to-r from-primary/10 to-primary/5'}`}>
                  <span className={`material-symbols-outlined text-xl ${isCancelled ? 'text-red-500' : 'text-primary'}`}
                        style={{ fontVariationSettings: "'FILL' 1" }}>
                    {isCancelled ? 'cancel' : STEPS[currentStep]?.icon}
                  </span>
                  <div>
                    <p className={`font-bold text-base ${isCancelled ? 'text-red-800' : 'text-on-surface'}`}>
                      {isCancelled ? statusLabel : STEPS[currentStep]?.label}
                    </p>
                    <p className="text-xs text-on-surface-variant">
                      {isCancelled ? 'Order could not be fulfilled.' : STEPS[currentStep]?.desc}
                    </p>
                  </div>
                </div>

                {/* Progress bar */}
                <div className="px-6 pt-5 pb-2">
                  <div className="relative h-2 bg-slate-100 rounded-full overflow-hidden">
                    <div
                      className="absolute inset-y-0 left-0 rounded-full transition-all duration-700 ease-in-out"
                      style={{
                        width: `${progressPct}%`,
                        background: isCancelled
                          ? '#ef4444'
                          : 'linear-gradient(90deg, var(--color-primary, #f97316), #fb923c)',
                      }}
                    />
                  </div>
                </div>

                {/* Step nodes */}
                <div className="px-4 pb-6 pt-3">
                  <div className="flex justify-between items-start">
                    {STEPS.map((step, idx) => {
                      const done    = idx < currentStep && !isCancelled
                      const active  = idx === currentStep && !isCancelled
                      const pending = idx > currentStep || isCancelled

                      // Find matching timeline entry
                      const tEntry = timeline.find(t => {
                        const k = (t.status || '').replace(/[\s_]/g, '').toLowerCase()
                        return STATUS_TO_STEP[k] === idx
                      })

                      return (
                        <div key={step.label} className="flex flex-col items-center gap-1.5 flex-1">
                          {/* Icon circle */}
                          <div className={`
                            w-10 h-10 rounded-full flex items-center justify-center transition-all duration-500
                            ${done    ? 'bg-primary shadow-md shadow-primary/30' : ''}
                            ${active  ? 'bg-white border-2 border-primary shadow-md shadow-primary/20' : ''}
                            ${pending ? 'bg-slate-100 border border-slate-200' : ''}
                            ${isCancelled ? 'bg-red-50 border border-red-200' : ''}
                          `}>
                            {done ? (
                              <span className="material-symbols-outlined text-white text-base" style={{ fontVariationSettings: "'FILL' 1" }}>
                                check_circle
                              </span>
                            ) : active ? (
                              <span className={`material-symbols-outlined text-primary text-base ${active ? 'animate-pulse' : ''}`}
                                    style={{ fontVariationSettings: "'FILL' 1" }}>
                                {step.icon}
                              </span>
                            ) : (
                              <span className={`material-symbols-outlined text-base ${isCancelled ? 'text-red-300' : 'text-slate-400'}`}>
                                {step.icon}
                              </span>
                            )}
                          </div>

                          {/* Label + time */}
                          <p className={`text-[10px] font-semibold text-center leading-tight
                            ${done || active ? 'text-on-surface' : 'text-slate-400'}
                            ${isCancelled ? 'text-red-300' : ''}
                          `}>
                            {step.label}
                          </p>
                          {tEntry && (
                            <p className="text-[9px] text-on-surface-variant text-center">
                              {fmtTime(tEntry.timestamp || tEntry.occurredAt)}
                            </p>
                          )}
                          {active && !tEntry && (
                            <p className="text-[9px] text-primary text-center animate-pulse">In progress…</p>
                          )}
                        </div>
                      )
                    })}
                  </div>
                </div>
              </section>

              {/* Delivery agent card */}
              {orderData.deliveryAssignment && (
                <section className="bg-white rounded-2xl p-5 shadow-sm border border-slate-100">
                  <p className="text-xs font-semibold uppercase tracking-wide text-slate-400 mb-3 flex items-center gap-1.5">
                    <span className="material-symbols-outlined text-sm">delivery_dining</span>
                    Your Delivery Agent
                  </p>
                  <div className="flex items-center justify-between gap-4">
                    <div className="flex items-center gap-3">
                      <div className="w-12 h-12 rounded-full bg-gradient-to-br from-primary/20 to-primary/10 flex items-center justify-center">
                        <span className="material-symbols-outlined text-primary">person</span>
                      </div>
                      <div>
                        <p className="font-semibold text-on-surface text-sm">
                          {orderData.deliveryAssignment.agentName || 'Delivery Agent'}
                        </p>
                        <p className="text-xs text-on-surface-variant mt-0.5">
                          Status: <span className="font-medium">{orderData.deliveryAssignment.currentStatus || 'Assigned'}</span>
                        </p>
                      </div>
                    </div>
                    {orderData.deliveryAssignment.agentPhone && (
                      <a
                        href={`tel:${orderData.deliveryAssignment.agentPhone}`}
                        className="flex items-center gap-1.5 bg-primary text-on-primary text-sm font-medium px-4 py-2 rounded-xl hover:bg-primary-container transition-colors"
                      >
                        <span className="material-symbols-outlined text-base">call</span> Call
                      </a>
                    )}
                  </div>
                </section>
              )}
            </div>

            {/* Right — order summary */}
            <div className="lg:col-span-1">
              <section className="bg-white rounded-2xl p-6 shadow-sm border border-slate-100 sticky top-24">
                <h2 className="text-base font-semibold text-on-surface mb-5 flex items-center gap-2">
                  <span className="material-symbols-outlined text-primary text-xl">receipt_long</span>
                  Order Summary
                </h2>

                {orderData.items?.length > 0 ? (
                  <div className="flex flex-col gap-3 mb-6">
                    {orderData.items.map((item, idx) => (
                      <div key={item.orderItemId || idx} className="flex justify-between items-start gap-2">
                        <div className="flex gap-2">
                          <div className="w-6 h-6 rounded bg-slate-100 flex items-center justify-center shrink-0 text-xs font-semibold text-on-surface">
                            {item.quantity}
                          </div>
                          <h4 className="text-sm font-medium text-on-surface">
                            {item.menuItemName || item.name || 'Item'}
                          </h4>
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

                <div className="border-t border-slate-200 pt-4 space-y-2 text-sm text-on-surface-variant">
                  <div className="flex justify-between">
                    <span>Subtotal</span>
                    <span>₹{Number(orderData.subtotal || orderData.total || 0).toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Delivery Fee</span>
                    <span className="text-emerald-600">Free</span>
                  </div>
                  <div className="flex justify-between font-semibold text-base text-on-surface pt-2 border-t border-slate-200">
                    <span>Total</span>
                    <span>₹{Number(orderData.total || orderData.totalAmount || 0).toFixed(2)}</span>
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
      `}</style>
    </div>
  )
}
