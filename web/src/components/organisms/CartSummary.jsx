import { Button } from '../atoms/Button'
import { CartItem } from '../molecules/CartItem'

export const CartSummary = ({ items = [], total = 0, onIncrease, onDecrease, onRemove, onCheckout }) => {
  return (
    <div className="space-y-4">
      {items.map((item) => (
        <CartItem
          key={item.id}
          item={item}
          onIncrease={onIncrease}
          onDecrease={onDecrease}
          onRemove={onRemove}
        />
      ))}

      <div className="rounded-xl border border-outline p-4">
        <div className="mb-3 flex items-center justify-between text-sm">
          <span>Total</span>
          <span className="font-semibold">₹{total.toFixed(2)}</span>
        </div>
        <Button fullWidth onClick={onCheckout} disabled={!items.length}>Proceed to Checkout</Button>
      </div>
    </div>
  )
}

export default CartSummary
