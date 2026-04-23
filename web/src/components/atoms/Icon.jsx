/**
 * Icon Component
 * Renders Material Symbols Outlined icons
 * Uses Google Material Symbols font
 */
export const Icon = ({
  name, // e.g., 'shopping_cart', 'person', 'search'
  size = 24, // icon size in pixels
  className = '',
  ...props
}) => {
  return (
    <span
      className={`material-symbols-outlined text-current ${className}`}
      style={{ fontSize: `${size}px`, lineHeight: `${size}px` }}
      {...props}
    >
      {name}
    </span>
  )
}
