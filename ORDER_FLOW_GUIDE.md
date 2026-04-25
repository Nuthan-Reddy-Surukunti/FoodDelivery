# 📦 Complete Order Flow Guide - QuickBite

This guide explains the complete order journey from adding items to cart, through payment, to final delivery.

---

## 🎯 Overview: 5 Major Stages

```
┌─────────────┐
│   SHOPPING  │  1. Browse menu & add items to cart
└──────┬──────┘
       ↓
┌─────────────┐
│  CHECKOUT   │  2. Select address, review total
└──────┬──────┘
       ↓
┌─────────────┐
│  PAYMENT    │  3. Complete payment (COD only)
└──────┬──────┘
       ↓
┌─────────────┐
│ PREPARATION │  4. Restaurant prepares order
└──────┬──────┘
       ↓
┌─────────────┐
│  DELIVERY   │  5. Agent picks up & delivers
└─────────────┘
```

---

# 📋 DETAILED STEP-BY-STEP FLOW

## Stage 1️⃣: SHOPPING (Build Cart)

### Step 1.1: Create/Get Cart
**Endpoint:** `GET /gateway/carts`
```
Request:
- userId: Your customer ID (from JWT)
- restaurantId: Which restaurant you're ordering from

Response: CartDto
{
  "cartId": "guid",
  "userId": "guid",
  "restaurantId": "guid",
  "status": "Active",
  "items": [],
  "totalAmount": 0,
  "currency": "INR"
}
```
**What happens:** 
- If no cart exists for this user+restaurant combination, a new one is created
- If a cart already exists, it's returned (you can add more items)

---

### Step 1.2: Add Items to Cart
**Endpoint:** `POST /gateway/carts/items`
```
Request: AddCartItemRequestDto
{
  "userId": "your-customer-id",
  "restaurantId": "restaurant-id",
  "menuItemId": "item-id",
  "quantity": 2,
  "customizationNotes": "Extra spicy, no onions"  // optional
}

Response: CartItemDto
{
  "cartItemId": "guid",
  "menuItemId": "guid",
  "quantity": 2,
  "priceSnapshot": 150,
  "customizationNotes": "Extra spicy, no onions",
  "subtotal": 300
}
```
**What happens:**
- Item is added to your cart
- Price is captured at this moment (used later for order)
- You can call this multiple times to add different items

---

### Step 1.3: Update Cart Item Quantity (Optional)
**Endpoint:** `PUT /gateway/carts/items/{cartItemId}`
```
Request: UpdateCartItemRequestDto
{
  "userId": "your-id",
  "cartItemId": "the-item-id",
  "newQuantity": 5  // change from 2 to 5
}

Response: Updated CartItemDto
```
**What happens:**
- Quantity is updated
- Subtotal is recalculated

---

### Step 1.4: Apply Coupon (Optional)
**Endpoint:** `POST /gateway/carts/apply-coupon`
```
Request: ApplyCouponRequestDto
{
  "userId": "your-id",
  "restaurantId": "restaurant-id",
  "couponCode": "SAVE50"
}

Response: CartDto (updated with discount applied)
{
  ...
  "appliedCouponCode": "SAVE50",
  "totalAmount": 475  // reduced from 500
}
```
**What happens:**
- Coupon is validated
- Discount is applied to cart total
- If invalid coupon, an error is returned

---

### Step 1.5: Remove Item from Cart (Optional)
**Endpoint:** `DELETE /gateway/carts/items/{cartItemId}`
```
Request: 
- userId
- restaurantId  
- cartItemId

Response: Updated CartDto
```
**What happens:**
- Item is removed
- Cart total is recalculated without this item

---

### Step 1.6: Clear Entire Cart (Optional)
**Endpoint:** `DELETE /gateway/carts`
```
Request:
- userId
- restaurantId

Response: Empty CartDto
```
**What happens:**
- All items removed
- Cart become empty (status still "Active")

---

## Stage 2️⃣: CHECKOUT (Prepare for Payment)

### Step 2.1: Save/Get Delivery Address
**Before placing order**, you must have at least one saved address.

#### 2.1a: List Your Saved Addresses
**Endpoint:** `GET /gateway/user/addresses`
```
Response: List of UserAddressDto
[
  {
    "addressId": "guid",
    "userId": "your-id",
    "addressLine1": "123 Main Street",
    "addressLine2": "Apt 4",
    "city": "Bangalore",
    "state": "Karnataka",
    "postalCode": "560001",
    "latitude": 12.9716,
    "longitude": 77.5946,
    "addressType": "Home",
    "isDefault": true,
    "createdAt": "2026-04-01T10:00:00Z",
    "updatedAt": "2026-04-01T10:00:00Z"
  },
  // ... more addresses
]
```

