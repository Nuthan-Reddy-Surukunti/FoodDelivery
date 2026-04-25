import { Card } from '../atoms/Card'
import { Rating } from './Rating'

export const RestaurantCard = ({ restaurant, onOpen }) => {
  return (
    <button type="button" className="w-full text-left" onClick={() => onOpen?.(restaurant)}>
      <Card className="h-full hover:shadow-md">
        <div className="mb-3 aspect-[16/9] w-full rounded-xl bg-surface-dim" />
        <h3 className="text-base font-semibold">{restaurant.name}</h3>
        <p className="text-sm text-on-background/70">{restaurant.cuisine}</p>
        <div className="mt-2 flex items-center justify-between">
          <Rating value={restaurant.rating ?? 4.2} />
          <span className="text-xs text-on-background/70">{restaurant.deliveryTime ?? '30-40 mins'}</span>
        </div>
      </Card>
    </button>
  )
}

export default RestaurantCard
