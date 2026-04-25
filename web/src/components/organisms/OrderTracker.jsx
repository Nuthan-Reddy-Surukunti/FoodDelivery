const timeline = ['Order Placed', 'Confirmed', 'Preparing', 'Picked Up', 'Delivered']

export const OrderTracker = ({ currentStatusIndex = 0 }) => {
  return (
    <div className="space-y-3 rounded-2xl border border-outline p-4">
      {timeline.map((step, index) => {
        const done = index <= currentStatusIndex
        return (
          <div key={step} className="flex items-center gap-3">
            <span className={`h-3 w-3 rounded-full ${done ? 'bg-primary' : 'bg-surface-dim'}`} />
            <p className={`text-sm ${done ? 'font-semibold text-on-background' : 'text-on-background/60'}`}>{step}</p>
          </div>
        )
      })}
    </div>
  )
}

export default OrderTracker
