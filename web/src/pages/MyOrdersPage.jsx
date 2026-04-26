import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
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

const normalizeOrder = (item) => ({
  id: item.orderId || item.id,
  restaurantId: item.restaurantId,
  restaurantName: item.restaurantName || item.restaurant?.name || '',
  amount: Number(item.total || item.totalAmount || item.totalPrice || 0),
  status: item.orderStatus || item.status || 'Unknown',
  createdAt: item.createdAt || item.placedAt,
  itemCount: item.items?.length ?? item.itemCount ?? 0,
  imageUrl: item.restaurantImageUrl ?? null,
})

export const MyOrdersPage = () => {
  const { user } = useAuth()
  const navigate = useNavigate()
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [tab, setTab] = useState('all') // all | active | past

  useEffect(() => {
    if (!user?.id) { setLoading(false); return }
    let active = true
    setLoading(true)
    setError('')
    orderApi.getOrdersByUser(user.id, false)
      .then((res) => {
        if (!active) return
        const raw = Array.isArray(res) ? res : (res?.items || res?.data || [])
        setOrders(raw.map(normalizeOrder))
      })
      .catch((err) => {
        if (!active) return
        setError(err.response?.data?.message || err.message || 'Failed to load orders')
      })
      .finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [user?.id])

  const filtered = orders.filter((o) => {
    if (tab === 'active') return isActive(o.status)
    if (tab === 'past') return !isActive(o.status)
    return true
  })

  return (
    <div className="bg-background min-h-screen">
      <main className="pt-8 px-6 pb-16 max-w-5xl mx-auto w-full">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-[32px] font-bold text-on-surface mb-1">My Orders</h1>
          <p className="text-on-surface-variant text-sm">View and manage your recent dining experiences.</p>
        </div>

        {/* Filter tabs */}
        <div className="flex gap-1 mb-8 border-b border-slate-200">
          {[['all', 'All'], ['active', 'Active'], ['past', 'Past']].map(([key, label]) => (
            <button
              key={key}
              onClick={() => setTab(key)}
              className={`px-5 py-2.5 text-sm font-medium transition-colors ${tab === key ? 'text-primary border-b-2 border-primary' : 'text-slate-500 hover:text-on-surface'}`}
            >
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
                className={`bg-white border border-outline-variant rounded-xl p-6 shadow-sm flex flex-col md:flex-row gap-6 items-start md:items-center justify-between ${cancelled ? 'opacity-70' : ''}`}
              >
                <div className="flex gap-5 items-center w-full md:w-auto">
                  {/* Restaurant image placeholder */}
                  <div className="w-20 h-20 rounded-xl overflow-hidden flex-shrink-0 bg-slate-100 flex items-center justify-center text-3xl">
                    {order.imageUrl ? (
                      <img src={order.imageUrl} alt={order.restaurantName} className={`w-full h-full object-cover ${cancelled ? 'grayscale' : ''}`} />
                    ) : '🏪'}
                  </div>
                  <div className="flex flex-col gap-1">
                    <div className="flex items-center gap-2 mb-1 flex-wrap">
                      <span className={`${badgeClass} text-xs font-semibold px-2.5 py-1 rounded-full`}>{statusText}</span>
                      <span className="text-on-surface-variant text-sm">#{String(order.id).split('-')[0].toUpperCase()}</span>
                    </div>
                    <h2 className="text-xl font-semibold text-on-surface leading-tight">
                      {order.restaurantName || 'Restaurant'}
                    </h2>
                    <p className="text-sm text-on-surface-variant">
                      {order.itemCount > 0 ? `${order.itemCount} item${order.itemCount > 1 ? 's' : ''} · ` : ''}{fmtDate(order.createdAt)}
                    </p>
                  </div>
                </div>

                <div className="flex flex-row md:flex-col items-center md:items-end justify-between w-full md:w-auto gap-4 md:gap-2">
                  <div className={`text-xl font-bold text-on-surface ${cancelled ? 'line-through text-outline' : ''}`}>
                    ₹{order.amount.toFixed(2)}
                  </div>
                  {isActive(order.status) ? (
                    <Link
                      to={`/track/${order.id}`}
                      className="bg-primary hover:bg-primary-container text-on-primary text-sm font-semibold px-5 py-2 rounded-lg transition-colors flex items-center gap-2"
                    >
                      <span className="material-symbols-outlined text-sm">local_shipping</span>
                      Track Order
                    </Link>
                  ) : cancelled ? (
                    <button
                      onClick={() => order.restaurantId && navigate(`/restaurant/${order.restaurantId}`)}
                      className="bg-slate-100 hover:bg-slate-200 text-on-surface text-sm font-semibold px-5 py-2 rounded-lg transition-colors flex items-center gap-2"
                    >
                      <span className="material-symbols-outlined text-sm">restaurant_menu</span>
                      View Menu
                    </button>
                  ) : (
                    <Link
                      to={`/track/${order.id}`}
                      className="bg-primary hover:bg-primary-container text-on-primary text-sm font-semibold px-5 py-2 rounded-lg transition-colors flex items-center gap-2"
                    >
                      <span className="material-symbols-outlined text-sm">refresh</span>
                      View Details
                    </Link>
                  )}
                </div>
              </div>
            )
          })}
        </div>
      </main>

      {/* Footer */}
      <footer className="bg-slate-50 border-t border-slate-200 py-12 mt-8">
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
