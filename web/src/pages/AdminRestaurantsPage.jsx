import { useState } from 'react'
import { useEffect } from 'react'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'
import adminApi from '../services/adminApi'

const normalizeRestaurants = (payload) => {
  const raw = Array.isArray(payload) ? payload : payload?.items || payload?.data || []
  return raw.map((item) => ({
    id: item.id,
    name: item.name,
    owner: item.ownerName || item.ownerId || 'Owner',
    status: item.status || 'Pending',
  }))
}

export const AdminRestaurantsPage = () => {
  const [restaurants, setRestaurants] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true

    const loadRestaurants = async () => {
      setLoading(true)
      setError('')
      try {
        const response = await adminApi.getRestaurants()
        if (!active) return
        setRestaurants(normalizeRestaurants(response))
      } catch (err) {
        if (!active) return
        setError(err.response?.data?.message || err.message || 'Failed to load restaurants')
      } finally {
        if (active) {
          setLoading(false)
        }
      }
    }

    loadRestaurants()
    return () => {
      active = false
    }
  }, [])

  const setStatus = async (id, status) => {
    try {
      if (status === 'Active') {
        await adminApi.approveRestaurant(id)
      }
      if (status === 'Rejected') {
        await adminApi.rejectRestaurant(id, 'Rejected from dashboard')
      }
      setRestaurants((prev) => prev.map((item) => (item.id === id ? { ...item, status } : item)))
    } catch {
      // Preserve list state if action fails.
    }
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">Restaurant Management</h1>
      {loading ? <p className="text-sm text-on-background/70">Loading restaurants...</p> : null}
      {error ? <p className="text-sm text-error">{error}</p> : null}
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
