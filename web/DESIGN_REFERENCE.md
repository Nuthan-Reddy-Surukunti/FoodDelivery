# Design Reference & Implementation Guide

## Overview

All page designs are provided in HTML format with complete styling using **Tailwind CSS**, **Material Symbols Outlined** icons, and the **Horizon UI design system**.

**Location**: `/docs/stitch_react_application_design/`

## Complete Design Inventory

### Customer Pages (8 pages)
| Page | Folder | Purpose | Key Components |
|------|--------|---------|-----------------|
| **Home** | `home_horizon_delivery/` | Landing with categories & restaurants | Hero search, categories carousel, restaurant grid |
| **Login** | `login_horizon_delivery/` | Email/password authentication | Split layout, form, brand messaging |
| **Register** | `register_horizon_delivery/` | User signup form | Multi-field form, validation, role selection |
| **Restaurant Details** | `restaurant_details_horizon_delivery/` | Menu & restaurant info | Restaurant header, menu items, filters |
| **Shopping Cart** | `your_cart_horizon_delivery/` | Review cart items | Cart list, quantity controls, total |
| **Checkout** | `checkout_horizon_delivery/` | Multi-step payment flow | Stepper, address, time slot, payment (COD) |
| **My Orders** | `my_orders_horizon_delivery/` | Order history | Order list, filters, status badges |
| **Order Tracking** | `track_order_horizon_delivery/` | Real-time tracking | Status timeline, location map, updates |

### Partner/Restaurant Pages (2 pages)
| Page | Folder | Purpose | Key Components |
|------|--------|---------|-----------------|
| **Dashboard** | `partner_dashboard_overview/` | Partner overview | Stats cards, charts, metrics |
| **Menu Management** | `partner_menu_management/` | CRUD menu items | Item list, forms, image upload |

### Admin Pages (4 pages)
| Page | Folder | Purpose | Key Components |
|------|--------|---------|-----------------|
| **Dashboard** | `admin_overview/` | Admin metrics | KPI cards, charts, analytics |
| **Orders** | `admin_orders/` | Order management | Order table, filters, bulk actions |
| **Restaurants** | `admin_restaurants/` | Partner management | Restaurant list, approval, status |
| **Users** | `admin_users/` | User management | User list, roles, permissions |

### Delivery Agent Pages (2 pages)
| Page | Folder | Purpose | Key Components |
|------|--------|---------|-----------------|
| **Active Deliveries** | `agent_active_deliveries/` | Live delivery tasks | Delivery list, maps, actions |
| **Earnings History** | `agent_earnings_history/` | Payment history | Earnings table, charts, filters |

### Design System
| Resource | Folder | Contents |
|----------|--------|----------|
| **UI Design System** | `horizon_ui/` | `DESIGN.md` - Color palette, typography, components |

## Design System Details (Horizon UI)

### Colors
```javascript
{
  primary: '#1978e5',           // Main brand color
  'on-primary': '#FFFFFF',      // Text on primary
  'primary-container': '#0a73e0',
  secondary: '#A3AED0',         // Secondary actions
  tertiary: '#01B574',          // Success/positive
  error: '#EE5D50',             // Errors
  background: '#F4F7FE',        // Page background
  surface: '#FFFFFF',           // Card/panel background
  outline: '#A3AED0',           // Borders
}
```

### Typography
All using **Inter** font family:
- **display-xl**: 34px, 700 weight (largest headers)
- **headline-lg**: 24px, 700 weight (page titles)
- **headline-md**: 20px, 600 weight (section titles)
- **title-lg**: 18px, 600 weight (component titles)
- **body-lg**: 16px, 400 weight (large body text)
- **body-md**: 14px, 400 weight (default body text)
- **label-md**: 14px, 500 weight (form labels)
- **caption-sm**: 12px, 400 weight (small text)

