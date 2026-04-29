import PropTypes from 'prop-types'

const CATEGORIES = [
  { label: 'All Categories', value: 'all' },
  { label: 'Italian', value: 'italian' },
  { label: 'Asian', value: 'asian' },
  { label: 'American', value: 'american' },
  { label: 'Healthy', value: 'healthy' },
  { label: 'Mexican', value: 'mexican' },
  { label: 'Thai', value: 'thai' },
  { label: 'Japanese', value: 'japanese' },
]

export const CategoryFilter = ({ selected = 'all', onSelect = () => {} }) => {
  return (
    <div className="flex flex-wrap gap-2">
      {CATEGORIES.map(({ label, value }) => (
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
  selected: PropTypes.string,
  onSelect: PropTypes.func,
}
