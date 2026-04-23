/**
 * Badge Component
 * Small label component for status, tags, or counters
 */
export const Badge = ({
  children,
  variant = 'primary', // primary, secondary, error, success, info
  size = 'md', // sm, md, lg
  className = '',
  ...props
}) => {
  const baseStyles = 'inline-flex items-center justify-center font-medium rounded-full'

  const variants = {
    primary: 'bg-primary text-on-primary',
    secondary: 'bg-secondary-container text-on-secondary-container',
    error: 'bg-error-container text-on-error-container',
    success: 'bg-tertiary text-on-tertiary',
    info: 'bg-primary-fixed text-on-primary-fixed'
  }

  const sizes = {
    sm: 'px-2 py-1 text-caption-sm',
    md: 'px-3 py-1.5 text-label-md',
    lg: 'px-4 py-2 text-body-md'
  }

  return (
    <span
      className={`${baseStyles} ${variants[variant]} ${sizes[size]} ${className}`}
      {...props}
    >
      {children}
    </span>
  )
}
