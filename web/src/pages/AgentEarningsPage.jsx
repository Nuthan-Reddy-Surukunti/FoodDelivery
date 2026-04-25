import { Card } from '../components/atoms/Card'

const history = [
  { id: 'DL-921', date: '2026-04-25', earnings: 92 },
  { id: 'DL-918', date: '2026-04-25', earnings: 74 },
  { id: 'DL-910', date: '2026-04-24', earnings: 88 },
]

export const AgentEarningsPage = () => {
  const total = history.reduce((sum, item) => sum + item.earnings, 0)

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">Earnings</h1>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Card>
          <p className="text-sm text-on-background/70">Today</p>
          <p className="mt-2 text-2xl font-bold">₹{total}</p>
        </Card>
        <Card>
          <p className="text-sm text-on-background/70">Completed Deliveries</p>
          <p className="mt-2 text-2xl font-bold">{history.length}</p>
        </Card>
      </div>

      <Card className="mt-5">
        <h2 className="mb-3 text-lg font-semibold">Recent Earnings</h2>
        <div className="space-y-2">
          {history.map((item) => (
            <div key={item.id} className="flex items-center justify-between rounded-lg border border-outline px-3 py-2 text-sm">
              <span>{item.id}</span>
              <span>{item.date}</span>
              <span className="font-semibold">₹{item.earnings}</span>
            </div>
          ))}
        </div>
      </Card>
    </div>
  )
}
