import { useNavigate } from 'react-router-dom'
import { CheckoutForm } from '../components/organisms/CheckoutForm'
import { useCart } from '../context/CartContext'
import { useNotification } from '../hooks/useNotification'

const slots = [
  { id: 's1', label: 'ASAP (30 mins)' },
  { id: 's2', label: '30-45 mins' },
  { id: 's3', label: '45-60 mins' },
]

export const CheckoutPage = () => {
  const navigate = useNavigate()
  const { clearCart } = useCart()
  const { showSuccess } = useNotification()

  const handlePlaceOrder = () => {
    clearCart()
    showSuccess('Order placed successfully')
    navigate('/orders')
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="mb-4 text-2xl font-bold">Checkout</h1>
      <CheckoutForm timeSlots={slots} onSubmit={handlePlaceOrder} />
    </div>
  )
}
