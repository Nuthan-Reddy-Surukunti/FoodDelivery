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
  const [searchQuery, setSearchQuery] = useState('')
  const [editingRestaurant, setEditingRestaurant] = useState(null)
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

  // Derived categories from fetched restaurants
  const dynamicCategories = [
    { label: 'All Categories', value: 'all' },
    ...Array.from(new Set(restaurants.map(r => r.cuisine).filter(Boolean))).map(c => ({
      label: c,
      value: c.toLowerCase(),
    })),
  ]

  // Filter restaurants based on search and selected category
  const searchedRestaurants = restaurants.filter(r => 
    r.name?.toLowerCase().includes(searchQuery.toLowerCase()) || 
    r.city?.toLowerCase().includes(searchQuery.toLowerCase())
  )

  const filteredRestaurants = selectedCategory === 'all' 
    ? searchedRestaurants 
    : searchedRestaurants.filter(r => r.cuisine?.toLowerCase() === selectedCategory.toLowerCase())

  // Paginate
  const totalItems = filteredRestaurants.length
  const paginatedRestaurants = filteredRestaurants.slice(
    (currentPage - 1) * pageSize,
    currentPage * pageSize
  )

  const handleSaveEdit = async () => {
    if (!editingRestaurant.name) return showError('Name is required')
    setActioning('edit')
    try {
      const payload = {
        id: editingRestaurant.id,
        name: editingRestaurant.name,
        cuisineType: editingRestaurant.cuisine,
        city: editingRestaurant.city,
      }
      await adminApi.updateRestaurant(editingRestaurant.id, payload)
      showSuccess('Restaurant updated successfully')
      setRestaurants(prev => prev.map(r => r.id === editingRestaurant.id ? { ...r, name: payload.name, cuisine: payload.cuisineType, city: payload.city } : r))
      setEditingRestaurant(null)
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to update restaurant')
    } finally {
      setActioning(null)
    }
  }

  return (
    <AdminLayout 
      title="Restaurant Management" 
      searchPlaceholder="Search restaurants by name or city..."
      searchQuery={searchQuery}
      onSearchChange={setSearchQuery}
    >
      {error && <div className="bg-error-container text-on-error-container px-4 py-3 rounded-xl text-sm mb-6">⚠️ {error}</div>}

      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="font-headline-lg text-headline-lg text-on-surface">Restaurants</h1>
          <p className="font-body-md text-body-md text-on-surface-variant mt-1">Manage partner restaurants, menus, and statuses.</p>
        </div>
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
        <CategoryFilter categories={dynamicCategories} selected={selectedCategory} onSelect={setSelectedCategory} />
      </div>

      {/* Table */}
      <RestaurantTable
        restaurants={paginatedRestaurants}
        loading={loading}
        totalItems={totalItems}
        currentPage={currentPage}
        pageSize={pageSize}
        onPageChange={setCurrentPage}
        onEdit={setEditingRestaurant}
        onDelete={handleDelete}
        onApprove={handleApprove}
        onReject={handleReject}
        onDeactivate={handleDeactivate}
        onActivate={handleActivate}
      />

      {/* Edit Modal */}
      {editingRestaurant && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-md overflow-hidden flex flex-col">
            <div className="p-6 border-b border-slate-200 flex justify-between items-center">
              <h3 className="text-lg font-bold text-slate-900">Edit Restaurant</h3>
              <button onClick={() => setEditingRestaurant(null)} className="text-slate-400 hover:text-slate-600">
                <span className="material-symbols-outlined">close</span>
              </button>
            </div>
            <div className="p-6 space-y-4 flex-1 overflow-y-auto">
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Name</label>
                <input
                  type="text"
                  value={editingRestaurant.name}
                  onChange={(e) => setEditingRestaurant({ ...editingRestaurant, name: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Category / Cuisine</label>
                <select
                  value={editingRestaurant.cuisine}
                  onChange={(e) => setEditingRestaurant({ ...editingRestaurant, cuisine: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
                >
                  <option value="">Select Cuisine</option>
                  <option value="Italian">Italian</option>
                  <option value="Chinese">Chinese</option>
                  <option value="Indian">Indian</option>
                  <option value="Mexican">Mexican</option>
                  <option value="American">American</option>
                  <option value="Thai">Thai</option>
                  <option value="Japanese">Japanese</option>
                  <option value="Continental">Continental</option>
                  <option value="FastFood">Fast Food</option>
                  <option value="Vegan">Vegan</option>
                  <option value="Mediterranean">Mediterranean</option>
                  <option value="Other">Other</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">City</label>
                <input
                  type="text"
                  value={editingRestaurant.city}
                  onChange={(e) => setEditingRestaurant({ ...editingRestaurant, city: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary"
                />
              </div>
            </div>
            <div className="p-6 border-t border-slate-200 bg-slate-50 flex justify-end gap-3">
              <button
                onClick={() => setEditingRestaurant(null)}
                className="px-4 py-2 rounded-lg font-medium text-slate-600 hover:bg-slate-200 transition-colors"
              >
                Cancel
              </button>
              <button
                onClick={handleSaveEdit}
                disabled={actioning === 'edit'}
                className="px-4 py-2 rounded-lg font-medium bg-primary text-white hover:bg-rose-500 transition-colors disabled:opacity-50"
              >
                {actioning === 'edit' ? 'Saving...' : 'Save Changes'}
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  )
}
