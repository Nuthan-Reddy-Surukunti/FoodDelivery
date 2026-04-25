import { useState } from 'react'
import { useEffect } from 'react'
import { Link } from 'react-router-dom'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'
import catalogApi from '../services/catalogApi'

const initialItems = [
  { id: 'pm1', name: 'Paneer Roll', price: 199, active: true },
  { id: 'pm2', name: 'Veg Biryani', price: 249, active: true },
  { id: 'pm3', name: 'Chocolate Lava Cake', price: 149, active: false },
]

export const MenuManagementPage = () => {
  const [loadingRestaurant, setLoadingRestaurant] = useState(true)
  const [hasRestaurant, setHasRestaurant] = useState(false)
  const [restaurantStatus, setRestaurantStatus] = useState('')
  const [items, setItems] = useState(initialItems)

  useEffect(() => {
    let active = true

    const loadMyRestaurant = async () => {
      setLoadingRestaurant(true)
      try {
        const response = await catalogApi.getMyRestaurant()
        if (!active) return
        setHasRestaurant(true)
        setRestaurantStatus(String(response?.status || ''))
      } catch {
        if (!active) return
        setHasRestaurant(false)
      } finally {
        if (active) {
          setLoadingRestaurant(false)
        }
      }
    }

    loadMyRestaurant()
    return () => {
      active = false
    }
  }, [])

  const toggleActive = (id) => {
    setItems((prev) => prev.map((item) => (item.id === id ? { ...item, active: !item.active } : item)))
  }

  if (loadingRestaurant) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-8">
        <p className="text-sm text-on-background/70">Loading menu setup...</p>
      </div>
    )
  }

  if (!hasRestaurant) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-8">
        <Card>
          <h1 className="text-2xl font-bold">Menu Management</h1>
          <p className="mt-2 text-sm text-on-background/70">
            You need to create your restaurant profile first before managing menu items.
          </p>
          <Link to="/partner/dashboard" className="mt-3 inline-block text-sm font-semibold text-primary">
            Complete restaurant setup
          </Link>
        </Card>
      </div>
    )
  }

  if (restaurantStatus.toLowerCase() !== 'active') {
    return (
      <div className="mx-auto max-w-5xl px-4 py-8">
        <Card>
          <h1 className="text-2xl font-bold">Menu Management</h1>
          <p className="mt-2 text-sm text-amber-700">
            Your restaurant is currently {restaurantStatus || 'pending'} and awaiting admin approval.
          </p>
          <p className="mt-1 text-sm text-on-background/70">
            Menu editing will be available after approval.
          </p>
        </Card>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <div className="mb-5 flex items-center justify-between">
        <h1 className="text-2xl font-bold">Menu Management</h1>
        <Button size="sm">Add Item</Button>
      </div>

      <div className="space-y-3">
        {items.map((item) => (
          <Card key={item.id} className="flex items-center justify-between">
            <div>
              <p className="font-semibold">{item.name}</p>
              <p className="text-sm text-on-background/70">₹{item.price}</p>
            </div>
            <Button size="sm" variant={item.active ? 'secondary' : 'tertiary'} onClick={() => toggleActive(item.id)}>
              {item.active ? 'Mark Inactive' : 'Mark Active'}
            </Button>
          </Card>
        ))}
      </div>
    </div>
  )
}
