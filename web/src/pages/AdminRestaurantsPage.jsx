import { useCallback, useEffect, useState } from 'react'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'
import { useNotification } from '../hooks/useNotification'
import adminApi from '../services/adminApi'

const STATUS_COLORS = {
  Active: 'text-green-600 bg-green-50 border-green-200',
  PendingApproval: 'text-amber-600 bg-amber-50 border-amber-200',
  Pending: 'text-amber-600 bg-amber-50 border-amber-200',
  Rejected: 'text-red-600 bg-red-50 border-red-200',
  Inactive: 'text-gray-500 bg-gray-50 border-gray-200',
}

const normalize = (payload) => {
  const raw = Array.isArray(payload) ? payload : (payload?.items || payload?.data || [])
  return raw.map(item => ({
    id: item.id,
    name: item.name,
    city: item.city || '',
    status: item.status ?? 'Unknown',
    ownerEmail: item.contactEmail || item.ownerEmail || '',
    cuisine: item.cuisineTypeName || item.cuisineType || '',
  }))
}

export const AdminRestaurantsPage = () => {
  const { showSuccess, showError } = useNotification()
  const [restaurants, setRestaurants] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [tab, setTab] = useState('all') // 'all' | 'pending'
  const [actioning, setActioning] = useState(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const data = tab === 'pending'
        ? await adminApi.getPendingApprovals()
        : await adminApi.getRestaurants()
      setRestaurants(normalize(data))
    } catch (err) {
      setError(err.response?.data?.message || err.message || 'Failed to load restaurants')
    } finally {
      setLoading(false)
    }
  }, [tab])

  useEffect(() => { load() }, [load])

  const handleApprove = async (id) => {
    setActioning(id + '-approve')
    try {
      await adminApi.approveRestaurant(id)
      showSuccess('Restaurant approved')
      setRestaurants(prev => prev.map(r => r.id === id ? { ...r, status: 'Active' } : r))
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to approve')
    } finally {
      setActioning(null)
    }
  }

  const handleReject = async (id) => {
    const reason = window.prompt('Enter rejection reason:')
    if (!reason) return
    setActioning(id + '-reject')
    try {
      await adminApi.rejectRestaurant(id, reason)
      showSuccess('Restaurant rejected')
      setRestaurants(prev => prev.map(r => r.id === id ? { ...r, status: 'Rejected' } : r))
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to reject')
    } finally {
      setActioning(null)
    }
  }

  const handleDelete = async (id, name) => {
    if (!window.confirm(`Permanently delete "${name}"? This cannot be undone.`)) return
    setActioning(id + '-delete')
    try {
      await adminApi.deleteRestaurant(id)
      showSuccess('Restaurant deleted')
      setRestaurants(prev => prev.filter(r => r.id !== id))
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to delete')
    } finally {
      setActioning(null)
    }
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">Restaurant Management</h1>

      {/* Tabs */}
      <div className="mb-6 flex gap-2">
        <button
          onClick={() => setTab('all')}
          className={`rounded-full px-4 py-1.5 text-sm font-medium border transition ${tab === 'all' ? 'bg-primary text-on-primary border-primary' : 'border-outline hover:bg-surface-dim'}`}
        >
          All Restaurants
        </button>
        <button
          onClick={() => setTab('pending')}
          className={`rounded-full px-4 py-1.5 text-sm font-medium border transition ${tab === 'pending' ? 'bg-amber-500 text-white border-amber-500' : 'border-outline hover:bg-surface-dim'}`}
        >
          ⏳ Pending Approvals
        </button>
      </div>

      {loading && <p className="text-sm text-on-background/70">Loading restaurants...</p>}
      {error && <p className="text-sm text-error">{error}</p>}

      <div className="space-y-3">
        {restaurants.map(restaurant => {
          const statusKey = String(restaurant.status)
          const statusCls = STATUS_COLORS[statusKey] || STATUS_COLORS.Inactive
          const isActive = statusKey === 'Active'
          const isPending = statusKey === 'Pending' || statusKey === 'PendingApproval'
          const actionId = restaurant.id
          return (
            <Card key={restaurant.id} className="p-4">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div className="flex-1 min-w-0">
                  <p className="font-semibold">{restaurant.name}</p>
                  <div className="flex flex-wrap gap-3 mt-1 text-xs text-on-background/60">
                    {restaurant.city && <span>📍 {restaurant.city}</span>}
                    {restaurant.cuisine && <span>🍽️ {restaurant.cuisine}</span>}
                    {restaurant.ownerEmail && <span>✉️ {restaurant.ownerEmail}</span>}
                  </div>
                </div>
                <div className="flex flex-wrap items-center gap-2">
                  <span className={`rounded-full border px-3 py-1 text-xs font-semibold ${statusCls}`}>
                    {statusKey}
                  </span>
                  {isPending && (
                    <>
                      <Button
                        size="sm"
                        disabled={actioning === actionId + '-approve'}
                        onClick={() => handleApprove(actionId)}
                      >
                        Approve
                      </Button>
                      <Button
                        size="sm"
                        variant="tertiary"
                        disabled={actioning === actionId + '-reject'}
                        onClick={() => handleReject(actionId)}
                      >
                        Reject
                      </Button>
                    </>
                  )}
                  {isActive && (
                    <Button
                      size="sm"
                      variant="tertiary"
                      disabled={actioning === actionId + '-reject'}
                      onClick={() => handleReject(actionId)}
                    >
                      Deactivate
                    </Button>
                  )}
                  <Button
                    size="sm"
                    variant="tertiary"
                    disabled={actioning === actionId + '-delete'}
                    onClick={() => handleDelete(actionId, restaurant.name)}
                  >
                    🗑️
                  </Button>
                </div>
              </div>
            </Card>
          )
        })}
        {!loading && restaurants.length === 0 && (
          <div className="rounded-2xl border border-dashed border-outline p-10 text-center text-on-background/60">
            {tab === 'pending' ? 'No pending approvals.' : 'No restaurants found.'}
          </div>
        )}
      </div>
    </div>
  )
}
