import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useCart } from '../context/CartContext'
import { useAuth } from '../context/AuthContext'
import { useNotification } from '../hooks/useNotification'
import { CheckoutForm } from '../components/organisms/CheckoutForm'
import orderApi from '../services/orderApi'

const VegDot = ({ isVeg }) => (
  <span className={`inline-flex h-3.5 w-3.5 flex-shrink-0 items-center justify-center rounded-sm border-2 ${isVeg ? 'border-green-600' : 'border-red-600'}`}>
    <span className={`h-1 w-1 rounded-full ${isVeg ? 'bg-green-600' : 'bg-red-600'}`} />
  </span>
)

export const CheckoutPage = () => {
  const navigate = useNavigate()
  const { user } = useAuth()
  const { items, restaurantId, totalPrice, clearCart } = useCart()
  const { showSuccess, showError } = useNotification()
  const [isPlacingOrder, setIsPlacingOrder] = useState(false)

  const handlePlaceOrder = async (orderData) => {
    if (!user || !restaurantId || items.length === 0) {
      showError('Cannot place empty order')
      return
    }
    try {
      setIsPlacingOrder(true)
      try { await orderApi.clearCart(user.id, restaurantId) } catch { /* ignore */ }
      for (const item of items) {
        await orderApi.addCartItem({
          userId: user.id, restaurantId,
          menuItemId: item.id, quantity: item.quantity,
        })
      }
      await orderApi.createOrder({
        userId: user.id, restaurantId,
        selectedAddressId: orderData.addressId,
        specialInstructions: orderData.specialInstructions || '',
      })
      clearCart()
      showSuccess('Order placed successfully!')
      navigate('/orders')
    } catch (error) {
      showError(error.message || 'Failed to place order')
    } finally {
      setIsPlacingOrder(false)
    }
  }

  if (!items.length) {
    return (
      <div className="min-h-screen bg-background flex flex-col items-center justify-center px-6 py-24 text-center">
        <p className="text-6xl mb-6">🛒</p>
        <h1 className="text-2xl font-bold text-on-surface mb-2">Your cart is empty</h1>
        <Link to="/" className="bg-primary text-on-primary px-8 py-3 rounded-xl font-semibold hover:bg-primary-container transition-colors mt-6">
          Browse Restaurants
        </Link>
      </div>
    )
  }

  const subtotal = Number(totalPrice || 0)

  return (
    <div className="min-h-screen bg-background pb-16">
      <div className="max-w-5xl mx-auto px-6 py-8">
        {/* Header */}
        <div className="mb-8 flex items-center gap-4">
          <Link to="/cart" className="p-2 rounded-xl hover:bg-slate-100 transition-colors text-slate-500">
            <span className="material-symbols-outlined">arrow_back</span>
          </Link>
          <div>
            <h1 className="text-[32px] font-bold text-on-surface leading-tight">Checkout</h1>
            <p className="text-on-surface-variant text-sm">Complete your order details</p>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Left column — address + form */}
          <div className="lg:col-span-2">
            <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden">
              <div className="p-5 border-b border-slate-100 flex items-center gap-2">
                <span className="material-symbols-outlined text-primary">location_on</span>
                <h2 className="text-base font-semibold text-on-surface">Delivery Details</h2>
              </div>
              <div className="p-5">
                <CheckoutForm onSubmit={handlePlaceOrder} loading={isPlacingOrder} />
              </div>
            </div>
          </div>

          {/* Right column — order summary */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-xl border border-slate-100 shadow-sm p-6 sticky top-24">
              <h2 className="text-base font-semibold text-on-surface mb-5 flex items-center gap-2">
                <span className="material-symbols-outlined text-primary text-xl">receipt_long</span>
                Order Summary
              </h2>

              <div className="flex flex-col gap-3 mb-5">
                {items.map((item) => (
                  <div key={item.id} className="flex items-start gap-3">
                    <div className="w-12 h-12 rounded-lg bg-slate-100 flex items-center justify-center flex-shrink-0 text-xl overflow-hidden">
                      {item.imageUrl ? (
                        <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
                      ) : '🍽️'}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-1.5 mb-0.5">
                        <VegDot isVeg={item.isVeg ?? true} />
                        <p className="text-sm font-medium text-on-surface truncate">{item.name}</p>
                      </div>
                      <p className="text-xs text-on-surface-variant">{item.quantity}× ₹{Number(item.price).toFixed(2)}</p>
                    </div>
                    <p className="text-sm font-semibold text-on-surface flex-shrink-0">
                      ₹{(item.quantity * Number(item.price)).toFixed(2)}
                    </p>
                  </div>
                ))}
              </div>

              <div className="border-t border-slate-200 pt-4 space-y-2 text-sm text-on-surface-variant">
                <div className="flex justify-between">
                  <span>Subtotal</span>
                  <span className="text-on-surface font-medium">₹{subtotal.toFixed(2)}</span>
                </div>
                <div className="flex justify-between">
                  <span>Delivery Fee</span>
                  <span className="text-emerald-600 font-medium">Free</span>
                </div>
              </div>

              <div className="border-t border-slate-200 mt-4 pt-4 flex justify-between items-center">
                <div>
                  <p className="text-xs text-on-surface-variant mb-0.5">Total (COD)</p>
                  <p className="text-2xl font-bold text-on-surface">₹{subtotal.toFixed(2)}</p>
                </div>
                <div className="flex items-center gap-2 bg-amber-50 border border-amber-200 rounded-xl px-3 py-2">
                  <span className="material-symbols-outlined text-amber-700 text-lg" style={{ fontVariationSettings: "'FILL' 1" }}>payments</span>
                  <span className="text-xs font-semibold text-amber-800">Cash on Delivery</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
