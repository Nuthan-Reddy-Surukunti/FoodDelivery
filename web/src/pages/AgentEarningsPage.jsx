import { useEffect, useState } from 'react'
import { AgentLayout } from '../components/organisms/AgentLayout'
import { useNotification } from '../hooks/useNotification'
import agentApi from '../services/agentApi'

const fmtDate = (iso) => iso
  ? new Date(iso).toLocaleString('en-IN', { dateStyle: 'medium', timeStyle: 'short' })
  : '—'

const fmtCurrency = (amount) =>
  `₹${Number(amount || 0).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`

export const AgentEarningsPage = () => {
  const { showError } = useNotification()
  const [summary, setSummary] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let active = true
    const load = async () => {
      setLoading(true)
      try {
        const data = await agentApi.getEarnings()
        if (active) setSummary(data)
      } catch (err) {
        if (active) showError(err.response?.data?.message || 'Failed to load earnings')
      } finally {
        if (active) setLoading(false)
      }
    }
    load()
    return () => { active = false }
  }, [])

  const kpis = [
    {
      label: "Today's Deliveries",
      value: loading ? '—' : (summary?.todayDeliveries ?? 0),
      icon: 'today',
      color: 'bg-sky-50 text-sky-600',
    },
    {
      label: "Today's Order Value",
      value: loading ? '—' : fmtCurrency(summary?.todayOrderValue),
      icon: 'payments',
      color: 'bg-emerald-50 text-emerald-600',
    },
    {
      label: 'Total Deliveries',
      value: loading ? '—' : (summary?.totalDeliveries ?? 0),
      icon: 'local_shipping',
      color: 'bg-violet-50 text-violet-600',
    },
    {
      label: 'Total Order Value',
      value: loading ? '—' : fmtCurrency(summary?.totalOrderValue),
      icon: 'account_balance_wallet',
      color: 'bg-amber-50 text-amber-600',
    },
  ]

  return (
    <AgentLayout title="Earnings & History">
      {/* KPI Bento grid */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
        {kpis.map(({ label, value, icon, color }) => (
          <div key={label} className="bg-white rounded-xl p-5 border border-slate-100 shadow-sm flex flex-col gap-3">
            <div className={`w-10 h-10 rounded-xl ${color} flex items-center justify-center`}>
              <span className="material-symbols-outlined text-xl">{icon}</span>
            </div>
            <div>
              <p className="text-xs text-on-surface-variant font-medium mb-0.5">{label}</p>
              <p className="text-2xl font-bold text-on-surface">
                {loading
                  ? <span className="inline-block w-20 h-6 bg-slate-200 animate-pulse rounded" />
                  : value}
              </p>
            </div>
          </div>
        ))}
      </div>

      {/* Delivery history table */}
      <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden">
        <div className="p-5 border-b border-slate-100 flex items-center gap-2">
          <span className="material-symbols-outlined text-primary">history</span>
          <h3 className="text-base font-semibold text-on-surface">Delivery History</h3>
          {!loading && summary?.history?.length > 0 && (
            <span className="ml-auto text-xs text-on-surface-variant bg-slate-100 px-2.5 py-1 rounded-full font-medium">
              {summary.history.length} records
            </span>
          )}
        </div>

        {loading && (
          <div className="divide-y divide-slate-50">
            {[1, 2, 3].map(i => (
              <div key={i} className="p-4 flex items-center gap-4">
                <div className="w-8 h-8 bg-slate-200 animate-pulse rounded-lg" />
                <div className="flex-1 space-y-2">
                  <div className="h-3 bg-slate-200 animate-pulse rounded w-3/4" />
                  <div className="h-3 bg-slate-100 animate-pulse rounded w-1/2" />
                </div>
                <div className="w-20 h-4 bg-slate-200 animate-pulse rounded" />
              </div>
            ))}
          </div>
        )}

        {!loading && (!summary?.history || summary.history.length === 0) && (
          <div className="py-16 text-center">
            <span className="material-symbols-outlined text-4xl text-slate-300 block mb-3">inbox</span>
            <p className="text-sm font-medium text-on-surface-variant">No deliveries completed yet</p>
          </div>
        )}

        {!loading && summary?.history?.length > 0 && (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-slate-50 text-on-surface-variant">
                  <th className="text-left font-medium px-5 py-3">Order ID</th>
                  <th className="text-right font-medium px-5 py-3">Items</th>
                  <th className="text-right font-medium px-5 py-3">Order Value</th>
                  <th className="text-right font-medium px-5 py-3">Delivered At</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-50">
                {summary.history.map((record, i) => (
                  <tr key={record.orderId || i} className="hover:bg-slate-50 transition-colors">
                    <td className="px-5 py-3.5 font-mono text-xs text-on-surface-variant">
                      #{String(record.orderId || '').split('-')[0].toUpperCase()}
                    </td>
                    <td className="px-5 py-3.5 text-right text-on-surface">
                      {record.itemCount ?? '—'}
                    </td>
                    <td className="px-5 py-3.5 text-right font-semibold text-on-surface">
                      {fmtCurrency(record.orderTotal)}
                    </td>
                    <td className="px-5 py-3.5 text-right text-on-surface-variant">
                      {fmtDate(record.deliveredAt)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </AgentLayout>
  )
}
