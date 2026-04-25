import { Card } from '../components/atoms/Card'

const cards = [
  { label: 'Active Restaurants', value: 182 },
  { label: 'Orders Today', value: 1289 },
  { label: 'Pending Approvals', value: 12 },
  { label: 'Refund Requests', value: 6 },
]

export const AdminOverviewPage = () => {
  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">Admin Overview</h1>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {cards.map((card) => (
          <Card key={card.label}>
            <p className="text-sm text-on-background/70">{card.label}</p>
            <p className="mt-2 text-2xl font-bold">{card.value}</p>
          </Card>
        ))}
      </div>
      <Card className="mt-5">
        <h2 className="text-lg font-semibold">Platform Health</h2>
        <p className="mt-2 text-sm text-on-background/80">API uptime is stable. No major incidents in the last 24 hours.</p>
      </Card>
    </div>
  )
}
