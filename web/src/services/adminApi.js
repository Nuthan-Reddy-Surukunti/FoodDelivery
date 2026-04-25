import api from './api'

const adminApi = {
	async getDashboardKpis() {
		const response = await api.get('/gateway/admin/dashboard')
		return response.data
	},

	async getSalesReport() {
		const response = await api.get('/gateway/admin/reports/all-sales')
		return response.data
	},

	async getUserAnalytics() {
		const response = await api.get('/gateway/admin/reports/all-users')
		return response.data
	},

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

	async getRestaurants(status) {
		const response = await api.get('/gateway/admin/restaurants', {
			params: status ? { status } : undefined,
		})
		return response.data
	},

	async approveRestaurant(restaurantId, comments = null) {
		const response = await api.post(`/gateway/admin/restaurants/${restaurantId}/approve`, {
			comments,
		})
		return response.data
	},

	async rejectRestaurant(restaurantId, rejectionReason) {
		const response = await api.post(`/gateway/admin/restaurants/${restaurantId}/reject`, {
			rejectionReason,
		})
		return response.data
	},
}

export default adminApi
