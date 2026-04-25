import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { SearchBar } from '../components/molecules/SearchBar'
import { CuisineChip } from '../components/molecules/CuisineChip'
import { RestaurantList } from '../components/organisms/RestaurantList'

const restaurantsSeed = [
  { id: 'r1', name: 'Spice Route Kitchen', cuisine: 'Indian', rating: 4.6, deliveryTime: '28-35 mins' },
  { id: 'r2', name: 'Tokyo Roll House', cuisine: 'Sushi', rating: 4.4, deliveryTime: '30-40 mins' },
  { id: 'r3', name: 'Burger Forge', cuisine: 'Burgers', rating: 4.3, deliveryTime: '20-30 mins' },
  { id: 'r4', name: 'Fire Oven Pizza', cuisine: 'Pizza', rating: 4.7, deliveryTime: '25-35 mins' },
]

const cuisines = ['All', 'Indian', 'Sushi', 'Burgers', 'Pizza']

export const HomePage = () => {
  const navigate = useNavigate()
  const [query, setQuery] = useState('')
  const [activeCuisine, setActiveCuisine] = useState('All')

  const restaurants = useMemo(() => {
    return restaurantsSeed.filter((item) => {
      const cuisineMatch = activeCuisine === 'All' || item.cuisine === activeCuisine
      const queryMatch = !query || item.name.toLowerCase().includes(query.toLowerCase()) || item.cuisine.toLowerCase().includes(query.toLowerCase())
      return cuisineMatch && queryMatch
    })
  }, [query, activeCuisine])

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
        {cuisines.map((item) => (
          <CuisineChip key={item} label={item} active={activeCuisine === item} onClick={() => setActiveCuisine(item)} />
        ))}
      </section>

      <RestaurantList restaurants={restaurants} onOpenRestaurant={(restaurant) => navigate(`/restaurant/${restaurant.id}`)} />
    </div>
  )
}
