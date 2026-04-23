# Design Catalog - Quick Reference

All designs are located in: `../docs/stitch_react_application_design/`

## 📱 Customer Pages (8 designs)

| Design | HTML File | Screenshot | Components |
|--------|-----------|-----------|-----------|
| **Home Page** | `home_horizon_delivery/code.html` | `screen.png` | Hero search, categories, restaurants grid |
| **Login** | `login_horizon_delivery/code.html` | `screen.png` | Split layout, form, brand messaging |
| **Register** | `register_horizon_delivery/code.html` | `screen.png` | Multi-field form, validation |
| **Restaurant Details** | `restaurant_details_horizon_delivery/code.html` | `screen.png` | Menu, ratings, restaurant info |
| **Shopping Cart** | `your_cart_horizon_delivery/code.html` | `screen.png` | Cart items, quantity, total |
| **Checkout** | `checkout_horizon_delivery/code.html` | `screen.png` | Address, time slot, payment |
| **My Orders** | `my_orders_horizon_delivery/code.html` | `screen.png` | Order list, filters |
| **Order Tracking** | `track_order_horizon_delivery/code.html` | `screen.png` | Timeline, status, map |

## 🏪 Partner Pages (2 designs)

| Design | HTML File | Screenshot | Components |
|--------|-----------|-----------|-----------|
| **Dashboard** | `partner_dashboard_overview/code.html` | `screen.png` | Stats, charts, metrics |
| **Menu Management** | `partner_menu_management/code.html` | `screen.png` | Item list, CRUD forms |

## 👨‍💼 Admin Pages (4 designs)

| Design | HTML File | Screenshot | Components |
|--------|-----------|-----------|-----------|
| **Dashboard** | `admin_overview/code.html` | `screen.png` | KPI cards, analytics |
| **Orders** | `admin_orders/code.html` | `screen.png` | Order table, filters |
| **Restaurants** | `admin_restaurants/code.html` | `screen.png` | Partner list, approval |
| **Users** | `admin_users/code.html` | `screen.png` | User list, roles |

## 🚚 Delivery Agent Pages (2 designs)

| Design | HTML File | Screenshot | Components |
|--------|-----------|-----------|-----------|
| **Active Deliveries** | `agent_active_deliveries/code.html` | `screen.png` | Delivery list, maps |
| **Earnings History** | `agent_earnings_history/code.html` | `screen.png` | Earnings table, charts |

## 🎨 Design System

| Resource | File |
|----------|------|
| **Horizon UI Design** | `horizon_ui/DESIGN.md` |

Contains: Colors, typography, spacing, border radius, components, icons

---

## How to Use

### View a Design
```bash
# Open in browser
open ../docs/stitch_react_application_design/login_horizon_delivery/code.html
```

### Convert to React
1. Open `code.html` in browser
2. Use DevTools to inspect elements
3. Copy HTML structure
4. Convert to React JSX in `src/pages/` or `src/components/`
5. Add state with `useState`
6. Add API calls from `src/services/`
7. Connect to context providers

### Example: Login Page
```bash
# Source design
docs/stitch_react_application_design/login_horizon_delivery/code.html

# Create React component
web/src/pages/LoginPage.jsx
```

---

## Key Design Details

### Color Scheme (Horizon UI)
```
Primary: #1978e5          (Brand blue)
On Primary: #FFFFFF      (White text)
Surface: #FFFFFF         (Card backgrounds)
Background: #F4F7FE      (Page background)
Outline: #A3AED0         (Borders)
Error: #EE5D50           (Error color)
Success: #01B574         (Success color)
```

### Spacing Scale
```
4px   = unit
8px   = stack-sm
16px  = stack-md
24px  = stack-lg
```

### Border Radius
```
16px = DEFAULT (buttons, inputs, cards)
20px = lg (larger components)
24px = xl (extra large)
9999px = full (pills, badges)
```

### Font Family
**Inter** from Google Fonts
- Light: 300, Normal: 400, Medium: 500, Bold: 700

### Icons
**Material Symbols Outlined**
- From: https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined
- Examples: person, lock, shopping_cart, arrow_forward, star, etc.

---

## Responsive Breakpoints

```
Mobile:    < 768px
Tablet:    768px (md)
Desktop:   1024px (lg)
```

All designs are **mobile-first** and scale up.

---

## Implementation Tips

✅ **DO:**
- Follow the exact Tailwind classes from the designs
- Use the provided color palette
- Match spacing exactly (16px, 24px, etc.)
- Use Material Symbols icons
- Keep responsive patterns (hidden on mobile, visible on desktop)

❌ **DON'T:**
- Change colors (use what's in the design)
- Use different spacing values
- Create custom icons (use Material Symbols)
- Modify the layout structure significantly
- Add extra complexity

---

## React Component Template

```jsx
import React, { useState } from 'react'

export function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  
  return (
    <div className="bg-background text-on-background min-h-screen">
      {/* Copy structure from code.html */}
      {/* Replace static content with React state */}
      {/* Add event handlers for form submission */}
    </div>
  )
}
```

---

## Start Here

1. **Read**: `DESIGN_REFERENCE.md`
2. **Pick**: Simplest design (LoginPage)
3. **Open**: `login_horizon_delivery/code.html`
4. **Create**: `src/pages/LoginPage.jsx`
5. **Copy**: HTML structure
6. **Convert**: To React JSX
7. **Add**: State management
8. **Connect**: API services
9. **Test**: Against screenshot

---

**All designs are production-ready. Follow them exactly.**
