import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { RestaurantCard } from '../components/molecules/RestaurantCard'
import catalogApi from '../services/catalogApi'

const CUISINE_LABELS = {
  1: 'Italian', 2: 'Chinese', 3: 'Indian', 4: 'Japanese', 5: 'Mexican',
  6: 'American', 7: 'Thai', 8: 'Mediterranean', 9: 'Fast Food', 10: 'Vegan', 11: 'Other'
}

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
          cuisine: CUISINE_LABELS[item.cuisineType] || item.cuisineType || 'Other',
          rating: Number(item.averageRating || item.rating || 0),
          deliveryTime: item.deliveryTime ? `${item.deliveryTime} mins` : 'N/A',
          imageUrl: item.imageUrl ?? null,
          city: item.city ?? '',
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

  const cuisines = ['All', ...new Set(restaurants.map(r => r.cuisine).filter(Boolean))]

  const filtered = restaurants.filter(item => {
    const cuisineMatch = activeCuisine === 'All' || item.cuisine === activeCuisine
    const queryMatch = !query || item.name.toLowerCase().includes(query.toLowerCase())
    return cuisineMatch && queryMatch
  })

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <div className="bg-white border-b border-slate-200 sticky top-0 z-30">
        <div className="max-w-7xl mx-auto px-6 py-4 flex items-center gap-4">
          <button onClick={() => navigate(-1)} className="w-9 h-9 rounded-full bg-slate-100 flex items-center justify-center hover:bg-slate-200 transition-colors">
            <span className="material-symbols-outlined text-lg">arrow_back</span>
          </button>
          <h1 className="text-lg font-bold text-on-surface">All Restaurants</h1>
          <div className="ml-auto relative">
            <span className="absolute inset-y-0 left-3 flex items-center pointer-events-none">
              <span className="material-symbols-outlined text-slate-400 text-lg">search</span>
            </span>
            <input
              value={query}
              onChange={e => setQuery(e.target.value)}
              placeholder="Search..."
              className="pl-9 pr-4 py-2 bg-slate-100 rounded-full text-sm focus:outline-none focus:ring-2 focus:ring-primary w-48"
            />
          </div>
        </div>
        {/* Cuisine filter row */}
        <div className="max-w-7xl mx-auto px-6 pb-3 flex gap-2 overflow-x-auto no-scrollbar">
          {cuisines.map(c => (
            <button
              key={c}
              onClick={() => setActiveCuisine(c)}
              className={`px-4 py-1.5 rounded-full text-sm font-semibold whitespace-nowrap transition-all ${
                activeCuisine === c
                  ? 'bg-primary text-white shadow-sm'
                  : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
              }`}
            >
              {c}
            </button>
          ))}
        </div>
      </div>

      {/* Grid */}
      <div className="max-w-7xl mx-auto px-6 py-8">
        {loading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
            {[1,2,3,4,5,6].map(i => (
              <div key={i} className="rounded-2xl bg-slate-200 animate-pulse h-64" />
            ))}
          </div>
        ) : filtered.length === 0 ? (
          <div className="text-center py-20 text-slate-400">
            <span className="material-symbols-outlined text-5xl mb-3 block">search_off</span>
            <p className="font-semibold">No restaurants found</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
            {filtered.map(restaurant => (
              <RestaurantCard
                key={restaurant.id}
                restaurant={restaurant}
                onOpen={(r) => navigate(`/restaurant/${r.id}`)}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
