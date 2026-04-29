import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useCart } from '../context/CartContext'
import { useNotification } from '../hooks/useNotification'
import orderApi from '../services/orderApi'

const VegDot = ({ isVeg }) => (
  <span className={`inline-flex h-4 w-4 flex-shrink-0 items-center justify-center rounded-sm border-2 ${isVeg ? 'border-green-600' : 'border-red-500'}`}>
    <span className={`h-1.5 w-1.5 rounded-full ${isVeg ? 'bg-green-600' : 'bg-red-500'}`} />
  </span>
)

const isGenericItemName = (name) => /^item\s*\d+$/i.test((name || '').trim())
const getItemDisplayName = (item) => {
  const candidates = [item.menuItemName, item.itemName, item.name, item.title]
  const valid = candidates.find((v) => v && !isGenericItemName(v))
  return valid || 'Menu Item'
}

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
      <div className="min-h-screen bg-slate-50 flex flex-col items-center justify-center px-6 py-24">
        <div className="text-center animate-fade-in-up">
          <div className="w-24 h-24 bg-slate-100 rounded-3xl flex items-center justify-center mx-auto mb-6 text-5xl">🛒</div>
          <h1 className="text-2xl font-extrabold text-slate-900 mb-2">Your cart is empty</h1>
          <p className="text-slate-500 text-sm mb-8">Add items from a restaurant to get started.</p>
          <Link
            to="/"
            className="btn-primary-gradient text-white px-8 py-3.5 rounded-xl font-semibold inline-flex items-center gap-2"
          >
            <span className="material-symbols-outlined text-lg">explore</span>
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
    <div className="min-h-screen bg-slate-50">
      {/* ── Gradient Header Banner ── */}
      <div className="bg-gradient-to-r from-slate-900 via-slate-800 to-primary px-6 py-8">
        <div className="max-w-5xl mx-auto">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-white/10 backdrop-blur-sm rounded-xl flex items-center justify-center">
              <span className="material-symbols-outlined text-white text-xl" style={{ fontVariationSettings: "'FILL' 1" }}>shopping_cart</span>
            </div>
            <div>
              <h1 className="text-2xl font-extrabold text-white">Your Cart</h1>
              <p className="text-white/60 text-sm">{items.length} item{items.length > 1 ? 's' : ''} from your restaurant</p>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-5xl mx-auto px-6 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Left — Items */}
          <div className="lg:col-span-2 space-y-4">
            {/* Order Items Card */}
            <div className="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
              <div className="p-5 border-b border-slate-100 flex items-center gap-2">
                <span className="material-symbols-outlined text-primary text-lg" style={{ fontVariationSettings: "'FILL' 1" }}>restaurant_menu</span>
                <h2 className="text-base font-bold text-slate-900">Order Items</h2>
              </div>
              <div className="divide-y divide-slate-50">
                {items.map((item) => (
                  <div key={item.id} className="p-5 flex items-center gap-4 hover:bg-slate-50/50 transition-colors">
                    {/* Food image */}
                    <div className="w-16 h-16 rounded-xl bg-slate-100 flex items-center justify-center flex-shrink-0 text-2xl overflow-hidden border border-slate-100">
                      {item.imageUrl
                        ? <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
                        : '🍽️'}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 mb-1">
                        <VegDot isVeg={item.isVeg ?? true} />
                        <h3 className="font-semibold text-slate-900 text-sm">{getItemDisplayName(item)}</h3>
                      </div>
                      <p className="text-primary font-bold text-base">₹{Number(item.price).toFixed(2)}</p>
                    </div>
                    {/* Quantity controls */}
                    <div className="flex items-center gap-2 flex-shrink-0">
                      <div className="flex items-center gap-1 bg-slate-100 rounded-xl p-1">
                        <button
                          onClick={() => item.quantity > 1 ? updateQuantity(item.id, item.quantity - 1) : removeItem(item.id)}
                          className="w-8 h-8 rounded-lg bg-white shadow-sm flex items-center justify-center text-slate-700 hover:bg-primary hover:text-white transition-all active:scale-95"
                          aria-label="Decrease"
                        >
                          <span className="material-symbols-outlined text-base">remove</span>
                        </button>
                        <span className="w-6 text-center font-bold text-slate-900 text-sm">{item.quantity}</span>
                        <button
                          onClick={() => updateQuantity(item.id, item.quantity + 1)}
                          className="w-8 h-8 rounded-lg bg-white shadow-sm flex items-center justify-center text-slate-700 hover:bg-primary hover:text-white transition-all active:scale-95"
                          aria-label="Increase"
                        >
                          <span className="material-symbols-outlined text-base">add</span>
                        </button>
                      </div>
                      <button
                        onClick={() => removeItem(item.id)}
                        className="p-2 rounded-lg text-slate-300 hover:text-rose-500 hover:bg-rose-50 transition-all"
                        aria-label="Remove"
                      >
                        <span className="material-symbols-outlined text-lg">delete</span>
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Coupon */}
            <div className="bg-white rounded-2xl border border-slate-100 shadow-sm p-5">
              <h2 className="text-base font-bold text-slate-900 mb-4 flex items-center gap-2">
                <span className="material-symbols-outlined text-primary text-lg" style={{ fontVariationSettings: "'FILL' 1" }}>confirmation_number</span>
                Promo Code
              </h2>
              {couponApplied ? (
                <div className="flex items-center justify-between bg-emerald-50 border border-emerald-200 rounded-xl px-4 py-3">
                  <div className="flex items-center gap-2">
                    <span className="material-symbols-outlined text-emerald-600 text-lg">check_circle</span>
                    <div>
                      <p className="text-sm font-bold text-emerald-800">"{couponApplied}" applied 🎉</p>
                      <p className="text-xs text-emerald-600">You saved ₹{discount.toFixed(2)}</p>
                    </div>
                  </div>
                  <button onClick={removeCoupon} className="text-xs text-emerald-600 hover:text-rose-600 font-semibold transition-colors">
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
                    className="flex-1 input-premium rounded-xl px-4 py-2.5 text-sm"
                  />
                  <button
                    onClick={handleApplyCoupon}
                    disabled={couponLoading || !couponCode.trim()}
                    className="btn-primary-gradient text-white px-5 py-2.5 rounded-xl text-sm font-semibold disabled:opacity-50"
                  >
                    {couponLoading ? '...' : 'Apply'}
                  </button>
                </div>
              )}
            </div>
          </div>

          {/* Right — Order Summary */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-2xl border border-slate-100 shadow-sm p-6 sticky top-24">
              <h2 className="text-base font-bold text-slate-900 mb-5 flex items-center gap-2">
                <span className="material-symbols-outlined text-primary text-lg">receipt_long</span>
                Order Summary
              </h2>

              <div className="space-y-3 text-sm text-slate-500 mb-5">
                <div className="flex justify-between">
                  <span>Subtotal ({items.length} item{items.length > 1 ? 's' : ''})</span>
                  <span className="font-semibold text-slate-900">₹{subtotal.toFixed(2)}</span>
                </div>
                {discount > 0 && (
                  <div className="flex justify-between text-emerald-600">
                    <span>Promo Discount</span>
                    <span className="font-semibold">- ₹{discount.toFixed(2)}</span>
                  </div>
                )}
                <div className="flex justify-between">
                  <span>Delivery Fee</span>
                  <span className="font-semibold text-emerald-600">🎉 Free</span>
                </div>
              </div>

              <div className="border-t border-slate-100 pt-4 mb-5 flex justify-between items-center">
                <span className="font-bold text-slate-900">Total</span>
                <span className="text-2xl font-extrabold text-slate-900">₹{total.toFixed(2)}</span>
              </div>

              <button
                id="proceed-checkout-btn"
                onClick={() => navigate('/checkout')}
                className="w-full btn-primary-gradient text-white py-4 rounded-xl font-bold flex items-center justify-center gap-2 active:scale-95 transition-all"
              >
                <span>Proceed to Checkout</span>
                <span className="material-symbols-outlined text-lg">arrow_forward</span>
              </button>

              <Link
                to={restaurantId ? `/restaurant/${restaurantId}` : '/'}
                className="mt-4 block text-center text-sm text-primary font-semibold hover:text-primary-container transition-colors"
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
