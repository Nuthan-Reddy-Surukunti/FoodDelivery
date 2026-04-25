import { useEffect, useMemo, useState } from 'react'
import { Card } from '../components/atoms/Card'
import adminApi from '../services/adminApi'

export const AdminUsersPage = () => {
  const [analytics, setAnalytics] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true

    const loadUsersAnalytics = async () => {
      setLoading(true)
      setError('')
      try {
        const response = await adminApi.getUserAnalytics()
        if (!active) return
        setAnalytics(response)
      } catch (err) {
        if (!active) return
        setError(err.response?.data?.message || err.message || 'Failed to load user analytics')
      } finally {
        if (active) {
          setLoading(false)
        }
      }
    }

    loadUsersAnalytics()
    return () => {
      active = false
    }
  }, [])

  const usersByRole = useMemo(() => {
    if (!analytics?.usersByRole) return []
    return Object.entries(analytics.usersByRole).map(([role, count]) => ({ role, count }))
  }, [analytics])

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">User Management</h1>
      {loading ? <p className="text-sm text-on-background/70">Loading user analytics...</p> : null}
      {error ? <p className="text-sm text-error">{error}</p> : null}

      <div className="mb-5 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <Card>
          <p className="text-sm text-on-background/70">Total Registered Users</p>
          <p className="mt-2 text-2xl font-bold">{analytics?.totalUsersRegistered ?? 0}</p>
        </Card>
        <Card>
          <p className="text-sm text-on-background/70">Active Users</p>
          <p className="mt-2 text-2xl font-bold">{analytics?.activeUsers ?? 0}</p>
        </Card>
      </div>

      <div className="space-y-3">
        {usersByRole.map((entry) => (
          <Card key={entry.role} className="flex items-center justify-between">
            <div>
              <p className="font-semibold">{entry.role}</p>
              <p className="text-sm text-on-background/70">Users by role from backend analytics</p>
            </div>
            <p className="text-lg font-bold">{entry.count}</p>
          </Card>
        ))}
      </div>
    </div>
  )
}
