import api from './api'

const catalogApi = {
	async getRestaurants() {
		const response = await api.get('/gateway/catalog/restaurants')
		return response.data
	},

	async getMyRestaurant() {
		const response = await api.get('/gateway/catalog/restaurants/my')
		return response.data
	},

	async getRestaurantById(restaurantId) {
		const response = await api.get(`/gateway/catalog/restaurants/${restaurantId}`)
		return response.data
	},

	async createRestaurant(payload) {
		const response = await api.post('/gateway/catalog/restaurants', payload)
		return response.data
	},

	async getRestaurantMenu(restaurantId) {
		const response = await api.get(`/gateway/catalog/restaurants/${restaurantId}/menu`)
		return response.data
	},

	async searchRestaurantsByName(query) {
		const response = await api.get('/gateway/catalog/search/restaurantsByName', {
			params: { query },
		})
		return response.data
	},
}

export default catalogApi
