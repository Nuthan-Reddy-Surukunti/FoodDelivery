export const Card = ({ children, className = '' }) => {
  return (
    <div className={`rounded-2xl border border-outline bg-surface p-4 shadow-sm ${className}`}>
      {children}
    </div>
  )
}

export default Card
