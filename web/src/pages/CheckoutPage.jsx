import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useCart } from '../context/CartContext'
import { useAuth } from '../context/AuthContext'
import { useNotification } from '../hooks/useNotification'
import { CheckoutForm } from '../components/organisms/CheckoutForm'
import orderApi from '../services/orderApi'
import api from '../services/api'

const VegDot = ({ isVeg }) => (
  <span className={`inline-flex h-3.5 w-3.5 flex-shrink-0 items-center justify-center rounded-sm border-2 ${isVeg ? 'border-green-600' : 'border-red-600'}`}>
    <span className={`h-1 w-1 rounded-full ${isVeg ? 'bg-green-600' : 'bg-red-600'}`} />
  </span>
)

// Payment method definitions
const PAYMENT_METHODS = [
  {
    id: 'CashOnDelivery',
    value: 3,
    label: 'Cash on Delivery',
    icon: 'payments',
    description: 'Pay in cash when your order arrives',
    color: 'text-amber-700',
    bg: 'bg-amber-50 border-amber-300',
    activeBg: 'bg-amber-50 border-amber-500 ring-2 ring-amber-200',
  },
  {
    id: 'Card',
    value: 2,
    label: 'Credit / Debit Card',
    icon: 'credit_card',
    description: 'Visa, Mastercard, RuPay accepted',
    color: 'text-blue-700',
    bg: 'bg-blue-50 border-blue-300',
    activeBg: 'bg-blue-50 border-blue-500 ring-2 ring-blue-200',
  },
  {
    id: 'Wallet',
    value: 1,
    label: 'Digital Wallet',
    icon: 'account_balance_wallet',
    description: 'Paytm, PhonePe, Google Pay',
    color: 'text-violet-700',
    bg: 'bg-violet-50 border-violet-300',
    activeBg: 'bg-violet-50 border-violet-500 ring-2 ring-violet-200',
  },
]

