import { Card } from '../components/atoms/Card'

const metrics = [
  { label: 'Today Orders', value: 42 },
  { label: 'Pending Queue', value: 9 },
  { label: 'Revenue Today', value: '₹12,890' },
]

export const PartnerDashboardPage = () => {
  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">Partner Dashboard</h1>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        {metrics.map((item) => (
          <Card key={item.label}>
            <p className="text-sm text-on-background/70">{item.label}</p>
            <p className="mt-2 text-2xl font-bold">{item.value}</p>
          </Card>
        ))}
      </div>

      <Card className="mt-5">
        <h2 className="text-lg font-semibold">Action Items</h2>
        <ul className="mt-3 list-disc space-y-2 pl-5 text-sm text-on-background/80">
          <li>Review incoming orders and update prep times.</li>
          <li>Keep high-demand menu items in stock.</li>
          <li>Check customer ratings and respond quickly.</li>
        </ul>
      </Card>
    </div>
  )
}
