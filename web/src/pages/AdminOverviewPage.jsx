import { useEffect, useMemo, useState } from 'react'
import { Card } from '../components/atoms/Card'
import adminApi from '../services/adminApi'

export const AdminOverviewPage = () => {
  const [kpis, setKpis] = useState(null)
  const [salesReport, setSalesReport] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true

    const loadDashboard = async () => {
      setLoading(true)
      setError('')
      try {
        const [kpiResponse, salesResponse] = await Promise.all([
          adminApi.getDashboardKpis(),
          adminApi.getSalesReport(),
        ])

        if (!active) return
        setKpis(kpiResponse)
        setSalesReport(salesResponse)
      } catch (err) {
        if (!active) return
        setError(err.response?.data?.message || err.message || 'Failed to load dashboard metrics')
      } finally {
        if (active) {
          setLoading(false)
        }
      }
    }

    loadDashboard()
    return () => {
      active = false
    }
  }, [])

  const cards = useMemo(() => {
    if (!kpis) return []
    return [
      { label: 'Active Partners', value: kpis.activePartners ?? 0 },
      { label: 'Orders Today', value: kpis.ordersToday ?? 0 },
      { label: 'Pending Approvals', value: kpis.pendingApprovals ?? 0 },
      { label: 'Total Orders', value: kpis.totalOrders ?? 0 },
    ]
  }, [kpis])

  const totalRevenue = salesReport?.totalRevenue ?? kpis?.totalRevenue ?? 0
  const avgOrderValue = salesReport?.averageOrderValue ?? 0

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">Admin Overview</h1>
      {loading ? <p className="text-sm text-on-background/70">Loading dashboard...</p> : null}
      {error ? <p className="text-sm text-error">{error}</p> : null}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {cards.map((card) => (
          <Card key={card.label}>
            <p className="text-sm text-on-background/70">{card.label}</p>
            <p className="mt-2 text-2xl font-bold">{card.value}</p>
          </Card>
        ))}
      </div>
      <Card className="mt-5">
        <h2 className="text-lg font-semibold">Revenue Snapshot</h2>
        <p className="mt-2 text-sm text-on-background/80">Total Revenue: ₹{Number(totalRevenue).toLocaleString()}</p>
        <p className="mt-1 text-sm text-on-background/80">Average Order Value: ₹{Number(avgOrderValue).toLocaleString()}</p>
      </Card>
    </div>
  )
}
