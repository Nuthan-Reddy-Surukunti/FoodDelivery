import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { SearchBar } from '../components/molecules/SearchBar'
import { CuisineChip } from '../components/molecules/CuisineChip'
import { RestaurantList } from '../components/organisms/RestaurantList'
import catalogApi from '../services/catalogApi'

const cuisines = ['All']

const normalizeRestaurants = (payload) => {
  const raw = Array.isArray(payload) ? payload : payload?.items || payload?.data || []
  return raw.map((item) => ({
    id: item.id,
    name: item.name,
    cuisine: item.cuisineTypeName || item.cuisineType || item.cuisine || 'Unknown',
    rating: Number(item.averageRating || item.rating || 0),
    deliveryTime: item.estimatedDeliveryTime ? `${item.estimatedDeliveryTime} mins` : 'N/A',
  }))
}

export const HomePage = () => {
  const navigate = useNavigate()
  const [query, setQuery] = useState('')
  const [activeCuisine, setActiveCuisine] = useState('All')
  const [restaurantsData, setRestaurantsData] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true

    const loadRestaurants = async () => {
      setLoading(true)
      setError('')
      try {
        const response = await catalogApi.getRestaurants()
        if (!active) return
        setRestaurantsData(normalizeRestaurants(response))
      } catch (err) {
        if (!active) return
        setError(err.response?.data?.message || err.message || 'Failed to load restaurants')
      } finally {
        if (active) {
          setLoading(false)
        }
      }
    }

    loadRestaurants()
    return () => {
      active = false
    }
  }, [])

  const restaurants = useMemo(() => {
    return restaurantsData.filter((item) => {
      const cuisineMatch = activeCuisine === 'All' || item.cuisine === activeCuisine
      const queryMatch = !query || item.name.toLowerCase().includes(query.toLowerCase()) || item.cuisine.toLowerCase().includes(query.toLowerCase())
      return cuisineMatch && queryMatch
    })
  }, [restaurantsData, query, activeCuisine])

  const availableCuisines = useMemo(() => {
    const distinct = Array.from(new Set(restaurantsData.map((item) => item.cuisine).filter(Boolean)))
    return [...cuisines, ...distinct]
  }, [restaurantsData])

  return (
    <div className="mx-auto max-w-7xl px-4 py-8">
      <section className="mb-8 rounded-3xl bg-primary p-6 text-on-primary">
        <h1 className="text-3xl font-bold">Hungry? Find your next meal fast.</h1>
        <p className="mt-2 text-sm opacity-90">Explore nearby restaurants, pick your favorites, and checkout in minutes.</p>
      </section>

      <section className="mb-5">
        <SearchBar value={query} onChange={(e) => setQuery(e.target.value)} onSearch={() => {}} />
      </section>

      <section className="mb-6 flex flex-wrap gap-2">
        {availableCuisines.map((item) => (
          <CuisineChip key={item} label={item} active={activeCuisine === item} onClick={() => setActiveCuisine(item)} />
        ))}
      </section>

      {loading ? <p className="text-sm text-on-background/70">Loading restaurants...</p> : null}
      {error ? <p className="text-sm text-error">{error}</p> : null}
      {!loading && !error ? (
        <RestaurantList restaurants={restaurants} onOpenRestaurant={(restaurant) => navigate(`/restaurant/${restaurant.id}`)} />
      ) : null}
    </div>
  )
}
