import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useCart } from '../context/CartContext'
import { useNotification } from '../hooks/useNotification'
import orderApi from '../services/orderApi'

const VegDot = ({ isVeg }) => (
  <span className={`inline-flex h-4 w-4 flex-shrink-0 items-center justify-center rounded-sm border-2 ${isVeg ? 'border-green-600' : 'border-red-600'}`}>
    <span className={`h-1.5 w-1.5 rounded-full ${isVeg ? 'bg-green-600' : 'bg-red-600'}`} />
  </span>
)

export const CartPage = () => {
  const navigate = useNavigate()
  const { showSuccess, showError } = useNotification()
  const { items, totalPrice, updateQuantity, removeItem, restaurantId } = useCart()

  const [couponCode, setCouponCode] = useState('')
  const [couponLoading, setCouponLoading] = useState(false)
  const [discount, setDiscount] = useState(0)
  const [couponApplied, setCouponApplied] = useState('')

  const handleApplyCoupon = async () => {
    if (!couponCode.trim()) return
    setCouponLoading(true)
    try {
      const res = await orderApi.applyCoupon({ couponCode: couponCode.trim(), restaurantId })
      const discountVal = res?.discountAmount ?? res?.discount ?? 0
      setDiscount(discountVal)
      setCouponApplied(couponCode.trim())
      showSuccess(`Coupon "${couponCode.trim()}" applied! You saved ₹${discountVal.toFixed(2)}`)
    } catch (err) {
      showError(err.response?.data?.message || 'Invalid or expired coupon code')
      setDiscount(0)
      setCouponApplied('')
    } finally {
      setCouponLoading(false)
    }
  }

  const removeCoupon = () => {
    setDiscount(0)
    setCouponApplied('')
    setCouponCode('')
  }

  if (!items.length) {
    return (
      <div className="min-h-screen bg-background flex flex-col items-center justify-center px-6 py-24">
        <div className="text-center">
          <p className="text-6xl mb-6">🛒</p>
          <h1 className="text-2xl font-bold text-on-surface mb-2">Your cart is empty</h1>
          <p className="text-on-surface-variant text-sm mb-8">Add items from a restaurant to get started.</p>
          <Link
            to="/"
            className="bg-primary text-on-primary px-8 py-3 rounded-xl font-semibold hover:bg-primary-container transition-colors"
          >
            Browse Restaurants
          </Link>
        </div>
      </div>
    )
  }

  const subtotal = Number(totalPrice || 0)
  const deliveryFee = 0
  const total = Math.max(0, subtotal - discount + deliveryFee)

  return (
    <div className="min-h-screen bg-background">
      <div className="max-w-5xl mx-auto px-6 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-[32px] font-bold text-on-surface mb-1">Your Cart</h1>
          <p className="text-on-surface-variant text-sm">{items.length} item{items.length > 1 ? 's' : ''} from your restaurant</p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Left — items list */}
          <div className="lg:col-span-2 space-y-4">
            <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden">
              <div className="p-5 border-b border-slate-100">
                <h2 className="text-base font-semibold text-on-surface">Order Items</h2>
              </div>
              <div className="divide-y divide-slate-50">
                {items.map((item) => (
                  <div key={item.id} className="p-5 flex items-start gap-4 hover:bg-slate-50 transition-colors">
                    {/* Food image */}
                    <div className="w-16 h-16 rounded-xl bg-slate-100 flex items-center justify-center flex-shrink-0 text-2xl overflow-hidden">
                      {item.imageUrl ? (
                        <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
                      ) : '🍽️'}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 mb-1">
                        <VegDot isVeg={item.isVeg ?? true} />
                        <h3 className="font-semibold text-on-surface text-sm">{item.name}</h3>
                      </div>
                      <p className="text-primary font-bold text-base">₹{Number(item.price).toFixed(2)}</p>
                    </div>
                    {/* Quantity controls */}
                    <div className="flex items-center gap-3 flex-shrink-0">
                      <div className="flex items-center gap-2 bg-slate-100 rounded-xl p-1">
                        <button
                          onClick={() => item.quantity > 1 ? updateQuantity(item.id, item.quantity - 1) : removeItem(item.id)}
                          className="w-8 h-8 rounded-lg bg-white shadow-sm flex items-center justify-center text-on-surface hover:bg-primary hover:text-white transition-colors"
                          aria-label="Decrease"
                        >
                          <span className="material-symbols-outlined text-base">remove</span>
                        </button>
                        <span className="w-5 text-center font-bold text-on-surface text-sm">{item.quantity}</span>
                        <button
                          onClick={() => updateQuantity(item.id, item.quantity + 1)}
                          className="w-8 h-8 rounded-lg bg-white shadow-sm flex items-center justify-center text-on-surface hover:bg-primary hover:text-white transition-colors"
                          aria-label="Increase"
                        >
                          <span className="material-symbols-outlined text-base">add</span>
                        </button>
                      </div>
                      <button
                        onClick={() => removeItem(item.id)}
                        className="p-2 text-slate-400 hover:text-red-500 transition-colors"
                        aria-label="Remove"
                      >
                        <span className="material-symbols-outlined text-lg">delete</span>
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Coupon section */}
            <div className="bg-white rounded-xl border border-slate-100 shadow-sm p-5">
              <h2 className="text-base font-semibold text-on-surface mb-4 flex items-center gap-2">
                <span className="material-symbols-outlined text-primary text-lg">confirmation_number</span>
                Promo Code
              </h2>

              {couponApplied ? (
                <div className="flex items-center justify-between bg-emerald-50 border border-emerald-200 rounded-xl px-4 py-3">
                  <div className="flex items-center gap-2">
                    <span className="material-symbols-outlined text-emerald-600 text-lg">check_circle</span>
                    <div>
                      <p className="text-sm font-semibold text-emerald-800">"{couponApplied}" applied</p>
                      <p className="text-xs text-emerald-600">You saved ₹{discount.toFixed(2)}</p>
                    </div>
                  </div>
                  <button
                    onClick={removeCoupon}
                    className="text-xs text-emerald-600 hover:text-red-600 font-medium transition-colors"
                  >
                    Remove
                  </button>
                </div>
              ) : (
                <div className="flex gap-3">
                  <input
                    id="coupon-input"
                    type="text"
                    value={couponCode}
                    onChange={(e) => setCouponCode(e.target.value.toUpperCase())}
                    onKeyDown={(e) => e.key === 'Enter' && handleApplyCoupon()}
                    placeholder="Enter promo code"
                    className="flex-1 rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm text-on-surface focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition"
                  />
                  <button
                    onClick={handleApplyCoupon}
                    disabled={couponLoading || !couponCode.trim()}
                    className="bg-primary text-on-primary px-5 py-2.5 rounded-xl text-sm font-semibold hover:bg-primary-container transition-colors disabled:opacity-50"
                  >
                    {couponLoading ? '...' : 'Apply'}
                  </button>
                </div>
              )}
            </div>
          </div>

          {/* Right — order summary */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-xl border border-slate-100 shadow-sm p-6 sticky top-24">
              <h2 className="text-base font-semibold text-on-surface mb-5">Order Summary</h2>
              <div className="flex flex-col gap-3 text-sm text-on-surface-variant mb-5">
                <div className="flex justify-between">
                  <span>Subtotal ({items.length} item{items.length > 1 ? 's' : ''})</span>
                  <span className="font-medium text-on-surface">₹{subtotal.toFixed(2)}</span>
                </div>
                {discount > 0 && (
                  <div className="flex justify-between text-emerald-600">
                    <span>Promo Discount</span>
                    <span className="font-medium">- ₹{discount.toFixed(2)}</span>
                  </div>
                )}
                <div className="flex justify-between">
                  <span>Delivery Fee</span>
                  <span className="text-emerald-600 font-medium">Free</span>
                </div>
              </div>
              <div className="border-t border-slate-200 pt-4 mb-6 flex justify-between font-bold text-lg text-on-surface">
                <span>Total (COD)</span>
                <span>₹{total.toFixed(2)}</span>
              </div>
              <button
                id="proceed-checkout-btn"
                onClick={() => navigate('/checkout')}
                className="w-full bg-primary text-on-primary py-3.5 rounded-xl font-semibold hover:bg-primary-container active:scale-95 transition-all flex items-center justify-center gap-2"
              >
                <span className="material-symbols-outlined text-base">arrow_forward</span>
                Proceed to Checkout
              </button>
              <Link
                to={restaurantId ? `/restaurant/${restaurantId}` : '/'}
                className="mt-3 block text-center text-sm text-primary font-medium hover:text-primary-container transition-colors"
              >
                + Add more items
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
