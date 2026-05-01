import PropTypes from 'prop-types'

export const CategoryFilter = ({ categories = [], selected = 'all', onSelect = () => {} }) => {
  return (
    <div className="flex flex-wrap gap-2">
      {categories.map(({ label, value }) => (
        <button
          key={value}
          onClick={() => onSelect(value)}
          className={`px-4 py-2 rounded-full text-sm font-medium transition-colors ${
            selected === value
              ? 'bg-primary text-on-primary'
              : 'bg-surface-container text-on-surface hover:bg-surface-container-high border border-outline-variant'
          }`}
        >
          {label}
        </button>
      ))}
    </div>
  )
}

CategoryFilter.propTypes = {
  categories: PropTypes.arrayOf(PropTypes.shape({
    label: PropTypes.string,
    value: PropTypes.string
  })),
  selected: PropTypes.string,
  onSelect: PropTypes.func,
}
