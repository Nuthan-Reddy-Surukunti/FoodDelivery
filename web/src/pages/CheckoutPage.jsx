import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { CheckoutForm } from '../components/organisms/CheckoutForm'
import { useCart } from '../context/CartContext'
import { useAuth } from '../context/AuthContext'
import { useNotification } from '../hooks/useNotification'
import orderApi from '../services/orderApi'

export const CheckoutPage = () => {
  const navigate = useNavigate()
  const { user } = useAuth()
  const { items, restaurantId, clearCart } = useCart()
  const { showSuccess, showError } = useNotification()
  const [isPlacingOrder, setIsPlacingOrder] = useState(false)

  const handlePlaceOrder = async (orderData) => {
    if (!user || !restaurantId || items.length === 0) {
      showError('Cannot place empty order')
      return
    }

    try {
      setIsPlacingOrder(true)
      
      try {
        await orderApi.clearCart(user.id, restaurantId)
      } catch (e) {
        // Ignore if cart doesn't exist
      }

      for (const item of items) {
        await orderApi.addCartItem({
          userId: user.id,
          restaurantId: restaurantId,
          menuItemId: item.id,
          quantity: item.quantity
        })
      }

      const payload = {
        userId: user.id,
        restaurantId: restaurantId,
        selectedAddressId: orderData.addressId,
        specialInstructions: orderData.specialInstructions || ''
      }
      
      await orderApi.createOrder(payload)

      clearCart()
      showSuccess('Order placed successfully')
      navigate('/orders')
    } catch (error) {
      showError(error.message || 'Failed to place order')
    } finally {
      setIsPlacingOrder(false)
    }
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <h1 className="mb-4 text-2xl font-bold">Checkout</h1>
      <CheckoutForm onSubmit={handlePlaceOrder} loading={isPlacingOrder} />
    </div>
  )
}
