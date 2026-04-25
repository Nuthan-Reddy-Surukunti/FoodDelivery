import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import { CartProvider } from './context/CartContext'
import { ThemeProvider } from './context/ThemeContext'
import { Layout } from './components/organisms/Layout'
import { ProtectedRoute, PublicRoute } from './components/ProtectedRoute'

// Pages
import { HomePage } from './pages/HomePage'
import { ProfilePage } from './pages/ProfilePage'
import { ChangePasswordPage } from './pages/ChangePasswordPage'
import { DeleteAccountPage } from './pages/DeleteAccountPage'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { ForgotPasswordPage } from './pages/ForgotPasswordPage'
import { ResetPasswordPage } from './pages/ResetPasswordPage'
import { VerifyEmailPage } from './pages/VerifyEmailPage'
import { VerifyTwoFactorPage } from './pages/VerifyTwoFactorPage'
import { RestaurantDetailsPage } from './pages/RestaurantDetailsPage'
import { CartPage } from './pages/CartPage'
import { CheckoutPage } from './pages/CheckoutPage'
import { MyOrdersPage } from './pages/MyOrdersPage'
import { OrderTrackingPage } from './pages/OrderTrackingPage'
import { PartnerDashboardPage } from './pages/PartnerDashboardPage'
import { MenuManagementPage } from './pages/MenuManagementPage'
import { AdminOverviewPage } from './pages/AdminOverviewPage'
import { AdminOrdersPage } from './pages/AdminOrdersPage'
import { AdminRestaurantsPage } from './pages/AdminRestaurantsPage'
import { AdminUsersPage } from './pages/AdminUsersPage'
import { AgentActivePage } from './pages/AgentActivePage'
import { AgentEarningsPage } from './pages/AgentEarningsPage'

export default function App() {
  return (
    <Router>
      <ThemeProvider>
        <AuthProvider>
          <CartProvider>
            <Layout>
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
                <Route path="/change-password" element={<ProtectedRoute><ChangePasswordPage /></ProtectedRoute>} />
                <Route path="/delete-account" element={<ProtectedRoute><DeleteAccountPage /></ProtectedRoute>} />
                <Route path="/restaurant/:id" element={<ProtectedRoute><RestaurantDetailsPage /></ProtectedRoute>} />
                <Route path="/cart" element={<ProtectedRoute><CartPage /></ProtectedRoute>} />
                <Route path="/checkout" element={<ProtectedRoute><CheckoutPage /></ProtectedRoute>} />
                <Route path="/orders" element={<ProtectedRoute><MyOrdersPage /></ProtectedRoute>} />
                <Route path="/track/:orderId" element={<ProtectedRoute><OrderTrackingPage /></ProtectedRoute>} />

                {/* Partner Routes - Protected (RestaurantPartner only) */}
                <Route path="/partner/dashboard" element={<ProtectedRoute requiredRole="RestaurantPartner"><PartnerDashboardPage /></ProtectedRoute>} />
                <Route path="/partner/menu" element={<ProtectedRoute requiredRole="RestaurantPartner"><MenuManagementPage /></ProtectedRoute>} />

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
            </Layout>
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
