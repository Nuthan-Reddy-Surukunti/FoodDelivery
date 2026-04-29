import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { useNotification } from '../hooks/useNotification'
import orderApi from '../services/orderApi'

// Status → badge config
const STATUS_BADGE = {
  Delivered: 'bg-emerald-100 text-emerald-800',
  Cancelled: 'bg-red-100 text-red-800 line-through',
  RestaurantRejected: 'bg-red-100 text-red-800',
  OutForDelivery: 'bg-purple-100 text-purple-800',
  PickedUp: 'bg-indigo-100 text-indigo-800',
  Preparing: 'bg-blue-100 text-blue-800',
  RestaurantAccepted: 'bg-teal-100 text-teal-800',
  Paid: 'bg-sky-100 text-sky-800',
  CheckoutStarted: 'bg-amber-100 text-amber-800',
}

const STATUS_LABEL = {
  CheckoutStarted: 'Pending',
  Paid: 'Paid',
  RestaurantAccepted: 'Accepted',
  Preparing: 'Preparing',
  ReadyForPickup: 'Ready',
  PickedUp: 'Picked Up',
  OutForDelivery: 'Out for Delivery',
  Delivered: 'Delivered',
  Cancelled: 'Cancelled',
  RestaurantRejected: 'Rejected',
  CancelRequestedByCustomer: 'Cancel Requested',
}

const isActive = (status) =>
  !['Delivered', 'Cancelled', 'RestaurantRejected', 'CancelRequestedByCustomer'].includes(status)

const fmtDate = (iso) =>
  iso ? new Date(iso).toLocaleString('en-IN', { dateStyle: 'medium', timeStyle: 'short' }) : ''

const fmtShortDate = (iso) =>
  iso ? new Date(iso).toLocaleDateString('en-IN', { dateStyle: 'medium' }) : ''

const fmtShortTime = (iso) =>
  iso ? new Date(iso).toLocaleTimeString('en-IN', { hour: 'numeric', minute: '2-digit', hour12: true }) : ''

const PAYMENT_METHOD_LABEL = {
  1: 'Digital Wallet',
  Wallet: 'Digital Wallet',
  2: 'Credit / Debit Card',
  Card: 'Credit / Debit Card',
  3: 'Cash on Delivery',
  CashOnDelivery: 'Cash on Delivery',
}

const buildRestaurantTitle = (restaurantName) => {
  if (!restaurantName) return 'Order details'
  return `Order from ${restaurantName}`
}

const isGenericItemName = (name) => /^item\s*\d+$/i.test((name || '').trim())

const extractItemNames = (order) => {
  const items = Array.isArray(order.items) ? order.items : []
  const names = items
    .map((item) => item.menuItemName || item.name || item.itemName)
    .filter((name) => name && !isGenericItemName(name))
  return [...new Set(names)]
}

const normalizeOrder = (item) => ({
  id: item.orderId || item.id,
  restaurantId: item.restaurantId,
  restaurantName: item.restaurantName || item.restaurant?.name || '',
  amount: Number(item.total || item.totalAmount || item.totalPrice || 0),
  status: item.orderStatus || item.status || 'Unknown',
  createdAt: item.createdAt || item.placedAt,
  itemCount: item.items?.length ?? item.itemCount ?? 0,
  itemNames: extractItemNames(item),
  deliveryAddress: item.deliveryAddress,
  estimatedDeliveryAt: item.estimatedDeliveryAt || item.estimatedDeliveryTime,
  paymentMethod: item.payment?.paymentMethod || item.paymentMethod,
  imageUrl: item.restaurantImageUrl ?? null,
})

