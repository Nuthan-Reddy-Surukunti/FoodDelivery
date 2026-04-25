import { Link } from 'react-router-dom'
import { Card } from '../components/atoms/Card'

const orders = [
  { id: 'OD-1024', restaurant: 'Fire Oven Pizza', amount: 729, status: 'Preparing' },
  { id: 'OD-1023', restaurant: 'Burger Forge', amount: 439, status: 'Delivered' },
]

export const MyOrdersPage = () => {
  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">My Orders</h1>
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
