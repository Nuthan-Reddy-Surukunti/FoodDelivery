# Frontend Implementation Checklist

## Phase 1: Project Setup ✅ (COMPLETED)

- [X] Create React project structure with Vite
- [X] Configure JavaScript with path aliases
- [X] Setup Tailwind CSS with Horizon UI tokens
- [X] Configure environment variables
- [X] Setup ESLint configuration
- [X] Create global styles
- [X] Remove TypeScript files (tsconfig.json, .ts/.tsx files)

## Phase 2: Constants & Enums (READY)

### To Do:

- [ ] Create constants for:

  - API endpoints
  - Error messages
  - Validation patterns (email, phone, password)
  - Success messages
  - Time slots for delivery
  - User roles
  - Order statuses
  - Payment methods
- [ ] Create enums/constants in JavaScript:

  - UserRole: { CUSTOMER, PARTNER, ADMIN, DELIVERY_AGENT }
  - OrderStatus: { PENDING, CONFIRMED, PREPARING, READY, PICKED_UP, DELIVERED, CANCELLED }
  - PaymentMethod: { COD }
  - CuisineType: { Pizza, Burgers, Sushi, etc. }

**Files to create:**

- `src/constants/enums.js` - All enum constants
- `src/constants/messages.js` - Error and success messages
- `src/constants/api.js` - API endpoint paths
- `src/constants/validation.js` - Regex patterns and validators

## Phase 3: API Service Layer (READY)

**Files to create:**

- `src/services/api.js` - Axios instance with interceptors
- `src/services/auth.js` - Auth endpoints
- `src/services/catalog.js` - Restaurant/menu endpoints
- `src/services/order.js` - Order endpoints
- `src/services/admin.js` - Admin endpoints (if needed initially)

**Key features:**

- [ ] Setup axios instance with base URL from .env
- [ ] Create request interceptor to add JWT token
- [ ] Create response interceptor for error handling
- [ ] Handle 401 Unauthorized (token refresh/logout)
- [ ] Create service methods for each API endpoint

## Phase 4: Context & Hooks (READY)

**Global Context:**

- [ ] AuthContext - Login, logout, token management
- [ ] CartContext - Add/remove items, calculate total
- [ ] NotificationContext - Toast notifications
- [ ] ThemeContext - Light/dark mode toggle

**Custom Hooks:**

- [ ] `useAuth()` - Get current user & auth functions
- [ ] `useCart()` - Get cart & cart operations
- [ ] `useApi(url, options)` - Fetch data with loading/error states
- [ ] `useNotification()` - Show toast messages
- [ ] `useLocalStorage(key)` - Persist cart locally

**Files to create:**

- `src/context/AuthContext.jsx`
- `src/context/CartContext.jsx`
- `src/context/NotificationContext.jsx`
- `src/context/ThemeContext.jsx`
- `src/hooks/useAuth.js`
- `src/hooks/useCart.js`
- `src/hooks/useApi.js`
- `src/hooks/useNotification.js`

## Phase 5: Atomic Components (READY)

### Atoms to Create:

- [ ] Button - Primary, secondary, outline variants
- [ ] Input - Text input with validation
- [ ] Label - Form labels
- [ ] Badge - Status/category badges
- [ ] Icon - Material Symbols wrapper
- [ ] Loader - Loading spinner
- [ ] Toast - Notification display
- [ ] Modal - Dialog/modal container
- [ ] Card - Generic card container

**Files:**

- `src/components/atoms/Button.jsx`
- `src/components/atoms/Input.jsx`
- `src/components/atoms/Badge.jsx`
- `src/components/atoms/Icon.jsx`
- `src/components/atoms/Loader.jsx`
- `src/components/atoms/Toast.jsx`
- `src/components/atoms/Modal.jsx`
- `src/components/atoms/Card.jsx`

## Phase 6: Molecule Components (READY)

### Molecules to Create:

- [ ] SearchBar - Input + search icon + button
- [ ] MenuItem - Image + name + price + rating
- [ ] FormField - Label + input + error message
- [ ] RestaurantCard - Image + name + rating + cuisine + delivery info
- [ ] CartItem - Item + quantity selector + remove button
- [ ] AddressField - Address input with validation
- [ ] TimeSlotSelector - Delivery time selection
- [ ] StepperIndicator - Multi-step form indicator
- [ ] Rating - Star rating display
- [ ] CuisineChip - Category chip

**Files:**

