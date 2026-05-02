import api from './api'

const profileApi = {
  async getProfileStats() {
    const response = await api.get('/gateway/profile/stats')
    return response.data
  }
}

export default profileApi
