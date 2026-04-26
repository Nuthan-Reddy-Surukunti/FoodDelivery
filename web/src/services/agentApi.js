import api from './api'

const agentApi = {
  async getActiveDeliveries() {
    const response = await api.get('/gateway/orders/deliveries/assigned')
    return response.data
  },

  async getEarnings() {
    const response = await api.get('/gateway/orders/deliveries/earnings')
    return response.data
  },

  async updateDeliveryStatus(orderId, status) {
    const response = await api.put(`/gateway/orders/${orderId}/status`, {
      targetStatus: status
    })
    return response.data
  }
}

export default agentApi