export const MyOrdersPage = () => {
  const { user } = useAuth()
  const navigate = useNavigate()
  const { showSuccess, showError } = useNotification()
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [tab, setTab] = useState('all') // all | active | past
  const [reordering, setReordering] = useState(null)
  const intervalRef = useRef(null)

  const handleReorder = async (orderId) => {
    setReordering(orderId)
    try {
      await orderApi.reorderFromHistory(orderId)
      showSuccess('Items added! Redirecting to checkout...')
      navigate('/checkout')
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to reorder')
    } finally {
      setReordering(null)
    }
  }

  // Function to fetch orders
  const fetchOrders = async (showErrors = false) => {
    if (!user?.id) return
    try {
      const res = await orderApi.getOrdersByUser(user.id, false)
      const raw = Array.isArray(res) ? res : (res?.items || res?.data || [])
      setOrders(raw.map(normalizeOrder))
      setError('')
    } catch (err) {
      if (showErrors) {
        setError(err.response?.data?.message || err.message || 'Failed to load orders')
      }
    }
  }

  // Initial load + polling for status updates
  useEffect(() => {
    if (!user?.id) { setLoading(false); return }
    let active = true
    
    const load = async () => {
      setLoading(true)
      await fetchOrders(true)
      if (active) setLoading(false)
    }
    
    load()
    
    // Poll for updated order status every 15 seconds
    intervalRef.current = setInterval(() => {
      if (active) fetchOrders(false)
    }, 15000)
    
    return () => { 
      active = false
      if (intervalRef.current) clearInterval(intervalRef.current)
    }
  }, [user?.id])

  const filtered = orders.filter((o) => {
    if (tab === 'active') return isActive(o.status)
    if (tab === 'past') return !isActive(o.status)
    return true
  })

  const summarizeAddress = (address) => {
    if (!address) return 'Delivery address not available'
    const parts = [address.street || address.addressLine1, address.city, address.state, address.pincode || address.pinCode]
      .filter(Boolean)
    return parts.length ? parts.join(', ') : 'Delivery address not available'
  }

  const STATUS_BORDER = {
    Delivered:            'status-border-delivered',
    Cancelled:            'status-border-cancelled',
    RestaurantRejected:   'status-border-cancelled',
    OutForDelivery:       'status-border-active',
    PickedUp:             'status-border-active',
    Preparing:            'status-border-preparing',
    RestaurantAccepted:   'status-border-active',
    Paid:                 'status-border-active',
    CheckoutStarted:      'status-border-active',
  }

  return (
    <div className="min-h-screen bg-slate-50">
      {/* ── Gradient Header Banner ── */}
      <div className="bg-gradient-to-r from-slate-900 via-slate-800 to-indigo-900 px-6 py-8">
        <div className="max-w-5xl mx-auto">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-white/10 backdrop-blur-sm rounded-xl flex items-center justify-center">
              <span className="material-symbols-outlined text-white text-xl" style={{ fontVariationSettings: "'FILL' 1" }}>receipt_long</span>
            </div>
            <div>
              <h1 className="text-2xl font-extrabold text-white">My Orders</h1>
              <p className="text-white/60 text-sm">Track and manage your dining experiences.</p>
            </div>
          </div>
        </div>
      </div>

      <main className="px-6 pb-16 max-w-5xl mx-auto w-full">
        {/* ── Pill Filter Tabs ── */}
        <div className="flex gap-2 my-6">
          {[['all', 'All', '🧾'], ['active', 'Active', '🚴'], ['past', 'Past', '✅']].map(([key, label, emoji]) => (
            <button
              key={key}
              onClick={() => setTab(key)}
              className={`px-5 py-2 rounded-full text-sm font-semibold flex items-center gap-1.5 transition-all ${
                tab === key
                  ? 'pill-tab-active'
                  : 'pill-tab-inactive'
              }`}
            >
              <span>{emoji}</span>
              {label}
            </button>
          ))}
        </div>

        {loading && (
          <div className="space-y-4">
            {[1,2,3].map(i => (
              <div key={i} className="h-28 animate-pulse rounded-xl bg-slate-200" />
            ))}
          </div>
        )}

        {error && (
          <div className="bg-error-container text-on-error-container px-4 py-3 rounded-xl text-sm">{error}</div>
        )}

        {!loading && !error && filtered.length === 0 && (
          <div className="py-20 text-center">
            <p className="text-5xl mb-4">🧾</p>
            <p className="text-lg font-semibold text-on-surface">No orders found</p>
            <p className="text-sm text-on-surface-variant mt-1 mb-6">
              {tab === 'active' ? "You have no active orders right now." : "You haven't placed any orders yet."}
            </p>
            <button
              onClick={() => navigate('/')}
              className="bg-primary text-on-primary px-6 py-3 rounded-lg text-sm font-semibold hover:bg-primary-container transition-colors"
            >
              Browse Restaurants
            </button>
          </div>
        )}

        {/* Order cards */}
        <div className="flex flex-col gap-6">
          {filtered.map((order) => {
            const badgeClass = STATUS_BADGE[order.status] || 'bg-slate-100 text-slate-700'
            const statusText = STATUS_LABEL[order.status] || order.status
            const cancelled = order.status === 'Cancelled' || order.status === 'RestaurantRejected'

            return (
              <div
                key={order.id}
                className={`bg-white border border-slate-100 rounded-2xl shadow-sm flex flex-col gap-5 overflow-hidden ${cancelled ? 'opacity-70' : ''} ${STATUS_BORDER[order.status] || 'border-l-4 border-l-slate-200'}`}
              >
                <div className="p-6 flex flex-col gap-5">
                <div className="flex flex-col lg:flex-row gap-5 lg:items-center justify-between">
                  <div className="flex gap-5 items-start w-full lg:w-auto">
                    <div className="w-20 h-20 rounded-2xl overflow-hidden flex-shrink-0 bg-slate-100 flex items-center justify-center text-3xl border border-outline-variant/50">
                      {order.imageUrl ? (
                        <img src={order.imageUrl} alt={order.restaurantName} className={`w-full h-full object-cover ${cancelled ? 'grayscale' : ''}`} />
                      ) : '🏪'}
                    </div>
                    <div className="flex flex-col gap-2 min-w-0">
                      <div className="flex items-center gap-2 flex-wrap">
                        <span className={`${badgeClass} text-xs font-semibold px-2.5 py-1 rounded-full`}>{statusText}</span>
                        <span className="text-on-surface-variant text-sm">#{String(order.id).split('-')[0].toUpperCase()}</span>
                      </div>
                      <h2 className="text-xl font-semibold text-on-surface leading-tight">
                        {buildRestaurantTitle(order.restaurantName)}
                      </h2>
                      <p className="text-sm text-on-surface-variant">
                        {order.itemNames?.length > 0
                          ? `${order.itemNames.slice(0, 3).join(', ')}${order.itemNames.length > 3 ? ` +${order.itemNames.length - 3} more` : ''}`
                          : (order.itemCount > 0 ? `${order.itemCount} item${order.itemCount > 1 ? 's' : ''}` : 'No items listed')}
                      </p>
                      <div className="flex flex-wrap gap-2 text-xs text-on-surface-variant">
                        <span className="rounded-full bg-slate-50 border border-slate-200 px-3 py-1">
                          Ordered {fmtShortDate(order.createdAt)}{fmtShortTime(order.createdAt) ? ` · ${fmtShortTime(order.createdAt)}` : ''}
                        </span>
                        <span className="rounded-full bg-slate-50 border border-slate-200 px-3 py-1">
                          Payment: {PAYMENT_METHOD_LABEL[order.paymentMethod] || 'Cash on Delivery'}
                        </span>
                        <span className="rounded-full bg-slate-50 border border-slate-200 px-3 py-1">
                          Total ₹{order.amount.toFixed(2)}
                        </span>
                      </div>
                    </div>
                  </div>

                  <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3 w-full lg:max-w-3xl">
                    <div className="rounded-xl bg-slate-50 border border-slate-200 p-3">
                      <p className="text-xs uppercase tracking-wide text-slate-500 font-semibold mb-1">Delivery</p>
                      <p className="text-sm text-on-surface line-clamp-2">{summarizeAddress(order.deliveryAddress)}</p>
                    </div>
                    <div className="rounded-xl bg-slate-50 border border-slate-200 p-3">
                      <p className="text-xs uppercase tracking-wide text-slate-500 font-semibold mb-1">Items</p>
                      <p className="text-sm text-on-surface line-clamp-2">
                        {order.itemNames?.length > 0
                          ? order.itemNames.slice(0, 2).join(', ')
                          : `${order.itemCount} item${order.itemCount > 1 ? 's' : ''}`}
                      </p>
                    </div>
                    <div className="rounded-xl bg-slate-50 border border-slate-200 p-3 sm:col-span-2 lg:col-span-1">
                      <p className="text-xs uppercase tracking-wide text-slate-500 font-semibold mb-1">ETA</p>
                      <p className="text-sm text-on-surface">
                        {order.estimatedDeliveryAt ? fmtDate(order.estimatedDeliveryAt) : (isActive(order.status) ? 'Tracking in progress' : 'Completed')}
                      </p>
                    </div>
                  </div>
                </div>

                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pt-1 border-t border-slate-100">
                  <div className={`text-xl font-bold text-on-surface ${cancelled ? 'line-through text-outline' : ''}`}>
                    ₹{order.amount.toFixed(2)}
                  </div>
                  {isActive(order.status) ? (
                    <Link
                      to={`/track/${order.id}`}
                      className="bg-primary hover:bg-primary-container text-on-primary text-sm font-semibold px-5 py-2 rounded-lg transition-colors flex items-center gap-2 w-full sm:w-auto justify-center"
                    >
                      <span className="material-symbols-outlined text-sm">local_shipping</span>
                      Track Order
                    </Link>
                  ) : cancelled ? (
                    <div className="flex gap-2">
                      <button
                        onClick={() => order.restaurantId && navigate(`/restaurant/${order.restaurantId}`)}
                        className="bg-slate-100 hover:bg-slate-200 text-on-surface text-sm font-semibold px-4 py-2 rounded-lg transition-colors flex items-center gap-2"
                      >
                        <span className="material-symbols-outlined text-sm">restaurant_menu</span>
                        View Menu
                      </button>
                    </div>
                  ) : order.status === 'Delivered' ? (
                    <div className="flex flex-col sm:flex-row gap-2 w-full sm:w-auto">
                      <Link
                        to={`/track/${order.id}`}
                        className="bg-primary hover:bg-primary-container text-on-primary text-sm font-semibold px-5 py-2 rounded-lg transition-colors flex items-center gap-2 w-full sm:w-auto justify-center"
                      >
                        <span className="material-symbols-outlined text-sm">receipt_long</span>
                        View Status
                      </Link>
                      <button
                        onClick={() => handleReorder(order.id)}
                        disabled={reordering === order.id}
                        className="bg-slate-100 hover:bg-slate-200 text-on-surface text-sm font-semibold px-5 py-2 rounded-lg transition-colors flex items-center gap-2 disabled:opacity-60 w-full sm:w-auto justify-center"
                      >
                        <span className="material-symbols-outlined text-sm">replay</span>
                        {reordering === order.id ? 'Reordering...' : 'Reorder'}
                      </button>
                    </div>
                  ) : (
                    <Link
                      to={`/track/${order.id}`}
                      className="bg-primary hover:bg-primary-container text-on-primary text-sm font-semibold px-5 py-2 rounded-lg transition-colors flex items-center gap-2 w-full sm:w-auto justify-center"
                    >
                      <span className="material-symbols-outlined text-sm">refresh</span>
                      View Details
                    </Link>
                  )}
                </div>
                </div>
              </div>
            )
          })}
        </div>
      </main>

      {/* Footer */}
      <footer className="bg-slate-900 py-10 mt-8">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 px-8 max-w-7xl mx-auto">
          <div>
            <div className="flex items-center gap-2 mb-2">
              <div className="w-7 h-7 bg-gradient-to-br from-primary to-indigo-600 rounded-lg flex items-center justify-center text-sm">🍔</div>
              <span className="text-base font-extrabold text-white">QuickBite</span>
            </div>
            <p className="text-sm text-slate-400">© 2024 QuickBite Food Delivery. All rights reserved.</p>
          </div>
          <div className="flex flex-wrap gap-x-6 gap-y-2 md:justify-end items-center">
            {['About Us', 'Terms of Service', 'Privacy Policy', 'Help Center'].map(l => (
              <a key={l} href="#" className="text-sm text-slate-400 hover:text-white transition-colors">{l}</a>
            ))}
          </div>
        </div>
      </footer>
    </div>
  )
}
