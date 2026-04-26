import { useCallback, useEffect, useRef, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import catalogApi from '../services/catalogApi'

const CUISINE_DISPLAY = {
  0: 'All', 1: 'Italian', 2: 'Chinese', 3: 'Indian',
  4: 'Japanese', 5: 'Mexican', 6: 'American', 7: 'Thai',
  8: 'Mediterranean', 9: 'Other',
}
const cuisineLabel = (type) => CUISINE_DISPLAY[type] || (typeof type === 'string' ? type : 'Other')
const cuisineEmoji = (t) => ['🍽️','🍕','🥢','🍛','🍱','🌮','🍔','🍜','🥙','🍽️'][Number(t)] || '🍽️'

const StarIcon = () => (
  <span className="material-symbols-outlined text-yellow-400" style={{ fontSize: 13, fontVariationSettings: "'FILL' 1" }}>star</span>
)

// ── Restaurant result card
const RestaurantCard = ({ r, onClick }) => (
  <div
    onClick={onClick}
    className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden group cursor-pointer hover:shadow-md hover:border-primary/30 transition-all"
  >
    <div className="relative h-36 bg-slate-100">
      {r.imageUrl ? (
        <img src={r.imageUrl} alt={r.name} className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500" />
      ) : (
        <div className="w-full h-full flex items-center justify-center text-5xl bg-gradient-to-br from-primary/10 to-slate-200">
          {cuisineEmoji(r.cuisineType)}
        </div>
      )}
      <div className="absolute inset-0 bg-gradient-to-t from-black/40 to-transparent" />
      <div className="absolute bottom-2 left-3 flex items-center gap-1 bg-white/90 backdrop-blur-sm px-2 py-0.5 rounded-full">
        <StarIcon />
        <span className="text-xs font-semibold text-on-surface">{Number(r.rating ?? r.averageRating ?? 0).toFixed(1)}</span>
      </div>
    </div>
    <div className="p-4">
      <h3 className="font-semibold text-on-surface text-sm mb-1 truncate">{r.name}</h3>
      <p className="text-xs text-on-surface-variant">
        {cuisineLabel(r.cuisineType)}{r.city ? ` · ${r.city}` : ''}{r.deliveryTime ? ` · ${r.deliveryTime} min` : ''}
      </p>
    </div>
  </div>
)

// ── Menu item result card
const MenuItemCard = ({ item, onClick }) => (
  <div
    onClick={onClick}
    className="bg-white rounded-xl border border-slate-100 shadow-sm overflow-hidden group cursor-pointer hover:shadow-md hover:border-primary/30 transition-all flex items-center gap-4 p-4"
  >
    <div className="w-16 h-16 rounded-xl bg-slate-100 flex items-center justify-center text-2xl flex-shrink-0 overflow-hidden">
      {item.imageUrl
        ? <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
        : '🍽️'}
    </div>
    <div className="flex-1 min-w-0">
      <div className="flex items-center gap-1.5 mb-0.5">
        {item.isVeg && (
          <span className="inline-flex h-3.5 w-3.5 items-center justify-center rounded-sm border border-green-600 flex-shrink-0">
            <span className="h-1.5 w-1.5 rounded-full bg-green-600" />
          </span>
        )}
        <h3 className="font-semibold text-on-surface text-sm truncate">{item.name}</h3>
      </div>
      {item.description && (
        <p className="text-xs text-on-surface-variant truncate mb-1">{item.description}</p>
      )}
      <p className="text-xs text-on-surface-variant flex items-center gap-1">
        <span className="material-symbols-outlined text-xs">storefront</span>
        {item.restaurantName}
        {item.categoryName && <span className="ml-1 opacity-70">· {item.categoryName}</span>}
      </p>
    </div>
    <div className="flex-shrink-0 text-right">
      <p className="text-base font-bold text-primary">₹{Number(item.price).toFixed(0)}</p>
      <p className="text-xs text-primary mt-1 flex items-center gap-0.5 justify-end group-hover:underline">
        View <span className="material-symbols-outlined text-xs">arrow_forward</span>
      </p>
    </div>
  </div>
)

// ── Empty state
const EmptyState = ({ query, tab }) => (
  <div className="py-16 text-center">
    <span className="material-symbols-outlined text-5xl text-slate-300 block mb-3">
      {tab === 'restaurants' ? 'storefront' : 'restaurant_menu'}
    </span>
    <p className="text-lg font-semibold text-on-surface mb-1">No {tab} found</p>
    <p className="text-sm text-on-surface-variant">
      No {tab} matched "<span className="font-medium">{query}</span>"
    </p>
  </div>
)

// ── Main Page ──────────────────────────────────────────────────────────────────
export const SearchResultsPage = () => {
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()
  const initialQ = searchParams.get('q') || ''

  const [inputValue, setInputValue] = useState(initialQ)
  const [activeTab, setActiveTab] = useState('restaurants')
  const [results, setResults] = useState({ restaurants: [], menuItems: [] })
  const [loading, setLoading] = useState(false)
  const debounceRef = useRef(null)

  const doSearch = useCallback(async (q) => {
    if (!q || q.length < 2) { setResults({ restaurants: [], menuItems: [] }); return }
    setLoading(true)
    try {
      const data = await catalogApi.searchAll(q)
      setResults(data)
    } catch {
      setResults({ restaurants: [], menuItems: [] })
    } finally {
      setLoading(false)
    }
  }, [])

  // Initial search from URL param
  useEffect(() => { if (initialQ) doSearch(initialQ) }, []) // eslint-disable-line

  const handleInputChange = (e) => {
    const val = e.target.value
    setInputValue(val)
    clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => {
      setSearchParams({ q: val })
      doSearch(val)
    }, 300)
  }

  const handleSubmit = (e) => {
    e.preventDefault()
    clearTimeout(debounceRef.current)
    setSearchParams({ q: inputValue })
    doSearch(inputValue)
  }

  const query = searchParams.get('q') || inputValue
  const totalCount = results.restaurants.length + results.menuItems.length

  const tabs = [
    { key: 'restaurants', label: 'Restaurants', count: results.restaurants.length, icon: 'storefront' },
    { key: 'menuItems', label: 'Menu Items', count: results.menuItems.length, icon: 'restaurant_menu' },
  ]

  return (
    <div className="min-h-screen bg-background">
      {/* ── Search Header ── */}
      <div className="bg-white border-b border-slate-100 sticky top-0 z-30 shadow-sm">
        <div className="max-w-5xl mx-auto px-6 py-4">
          <form onSubmit={handleSubmit} className="relative">
            <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-on-surface-variant text-xl pointer-events-none">search</span>
            <input
              id="search-results-input"
              type="text"
              value={inputValue}
              onChange={handleInputChange}
              placeholder="Search restaurants or dishes..."
              autoComplete="off"
              className="w-full pl-12 pr-28 py-3 bg-slate-50 border border-slate-200 rounded-full text-base text-on-surface focus:ring-2 focus:ring-primary focus:border-primary outline-none transition"
            />
            <button
              type="submit"
              className="absolute right-2 top-1/2 -translate-y-1/2 bg-primary text-on-primary px-5 py-1.5 rounded-full text-sm font-semibold hover:bg-primary-container transition-colors"
            >
              Search
            </button>
          </form>
        </div>
      </div>

      <div className="max-w-5xl mx-auto px-6 py-6">
        {/* Results summary */}
        {query && !loading && (
          <p className="text-sm text-on-surface-variant mb-4">
            {totalCount > 0
              ? <><span className="font-semibold text-on-surface">{totalCount}</span> results for "<span className="font-medium">{query}</span>"</>
              : <>No results for "<span className="font-medium">{query}</span>"</>}
          </p>
        )}

        {/* Tabs */}
        {!loading && totalCount > 0 && (
          <div className="flex gap-1 bg-slate-100 rounded-xl p-1 w-fit mb-6">
            {tabs.map((tab) => (
              <button
                key={tab.key}
                onClick={() => setActiveTab(tab.key)}
                className={`flex items-center gap-2 px-5 py-2 rounded-lg text-sm font-semibold transition-all ${
                  activeTab === tab.key
                    ? 'bg-white text-on-surface shadow-sm'
                    : 'text-on-surface-variant hover:text-on-surface'
                }`}
              >
                <span className="material-symbols-outlined text-base">{tab.icon}</span>
                {tab.label}
                <span className={`text-xs px-1.5 py-0.5 rounded-full font-medium ${
                  activeTab === tab.key ? 'bg-primary text-on-primary' : 'bg-slate-200 text-on-surface-variant'
                }`}>{tab.count}</span>
              </button>
            ))}
          </div>
        )}

        {/* Loading skeleton */}
        {loading && (
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
            {[1, 2, 3, 4, 5, 6].map((i) => (
              <div key={i} className="bg-white rounded-xl border border-slate-100 overflow-hidden">
                <div className="h-36 bg-slate-200 animate-pulse" />
                <div className="p-4 space-y-2">
                  <div className="h-3 bg-slate-200 animate-pulse rounded w-3/4" />
                  <div className="h-3 bg-slate-100 animate-pulse rounded w-1/2" />
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Restaurants tab */}
        {!loading && activeTab === 'restaurants' && (
          results.restaurants.length > 0 ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
              {results.restaurants.map((r) => (
                <RestaurantCard
                  key={r.id}
                  r={r}
                  onClick={() => navigate(`/restaurant/${r.id}`)}
                />
              ))}
            </div>
          ) : query && <EmptyState query={query} tab="restaurants" />
        )}

        {/* Menu items tab */}
        {!loading && activeTab === 'menuItems' && (
          results.menuItems.length > 0 ? (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {results.menuItems.map((item) => (
                <MenuItemCard
                  key={item.id}
                  item={item}
                  onClick={() => navigate(`/restaurant/${item.restaurantId}`)}
                />
              ))}
            </div>
          ) : query && <EmptyState query={query} tab="menu items" />
        )}

        {/* Initial empty state (no query) */}
        {!loading && !query && (
          <div className="py-24 text-center">
            <span className="material-symbols-outlined text-6xl text-slate-200 block mb-4">search</span>
            <p className="text-lg font-semibold text-on-surface mb-1">Search for anything</p>
            <p className="text-sm text-on-surface-variant">Find restaurants by name, cuisine, or search for specific dishes</p>
          </div>
        )}
      </div>
    </div>
  )
}
