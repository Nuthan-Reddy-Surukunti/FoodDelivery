import api from './api'

const orderApi = {
	async getOrdersByUser(userId, activeOnly = false) {
		const response = await api.get('/gateway/orders', {
			params: { userId, activeOnly },
		})
		return response.data
	},

	async getOrderById(orderId) {
		const response = await api.get(`/gateway/orders/${orderId}`)
		return response.data
	},

	async cancelOrder(orderId) {
		const response = await api.post(`/gateway/orders/${orderId}/cancel`)
		return response.data
	},
}

export default orderApi
