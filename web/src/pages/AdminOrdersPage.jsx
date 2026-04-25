import { useState } from 'react'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'

const initialOrders = [
  { id: 'OD-1100', customer: 'Rohan P', restaurant: 'Spice Route Kitchen', status: 'Pending' },
  { id: 'OD-1098', customer: 'Ava M', restaurant: 'Burger Forge', status: 'Preparing' },
  { id: 'OD-1095', customer: 'Neha S', restaurant: 'Tokyo Roll House', status: 'Delivered' },
]

export const AdminOrdersPage = () => {
  const [orders, setOrders] = useState(initialOrders)

  const markCompleted = (id) => {
    setOrders((prev) => prev.map((order) => (order.id === id ? { ...order, status: 'Delivered' } : order)))
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">Admin Orders</h1>
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
