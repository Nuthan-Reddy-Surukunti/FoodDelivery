import { useState } from 'react'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'

const initialItems = [
  { id: 'pm1', name: 'Paneer Roll', price: 199, active: true },
  { id: 'pm2', name: 'Veg Biryani', price: 249, active: true },
  { id: 'pm3', name: 'Chocolate Lava Cake', price: 149, active: false },
]

export const MenuManagementPage = () => {
  const [items, setItems] = useState(initialItems)

  const toggleActive = (id) => {
    setItems((prev) => prev.map((item) => (item.id === id ? { ...item, active: !item.active } : item)))
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <div className="mb-5 flex items-center justify-between">
        <h1 className="text-2xl font-bold">Menu Management</h1>
        <Button size="sm">Add Item</Button>
      </div>

      <div className="space-y-3">
        {items.map((item) => (
          <Card key={item.id} className="flex items-center justify-between">
            <div>
              <p className="font-semibold">{item.name}</p>
              <p className="text-sm text-on-background/70">₹{item.price}</p>
            </div>
            <Button size="sm" variant={item.active ? 'secondary' : 'tertiary'} onClick={() => toggleActive(item.id)}>
              {item.active ? 'Mark Inactive' : 'Mark Active'}
            </Button>
          </Card>
        ))}
      </div>
    </div>
  )
}