- `src/components/molecules/SearchBar.jsx`
- `src/components/molecules/MenuItem.jsx`
- `src/components/molecules/FormField.jsx`
- `src/components/molecules/RestaurantCard.jsx`
- `src/components/molecules/CartItem.jsx`
- `src/components/molecules/AddressField.jsx`
- `src/components/molecules/TimeSlotSelector.jsx`
- `src/components/molecules/StepperIndicator.jsx`
- etc.

## Phase 7: Organism Components (READY)

### Organisms to Create:

- [ ] Header - Logo + nav + cart + profile menu
- [ ] Footer - Links + copyright
- [ ] NavBar - Navigation with role-based menu
- [ ] RegistrationForm - Multi-field form with validation
- [ ] LoginForm - Email/password form
- [ ] RestaurantList - Filter + restaurant grid
- [ ] MenuSection - Items organized by category
- [ ] CartSummary - Item list + total + checkout button
- [ ] CheckoutForm - Stepper with address/slot/payment/review
- [ ] OrderTracker - Real-time order status timeline

**Files:**

- `src/components/organisms/Header.jsx`
- `src/components/organisms/Footer.jsx`
- `src/components/organisms/NavBar.jsx`
- `src/components/organisms/RegistrationForm.jsx`
- `src/components/organisms/LoginForm.jsx`
- `src/components/organisms/RestaurantList.jsx`
- `src/components/organisms/MenuSection.jsx`
- `src/components/organisms/CartSummary.jsx`
- `src/components/organisms/CheckoutForm.jsx`
- `src/components/organisms/OrderTracker.jsx`

## Phase 8: Pages (READY)

### Customer Pages:

- [ ] HomePage - Hero + categories + nearby restaurants
- [ ] LoginPage - Auth page with form
- [ ] RegisterPage - Signup with role selection
- [ ] ExploreRestaurantsPage - Full restaurant listing with filters
- [ ] RestaurantDetailPage - Menu + reviews + order info
- [ ] CartPage - Review items + quantity adjustment
- [ ] CheckoutPage - Stepper flow (address → time → payment → confirmation)
- [ ] MyOrdersPage - Past orders list
- [ ] OrderTrackingPage - Real-time order status

### Partner Pages:

- [ ] PartnerDashboard - Overview + stats
- [ ] MenuManagementPage - CRUD operations on menu items
- [ ] OrderQueuePage - Incoming orders to prepare

### Admin Pages:

- [ ] AdminDashboard - Key metrics
- [ ] RestaurantManagementPage - Approve/manage partners
- [ ] ReportsPage - Sales, cancellations, etc.

**Files:**

- `src/pages/customer/HomePage.jsx`
- `src/pages/customer/LoginPage.jsx`
- `src/pages/customer/RegisterPage.jsx`
- `src/pages/customer/RestaurantDetailPage.jsx`
- `src/pages/customer/CartPage.jsx`
- `src/pages/customer/CheckoutPage.jsx`
- `src/pages/customer/MyOrdersPage.jsx`
- `src/pages/customer/OrderTrackingPage.jsx`
- `src/pages/partner/DashboardPage.jsx`
- `src/pages/admin/DashboardPage.tsx`

## Phase 9: Routing Setup (READY)

- [ ] Create main App.jsx with React Router
- [ ] Setup route guards (PrivateRoute component)
- [ ] Role-based route protection
- [ ] Lazy load pages with React.lazy
- [ ] Setup 404 page
- [ ] Configure redirect logic

## Phase 10: Integration & Polish (READY)

- [ ] Connect all API calls
- [ ] Test API integration with backend
- [ ] Add error boundaries
- [ ] Add loading states
- [ ] Add empty states
- [ ] Performance optimization (memoization, code splitting)
- [ ] Accessibility audit
- [ ] Mobile responsiveness testing
- [ ] Dark mode testing

## Success Criteria

✅ All type definitions created
✅ API service layer fully functional
✅ Context & hooks implemented
✅ All atomic components built
✅ All molecule components built
✅ All organism components built
✅ All pages created
✅ Routing fully configured
✅ Integration with backend APIs complete
✅ Error handling & loading states
✅ Mobile-responsive design
✅ Accessibility (WCAG 2.1 AA)
✅ ESLint passes with 0 warnings
✅ JavaScript strict mode enabled
✅ No TypeScript errors (N/A - using JavaScript)

## Notes

- **Design System**: All colors/spacing from Horizon UI in `tailwind.config.js`
- **PRD Compliance**: Implementation follows PRD.md exactly
- **API Gateway**: All requests go through http://localhost:5000/gateway
- **JWT Auth**: Token stored in context, sent in Authorization header
- **Cart Rules**: One restaurant per cart (PRD rule #7)
- **Payment**: COD only (PRD rule #7)
