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
│   ├── types/                  # TypeScript type definitions
│   ├── constants/              # App constants and enums
│   ├── styles/                 # Global styles
│   ├── App.tsx                 # Root component
│   └── main.tsx                # Entry point
├── public/                     # Static assets
├── package.json
├── tsconfig.json
├── tailwind.config.js
├── vite.config.ts
└── .env.example
```

## Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Framework** | React 18 | UI library |
| **Language** | TypeScript | Type safety |
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
│   ├── Button.tsx
│   ├── Input.tsx
│   ├── Badge.tsx
│   └── Icon.tsx
├── molecules/
│   ├── SearchBar.tsx
│   ├── MenuItem.tsx
│   └── CartBadge.tsx
└── organisms/
    ├── Header.tsx
    ├── Footer.tsx
    └── RestaurantCard.tsx
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
├── api.ts          # Axios instance with interceptors
├── auth.ts         # Auth endpoints
├── catalog.ts      # Catalog/restaurant endpoints
├── order.ts        # Order endpoints
└── admin.ts        # Admin endpoints
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
```tsx
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

1. **Create type definitions** in `src/types/`
2. **Design API services** in `src/services/`
3. **Build atoms** in `src/components/atoms/`
4. **Compose molecules** from atoms
5. **Assemble organisms** from molecules
6. **Create pages** using organisms
7. **Setup routing** in App.tsx
8. **Implement context/hooks** for state management
9. **Add error handling & validation**
10. **Style with Tailwind + Horizon UI colors**

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