### Spacing
- **unit**: 4px (smallest)
- **stack-sm**: 8px (small margins)
- **stack-md**: 16px (default margins)
- **stack-lg**: 24px (large margins)
- **gutter**: 20px (horizontal padding)
- **container-padding**: 24px (page padding)

### Border Radius
- **DEFAULT**: 16px (standard components)
- **lg**: 20px (larger components)
- **xl**: 24px (extra large)
- **full**: 9999px (pills/badges)

### Icons
**Material Symbols Outlined** from Google Fonts
- Examples: `person`, `lock`, `arrow_forward`, `shopping_cart`, `star`, `location`, etc.
- Include in HTML: `<link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&display=swap">`
- Usage: `<span class="material-symbols-outlined">icon_name</span>`

## How to Use These Designs

### Step 1: View the Design
```bash
# Each design folder contains:
# - code.html : Complete HTML with all styling
# - screen.png : Screenshot of the design
```

### Step 2: Extract Components
Open `code.html` in a browser to see the design, then:
1. Identify individual sections (header, form, list, etc.)
2. Note the Tailwind classes used
3. Extract the HTML structure

### Step 3: Convert to React Component
```jsx
// Example: Convert login_horizon_delivery/code.html to LoginPage.jsx

import React, { useState } from 'react'

export function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  
  const handleSubmit = (e) => {
    e.preventDefault()
    // Call login API from services
  }
  
  return (
    <div className="bg-background text-on-background min-h-screen flex flex-col md:flex-row">
      {/* Left: Brand messaging section */}
      <div className="hidden md:flex md:w-1/2 lg:w-[55%] relative h-screen bg-surface-container-highest">
        {/* Copy structure from code.html */}
      </div>
      
      {/* Right: Login form */}
      <div className="w-full md:w-1/2 lg:w-[45%] h-screen overflow-y-auto flex items-center justify-center">
        {/* Copy form structure from code.html */}
      </div>
    </div>
  )
}
```

### Step 4: Add Interactivity
Replace static HTML with React state and API calls:
```jsx
// Before (HTML):
<input placeholder="Enter your email" />

// After (React):
<input 
  value={email}
  onChange={(e) => setEmail(e.target.value)}
  placeholder="Enter your email"
  onBlur={() => validateEmail(email)}
/>
```

### Step 5: Connect API Services
```jsx
import { authService } from '@services/auth'

const handleLogin = async () => {
  try {
    const response = await authService.login(email, password)
    // Update auth context, navigate to home
  } catch (err) {
    setError(err.message)
  }
}
```

## Key Design Patterns Used

### 1. Form Patterns
**All forms follow this pattern:**
```html
<div class="space-y-unit">
  <label class="font-label-md">Label</label>
  <div class="relative">
    <span class="material-symbols-outlined">icon</span>
    <input class="rounded-[16px] border..." />
  </div>
  <p class="hidden font-caption-sm text-error">Error message</p>
</div>
```

### 2. Card/List Patterns
**Items in lists/grids use this pattern:**
```html
<div class="rounded-[16px] bg-surface shadow-ambient">
  <img class="rounded-[16px]" />
  <div class="p-4">
    <h3 class="font-title-lg">Title</h3>
    <p class="font-body-md text-on-surface-variant">Subtitle</p>
  </div>
</div>
```

### 3. Button Patterns
```html
<!-- Primary Button -->
<button class="bg-primary text-on-primary rounded-[16px] py-4">
  Action
</button>

<!-- Secondary Button -->
<button class="bg-surface-container text-on-surface rounded-[16px] border border-outline">
  Secondary
</button>
```

### 4. Responsive Patterns
- Mobile-first approach: `class="block md:hidden"` for mobile-only
- Desktop enhancements: `class="hidden md:flex"` for desktop-only
- Responsive text: `class="text-sm md:text-base lg:text-lg"`
- Responsive grid: `class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3"`

## Implementation Order (Recommended)

