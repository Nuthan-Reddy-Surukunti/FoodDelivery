# Frontend Implementation Checklist

## Phase 1: Project Setup ✅ (COMPLETED)
- [x] Create React project structure with Vite
- [x] Configure TypeScript with path aliases
- [x] Setup Tailwind CSS with Horizon UI tokens
- [x] Configure environment variables
- [x] Setup ESLint configuration
- [x] Create global styles

## Phase 2: Type Definitions & Constants (READY)

### To Do:
- [ ] Create TypeScript interfaces for:
  - User (name, email, phone, role)
  - Restaurant (id, name, address, rating, cuisine)
  - MenuItem (id, name, price, description, image)
  - Cart (items, restaurantId, total)
  - Order (id, status, items, total, deliveryAddress, trackingUpdates)
  - Address (street, city, zipcode, coordinates)
  - Auth (token, refreshToken, user)

- [ ] Create enums:
  - UserRole (CUSTOMER, PARTNER, ADMIN, DELIVERY_AGENT)
  - OrderStatus (PENDING, CONFIRMED, PREPARING, READY, PICKED_UP, DELIVERED, CANCELLED)
  - PaymentMethod (COD)
  - CuisineType (Pizza, Burgers, Sushi, etc.)

- [ ] Create constants:
  - API endpoints
  - Error messages
  - Validation patterns (email, phone, password)
  - Success messages
  - Time slots for delivery

**Files to create:**
- `src/types/index.ts`
- `src/types/auth.ts`
- `src/types/restaurant.ts`
- `src/types/order.ts`
- `src/types/user.ts`
- `src/constants/enums.ts`
- `src/constants/messages.ts`
- `src/constants/api.ts`

## Phase 3: API Service Layer (READY)

**Files to create:**
- `src/services/api.ts` - Axios instance with interceptors
- `src/services/auth.ts` - Auth endpoints
- `src/services/catalog.ts` - Restaurant/menu endpoints
- `src/services/order.ts` - Order endpoints
- `src/services/admin.ts` - Admin endpoints (if needed initially)

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
- `src/context/AuthContext.tsx`
- `src/context/CartContext.tsx`
- `src/context/NotificationContext.tsx`
- `src/context/ThemeContext.tsx`
- `src/hooks/useAuth.ts`
- `src/hooks/useCart.ts`
- `src/hooks/useApi.ts`
- `src/hooks/useNotification.ts`

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
- `src/components/atoms/Button.tsx`
- `src/components/atoms/Input.tsx`
- `src/components/atoms/Badge.tsx`
- `src/components/atoms/Icon.tsx`
- `src/components/atoms/Loader.tsx`
- `src/components/atoms/Toast.tsx`
- `src/components/atoms/Modal.tsx`
- `src/components/atoms/Card.tsx`

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
- `src/components/molecules/SearchBar.tsx`
- `src/components/molecules/MenuItem.tsx`
- `src/components/molecules/FormField.tsx`
- `src/components/molecules/RestaurantCard.tsx`
- `src/components/molecules/CartItem.tsx`
- `src/components/molecules/AddressField.tsx`
- `src/components/molecules/TimeSlotSelector.tsx`
- `src/components/molecules/StepperIndicator.tsx`
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
- `src/components/organisms/Header.tsx`
- `src/components/organisms/Footer.tsx`
- `src/components/organisms/NavBar.tsx`
- `src/components/organisms/RegistrationForm.tsx`
- `src/components/organisms/LoginForm.tsx`
- `src/components/organisms/RestaurantList.tsx`
- `src/components/organisms/MenuSection.tsx`
- `src/components/organisms/CartSummary.tsx`
- `src/components/organisms/CheckoutForm.tsx`
- `src/components/organisms/OrderTracker.tsx`

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
- `src/pages/customer/HomePage.tsx`
- `src/pages/customer/LoginPage.tsx`
- `src/pages/customer/RegisterPage.tsx`
- `src/pages/customer/RestaurantDetailPage.tsx`
- `src/pages/customer/CartPage.tsx`
- `src/pages/customer/CheckoutPage.tsx`
- `src/pages/customer/MyOrdersPage.tsx`
- `src/pages/customer/OrderTrackingPage.tsx`
- `src/pages/partner/DashboardPage.tsx`
- `src/pages/admin/DashboardPage.tsx`

## Phase 9: Routing Setup (READY)

- [ ] Create main App.tsx with React Router
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
✅ TypeScript strict mode passes

## Notes

- **Design System**: All colors/spacing from Horizon UI in `tailwind.config.js`
- **PRD Compliance**: Implementation follows PRD.md exactly
- **API Gateway**: All requests go through http://localhost:5000/gateway
- **JWT Auth**: Token stored in context, sent in Authorization header
- **Cart Rules**: One restaurant per cart (PRD rule #7)
- **Payment**: COD only (PRD rule #7)
