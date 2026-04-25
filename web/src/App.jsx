import { Suspense, lazy } from 'react'
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import { CartProvider } from './context/CartContext'
import { ThemeProvider } from './context/ThemeContext'
import { NotificationProvider } from './context/NotificationContext'
import { Layout } from './components/organisms/Layout'
import { ProtectedRoute, PublicRoute } from './components/ProtectedRoute'

const HomePage = lazy(() => import('./pages/HomePage').then((m) => ({ default: m.HomePage })))
const ProfilePage = lazy(() => import('./pages/ProfilePage').then((m) => ({ default: m.ProfilePage })))
const ChangePasswordPage = lazy(() => import('./pages/ChangePasswordPage').then((m) => ({ default: m.ChangePasswordPage })))
const DeleteAccountPage = lazy(() => import('./pages/DeleteAccountPage').then((m) => ({ default: m.DeleteAccountPage })))
const LoginPage = lazy(() => import('./pages/LoginPage').then((m) => ({ default: m.LoginPage })))
const RegisterPage = lazy(() => import('./pages/RegisterPage').then((m) => ({ default: m.RegisterPage })))
const ForgotPasswordPage = lazy(() => import('./pages/ForgotPasswordPage').then((m) => ({ default: m.ForgotPasswordPage })))
const ResetPasswordPage = lazy(() => import('./pages/ResetPasswordPage').then((m) => ({ default: m.ResetPasswordPage })))
const VerifyEmailPage = lazy(() => import('./pages/VerifyEmailPage').then((m) => ({ default: m.VerifyEmailPage })))
const VerifyTwoFactorPage = lazy(() => import('./pages/VerifyTwoFactorPage').then((m) => ({ default: m.VerifyTwoFactorPage })))
const RestaurantDetailsPage = lazy(() => import('./pages/RestaurantDetailsPage').then((m) => ({ default: m.RestaurantDetailsPage })))
const ExploreRestaurantsPage = lazy(() => import('./pages/ExploreRestaurantsPage').then((m) => ({ default: m.ExploreRestaurantsPage })))
const CartPage = lazy(() => import('./pages/CartPage').then((m) => ({ default: m.CartPage })))
const CheckoutPage = lazy(() => import('./pages/CheckoutPage').then((m) => ({ default: m.CheckoutPage })))
const AddressManagementPage = lazy(() => import('./pages/AddressManagementPage').then((m) => ({ default: m.AddressManagementPage })))
const MyOrdersPage = lazy(() => import('./pages/MyOrdersPage').then((m) => ({ default: m.MyOrdersPage })))
const OrderTrackingPage = lazy(() => import('./pages/OrderTrackingPage').then((m) => ({ default: m.OrderTrackingPage })))
const PartnerDashboardPage = lazy(() => import('./pages/PartnerDashboardPage').then((m) => ({ default: m.PartnerDashboardPage })))
const MenuManagementPage = lazy(() => import('./pages/MenuManagementPage').then((m) => ({ default: m.MenuManagementPage })))
const OrderQueuePage = lazy(() => import('./pages/OrderQueuePage').then((m) => ({ default: m.OrderQueuePage })))
const AdminOverviewPage = lazy(() => import('./pages/AdminOverviewPage').then((m) => ({ default: m.AdminOverviewPage })))
const AdminOrdersPage = lazy(() => import('./pages/AdminOrdersPage').then((m) => ({ default: m.AdminOrdersPage })))
const AdminRestaurantsPage = lazy(() => import('./pages/AdminRestaurantsPage').then((m) => ({ default: m.AdminRestaurantsPage })))
const AdminUsersPage = lazy(() => import('./pages/AdminUsersPage').then((m) => ({ default: m.AdminUsersPage })))
const AgentActivePage = lazy(() => import('./pages/AgentActivePage').then((m) => ({ default: m.AgentActivePage })))
const AgentEarningsPage = lazy(() => import('./pages/AgentEarningsPage').then((m) => ({ default: m.AgentEarningsPage })))