#### 2.1b: Create New Address (If Needed)
**Endpoint:** `POST /gateway/user/addresses`
```
Request: CreateUserAddressRequestDto
{
  "addressLine1": "123 Main Street",
  "addressLine2": "Apt 4",
  "city": "Bangalore",
  "state": "Karnataka",
  "postalCode": "560001",
  "latitude": 12.9716,
  "longitude": 77.5946,
  "addressType": "Home",  // Home, Work, or Other
  "isDefault": true       // Set as default delivery address
}

Response: UserAddressDto (created address)
```
**What it does:**
- Creates a new delivery address in your profile
- All future orders can reference this address
- Can save multiple addresses (home, work, etc.)

#### 2.1c: Update Address (Optional)
**Endpoint:** `PUT /gateway/user/addresses/{addressId}`
```
Request: UpdateUserAddressRequestDto (partial update)
{
  "addressLine1": "456 New Street",  // only change what you want
  "isDefault": true
}

Response: Updated UserAddressDto
```

#### 2.1d: Delete Address (Optional)
**Endpoint:** `DELETE /gateway/user/addresses/{addressId}`
```
Response: 204 No Content (success)
```
**Constraint:** Cannot delete if it's your only address

---

### Step 2.2: Review Cart & Calculate Totals
**Endpoint:** `POST /gateway/carts/calculate-totals`
```
Request:
{
  "userId": "your-id",
  "restaurantId": "restaurant-id",
  "taxPercentage": 5  // e.g., 5% GST
}

Response: PricingBreakdownDto
{
  "subtotal": 500,
  "taxAmount": 25,
  "discountAmount": 0,
  "deliveryFee": 50,
  "totalAmount": 575,
  "currency": "INR"
}
```
**What it does:**
- Calculates final amount you'll pay
- Includes: item costs + taxes + delivery fee - discounts

---

### Step 2.3: Validate Cart Items Still Available
**Endpoint:** `POST /gateway/carts/validate-items`
```
Request:
{
  "userId": "your-id",
  "restaurantId": "restaurant-id"
}

Response: ValidationResultDto
{
  "isValid": true,
  "invalidItems": [],  // any items that became unavailable
  "message": "All items are available"
}
```
**What it does:**
- Checks if items are still in stock
- Checks if prices changed
- Warns if any items became unavailable

---

## Stage 3️⃣: PAYMENT (Place Order & Pay)

### Step 3.1: Place Order
**Endpoint:** `POST /gateway/orders`
```
Request: PlaceOrderRequestDto
{
  "userId": "your-customer-id",
  "restaurantId": "restaurant-id",
  "selectedAddressId": "address-id",  // Must be one of your saved addresses
  "specialInstructions": "Ring bell twice"  // optional
}

Response: OrderDetailDto
{
  "orderId": "new-order-id",
  "userId": "your-id",
  "restaurantId": "restaurant-id",
  "orderStatus": "Paid",
  "deliveryAddress": {
    "street": "123 Main Street",
    "city": "Bangalore",
    "pincode": "560001",
    ...
  },
  "items": [
    {
      "orderItemId": "guid",
      "menuItemId": "guid",
      "quantity": 2,
      "unitPriceSnapshot": 150,
      "subtotal": 300
    }
  ],
  "payment": {
    "paymentId": "guid",
    "paymentMethod": "CashOnDelivery",
    "paymentStatus": "Success",
    "amount": 575,
    "currency": "INR",
    "processedAt": "2026-04-09T15:30:00Z"
  },
  "deliveryAssignment": {
    "deliveryAssignmentId": "guid",
    "deliveryAgentId": "agent-id",
    "assignedAt": "2026-04-09T15:30:10Z",
    "currentStatus": "PickupPending"
  },
  "subtotal": 500,
  "total": 575,
  "currency": "INR",
  "createdAt": "2026-04-09T15:30:00Z",
  "updatedAt": "2026-04-09T15:30:00Z"
}
```

**What happens automatically:**
1. ✅ Cart items are converted to Order items
2. ✅ Prices are locked (prices from when added to cart)
3. ✅ Delivery address is copied to Order
4. ✅ Payment is processed (COD = automatic success)
5. ✅ Delivery agent is automatically assigned
6. ✅ Restaurant is notified
7. ✅ Cart is cleared

**Order Status After This Step:** `Paid` (Order is confirmed!)

---

### Step 3.2: Process Payment (Already Done Above)
**Endpoint:** `POST /gateway/payments/{orderId}/process`
```
Request: ProcessPaymentRequestDto
{
  "paymentMethod": "CashOnDelivery",
  "amount": 575,
  "transactionId": "optional-ref"
}

Response: PaymentResponseDto
{
  "paymentId": "guid",
  "paymentStatus": "Success",
  "transactionId": "optional-ref",
  "amount": 575,
  "currency": "INR",
  "paymentMethod": "CashOnDelivery",
  "processedAt": "2026-04-09T15:30:00Z"
}
```
**Note:** This is already called automatically during `PlaceOrder`, but you can also call it separately if needed.

