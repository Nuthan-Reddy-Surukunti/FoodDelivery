import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { PartnerLayout } from '../components/organisms/PartnerLayout'
import { KpiCard } from '../components/molecules/KpiCard'
import { useNotification } from '../hooks/useNotification'
import api from '../services/api'
import catalogApi from '../services/catalogApi'
import orderApi from '../services/orderApi'
import partnerApi from '../services/partnerApi'

const CUISINE_OPTIONS = [
  { label: 'Italian', value: 1 }, { label: 'Chinese', value: 2 }, { label: 'Indian', value: 3 },
  { label: 'Mexican', value: 4 }, { label: 'American', value: 5 }, { label: 'Thai', value: 6 },
  { label: 'Japanese', value: 7 }, { label: 'Continental', value: 8 }, { label: 'Fast Food', value: 9 },
  { label: 'Vegan', value: 10 }, { label: 'Mediterranean', value: 11 }
]

const STATUS_BADGE = {
  Active: 'bg-emerald-100 text-emerald-800',
  Pending: 'bg-amber-100 text-amber-800',
  PendingApproval: 'bg-amber-100 text-amber-800',
  Rejected: 'bg-red-100 text-red-800',
  Inactive: 'bg-slate-100 text-slate-600',
}

const ORDER_BADGE = {
  CheckoutStarted: 'bg-amber-100 text-amber-800',
  Paid: 'bg-sky-100 text-sky-800',
  RestaurantAccepted: 'bg-teal-100 text-teal-800',
  Preparing: 'bg-blue-100 text-blue-800',
  ReadyForPickup: 'bg-orange-100 text-orange-800',
  OutForDelivery: 'bg-purple-100 text-purple-800',
  Delivered: 'bg-emerald-100 text-emerald-800',
  Cancelled: 'bg-red-100 text-red-800',
  RestaurantRejected: 'bg-red-100 text-red-800',
}

const emptyForm = {
  name: '', description: '', address: '', city: '',
  serviceZoneId: '', cuisineType: '3', contactPhone: '',
  contactEmail: '', minOrderValue: '', deliveryTime: '',
  imageUrl: '', rating: '',
}

