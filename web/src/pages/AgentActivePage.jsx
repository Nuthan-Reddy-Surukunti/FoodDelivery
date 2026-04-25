import { useState } from 'react'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'

const initialDeliveries = [
  { id: 'DL-921', restaurant: 'Burger Forge', customer: 'Asha V', status: 'Assigned' },
  { id: 'DL-918', restaurant: 'Fire Oven Pizza', customer: 'Rakesh P', status: 'Picked Up' },
]

export const AgentActivePage = () => {
  const [deliveries, setDeliveries] = useState(initialDeliveries)

  const advance = (id) => {
    setDeliveries((prev) => prev.map((delivery) => {
      if (delivery.id !== id) return delivery
      if (delivery.status === 'Assigned') return { ...delivery, status: 'Picked Up' }
      if (delivery.status === 'Picked Up') return { ...delivery, status: 'Delivered' }
      return delivery
    }))
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">Active Deliveries</h1>
      <div className="space-y-3">
        {deliveries.map((delivery) => (
          <Card key={delivery.id} className="flex items-center justify-between">
            <div>
              <p className="font-semibold">{delivery.id}</p>
              <p className="text-sm text-on-background/70">{delivery.restaurant} → {delivery.customer}</p>
              <p className="text-xs text-on-background/70">Status: {delivery.status}</p>
            </div>
            <Button size="sm" onClick={() => advance(delivery.id)} disabled={delivery.status === 'Delivered'}>
              Next Status
            </Button>
          </Card>
        ))}
      </div>
    </div>
  )
}
