import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { SearchBar } from '../components/molecules/SearchBar'
import { CuisineChip } from '../components/molecules/CuisineChip'
import { RestaurantList } from '../components/organisms/RestaurantList'
import catalogApi from '../services/catalogApi'

export const ExploreRestaurantsPage = () => {
  const navigate = useNavigate()
  const [query, setQuery] = useState('')
  const [activeCuisine, setActiveCuisine] = useState('All')
  const [restaurants, setRestaurants] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const fetchRestaurants = async () => {
      try {
        const data = await catalogApi.getRestaurants()
        const raw = Array.isArray(data) ? data : (data?.items || data?.data || [])
        const mapped = raw.map(item => ({
          id: item.id,
          name: item.name,
          cuisine: item.cuisineTypeName || item.cuisineType || item.cuisine || 'Unknown',
          rating: Number(item.averageRating || item.rating || 0),
          deliveryTime: item.estimatedDeliveryTime ? `${item.estimatedDeliveryTime} mins` : 'N/A',
        }))
        setRestaurants(mapped)
      } catch (err) {
        console.error(err)
      } finally {
        setLoading(false)
      }
    }
    fetchRestaurants()
  }, [])

  const filtered = restaurants.filter(item => {
    const cuisineMatch = activeCuisine === 'All' || item.cuisine === activeCuisine
    const queryMatch = !query || item.name.toLowerCase().includes(query.toLowerCase())
    return cuisineMatch && queryMatch
  })

  const cuisines = ['All', ...new Set(restaurants.map(r => r.cuisine).filter(Boolean))]

  return (
    <div className="mx-auto max-w-7xl px-4 py-8">
      <h1 className="text-3xl font-bold mb-6">Explore All Restaurants</h1>
      
      <div className="mb-6 max-w-md">
        <SearchBar value={query} onChange={(e) => setQuery(e.target.value)} onSearch={() => {}} />
      </div>

      <div className="mb-8 flex flex-wrap gap-2">
        {cuisines.map(c => (
          <CuisineChip key={c} label={c} active={activeCuisine === c} onClick={() => setActiveCuisine(c)} />
        ))}
      </div>

      {loading ? (
        <p>Loading...</p>
      ) : (
        <RestaurantList restaurants={filtered} onOpenRestaurant={(r) => navigate(`/restaurant/${r.id}`)} />
      )}
    </div>
  )
}
