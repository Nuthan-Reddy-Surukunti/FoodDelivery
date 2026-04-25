import api from './api'

const adminApi = {
  // ─── Dashboard ──────────────────────────────────────────────────────────────
  async getDashboardKpis() {
    const response = await api.get('/gateway/admin/dashboard')
    return response.data
  },

  // ─── Reports ────────────────────────────────────────────────────────────────
  async getSalesReport() {
    const response = await api.get('/gateway/admin/reports/all-sales')
    return response.data
  },

  async getUserAnalytics() {
    const response = await api.get('/gateway/admin/reports/all-users')
    return response.data
  },

  async getRestaurantsReport() {
    const response = await api.get('/gateway/admin/reports/all-restaurants')
    return response.data
  },

  async getPartnersReport() {
    const response = await api.get('/gateway/admin/reports/all-partners')
    return response.data
  },

  // ─── Orders ─────────────────────────────────────────────────────────────────
  async getOrders(status) {
    const response = await api.get('/gateway/admin/orders', {
      params: status ? { status } : undefined,
    })
    return response.data
  },

  async updateOrderStatus(orderId, newStatus, reason = null, refundAmount = null) {
    const response = await api.put(`/gateway/admin/orders/${orderId}/status`, {
      newStatus,
      reason,
      refundAmount,
    })
    return response.data
  },

  // ─── Restaurants ────────────────────────────────────────────────────────────
  async getRestaurants(status) {
    const response = await api.get('/gateway/admin/restaurants', {
      params: status ? { status } : undefined,
    })
    return response.data
  },

  async getPendingApprovals() {
    const response = await api.get('/gateway/admin/restaurants/pending-approvals')
    return response.data
  },

  async approveRestaurant(restaurantId, comments = null) {
    const response = await api.post(`/gateway/admin/restaurants/${restaurantId}/approve`, { comments })
    return response.data
  },

  async rejectRestaurant(restaurantId, rejectionReason) {
    const response = await api.post(`/gateway/admin/restaurants/${restaurantId}/reject`, { rejectionReason })
    return response.data
  },

  async deleteRestaurant(restaurantId) {
    const response = await api.delete(`/gateway/admin/restaurants/${restaurantId}`)
    return response.data
  },
}

export default adminApi
