import api from './api'

const catalogApi = {
  // ─── Restaurants ───────────────────────────────────────────────────────────

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

  async updateRestaurant(id, payload) {
    const response = await api.put(`/gateway/catalog/restaurants/${id}`, payload)
    return response.data
  },

  // ─── Categories ────────────────────────────────────────────────────────────

  async getCategories(restaurantId) {
    const response = await api.get(`/gateway/catalog/restaurants/${restaurantId}/categories`)
    return response.data
  },

  async createCategory(payload) {
    const response = await api.post('/gateway/catalog/categories', payload)
    return response.data
  },

  async updateCategory(id, payload) {
    const response = await api.put(`/gateway/catalog/categories/${id}`, { id, ...payload })
    return response.data
  },

  async deleteCategory(id) {
    const response = await api.delete(`/gateway/catalog/categories/${id}`)
    return response.data
  },

  // ─── Menu Items ────────────────────────────────────────────────────────────

  async getRestaurantMenu(restaurantId) {
    const response = await api.get(`/gateway/catalog/restaurants/${restaurantId}/menu`)
    return response.data
  },

  async createMenuItem(payload) {
    const response = await api.post('/gateway/catalog/menuitems', payload)
    return response.data
  },

  async updateMenuItem(id, payload) {
    const response = await api.put(`/gateway/catalog/menuitems/${id}`, { id, ...payload })
    return response.data
  },

  async deleteMenuItem(id) {
    const response = await api.delete(`/gateway/catalog/menuitems/${id}`)
    return response.data
  },

  // ─── Search ────────────────────────────────────────────────────────────────

  async searchRestaurantsByName(query) {
    const response = await api.get('/gateway/catalog/search/restaurantsByName', {
      params: { query },
    })
    return response.data
  },
}

export default catalogApi
