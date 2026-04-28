const CUISINE_EMOJI = {
  1: '🍕', 2: '🥢', 3: '🍛', 4: '🍱', 5: '🌮',
  6: '🍔', 7: '🍜', 8: '🥙', 9: '🍟', 10: '🥗', Other: '🍽️'
}

const GRADIENT_COLORS = [
  'from-orange-400 to-rose-500',
  'from-violet-500 to-purple-600',
  'from-sky-400 to-blue-600',
  'from-emerald-400 to-teal-600',
  'from-amber-400 to-orange-500',
  'from-pink-400 to-rose-500',
]

const getGradient = (id = '') =>
  GRADIENT_COLORS[id.charCodeAt(0) % GRADIENT_COLORS.length] || GRADIENT_COLORS[0]

export const RestaurantCard = ({ restaurant, onOpen }) => {
  const emoji = CUISINE_EMOJI[restaurant.cuisineType] || CUISINE_EMOJI[restaurant.cuisine] || '🍽️'
  const gradient = getGradient(restaurant.id)

  return (
    <button
      type="button"
      className="w-full text-left group focus:outline-none"
      onClick={() => onOpen?.(restaurant)}
    >
      <div className="rounded-2xl overflow-hidden bg-white shadow-sm border border-slate-100 hover:shadow-lg transition-all duration-300 hover:-translate-y-1">
        {/* Image area */}
        <div className="relative aspect-[16/9] w-full overflow-hidden">
          {restaurant.imageUrl ? (
            <img
              src={restaurant.imageUrl}
              alt={restaurant.name}
              className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
            />
          ) : (
            <div className={`w-full h-full bg-gradient-to-br ${gradient} flex items-center justify-center`}>
              <span className="text-5xl">{emoji}</span>
            </div>
          )}
          {/* Delivery time badge */}
          {restaurant.deliveryTime && restaurant.deliveryTime !== 'N/A' && (
            <div className="absolute bottom-2 right-2 bg-black/60 text-white text-xs font-semibold px-2 py-1 rounded-lg backdrop-blur-sm flex items-center gap-1">
              <span className="material-symbols-outlined text-[12px]">schedule</span>
              {restaurant.deliveryTime}
            </div>
          )}
        </div>

        {/* Info area */}
        <div className="p-4">
          <h3 className="text-base font-bold text-on-surface truncate">{restaurant.name}</h3>
          <p className="text-sm text-slate-500 mt-0.5">{restaurant.cuisine}{restaurant.city ? ` · ${restaurant.city}` : ''}</p>
          <div className="mt-2 flex items-center gap-1">
            <span className="material-symbols-outlined text-yellow-400 text-sm" style={{ fontVariationSettings: "'FILL' 1" }}>star</span>
            <span className="text-sm font-semibold text-on-surface">{Number(restaurant.rating || 0).toFixed(1)}</span>
          </div>
        </div>
      </div>
    </button>
  )
}

export default RestaurantCard
