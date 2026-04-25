import { useParams } from 'react-router-dom'
import { OrderTracker } from '../components/organisms/OrderTracker'

export const OrderTrackingPage = () => {
  const { orderId } = useParams()

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="mb-1 text-2xl font-bold">Track Order</h1>
      <p className="mb-5 text-sm text-on-background/70">Order ID: {orderId}</p>
      <OrderTracker currentStatusIndex={2} />
    </div>
  )
}
