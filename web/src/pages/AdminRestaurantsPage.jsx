import { useState } from 'react'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'

const initialRestaurants = [
  { id: 'RST-77', name: 'Urban Tacos', owner: 'Ravi M', status: 'Pending' },
  { id: 'RST-72', name: 'Bowl Theory', owner: 'Isha K', status: 'Active' },
  { id: 'RST-70', name: 'Noodle Nation', owner: 'Arjun T', status: 'Pending' },
]

export const AdminRestaurantsPage = () => {
  const [restaurants, setRestaurants] = useState(initialRestaurants)

  const setStatus = (id, status) => {
    setRestaurants((prev) => prev.map((item) => (item.id === id ? { ...item, status } : item)))
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">Restaurant Management</h1>
      <div className="space-y-3">
        {restaurants.map((restaurant) => (
          <Card key={restaurant.id} className="flex items-center justify-between">
            <div>
              <p className="font-semibold">{restaurant.name}</p>
              <p className="text-sm text-on-background/70">Owner: {restaurant.owner} • {restaurant.status}</p>
            </div>
            <div className="flex gap-2">
              <Button size="sm" onClick={() => setStatus(restaurant.id, 'Active')}>Approve</Button>
              <Button size="sm" variant="tertiary" onClick={() => setStatus(restaurant.id, 'Rejected')}>Reject</Button>
            </div>
          </Card>
        ))}
      </div>
    </div>
  )
}
