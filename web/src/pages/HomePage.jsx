import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import catalogApi from '../services/catalogApi'
import { AppFeaturesSection } from '../components/organisms/AppFeaturesSection'
import { useAuth } from '../context/AuthContext'
import { getHybridGreeting } from '../utils/greetingUtils'

// ── Constants ─────────────────────────────────────────────────────────────────
const CUISINE_DISPLAY = {
  0:  { label: 'All',           enumName: null,            emoji: '🍽️', imageUrl: '/images/categories/all.png' },
  1:  { label: 'Italian',       enumName: 'Italian',       emoji: '🍕', imageUrl: '/images/categories/italian.png' },
  2:  { label: 'Chinese',       enumName: 'Chinese',       emoji: '🥢', imageUrl: '/images/categories/chinese.png' },
  3:  { label: 'Indian',        enumName: 'Indian',        emoji: '🍛', imageUrl: '/images/categories/indian.png' },
  4:  { label: 'Mexican',       enumName: 'Mexican',       emoji: '🌮', imageUrl: '/images/categories/mexican.png' },
  5:  { label: 'American',      enumName: 'American',      emoji: '🍔', imageUrl: '/images/categories/american.png' },
  6:  { label: 'Thai',          enumName: 'Thai',          emoji: '🍜', imageUrl: '/images/categories/thai.png' },
  7:  { label: 'Japanese',      enumName: 'Japanese',      emoji: '🍱', imageUrl: '/images/categories/japanese.png' },
  8:  { label: 'Continental',   enumName: 'Continental',   emoji: '🥘', imageUrl: '/images/categories/continental.png' },
  9:  { label: 'Fast Food',     enumName: 'FastFood',      emoji: '🍟', imageUrl: '/images/categories/fastfood.png' },
  10: { label: 'Vegan',         enumName: 'Vegan',         emoji: '🥗', imageUrl: '/images/categories/vegan.png' },
  11: { label: 'Mediterranean', enumName: 'Mediterranean', emoji: '🥙', imageUrl: '/images/categories/mediterranean.png' },
}

const cuisineLabel = (type) =>
  CUISINE_DISPLAY[type]?.label || (typeof type === 'string' ? type : 'General')
const cuisineEmoji = (type) => CUISINE_DISPLAY[type]?.emoji || '🍽️'
const cuisineEnumName = (type) => CUISINE_DISPLAY[type]?.enumName || null

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
const CategoryIcon = ({ imageUrl, emoji }) => {
  const [imgError, setImgError] = useState(false)
  
  if (!imageUrl || imgError) {
    return <span className="group-hover:scale-110 transition-transform">{emoji}</span>
  }
  
  return (
    <img 
      src={imageUrl} 
      alt="category" 
      className="w-14 h-14 object-contain group-hover:scale-110 transition-transform drop-shadow-sm" 
      onError={() => setImgError(true)} 
    />
  )
}

