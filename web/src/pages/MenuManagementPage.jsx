import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'
import { Modal } from '../components/atoms/Modal'
import { FormField } from '../components/molecules/FormField'
import { PartnerLayout } from '../components/organisms/PartnerLayout'
import { useNotification } from '../hooks/useNotification'
import catalogApi from '../services/catalogApi'

// ─── Menu Item Modal ─────────────────────────────────────────────────────────

const emptyItem = { name: '', description: '', price: '', categoryId: '', isVeg: true, prepTime: '', imageUrl: '', isAvailable: true }

const MenuItemModal = ({ isOpen, onClose, onSave, categories, initial }) => {
  const [form, setForm] = useState(initial || emptyItem)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    setForm(initial || emptyItem)
  }, [initial, isOpen])

  const onChange = (e) => {
    const { name, value, type, checked } = e.target
    setForm(prev => ({ ...prev, [name]: type === 'checkbox' ? checked : value }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setSaving(true)
    try {
      await onSave(form)
      onClose()
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={initial ? 'Edit Menu Item' : 'Add Menu Item'}>
      <form onSubmit={handleSubmit} className="space-y-4">
        <FormField label="Item Name" name="name" value={form.name} onChange={onChange} required />
        <FormField label="Description" name="description" value={form.description} onChange={onChange} />
        <div className="grid grid-cols-2 gap-4">
          <FormField label="Price (₹)" name="price" type="number" value={form.price} onChange={onChange} required />
          <FormField label="Prep Time (mins)" name="prepTime" type="number" value={form.prepTime} onChange={onChange} />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium text-on-background">Category</label>
          <select
            name="categoryId" value={form.categoryId} onChange={onChange} required
            className="w-full rounded-2xl border-2 border-outline bg-surface px-4 py-3 text-sm text-on-background"
          >
            <option value="">Select category...</option>
            {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
        </div>
        <div className="flex gap-6">
          <label className="flex items-center gap-2 cursor-pointer">
            <input type="checkbox" name="isVeg" checked={form.isVeg} onChange={onChange} className="h-4 w-4 rounded" />
            <span className="text-sm font-medium">Vegetarian item</span>
          </label>
          <label className="flex items-center gap-2 cursor-pointer">
            <input type="checkbox" name="isAvailable" checked={form.isAvailable} onChange={onChange} className="h-4 w-4 rounded text-primary" />
            <span className="text-sm font-medium text-on-surface">Available (In Stock)</span>
          </label>
        </div>
        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="secondary" onClick={onClose}>Cancel</Button>
          <Button type="submit" disabled={saving}>{saving ? 'Saving...' : 'Save Item'}</Button>
        </div>
      </form>
    </Modal>
  )
}

// ─── Category Modal ──────────────────────────────────────────────────────────

const CategoryModal = ({ isOpen, onClose, onSave, initial }) => {
  const [name, setName] = useState(initial?.name || '')
  const [saving, setSaving] = useState(false)

  useEffect(() => { setName(initial?.name || '') }, [initial, isOpen])

  const handleSubmit = async (e) => {
    e.preventDefault()
    setSaving(true)
    try {
      await onSave(name)
      onClose()
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={initial ? 'Edit Category' : 'Add Category'}>
      <form onSubmit={handleSubmit} className="space-y-4">
        <FormField label="Category Name" name="name" value={name} onChange={e => setName(e.target.value)} required />
        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="secondary" onClick={onClose}>Cancel</Button>
          <Button type="submit" disabled={saving}>{saving ? 'Saving...' : 'Save Category'}</Button>
        </div>
      </form>
    </Modal>
  )
}

// ─── Main Page ───────────────────────────────────────────────────────────────

export const MenuManagementPage = () => {
  const { showSuccess, showError } = useNotification()
  const [loadingRestaurant, setLoadingRestaurant] = useState(true)
  const [restaurant, setRestaurant] = useState(null)

  const [categories, setCategories] = useState([])
  const [items, setItems] = useState([])
  const [loadingData, setLoadingData] = useState(false)
  const [activeCategoryId, setActiveCategoryId] = useState(null)

  const [itemModal, setItemModal] = useState({ open: false, item: null })
  const [catModal, setCatModal] = useState({ open: false, cat: null })
  const [deletingId, setDeletingId] = useState(null)

  // Load restaurant info first
  useEffect(() => {
    let active = true
    const load = async () => {
      setLoadingRestaurant(true)
      try {
        const r = await catalogApi.getMyRestaurant()
        if (!active) return
        setRestaurant(r)
      } catch {
        if (!active) return
        setRestaurant(null)
      } finally {
        if (active) setLoadingRestaurant(false)
      }
    }
    load()
    return () => { active = false }
  }, [])

  // Load categories & items once restaurant is confirmed
  const loadData = useCallback(async (restaurantId) => {
    setLoadingData(true)
    try {
      const [cats, menuItems] = await Promise.all([
        catalogApi.getCategories(restaurantId),
        catalogApi.getRestaurantMenu(restaurantId),
      ])
      const catsArr = Array.isArray(cats) ? cats : (cats?.items || [])
      const itemsArr = Array.isArray(menuItems) ? menuItems : (menuItems?.items || menuItems?.data || [])
      setCategories(catsArr)
      setItems(itemsArr)
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to load menu data')
    } finally {
      setLoadingData(false)
    }
  }, [showError])

  useEffect(() => {
    if (restaurant?.id) loadData(restaurant.id)
  }, [restaurant?.id, loadData])

  // ── Guards ───────────────────────────────────────────────────────────────────

  if (loadingRestaurant) {
    return (
      <PartnerLayout title="Menu Management">
        <div className="space-y-4">
          {[1,2,3].map(i => <div key={i} className="h-24 bg-slate-200 animate-pulse rounded-xl" />)}
        </div>
      </PartnerLayout>
    )
  }

  if (!restaurant) {
    return (
      <PartnerLayout title="Menu Management">
        <div className="bg-amber-50 border border-amber-200 rounded-xl p-5 text-amber-800 text-sm">
          You need to create your restaurant profile first.
          <Link to="/partner/dashboard" className="ml-2 text-primary font-semibold">Complete restaurant setup →</Link>
        </div>
      </PartnerLayout>
    )
  }

  const statusLower = String(restaurant.status).toLowerCase()
  if (statusLower !== 'active') {
    return (
      <PartnerLayout title="Menu Management">
        <div className="bg-amber-50 border border-amber-200 rounded-xl p-5">
          <p className="text-sm text-amber-800">
            Your restaurant is <strong>{restaurant.status}</strong> and awaiting admin approval. Menu editing will be available after approval.
          </p>
          <Link to="/partner/dashboard" className="mt-3 inline-block text-sm font-semibold text-primary">← Back to Dashboard</Link>
        </div>
      </PartnerLayout>
    )
  }

  // ── Category CRUD ─────────────────────────────────────────────────────────────

  const handleSaveCategory = async (name) => {
    try {
      if (catModal.cat) {
        await catalogApi.updateCategory(catModal.cat.id, { restaurantId: restaurant.id, name })
        showSuccess('Category updated')
      } else {
        await catalogApi.createCategory({ restaurantId: restaurant.id, name, displayOrder: categories.length })
        showSuccess('Category created')
      }
      await loadData(restaurant.id)
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to save category')
      throw err
    }
  }

  const handleDeleteCategory = async (catId) => {
    if (!window.confirm('Delete this category? All items in it will be unassigned.')) return
    try {
      await catalogApi.deleteCategory(catId)
      showSuccess('Category deleted')
      if (activeCategoryId === catId) setActiveCategoryId(null)
      await loadData(restaurant.id)
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to delete category')
    }
  }

  // ── Menu Item CRUD ────────────────────────────────────────────────────────────

  const handleSaveItem = async (form) => {
    try {
      const payload = {
        restaurantId: restaurant.id,
        categoryId: form.categoryId,
        name: form.name,
        description: form.description || null,
        price: Number(form.price),
        isVeg: form.isVeg,
        prepTime: form.prepTime ? Number(form.prepTime) : null,
        imageUrl: form.imageUrl || null,
        availabilityStatus: form.isAvailable ? 1 : 2, // 1: Available, 2: OutOfStock
      }
      if (itemModal.item) {
        await catalogApi.updateMenuItem(itemModal.item.id, payload)
        showSuccess('Item updated')
      } else {
        await catalogApi.createMenuItem(payload)
        showSuccess('Item added to menu')
      }
      await loadData(restaurant.id)
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to save item')
      throw err
    }
  }

  const handleDeleteItem = async (itemId) => {
    if (!window.confirm('Remove this item from the menu?')) return
    setDeletingId(itemId)
    try {
      await catalogApi.deleteMenuItem(itemId)
      showSuccess('Item removed')
      await loadData(restaurant.id)
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to delete item')
    } finally {
      setDeletingId(null)
    }
  }

  const handleToggleAvailability = async (item) => {
    // Current is Available (1) or missing (assumed available). If so, change to OutOfStock (2). Else, to Available (1).
    const isCurrentlyAvailable = item.availabilityStatus === 1 || item.availabilityStatus === 'Available' || item.availabilityStatus == null
    const newStatus = isCurrentlyAvailable ? 2 : 1
    try {
      await catalogApi.toggleMenuItemAvailability(item.id, newStatus)
      showSuccess(`Item marked as ${isCurrentlyAvailable ? 'Out of Stock' : 'Available'}`)
      
      // Update local state without full reload
      setItems(prev => prev.map(i => i.id === item.id ? { ...i, availabilityStatus: newStatus } : i))
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to update availability')
    }
  }

  const openEditItem = (item) => {
    const isAvailable = item.availabilityStatus === 1 || item.availabilityStatus === 'Available' || item.availabilityStatus == null;
    setItemModal({
      open: true,
      item: {
        ...item,
        price: String(item.price),
        prepTime: item.prepTime != null ? String(item.prepTime) : '',
        categoryId: categories.find(c => c.name === item.categoryName)?.id || '',
        isAvailable: isAvailable
      }
    })
  }

  // ── Filtered items ────────────────────────────────────────────────────────────

  const visibleItems = activeCategoryId
    ? items.filter(item => categories.find(c => c.id === activeCategoryId && c.name === item.categoryName))
    : items

  const availabilityLabel = (status) => {
    if (status === 1 || status === 'Available' || status == null) return { label: 'Available', cls: 'text-green-600 font-semibold bg-green-50 px-2 py-0.5 rounded' }
    if (status === 2 || status === 'OutOfStock') return { label: 'Out of Stock', cls: 'text-red-600 font-semibold bg-red-50 px-2 py-0.5 rounded' }
    return { label: 'Unavailable', cls: 'text-gray-500 font-semibold bg-gray-50 px-2 py-0.5 rounded' }
  }

  return (
    <PartnerLayout title={`${restaurant.name} — Menu`}>
      {/* Add Item button */}
      <div className="flex justify-end">
        <button
          onClick={() => setItemModal({ open: true, item: null })}
          className="bg-primary text-on-primary px-5 py-2.5 rounded-xl text-sm font-semibold hover:bg-primary-container transition-colors flex items-center gap-2"
        >
          <span className="material-symbols-outlined text-sm">add</span> Add Item
        </button>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-4">
        {/* Categories sidebar */}
        <div className="lg:col-span-1">
          <div className="mb-3 flex items-center justify-between">
            <h2 className="font-semibold text-sm text-on-background/70 uppercase tracking-wide">Categories</h2>
            <button onClick={() => setCatModal({ open: true, cat: null })} className="text-xs font-semibold text-primary hover:underline">+ Add</button>
          </div>
          <div className="space-y-1">
            <button
              onClick={() => setActiveCategoryId(null)}
              className={`w-full rounded-xl px-3 py-2 text-left text-sm font-medium transition ${!activeCategoryId ? 'bg-primary text-on-primary' : 'hover:bg-surface-dim'}`}
            >
              All Items ({items.length})
            </button>
            {categories.map(cat => (
              <div key={cat.id} className={`group flex items-center justify-between rounded-xl px-3 py-2 transition ${activeCategoryId === cat.id ? 'bg-primary text-on-primary' : 'hover:bg-surface-dim'}`}>
                <button onClick={() => setActiveCategoryId(cat.id)} className="flex-1 text-left text-sm font-medium">
                  {cat.name} <span className="opacity-60 text-xs">({cat.itemCount ?? 0})</span>
                </button>
                <div className="hidden gap-1 group-hover:flex">
                  <button onClick={() => setCatModal({ open: true, cat })} className="text-xs px-1 hover:text-primary" title="Edit">✏️</button>
                  <button onClick={() => handleDeleteCategory(cat.id)} className="text-xs px-1 hover:text-red-500" title="Delete">🗑️</button>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Items list */}
        <div className="lg:col-span-3">
          {loadingData ? (
            <p className="text-sm text-on-background/70">Loading menu items...</p>
          ) : visibleItems.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-outline p-10 text-center">
              <p className="text-on-background/60 mb-3">No menu items yet.</p>
              <Button onClick={() => setItemModal({ open: true, item: null })}>Add your first item</Button>
            </div>
          ) : (
            <div className="space-y-3">
              {visibleItems.map(item => {
                const avail = availabilityLabel(item.availabilityStatus)
                return (
                  <Card key={item.id} className="flex items-center gap-4 p-4">
                    {/* Veg badge */}
                    <span className={`flex-shrink-0 h-5 w-5 rounded-full border-2 ${item.isVeg ? 'border-green-500' : 'border-red-500'} flex items-center justify-center`}>
                      <span className={`h-2.5 w-2.5 rounded-full ${item.isVeg ? 'bg-green-500' : 'bg-red-500'}`} />
                    </span>

                    <div className="flex-1 min-w-0">
                      <p className="font-semibold truncate">{item.name}</p>
                      {item.description && <p className="text-xs text-on-background/60 truncate">{item.description}</p>}
                      <div className="flex flex-wrap gap-3 mt-1 text-xs text-on-background/70">
                        <span>₹{item.price}</span>
                        {item.categoryName && <span className="rounded bg-surface-dim px-1.5 py-0.5">{item.categoryName}</span>}
                        {item.prepTime && <span>⏱ {item.prepTime}m</span>}
                        <span className={avail.cls}>{avail.label}</span>
                      </div>
                    </div>

                    <div className="flex flex-col gap-2 flex-shrink-0 min-w-[120px]">
                      <button
                        onClick={() => handleToggleAvailability(item)}
                        className={`text-xs font-semibold py-1.5 px-3 rounded-md transition-colors ${
                          (item.availabilityStatus === 1 || item.availabilityStatus === 'Available' || item.availabilityStatus == null)
                            ? 'bg-red-50 text-red-600 hover:bg-red-100 border border-red-200'
                            : 'bg-green-50 text-green-600 hover:bg-green-100 border border-green-200'
                        }`}
                      >
                        {(item.availabilityStatus === 1 || item.availabilityStatus === 'Available' || item.availabilityStatus == null) 
                          ? 'Mark Out of Stock' 
                          : 'Mark Available'}
                      </button>
                      <div className="flex gap-2">
                        <Button className="flex-1" size="sm" variant="secondary" onClick={() => openEditItem(item)}>Edit</Button>
                        <Button
                          className="flex-1" size="sm" variant="tertiary"
                          disabled={deletingId === item.id}
                          onClick={() => handleDeleteItem(item.id)}
                        >
                          {deletingId === item.id ? '...' : 'Del'}
                        </Button>
                      </div>
                    </div>
                  </Card>
                )
              })}
            </div>
          )}
        </div>
      </div>

      {/* Modals */}
      <MenuItemModal
        isOpen={itemModal.open}
        onClose={() => setItemModal({ open: false, item: null })}
        onSave={handleSaveItem}
        categories={categories}
        initial={itemModal.item}
      />
      <CategoryModal
        isOpen={catModal.open}
        onClose={() => setCatModal({ open: false, cat: null })}
        onSave={handleSaveCategory}
        initial={catModal.cat}
      />
    </PartnerLayout>
  )
}
