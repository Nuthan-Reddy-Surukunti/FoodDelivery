import { useEffect, useMemo, useRef, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import orderApi from '../services/orderApi'

// Map status string/int → step index (0-4)
const STATUS_TO_STEP = {
  // 0 – Placed
  checkoutstarted: 0, 1: 0, pending: 0,
  paid: 0, 3: 0,
  // 1 – Confirmed
  restaurantaccepted: 1, 4: 1, confirmed: 1,
  // 2 – Preparing
  preparing: 2, 6: 2,
  readyforpickup: 2, 7: 2,
  // 3 – Picked Up / Out for Delivery
  pickedup: 3, 8: 3,
  outfordelivery: 3, 9: 3,
  // 4 – Delivered
  delivered: 4, 10: 4,
}

const STEPS = [
  { label: 'Order Placed', icon: 'receipt_long' },
  { label: 'Confirmed', icon: 'check_circle' },
  { label: 'Preparing', icon: 'skillet' },
  { label: 'Out for Delivery', icon: 'local_shipping' },
  { label: 'Delivered', icon: 'home' },
]

const STATUS_LABEL = {
  CheckoutStarted: 'Order Placed', Paid: 'Payment Confirmed',
  RestaurantAccepted: 'Restaurant Accepted', Preparing: 'Preparing Your Order',
  ReadyForPickup: 'Ready for Pickup', PickedUp: 'Picked Up by Agent',
  OutForDelivery: 'Out for Delivery', Delivered: 'Delivered 🎉',
  Cancelled: 'Order Cancelled', RestaurantRejected: 'Rejected by Restaurant',
  CancelRequestedByCustomer: 'Cancellation Requested',
}

const fmtTime = (iso) => iso
  ? new Date(iso).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit', hour12: true })
  : ''

const fmtDate = (iso) => iso
  ? new Date(iso).toLocaleString('en-IN', { dateStyle: 'medium', timeStyle: 'short' })
  : ''

export const OrderTrackingPage = () => {
  const { orderId } = useParams()
  const [orderData, setOrderData] = useState(null)
  const [timeline, setTimeline] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const intervalRef = useRef(null)

  // Function to fetch order details
  const fetchOrderData = async (showErrors = false) => {
    if (!orderId) return
    try {
      const res = await orderApi.getOrderById(orderId)
      const order = res?.order ?? res
      const tl = res?.timeline ?? []
      setOrderData(order)
      setTimeline(Array.isArray(tl) ? tl : [])
      setError('')
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
    
    // Poll for updated order status every 10 seconds
    intervalRef.current = setInterval(() => {
      if (active) fetchOrderData(false)
    }, 10000)
    
    return () => { 
      active = false
      if (intervalRef.current) clearInterval(intervalRef.current)
    }
  }, [orderId])

  const currentStep = useMemo(() => {
    if (!orderData) return 0
    const raw = orderData.orderStatus || orderData.status || ''
    const key = typeof raw === 'number' ? raw : raw.replace(/[\s_]/g, '').toLowerCase()
    return STATUS_TO_STEP[key] ?? 0
  }, [orderData])

  const isCancelled = ['Cancelled', 'RestaurantRejected', 'CancelRequestedByCustomer'].includes(orderData?.orderStatus)
  const statusLabel = STATUS_LABEL[orderData?.orderStatus] || orderData?.orderStatus || 'Pending'

  return (
    <div className="bg-background min-h-screen">
      <main className="pt-8 pb-16 px-6 max-w-7xl mx-auto w-full">
        {/* Header */}
        <div className="mb-8 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-[32px] font-bold text-on-background mb-1">
              Order #{String(orderId || '').split('-')[0].toUpperCase()}
            </h1>
            <p className="text-on-surface-variant text-sm flex items-center gap-1">
              <span className="material-symbols-outlined text-lg">calendar_today</span>
              {orderData ? fmtDate(orderData.createdAt || orderData.placedAt) : 'Loading...'}
            </p>
          </div>
          <div className="flex items-center gap-2 bg-surface-container py-2 px-4 rounded-lg border border-outline-variant/30">
            <span className="material-symbols-outlined text-primary">local_shipping</span>
            <span className="text-sm font-medium text-on-surface">Cash on Delivery (COD)</span>
          </div>
        </div>

        {loading && (
          <div className="space-y-6">
            <div className="h-48 bg-slate-200 animate-pulse rounded-xl" />
            <div className="h-32 bg-slate-200 animate-pulse rounded-xl" />
          </div>
        )}

        {error && (
          <div className="bg-error-container text-on-error-container px-4 py-3 rounded-xl text-sm">{error}</div>
        )}

        {!loading && !error && orderData && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Left column: timeline + agent */}
            <div className="lg:col-span-2 flex flex-col gap-8">

              {/* Cancelled banner */}
              {isCancelled && (
                <div className="bg-error-container border border-error/30 rounded-xl p-5 flex items-center gap-3">
                  <span className="material-symbols-outlined text-error text-2xl">cancel</span>
                  <div>
                    <p className="font-semibold text-on-error-container">{statusLabel}</p>
                    <p className="text-sm text-on-error-container/70">This order has been cancelled.</p>
                  </div>
                </div>
              )}

              {/* Vertical timeline tracker card */}
              <section className="bg-white rounded-xl p-8 shadow-sm border border-outline-variant/30 relative overflow-hidden">
                {/* Decorative blur */}
                <div className="absolute top-0 right-0 w-48 h-48 bg-primary-fixed/20 rounded-full blur-3xl -translate-y-1/2 translate-x-1/4 pointer-events-none" />

                <h2 className="text-xl font-semibold text-on-surface mb-8 relative z-10">Delivery Status</h2>

                <div className="relative z-10 pl-4">
                  {/* Vertical line */}
                  <div className="absolute left-[19px] top-3 bottom-3 w-0.5 bg-slate-200" />
                  {/* Active progress line */}
                  {!isCancelled && (
                    <div
                      className="absolute left-[19px] top-3 w-0.5 bg-primary transition-all duration-700"
                      style={{ height: `${(currentStep / (STEPS.length - 1)) * 100}%` }}
                    />
                  )}

                  {STEPS.map((step, idx) => {
                    const done = idx < currentStep
                    const active = idx === currentStep && !isCancelled
                    const pending = idx > currentStep || isCancelled

                    // Find matching timeline entry
                    const tEntry = timeline.find((t) => {
                      const k = (t.status || '').replace(/[\s_]/g, '').toLowerCase()
                      return STATUS_TO_STEP[k] === idx
                    })

                    return (
                      <div key={step.label} className={`flex gap-4 mb-7 relative ${idx === STEPS.length - 1 ? 'mb-0' : ''}`}>
                        {/* Step circle */}
                        <div className={`w-6 h-6 rounded-full flex items-center justify-center shrink-0 z-10 ring-4 ring-white transition-all ${done ? 'bg-primary' : active ? 'bg-white border-2 border-primary' : 'bg-slate-200'}`}>
                          {done ? (
                            <span className="material-symbols-outlined text-white text-sm" style={{ fontVariationSettings: "'FILL' 1" }}>check</span>
                          ) : active ? (
                            <div className="w-2 h-2 rounded-full bg-primary" />
                          ) : null}
                        </div>
                        {/* Step text */}
                        <div className="-mt-1 flex-1">
                          <h3 className={`text-sm font-semibold ${active ? 'text-primary' : done ? 'text-on-surface' : 'text-slate-400'}`}>
                            {step.label}
                          </h3>
                          <p className="text-xs text-on-surface-variant mt-0.5">
                            {tEntry ? fmtTime(tEntry.timestamp) : (active ? 'In progress...' : pending ? 'Pending' : '')}
                          </p>
                        </div>
                      </div>
                    )
                  })}
                </div>
              </section>

              {/* Agent / delivery info */}
              {orderData.deliveryAssignment && (
                <section className="bg-white rounded-xl p-5 shadow-sm border border-outline-variant/30 flex flex-col md:flex-row gap-4 items-center">
                  <div className="w-full md:w-48 h-28 rounded-lg bg-slate-100 overflow-hidden shrink-0 flex items-center justify-center">
                    <span className="material-symbols-outlined text-4xl text-slate-400">map</span>
                  </div>
                  <div className="flex-grow flex flex-col sm:flex-row items-start sm:items-center justify-between w-full gap-4">
                    <div className="flex items-center gap-3">
                      <div className="w-12 h-12 rounded-full bg-slate-200 overflow-hidden shrink-0 flex items-center justify-center">
                        <span className="material-symbols-outlined text-slate-400">person</span>
                      </div>
                      <div>
                        <h3 className="text-sm font-semibold text-on-surface">Delivery Agent</h3>
                        <p className="text-xs text-on-surface-variant">
                          Status: {orderData.deliveryAssignment.currentStatus || 'Assigned'}
                        </p>
                      </div>
                    </div>
                    <div className="flex gap-2 w-full sm:w-auto">
                      <a
                        href={`tel:`}
                        className="flex-1 sm:flex-none py-2 px-4 rounded-lg bg-primary text-on-primary text-sm font-medium hover:bg-primary-container transition-colors flex items-center justify-center gap-1"
                      >
                        <span className="material-symbols-outlined text-lg">call</span> Call
                      </a>
                    </div>
                  </div>
                </section>
              )}
            </div>

            {/* Right column: order summary */}
            <div className="lg:col-span-1">
              <section className="bg-white rounded-xl p-6 shadow-sm border border-outline-variant/30 sticky top-24">
                <h2 className="text-xl font-semibold text-on-surface mb-5">Order Summary</h2>

                {orderData.items?.length > 0 ? (
                  <div className="flex flex-col gap-4 mb-6">
                    {orderData.items.map((item, idx) => (
                      <div key={item.orderItemId || idx} className="flex justify-between items-start">
                        <div className="flex gap-2">
                          <div className="w-6 h-6 rounded bg-slate-100 flex items-center justify-center shrink-0 text-xs font-semibold text-on-surface">
                            {item.quantity}
                          </div>
                          <div>
                            <h4 className="text-sm font-medium text-on-surface">
                              {item.menuItemName || item.name || 'Item'}
                            </h4>
                            {item.customizationNotes && (
                              <p className="text-xs text-on-surface-variant">{item.customizationNotes}</p>
                            )}
                          </div>
                        </div>
                        <span className="text-sm font-medium text-on-surface shrink-0">₹{item.subtotal || (item.quantity * item.unitPriceSnapshot) || 0}</span>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p className="text-sm text-on-surface-variant mb-6">No items available</p>
                )}

                <div className="border-t border-slate-200 pt-4 flex flex-col gap-2">
                  <div className="flex justify-between text-sm text-on-surface-variant">
                    <span>Subtotal</span>
                    <span>₹{Number(orderData.subtotal || orderData.total || 0).toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between text-sm text-on-surface-variant">
                    <span>Delivery Fee</span>
                    <span>₹{Number(orderData.deliveryFee || 0).toFixed(2)}</span>
                  </div>
                  <div className="flex justify-between font-semibold text-base text-on-surface mt-2 pt-2 border-t border-slate-200">
                    <span>Total (COD)</span>
                    <span>₹{Number(orderData.total || orderData.totalAmount || 0).toFixed(2)}</span>
                  </div>
                </div>

                {/* Delivery address */}
                {orderData.deliveryAddress && (
                  <div className="mt-5 pt-5 border-t border-slate-200">
                    <p className="text-xs font-semibold uppercase tracking-wide text-on-surface-variant mb-2">Deliver To</p>
                    <p className="text-sm text-on-surface">{orderData.deliveryAddress.street}</p>
                    <p className="text-sm text-on-surface-variant">{orderData.deliveryAddress.city}{orderData.deliveryAddress.pincode ? `, ${orderData.deliveryAddress.pincode}` : ''}</p>
                  </div>
                )}

                <Link
                  to="/orders"
                  className="mt-5 block text-center text-sm text-primary font-medium hover:text-primary-container transition-colors"
                >
                  ← Back to My Orders
                </Link>
              </section>
            </div>
          </div>
        )}
      </main>

      {/* Footer */}
      <footer className="bg-slate-50 border-t border-slate-200 py-12">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8 px-8 max-w-7xl mx-auto">
          <div>
            <span className="text-lg font-bold text-on-surface block mb-2">QuickBite</span>
            <p className="text-sm text-slate-500">© 2024 QuickBite Food Delivery. All rights reserved.</p>
          </div>
          <div className="flex flex-wrap gap-x-6 gap-y-2 md:justify-end items-center">
            {['About Us', 'Terms of Service', 'Privacy Policy', 'Help Center'].map(l => (
              <a key={l} href="#" className="text-sm text-slate-500 hover:text-primary transition-colors">{l}</a>
            ))}
          </div>
        </div>
      </footer>
    </div>
  )
}
