export const Loader = ({ size = 'md', className = '' }) => {
  const sizeClass = {
    sm: 'h-4 w-4 border-2',
    md: 'h-6 w-6 border-2',
    lg: 'h-10 w-10 border-4',
  }[size] || 'h-6 w-6 border-2'

  return (
    <span
      className={`inline-block animate-spin rounded-full border-primary border-r-transparent ${sizeClass} ${className}`}
      aria-label="Loading"
      role="status"
    />
  )
}

export default Loader
