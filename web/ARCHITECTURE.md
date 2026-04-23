# Frontend Architecture Guide

## Project Structure

```
web/
├── src/
│   ├── components/
│   │   ├── atoms/              # Basic UI elements (buttons, inputs, badges)
│   │   ├── molecules/          # Composed components (search bar, menu card)
│   │   └── organisms/          # Complex components (header, footer, forms)
│   ├── pages/                  # Full page components
│   ├── hooks/                  # Custom React hooks
│   ├── context/                # React context providers
│   ├── services/               # API service layer
│   ├── utils/                  # Helper functions
│   ├── constants/              # App constants and enums
│   ├── styles/                 # Global styles
│   ├── App.jsx                 # Root component
│   └── main.jsx                # Entry point
├── public/                     # Static assets
├── package.json
├── vite.config.js
├── tailwind.config.js
└── .env.example
```

## Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Framework** | React 18 | UI library with hooks |
| **Language** | JavaScript (ES2020) | Dynamic typing for rapid development |
| **Routing** | React Router v6 | Client-side routing |
| **Styling** | Tailwind CSS | Utility-first CSS framework |
| **HTTP Client** | Axios | API requests with interceptors |
| **Build Tool** | Vite | Fast build & dev server |
| **Design System** | Horizon UI | Pre-configured color palette & tokens |

## Component Patterns

### Atomic Design Structure

**Atoms**: Smallest, most basic UI components
- Button, Input, Label, Badge, Icon
- No dependencies on other components (except styling)
- Fully reusable and flexible

**Molecules**: Simple components made of atoms
- SearchBar (Input + Button + Icon)
- MenuItem (Image + Text + Price)
- CartBadge (Badge + Counter)
- FormField (Label + Input + Error message)

**Organisms**: Complex components combining molecules
- Header (Logo + Nav + Cart + Profile)
- Footer (Links + Copyright)
- RestaurantList (Filter + Multiple MenuItem cards)
- CheckoutForm (Address field + Time slot + Payment)

**Pages**: Full-page components combining organisms
- HomePage, RestaurantDetailPage, CartPage, CheckoutPage, etc.

### Naming Conventions

```
components/
├── atoms/
│   ├── Button.jsx
│   ├── Input.jsx
│   ├── Badge.jsx
│   └── Icon.jsx
├── molecules/
│   ├── SearchBar.jsx
│   ├── MenuItem.jsx
│   └── CartBadge.jsx
└── organisms/
    ├── Header.jsx
    ├── Footer.jsx
    └── RestaurantCard.jsx
```

## State Management

### Context API (for global state)
- **AuthContext**: User authentication & JWT token
- **CartContext**: Shopping cart items & totals
- **ThemeContext**: Light/dark mode
- **NotificationContext**: Toasts & alerts

### Local Component State
Use `useState` for component-specific state:
- Form inputs, modals, dropdowns
- Loading/error states for API calls

### Custom Hooks
Create reusable hook logic:
- `useAuth()`: Authentication logic
- `useCart()`: Cart operations
- `useApi()`: API request wrapper with error handling
- `useFetch()`: Data fetching with loading/error states

## API Integration

### Service Layer Structure
```
services/
├── api.js          # Axios instance with interceptors
├── auth.js         # Auth endpoints
├── catalog.js      # Catalog/restaurant endpoints
├── order.js        # Order endpoints
└── admin.js        # Admin endpoints
```

### API Interceptors
- **Request**: Add JWT token to Authorization header
- **Response**: Handle 401 Unauthorized (refresh token / logout)
- **Error**: Transform error responses into consistent format

### Gateway Routes
```
GET /gateway/catalog/home              → Home data
GET /gateway/catalog/restaurants/nearby → Nearby restaurants
POST /gateway/auth/register            → User registration
POST /gateway/auth/login               → User login
GET /gateway/catalog/restaurants/{id}  → Restaurant details
GET /gateway/order/orders              → User's orders
POST /gateway/order/orders             → Create order
```

## Styling Strategy

