import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useCart } from '../context/CartContext'
import { useAuth } from '../context/AuthContext'
import { useNotification } from '../hooks/useNotification'
import { CheckoutForm } from '../components/organisms/CheckoutForm'
import orderApi from '../services/orderApi'
import api from '../services/api'
import catalogApi from '../services/catalogApi'

const VegDot = ({ isVeg }) => (
  <span className={`inline-flex h-3.5 w-3.5 flex-shrink-0 items-center justify-center rounded-sm border-2 ${isVeg ? 'border-green-600' : 'border-red-600'}`}>
    <span className={`h-1 w-1 rounded-full ${isVeg ? 'bg-green-600' : 'bg-red-600'}`} />
  </span>
)

const isGenericItemName = (name) => /^item\s*\d+$/i.test((name || '').trim())
const getItemDisplayName = (item) => {
  const candidates = [item.menuItemName, item.itemName, item.name, item.title]
  const valid = candidates.find((value) => value && !isGenericItemName(value))
  return valid || 'Menu Item'
}

// Payment method definitions
const PAYMENT_METHODS = [
  {
    id: 'CashOnDelivery',
    value: 2,
    label: 'Cash on Delivery',
    icon: 'payments',
    description: 'Pay in cash when your order arrives',
    color: 'text-amber-700',
    bg: 'bg-amber-50 border-amber-300',
    activeBg: 'bg-amber-50 border-amber-500 ring-2 ring-amber-200',
  },
  {
    id: 'Online',
    value: 1,
    label: 'Online Payment',
    icon: 'account_balance_wallet',
    description: 'Pay securely via Razorpay (UPI, Cards, Wallets)',
    color: 'text-blue-700',
    bg: 'bg-blue-50 border-blue-300',
    activeBg: 'bg-blue-50 border-blue-500 ring-2 ring-blue-200',
    branding: (
      <div className="mt-3 pt-3 border-t border-blue-100 flex items-center gap-2">
        <span className="text-[10px] uppercase tracking-wider text-blue-400 font-bold">Securely processed by</span>
        <img 
          src="https://upload.wikimedia.org/wikipedia/commons/8/89/Razorpay_logo.svg" 
          alt="Razorpay" 
          className="h-3 opacity-70"
        />
      </div>
    )
  },
]

