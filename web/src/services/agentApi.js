import api from './api'

const agentApi = {
  async getActiveDeliveries(agentId) {
    const response = await api.get('/gateway/orders', { 
      params: { deliveryAgentId: agentId, activeOnly: true } 
    })
    return response.data
  },

  async getEarnings(agentId) {
    const response = await api.get('/gateway/delivery-assignments/earnings', { 
      params: { agentId } 
    })
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
