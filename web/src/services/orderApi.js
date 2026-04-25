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

	// Address CRUD
	async getAddresses() {
		const response = await api.get('/gateway/user/addresses')
		return response.data
	},

	async getAddressById(addressId) {
		const response = await api.get(`/gateway/user/addresses/${addressId}`)
		return response.data
	},

	async createAddress(payload) {
		const response = await api.post('/gateway/user/addresses', payload)
		return response.data
	},

	async updateAddress(addressId, payload) {
		const response = await api.put(`/gateway/user/addresses/${addressId}`, payload)
		return response.data
	},

	async deleteAddress(addressId) {
		const response = await api.delete(`/gateway/user/addresses/${addressId}`)
		return response.data
	},
}

export default orderApi