// ── Restaurant Form ──────────────────────────────────────────────────────────
const RestaurantForm = ({ form, onChange, onSubmit, submitting, submitLabel, onCancel }) => (
  <form onSubmit={onSubmit} className="bg-white rounded-xl border border-slate-100 shadow-sm p-6 space-y-4">
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      {[
        { label: 'Restaurant Name', name: 'name', required: true },
        { label: 'City', name: 'city', required: true },
      ].map(({ label, name, required }) => (
        <div key={name}>
          <label className="block text-sm font-medium text-on-surface mb-1.5">{label}</label>
          <input
            name={name} value={form[name]} onChange={onChange} required={required}
            className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-on-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition"
          />
        </div>
      ))}
    </div>
    <div>
      <label className="block text-sm font-medium text-on-surface mb-1.5">Description</label>
      <textarea name="description" value={form.description} onChange={onChange} rows={2}
        className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-on-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition resize-none"
      />
    </div>
    <div>
      <label className="block text-sm font-medium text-on-surface mb-1.5">Address</label>
      <input name="address" value={form.address} onChange={onChange} required
        className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-on-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition"
      />
    </div>
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      <div>
        <label className="block text-sm font-medium text-on-surface mb-1.5">Service Zone ID</label>
        <input name="serviceZoneId" value={form.serviceZoneId} onChange={onChange} required
          className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-on-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition"
        />
      </div>
      <div>
        <label className="block text-sm font-medium text-on-surface mb-1.5">Cuisine Type</label>
        <select name="cuisineType" value={form.cuisineType} onChange={onChange}
          className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-on-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition"
        >
          {CUISINE_OPTIONS.map(o => <option key={o.value} value={String(o.value)}>{o.label}</option>)}
        </select>
      </div>
    </div>
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      <div>
        <label className="block text-sm font-medium text-on-surface mb-1.5">Contact Phone</label>
        <input name="contactPhone" value={form.contactPhone} onChange={onChange}
          className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-on-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition"
        />
      </div>
      <div>
        <label className="block text-sm font-medium text-on-surface mb-1.5">Contact Email</label>
        <input name="contactEmail" type="email" value={form.contactEmail} onChange={onChange}
          className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-on-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition"
        />
      </div>
    </div>
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      <div>
        <label className="block text-sm font-medium text-on-surface mb-1.5">Min Order Value (₹)</label>
        <input name="minOrderValue" type="number" value={form.minOrderValue} onChange={onChange}
          className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-on-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition"
        />
      </div>
      <div>
        <label className="block text-sm font-medium text-on-surface mb-1.5">Delivery Time (mins)</label>
        <input name="deliveryTime" type="number" value={form.deliveryTime} onChange={onChange}
          className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-on-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition"
        />
      </div>
    </div>
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
      <div>
        <label className="block text-sm font-medium text-on-surface mb-1.5">Rating (0-5)</label>
        <input name="rating" type="number" step="0.1" min="0" max="5" value={form.rating} onChange={onChange}
          className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-on-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition"
        />
      </div>
      <div>
        <label className="block text-sm font-medium text-on-surface mb-1.5">Restaurant Image URL</label>
        <input name="imageUrl" value={form.imageUrl} onChange={onChange} placeholder="https://images.unsplash.com/..."
          className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-on-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition"
        />
      </div>
    </div>
      {form.imageUrl && (
        <div className="mt-3 relative rounded-xl overflow-hidden border border-slate-200 h-40 bg-slate-100">
          <img src={form.imageUrl} alt="Preview" className="w-full h-full object-cover" 
               onError={(e) => { e.target.src = 'https://via.placeholder.com/400x200?text=Invalid+Image+URL'; }} />
          <div className="absolute top-2 left-2 bg-black/50 text-white text-[10px] px-2 py-1 rounded backdrop-blur-sm">Live Preview</div>
        </div>
      )}
    <div className="flex gap-3 pt-2">
      <button type="submit" disabled={submitting}
        className="bg-primary text-on-primary px-6 py-2.5 rounded-xl text-sm font-semibold hover:bg-primary-container transition-colors disabled:opacity-50"
      >
        {submitting ? 'Saving...' : submitLabel}
      </button>
      {onCancel && (
        <button type="button" onClick={onCancel}
          className="border border-slate-200 text-on-surface px-6 py-2.5 rounded-xl text-sm font-semibold hover:bg-slate-50 transition-colors"
        >
          Cancel
        </button>
      )}
    </div>
  </form>
)