---

## Stage 4️⃣: PREPARATION (Restaurant Makes Order)

### Step 4.1: Check Order Status
**Endpoint:** `GET /gateway/orders/{orderId}`
```
Response: OrderDetailDto with current status

Possible Statuses:
- Paid → Order confirmed, sent to restaurant
- RestaurantAccepted → Restaurant confirmed they'll make it
- Preparing → Being cooked
- ReadyForPickup → Ready at restaurant counter
```

---

### Step 4.2: Get Order Timeline
**Endpoint:** `GET /gateway/orders/{orderId}/timeline`
```
Response: List of OrderTimelineEntryDto
[
  {
    "status": "Paid",
    "occurredAt": "2026-04-09T15:30:00Z",
    "label": "Order Placed"
  },
  {
    "status": "RestaurantAccepted",
    "occurredAt": "2026-04-09T15:32:00Z",
    "label": "Restaurant Confirmed"
  },
  {
    "status": "Preparing",
    "occurredAt": "2026-04-09T15:35:00Z",
    "label": "Order Being Prepared"
  },
  // ... more entries as status changes
]
```

---

## Stage 5️⃣: DELIVERY (Agent Picks Up & Delivers)

### Step 5.1: View Delivery Assignment
**Endpoint:** `GET /gateway/orders/{orderId}`
```
Response includes:
{
  ...
  "deliveryAssignment": {
    "deliveryAssignmentId": "guid",
    "deliveryAgentId": "agent-id",        // Agent's ID
    "assignedAt": "2026-04-09T15:30:10Z", // When assigned
    "pickedUpAt": null,                   // When agent picks up from restaurant
    "deliveredAt": null,                  // When delivered to you
    "currentStatus": "PickupPending"      // Current delivery status
  }
}
```

**Delivery Status Flow:**
```
PickupPending (waiting at restaurant)
    ↓
PickedUp (agent picked up from restaurant)
    ↓
InTransit (on the way to you)
    ↓
Delivered (arrived at your address)
```

---

### Step 5.2: View Assigned Deliveries (Agent's View)
**Endpoint:** `GET /gateway/orders` (add filter for agent)
```
Response: List of OrderDetailDto for all assigned orders

OR

Endpoint: `GET /gateway/delivery-assignments/my-deliveries`
```

---

### Step 5.3: Update Delivery Status (Agent Only)
**Endpoint:** `PUT /gateway/orders/{orderId}/status`
```
Request: UpdateOrderStatusRequestDto
{
  "orderId": "order-id",
  "targetStatus": "OutForDelivery"  // or "Delivered"
}

Response: Updated OrderDetailDto
```

---

# 🔄 Complete Example: End-to-End Order

```bash
# 1. SHOPPING
GET /gateway/carts?userId=customer1&restaurantId=rest1
# Response: Empty cart created

POST /gateway/carts/items
{
  "userId": "customer1",
  "restaurantId": "rest1",
  "menuItemId": "item1",
  "quantity": 2
}
# Response: Cart now has 2 items worth 300 INR

# 2. CHECKOUT
GET /gateway/user/addresses
# Response: List of saved addresses

# If no addresses:
POST /gateway/user/addresses
{
  "addressLine1": "123 Main St",
  "city": "Bangalore",
  "state": "Karnataka",
  "postalCode": "560001",
  "addressType": "Home",
  "isDefault": true
}
# Response: Address saved with ID "addr1"

POST /gateway/carts/calculate-totals?userId=customer1&restaurantId=rest1
# Response: Total = 350 INR (item 300 + tax 50)

# 3. PAYMENT
POST /gateway/orders
{
  "userId": "customer1",
  "restaurantId": "rest1",
  "selectedAddressId": "addr1",
  "specialInstructions": "No onions"
}
# Response: Order created with ID "order1"
#          Payment: Success (COD)
#          Status: Paid
#          Delivery Agent Assigned: agent1

# 4. PREPARATION (Automatic - Restaurant accepts)
GET /gateway/orders/order1
# Response: Status updated to "RestaurantAccepted"

# 5. DELIVERY (Automatic - Agent picks up)
GET /gateway/orders/order1
# Response: Delivery Status = "PickedUp"

# Final
GET /gateway/orders/order1
# Response: Delivery Status = "Delivered"
#          Order Complete! ✅
```

---

# 📊 Status State Machine