### Phase 1: Shared Components (Atoms & Molecules)
1. Buttons (primary, secondary, tertiary variants)
2. Input fields (text, email, password)
3. Form labels & validation messages
4. Badges & status indicators
5. Cards & containers
6. Icons wrapper

### Phase 2: Page Layouts (Organisms)
1. Header/Navigation
2. Footer
3. Forms (Login, Register)
4. Lists (Orders, Restaurants)
5. Grids (Menu items, Product cards)

### Phase 3: Full Pages
1. LoginPage - simplest
2. RegisterPage
3. HomePage - complex
4. RestaurantDetailPage
5. CartPage
6. CheckoutPage (includes stepper)
7. MyOrdersPage
8. OrderTrackingPage

### Phase 4: Partner/Admin Pages
1. PartnerDashboardPage
2. PartnerMenuManagementPage
3. AdminDashboardPage
4. AdminOrdersPage
5. AdminRestaurantsPage
6. AdminUsersPage

### Phase 5: Delivery Agent Pages
1. AgentActiveDeliveriesPage
2. AgentEarningsHistoryPage

## Quick Reference: Tailwind Classes from Designs

```javascript
// Colors
bg-primary              // #1978e5
text-on-primary        // #FFFFFF
bg-surface             // #FFFFFF
bg-surface-container-low  // #F4F7FE
text-on-surface-variant // #A3AED0
border-outline         // #A3AED0
bg-error-container     // Error background
text-error             // Error text color

// Spacing
p-4 / px-4 / py-4      // 16px padding
space-y-unit           // 4px gap between children
space-y-stack-sm       // 8px gap
space-y-stack-md       // 16px gap
space-y-stack-lg       // 24px gap

// Border Radius
rounded-[16px]         // 16px radius (standard)
rounded-[20px]         // 20px radius (large)
rounded-[24px]         // 24px radius (extra large)
rounded-full           // Pill shape

// Typography
font-display-xl        // 34px, 700 weight
font-headline-lg       // 24px, 700 weight
font-title-lg          // 18px, 600 weight
font-body-md           // 14px, 400 weight
font-label-md          // 14px, 500 weight
font-caption-sm        // 12px, 400 weight

// Effects
shadow-ambient         // Subtle shadow
backdrop-blur-md       // Blur effect
```

## Files Available in Each Design Folder

```
design-folder/
├── code.html          # Complete interactive HTML
└── screen.png         # Design screenshot
```

**How to use:**
1. Open `code.html` in a browser
2. Use browser DevTools to inspect elements
3. Note the Tailwind classes and structure
4. Copy structure into React component
5. Convert to JSX with state management

## Testing Against Designs

When converting to React:
1. ✅ Colors match Horizon UI palette
2. ✅ Spacing uses the defined scale (4px, 8px, 16px, 24px, 32px)
3. ✅ Border radius is 16px/20px/24px (not random values)
4. ✅ Typography uses correct sizes and weights
5. ✅ Icons use Material Symbols Outlined
6. ✅ Responsive breakpoints at md (768px) and lg (1024px)
7. ✅ Form patterns consistent (icon, label, input, error)
8. ✅ Button styles match (primary, secondary variants)
9. ✅ Shadows are subtle and consistent
10. ✅ Dark mode support with proper color mappings

## Notes

- All designs use **Tailwind CSS** inline configuration in the script tag
- Material Symbols must be loaded from Google Fonts CDN
- Images are from Unsplash/Google (replace with actual assets)
- Forms show validation error states (hidden by default)
- Dark mode is supported but not shown in screenshots

## Next Steps

1. Pick the **simplest page** (LoginPage)
2. Open `docs/stitch_react_application_design/login_horizon_delivery/code.html`
3. Create `src/pages/LoginPage.jsx`
4. Copy the HTML structure
5. Convert to React JSX
6. Add state management and API integration
7. Test against the design screenshot

**Remember**: Follow the design **exactly** - it's production-ready!
