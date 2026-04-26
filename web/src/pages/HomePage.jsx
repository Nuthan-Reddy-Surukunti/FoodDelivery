import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import catalogApi from '../services/catalogApi'

// ── Constants ─────────────────────────────────────────────────────────────────
const CUISINE_DISPLAY = {
  0: { label: 'All', emoji: '🍽️' },
  1: { label: 'Italian', emoji: '🍕' },
  2: { label: 'Chinese', emoji: '🥢' },
  3: { label: 'Indian', emoji: '🍛' },
  4: { label: 'Japanese', emoji: '🍱' },
  5: { label: 'Mexican', emoji: '🌮' },
  6: { label: 'American', emoji: '🍔' },
  7: { label: 'Thai', emoji: '🍜' },
  8: { label: 'Mediterranean', emoji: '🥙' },
  9: { label: 'Other', emoji: '🍽️' },
}

const cuisineLabel = (type) =>
  CUISINE_DISPLAY[type]?.label || (typeof type === 'string' ? type : 'Other')
const cuisineEmoji = (type) => CUISINE_DISPLAY[type]?.emoji || '🍽️'

const normalizeRestaurant = (item) => ({
  id: item.id,
  name: item.name,
  description: item.description,
  cuisineType: item.cuisineType ?? item.cuisine,
  cuisineLabel: cuisineLabel(item.cuisineType ?? item.cuisine),
  rating: Number(item.rating ?? item.averageRating ?? 0).toFixed(1),
  deliveryTime: item.deliveryTime ?? item.estimatedDeliveryTime ?? null,
  imageUrl: item.imageUrl ?? null,
  city: item.city ?? '',
})

// ── Sub-components ────────────────────────────────────────────────────────────
const SkeletonCard = ({ className = '' }) => (
  <div className={`rounded-xl bg-slate-200 animate-pulse ${className}`} />
)

const StarIcon = () => (
  <span className="material-symbols-outlined text-yellow-400" style={{ fontSize: 14, fontVariationSettings: "'FILL' 1" }}>
    star
  </span>
)

