import { useEffect, useState } from 'react'
import { AdminLayout } from '../components/organisms/AdminLayout'
import adminApi from '../services/adminApi'

const ROLE_CONFIG = {
  Admin: { icon: 'shield', color: 'bg-red-50 text-red-600' },
  RestaurantPartner: { icon: 'storefront', color: 'bg-purple-50 text-purple-600' },
  DeliveryAgent: { icon: 'two_wheeler', color: 'bg-indigo-50 text-indigo-600' },
  Customer: { icon: 'person', color: 'bg-blue-50 text-blue-600' },
}

export const AdminUsersPage = () => {
  const [users, setUsers] = useState(null)
  const [partners, setPartners] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    const load = async () => {
      setLoading(true)
      setError('')
      try {
        const [usersRes, partnersRes] = await Promise.all([
          adminApi.getUserAnalytics(),
          adminApi.getPartnersReport().catch(() => null),
        ])
        if (!active) return
        setUsers(usersRes)
        setPartners(partnersRes)
      } catch (err) {
        if (!active) return
        setError(err.response?.data?.message || err.message || 'Failed to load analytics')
      } finally {
        if (active) setLoading(false)
      }
    }
    load()
    return () => { active = false }
  }, [])

  const usersByRole = users?.usersByRole
    ? Object.entries(users.usersByRole).map(([role, count]) => ({ role, count }))
    : []

  return (
    <AdminLayout title="Users & Analytics" searchPlaceholder="Search users...">
      {error && <div className="bg-error-container text-on-error-container px-4 py-3 rounded-xl text-sm">{error}</div>}

      {loading && (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          {[1,2,3].map(i => <div key={i} className="h-28 bg-slate-200 animate-pulse rounded-xl" />)}
        </div>
      )}

      {users && (
        <>
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

          {/* Users by role */}
          {usersByRole.length > 0 && (
            <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden">
              <div className="p-5 border-b border-slate-100">
                <h3 className="text-lg font-semibold text-on-surface flex items-center gap-2">
                  <span className="material-symbols-outlined text-primary">people</span>
                  Users by Role
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
        </>
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
