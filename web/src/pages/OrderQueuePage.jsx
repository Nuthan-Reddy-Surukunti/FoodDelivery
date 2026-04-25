import { useEffect, useState } from 'react'
import { Card } from '../components/atoms/Card'
import { Button } from '../components/atoms/Button'
import partnerApi from '../services/partnerApi'
import { useAuth } from '../context/AuthContext'

export const OrderQueuePage = () => {
  const { user } = useAuth()
  const [orders, setOrders] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const fetchOrders = async () => {
      try {
        // Assume user object has the restaurantId for the partner
        const data = await partnerApi.getIncomingOrders(user?.restaurantId || user?.id)
        // If data is an array return it, else check items or data
        const items = Array.isArray(data) ? data : (data?.items || [])
        setOrders(items)
      } catch (err) {
        console.error("Failed to fetch incoming orders:", err)
      } finally {
        setLoading(false)
      }
    }
    
    if (user) {
        fetchOrders()
    }
  }, [user])

  const handleUpdateStatus = async (orderId, status) => {
    try {
      await partnerApi.updateOrderStatus(orderId, status)
      setOrders(prev => prev.map(o => (o.orderId || o.id) === orderId ? { ...o, orderStatus: status } : o))
    } catch (err) {
      console.error("Failed to update status:", err)
    }
  }

  if (loading) return <div className="p-8 text-center">Loading orders...</div>

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-bold">Order Queue</h1>
      <div className="space-y-4">
        {orders.map(order => {
          const id = order.orderId || order.id;
          return (
            <Card key={id} className="flex flex-col sm:flex-row justify-between items-start sm:items-center p-4">
              <div className="mb-4 sm:mb-0">
                <p className="font-semibold">Order #{id.split('-')[0]}</p>
                <p className="text-sm text-on-background/70">Status: <span className="font-medium text-primary">{order.orderStatus}</span></p>
                <p className="text-sm text-on-background/70">Total: {order.total} {order.currency}</p>
              </div>
              <div className="flex gap-2">
                <Button 
                  size="sm" 
                  variant="outline"
                  disabled={order.orderStatus !== 'Paid'}
                  onClick={() => handleUpdateStatus(id, 'Preparing')}
                >
                  Prepare
                </Button>
                <Button 
                  size="sm" 
                  disabled={order.orderStatus !== 'Preparing'}
                  onClick={() => handleUpdateStatus(id, 'ReadyForPickup')}
                >
                  Ready for Pickup
                </Button>
              </div>
            </Card>
          )
        })}
        {orders.length === 0 && (
          <div className="p-8 text-center text-on-background/70 border border-dashed rounded-xl">
            No incoming orders at the moment.
          </div>
        )}
      </div>
    </div>
  )
}