## Order Status Flow
```
DraftCart
    ↓
CheckoutStarted
    ↓
PaymentPending
    ↓
Paid ✅ (Payment successful)
    ├─→ RestaurantAccepted (Restaurant confirmed)
    │       ↓
    │   Preparing (Being cooked)
    │       ↓
    │   ReadyForPickup (At restaurant counter)
    │       ↓
    │   PickedUp (Agent picked up)
    │       ↓
    │   OutForDelivery (In transit to you)
    │       ↓
    │   Delivered ✅ (Completed!)
    │
    ├─→ PaymentFailed ❌ (Payment didn't go through)
    │
    ├─→ RestaurantRejected ❌ (Restaurant declined)
    │
    └─→ CancelRequestedByCustomer (You cancelled)
            ↓
        RefundInitiated
            ↓
        Refunded ❌ (Marked as cancelled)
```

---

# 🎯 Key Endpoints Quick Reference

| What You Want | HTTP Method | Endpoint | Role |
|---|---|---|---|
| Browse menu | GET | `/gateway/catalog/restaurants/{id}/menu` | Customer |
| Get/Create cart | GET | `/gateway/carts` | Customer |
| Add to cart | POST | `/gateway/carts/items` | Customer |
| Update cart item | PUT | `/gateway/carts/items/{itemId}` | Customer |
| Remove from cart | DELETE | `/gateway/carts/items/{itemId}` | Customer |
| Save address | POST | `/gateway/user/addresses` | Customer |
| Get addresses | GET | `/gateway/user/addresses` | Customer |
| Calculate totals | POST | `/gateway/carts/calculate-totals` | Customer |
| Place order | POST | `/gateway/orders` | Customer |
| Check order status | GET | `/gateway/orders/{orderId}` | All |
| Get order timeline | GET | `/gateway/orders/{orderId}/timeline` | All |
| View my orders | GET | `/gateway/orders?userId={...}` | Customer |
| Cancel order | POST | `/gateway/orders/{orderId}/cancel` | Customer |
| Update order (payment, status) | PUT | `/gateway/orders/{orderId}/status` | Authorized |
| Process payment | POST | `/gateway/payments/{orderId}/process` | (Auto with order) |

---

# ⚠️ Important Rules & Constraints

1. **Cart & Order Separation**
   - Cart holds temporary items
   - Order is created only after payment
   - After order placed, cart is cleared

2. **Address Required**
   - Must have at least 1 saved address before placing order
   - Order locks address values (even if address updated later)

3. **Payment Method**
   - Only **Cash on Delivery (COD)** supported
   - Payment is processed automatically when order placed
   - No refunds for COD (handled manually)

4. **Pricing**
   - Cart item prices captured when added
   - Order prices locked when placed (prices at order time)
   - If menu prices change later, doesn't affect existing orders

5. **Delivery Assignment**
   - Automatic: After payment success, agent is assigned
   - If no agents available: Order status = "Paid" but not assigned
   - Admin can manually assign later

6. **Status Transitions**
   - Cannot go backwards (e.g., from "Delivered" to "Preparing")
   - Some statuses can only be set by specific roles
   - Timeline is immutable (each status change is recorded)

---

# 🔐 Authorization Rules

```
Endpoint                          | Allowed Roles
─────────────────────────────────────────────────────
POST /orders (Place)              | Customer only
GET /orders/{id}                  | Customer, RestaurantPartner, DeliveryAgent, Admin
PUT /orders/{id}/status           | RestaurantPartner, DeliveryAgent, Admin
POST /orders/{id}/cancel          | Customer, Admin
GET /carts                        | Customer only
POST /carts/items                 | Customer only
GET /user/addresses               | Customer only
POST /user/addresses              | Customer only
PUT /user/addresses/{id}          | Customer only
DELETE /user/addresses/{id}       | Customer only
POST /payments/{id}/process       | DeliveryAgent (for COD), Admin
```

---

# ❓ Common Questions

**Q: Can I change my order after paying?**
A: No, once paid, the order is locked. You can cancel it, but cannot modify items.

**Q: What if the restaurant is closed when order placed?**
A: Currently, there's no restaurant hours validation. Admin handles manually.

**Q: What if delivery agent is not online?**
A: Order stays in "Paid" status. Admin can manually assign later.

**Q: Can I modify address after order placed?**
A: No, address is locked in the Order. Only the saved address can be modified.

**Q: How long does delivery take?**
A: No estimated delivery time in this version. It's manual after agent picks up.

**Q: Can I reorder from history?**
A: Yes! `POST /gateway/orders/{orderId}/reorder` creates a new order with same items.

---

# 📝 Summary

**The 3-Step Order Process:**
1. **Shop:** Add items to cart → Select address
2. **Pay:** Place order → Payment processed (COD automatic)
3. **Deliver:** Restaurant prepares → Agent picks up → Delivered to you

**Always remember:**
-🛒 Cart is temporary (cleared after order)
- 📦 Order is final (cannot modify items)
- 💳 Only COD supported
- 🏠 Must have saved address
- 🚗 Agent assigned automatically after payment