### Tailwind CSS + Horizon UI Colors
All colors, spacing, and typography are pre-configured in `tailwind.config.js` from the Horizon UI design system.

### Usage Examples
```jsx
<button className="bg-primary text-on-primary px-4 py-2 rounded-lg hover:bg-primary-container">
  Submit
</button>

<div className="flex gap-stack-md px-container-padding">
  Content
</div>
```

### Custom Styling
- Use Tailwind classes first
- Global styles in `src/styles/globals.css`
- Component-scoped styles if needed (optional .module.css files)

## Routing Structure

```
/                          → Home page
/auth/login                → Login page
/auth/register             → Registration page
/customer/*                → Customer routes
  /customer/restaurants    → Explore restaurants
  /customer/restaurant/:id → Restaurant details
  /customer/cart           → Shopping cart
  /customer/checkout       → Checkout flow
  /customer/orders         → My orders
  /customer/order/:id      → Order tracking
/partner/*                 → Restaurant partner routes
/admin/*                   → Admin dashboard routes
/delivery/*                → Delivery agent routes (optional)
```

## Error Handling

### Global Error Handler
```tsx
// HTTP errors → Consistent error format
// Display via NotificationContext toast
```

### Form Validation
```tsx
// Client-side validation before submission
// Backend validation errors displayed inline
// Error messages from constants/messages.ts
```

## PRD Requirements Integration

✅ **Must implement in components:**
1. **Registration fields**: Name, Email, Phone, Password (PRD requirement)
2. **COD payment only** (PRD rule #7)
3. **One restaurant cart active at a time** (PRD rule #7)
4. **Loading skeletons** for data fetching
5. **Empty state** handling
6. **Checkout wizard/stepper** for address → time slot → payment → review
7. **Order tracking** with real-time status updates
8. **Mobile-first responsive design**

## Development Workflow

1. **Create service functions** in `src/services/` for API calls
2. **Design API services** with proper error handling
3. **Build atoms** in `src/components/atoms/` using existing design HTML
4. **Compose molecules** from atoms for reusability
5. **Assemble organisms** from molecules (Header, Footer, Forms)
6. **Create pages** using organisms with routing
7. **Setup routing** in App.jsx with React Router v6
8. **Implement context/hooks** for global state management
9. **Add error handling & form validation** with user feedback
10. **Style with Tailwind + Horizon UI colors** (pre-configured)

## Design Reference

**⭐ ALL PAGE DESIGNS ARE PROVIDED - FOLLOW EXACTLY**

- **[DESIGN_REFERENCE.md](./DESIGN_REFERENCE.md)** - Complete guide to all designs and how to use them
- Location: `/docs/stitch_react_application_design/`
- 16 complete page designs with HTML code and screenshots
- All using Horizon UI design system with Tailwind CSS

### Available Designs

**Customer Pages (8):**
- home_horizon_delivery/ - Landing page
- login_horizon_delivery/ - Login form
- register_horizon_delivery/ - Registration form
- restaurant_details_horizon_delivery/ - Menu & details
- your_cart_horizon_delivery/ - Shopping cart
- checkout_horizon_delivery/ - Multi-step checkout
- my_orders_horizon_delivery/ - Order history
- track_order_horizon_delivery/ - Order tracking

**Partner Pages (2):**
- partner_dashboard_overview/ - Dashboard
- partner_menu_management/ - Menu CRUD

**Admin Pages (4):**
- admin_overview/ - Metrics dashboard
- admin_orders/ - Order management
- admin_restaurants/ - Partner management
- admin_users/ - User management

**Delivery Agent Pages (2):**
- agent_active_deliveries/ - Live deliveries
- agent_earnings_history/ - Payment history

Each design contains:
- `code.html` - Complete working HTML with Tailwind CSS
- `screen.png` - Design screenshot

## Setup Instructions

```bash
# Install dependencies
npm install

# Copy environment variables
cp .env.example .env.local

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

## Available Commands

- `npm run dev` - Start development server (port 3000)
- `npm run build` - Build for production
- `npm run preview` - Preview production build locally
- `npm run lint` - Run ESLint
