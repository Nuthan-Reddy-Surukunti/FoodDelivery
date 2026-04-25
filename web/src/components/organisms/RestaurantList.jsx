import { RestaurantCard } from '../molecules/RestaurantCard'

export const RestaurantList = ({ restaurants = [], onOpenRestaurant }) => {
  if (!restaurants.length) {
    return <p className="text-sm text-on-background/70">No restaurants found.</p>
  }

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {restaurants.map((restaurant) => (
        <RestaurantCard
          key={restaurant.id}
          restaurant={restaurant}
          onOpen={onOpenRestaurant}
        />
      ))}
    </div>
  )
}

export default RestaurantList
