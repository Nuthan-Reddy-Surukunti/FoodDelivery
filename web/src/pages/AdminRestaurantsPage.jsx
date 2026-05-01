import { useCallback, useEffect, useState } from 'react'
import { AdminLayout } from '../components/organisms/AdminLayout'
import { CategoryFilter } from '../components/molecules/CategoryFilter'
import { RestaurantTable } from '../components/molecules/RestaurantTable'
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
    rating: item.rating || 0,
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
  const [selectedCategory, setSelectedCategory] = useState('all')
  const [currentPage, setCurrentPage] = useState(1)
  const [actioning, setActioning] = useState(null)
  const pageSize = 10

  const load = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const data = tab === 'pending'
        ? await adminApi.getPendingApprovals()
        : await adminApi.getRestaurants()
      setRestaurants(normalize(data))
      setCurrentPage(1)
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
      showSuccess('Restaurant approved ✅')
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
      showSuccess('Restaurant rejected ❌')
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
      showSuccess('Restaurant deleted 🗑️')
      setRestaurants(prev => prev.filter(r => r.id !== id))
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to delete')
    } finally { setActioning(null) }
  }

  const handleDeactivate = async (id, name) => {
    const reason = window.prompt(`Enter reason for deactivating "${name}":`)
    if (!reason) return
    setActioning(id + '-deactivate')
    try {
      await adminApi.deactivateRestaurant(id, reason)
      showSuccess('Restaurant deactivated ⏸️')
      setRestaurants(prev => prev.map(r => r.id === id ? { ...r, status: 'Inactive' } : r))
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to deactivate')
    } finally { setActioning(null) }
  }

  const handleActivate = async (id, name) => {
    if (!window.confirm(`Reactivate "${name}"?`)) return
    setActioning(id + '-activate')
    try {
      await adminApi.activateRestaurant(id)
      showSuccess('Restaurant reactivated 🔋')
      setRestaurants(prev => prev.map(r => r.id === id ? { ...r, status: 'Active' } : r))
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to reactivate')
    } finally { setActioning(null) }
  }

  // Filter restaurants based on selected category
  const filteredRestaurants = selectedCategory === 'all' 
    ? restaurants 
    : restaurants.filter(r => r.cuisine?.toLowerCase().includes(selectedCategory.toLowerCase()))

  // Paginate
  const totalItems = filteredRestaurants.length
  const paginatedRestaurants = filteredRestaurants.slice(
    (currentPage - 1) * pageSize,
    currentPage * pageSize
  )

  return (
    <AdminLayout title="Restaurant Management" searchPlaceholder="Search restaurants...">
      {error && <div className="bg-error-container text-on-error-container px-4 py-3 rounded-xl text-sm mb-6">⚠️ {error}</div>}

      {/* Header with Add Button */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="font-headline-lg text-headline-lg text-on-surface">Restaurants</h1>
          <p className="font-body-md text-body-md text-on-surface-variant mt-1">Manage partner restaurants, menus, and statuses.</p>
        </div>
        <button className="bg-primary text-on-primary px-4 py-2 rounded-lg font-label-md text-label-md hover:bg-primary-container transition-colors flex items-center gap-2">
          <span className="material-symbols-outlined">add</span>
          Add New Restaurant
        </button>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 border-b border-slate-200 mb-6">
        <button
          onClick={() => { setTab('all'); setCurrentPage(1) }}
          className={`px-5 py-2.5 text-sm font-medium transition-colors ${tab === 'all' ? 'text-primary border-b-2 border-primary' : 'text-slate-500 hover:text-on-surface'}`}
        >
          All Restaurants 🍽️
        </button>
        <button
          onClick={() => { setTab('pending'); setCurrentPage(1) }}
          className={`px-5 py-2.5 text-sm font-medium transition-colors flex items-center gap-2 ${tab === 'pending' ? 'text-amber-600 border-b-2 border-amber-500' : 'text-slate-500 hover:text-on-surface'}`}
        >
          <span className="material-symbols-outlined text-sm">pending</span>
          Pending Approvals
        </button>
      </div>

      {/* Category Filter */}
      <div className="mb-6">
        <CategoryFilter selected={selectedCategory} onSelect={setSelectedCategory} />
      </div>

      {/* Table */}
      <RestaurantTable
        restaurants={paginatedRestaurants}
        loading={loading}
        totalItems={totalItems}
        currentPage={currentPage}
        pageSize={pageSize}
        onPageChange={setCurrentPage}
        onEdit={(restaurant) => console.log('Edit:', restaurant)}
        onDelete={handleDelete}
        onApprove={handleApprove}
        onReject={handleReject}
        onDeactivate={handleDeactivate}
        onActivate={handleActivate}
      />
    </AdminLayout>
  )
}