// ── Main Component ────────────────────────────────────────────────────────────
export const PartnerDashboardPage = () => {
  const { showSuccess, showError } = useNotification()
  const navigate = useNavigate()
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [restaurant, setRestaurant] = useState(null)
  const [editMode, setEditMode] = useState(false)
  const [form, setForm] = useState(emptyForm)
  const [recentOrders, setRecentOrders] = useState([])
  const [ordersLoading, setOrdersLoading] = useState(false)
  const [stats, setStats] = useState(null)

  useEffect(() => {
    let active = true
    const load = async () => {
      setLoading(true)
      try {
        const response = await catalogApi.getMyRestaurant()
        if (!active) return
        setRestaurant(response)
        // Load recent orders
        if (response?.id) {
          setOrdersLoading(true)
          // Load recent orders queue for the preview list
          Promise.all([
            orderApi.getOrdersByUser
              ? api.get('/gateway/orders/queue').catch(() => ({ data: [] }))
              : Promise.resolve({ data: [] }),
            partnerApi.getDashboardStats(response.id).catch(() => null),
          ]).then(([ordersRes, statsRes]) => {
            if (!active) return
            const raw = Array.isArray(ordersRes?.data) ? ordersRes.data : (ordersRes?.data?.items || [])
            setRecentOrders(raw.slice(0, 5))
            if (statsRes) setStats(statsRes)
          }).catch(() => {}).finally(() => { if (active) setOrdersLoading(false) })
        }
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
      name: restaurant.name || '', description: restaurant.description || '',
      address: restaurant.address || '', city: restaurant.city || '',
      serviceZoneId: restaurant.serviceZoneId || '', cuisineType: String(restaurant.cuisineType ?? 3),
      contactPhone: restaurant.contactPhone || '', contactEmail: restaurant.contactEmail || '',
      minOrderValue: restaurant.minOrderValue ?? '', deliveryTime: restaurant.deliveryTime ?? '',
      imageUrl: restaurant.imageUrl || '', rating: restaurant.rating ?? '',
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
        imageUrl: form.imageUrl || null,
        rating: form.rating ? Number(form.rating) : 0,
      }
      const created = await catalogApi.createRestaurant(payload)
      setRestaurant(created)
      showSuccess('Restaurant profile created. Awaiting admin approval.')
    } catch (err) {
      showError(err.response?.data?.message || err.message || 'Failed to create restaurant')
    } finally { setSubmitting(false) }
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
        imageUrl: form.imageUrl || null,
        rating: form.rating ? Number(form.rating) : 0,
      }
      const updated = await catalogApi.updateRestaurant(restaurant.id, payload)
      setRestaurant(updated)
      setEditMode(false)
      showSuccess('Restaurant updated successfully.')
    } catch (err) {
      showError(err.response?.data?.message || err.message || 'Failed to update restaurant')
    } finally { setSubmitting(false) }
  }

  if (loading) {
    return (
      <PartnerLayout title="">
        <div className="grid grid-cols-1 sm:grid-cols-4 gap-4">
          {[1,2,3,4].map(i => <div key={i} className="h-28 animate-pulse bg-slate-200 rounded-xl" />)}
        </div>
      </PartnerLayout>
    )
  }

  if (!restaurant) {
    return (
      <PartnerLayout title="Complete Partner Setup">
        <p className="text-on-surface-variant text-sm mb-6">
          Create your restaurant profile to start receiving orders and managing your menu.
        </p>
        <RestaurantForm form={form} onChange={updateField} onSubmit={handleCreate} submitting={submitting} submitLabel="Create Restaurant" />
      </PartnerLayout>
    )
  }

  if (editMode) {
    return (
      <PartnerLayout title="Edit Restaurant">
        <RestaurantForm
          form={form} onChange={updateField} onSubmit={handleUpdate}
          submitting={submitting} submitLabel="Save Changes"
          onCancel={() => setEditMode(false)}
        />
      </PartnerLayout>
    )
  }

  const statusKey = String(restaurant.status)
  const badgeClass = STATUS_BADGE[statusKey] || 'bg-slate-100 text-slate-600'
  const isActive = statusKey.toLowerCase() === 'active'
  const cuisineLabel = CUISINE_OPTIONS.find(c => c.value === Number(restaurant.cuisineType))?.label || restaurant.cuisineType
  const today = new Date().toLocaleDateString('en-IN', { dateStyle: 'long' })

  // Use server stats if available, otherwise derive from recent orders as fallback
  const todayOrderCount = stats?.todayOrders ?? recentOrders.filter(o => {
    const d = new Date(o.createdAt || o.placedAt || 0)
    return d.toDateString() === new Date().toDateString()
  }).length
  const todayRevenue = stats?.todayRevenue ?? recentOrders.reduce((s, o) => s + Number(o.total || o.totalAmount || 0), 0)
  const pendingCount = stats?.pendingOrders ?? recentOrders.filter(o => o.orderStatus === 'Paid' || o.orderStatus === 'CheckoutStarted').length

  return (
    <PartnerLayout title="">
      {/* Header row */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-slate-100">
        <div>
          <h1 className="text-3xl font-bold text-on-surface">Overview</h1>
          <p className="text-on-surface-variant text-sm mt-1">Dashboard & restaurant management</p>
        </div>
        <div className="flex items-center gap-2 text-sm bg-surface-container px-4 py-2.5 rounded-lg border border-surface-variant">
          <span className="material-symbols-outlined text-base">calendar_today</span>
          <span className="font-medium">{today}</span>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <KpiCard
          icon="receipt_long"
          label="Today's Orders"
          value={ordersLoading ? '—' : String(todayOrderCount)}
          iconBg="bg-blue-50"
          iconColor="text-blue-600"
          prefix="📦 "
        />
        <KpiCard
          icon="payments"
          label="Today's Revenue"
          value={ordersLoading ? '—' : `₹${Number(todayRevenue).toLocaleString('en-IN')}`}
          iconBg="bg-emerald-50"
          iconColor="text-emerald-600"
          prefix="💰 "
        />
        <KpiCard
          icon="local_pizza"
          label="Top Selling Item"
          value={ordersLoading ? '—' : 'Pizza Margherita'}
          iconBg="bg-purple-50"
          iconColor="text-purple-600"
          prefix="⭐ "
        />
        <KpiCard
          icon="timer"
          label="Avg. Prep Time"
          value={ordersLoading ? '—' : '15 mins'}
          iconBg="bg-orange-50"
          iconColor="text-orange-600"
          prefix="⏱️ "
        />
      </div>

      {/* Restaurant info card */}
      <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden">
        {restaurant.imageUrl && (
          <div className="h-64 w-full overflow-hidden">
            <img src={restaurant.imageUrl} alt={restaurant.name} className="w-full h-full object-cover" />
          </div>
        )}
        <div className="p-5 border-b border-slate-100 flex justify-between items-center">
          <div className="flex items-center gap-3">
            <h3 className="text-lg font-semibold text-on-surface">{restaurant.name}</h3>
            <span className={`${badgeClass} text-xs font-semibold px-2.5 py-1 rounded-full`}>{statusKey}</span>
          </div>
          <button onClick={openEdit} className="text-sm font-medium text-primary flex items-center gap-1 hover:text-primary-container transition-colors">
            <span className="material-symbols-outlined text-sm">edit</span> Edit
          </button>
        </div>
        <div className="p-5 grid grid-cols-2 md:grid-cols-4 gap-4">
          {[
            { label: 'Address', value: `${restaurant.address}, ${restaurant.city}` },
            { label: 'Rating', value: `${Number(restaurant.rating ?? 0).toFixed(1)} ★` },
            { label: 'Min Order', value: restaurant.minOrderValue ? `₹${restaurant.minOrderValue}` : 'N/A' },
            { label: 'Phone', value: restaurant.contactPhone || 'N/A' },
          ].map(({ label, value }) => (
            <div key={label} className="bg-slate-50 rounded-xl p-3">
              <p className="text-xs text-on-surface-variant mb-1">{label}</p>
              <p className="text-sm font-semibold text-on-surface">{value}</p>
            </div>
          ))}
        </div>
        {!isActive && (
          <div className="mx-5 mb-5 bg-amber-50 border border-amber-200 rounded-xl p-4 flex items-start gap-2">
            <span className="material-symbols-outlined text-amber-600 text-xl mt-0.5">warning</span>
            <p className="text-sm text-amber-800">
              Your restaurant is <strong>{statusKey}</strong> — awaiting admin approval. Menu editing and live orders will be enabled once approved.
            </p>
          </div>
        )}
      </div>

      {/* Active orders list */}
      <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden">
        <div className="p-5 border-b border-slate-100 flex justify-between items-center">
          <h3 className="text-lg font-semibold text-on-surface flex items-center gap-2">
            <span className="material-symbols-outlined text-primary">list_alt</span>
            Active Orders
          </h3>
          <Link to="/partner/queue" className="text-sm font-medium text-primary flex items-center gap-1 hover:text-primary-container transition-colors">
            View All <span className="material-symbols-outlined text-base">chevron_right</span>
          </Link>
        </div>

        {ordersLoading ? (
          <div className="p-5 space-y-3">
            {[1,2,3].map(i => <div key={i} className="h-14 bg-slate-100 animate-pulse rounded-xl" />)}
          </div>
        ) : recentOrders.length === 0 ? (
          <div className="py-12 text-center text-on-surface-variant text-sm">No active orders</div>
        ) : (
          <div className="divide-y divide-slate-50">
            {recentOrders.map(order => {
              const status = order.orderStatus || order.status || ''
              const badgeCls = ORDER_BADGE[status] || 'bg-slate-100 text-slate-700'
              const isNew = status === 'Paid' || status === 'CheckoutStarted'
              return (
                <div key={order.orderId || order.id} className="p-5 hover:bg-slate-50 transition-colors flex flex-col lg:flex-row lg:items-center justify-between gap-4">
                  <div className="flex items-start gap-4 flex-1">
                    <div className="bg-slate-100 h-11 w-11 rounded-lg flex items-center justify-center shrink-0 border border-slate-200 text-xs font-bold text-slate-500">
                      #{String(order.orderId || order.id || '').split('-')[0].slice(0, 3).toUpperCase()}
                    </div>
                    <div>
                      <h4 className="font-semibold text-on-surface text-sm">{order.customerName || order.customerEmail || 'Customer'}</h4>
                      <p className="text-xs text-on-surface-variant mt-0.5">
                        {order.items?.map(i => `${i.quantity}× ${i.menuItemName || 'Item'}`).join(', ') || 'Order items'}
                      </p>
                      <div className="flex items-center gap-2 mt-1.5">
                        <span className={`${badgeCls} text-xs font-semibold px-2 py-0.5 rounded-full flex items-center gap-1`}>
                          {isNew && <span className="material-symbols-outlined text-xs">new_releases</span>}
                          {status}
                        </span>
                        <span className="text-xs text-slate-400 flex items-center gap-0.5">
                          <span className="material-symbols-outlined text-xs">schedule</span>
                          {order.createdAt ? new Date(order.createdAt).toLocaleTimeString('en-IN', { hour: '2-digit', minute: '2-digit' }) : ''}
                        </span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-bold text-on-surface">₹{Number(order.total || order.totalAmount || 0).toFixed(2)}</span>
                    {isNew && (
                      <Link to="/partner/queue" className="bg-primary text-on-primary text-xs font-semibold px-4 py-2 rounded-lg hover:bg-primary-container transition-colors">
                        Accept Order
                      </Link>
                    )}
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>

      {/* Quick links */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        {[
          { to: '/partner/menu', icon: 'restaurant_menu', label: 'Menu Management', desc: 'Add, edit & remove menu items and categories', color: 'bg-blue-50 text-blue-600' },
          { to: '/partner/queue', icon: 'receipt_long', label: 'Order Queue', desc: 'View and manage incoming customer orders', color: 'bg-purple-50 text-purple-600' },
        ].map(({ to, icon, label, desc, color }) => (
          <Link key={to} to={to} className="bg-white border border-slate-100 rounded-xl p-5 hover:border-primary/30 hover:shadow-md transition-all flex items-center gap-4 group">
            <div className={`w-12 h-12 rounded-xl ${color} flex items-center justify-center flex-shrink-0`}>
              <span className="material-symbols-outlined text-xl">{icon}</span>
            </div>
            <div className="flex-1 min-w-0">
              <p className="font-semibold text-on-surface group-hover:text-primary transition-colors">{label}</p>
              <p className="text-xs text-on-surface-variant mt-0.5">{desc}</p>
            </div>
            <span className="material-symbols-outlined text-slate-300 ml-auto">arrow_forward</span>
          </Link>
        ))}
      </div>
    </PartnerLayout>
  )
}
