import { Button } from '../atoms/Button'
import { Card } from '../atoms/Card'

export const MenuItem = ({ item, onAdd }) => {
  return (
    <Card className="flex items-start justify-between gap-3">
      <div>
        <h4 className="font-semibold">{item.name}</h4>
        <p className="text-sm text-on-background/70">{item.description}</p>
        <p className="mt-2 text-sm font-semibold">₹{item.price}</p>
      </div>
      <Button size="sm" onClick={() => onAdd?.(item)}>Add</Button>
    </Card>
  )
}

export default MenuItem