export const CheckoutPage = () => {
  const navigate = useNavigate()
  const { user } = useAuth()
  const { items, restaurantId, totalPrice, clearCart } = useCart()
  const { showSuccess, showError } = useNotification()
  const [isPlacingOrder, setIsPlacingOrder] = useState(false)
  const [selectedMethod, setSelectedMethod] = useState('CashOnDelivery')

  const [checkoutData, setCheckoutData] = useState({ addressId: null, deliveryOption: 'standard' })
  const [restaurantInfo, setRestaurantInfo] = useState({ name: 'QuickBite Restaurant', branch: 'Selected Branch' })

  useEffect(() => {
    // Dynamically load Razorpay SDK
    const script = document.createElement('script')
    script.src = 'https://checkout.razorpay.com/v1/checkout.js'
    script.async = true
    document.body.appendChild(script)

    const loadRestaurant = async () => {
      if (!restaurantId) return
      try {
        const res = await catalogApi.getRestaurantById(restaurantId)
        setRestaurantInfo({
          name: res?.name || 'QuickBite Restaurant',
          branch: res?.city ? `${res.city} Branch` : 'Selected Branch',
        })
      } catch {
        // Keep graceful fallback labels if restaurant details API is unavailable.
      }
    }
    loadRestaurant()

    return () => {
      if (document.body.contains(script)) {
        document.body.removeChild(script)
      }
    }
  }, [restaurantId])

  const handlePlaceOrder = async (orderData = checkoutData) => {
    if (!user || !restaurantId || items.length === 0) {
      showError('Cannot place empty order')
      return
    }

    if (!orderData?.addressId) {
      showError('Please select a delivery address')
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
      const priorityDeliveryFee = orderData?.deliveryOption === 'priority' ? 49 : 0
      const method = PAYMENT_METHODS.find(m => m.id === selectedMethod)

      const orderRes = await orderApi.createOrder({
        userId: user.id, restaurantId,
        selectedAddressId: orderData.addressId,
        specialInstructions: orderData.specialInstructions || '',
        deliveryInstructions: orderData.deliveryOption === 'priority' ? 'Priority Delivery' : 'Standard Delivery',
        deliveryFee: priorityDeliveryFee,
        paymentMethod: method.value // Pass actual payment method
      })

      // If COD, we are done
      if (selectedMethod === 'CashOnDelivery') {
        clearCart()
        showSuccess('Order placed successfully via Cash on Delivery!')
        navigate('/orders')
        return;
      }

      // --- RAZORPAY ONLINE PAYMENT FLOW ---
      if (orderRes?.orderId && window.Razorpay) {
        try {
          // 1. Get Razorpay Order ID from backend
          const rzpOrderRes = await api.post(`/gateway/payments/${orderRes.orderId}/create-razorpay-order`)
          const razorpayOrderId = rzpOrderRes.data.razorpayOrderId
          
          const options = {
              key: "rzp_test_Sk6FOwIhYGomaX", // Replace with your test key id if you haven't
              amount: Math.round(grandTotal * 100), // Ensures amount is a strict integer
              currency: "INR",
              name: "QuickBite",
              description: `Payment for Order ${orderRes.orderId.substring(0, 8)}`,
              order_id: razorpayOrderId,
              handler: async function (response) {
                  try {
                      // 2. Verify payment signature on backend
                      await api.post(`/gateway/payments/${orderRes.orderId}/verify`, {
                          razorpayOrderId: response.razorpay_order_id,
                          razorpayPaymentId: response.razorpay_payment_id,
                          razorpaySignature: response.razorpay_signature
                      })
                      
                      clearCart()
                      showSuccess('Payment successful and order placed!')
                      navigate('/orders')
                  } catch (err) {
                      console.error("Verification error:", err)
                      showError(err.response?.data?.message || 'Payment verification failed, please contact support.')
                      navigate('/orders')
                  }
              },
              prefill: {
                  name: user.name || "",
                  email: user.email || "",
                  contact: user.phone || ""
              },
              theme: {
                  color: "#1978e5" // Primary brand color
              }
          };

          const rzp = new window.Razorpay(options);
          
          rzp.on('payment.failed', function (response){
              showError(`Payment failed: ${response.error.description}`);
              // We do not clear cart here, so user can try again!
              navigate('/orders'); // Redirect to orders so they can see pending order, or stay on page.
          });
          
          rzp.open();
        } catch (paymentInitError) {
           console.error("Razorpay Init Error:", paymentInitError);
           showError('Failed to initialize payment gateway.');
           setIsPlacingOrder(false);
        }
      } else {
         showError('Payment gateway is not available right now.')
         setIsPlacingOrder(false);
      }
    } catch (error) {
      showError(error.response?.data?.message || error.message || 'Failed to place order')
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
  const deliveryFee = checkoutData.deliveryOption === 'priority' ? 49 : 0
  const taxesAndFees = 0
  const grandTotal = subtotal + deliveryFee + taxesAndFees
  const selectedPaymentMethod = PAYMENT_METHODS.find((m) => m.id === selectedMethod)

  return (
    <div className="min-h-screen bg-background pb-16">
      <div className="max-w-7xl mx-auto px-6 py-8 grid grid-cols-1 lg:grid-cols-12 gap-8">
        <div className="lg:col-span-8 space-y-6">
          <CheckoutForm onCheckoutDataChange={setCheckoutData} />

            {/* Payment Method Selector */}
            <div className="bg-surface-container-lowest rounded-xl border border-slate-100 shadow-sm overflow-hidden">
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
                    className={`w-full flex items-center gap-4 p-4 rounded-xl border-2 transition-all text-left ${selectedMethod === method.id ? method.activeBg : 'bg-surface-container-lowest border-slate-200 hover:border-slate-300'}`}
                  >
                    <span className={`material-symbols-outlined text-2xl ${selectedMethod === method.id ? method.color : 'text-slate-400'}`} style={{ fontVariationSettings: "'FILL' 1" }}>
                      {method.icon}
                    </span>
                    <div className="flex-1">
                      <div className="flex items-center justify-between">
                        <p className={`font-semibold text-sm ${selectedMethod === method.id ? 'text-on-surface' : 'text-on-surface-variant'}`}>
                          {method.label}
                        </p>
                        <div className={`w-5 h-5 rounded-full border-2 flex items-center justify-center transition-all ${selectedMethod === method.id ? 'border-primary bg-primary' : 'border-slate-300'}`}>
                          {selectedMethod === method.id && <span className="w-2 h-2 rounded-full bg-white" />}
                        </div>
                      </div>
                      <p className="text-xs text-on-surface-variant mt-0.5">{method.description}</p>
                      {method.branding}
                    </div>
                  </button>
                ))}
              </div>
            </div>
        </div>

        <div className="lg:col-span-4">
          <div className="sticky top-24 bg-surface-container-lowest rounded-xl border border-slate-100 shadow-sm p-6">
            <h2 className="text-title-lg font-semibold text-on-surface mb-6">Order Summary</h2>

            <div className="flex items-center gap-3 mb-6 pb-6 border-b border-outline-variant">
              <div className="w-12 h-12 rounded-full bg-slate-100 flex items-center justify-center text-xl">
                <span className="material-symbols-outlined text-slate-500">storefront</span>
              </div>
              <div>
                <h3 className="text-sm font-semibold text-on-surface">{restaurantInfo.name}</h3>
                <span className="text-xs text-on-surface-variant">{restaurantInfo.branch}</span>
              </div>
            </div>

            <div className="flex flex-col gap-4 mb-6 pb-6 border-b border-outline-variant">
                {items.map((item) => (
                  <div key={item.id} className="flex justify-between items-start gap-3">
                    <div className="flex items-start gap-3 min-w-0">
                      <div className="w-6 h-6 rounded-md bg-surface-container-high flex items-center justify-center text-xs font-semibold text-on-surface shrink-0">
                        {item.quantity}
                      </div>
                      <div className="min-w-0">
                        <p className="text-sm text-on-surface leading-tight truncate">{getItemDisplayName(item)}</p>
                      </div>
                    </div>
                    <span className="text-sm font-semibold text-on-surface shrink-0">
                      ₹{(item.quantity * Number(item.price)).toFixed(2)}
                    </span>
                  </div>
                ))}
              </div>

            <div className="space-y-2 mb-6">
                <div className="flex justify-between">
                <span className="text-sm text-on-surface-variant">Subtotal</span>
                <span className="text-sm text-on-surface">₹{subtotal.toFixed(2)}</span>
                </div>
                <div className="flex justify-between">
                <span className="text-sm text-on-surface-variant">Delivery Fee</span>
                <span className="text-sm text-on-surface">₹{deliveryFee.toFixed(2)}</span>
                </div>
              <div className="flex justify-between">
                <span className="text-sm text-on-surface-variant">Taxes &amp; Fees</span>
                <span className="text-sm text-on-surface">₹{taxesAndFees.toFixed(2)}</span>
              </div>
            </div>

            <div className="flex justify-between items-center mb-8 pt-4 border-t border-outline-variant">
              <span className="text-title-lg font-semibold text-on-surface">Total</span>
              <span className="text-price-lg font-bold text-primary">₹{grandTotal.toFixed(2)}</span>
            </div>

            <button
              type="button"
              onClick={() => handlePlaceOrder()}
              disabled={isPlacingOrder || !checkoutData.addressId}
              className="w-full py-4 bg-primary text-on-primary rounded-full text-sm font-semibold hover:bg-primary-container transition-all active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed shadow-[0_8px_16px_rgba(25,120,229,0.12)]"
            >
              {isPlacingOrder ? 'Placing Order...' : `Place Order (${selectedPaymentMethod?.id === 'CashOnDelivery' ? 'COD' : selectedPaymentMethod?.label})`}
            </button>

            <Link
              to="/cart"
              className="mt-3 block text-center text-sm text-primary font-medium hover:text-primary-container transition-colors"
            >
              Back to Cart
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}

