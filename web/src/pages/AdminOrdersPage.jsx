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
  const [selectedOrderId, setSelectedOrderId] = useState(null)
  const [orderDetails, setOrderDetails] = useState(null)
  const [loadingDetails, setLoadingDetails] = useState(false)

  const handleViewDetails = async (id) => {
    setSelectedOrderId(id)
    setLoadingDetails(true)
    try {
      const data = await adminApi.getOrderDetails(id)
      setOrderDetails(data)
    } catch (err) {
      showError('Failed to load order details')
    } finally {
      setLoadingDetails(false)
    }
  }

  const closeDetails = () => {
    setSelectedOrderId(null)
    setOrderDetails(null)
  }

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
                    <button
                      onClick={() => handleViewDetails(order.id)}
                      className="text-xs font-medium text-slate-600 bg-slate-100 px-3 py-1.5 rounded-lg hover:bg-slate-200 transition-colors"
                    >
                      View Details
                    </button>
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

      {/* Order Details Modal */}
      {selectedOrderId && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-hidden flex flex-col">
            <div className="p-6 border-b border-slate-200 flex justify-between items-center bg-slate-50">
              <h3 className="text-xl font-bold text-slate-900 flex items-center gap-2">
                <span className="material-symbols-outlined text-primary">receipt_long</span>
                Order Details
              </h3>
              <button onClick={closeDetails} className="text-slate-400 hover:text-slate-600 transition-colors p-1 rounded-lg hover:bg-slate-200">
                <span className="material-symbols-outlined">close</span>
              </button>
            </div>
            
            <div className="flex-1 overflow-y-auto p-6">
              {loadingDetails ? (
                <div className="flex flex-col items-center justify-center py-12 space-y-4">
                  <div className="w-8 h-8 border-4 border-primary/20 border-t-primary rounded-full animate-spin" />
                  <p className="text-sm text-slate-500 font-medium">Loading details...</p>
                </div>
              ) : orderDetails?.order ? (
                <div className="space-y-6">
                  {/* Order Meta */}
                  <div className="grid grid-cols-2 gap-4">
                    <div className="bg-slate-50 p-4 rounded-xl border border-slate-100">
                      <p className="text-[11px] font-bold text-slate-400 uppercase tracking-wider mb-1">Order ID</p>
                      <p className="text-sm font-semibold text-slate-800 break-all">{orderDetails.order.orderId}</p>
                    </div>
                    <div className="bg-slate-50 p-4 rounded-xl border border-slate-100">
                      <p className="text-[11px] font-bold text-slate-400 uppercase tracking-wider mb-1">Status</p>
                      <span className={`inline-block px-2.5 py-1 rounded-md text-xs font-bold ${STATUS_BADGE[orderDetails.order.orderStatus] || 'bg-slate-200 text-slate-700'}`}>
                        {orderDetails.order.orderStatus}
                      </span>
                    </div>
                  </div>

                  {/* Order Items */}
                  <div>
                    <h4 className="text-sm font-bold text-slate-800 mb-3 uppercase tracking-wider flex items-center gap-2 border-b border-slate-100 pb-2">
                      <span className="material-symbols-outlined text-base text-slate-400">restaurant_menu</span>
                      Items
                    </h4>
                    <div className="space-y-3">
                      {orderDetails.order.items?.map((item, idx) => (
                        <div key={idx} className="flex justify-between items-start">
                          <div>
                            <p className="text-sm font-medium text-slate-800">
                              <span className="text-slate-400 mr-2">{item.quantity}x</span>
                              {item.menuItemName || `Item #${String(item.menuItemId).split('-')[0]}`}
                            </p>
                            {item.customizationNotes && (
                              <p className="text-xs text-slate-500 mt-0.5 ml-6 italic">Note: {item.customizationNotes}</p>
                            )}
                          </div>
                          <p className="text-sm font-semibold text-slate-800">₹{item.subtotal.toFixed(2)}</p>
                        </div>
                      ))}
                    </div>
                    
                    <div className="mt-4 pt-3 border-t border-slate-100 flex justify-between items-center">
                      <p className="text-sm font-bold text-slate-800">Total Amount</p>
                      <p className="text-lg font-black text-primary">₹{orderDetails.order.total.toFixed(2)}</p>
                    </div>
                  </div>

                  {/* Customer & Delivery */}
                  <div>
                    <h4 className="text-sm font-bold text-slate-800 mb-3 uppercase tracking-wider flex items-center gap-2 border-b border-slate-100 pb-2">
                      <span className="material-symbols-outlined text-base text-slate-400">local_shipping</span>
                      Delivery Details
                    </h4>
                    {orderDetails.order.deliveryAddress ? (
                      <div className="bg-slate-50 p-4 rounded-xl border border-slate-100">
                        <p className="text-sm font-semibold text-slate-800">{orderDetails.order.deliveryAddress.street}</p>
                        <p className="text-sm text-slate-600 mt-1">{orderDetails.order.deliveryAddress.city}, {orderDetails.order.deliveryAddress.state} {orderDetails.order.deliveryAddress.zipCode}</p>
                      </div>
                    ) : (
                      <p className="text-sm text-slate-500 italic">No delivery address provided.</p>
                    )}
                  </div>
                </div>
              ) : (
                <div className="py-12 text-center">
                  <span className="material-symbols-outlined text-4xl text-slate-300 mb-2">error</span>
                  <p className="text-slate-500">Could not load order details.</p>
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  )
}
