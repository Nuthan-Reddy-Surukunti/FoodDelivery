import { useEffect, useState } from 'react'
import { Card } from '../components/atoms/Card'
import adminApi from '../services/adminApi'

const StatCard = ({ label, value, sub }) => (
  <Card className="p-4">
    <p className="text-xs text-on-background/60">{label}</p>
    <p className="mt-2 text-2xl font-bold">{value ?? '—'}</p>
    {sub && <p className="mt-1 text-xs text-on-background/50">{sub}</p>}
  </Card>
)

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

  const usersByRole = (() => {
    if (!users?.usersByRole) return []
    return Object.entries(users.usersByRole).map(([role, count]) => ({ role, count }))
  })()

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-bold">Users & Analytics</h1>

      {loading && <p className="text-sm text-on-background/70">Loading analytics...</p>}
      {error && <p className="text-sm text-error">{error}</p>}

      {users && (
        <>
          {/* User summary cards */}
          <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <StatCard
              label="Total Registered Users"
              value={users.totalUsersRegistered ?? users.totalUsers ?? '—'}
            />
            <StatCard
              label="Active Users"
              value={users.activeUsers ?? '—'}
            />
            <StatCard
              label="New This Month"
              value={users.newUsersThisMonth ?? '—'}
            />
          </div>

          {/* Users by role */}
          {usersByRole.length > 0 && (
            <Card className="mb-6 p-5">
              <h2 className="mb-4 text-lg font-semibold">Users by Role</h2>
              <div className="space-y-3">
                {usersByRole.map(entry => (
                  <div key={entry.role} className="flex items-center justify-between rounded-xl border border-outline px-4 py-3">
                    <div className="flex items-center gap-3">
                      <span className="text-lg">
                        {entry.role === 'Admin' ? '🛡️' : entry.role === 'RestaurantPartner' ? '🏪' : entry.role === 'DeliveryAgent' ? '🛵' : '👤'}
                      </span>
                      <p className="font-medium">{entry.role}</p>
                    </div>
                    <p className="text-xl font-bold">{entry.count}</p>
                  </div>
                ))}
              </div>
            </Card>
          )}
        </>
      )}

      {/* Partners report */}
      {partners && (
        <Card className="p-5">
          <h2 className="mb-4 text-lg font-semibold">Partner Analytics</h2>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
            <div>
              <p className="text-xs text-on-background/60">Total Partners</p>
              <p className="text-xl font-bold mt-1">{partners.totalPartners ?? partners.total ?? '—'}</p>
            </div>
            <div>
              <p className="text-xs text-on-background/60">Active Partners</p>
              <p className="text-xl font-bold mt-1">{partners.activePartners ?? partners.active ?? '—'}</p>
            </div>
            <div>
              <p className="text-xs text-on-background/60">Pending Approval</p>
              <p className="text-xl font-bold mt-1">{partners.pendingPartners ?? partners.pending ?? '—'}</p>
            </div>
          </div>
        </Card>
      )}
    </div>
  )
}