export default function App() {
  return (
    <Router>
      <ThemeProvider>
        <AuthProvider>
          <CartProvider>
            <NotificationProvider>
              <Layout>
                <Suspense fallback={<PageLoader />}>
                  <Routes>
                {/* Auth Routes - Public (redirect to home if already logged in) */}
                <Route path="/login" element={<PublicRoute><LoginPage /></PublicRoute>} />
                <Route path="/register" element={<PublicRoute><RegisterPage /></PublicRoute>} />
                <Route path="/forgot-password" element={<PublicRoute><ForgotPasswordPage /></PublicRoute>} />
                <Route path="/reset-password" element={<PublicRoute><ResetPasswordPage /></PublicRoute>} />
                <Route path="/verify-email" element={<VerifyEmailPage />} />
                <Route path="/verify-2fa" element={<VerifyTwoFactorPage />} />
                
                {/* Customer Routes - Protected */}
                <Route path="/" element={<ProtectedRoute><HomePage /></ProtectedRoute>} />
                <Route path="/profile" element={<ProtectedRoute><ProfilePage /></ProtectedRoute>} />
                <Route path="/explore" element={<ProtectedRoute><ExploreRestaurantsPage /></ProtectedRoute>} />
                <Route path="/change-password" element={<ProtectedRoute><ChangePasswordPage /></ProtectedRoute>} />
                <Route path="/delete-account" element={<ProtectedRoute><DeleteAccountPage /></ProtectedRoute>} />
                <Route path="/restaurant/:id" element={<ProtectedRoute><RestaurantDetailsPage /></ProtectedRoute>} />
                <Route path="/cart" element={<ProtectedRoute><CartPage /></ProtectedRoute>} />
                <Route path="/checkout" element={<ProtectedRoute><CheckoutPage /></ProtectedRoute>} />
                <Route path="/addresses" element={<ProtectedRoute><AddressManagementPage /></ProtectedRoute>} />
                <Route path="/orders" element={<ProtectedRoute><MyOrdersPage /></ProtectedRoute>} />
                <Route path="/track/:orderId" element={<ProtectedRoute><OrderTrackingPage /></ProtectedRoute>} />

                {/* Partner Routes - Protected (RestaurantPartner only) */}
                <Route path="/partner/dashboard" element={<ProtectedRoute requiredRole="RestaurantPartner"><PartnerDashboardPage /></ProtectedRoute>} />
                <Route path="/partner/menu" element={<ProtectedRoute requiredRole="RestaurantPartner"><MenuManagementPage /></ProtectedRoute>} />
                <Route path="/partner/queue" element={<ProtectedRoute requiredRole="RestaurantPartner"><OrderQueuePage /></ProtectedRoute>} />

                {/* Admin Routes - Protected (Admin only) */}
                <Route path="/admin" element={<ProtectedRoute requiredRole="Admin"><AdminOverviewPage /></ProtectedRoute>} />
                <Route path="/admin/orders" element={<ProtectedRoute requiredRole="Admin"><AdminOrdersPage /></ProtectedRoute>} />
                <Route path="/admin/restaurants" element={<ProtectedRoute requiredRole="Admin"><AdminRestaurantsPage /></ProtectedRoute>} />
                <Route path="/admin/users" element={<ProtectedRoute requiredRole="Admin"><AdminUsersPage /></ProtectedRoute>} />

                {/* Agent Routes - Protected (DeliveryAgent only) */}
                <Route path="/agent/active" element={<ProtectedRoute requiredRole="DeliveryAgent"><AgentActivePage /></ProtectedRoute>} />
                <Route path="/agent/earnings" element={<ProtectedRoute requiredRole="DeliveryAgent"><AgentEarningsPage /></ProtectedRoute>} />

                {/* 404 */}
                <Route path="*" element={<NotFoundPage />} />
                  </Routes>
                </Suspense>
              </Layout>
            </NotificationProvider>
          </CartProvider>
        </AuthProvider>
      </ThemeProvider>
    </Router>
  )
}

// 404 Page
const NotFoundPage = () => (
  <div className="max-w-7xl mx-auto px-4 py-12 text-center">
    <h1 className="text-headline-lg font-bold mb-4">404 - Page Not Found</h1>
    <p className="text-body-md text-on-background/80">The page you're looking for doesn't exist.</p>
  </div>
)

const PageLoader = () => (
  <div className="mx-auto max-w-4xl px-4 py-16 text-center">
    <p className="text-sm text-on-background/70">Loading page...</p>
  </div>
)
