export const CuisineChip = ({ label, active = false, onClick }) => {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`rounded-full border px-3 py-1 text-xs font-medium transition ${active ? 'border-primary bg-primary text-on-primary' : 'border-outline bg-surface hover:bg-surface-dim'}`}
    >
      {label}
    </button>
  )
}

export default CuisineChip
