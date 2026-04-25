import { useNavigate } from 'react-router-dom'
import { CartSummary } from '../components/organisms/CartSummary'
import { useCart } from '../context/CartContext'

export const CartPage = () => {
  const navigate = useNavigate()
  const { items, totalPrice, updateQuantity, removeItem } = useCart()

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">Your Cart</h1>

      {items.length ? (
        <CartSummary
          items={items}
          total={totalPrice}
          onIncrease={(item) => updateQuantity(item.id, item.quantity + 1)}
          onDecrease={(item) => updateQuantity(item.id, item.quantity - 1)}
          onRemove={(item) => removeItem(item.id)}
          onCheckout={() => navigate('/checkout')}
        />
      ) : (
        <p className="rounded-xl border border-outline bg-surface p-4 text-sm text-on-background/70">
          Cart is empty. Add items from a restaurant to continue.
        </p>
      )}
    </div>
  )
}
