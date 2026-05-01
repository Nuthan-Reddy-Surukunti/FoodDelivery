import { useCallback, useEffect, useState } from 'react'
import { AdminLayout } from '../components/organisms/AdminLayout'
import { useNotification } from '../hooks/useNotification'
import adminApi from '../services/adminApi'

const STATUS_FILTERS = ['All', 'Pending', 'Confirmed', 'Preparing', 'Ready', 'OutForDelivery', 'Delivered', 'Cancelled']

const STATUS_BADGE = {
  Pending: 'bg-sky-100 text-sky-800',
  Confirmed: 'bg-teal-100 text-teal-800',
  Preparing: 'bg-amber-100 text-amber-800',
  Ready: 'bg-orange-100 text-orange-800',
  OutForDelivery: 'bg-purple-100 text-purple-800',
  Delivered: 'bg-emerald-100 text-emerald-800',
  Cancelled: 'bg-red-100 text-red-800',
  RestaurantRejected: 'bg-red-100 text-red-800',
}

// Map backend enum values (ints) to strings if needed
const STATUS_MAP = {
  1: 'Pending',
  2: 'Confirmed',
  3: 'Preparing',
  4: 'Ready',
  5: 'OutForDelivery',
  6: 'Delivered',
  7: 'Cancelled'
}

const normalize = (payload) => {
  const raw = Array.isArray(payload) ? payload : (payload?.items || payload?.data || [])
  return raw.map(item => {
    let status = item.orderStatus || item.status || 'Unknown'
    if (STATUS_MAP[status]) status = STATUS_MAP[status]
    
    return {
      id: item.orderId || item.id,
      restaurant: item.restaurantName || item.restaurant?.name || 'Restaurant',
      userId: item.userId,
      customerEmail: item.customerEmail || item.userId || '',
      status: status,
      total: Number(item.total || item.totalAmount || 0),
      createdAt: item.createdAt,
      itemCount: item.items?.length ?? item.itemCount ?? 0,
    }
  })
}

const fmtDate = (iso) => iso ? new Date(iso).toLocaleString('en-IN', { dateStyle: 'short', timeStyle: 'short' }) : ''

export const AdminOrdersPage = () => {
  const { showSuccess, showError } = useNotification()
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [activeFilter, setActiveFilter] = useState('All')
  const [actioning, setActioning] = useState(null)

  const loadOrders = useCallback(async (status) => {
    setLoading(true)
    setError('')
    try {
      const response = await adminApi.getOrders(status === 'All' ? undefined : status)
      setOrders(normalize(response))
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to load orders')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { loadOrders(activeFilter) }, [activeFilter, loadOrders])

  const handleMarkDelivered = async (id) => {
    setActioning(id)
    try {
      await adminApi.updateOrderStatus(id, 'Delivered', 'Admin override')
      showSuccess('Order marked as Delivered')
      setOrders(prev => prev.map(o => o.id === id ? { ...o, status: 'Delivered' } : o))
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to update order')
    } finally {
      setActioning(null)
    }
  }

  return (
    <AdminLayout title="Orders Management" searchPlaceholder="Search orders...">
      {error && <div className="bg-error-container text-on-error-container px-4 py-3 rounded-xl text-sm">{error}</div>}

      {/* Filter tabs */}
      <div className="flex gap-1 border-b border-slate-200 overflow-x-auto no-scrollbar">
        {STATUS_FILTERS.map(f => (
          <button
            key={f}
            onClick={() => setActiveFilter(f)}
            className={`px-4 py-2.5 text-sm font-medium whitespace-nowrap transition-colors ${activeFilter === f ? 'text-primary border-b-2 border-primary' : 'text-slate-500 hover:text-on-surface'}`}
          >
            {f}
          </button>
        ))}
      </div>

      {/* Orders table */}
      <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden">
        {loading ? (
          <div className="p-6 space-y-3">
            {[1,2,3,4,5].map(i => <div key={i} className="h-14 bg-slate-100 animate-pulse rounded-xl" />)}
          </div>
        ) : orders.length === 0 ? (
          <div className="py-16 text-center text-on-surface-variant text-sm">
            No orders{activeFilter !== 'All' ? ` with status "${activeFilter}"` : ''}.
          </div>
        ) : (
          <div className="divide-y divide-slate-50">
            {orders.map(order => {
              const badgeClass = STATUS_BADGE[order.status] || 'bg-slate-100 text-slate-700'
              const isActioning = actioning === order.id
              const canForce = !['Delivered', 'Cancelled', 'RestaurantRejected'].includes(order.status)

              return (
                <div key={order.id} className="p-5 hover:bg-slate-50 transition-colors flex flex-col md:flex-row md:items-center justify-between gap-4">
                  <div className="flex items-start gap-4 flex-1">
                    <div className="bg-slate-100 h-11 w-11 rounded-lg flex items-center justify-center shrink-0 border border-slate-200">
                      <span className="text-xs font-bold text-slate-500">#{String(order.id).split('-')[0].slice(0, 4).toUpperCase()}</span>
                    </div>
                    <div>
                      <h4 className="font-semibold text-on-surface text-sm">{order.restaurant}</h4>
                      <p className="text-xs text-on-surface-variant mt-0.5">
                        {order.customerEmail && `${order.customerEmail} · `}{fmtDate(order.createdAt)}
                      </p>
                      <div className="flex items-center gap-2 mt-1">
                        {order.itemCount > 0 && <span className="text-xs text-slate-400">{order.itemCount} items</span>}
                        <span className="text-xs font-semibold text-on-surface">₹{order.total.toLocaleString()}</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-3 flex-shrink-0">
                    <span className={`${badgeClass} text-xs font-semibold px-3 py-1 rounded-full`}>{order.status}</span>
                    {canForce && (
                      <button
                        disabled={isActioning}
                        onClick={() => handleMarkDelivered(order.id)}
                        className="text-xs font-medium text-primary border border-primary px-3 py-1.5 rounded-lg hover:bg-primary hover:text-white transition-colors disabled:opacity-50"
                      >
                        {isActioning ? '...' : 'Force Deliver'}
                      </button>
                    )}
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>
    </AdminLayout>
  )
}
