import { useEffect, useState } from 'react'
import { AdminLayout } from '../components/organisms/AdminLayout'
import { UserTable } from '../components/molecules/UserTable'
import { useNotification } from '../hooks/useNotification'
import adminApi from '../services/adminApi'
import api from '../services/api'

const ROLE_CONFIG = {
  Admin: { icon: 'shield', color: 'bg-red-50 text-red-600' },
  RestaurantPartner: { icon: 'storefront', color: 'bg-purple-50 text-purple-600' },
  DeliveryAgent: { icon: 'two_wheeler', color: 'bg-indigo-50 text-indigo-600' },
  Customer: { icon: 'person', color: 'bg-blue-50 text-blue-600' },
}

const emptyAdminForm = { fullName: '', email: '', password: '', confirmPassword: '' }

// ── Create Admin Modal ──────────────────────────────────────────────────────
const CreateAdminModal = ({ onClose, onCreated }) => {
  const { showSuccess, showError } = useNotification()
  const [form, setForm] = useState(emptyAdminForm)
  const [submitting, setSubmitting] = useState(false)
  const [fieldError, setFieldError] = useState('')

  const handleChange = (e) => {
    setForm(f => ({ ...f, [e.target.name]: e.target.value }))
    setFieldError('')
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (form.password !== form.confirmPassword) {
      setFieldError('Passwords do not match')
      return
    }
    if (form.password.length < 8) {
      setFieldError('Password must be at least 8 characters')
      return
    }
    setSubmitting(true)
    try {
      await api.post('/gateway/auth/admin/create', {
        fullName: form.fullName,
        email: form.email,
        password: form.password,
      })
      showSuccess(`Admin account created for ${form.email}`)
      onCreated()
      onClose()
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to create admin')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm px-4">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md p-8 relative">
        <button
          onClick={onClose}
          className="absolute top-4 right-4 text-slate-400 hover:text-slate-600 transition-colors"
        >
          <span className="material-symbols-outlined">close</span>
        </button>

        <div className="flex items-center gap-3 mb-6">
          <div className="w-11 h-11 rounded-xl bg-red-50 text-red-600 flex items-center justify-center">
            <span className="material-symbols-outlined">admin_panel_settings</span>
          </div>
          <div>
            <h2 className="text-xl font-bold text-on-surface">Create Admin</h2>
            <p className="text-xs text-on-surface-variant">Grant full portal access</p>
          </div>
        </div>

        {fieldError && (
          <div className="bg-error-container text-on-error-container text-sm px-4 py-3 rounded-xl mb-4">
            {fieldError}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          {[
            { label: 'Full Name', name: 'fullName', type: 'text', placeholder: 'Jane Doe' },
            { label: 'Email Address', name: 'email', type: 'email', placeholder: 'jane@example.com' },
            { label: 'Password', name: 'password', type: 'password', placeholder: '••••••••' },
            { label: 'Confirm Password', name: 'confirmPassword', type: 'password', placeholder: '••••••••' },
          ].map(({ label, name, type, placeholder }) => (
            <div key={name}>
              <label className="block text-sm font-medium text-on-surface mb-1.5">{label}</label>
              <input
                type={type}
                name={name}
                value={form[name]}
                onChange={handleChange}
                placeholder={placeholder}
                required
                className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-on-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition"
              />
            </div>
          ))}

          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 bg-slate-100 hover:bg-slate-200 text-on-surface text-sm font-semibold py-2.5 rounded-xl transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={submitting}
              className="flex-1 bg-primary text-on-primary text-sm font-semibold py-2.5 rounded-xl hover:bg-rose-500 transition-colors disabled:opacity-60"
            >
              {submitting ? 'Creating...' : 'Create Admin'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

// ── User Details Modal ──────────────────────────────────────────────────────
const UserDetailsModal = ({ user, onClose }) => {
  const [activeTab, setActiveTab] = useState('orders')
  const [orders, setOrders] = useState(null)
  const [addresses, setAddresses] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const fetchDetails = async () => {
      setLoading(true)
      try {
        const [ordersRes, addressesRes] = await Promise.all([
          api.get(`/gateway/orders?userId=${user.id}`),
          adminApi.getUserAddresses(user.id).catch(() => [])
        ])
        setOrders(ordersRes.data || ordersRes)
        setAddresses(addressesRes)
      } catch (err) {
        console.error('Failed to fetch user details', err)
      } finally {
        setLoading(false)
      }
    }
    fetchDetails()
  }, [user.id])

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-4xl max-h-[90vh] flex flex-col relative overflow-hidden">
        <div className="p-6 border-b border-surface-variant flex justify-between items-start bg-surface-container-lowest">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center text-xl font-bold">
              {(user.name || user.email || 'U')[0].toUpperCase()}
            </div>
            <div>
              <h2 className="text-xl font-bold text-on-surface">{user.name || 'Unknown User'}</h2>
              <div className="flex gap-3 text-sm text-on-surface-variant mt-1">
                <span className="flex items-center gap-1"><span className="material-symbols-outlined text-[16px]">mail</span>{user.email}</span>
                {user.phone && <span className="flex items-center gap-1"><span className="material-symbols-outlined text-[16px]">call</span>{user.phone}</span>}
              </div>
            </div>
          </div>
          <button onClick={onClose} className="p-2 hover:bg-surface-container rounded-full transition-colors text-on-surface-variant">
            <span className="material-symbols-outlined">close</span>
          </button>
        </div>

        <div className="flex border-b border-surface-variant bg-surface-container-lowest px-6 gap-6">
          <button
            onClick={() => setActiveTab('orders')}
            className={`py-3 font-medium text-sm transition-colors border-b-2 ${
              activeTab === 'orders' ? 'border-primary text-primary' : 'border-transparent text-on-surface-variant hover:text-on-surface'
            }`}
          >
            Order History
          </button>
          <button
            onClick={() => setActiveTab('addresses')}
            className={`py-3 font-medium text-sm transition-colors border-b-2 ${
              activeTab === 'addresses' ? 'border-primary text-primary' : 'border-transparent text-on-surface-variant hover:text-on-surface'
            }`}
          >
            Saved Addresses
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-6 bg-surface-container-lowest">
          {loading ? (
            <div className="flex items-center justify-center py-12">
              <div className="w-8 h-8 border-4 border-primary/20 border-t-primary rounded-full animate-spin" />
            </div>
          ) : (
            <>
              {activeTab === 'orders' && (
                <div className="space-y-4">
                  {orders?.length === 0 ? (
                    <p className="text-on-surface-variant text-center py-8">No orders found for this user.</p>
                  ) : (
                    orders?.map(order => (
                      <div key={order.orderId} className="p-4 border border-surface-variant rounded-xl flex justify-between items-center hover:bg-surface-container-low transition-colors">
                        <div>
                          <p className="font-semibold text-on-surface">Order #{order.orderId.substring(0,8)}</p>
                          <p className="text-sm text-on-surface-variant">{new Date(order.createdAt).toLocaleDateString()}</p>
                        </div>
                        <div className="text-right">
                          <p className="font-bold text-on-surface">₹{order.totalAmount}</p>
                          <span className="text-xs px-2 py-1 bg-surface-variant text-on-surface rounded-full">{order.status}</span>
                        </div>
                      </div>
                    ))
                  )}
                </div>
              )}

              {activeTab === 'addresses' && (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {addresses?.length === 0 ? (
                    <p className="text-on-surface-variant text-center py-8 col-span-full">No addresses found.</p>
                  ) : (
                    addresses?.map(addr => (
                      <div key={addr.addressId} className="p-4 border border-surface-variant rounded-xl bg-surface-container-lowest shadow-sm">
                        <div className="flex justify-between items-start mb-2">
                          <span className="text-xs font-semibold px-2 py-1 bg-blue-50 text-blue-600 rounded-full">{addr.addressType}</span>
                          {addr.isDefault && <span className="text-xs font-semibold px-2 py-1 bg-emerald-50 text-emerald-600 rounded-full">Default</span>}
                        </div>
                        <p className="font-medium text-on-surface">{addr.street}</p>
                        <p className="text-sm text-on-surface-variant">{addr.city}, {addr.state} {addr.zipCode}</p>
                      </div>
                    ))
                  )}
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  )
}

export const AdminUsersPage = () => {
  const { showSuccess, showError } = useNotification()
  const [users, setUsers] = useState(null)
  const [userList, setUserList] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [showCreateAdmin, setShowCreateAdmin] = useState(false)
  const [selectedUser, setSelectedUser] = useState(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const pageSize = 10

  const loadData = async () => {
    let active = true
    setLoading(true)
    setError('')
    try {
      const [usersRes, listRes] = await Promise.all([
        adminApi.getUserAnalytics(),
        adminApi.getUsersList().catch(() => []),
      ])
      if (!active) return
      setUsers(usersRes)
      setUserList(listRes)
    } catch (err) {
      if (!active) return
      setError(err.response?.data?.message || err.message || 'Failed to load data')
    } finally {
      if (active) setLoading(false)
    }
    return () => { active = false }
  }

  useEffect(() => { loadData() }, [])

  const filteredUsers = userList.filter(u => {
    const term = searchTerm.toLowerCase()
    return (u.fullName?.toLowerCase() || '').includes(term) || 
           (u.email?.toLowerCase() || '').includes(term) || 
           (u.role?.toLowerCase() || '').includes(term) || 
           (u.phone?.toLowerCase() || '').includes(term)
  })

  useEffect(() => {
    setCurrentPage(1)
  }, [searchTerm])

  useEffect(() => {
    const totalPages = Math.max(1, Math.ceil(filteredUsers.length / pageSize))
    if (currentPage > totalPages) {
      setCurrentPage(totalPages)
    }
  }, [currentPage, filteredUsers.length])

  const paginatedUsers = filteredUsers.slice(
    (currentPage - 1) * pageSize,
    currentPage * pageSize
  )

  const handleToggleStatus = async (user) => {
    const action = user.status === 'Active' ? 'suspend' : 'activate'
    if (!window.confirm(`Are you sure you want to ${action} user ${user.name}?`)) return
    
    try {
      await adminApi.toggleUserStatus(user.id)
      showSuccess(`User ${user.name} has been ${action}d.`)
      setUserList(prev => prev.map(u => 
        u.id === user.id ? { ...u, isActive: !u.isActive } : u
      ))
    } catch (err) {
      showError(err.response?.data?.message || `Failed to ${action} user`)
    }
  }

  const handleDeleteUser = async (user) => {
    if (!window.confirm(`WARNING: Are you sure you want to PERMANENTLY delete user ${user.name}? This cannot be undone.`)) return
    
    try {
      await adminApi.deleteUser(user.id)
      showSuccess(`User ${user.name} has been deleted.`)
      setUserList(prev => prev.filter(u => u.id !== user.id))
    } catch (err) {
      showError(err.response?.data?.message || 'Failed to delete user')
    }
  }

  const usersByRole = users?.usersByRole
    ? Object.entries(users.usersByRole).map(([role, count]) => ({ role, count }))
    : []

  return (
    <AdminLayout>
      {showCreateAdmin && (
        <CreateAdminModal
          onClose={() => setShowCreateAdmin(false)}
          onCreated={loadData}
        />
      )}

      {selectedUser && (
        <UserDetailsModal
          user={selectedUser}
          onClose={() => setSelectedUser(null)}
        />
      )}

      {/* Premium Integrated Header */}
      <div className="flex items-end justify-between mb-10 pb-6 border-b border-slate-100">
        <div>
          <div className="flex items-center gap-3 mb-2 text-primary font-bold text-xs uppercase tracking-[0.2em]">
            <span className="material-symbols-outlined text-sm">admin_panel_settings</span>
            Administrative Portal
          </div>
          <h1 className="text-4xl font-extrabold text-slate-900 tracking-tight mb-3">User Management</h1>
          
          {users && (
            <div className="flex items-center gap-6">
              <div className="flex items-center gap-2.5">
                <div className="w-2.5 h-2.5 rounded-full bg-primary shadow-[0_0_8px_rgba(251,113,133,0.6)]" />
                <span className="text-sm font-semibold text-slate-600">
                  <span className="text-slate-900 font-bold">{users.totalUsersRegistered ?? users.totalUsers ?? '0'}</span> Total Registered
                </span>
              </div>
              <div className="w-px h-4 bg-slate-200" />
              <div className="flex items-center gap-2.5">
                <div className="w-2.5 h-2.5 rounded-full bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.6)]" />
                <span className="text-sm font-semibold text-slate-600">
                  <span className="text-slate-900 font-bold">{users.activeUsers ?? '0'}</span> Active Now
                </span>
              </div>
            </div>
          )}
        </div>

        <div className="flex items-center gap-3">
          <button className="flex items-center gap-2 px-5 py-2.5 bg-white border border-slate-200 rounded-xl text-sm font-bold text-slate-700 hover:bg-slate-50 hover:border-slate-300 transition-all active:scale-95">
            <span className="material-symbols-outlined text-[20px]">filter_list</span>
            Filter
          </button>
          <button
            onClick={() => setShowCreateAdmin(true)}
            className="flex items-center gap-2 px-5 py-2.5 bg-primary text-white rounded-xl text-sm font-bold hover:bg-rose-600 shadow-lg shadow-primary/20 transition-all active:scale-95"
          >
            <span className="material-symbols-outlined text-[20px]">add</span>
            Add New User
          </button>
        </div>
      </div>

      {error && <div className="bg-error-container text-on-error-container px-4 py-3 rounded-xl text-sm mb-6 animate-fade-in">{error}</div>}


      {/* Main Table Container (Full Width) */}
      <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden flex flex-col">
            {/* Box Header with Search and Columns */}
            <div className="p-4 border-b border-slate-100 flex items-center justify-between bg-white">
              <div className="relative w-full max-w-md">
                <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-[20px]">search</span>
                <input
                  type="text"
                  placeholder="Search by name, email, or phone..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-10 pr-4 py-2.5 bg-[#f8f9fa] border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all placeholder:text-slate-500"
                />
              </div>
            </div>

            {/* Table Area */}
            {loading ? (
              <div className="p-8 flex items-center justify-center min-h-[400px]">
                <div className="text-center">
                <div className="w-10 h-10 mx-auto mb-4 border-4 border-primary/20 border-t-primary rounded-full animate-spin" />
                  <p className="text-slate-500">Loading users...</p>
                </div>
              </div>
            ) : (
              <UserTable
                users={paginatedUsers.map(u => ({
                  id: u.id,
                  name: u.fullName,
                  email: u.email,
                  phone: u.phone,
                  joinedDate: u.createdAt,
                  status: u.isActive ? 'Active' : 'Suspended',
                }))}
                loading={loading}
                totalItems={filteredUsers.length}
                currentPage={currentPage}
                pageSize={pageSize}
                onPageChange={setCurrentPage}
                onView={(user) => setSelectedUser(user)}
                onToggleStatus={handleToggleStatus}
                onDelete={handleDeleteUser}
              />
            )}
      </div>
    </AdminLayout>
  )
}