export const CheckoutPage = () => {
  const navigate = useNavigate()
  const { user } = useAuth()
  const { items, restaurantId, totalPrice, clearCart } = useCart()
  const { showSuccess, showError } = useNotification()
  const [isPlacingOrder, setIsPlacingOrder] = useState(false)
  const [selectedMethod, setSelectedMethod] = useState('CashOnDelivery')

  // Card fields
  const [cardNumber, setCardNumber] = useState('')
  const [cardExpiry, setCardExpiry] = useState('')
  const [cardCvv, setCardCvv] = useState('')
  const [cardHolder, setCardHolder] = useState('')

  // Wallet field
  const [walletId, setWalletId] = useState('')

  const formatCardNumber = (val) => {
    const digits = val.replace(/\D/g, '').slice(0, 16)
    return digits.replace(/(.{4})/g, '$1 ').trim()
  }

  const formatExpiry = (val) => {
    const digits = val.replace(/\D/g, '').slice(0, 4)
    if (digits.length >= 3) return `${digits.slice(0, 2)}/${digits.slice(2)}`
    return digits
  }

  const validatePayment = () => {
    if (selectedMethod === 'Card') {
      const digits = cardNumber.replace(/\D/g, '')
      if (digits.length < 12) return 'Please enter a valid card number'
      if (!cardExpiry.match(/^\d{2}\/\d{2}$/)) return 'Please enter a valid expiry (MM/YY)'
      if (cardCvv.length < 3) return 'Please enter a valid CVV'
      if (!cardHolder.trim()) return 'Please enter the card holder name'
    }
    if (selectedMethod === 'Wallet') {
      if (!walletId.trim()) return 'Please enter your wallet ID or UPI ID'
    }
    return null
  }

  const handlePlaceOrder = async (orderData) => {
    if (!user || !restaurantId || items.length === 0) {
      showError('Cannot place empty order')
      return
    }

    const validationError = validatePayment()
    if (validationError) {
      showError(validationError)
      return
    }

    try {
      setIsPlacingOrder(true)

      // Sync cart
      try { await orderApi.clearCart(user.id, restaurantId) } catch { /* ignore */ }
      for (const item of items) {
        await orderApi.addCartItem({
          userId: user.id, restaurantId,
          menuItemId: item.id, quantity: item.quantity,
        })
      }

      // Place order
      const orderRes = await orderApi.createOrder({
        userId: user.id, restaurantId,
        selectedAddressId: orderData.addressId,
        specialInstructions: orderData.specialInstructions || '',
      })

      // If the order's payment needs to be finalized with card/wallet details,
      // send them separately. COD is handled automatically during order creation.
      if (selectedMethod !== 'CashOnDelivery' && orderRes?.orderId) {
        const method = PAYMENT_METHODS.find(m => m.id === selectedMethod)
        await api.post(`/gateway/payments/${orderRes.orderId}/process`, {
          paymentMethod: method.value,
          amount: totalPrice,
          ...(selectedMethod === 'Card' && {
            cardNumber: cardNumber.replace(/\s/g, ''),
            cardExpiry,
            cardCvv,
            cardHolderName: cardHolder,
          }),
          ...(selectedMethod === 'Wallet' && { walletId }),
        })
      }

      clearCart()
      showSuccess('Order placed successfully!')
      navigate('/orders')
    } catch (error) {
      showError(error.response?.data?.message || error.message || 'Failed to place order')
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
          {/* Left column */}
          <div className="lg:col-span-2 space-y-6">
            {/* Delivery Address */}
            <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden">
              <div className="p-5 border-b border-slate-100 flex items-center gap-2">
                <span className="material-symbols-outlined text-primary">location_on</span>
                <h2 className="text-base font-semibold text-on-surface">Delivery Details</h2>
              </div>
              <div className="p-5">
                <CheckoutForm onSubmit={handlePlaceOrder} loading={isPlacingOrder} />
              </div>
            </div>

            {/* Payment Method Selector */}
            <div className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden">
              <div className="p-5 border-b border-slate-100 flex items-center gap-2">
                <span className="material-symbols-outlined text-primary">credit_card</span>
                <h2 className="text-base font-semibold text-on-surface">Payment Method</h2>
              </div>
              <div className="p-5 space-y-3">
                {/* Method cards */}
                {PAYMENT_METHODS.map(method => (
                  <button
                    key={method.id}
                    type="button"
                    onClick={() => setSelectedMethod(method.id)}
                    className={`w-full flex items-center gap-4 p-4 rounded-xl border-2 transition-all text-left ${selectedMethod === method.id ? method.activeBg : 'bg-white border-slate-200 hover:border-slate-300'}`}
                  >
                    <span className={`material-symbols-outlined text-2xl ${selectedMethod === method.id ? method.color : 'text-slate-400'}`} style={{ fontVariationSettings: "'FILL' 1" }}>
                      {method.icon}
                    </span>
                    <div className="flex-1">
                      <p className={`font-semibold text-sm ${selectedMethod === method.id ? 'text-on-surface' : 'text-on-surface-variant'}`}>
                        {method.label}
                      </p>
                      <p className="text-xs text-on-surface-variant">{method.description}</p>
                    </div>
                    <div className={`w-5 h-5 rounded-full border-2 flex items-center justify-center transition-all ${selectedMethod === method.id ? 'border-primary bg-primary' : 'border-slate-300'}`}>
                      {selectedMethod === method.id && <span className="w-2 h-2 rounded-full bg-white" />}
                    </div>
                  </button>
                ))}

                {/* Card details */}
                {selectedMethod === 'Card' && (
                  <div className="mt-4 p-4 bg-blue-50 rounded-xl border border-blue-100 space-y-3">
                    <div>
                      <label className="text-xs font-semibold text-slate-600 block mb-1">Card Number</label>
                      <input
                        type="text"
                        inputMode="numeric"
                        placeholder="1234 5678 9012 3456"
                        value={cardNumber}
                        onChange={e => setCardNumber(formatCardNumber(e.target.value))}
                        className="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 bg-white"
                        maxLength={19}
                      />
                    </div>
                    <div className="grid grid-cols-2 gap-3">
                      <div>
                        <label className="text-xs font-semibold text-slate-600 block mb-1">Expiry (MM/YY)</label>
                        <input
                          type="text"
                          inputMode="numeric"
                          placeholder="12/27"
                          value={cardExpiry}
                          onChange={e => setCardExpiry(formatExpiry(e.target.value))}
                          className="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 bg-white"
                          maxLength={5}
                        />
                      </div>
                      <div>
                        <label className="text-xs font-semibold text-slate-600 block mb-1">CVV</label>
                        <input
                          type="password"
                          inputMode="numeric"
                          placeholder="•••"
                          value={cardCvv}
                          onChange={e => setCardCvv(e.target.value.replace(/\D/g, '').slice(0, 4))}
                          className="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 bg-white"
                          maxLength={4}
                        />
                      </div>
                    </div>
                    <div>
                      <label className="text-xs font-semibold text-slate-600 block mb-1">Card Holder Name</label>
                      <input
                        type="text"
                        placeholder="As printed on card"
                        value={cardHolder}
                        onChange={e => setCardHolder(e.target.value)}
                        className="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 bg-white"
                      />
                    </div>
                  </div>
                )}

                {/* Wallet details */}
                {selectedMethod === 'Wallet' && (
                  <div className="mt-4 p-4 bg-violet-50 rounded-xl border border-violet-100">
                    <label className="text-xs font-semibold text-slate-600 block mb-1">UPI ID / Wallet ID</label>
                    <input
                      type="text"
                      placeholder="yourname@upi or phone number"
                      value={walletId}
                      onChange={e => setWalletId(e.target.value)}
                      className="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-violet-400 bg-white"
                    />
                    <p className="text-xs text-on-surface-variant mt-2">
                      Supports Paytm, PhonePe, Google Pay, and any UPI-enabled wallet.
                    </p>
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Right column — Order summary */}
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
                  <p className="text-xs text-on-surface-variant mb-0.5">Total</p>
                  <p className="text-2xl font-bold text-on-surface">₹{subtotal.toFixed(2)}</p>
                </div>
                {(() => {
                  const method = PAYMENT_METHODS.find(m => m.id === selectedMethod)
                  return (
                    <div className={`flex items-center gap-2 rounded-xl px-3 py-2 border ${method?.bg}`}>
                      <span className={`material-symbols-outlined text-lg ${method?.color}`} style={{ fontVariationSettings: "'FILL' 1" }}>
                        {method?.icon}
                      </span>
                      <span className={`text-xs font-semibold ${method?.color}`}>{method?.label}</span>
                    </div>
                  )
                })()}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

