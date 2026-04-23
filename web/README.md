# CraveCloud Frontend - React SPA

A modern, mobile-first React single-page application for food delivery and restaurant aggregation, built with JavaScript, Tailwind CSS, and the Horizon UI design system.

## Quick Start

```bash
# Install dependencies
npm install

# Copy environment variables
cp .env.example .env.local

# Start development server
npm run dev
```

Development server will start at `http://localhost:3000`

## Project Status

🚀 **Project Setup Complete** - All configuration files and directory structure are ready.

Currently the project contains:
- ✅ JavaScript configuration with Vite and path aliases
- ✅ Tailwind CSS with Horizon UI design tokens
- ✅ Vite development server with API proxy
- ✅ ESLint configuration for JavaScript/React
- ✅ Global styles and CSS reset
- ✅ Directory structure for atomic design components
- ✅ Architecture documentation
- ✅ Implementation checklist
- ✅ Reference designs in `/docs/stitch_react_application_design/`

## Documentation

- [DESIGN_REFERENCE.md](./DESIGN_REFERENCE.md) - **START HERE** - Complete guide to all 16 page designs
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Project structure, component patterns, and development workflow
- [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md) - Phase-by-phase implementation tasks

## Key Features (To Be Implemented)

- 🔐 **User Authentication** - JWT-based auth with login/registration
- 🍽️ **Restaurant Discovery** - Browse restaurants with filtering and search
- 🛒 **Shopping Cart** - Add/remove items with one-restaurant-per-cart rule
- 📦 **Order Checkout** - Multi-step wizard (address → time slot → payment → review)
- 📍 **Order Tracking** - Real-time order status updates
- 👨‍💼 **Restaurant Partner Portal** - Menu management and order queue
- 🔧 **Admin Dashboard** - Restaurant management and reporting
- 🎨 **Responsive Design** - Mobile-first design with Horizon UI
- 🌙 **Dark Mode** - Light/dark theme support

## Tech Stack

| Layer | Technology |
|-------|-----------|
| UI Framework | React 18 |
| Language | TypeScript |
| Routing | React Router v6 |
| Styling | Tailwind CSS + Horizon UI |
| HTTP Client | Axios |
| Build Tool | Vite |
| State Management | React Context API |

## API Gateway

The frontend communicates with the backend through an API Gateway at `http://localhost:5000/gateway`

### Available Endpoints

```
POST /gateway/auth/register        - User registration
POST /gateway/auth/login           - User login
GET /gateway/catalog/home          - Home page data
GET /gateway/catalog/restaurants   - List restaurants
GET /gateway/catalog/restaurants/{id} - Restaurant details
GET /gateway/order/orders          - User's orders
POST /gateway/order/orders         - Create order
```

See [ARCHITECTURE.md](./ARCHITECTURE.md) for complete API documentation.

## Folder Structure

```
web/
├── src/
│   ├── components/
│   │   ├── atoms/              # Basic UI elements
│   │   ├── molecules/          # Composed components
│   │   └── organisms/          # Complex components
│   ├── pages/                  # Full page components
│   ├── hooks/                  # Custom React hooks
│   ├── context/                # Context providers
│   ├── services/               # API services
│   ├── utils/                  # Helper functions
│   ├── types/                  # TypeScript types
│   ├── constants/              # Constants & enums
│   ├── styles/                 # Global styles
│   ├── App.tsx
│   └── main.tsx
├── public/                     # Static assets
├── index.html
├── package.json
├── tsconfig.json
├── tailwind.config.js
├── vite.config.ts
└── ARCHITECTURE.md
```

## Available Scripts

```bash
npm run dev      # Start development server
npm run build    # Build for production
npm run preview  # Preview production build
npm run lint     # Run ESLint checks
```

## Environment Variables

Create `.env.local` file (copy from `.env.example`):

```env
VITE_API_BASE_URL=http://localhost:5000/gateway
VITE_API_TIMEOUT=30000
VITE_ENABLE_DELIVERY_AGENT=false
VITE_ENABLE_ANALYTICS=true
VITE_ENV=development
```

## Component Patterns

The project uses **Atomic Design** methodology:

- **Atoms**: Buttons, inputs, badges, icons
- **Molecules**: Search bars, menu items, form fields
- **Organisms**: Header, footer, forms, lists
- **Pages**: Full-page views combining organisms

See [ARCHITECTURE.md](./ARCHITECTURE.md) for detailed pattern guidelines.

## PRD Compliance

This frontend implementation follows the Food Delivery & Restaurant Aggregator PRD:

✅ Customer registration with name, email, phone, password
✅ JWT-based authentication
✅ One restaurant per cart (PRD rule #7)
✅ Cash on Delivery (COD) payment only (PRD rule #7)
✅ Mobile-first responsive design
✅ Loading & empty states
✅ Form validation with error messages
✅ Order tracking with real-time updates

## Next Steps

1. Install dependencies: `npm install`
2. Setup environment variables: `cp .env.example .env.local`
3. Follow [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md) for phase-by-phase development
4. Start with Phase 2: Type Definitions & Constants

## Contributing

Follow the component patterns and architecture guidelines in [ARCHITECTURE.md](./ARCHITECTURE.md).

## License

Part of the FoodDelivery project.
