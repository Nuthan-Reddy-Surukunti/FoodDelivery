import api from './api'

const partnerApi = {
  async getIncomingOrders() {
    // Uses the restaurant-partner queue endpoint (filtered to their restaurant by JWT)
    const response = await api.get('/gateway/orders/queue')
    return response.data
  },

  async updateOrderStatus(orderId, status) {
    const response = await api.put(`/gateway/orders/${orderId}/status`, {
      targetStatus: status
    })
    return response.data
  },

  async getDashboardStats(restaurantId) {
    const response = await api.get(`/gateway/orders/partner/${restaurantId}/stats`)
    return response.data
  }
}

export default partnerApi
