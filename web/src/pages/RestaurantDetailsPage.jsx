import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { useCart } from '../context/CartContext'
import { useNotification } from '../hooks/useNotification'
import catalogApi from '../services/catalogApi'
import { Button } from '../components/atoms/Button'

const VegBadge = ({ isVeg }) => (
  <span className={`inline-flex h-4 w-4 flex-shrink-0 items-center justify-center rounded border-2 ${isVeg ? 'border-green-500' : 'border-red-500'}`}>
    <span className={`h-2 w-2 rounded-full ${isVeg ? 'bg-green-500' : 'bg-red-500'}`} />
  </span>
)

export const RestaurantDetailsPage = () => {
  const { id } = useParams()
  const { addItem } = useCart()
  const { showSuccess } = useNotification()

  const [restaurant, setRestaurant] = useState(null)
  const [categories, setCategories] = useState([])
  const [allItems, setAllItems] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [activeCategory, setActiveCategory] = useState(null)

  useEffect(() => {
    if (!id) return
    let active = true

    const loadDetails = async () => {
      setLoading(true)
      setError('')
      try {
        const [restaurantData, menuData, categoryData] = await Promise.all([
          catalogApi.getRestaurantById(id),
          catalogApi.getRestaurantMenu(id),
          catalogApi.getCategories(id).catch(() => []), // categories optional
        ])
        if (!active) return

        setRestaurant(restaurantData)

        const cats = Array.isArray(categoryData) ? categoryData : (categoryData?.items || [])
        setCategories(cats)

        const raw = Array.isArray(menuData) ? menuData : (menuData?.items || menuData?.data || [])
        setAllItems(raw.map(item => ({
          id: item.id,
          name: item.name,
          description: item.description || '',
          price: Number(item.price || 0),
          isVeg: item.isVeg ?? true,
          prepTime: item.prepTime,
          categoryName: item.categoryName || null,
          availabilityStatus: item.availabilityStatus,
        })))
      } catch (err) {
        if (!active) return
        setError(err.response?.data?.message || err.message || 'Failed to load restaurant')
      } finally {
        if (active) setLoading(false)
      }
    }

    loadDetails()
    return () => { active = false }
  }, [id])

  const handleAddToCart = (item) => {
    addItem({ ...item, restaurantId: id }, id)
    showSuccess(`${item.name} added to cart`)
  }

  const isAvailable = (item) => {
    const s = item.availabilityStatus
    return s === 0 || s === 'Available' || s == null
  }

  // Group items by category
  const grouped = (() => {
    if (categories.length === 0) {
      return [{ name: 'Menu', items: allItems }]
    }
    const catOrder = categories.map(c => c.name)
    const map = {}
    for (const cat of catOrder) map[cat] = []

    const uncategorised = []
    for (const item of allItems) {
      if (item.categoryName && map[item.categoryName] !== undefined) {
        map[item.categoryName].push(item)
      } else {
        uncategorised.push(item)
      }
    }

    const sections = catOrder.map(name => ({ name, items: map[name] })).filter(s => s.items.length > 0)
    if (uncategorised.length > 0) sections.push({ name: 'Other', items: uncategorised })
    return sections
  })()

  const visibleGroups = activeCategory
    ? grouped.filter(g => g.name === activeCategory)
    : grouped

  if (loading) return <div className="mx-auto max-w-5xl px-4 py-12 text-sm text-on-background/70">Loading restaurant...</div>
  if (error) return <div className="mx-auto max-w-5xl px-4 py-12 text-sm text-error">{error}</div>
  if (!restaurant) return null

  const cuisineLabel = restaurant.cuisineTypeName || restaurant.cuisineType || ''

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      {/* Hero */}
      <section className="mb-6 rounded-3xl border border-outline bg-surface p-6">
        <h1 className="text-2xl font-bold">{restaurant.name}</h1>
        {restaurant.description && (
          <p className="mt-1 text-sm text-on-background/70">{restaurant.description}</p>
        )}
        <div className="mt-4 flex flex-wrap gap-4 text-sm text-on-background/70">
          {cuisineLabel && <span>🍽️ {cuisineLabel}</span>}
          {restaurant.rating > 0 && <span>⭐ {Number(restaurant.rating).toFixed(1)}</span>}
          {restaurant.deliveryTime && <span>⏱ {restaurant.deliveryTime} mins</span>}
          {restaurant.minOrderValue && <span>Min order ₹{restaurant.minOrderValue}</span>}
          <span className={`font-medium ${String(restaurant.status).toLowerCase() === 'active' ? 'text-green-600' : 'text-amber-600'}`}>
            {restaurant.status}
          </span>
        </div>
      </section>

      {/* Category filter bar */}
      {categories.length > 0 && (
        <div className="mb-6 flex gap-2 overflow-x-auto pb-1">
          <button
            onClick={() => setActiveCategory(null)}
            className={`rounded-full px-4 py-1.5 text-sm font-medium transition flex-shrink-0 ${!activeCategory ? 'bg-primary text-on-primary' : 'border border-outline hover:bg-surface-dim'}`}
          >
            All
          </button>
          {categories.map(cat => (
            <button
              key={cat.id}
              onClick={() => setActiveCategory(cat.name)}
              className={`rounded-full px-4 py-1.5 text-sm font-medium transition flex-shrink-0 ${activeCategory === cat.name ? 'bg-primary text-on-primary' : 'border border-outline hover:bg-surface-dim'}`}
            >
              {cat.name}
            </button>
          ))}
        </div>
      )}

      {/* Menu grouped by category */}
      {allItems.length === 0 ? (
        <p className="text-sm text-on-background/70">No menu items available.</p>
      ) : (
        <div className="space-y-8">
          {visibleGroups.map(group => (
            <section key={group.name}>
              <h2 className="mb-3 text-lg font-bold border-b border-outline pb-2">{group.name}</h2>
              <div className="space-y-3">
                {group.items.map(item => (
                  <div
                    key={item.id}
                    className={`flex items-center gap-4 rounded-2xl border p-4 transition ${isAvailable(item) ? 'border-outline bg-surface' : 'border-outline bg-surface opacity-50'}`}
                  >
                    <VegBadge isVeg={item.isVeg} />
                    <div className="flex-1 min-w-0">
                      <p className="font-semibold">{item.name}</p>
                      {item.description && (
                        <p className="text-xs text-on-background/60 mt-0.5 line-clamp-2">{item.description}</p>
                      )}
                      <div className="flex flex-wrap gap-3 mt-1 text-xs text-on-background/70">
                        <span className="font-semibold text-on-background">₹{item.price}</span>
                        {item.prepTime && <span>⏱ {item.prepTime} min</span>}
                        {!isAvailable(item) && <span className="text-red-500">Unavailable</span>}
                      </div>
                    </div>
                    <Button
                      size="sm"
                      disabled={!isAvailable(item)}
                      onClick={() => handleAddToCart(item)}
                    >
                      Add
                    </Button>
                  </div>
                ))}
              </div>
            </section>
          ))}
        </div>
      )}
    </div>
  )
}
