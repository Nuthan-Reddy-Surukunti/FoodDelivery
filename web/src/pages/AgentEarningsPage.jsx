import { useEffect, useState } from 'react'
import { Card } from '../components/atoms/Card'
import { useNotification } from '../hooks/useNotification'
import api from '../services/api'

const formatDate = (iso) => {
  if (!iso) return ''
  return new Date(iso).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' })
}

const isToday = (iso) => {
  if (!iso) return false
  const d = new Date(iso)
  const now = new Date()
  return d.getDate() === now.getDate() && d.getMonth() === now.getMonth() && d.getFullYear() === now.getFullYear()
}

export const AgentEarningsPage = () => {
  const { showError } = useNotification()
  const [deliveries, setDeliveries] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let active = true
    const load = async () => {
      setLoading(true)
      try {
        const response = await api.get('/gateway/orders/deliveries/assigned')
        const arr = Array.isArray(response.data) ? response.data : (response.data?.items || [])
        // Only show completed deliveries
        const completed = arr.filter(d => {
          const status = d.deliveryAssignment?.currentStatus || d.orderStatus || ''
          return status === 'Delivered'
        })
        if (!active) return
        setDeliveries(completed)
      } catch (err) {
        if (!active) return
        showError(err.response?.data?.message || 'Failed to load earnings')
      } finally {
        if (active) setLoading(false)
      }
    }
    load()
    return () => { active = false }
  }, [showError])

  const todayDeliveries = deliveries.filter(d => isToday(d.deliveryAssignment?.deliveredAt || d.updatedAt))
  const totalEarnings = deliveries.reduce((sum, d) => sum + Number(d.total || d.totalAmount || 0), 0)
  const todayEarnings = todayDeliveries.reduce((sum, d) => sum + Number(d.total || d.totalAmount || 0), 0)

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-bold">Earnings</h1>

      {loading ? (
        <p className="text-sm text-on-background/70">Loading earnings...</p>
      ) : (
        <>
          {/* Summary cards */}
          <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            <Card className="p-4">
              <p className="text-xs text-on-background/60">Today's Deliveries</p>
              <p className="mt-2 text-2xl font-bold">{todayDeliveries.length}</p>
            </Card>
            <Card className="p-4">
              <p className="text-xs text-on-background/60">Today's Orders Value</p>
              <p className="mt-2 text-2xl font-bold">₹{todayEarnings.toLocaleString()}</p>
            </Card>
            <Card className="p-4">
              <p className="text-xs text-on-background/60">Total Deliveries</p>
              <p className="mt-2 text-2xl font-bold">{deliveries.length}</p>
            </Card>
            <Card className="p-4">
              <p className="text-xs text-on-background/60">Total Orders Value</p>
              <p className="mt-2 text-2xl font-bold">₹{totalEarnings.toLocaleString()}</p>
            </Card>
          </div>

          {/* Delivery history */}
          <Card className="p-5">
            <h2 className="mb-4 text-lg font-semibold">Delivery History</h2>
            {deliveries.length === 0 ? (
              <p className="text-sm text-on-background/60">No completed deliveries yet.</p>
            ) : (
              <div className="space-y-2">
                {deliveries.map(delivery => {
                  const id = delivery.orderId || delivery.id
                  const deliveredAt = delivery.deliveryAssignment?.deliveredAt || delivery.updatedAt
                  const addr = delivery.deliveryAddress
                  const addrCity = addr?.city || ''
                  const total = Number(delivery.total || delivery.totalAmount || 0)
                  return (
                    <div
                      key={id}
                      className="flex flex-wrap items-center justify-between gap-2 rounded-xl border border-outline px-4 py-3 text-sm"
                    >
                      <div>
                        <span className="font-semibold">#{id.split('-')[0].toUpperCase()}</span>
                        {addrCity && <span className="ml-2 text-on-background/60">{addrCity}</span>}
                      </div>
                      <span className="text-on-background/50">{formatDate(deliveredAt)}</span>
                      <span className="font-semibold text-green-600">₹{total}</span>
                    </div>
                  )
                })}
              </div>
            )}
          </Card>

          <p className="mt-4 text-center text-xs text-on-background/50">
            Earnings reflect total order value of completed deliveries (COD basis)
          </p>
        </>
      )}
    </div>
  )
}