// ── Universal Search Dropdown ─────────────────────────────────────────────────
const SearchDropdown = ({ results, loading, query, onSelectRestaurant, onSelectMenuItem }) => {
  const { restaurants = [], menuItems = [] } = results
  const hasResults = restaurants.length > 0 || menuItems.length > 0

  if (loading) {
    return (
      <div className="absolute top-full left-0 right-0 mt-2 bg-white rounded-2xl shadow-2xl border border-slate-100 z-50 overflow-hidden">
        <div className="p-4 space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-slate-200 animate-pulse flex-shrink-0" />
              <div className="flex-1 space-y-1.5">
                <div className="h-3 bg-slate-200 animate-pulse rounded w-3/4" />
                <div className="h-2.5 bg-slate-100 animate-pulse rounded w-1/2" />
              </div>
            </div>
          ))}
        </div>
      </div>
    )
  }

  if (!hasResults && query.length >= 2) {
    return (
      <div className="absolute top-full left-0 right-0 mt-2 bg-white rounded-2xl shadow-2xl border border-slate-100 z-50 p-6 text-center">
        <span className="material-symbols-outlined text-3xl text-slate-300 block mb-2">search_off</span>
        <p className="text-sm font-medium text-on-surface-variant">No results for "{query}"</p>
      </div>
    )
  }

  if (!hasResults) return null

  return (
    <div className="absolute top-full left-0 right-0 mt-2 bg-white rounded-2xl shadow-2xl border border-slate-100 z-50 overflow-hidden max-h-[420px] overflow-y-auto">
      {/* Restaurants section */}
      {restaurants.length > 0 && (
        <div>
          <div className="px-4 pt-3 pb-1.5 flex items-center gap-2">
            <span className="material-symbols-outlined text-sm text-on-surface-variant">storefront</span>
            <span className="text-xs font-semibold text-on-surface-variant uppercase tracking-wider">Restaurants</span>
          </div>
          {restaurants.slice(0, 4).map((r) => (
            <button
              key={r.id}
              onClick={() => onSelectRestaurant(r.id)}
              className="w-full flex items-center gap-3 px-4 py-3 hover:bg-slate-50 transition-colors text-left"
            >
              <div className="w-10 h-10 rounded-xl bg-slate-100 flex items-center justify-center text-xl flex-shrink-0 overflow-hidden">
                {r.imageUrl ? <img src={r.imageUrl} alt={r.name} className="w-full h-full object-cover" /> : cuisineEmoji(r.cuisineType)}
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-semibold text-on-surface truncate">{r.name}</p>
                <p className="text-xs text-on-surface-variant">{cuisineLabel(r.cuisineType)}{r.city ? ` · ${r.city}` : ''}</p>
              </div>
              <div className="flex items-center gap-0.5 text-xs text-on-surface-variant flex-shrink-0">
                <StarIcon />
                <span>{Number(r.rating ?? r.averageRating ?? 0).toFixed(1)}</span>
              </div>
            </button>
          ))}
        </div>
      )}

      {/* Divider */}
      {restaurants.length > 0 && menuItems.length > 0 && (
        <div className="border-t border-slate-100 mx-4" />
      )}

      {/* Menu items section */}
      {menuItems.length > 0 && (
        <div>
          <div className="px-4 pt-3 pb-1.5 flex items-center gap-2">
            <span className="material-symbols-outlined text-sm text-on-surface-variant">restaurant_menu</span>
            <span className="text-xs font-semibold text-on-surface-variant uppercase tracking-wider">Menu Items</span>
          </div>
          {menuItems.slice(0, 5).map((item) => (
            <button
              key={item.id}
              onClick={() => onSelectMenuItem(item.restaurantId)}
              className="w-full flex items-center gap-3 px-4 py-3 hover:bg-slate-50 transition-colors text-left"
            >
              <div className="w-10 h-10 rounded-xl bg-slate-50 border border-slate-100 flex items-center justify-center text-xl flex-shrink-0 overflow-hidden">
                {item.imageUrl ? <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" /> : '🍽️'}
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2">
                  {item.isVeg && (
                    <span className="inline-flex h-3.5 w-3.5 items-center justify-center rounded-sm border border-green-600 flex-shrink-0">
                      <span className="h-1.5 w-1.5 rounded-full bg-green-600" />
                    </span>
                  )}
                  <p className="text-sm font-semibold text-on-surface truncate">{item.name}</p>
                </div>
                <p className="text-xs text-on-surface-variant truncate">at {item.restaurantName}</p>
              </div>
              <span className="text-sm font-bold text-primary flex-shrink-0">₹{Number(item.price).toFixed(0)}</span>
            </button>
          ))}
        </div>
      )}

      {/* "See all results" footer */}
      <div className="border-t border-slate-100 px-4 py-3">
        <button
          onClick={() => onSelectRestaurant(null, query)}
          className="w-full text-sm font-semibold text-primary hover:text-primary-container transition-colors flex items-center justify-center gap-1"
        >
          <span className="material-symbols-outlined text-base">search</span>
          See all results for "{query}"
        </button>
      </div>
    </div>
  )
}

