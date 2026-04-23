import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import { CartProvider } from './context/CartContext'
import { ThemeProvider } from './context/ThemeContext'
import { Layout } from './components/organisms/Layout'

// Pages
import { HomePage } from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
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
                {/* Customer Routes */}
                <Route path="/" element={<HomePage />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route path="/restaurant/:id" element={<RestaurantDetailsPage />} />
                <Route path="/cart" element={<CartPage />} />
                <Route path="/checkout" element={<CheckoutPage />} />
                <Route path="/orders" element={<MyOrdersPage />} />
                <Route path="/track/:orderId" element={<OrderTrackingPage />} />

                {/* Partner Routes */}
                <Route path="/partner/dashboard" element={<PartnerDashboardPage />} />
                <Route path="/partner/menu" element={<MenuManagementPage />} />

                {/* Admin Routes */}
                <Route path="/admin" element={<AdminOverviewPage />} />
                <Route path="/admin/orders" element={<AdminOrdersPage />} />
                <Route path="/admin/restaurants" element={<AdminRestaurantsPage />} />
                <Route path="/admin/users" element={<AdminUsersPage />} />

                {/* Agent Routes */}
                <Route path="/agent/active" element={<AgentActivePage />} />
                <Route path="/agent/earnings" element={<AgentEarningsPage />} />

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
