import { useState } from 'react'
import { useEffect } from 'react'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'
import adminApi from '../services/adminApi'

const normalizeOrders = (payload) => {
  const raw = Array.isArray(payload) ? payload : payload?.items || payload?.data || []
  return raw.map((item) => ({
    id: item.id || item.orderId,
    customer: item.customerName || item.userName || item.userId || 'Customer',
    restaurant: item.restaurantName || item.restaurant?.name || 'Restaurant',
    status: item.status || 'Pending',
  }))
}

export const AdminOrdersPage = () => {
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true

    const loadOrders = async () => {
      setLoading(true)
      setError('')
      try {
        const response = await adminApi.getOrders()
        if (!active) return
        setOrders(normalizeOrders(response))
      } catch (err) {
        if (!active) return
        setError(err.response?.data?.message || err.message || 'Failed to load orders')
      } finally {
        if (active) {
          setLoading(false)
        }
      }
    }

    loadOrders()
    return () => {
      active = false
    }
  }, [])

  const markCompleted = async (id) => {
    try {
      await adminApi.updateOrderStatus(id, 'Delivered')
      setOrders((prev) => prev.map((order) => (order.id === id ? { ...order, status: 'Delivered' } : order)))
    } catch {
      // Keep UI stable if status transition fails server-side.
    }
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">Admin Orders</h1>
      {loading ? <p className="text-sm text-on-background/70">Loading orders...</p> : null}
      {error ? <p className="text-sm text-error">{error}</p> : null}
      <div className="space-y-3">
        {orders.map((order) => (
          <Card key={order.id} className="flex items-center justify-between">
            <div>
              <p className="font-semibold">{order.id}</p>
              <p className="text-sm text-on-background/70">{order.customer} • {order.restaurant}</p>
            </div>
            <div className="flex items-center gap-3">
              <span className="text-xs text-on-background/70">{order.status}</span>
              <Button size="sm" variant="secondary" onClick={() => markCompleted(order.id)}>
                Mark Delivered
              </Button>
            </div>
          </Card>
        ))}
      </div>
    </div>
  )
}