const SkeletonCard = ({ className = '' }) => (
  <div className={`rounded-xl skeleton-shimmer ${className}`} />
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
      <div className="absolute top-full left-0 right-0 mt-2 glass-panel-premium rounded-2xl shadow-soft-xl z-50 overflow-hidden animate-fade-in-up">
        <div className="p-4 space-y-3">
          {[1, 2, 3].map((i) => (
            <div key={i} className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl skeleton-shimmer flex-shrink-0" />
              <div className="flex-1 space-y-1.5">
                <div className="h-3 skeleton-shimmer rounded w-3/4" />
                <div className="h-2.5 skeleton-shimmer rounded w-1/2" />
              </div>
            </div>
          ))}
        </div>
      </div>
    )
  }

  if (!hasResults && query.length >= 2) {
    return (
      <div className="absolute top-full left-0 right-0 mt-2 glass-panel-premium rounded-2xl shadow-soft-xl z-50 p-6 text-center animate-fade-in-up">
        <span className="material-symbols-outlined text-3xl text-slate-300 block mb-2">search_off</span>
        <p className="text-sm font-medium text-on-surface-variant">No results for "{query}"</p>
      </div>
    )
  }

  if (!hasResults) return null

  return (
    <div className="absolute top-full left-0 right-0 mt-2 glass-panel-premium rounded-2xl shadow-soft-xl z-50 overflow-hidden max-h-[420px] overflow-y-auto animate-fade-in-up">
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
  const { user } = useAuth()
  
  const greeting = getHybridGreeting(user?.role, user?.name || user?.email)

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

  const [isVegetarianOnly, setIsVegetarianOnly] = useState(false)

  // Fetch restaurants from backend when filters change
  useEffect(() => {
    let active = true
    setLoading(true)
    setError('')
    
    // Construct params for backend filtering
    const params = {}
    if (activeCuisine !== null) {
      const enumName = cuisineEnumName(activeCuisine)
      if (enumName) params.Cuisine = enumName
    }
    if (isVegetarianOnly) {
      params.IsVegetarianOnly = true
    }
    // We keep 'query' filtering client-side for immediate responsiveness of the grid, 
    // or we could pass it if we hit "enter". Let's pass it if there's a hard search.

    catalogApi.getRestaurants(params)
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
  }, [activeCuisine, isVegetarianOnly])

  // Live search with 300ms debounce (for dropdown)
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

  // The backend handles Cuisine and Vegetarian filtering.
  // We apply the text search 'query' filtering client-side on the returned results for smooth UI.
  const filtered = useMemo(() => {
    return restaurantsData.filter((r) => {
      const queryMatch = !query || r.name.toLowerCase().includes(query.toLowerCase()) || r.cuisineLabel.toLowerCase().includes(query.toLowerCase())
      return queryMatch
    })
  }, [restaurantsData, query])

  // Hardcode available cuisines so the category switcher doesn't disappear when filtered
  const availableCuisines = useMemo(() => {
    return Object.keys(CUISINE_DISPLAY)
      .filter(k => k !== '0') // Remove 'All'
      .map(k => Number(k))
  }, [])

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
    <div className="bg-transparent min-h-screen">
      {/* ── Hero Search Section ── */}
      <section 
        className="relative px-6 pt-10 pb-10 bg-slate-900 overflow-hidden"
        style={{
          backgroundImage: `url('/images/home_hero.png')`,
          backgroundSize: 'cover',
          backgroundPosition: 'center',
        }}
      >
        {/* Smooth gradient overlay so text remains perfectly readable on the left */}
        <div className="absolute inset-0 bg-gradient-to-r from-slate-900/95 via-slate-900/70 to-transparent" />

        <div className="max-w-7xl mx-auto relative z-10">
          <div className="inline-flex items-center gap-2 bg-white/10 backdrop-blur-sm px-3 py-1.5 rounded-full border border-white/20 mb-5">
            <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse" />
            <span className="text-xs font-semibold text-white/90">
              {filtered.length > 0
                ? `${filtered.length} restaurant${filtered.length !== 1 ? 's' : ''} near you`
                : restaurantsData.length > 0
                ? `${restaurantsData.length} restaurants available`
                : 'Discovering restaurants near you...'}
            </span>
          </div>
          <h1 className="text-4xl lg:text-5xl font-extrabold text-white leading-tight max-w-xl mb-3">
            {user ? (
              <>
                {greeting.main} <span className="text-yellow-400">{greeting.sub}</span>
              </>
            ) : (
              <>
                What are you <span className="text-yellow-400">craving</span> today?
              </>
            )}
          </h1>
          <p className="text-white/70 text-base mb-7">
            {user ? `Welcome back to QuickBite! Discover top-rated restaurants near you.` : `Discover top-rated restaurants and order in minutes.`}
          </p>

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
              className="block w-full pl-12 pr-32 py-4 bg-white border border-slate-300 rounded-full text-base text-slate-900 placeholder:text-slate-500 focus:ring-2 focus:ring-primary/20 focus:border-primary shadow-sm transition-shadow outline-none"
            />
            <button
              type="submit"
              className="absolute inset-y-2 right-2 px-6 bg-primary text-white rounded-full text-sm font-bold hover:bg-rose-500 hover:shadow-lg active:scale-95 transition-all"
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

          {/* Quick Filters */}
          <div className="mt-5 flex flex-wrap gap-3">
            <button
              onClick={() => setIsVegetarianOnly(!isVegetarianOnly)}
              className={`px-4 py-2 rounded-full text-sm font-semibold flex items-center gap-2 border transition-all ${
                isVegetarianOnly
                  ? 'bg-green-100 border-green-200 text-green-800'
                  : 'bg-white/10 border-white/20 text-white hover:bg-white/20'
              }`}
            >
              <span className={`inline-flex h-4 w-4 items-center justify-center rounded-sm border flex-shrink-0 ${isVegetarianOnly ? 'border-green-600' : 'border-white/60'}`}>
                <span className={`h-2 w-2 rounded-full ${isVegetarianOnly ? 'bg-green-600' : 'bg-transparent'}`} />
              </span>
              Vegetarian Only
            </button>
          </div>
        </div>
      </section>

      {/* ── Cuisine Carousel ── */}
      {availableCuisines.length > 0 && (
        <section className="py-8 bg-slate-50">
          <div className="px-6 max-w-7xl mx-auto mb-5">
            <h2 className="text-xl font-bold text-slate-900">Browse by Category</h2>
          </div>
          <div className="flex gap-4 overflow-x-auto no-scrollbar px-6 max-w-7xl mx-auto snap-x py-4">
            {/* All button */}
            <button
              onClick={() => setActiveCuisine(null)}
              className={`flex flex-col items-center gap-3 snap-start group active:scale-95 transition-transform flex-shrink-0`}
            >
              <div className={`w-16 h-16 rounded-2xl flex items-center justify-center text-3xl transition-all ${
                activeCuisine === null
                  ? 'bg-white ring-2 ring-primary ring-offset-2 shadow-lg shadow-primary/20 scale-110'
                  : 'bg-white shadow-sm border border-slate-100 group-hover:shadow-md group-hover:-translate-y-1'
              }`}>
                <CategoryIcon imageUrl={CUISINE_DISPLAY[0].imageUrl} emoji={CUISINE_DISPLAY[0].emoji} />
              </div>
              <span className={`text-xs font-bold ${ activeCuisine === null ? 'text-primary' : 'text-slate-600' }`}>All</span>
            </button>

            {availableCuisines.map((type) => {
              const meta = CUISINE_DISPLAY[type] || { label: 'Other', emoji: '🍽️' }
              const isActive = activeCuisine === type
              return (
                <button
                  key={type}
                  onClick={() => setActiveCuisine(isActive ? null : type)}
                  className={`flex flex-col items-center gap-3 snap-start group active:scale-95 transition-transform flex-shrink-0`}
                >
                  <div className={`w-16 h-16 rounded-2xl flex items-center justify-center text-3xl transition-all ${
                    isActive
                      ? `bg-white ring-2 ring-primary ring-offset-2 shadow-lg shadow-primary/20 scale-110`
                      : `bg-white shadow-sm border border-slate-100 group-hover:shadow-md group-hover:-translate-y-1`
                  }`}>
                    <CategoryIcon imageUrl={meta.imageUrl} emoji={meta.emoji} />
                  </div>
                  <span className={`text-xs font-bold ${ isActive ? 'text-primary' : 'text-slate-600' }`}>{meta.label}</span>
                </button>
              )
            })}
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
                  className="md:col-span-2 relative rounded-xl overflow-hidden bg-white shadow-sm border border-slate-200 group cursor-pointer hover:shadow-md transition-shadow h-[300px]"
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
                  <div className="absolute top-4 right-4 bg-white px-3 py-1 rounded-full flex items-center gap-1 shadow-sm">
                    <StarIcon />
                    <span className="text-sm font-semibold text-on-surface">{featured.rating}</span>
                  </div>
                  <div className="absolute bottom-0 left-0 p-6 w-full">
                    <h3 className="text-2xl font-bold text-white mb-2">{featured.name}</h3>
                    <div className="flex flex-wrap gap-3 items-center">
                      <span className="bg-white/20 px-3 py-1 rounded-full text-xs text-white flex items-center gap-1">
                        <span className="material-symbols-outlined text-sm">storefront</span>
                        {featured.cuisineLabel}
                      </span>
                      {featured.deliveryTime && (
                        <span className="bg-white/20 px-3 py-1 rounded-full text-xs text-white flex items-center gap-1">
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
                      className="flex-1 relative rounded-xl overflow-hidden bg-white shadow-sm border border-slate-200 group cursor-pointer hover:shadow-md transition-shadow"
                    >
                      {r.imageUrl ? (
                        <img src={r.imageUrl} alt={r.name} className="absolute inset-0 w-full h-full object-cover transition-transform duration-500 group-hover:scale-105" />
                      ) : (
                        <div className="absolute inset-0 bg-gradient-to-br from-primary/20 to-slate-600 flex items-center justify-center text-4xl">
                          {cuisineEmoji(r.cuisineType)}
                        </div>
                      )}
                      <div className="absolute inset-0 bg-gradient-to-t from-black/70 to-transparent" />
                      <div className="absolute top-3 right-3 bg-white px-2 py-0.5 rounded-full flex items-center gap-1">
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

          </>
        )}
      </section>

      <AppFeaturesSection />

      {/* ── Footer ── */}
      <footer className="bg-slate-900 py-12">
        <div className="flex flex-col md:flex-row justify-between items-center px-8 max-w-7xl mx-auto gap-6 text-sm">
          <div className="flex items-center gap-2.5">
            <img src="/quickbite-logo.png" alt="QuickBite Logo" className="w-11 h-11 object-contain" />
            <span className="text-xl font-extrabold text-white">QuickBite</span>
          </div>
          <div className="flex gap-6">
            {['Privacy Policy', 'Terms of Service', 'Help Center', 'Contact Us'].map((l) => (
              <a key={l} href="#" className="text-slate-400 hover:text-white transition-colors">{l}</a>
            ))}
          </div>
          <p className="text-slate-500">© {new Date().getFullYear()} QuickBite. All rights reserved.</p>
        </div>
      </footer>
    </div>
  )
}
