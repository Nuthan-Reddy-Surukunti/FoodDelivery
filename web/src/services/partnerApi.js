import api from './api'

const partnerApi = {
  async getIncomingOrders(restaurantId) {
    const response = await api.get('/gateway/orders', { 
      params: { restaurantId, activeOnly: true } 
    })
    return response.data
  },

  async updateOrderStatus(orderId, status) {
    const response = await api.put(`/gateway/orders/${orderId}/status`, { 
      targetStatus: status 
    })
    return response.data
  },

  async getDashboardStats(restaurantId) {
    const response = await api.get(`/gateway/partner/restaurants/${restaurantId}/stats`)
    return response.data
  }
}

export default partnerApi
