import { useMemo } from 'react'
import { useParams } from 'react-router-dom'
import { MenuSection } from '../components/organisms/MenuSection'
import { useCart } from '../context/CartContext'
import { useNotification } from '../hooks/useNotification'

const menuByRestaurant = {
  r1: [
    { id: 'm1', name: 'Butter Chicken Bowl', description: 'Creamy curry with saffron rice', price: 299 },
    { id: 'm2', name: 'Paneer Tikka Wrap', description: 'Tandoori paneer with mint sauce', price: 219 },
  ],
  r2: [
    { id: 'm3', name: 'Salmon Maki', description: '8 piece fresh salmon roll', price: 349 },
    { id: 'm4', name: 'Crispy Tempura Roll', description: 'Shrimp tempura with spicy mayo', price: 289 },
  ],
  r3: [
    { id: 'm5', name: 'Smash Burger', description: 'Double patty, cheddar, pickles', price: 259 },
    { id: 'm6', name: 'Loaded Fries', description: 'Cheese sauce and jalapenos', price: 179 },
  ],
  r4: [
    { id: 'm7', name: 'Margherita Pizza', description: 'Classic basil and mozzarella', price: 329 },
    { id: 'm8', name: 'Pepperoni Pizza', description: 'Spicy pepperoni and olives', price: 389 },
  ],
}

export const RestaurantDetailsPage = () => {
  const { id } = useParams()
  const { addItem } = useCart()
  const { showSuccess } = useNotification()

  const menuItems = useMemo(() => menuByRestaurant[id] || [], [id])

  const handleAddToCart = (item) => {
    addItem(item, id)
    showSuccess(`${item.name} added to cart`)
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <section className="mb-6 rounded-3xl border border-outline bg-surface p-6">
        <h1 className="text-2xl font-bold">Restaurant Menu</h1>
        <p className="mt-2 text-sm text-on-background/70">Pick your favorites and add them to cart.</p>
      </section>

      <MenuSection title="Popular Items" items={menuItems} onAddToCart={handleAddToCart} />
    </div>
  )
}
