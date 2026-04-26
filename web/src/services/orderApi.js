import api from './api'

const orderApi = {
  async getOrdersByUser(userId, activeOnly = false, extraParams = {}) {
    const response = await api.get('/gateway/orders', {
      params: { userId, activeOnly, ...extraParams },
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

  async createOrder(payload) {
    const response = await api.post('/gateway/orders', payload)
    return response.data
  },

  async updateOrderStatus(orderId, targetStatus) {
    const response = await api.put(`/gateway/orders/${orderId}/status`, { orderId, targetStatus })
    return response.data
  },

  async reorderFromHistory(orderId) {
    const response = await api.post(`/gateway/orders/${orderId}/reorder`)
    return response.data
  },

  async calculateTotals(payload) {
    const response = await api.post('/gateway/carts/calculate-totals', payload)
    return response.data
  },

  async applyCoupon(payload) {
    const response = await api.post('/gateway/carts/apply-coupon', payload)
    return response.data
  },

	// Cart Operations
	async clearCart(userId, restaurantId) {
		const response = await api.delete('/gateway/carts', { params: { userId, restaurantId } })
		return response.data
	},

	async addCartItem(payload) {
		const response = await api.post('/gateway/carts/items', payload)
		return response.data
	},

	// Address CRUD
	async getAddresses() {
		const response = await api.get('/gateway/user/addresses')
		return (response.data || []).map(mapAddressToFrontend)
	},

	async getAddressById(addressId) {
		const response = await api.get(`/gateway/user/addresses/${addressId}`)
		return mapAddressToFrontend(response.data)
	},

	async createAddress(payload) {
		const response = await api.post('/gateway/user/addresses', mapAddressToBackend(payload))
		return mapAddressToFrontend(response.data)
	},

	async updateAddress(addressId, payload) {
		const response = await api.put(`/gateway/user/addresses/${addressId}`, mapAddressToBackend(payload))
		return mapAddressToFrontend(response.data)
	},

	async deleteAddress(addressId) {
		const response = await api.delete(`/gateway/user/addresses/${addressId}`)
		return response.data
	},
}

function mapAddressToFrontend(data) {
	if (!data) return null;
	const labelMap = { 1: 'Home', 2: 'Work', 3: 'Other' };
	return {
		id: data.addressId || data.id,
		label: labelMap[data.addressType] || 'Other',
		street: data.addressLine1 || '',
		city: data.city || '',
		state: data.state || '',
		pinCode: data.postalCode || '',
		isDefault: data.isDefault || false,
	};
}

function mapAddressToBackend(data) {
	const labelLower = (data.label || '').toLowerCase();
	let addressType = 3; // Other
	if (labelLower.includes('home')) addressType = 1;
	else if (labelLower.includes('work') || labelLower.includes('office')) addressType = 2;

	return {
		AddressLine1: data.street,
		City: data.city,
		State: data.state,
		PostalCode: data.pinCode,
		AddressType: addressType,
		IsDefault: data.isDefault
	};
}

export default orderApi
