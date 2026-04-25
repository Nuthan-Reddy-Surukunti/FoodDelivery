import { useEffect, useMemo, useState } from 'react'
import { useParams } from 'react-router-dom'
import { OrderTracker } from '../components/organisms/OrderTracker'
import orderApi from '../services/orderApi'

const statusToIndex = {
  pending: 0,
  placed: 0,
  confirmed: 1,
  preparing: 2,
  pickedup: 3,
  picked_up: 3,
  delivered: 4,
}

export const OrderTrackingPage = () => {
  const { orderId } = useParams()
  const [status, setStatus] = useState('Pending')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true

    const loadOrder = async () => {
      setLoading(true)
      setError('')
      try {
        const response = await orderApi.getOrderById(orderId)
        if (!active) return
        const currentStatus = response?.order?.status || response?.status || 'Pending'
        setStatus(currentStatus)
      } catch (err) {
        if (!active) return
        setError(err.response?.data?.message || err.message || 'Failed to load order tracking')
      } finally {
        if (active) {
          setLoading(false)
        }
      }
    }

    if (orderId) {
      loadOrder()
    }

    return () => {
      active = false
    }
  }, [orderId])

  const trackerIndex = useMemo(() => {
    return statusToIndex[String(status).replace(/\s+/g, '').toLowerCase()] ?? 0
  }, [status])

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="mb-1 text-2xl font-bold">Track Order</h1>
      <p className="mb-5 text-sm text-on-background/70">Order ID: {orderId}</p>
      {loading ? <p className="text-sm text-on-background/70">Loading tracking details...</p> : null}
      {error ? <p className="text-sm text-error">{error}</p> : null}
      {!loading && !error ? <OrderTracker currentStatusIndex={trackerIndex} /> : null}
    </div>
  )
}
