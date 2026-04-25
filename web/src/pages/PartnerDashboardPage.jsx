import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'
import { FormField } from '../components/molecules/FormField'
import { useNotification } from '../hooks/useNotification'
import catalogApi from '../services/catalogApi'

const CUISINE_OPTIONS = [
  { label: 'Italian', value: 1 },
  { label: 'Chinese', value: 2 },
  { label: 'Indian', value: 3 },
  { label: 'Mexican', value: 4 },
  { label: 'American', value: 5 },
  { label: 'Thai', value: 6 },
  { label: 'Japanese', value: 7 },
  { label: 'Continental', value: 8 },
  { label: 'Fast Food', value: 9 },
  { label: 'Vegan', value: 10 },
  { label: 'Mediterranean', value: 11 },
  { label: 'Other', value: 12 },
]

const STATUS_COLORS = {
  Active: 'text-green-600 bg-green-50 border-green-200',
  Pending: 'text-amber-600 bg-amber-50 border-amber-200',
  PendingApproval: 'text-amber-600 bg-amber-50 border-amber-200',
  Rejected: 'text-red-600 bg-red-50 border-red-200',
  Inactive: 'text-gray-600 bg-gray-50 border-gray-200',
}

const emptyForm = {
  name: '', description: '', address: '', city: '',
  serviceZoneId: '', cuisineType: '3', contactPhone: '',
  contactEmail: '', minOrderValue: '', deliveryTime: '',
}

export const PartnerDashboardPage = () => {
  const { showSuccess, showError } = useNotification()
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [restaurant, setRestaurant] = useState(null)
  const [editMode, setEditMode] = useState(false)
  const [form, setForm] = useState(emptyForm)

  useEffect(() => {
    let active = true
    const load = async () => {
      setLoading(true)
      try {
        const response = await catalogApi.getMyRestaurant()
        if (!active) return
        setRestaurant(response)
      } catch (err) {
        if (!active) return
        if (err?.response?.status !== 404) {
          showError(err.response?.data?.message || err.message || 'Failed to load restaurant data')
        }
      } finally {
        if (active) setLoading(false)
      }
    }
    load()
    return () => { active = false }
  }, [showError])

  const openEdit = () => {
    setForm({
      name: restaurant.name || '',
      description: restaurant.description || '',
      address: restaurant.address || '',
      city: restaurant.city || '',
      serviceZoneId: restaurant.serviceZoneId || '',
      cuisineType: String(restaurant.cuisineType ?? 3),
      contactPhone: restaurant.contactPhone || '',
      contactEmail: restaurant.contactEmail || '',
      minOrderValue: restaurant.minOrderValue ?? '',
      deliveryTime: restaurant.deliveryTime ?? '',
    })
    setEditMode(true)
  }

  const updateField = (e) => {
    const { name, value } = e.target
    setForm(prev => ({ ...prev, [name]: value }))
  }

  const handleCreate = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    try {
      const payload = {
        name: form.name, description: form.description || null,
        address: form.address, city: form.city, serviceZoneId: form.serviceZoneId,
        cuisineType: Number(form.cuisineType),
        contactPhone: form.contactPhone || null, contactEmail: form.contactEmail || null,
        minOrderValue: form.minOrderValue ? Number(form.minOrderValue) : null,
        deliveryTime: form.deliveryTime ? Number(form.deliveryTime) : null,
      }
      const created = await catalogApi.createRestaurant(payload)
      setRestaurant(created)
      showSuccess('Restaurant profile created. Awaiting admin approval.')
    } catch (err) {
      showError(err.response?.data?.message || err.message || 'Failed to create restaurant')
    } finally {
      setSubmitting(false)
    }
  }

  const handleUpdate = async (e) => {
    e.preventDefault()
    setSubmitting(true)
    try {
      const payload = {
        id: restaurant.id, name: form.name, description: form.description || null,
        address: form.address, city: form.city, serviceZoneId: form.serviceZoneId,
        cuisineType: Number(form.cuisineType),
        contactPhone: form.contactPhone || null, contactEmail: form.contactEmail || null,
        minOrderValue: form.minOrderValue ? Number(form.minOrderValue) : null,
        deliveryTime: form.deliveryTime ? Number(form.deliveryTime) : null,
      }
      const updated = await catalogApi.updateRestaurant(restaurant.id, payload)
      setRestaurant(updated)
      setEditMode(false)
      showSuccess('Restaurant updated successfully.')
    } catch (err) {
      showError(err.response?.data?.message || err.message || 'Failed to update restaurant')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <div className="mx-auto max-w-6xl px-4 py-8">
        <p className="text-sm text-on-background/70">Loading partner setup...</p>
      </div>
    )
  }

  // ── Create form ─────────────────────────────────────────────────────────────
  if (!restaurant) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8">
        <h1 className="mb-2 text-2xl font-bold">Complete Partner Setup</h1>
        <p className="mb-6 text-sm text-on-background/70">
          Create your restaurant profile to continue with menu management and incoming orders.
        </p>
        <RestaurantForm form={form} onChange={updateField} onSubmit={handleCreate} submitting={submitting} submitLabel="Create Restaurant" />
      </div>
    )
  }

  const statusKey = String(restaurant.status)
  const statusColor = STATUS_COLORS[statusKey] || STATUS_COLORS.Inactive
  const isActive = statusKey.toLowerCase() === 'active'
  const cuisineLabel = CUISINE_OPTIONS.find(c => c.value === Number(restaurant.cuisineType))?.label || restaurant.cuisineType

  // ── Edit form ───────────────────────────────────────────────────────────────
  if (editMode) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8">
        <div className="mb-6 flex items-center gap-3">
          <button onClick={() => setEditMode(false)} className="text-sm text-on-background/60 hover:text-primary">← Back</button>
          <h1 className="text-2xl font-bold">Edit Restaurant</h1>
        </div>
        <RestaurantForm form={form} onChange={updateField} onSubmit={handleUpdate} submitting={submitting} submitLabel="Save Changes" />
      </div>
    )
  }

  // ── Dashboard ───────────────────────────────────────────────────────────────
  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-bold">Partner Dashboard</h1>
        <Button variant="secondary" onClick={openEdit}>Edit Restaurant</Button>
      </div>

      {/* Restaurant card */}
      <Card className="mb-6 p-5">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h2 className="text-xl font-semibold">{restaurant.name}</h2>
            {restaurant.description && <p className="mt-1 text-sm text-on-background/70">{restaurant.description}</p>}
            <p className="mt-2 text-sm text-on-background/70">{restaurant.address}, {restaurant.city}</p>
          </div>
          <span className={`rounded-full border px-3 py-1 text-xs font-semibold ${statusColor}`}>
            {statusKey}
          </span>
        </div>

        <div className="mt-4 grid grid-cols-2 gap-4 sm:grid-cols-4">
          <Stat label="Cuisine" value={cuisineLabel} />
          <Stat label="Rating" value={`${Number(restaurant.rating ?? 0).toFixed(1)} ★`} />
          <Stat label="Delivery Time" value={restaurant.deliveryTime ? `${restaurant.deliveryTime} mins` : 'N/A'} />
          <Stat label="Min Order" value={restaurant.minOrderValue ? `₹${restaurant.minOrderValue}` : 'N/A'} />
        </div>

        {restaurant.contactPhone && (
          <p className="mt-3 text-sm text-on-background/70">📞 {restaurant.contactPhone}</p>
        )}
        {restaurant.contactEmail && (
          <p className="text-sm text-on-background/70">✉️ {restaurant.contactEmail}</p>
        )}

        {!isActive && (
          <p className="mt-4 rounded-lg bg-amber-50 p-3 text-sm text-amber-700">
            ⚠️ Your restaurant is <strong>{statusKey}</strong> and awaiting admin approval. Menu editing and live orders will be available once approved.
          </p>
        )}
      </Card>

      {/* Quick links */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <QuickLink to="/partner/menu" icon="🍽️" title="Menu Management" desc="Add, edit & remove menu items and categories" />
        <QuickLink to="/partner/queue" icon="📋" title="Order Queue" desc="View and manage incoming customer orders" />
      </div>
    </div>
  )
}

