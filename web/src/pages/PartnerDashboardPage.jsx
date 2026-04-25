import { useEffect, useMemo, useState } from 'react'
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

export const PartnerDashboardPage = () => {
  const { showSuccess, showError } = useNotification()
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [restaurant, setRestaurant] = useState(null)
  const [form, setForm] = useState({
    name: '',
    description: '',
    address: '',
    city: '',
    serviceZoneId: '',
    cuisineType: '3',
    contactPhone: '',
    contactEmail: '',
    minOrderValue: '',
    deliveryTime: '',
  })

  useEffect(() => {
    let active = true

    const loadMyRestaurant = async () => {
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
        if (active) {
          setLoading(false)
        }
      }
    }

    loadMyRestaurant()
    return () => {
      active = false
    }
  }, [showError])

  const metrics = useMemo(() => {
    if (!restaurant) return []

    return [
      { label: 'Restaurant Status', value: restaurant.status ?? 'Pending' },
      { label: 'Cuisine', value: CUISINE_OPTIONS.find((c) => c.value === restaurant.cuisineType)?.label || restaurant.cuisineType },
      { label: 'City', value: restaurant.city || 'N/A' },
    ]
  }, [restaurant])

  const updateField = (event) => {
    const { name, value } = event.target
    setForm((prev) => ({ ...prev, [name]: value }))
  }

  const handleCreateRestaurant = async (event) => {
    event.preventDefault()

    setSubmitting(true)
    try {
      const payload = {
        name: form.name,
        description: form.description || null,
        address: form.address,
        city: form.city,
        serviceZoneId: form.serviceZoneId,
        cuisineType: Number(form.cuisineType),
        contactPhone: form.contactPhone || null,
        contactEmail: form.contactEmail || null,
        minOrderValue: form.minOrderValue ? Number(form.minOrderValue) : null,
        deliveryTime: form.deliveryTime ? Number(form.deliveryTime) : null,
      }

      const created = await catalogApi.createRestaurant(payload)
      setRestaurant(created)
      showSuccess('Restaurant profile created. It is now submitted for admin approval.')
    } catch (err) {
      showError(err.response?.data?.message || err.message || 'Failed to create restaurant')
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

  if (!restaurant) {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8">
        <h1 className="mb-2 text-2xl font-bold">Complete Partner Setup</h1>
        <p className="mb-6 text-sm text-on-background/70">
          Create your restaurant profile to continue with menu management and incoming orders.
        </p>

        <form onSubmit={handleCreateRestaurant} className="space-y-4 rounded-2xl border border-outline bg-surface p-5">
          <FormField label="Restaurant Name" name="name" value={form.name} onChange={updateField} required />
          <FormField label="Description" name="description" value={form.description} onChange={updateField} />
          <FormField label="Address" name="address" value={form.address} onChange={updateField} required />
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <FormField label="City" name="city" value={form.city} onChange={updateField} required />
            <FormField label="Service Zone ID" name="serviceZoneId" value={form.serviceZoneId} onChange={updateField} required />
          </div>
          <div>
            <label htmlFor="cuisineType" className="mb-1 block text-sm font-medium text-on-background">Cuisine Type</label>
            <select
              id="cuisineType"
              name="cuisineType"
              value={form.cuisineType}
              onChange={updateField}
              className="w-full rounded-2xl border-2 border-outline bg-surface px-4 py-3 text-body-md text-on-background"
            >
              {CUISINE_OPTIONS.map((item) => (
                <option key={item.value} value={String(item.value)}>{item.label}</option>
              ))}
            </select>
          </div>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <FormField label="Contact Phone" name="contactPhone" value={form.contactPhone} onChange={updateField} />
            <FormField label="Contact Email" name="contactEmail" type="email" value={form.contactEmail} onChange={updateField} />
          </div>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <FormField label="Min Order Value" name="minOrderValue" type="number" value={form.minOrderValue} onChange={updateField} />
            <FormField label="Delivery Time (mins)" name="deliveryTime" type="number" value={form.deliveryTime} onChange={updateField} />
          </div>
          <Button type="submit" disabled={submitting}>{submitting ? 'Creating...' : 'Create Restaurant'}</Button>
        </form>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">Partner Dashboard</h1>

      <Card className="mb-5">
        <h2 className="text-lg font-semibold">Connected Restaurant</h2>
        <p className="mt-2 text-sm text-on-background/80">{restaurant.name}</p>
        <p className="text-sm text-on-background/70">Status: {restaurant.status}</p>
        {String(restaurant.status).toLowerCase() !== 'active' ? (
          <p className="mt-2 text-sm text-amber-700">
            Your restaurant is awaiting admin approval. Menu and live ordering will become fully available once approved.
          </p>
        ) : null}
        <Link to="/partner/menu" className="mt-3 inline-block text-sm font-semibold text-primary">Go to Menu Management</Link>
      </Card>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        {metrics.map((item) => (
          <Card key={item.label}>
            <p className="text-sm text-on-background/70">{item.label}</p>
            <p className="mt-2 text-2xl font-bold">{item.value}</p>
          </Card>
        ))}
      </div>
    </div>
  )
}
