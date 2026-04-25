import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Card } from '../components/atoms/Card'
import { useAuth } from '../context/AuthContext'
import orderApi from '../services/orderApi'

const normalizeOrders = (payload) => {
  const raw = Array.isArray(payload) ? payload : payload?.items || payload?.data || []
  return raw.map((item) => ({
    id: item.orderId || item.id,
    restaurant: item.restaurantName || item.restaurant?.name || 'Restaurant',
    amount: Number(item.totalAmount || item.totalPrice || 0),
    status: item.status || 'Unknown',
  }))
}

export const MyOrdersPage = () => {
  const { user } = useAuth()
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true

    const loadOrders = async () => {
      if (!user?.id) {
        setLoading(false)
        return
      }

      setLoading(true)
      setError('')
      try {
        const response = await orderApi.getOrdersByUser(user.id, false)
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
  }, [user?.id])

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">My Orders</h1>
      {loading ? <p className="text-sm text-on-background/70">Loading orders...</p> : null}
      {error ? <p className="text-sm text-error">{error}</p> : null}
      {!loading && !error && !orders.length ? <p className="text-sm text-on-background/70">No orders found.</p> : null}
      <div className="space-y-3">
        {orders.map((order) => (
          <Card key={order.id} className="flex items-center justify-between">
            <div>
              <p className="font-semibold">{order.id}</p>
              <p className="text-sm text-on-background/70">{order.restaurant}</p>
            </div>
            <div className="text-right">
              <p className="text-sm">₹{order.amount}</p>
              <p className="text-xs text-on-background/70">{order.status}</p>
              <Link to={`/track/${order.id}`} className="text-xs font-semibold text-primary">Track</Link>
            </div>
          </Card>
        ))}
      </div>
    </div>
  )
}
