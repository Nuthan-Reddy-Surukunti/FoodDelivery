import { Button } from '../atoms/Button'

export const CartItem = ({ item, onIncrease, onDecrease, onRemove }) => {
  return (
    <div className="flex items-center justify-between gap-3 rounded-xl border border-outline p-3">
      <div>
        <h4 className="font-medium">{item.name}</h4>
        <p className="text-sm text-on-background/70">₹{item.price} each</p>
      </div>
      <div className="flex items-center gap-2">
        <Button size="sm" variant="secondary" onClick={() => onDecrease?.(item)}>-</Button>
        <span className="min-w-6 text-center text-sm font-semibold">{item.quantity}</span>
        <Button size="sm" variant="secondary" onClick={() => onIncrease?.(item)}>+</Button>
        <Button size="sm" variant="tertiary" onClick={() => onRemove?.(item)}>Remove</Button>
      </div>
    </div>
  )
}

export default CartItem
