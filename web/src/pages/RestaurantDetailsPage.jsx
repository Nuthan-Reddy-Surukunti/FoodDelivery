import { useEffect, useRef, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useCart } from '../context/CartContext'
import { useNotification } from '../hooks/useNotification'
import catalogApi from '../services/catalogApi'

const VegDot = ({ isVeg }) => (
  <span className={`inline-flex h-4 w-4 flex-shrink-0 items-center justify-center rounded-sm border-2 ${isVeg ? 'border-green-600' : 'border-red-600'}`}>
    <span className={`h-1.5 w-1.5 rounded-full ${isVeg ? 'bg-green-600' : 'bg-red-600'}`} />
  </span>
)

const CUISINE_LABEL = { 1: 'Italian', 2: 'Chinese', 3: 'Indian', 4: 'Japanese', 5: 'Mexican', 6: 'American', 7: 'Thai', 8: 'Mediterranean' }

export const RestaurantDetailsPage = () => {
  const { id } = useParams()
  const navigate = useNavigate()
  const { addItem, totalItems, totalPrice } = useCart()
  const { showSuccess } = useNotification()

  const [restaurant, setRestaurant] = useState(null)
  const [categories, setCategories] = useState([])
  const [allItems, setAllItems] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [activeCategory, setActiveCategory] = useState(null)
  const sectionRefs = useRef({})

  useEffect(() => {
    if (!id) return
    let active = true
    setLoading(true)
    setError('')

    const loadDetails = async () => {
      try {
        const [restaurantData, menuData, categoryData] = await Promise.all([
          catalogApi.getRestaurantById(id),
          catalogApi.getRestaurantMenu(id),
          catalogApi.getCategories(id).catch(() => []),
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
          imageUrl: item.imageUrl ?? null,
          categoryName: item.categoryName || null,
          availabilityStatus: item.availabilityStatus,
        })))
        if (cats.length > 0) setActiveCategory(cats[0].name)
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
    return s === 1 || s === 'Available' || s == null
  }

  // Group items by category
  const grouped = (() => {
    if (categories.length === 0) return [{ name: 'Menu', items: allItems }]
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
    if (uncategorised.length > 0) sections.push({ name: 'Menu Items', items: uncategorised })
    return sections
  })()

  const scrollToSection = (name) => {
    setActiveCategory(name)
    const el = sectionRefs.current[name]
    if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }

  if (loading) return (
    <div className="min-h-screen bg-transparent">
      <div className="h-64 bg-slate-200 animate-pulse" />
      <div className="max-w-5xl mx-auto px-6 py-8 space-y-4">
        <div className="h-8 w-64 bg-slate-200 animate-pulse rounded-xl" />
        <div className="h-4 w-48 bg-slate-200 animate-pulse rounded-xl" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mt-8">
          {[1,2,3,4].map(i => <div key={i} className="h-28 bg-slate-200 animate-pulse rounded-xl" />)}
        </div>
      </div>
    </div>
  )

  if (error) return (
    <div className="max-w-5xl mx-auto px-6 py-12 text-sm text-error bg-error-container rounded-xl mx-6 mt-8 p-4">
      {error}
    </div>
  )
  if (!restaurant) return null

  const cuisineLabel = CUISINE_LABEL[restaurant.cuisineType] || restaurant.cuisineType || ''
  const rating = Number(restaurant.rating ?? 0).toFixed(1)

  return (
    <div className="min-h-screen bg-transparent pb-32">
      {/* ── Hero ── */}
      <section className="relative h-64 md:h-96 w-full">
        <div className="absolute inset-0 bg-gradient-to-t from-slate-900/90 via-slate-900/30 to-transparent z-10" />
        {restaurant.imageUrl ? (
          <img src={restaurant.imageUrl} alt={restaurant.name} className="w-full h-full object-cover" />
        ) : (
          <div className="w-full h-full bg-gradient-to-br from-primary/40 to-slate-800 flex items-center justify-center">
            <span className="text-8xl">🍽️</span>
          </div>
        )}
        {/* Info overlay */}
        <div className="absolute bottom-0 left-0 w-full z-20 px-6 py-6 max-w-5xl mx-auto">
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-4">
            <div>
              <h1 className="text-3xl md:text-4xl font-bold text-white mb-2 tracking-tight">{restaurant.name}</h1>
              <p className="text-slate-300 flex items-center flex-wrap gap-2 text-sm font-medium">
                {cuisineLabel && <span>{cuisineLabel}</span>}
                {restaurant.description && (
                  <>
                    <span className="w-1 h-1 rounded-full bg-slate-400" />
                    <span className="line-clamp-1 max-w-xs">{restaurant.description}</span>
                  </>
                )}
              </p>
            </div>
            {/* Stats pill */}
            <div className="flex gap-4 bg-white/10 backdrop-blur-md rounded-xl p-3 w-fit border border-white/20">
              <div className="flex flex-col items-center px-3 border-r border-white/20">
                <div className="flex items-center gap-1 text-yellow-400 mb-1">
                  <span className="material-symbols-outlined text-lg" style={{ fontVariationSettings: "'FILL' 1" }}>star</span>
                  <span className="text-sm font-semibold text-white">{rating}</span>
                </div>
                <span className="text-xs text-slate-300 font-medium">Rating</span>
              </div>
              {restaurant.deliveryTime && (
                <div className="flex flex-col items-center px-3 border-r border-white/20">
                  <span className="text-sm font-semibold text-white mb-1">{restaurant.deliveryTime}</span>
                  <span className="text-xs text-slate-300 font-medium">Mins</span>
                </div>
              )}
              {restaurant.minOrderValue && (
                <div className="flex flex-col items-center px-3">
                  <span className="text-sm font-semibold text-white mb-1">₹{restaurant.minOrderValue}</span>
                  <span className="text-xs text-slate-300 font-medium">Min Order</span>
                </div>
              )}
            </div>
          </div>
        </div>
      </section>

      <div className="max-w-5xl mx-auto px-6 py-8">
        {/* ── Sticky category chip bar ── */}
        {grouped.length > 1 && (
          <div className="sticky top-[64px] z-40 bg-background/95 backdrop-blur py-4 mb-8 -mx-6 px-6 border-b border-slate-200 flex gap-3 overflow-x-auto no-scrollbar">
            {grouped.map(g => (
              <button
                key={g.name}
                onClick={() => scrollToSection(g.name)}
                className={`px-5 py-2 rounded-lg text-sm font-medium whitespace-nowrap transition-all active:scale-95 shadow-sm ${activeCategory === g.name ? 'bg-primary text-on-primary' : 'bg-white border border-slate-200 text-slate-700 hover:bg-slate-50'}`}
              >
                {g.name}
              </button>
            ))}
          </div>
        )}

        {/* ── Menu sections ── */}
        {allItems.length === 0 ? (
          <div className="py-16 text-center text-on-surface-variant">
            <p className="text-4xl mb-3">🍽️</p>
            <p className="text-lg font-semibold">No menu items available</p>
          </div>
        ) : (
          <div className="space-y-12">
            {grouped.map(group => (
              <section
                key={group.name}
                id={`section-${group.name}`}
                ref={el => { sectionRefs.current[group.name] = el }}
              >
                <h2 className="text-2xl font-bold text-on-surface mb-6 tracking-tight">{group.name}</h2>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  {group.items.map(item => (
                    <div
                      key={item.id}
                      className={`bg-white rounded-xl border shadow-sm hover:shadow-md transition-shadow p-5 flex gap-4 items-center ${!isAvailable(item) ? 'opacity-60' : 'border-slate-100'}`}
                    >
                      {/* Text */}
                      <div className="flex-grow min-w-0">
                        <div className="flex items-center gap-2 mb-1">
                          <VegDot isVeg={item.isVeg} />
                          <h3 className="text-base font-semibold text-on-surface">{item.name}</h3>
                        </div>
                        {item.description && (
                          <p className="text-sm text-slate-500 mb-2 line-clamp-2">{item.description}</p>
                        )}
                        <div className="flex items-center gap-3">
                          <span className="text-lg font-bold text-primary">₹{item.price.toFixed(2)}</span>
                          {item.prepTime && (
                            <span className="text-xs text-slate-500 flex items-center gap-1">
                              <span className="material-symbols-outlined text-sm">schedule</span>
                              {item.prepTime} min
                            </span>
                          )}
                          {!isAvailable(item) && <span className="text-xs text-red-500 font-medium">Unavailable</span>}
                        </div>
                      </div>
                      {/* Image + Add button */}
                      <div className="relative w-28 h-28 flex-shrink-0">
                        {item.imageUrl ? (
                          <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover rounded-xl" />
                        ) : (
                          <div className="w-full h-full rounded-xl bg-slate-100 flex items-center justify-center text-3xl">
                            🍽️
                          </div>
                        )}
                        {isAvailable(item) && (
                          <button
                            onClick={() => handleAddToCart(item)}
                            aria-label={`Add ${item.name} to cart`}
                            className="absolute -bottom-3 -right-3 w-10 h-10 rounded-full bg-primary text-white shadow-md flex items-center justify-center hover:bg-primary-container active:scale-95 transition-all"
                          >
                            <span className="material-symbols-outlined text-sm">add</span>
                          </button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </section>
            ))}
          </div>
        )}
      </div>

      {/* ── Floating View Cart FAB ── */}
      {totalItems > 0 && (
        <div className="fixed bottom-6 left-0 right-0 px-5 z-50 pointer-events-none flex justify-center">
          <button
            onClick={() => navigate('/cart')}
            className="pointer-events-auto bg-primary text-white rounded-2xl px-6 py-4 flex items-center gap-3 shadow-lg hover:shadow-xl hover:bg-primary-container active:scale-95 transition-all max-w-sm w-full justify-between"
          >
            <div className="flex items-center gap-3">
              <div className="bg-white text-primary w-7 h-7 rounded-full flex items-center justify-center font-bold text-sm shadow-sm">
                {totalItems}
              </div>
              <span className="text-base font-semibold">View Cart</span>
            </div>
            <span className="text-lg font-bold">₹{Number(totalPrice || 0).toFixed(2)}</span>
          </button>
        </div>
      )}
    </div>
  )
}
