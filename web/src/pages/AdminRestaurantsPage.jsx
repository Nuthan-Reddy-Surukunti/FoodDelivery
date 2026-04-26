import { useCallback, useEffect, useState } from 'react'
import { AdminLayout } from '../components/organisms/AdminLayout'
import { useNotification } from '../hooks/useNotification'
import adminApi from '../services/adminApi'

const STATUS_BADGE = {
  Active: 'bg-emerald-100 text-emerald-800',
  PendingApproval: 'bg-amber-100 text-amber-800',
  Pending: 'bg-amber-100 text-amber-800',
  Rejected: 'bg-red-100 text-red-800',
  Inactive: 'bg-slate-100 text-slate-600',
}

const normalize = (payload) => {
  const raw = Array.isArray(payload) ? payload : (payload?.items || payload?.data || [])
  return raw.map(item => ({
    id: item.id,
    name: item.name,
    city: item.city || '',
    status: String(item.status ?? 'Unknown'),
    ownerEmail: item.contactEmail || item.ownerEmail || '',
    cuisine: item.cuisineTypeName || item.cuisineType || '',
    phone: item.contactPhone || '',
  }))
}

export const AdminRestaurantsPage = () => {
  const { showSuccess, showError } = useNotification()
  const [restaurants, setRestaurants] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [tab, setTab] = useState('all')
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
    } finally { setActioning(null) }
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
    } finally { setActioning(null) }
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
    } finally { setActioning(null) }
  }

  return (
    <AdminLayout title="Restaurant Management" searchPlaceholder="Search restaurants...">
      {error && <div className="bg-error-container text-on-error-container px-4 py-3 rounded-xl text-sm">{error}</div>}

      {/* Tabs */}
      <div className="flex gap-1 border-b border-slate-200">
        <button
          onClick={() => setTab('all')}
          className={`px-5 py-2.5 text-sm font-medium transition-colors ${tab === 'all' ? 'text-primary border-b-2 border-primary' : 'text-slate-500 hover:text-on-surface'}`}
        >
          All Restaurants
        </button>
        <button
          onClick={() => setTab('pending')}
          className={`px-5 py-2.5 text-sm font-medium transition-colors flex items-center gap-2 ${tab === 'pending' ? 'text-amber-600 border-b-2 border-amber-500' : 'text-slate-500 hover:text-on-surface'}`}
        >
          <span className="material-symbols-outlined text-sm">pending</span>
          Pending Approvals
        </button>
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden">
        {loading ? (
          <div className="p-6 space-y-3">
            {[1,2,3,4].map(i => <div key={i} className="h-16 bg-slate-100 animate-pulse rounded-xl" />)}
          </div>
        ) : restaurants.length === 0 ? (
          <div className="py-16 text-center text-on-surface-variant text-sm">
            {tab === 'pending' ? 'No pending approvals.' : 'No restaurants found.'}
          </div>
        ) : (
          <div className="divide-y divide-slate-50">
            {restaurants.map(r => {
              const badgeClass = STATUS_BADGE[r.status] || 'bg-slate-100 text-slate-600'
              const isPending = r.status === 'Pending' || r.status === 'PendingApproval'
              const isActive = r.status === 'Active'

              return (
                <div key={r.id} className="p-5 hover:bg-slate-50 transition-colors flex flex-col md:flex-row md:items-center justify-between gap-4">
                  <div className="flex items-start gap-4 flex-1">
                    {/* Restaurant icon */}
                    <div className="w-11 h-11 rounded-xl bg-slate-100 flex items-center justify-center shrink-0 border border-slate-200">
                      <span className="material-symbols-outlined text-slate-500 text-xl">storefront</span>
                    </div>
                    <div>
                      <h4 className="font-semibold text-on-surface">{r.name}</h4>
                      <div className="flex flex-wrap items-center gap-3 mt-0.5 text-xs text-on-surface-variant">
                        {r.city && <span className="flex items-center gap-0.5"><span className="material-symbols-outlined text-sm">location_on</span>{r.city}</span>}
                        {r.cuisine && <span>{r.cuisine}</span>}
                        {r.ownerEmail && <span className="flex items-center gap-0.5"><span className="material-symbols-outlined text-sm">email</span>{r.ownerEmail}</span>}
                      </div>
                    </div>
                  </div>

                  <div className="flex items-center gap-2 flex-shrink-0">
                    <span className={`${badgeClass} text-xs font-semibold px-3 py-1 rounded-full`}>{r.status}</span>
                    {isPending && (
                      <>
                        <button
                          disabled={actioning === r.id + '-approve'}
                          onClick={() => handleApprove(r.id)}
                          className="bg-primary text-on-primary text-xs font-semibold px-4 py-1.5 rounded-lg hover:bg-primary-container transition-colors disabled:opacity-50"
                        >
                          {actioning === r.id + '-approve' ? '...' : 'Approve'}
                        </button>
                        <button
                          disabled={actioning === r.id + '-reject'}
                          onClick={() => handleReject(r.id)}
                          className="border border-red-300 text-red-600 text-xs font-semibold px-4 py-1.5 rounded-lg hover:bg-red-50 transition-colors disabled:opacity-50"
                        >
                          Reject
                        </button>
                      </>
                    )}
                    {isActive && (
                      <button
                        disabled={actioning === r.id + '-reject'}
                        onClick={() => handleReject(r.id)}
                        className="border border-slate-300 text-slate-600 text-xs font-semibold px-4 py-1.5 rounded-lg hover:bg-slate-100 transition-colors disabled:opacity-50"
                      >
                        Deactivate
                      </button>
                    )}
                    <button
                      disabled={actioning === r.id + '-delete'}
                      onClick={() => handleDelete(r.id, r.name)}
                      className="border border-red-200 text-red-500 text-xs font-semibold px-3 py-1.5 rounded-lg hover:bg-red-50 transition-colors disabled:opacity-50"
                    >
                      <span className="material-symbols-outlined text-base">delete</span>
                    </button>
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>
    </AdminLayout>
  )
}
