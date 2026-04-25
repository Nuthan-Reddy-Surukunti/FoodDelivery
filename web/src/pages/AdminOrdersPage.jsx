import { useCallback, useEffect, useState } from 'react'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'
import { useNotification } from '../hooks/useNotification'
import adminApi from '../services/adminApi'

const STATUS_FILTERS = ['All', 'Paid', 'Preparing', 'ReadyForPickup', 'OutForDelivery', 'Delivered', 'Cancelled']

const STATUS_COLORS = {
  Paid: 'text-blue-600 bg-blue-50',
  RestaurantAccepted: 'text-indigo-600 bg-indigo-50',
  Preparing: 'text-amber-600 bg-amber-50',
  ReadyForPickup: 'text-orange-600 bg-orange-50',
  PickedUp: 'text-teal-600 bg-teal-50',
  OutForDelivery: 'text-cyan-600 bg-cyan-50',
  Delivered: 'text-green-600 bg-green-50',
  Cancelled: 'text-red-600 bg-red-50',
  RestaurantRejected: 'text-red-600 bg-red-50',
  CancelRequestedByCustomer: 'text-rose-600 bg-rose-50',
}

const normalize = (payload) => {
  const raw = Array.isArray(payload) ? payload : (payload?.items || payload?.data || [])
  return raw.map(item => ({
    id: item.orderId || item.id,
    restaurant: item.restaurantName || item.restaurant?.name || 'Restaurant',
    userId: item.userId,
    status: item.orderStatus || item.status || 'Unknown',
    total: Number(item.total || item.totalAmount || 0),
    createdAt: item.createdAt,
    itemCount: item.items?.length ?? 0,
  }))
}

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

  useEffect(() => {
    loadOrders(activeFilter)
  }, [activeFilter, loadOrders])

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
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">Orders Management</h1>

      {/* Status filter tabs */}
      <div className="mb-6 flex flex-wrap gap-2">
        {STATUS_FILTERS.map(f => (
          <button
            key={f}
            onClick={() => setActiveFilter(f)}
            className={`rounded-full px-4 py-1.5 text-sm font-medium border transition ${activeFilter === f ? 'bg-primary text-on-primary border-primary' : 'border-outline hover:bg-surface-dim'}`}
          >
            {f}
          </button>
        ))}
      </div>

      {loading && <p className="text-sm text-on-background/70">Loading orders...</p>}
      {error && <p className="text-sm text-error">{error}</p>}

      <div className="space-y-3">
        {orders.map(order => {
          const statusCls = STATUS_COLORS[order.status] || 'text-gray-600 bg-gray-50'
          const isActioning = actioning === order.id
          return (
            <Card key={order.id} className="p-4">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div className="flex-1 min-w-0">
                  <p className="font-semibold text-sm">#{order.id.split('-')[0].toUpperCase()}</p>
                  <p className="text-sm text-on-background/70 mt-0.5">{order.restaurant}</p>
                  <div className="flex flex-wrap gap-3 mt-2 text-xs text-on-background/60">
                    {order.itemCount > 0 && <span>{order.itemCount} items</span>}
                    <span className="font-semibold text-on-background">₹{order.total.toLocaleString()}</span>
                    {order.createdAt && (
                      <span>{new Date(order.createdAt).toLocaleString('en-IN', { dateStyle: 'short', timeStyle: 'short' })}</span>
                    )}
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <span className={`rounded-full px-3 py-1 text-xs font-semibold ${statusCls}`}>{order.status}</span>
                  {order.status !== 'Delivered' && order.status !== 'Cancelled' && (
                    <Button size="sm" variant="secondary" disabled={isActioning} onClick={() => handleMarkDelivered(order.id)}>
                      {isActioning ? '...' : 'Force Deliver'}
                    </Button>
                  )}
                </div>
              </div>
            </Card>
          )
        })}
        {!loading && orders.length === 0 && (
          <div className="rounded-2xl border border-dashed border-outline p-10 text-center text-on-background/60">
            No orders found{activeFilter !== 'All' ? ` with status "${activeFilter}"` : ''}.
          </div>
        )}
      </div>
    </div>
  )
}
