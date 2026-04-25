import { MenuItem } from '../molecules/MenuItem'

export const MenuSection = ({ title, items = [], onAddToCart }) => {
  return (
    <section>
      <h3 className="mb-3 text-lg font-semibold">{title}</h3>
      <div className="space-y-3">
        {items.map((item) => (
          <MenuItem key={item.id} item={item} onAdd={onAddToCart} />
        ))}
      </div>
    </section>
  )
}

export default MenuSection