// ── Main Page ─────────────────────────────────────────────────────────────────
export const HomePage = () => {
  const navigate = useNavigate()

  // Restaurant list state
  const [restaurantsData, setRestaurantsData] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [activeCuisine, setActiveCuisine] = useState(null)

  // Search state
  const [query, setQuery] = useState('')
  const [dropdownOpen, setDropdownOpen] = useState(false)
  const [searchResults, setSearchResults] = useState({ restaurants: [], menuItems: [] })
  const [searchLoading, setSearchLoading] = useState(false)

  const searchContainerRef = useRef(null)
  const debounceRef = useRef(null)

  // Load all restaurants on mount
  useEffect(() => {
    let active = true
    setLoading(true)
    setError('')
    catalogApi.getRestaurants()
      .then((res) => {
        if (!active) return
        const raw = Array.isArray(res) ? res : (res?.items || res?.data || [])
        setRestaurantsData(raw.map(normalizeRestaurant))
      })
      .catch((err) => {
        if (!active) return
        setError(err.response?.data?.message || err.message || 'Failed to load restaurants')
      })
      .finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [])

  // Live search with 300ms debounce
  const doSearch = useCallback(async (q) => {
    if (q.length < 2) {
      setSearchResults({ restaurants: [], menuItems: [] })
      setDropdownOpen(false)
      return
    }
    setSearchLoading(true)
    setDropdownOpen(true)
    try {
      const results = await catalogApi.searchAll(q)
      setSearchResults(results)
    } catch {
      setSearchResults({ restaurants: [], menuItems: [] })
    } finally {
      setSearchLoading(false)
    }
  }, [])

  const handleQueryChange = (e) => {
    const val = e.target.value
    setQuery(val)
    clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => doSearch(val), 300)
  }

  const handleSearchSubmit = (e) => {
    e?.preventDefault()
    if (query.trim()) navigate(`/search?q=${encodeURIComponent(query.trim())}`)
  }

  // Close dropdown on outside click
  useEffect(() => {
    const handler = (e) => {
      if (searchContainerRef.current && !searchContainerRef.current.contains(e.target)) {
        setDropdownOpen(false)
      }
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [])

  // Filter restaurant grid by cuisine + query (client-side for the cards)
  const availableCuisines = useMemo(() => {
    const types = new Set(restaurantsData.map((r) => r.cuisineType))
    return Array.from(types).filter((t) => t != null)
  }, [restaurantsData])

  const filtered = useMemo(() => {
    return restaurantsData.filter((r) => {
      const cuisineMatch = activeCuisine === null || r.cuisineType === activeCuisine
      const queryMatch = !query || r.name.toLowerCase().includes(query.toLowerCase()) || r.cuisineLabel.toLowerCase().includes(query.toLowerCase())
      return cuisineMatch && queryMatch
    })
  }, [restaurantsData, query, activeCuisine])

  const featured = filtered[0] ?? null
  const secondary = filtered.slice(1, 3)
  const rest = filtered.slice(3)

  const goToRestaurant = (id) => navigate(`/restaurant/${id}`)

  const handleDropdownRestaurantSelect = (restaurantId, fallbackQuery) => {
    setDropdownOpen(false)
    if (restaurantId) goToRestaurant(restaurantId)
    else navigate(`/search?q=${encodeURIComponent(fallbackQuery || query)}`)
  }

  const handleDropdownMenuItemSelect = (restaurantId) => {
    setDropdownOpen(false)
    goToRestaurant(restaurantId)
  }

  return (
    <div className="bg-background min-h-screen">
      {/* ── Hero Search Section ── */}
      <section className="px-6 pt-8 pb-6 max-w-7xl mx-auto">
        <h1 className="text-[32px] font-bold text-on-surface leading-tight max-w-md mb-6">
          What are you craving today?
        </h1>

        {/* Search box with live dropdown */}
        <div className="relative w-full max-w-2xl" ref={searchContainerRef}>
          <form onSubmit={handleSearchSubmit}>
            <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none z-10">
              <span className="material-symbols-outlined text-on-surface-variant text-xl">search</span>
            </div>
            <input
              id="universal-search-input"
              type="text"
              value={query}
              onChange={handleQueryChange}
              onFocus={() => query.length >= 2 && setDropdownOpen(true)}
              placeholder="Search restaurants or dishes..."
              autoComplete="off"
              className="block w-full pl-12 pr-32 py-4 bg-surface-container-low border border-slate-200 rounded-full text-base text-on-surface placeholder:text-on-surface-variant focus:ring-2 focus:ring-primary focus:border-primary shadow-sm transition-shadow outline-none"
            />
            <button
              type="submit"
              className="absolute inset-y-2 right-2 px-6 bg-primary text-on-primary rounded-full text-sm font-semibold hover:bg-primary-container active:scale-95 transition-all"
            >
              Find Food
            </button>
          </form>

          {/* Live results dropdown */}
          {dropdownOpen && (
            <SearchDropdown
              results={searchResults}
              loading={searchLoading}
              query={query}
              onSelectRestaurant={handleDropdownRestaurantSelect}
              onSelectMenuItem={handleDropdownMenuItemSelect}
            />
          )}
        </div>
      </section>

      {/* ── Cuisine Carousel ── */}
      {availableCuisines.length > 0 && (
        <section className="pb-6">
          <div className="px-6 max-w-7xl mx-auto mb-4">
            <h2 className="text-xl font-semibold text-on-surface">Categories</h2>
          </div>
          <div className="flex gap-4 overflow-x-auto no-scrollbar px-6 max-w-7xl mx-auto snap-x">
            <button
              onClick={() => setActiveCuisine(null)}
              className="flex flex-col items-center gap-2 min-w-[80px] snap-start group active:scale-95 transition-transform"
            >
              <div className={`w-16 h-16 rounded-full flex items-center justify-center text-2xl shadow-sm border-2 transition-all group-hover:ring-2 group-hover:ring-primary ${activeCuisine === null ? 'border-primary bg-primary/10 ring-2 ring-primary' : 'border-slate-200 bg-surface-container-high'}`}>
                🍽️
              </div>
              <span className="text-sm font-semibold text-on-surface">All</span>
            </button>
            {availableCuisines.map((type) => (
              <button
                key={type}
                onClick={() => setActiveCuisine(activeCuisine === type ? null : type)}
                className="flex flex-col items-center gap-2 min-w-[80px] snap-start group active:scale-95 transition-transform"
              >
                <div className={`w-16 h-16 rounded-full flex items-center justify-center text-2xl shadow-sm border-2 transition-all group-hover:ring-2 group-hover:ring-primary ${activeCuisine === type ? 'border-primary bg-primary/10 ring-2 ring-primary' : 'border-slate-200 bg-surface-container-high'}`}>
                  {cuisineEmoji(type)}
                </div>
                <span className="text-sm font-semibold text-on-surface">{cuisineLabel(type)}</span>
              </button>
            ))}
          </div>
        </section>
      )}

      {/* ── Main Content ── */}
      <section className="px-6 pb-16 max-w-7xl mx-auto">
        <div className="flex justify-between items-end mb-6">
          <h2 className="text-2xl font-bold text-on-surface">Popular Near You</h2>
          <button
            onClick={() => navigate('/explore')}
            className="text-sm font-semibold text-primary hover:text-primary-container transition-colors"
          >
            See all
          </button>
        </div>

        {error && (
          <div className="bg-error-container text-on-error-container px-4 py-3 rounded-xl text-sm mb-6">
            {error}
          </div>
        )}

        {loading && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <SkeletonCard className="md:col-span-2 h-[300px]" />
            <div className="flex flex-col gap-6 h-[300px]">
              <SkeletonCard className="flex-1" />
              <SkeletonCard className="flex-1" />
            </div>
          </div>
        )}

        {!loading && filtered.length === 0 && (
          <div className="py-16 text-center text-on-surface-variant">
            <p className="text-4xl mb-3">🍽️</p>
            <p className="text-lg font-semibold">No restaurants found</p>
            <p className="text-sm mt-1">Try a different search or category</p>
          </div>
        )}

        {!loading && filtered.length > 0 && (
          <>
            {/* Primary bento row */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
              {featured && (
                <div
                  onClick={() => goToRestaurant(featured.id)}
                  className="md:col-span-2 relative rounded-xl overflow-hidden bg-surface-container-lowest shadow-sm border border-slate-200 group cursor-pointer h-[300px]"
                >
                  <div className="absolute inset-0 bg-gradient-to-br from-primary/30 to-slate-900/60" />
                  {featured.imageUrl ? (
                    <img src={featured.imageUrl} alt={featured.name} className="absolute inset-0 w-full h-full object-cover transition-transform duration-500 group-hover:scale-105" />
                  ) : (
                    <div className="absolute inset-0 bg-gradient-to-br from-primary/20 to-slate-700 flex items-center justify-center text-7xl">
                      {cuisineEmoji(featured.cuisineType)}
                    </div>
                  )}
                  <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent" />
                  <div className="absolute top-4 right-4 bg-white/90 backdrop-blur-sm px-3 py-1 rounded-full flex items-center gap-1 shadow-sm">
                    <StarIcon />
                    <span className="text-sm font-semibold text-on-surface">{featured.rating}</span>
                  </div>
                  <div className="absolute bottom-0 left-0 p-6 w-full">
                    <h3 className="text-2xl font-bold text-white mb-2">{featured.name}</h3>
                    <div className="flex flex-wrap gap-3 items-center">
                      <span className="bg-white/20 backdrop-blur-md px-3 py-1 rounded-full text-xs text-white flex items-center gap-1">
                        <span className="material-symbols-outlined text-sm">storefront</span>
                        {featured.cuisineLabel}
                      </span>
                      {featured.deliveryTime && (
                        <span className="bg-white/20 backdrop-blur-md px-3 py-1 rounded-full text-xs text-white flex items-center gap-1">
                          <span className="material-symbols-outlined text-sm">schedule</span>
                          {featured.deliveryTime} min
                        </span>
                      )}
                    </div>
                  </div>
                </div>
              )}

              {secondary.length > 0 && (
                <div className="flex flex-col gap-6 h-[300px]">
                  {secondary.map((r) => (
                    <div
                      key={r.id}
                      onClick={() => goToRestaurant(r.id)}
                      className="flex-1 relative rounded-xl overflow-hidden bg-surface-container-lowest shadow-sm border border-slate-200 group cursor-pointer"
                    >
                      {r.imageUrl ? (
                        <img src={r.imageUrl} alt={r.name} className="absolute inset-0 w-full h-full object-cover transition-transform duration-500 group-hover:scale-105" />
                      ) : (
                        <div className="absolute inset-0 bg-gradient-to-br from-primary/20 to-slate-600 flex items-center justify-center text-4xl">
                          {cuisineEmoji(r.cuisineType)}
                        </div>
                      )}
                      <div className="absolute inset-0 bg-gradient-to-t from-black/70 to-transparent" />
                      <div className="absolute top-3 right-3 bg-white/90 backdrop-blur-sm px-2 py-0.5 rounded-full flex items-center gap-1">
                        <StarIcon />
                        <span className="text-xs font-semibold text-on-surface">{r.rating}</span>
                      </div>
                      <div className="absolute bottom-0 left-0 p-4 w-full">
                        <h3 className="text-lg font-semibold text-white mb-0.5">{r.name}</h3>
                        <p className="text-xs text-white/80">{r.cuisineLabel}{r.deliveryTime ? ` • ${r.deliveryTime} min` : ''}</p>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* Remaining restaurants row */}
            {rest.length > 0 && (
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
                {rest.map((r) => (
                  <div
                    key={r.id}
                    onClick={() => goToRestaurant(r.id)}
                    className="relative rounded-xl overflow-hidden bg-surface-container-lowest shadow-sm border border-slate-200 group cursor-pointer h-[180px]"
                  >
                    {r.imageUrl ? (
                      <img src={r.imageUrl} alt={r.name} className="absolute inset-0 w-full h-full object-cover transition-transform duration-500 group-hover:scale-105" />
                    ) : (
                      <div className="absolute inset-0 bg-gradient-to-br from-primary/20 to-slate-600 flex items-center justify-center text-4xl">
                        {cuisineEmoji(r.cuisineType)}
                      </div>
                    )}
                    <div className="absolute inset-0 bg-gradient-to-t from-black/70 to-transparent" />
                    <div className="absolute top-3 right-3 bg-white/90 backdrop-blur-sm px-2 py-0.5 rounded-full flex items-center gap-1">
                      <StarIcon />
                      <span className="text-xs font-semibold text-on-surface">{r.rating}</span>
                    </div>
                    <div className="absolute bottom-0 left-0 p-4 w-full">
                      <h3 className="text-base font-semibold text-white mb-0.5">{r.name}</h3>
                      <p className="text-xs text-white/80">{r.cuisineLabel}{r.deliveryTime ? ` • ${r.deliveryTime} min` : ''}</p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </>
        )}
      </section>

      {/* ── Footer ── */}
      <footer className="bg-white border-t border-slate-200 py-12">
        <div className="flex flex-col md:flex-row justify-between items-center px-8 max-w-7xl mx-auto gap-4 text-sm">
          <div className="text-lg font-bold text-on-surface">QuickBite</div>
          <div className="flex gap-6">
            {['Privacy Policy', 'Terms of Service', 'Help Center', 'Contact Us'].map((l) => (
              <a key={l} href="#" className="text-on-surface-variant hover:text-primary transition-colors">{l}</a>
            ))}
          </div>
          <p className="text-on-surface-variant">© 2024 QuickBite. All rights reserved.</p>
        </div>
      </footer>
    </div>
  )
}
