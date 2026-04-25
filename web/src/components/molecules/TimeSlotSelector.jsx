export const TimeSlotSelector = ({ slots = [], selected, onSelect }) => {
  return (
    <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
      {slots.map((slot) => (
        <button
          key={slot.id}
          type="button"
          onClick={() => onSelect?.(slot)}
          className={`rounded-xl border px-3 py-2 text-sm ${selected?.id === slot.id ? 'border-primary bg-primary text-on-primary' : 'border-outline bg-surface'}`}
        >
          {slot.label}
        </button>
      ))}
    </div>
  )
}

export default TimeSlotSelector
