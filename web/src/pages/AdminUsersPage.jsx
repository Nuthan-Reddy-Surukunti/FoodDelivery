import { useEffect, useState } from 'react'
import { AdminLayout } from '../components/organisms/AdminLayout'
import { UserTable } from '../components/molecules/UserTable'
import { useNotification } from '../hooks/useNotification'
import adminApi from '../services/adminApi'
import api from '../services/api'
import authApi from '../services/authApi'

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

export const AdminUsersPage = () => {
  const { showSuccess, showError } = useNotification()
  const [users, setUsers] = useState(null)
  const [userList, setUserList] = useState([])
  const [partners, setPartners] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [showCreateAdmin, setShowCreateAdmin] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')

  const loadData = async () => {
    let active = true
    setLoading(true)
    setError('')
    try {
      const [usersRes, partnersRes, listRes] = await Promise.all([
        adminApi.getUserAnalytics(),
        adminApi.getPartnersReport().catch(() => null),
        adminApi.getUsersList().catch(() => []),
      ])
      if (!active) return
      setUsers(usersRes)
      setPartners(partnersRes)
      setUserList(listRes)
    } catch (err) {
      if (!active) return
      setError(err.response?.data?.message || err.message || 'Failed to load analytics')
    } finally {
      if (active) setLoading(false)
    }
    return () => { active = false }
  }

  useEffect(() => { loadData() }, [])

  const filteredUsers = userList.filter(u => 
    u.fullName.toLowerCase().includes(searchTerm.toLowerCase()) || 
    u.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
    u.role.toLowerCase().includes(searchTerm.toLowerCase())
  )

  const usersByRole = users?.usersByRole
    ? Object.entries(users.usersByRole).map(([role, count]) => ({ role, count }))
    : []

  return (
    <AdminLayout 
      title="Users & Analytics" 
      searchPlaceholder="Search users by name, email or role..."
      onSearch={setSearchTerm}
    >
      {showCreateAdmin && (
        <CreateAdminModal
          onClose={() => setShowCreateAdmin(false)}
          onCreated={loadData}
        />
      )}

      {/* Header row with action */}
      <div className="flex items-center justify-between mb-2">
        <p className="text-sm text-on-surface-variant">Manage portal access and view user metrics</p>
        <button
          id="create-admin-btn"
          onClick={() => setShowCreateAdmin(true)}
          className="bg-primary text-on-primary text-sm font-semibold px-4 py-2 rounded-xl flex items-center gap-2 hover:bg-rose-500 transition-colors"
        >
          <span className="material-symbols-outlined text-sm">admin_panel_settings</span>
          Create Admin
        </button>
      </div>

      {error && <div className="bg-error-container text-on-error-container px-4 py-3 rounded-xl text-sm mb-6">{error}</div>}

      {loading && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            {[1,2,3].map(i => <div key={i} className="h-28 bg-slate-200 animate-pulse rounded-xl" />)}
          </div>
          <div className="h-96 bg-slate-200 animate-pulse rounded-xl" />
        </div>
      )}

      {users && (
        <div className="space-y-8">
          {/* Summary stat cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {[
              { label: 'Total Registered Users', value: users.totalUsersRegistered ?? users.totalUsers, icon: 'group', color: 'bg-blue-50 text-blue-600' },
              { label: 'Active Users', value: users.activeUsers, icon: 'verified_user', color: 'bg-emerald-50 text-emerald-600' },
              { label: 'New This Month', value: users.newUsersThisMonth, icon: 'person_add', color: 'bg-violet-50 text-violet-600' },
            ].map(({ label, value, icon, color }) => (
              <div key={label} className="bg-white rounded-xl p-6 border border-slate-100 shadow-sm flex items-start gap-4">
                <div className={`w-12 h-12 rounded-xl ${color} flex items-center justify-center flex-shrink-0`}>
                  <span className="material-symbols-outlined text-xl">{icon}</span>
                </div>
                <div>
                  <p className="text-sm text-on-surface-variant mb-1">{label}</p>
                  <p className="text-3xl font-bold text-on-surface">{value ?? '—'}</p>
                </div>
              </div>
            ))}
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Users by role */}
            <div className="lg:col-span-1">
              {usersByRole.length > 0 && (
                <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden h-full">
                  <div className="p-5 border-b border-slate-100">
                    <h3 className="text-lg font-semibold text-on-surface flex items-center gap-2">
                      <span className="material-symbols-outlined text-primary">analytics</span>
                      Distribution
                    </h3>
                  </div>
                  <div className="divide-y divide-slate-50">
                    {usersByRole.map(({ role, count }) => {
                      const cfg = ROLE_CONFIG[role] || { icon: 'person', color: 'bg-slate-50 text-slate-600' }
                      const total = usersByRole.reduce((s, r) => s + Number(r.count || 0), 0)
                      const pct = total > 0 ? Math.round((Number(count) / total) * 100) : 0
                      return (
                        <div key={role} className="p-5 flex items-center gap-4 hover:bg-slate-50 transition-colors">
                          <div className={`w-10 h-10 rounded-xl ${cfg.color} flex items-center justify-center flex-shrink-0`}>
                            <span className="material-symbols-outlined text-lg">{cfg.icon}</span>
                          </div>
                          <div className="flex-1">
                            <div className="flex justify-between items-center mb-1.5">
                              <span className="text-sm font-semibold text-on-surface">{role}</span>
                              <span className="text-sm font-bold text-on-surface">{count}</span>
                            </div>
                            <div className="w-full bg-slate-100 rounded-full h-1.5">
                              <div className="bg-primary h-1.5 rounded-full transition-all duration-500" style={{ width: `${pct}%` }} />
                            </div>
                            <p className="text-xs text-on-surface-variant mt-1">{pct}% of total users</p>
                          </div>
                        </div>
                      )
                    })}
                  </div>
                </div>
              )}
            </div>

    {/* Detailed User Table */}
            <div className="lg:col-span-2">
              <div>
                <h3 className="text-lg font-semibold text-on-surface mb-4 flex items-center gap-2">
                  <span className="material-symbols-outlined text-primary">list_alt</span>
                  Registered Users
                </h3>
              </div>
              <UserTable
                users={filteredUsers.map(u => ({
                  id: u.id,
                  name: u.fullName,
                  email: u.email,
                  phone: u.phone,
                  joinedDate: u.createdAt,
                  status: u.isActive ? 'Active' : 'Suspended',
                }))}
                loading={loading}
                totalItems={filteredUsers.length}
                currentPage={1}
                pageSize={10}
                onPageChange={() => {}}
              />
            </div>
          </div>
        </div>
      )}

      {/* Partners analytics */}
      {partners && (
        <div className="bg-white rounded-xl border border-slate-100 shadow-sm p-6">
          <h3 className="text-lg font-semibold text-on-surface mb-5 flex items-center gap-2">
            <span className="material-symbols-outlined text-primary">storefront</span>
            Partner Analytics
          </h3>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
            {[
              { label: 'Total Partners', value: partners.totalPartners ?? partners.total },
              { label: 'Active Partners', value: partners.activePartners ?? partners.active },
              { label: 'Pending Approval', value: partners.pendingPartners ?? partners.pending },
            ].map(({ label, value }) => (
              <div key={label} className="border border-slate-100 rounded-xl p-4">
                <p className="text-xs font-medium text-on-surface-variant mb-2">{label}</p>
                <p className="text-3xl font-bold text-on-surface">{value ?? '—'}</p>
              </div>
            ))}
          </div>
        </div>
      )}
    </AdminLayout>
  )
}
