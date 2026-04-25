export const Rating = ({ value = 0, outOf = 5 }) => {
  const stars = Array.from({ length: outOf }, (_, i) => i + 1)
  return (
    <div className="flex items-center gap-1" aria-label={`Rating ${value} out of ${outOf}`}>
      {stars.map((star) => (
        <span key={star} className={star <= Math.round(value) ? 'text-amber-500' : 'text-gray-300'}>
          ★
        </span>
      ))}
      <span className="ml-1 text-xs text-on-background/70">{value.toFixed?.(1) ?? value}</span>
    </div>
  )
}

export default Rating