const Stat = ({ label, value }) => (
  <div className="rounded-xl bg-surface-dim p-3">
    <p className="text-xs text-on-background/60">{label}</p>
    <p className="mt-1 font-semibold">{value}</p>
  </div>
)

const QuickLink = ({ to, icon, title, desc }) => (
  <Link to={to} className="block rounded-2xl border border-outline bg-surface p-5 transition hover:border-primary hover:shadow-md">
    <div className="text-3xl mb-2">{icon}</div>
    <p className="font-semibold">{title}</p>
    <p className="mt-1 text-xs text-on-background/60">{desc}</p>
  </Link>
)

const RestaurantForm = ({ form, onChange, onSubmit, submitting, submitLabel }) => (
  <form onSubmit={onSubmit} className="space-y-4 rounded-2xl border border-outline bg-surface p-5">
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      <FormField label="Restaurant Name" name="name" value={form.name} onChange={onChange} required />
      <FormField label="City" name="city" value={form.city} onChange={onChange} required />
    </div>
    <FormField label="Description" name="description" value={form.description} onChange={onChange} />
    <FormField label="Address" name="address" value={form.address} onChange={onChange} required />
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      <FormField label="Service Zone ID" name="serviceZoneId" value={form.serviceZoneId} onChange={onChange} required />
      <div>
        <label htmlFor="cuisineType" className="mb-1 block text-sm font-medium text-on-background">Cuisine Type</label>
        <select
          id="cuisineType" name="cuisineType" value={form.cuisineType} onChange={onChange}
          className="w-full rounded-2xl border-2 border-outline bg-surface px-4 py-3 text-sm text-on-background"
        >
          {[{ label: 'Italian', value: 1 }, { label: 'Chinese', value: 2 }, { label: 'Indian', value: 3 }, { label: 'Mexican', value: 4 }, { label: 'American', value: 5 }, { label: 'Thai', value: 6 }, { label: 'Japanese', value: 7 }, { label: 'Continental', value: 8 }, { label: 'Fast Food', value: 9 }, { label: 'Vegan', value: 10 }, { label: 'Mediterranean', value: 11 }, { label: 'Other', value: 12 }].map(o => (
            <option key={o.value} value={String(o.value)}>{o.label}</option>
          ))}
        </select>
      </div>
    </div>
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      <FormField label="Contact Phone" name="contactPhone" value={form.contactPhone} onChange={onChange} />
      <FormField label="Contact Email" name="contactEmail" type="email" value={form.contactEmail} onChange={onChange} />
    </div>
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      <FormField label="Min Order Value (₹)" name="minOrderValue" type="number" value={form.minOrderValue} onChange={onChange} />
      <FormField label="Delivery Time (mins)" name="deliveryTime" type="number" value={form.deliveryTime} onChange={onChange} />
    </div>
    <Button type="submit" disabled={submitting}>{submitting ? 'Saving...' : submitLabel}</Button>
  </form>
)
