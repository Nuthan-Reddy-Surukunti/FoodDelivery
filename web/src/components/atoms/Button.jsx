/**
 * Button Component
 * Flexible button with multiple variants and sizes
 */
export const Button = ({
  children,
  onClick,
  type = 'button',
  variant = 'primary', // primary, secondary, error, tertiary
  size = 'md', // sm, md, lg
  disabled = false,
  fullWidth = false,
  className = '',
  ...props
}) => {
  const baseStyles = 'font-medium rounded-2xl transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2'
  
  const variants = {
    primary: 'bg-primary text-on-primary hover:bg-primary-container disabled:bg-surface-dim disabled:text-on-background',
    secondary: 'bg-secondary-container text-on-secondary-container hover:bg-secondary disabled:bg-surface-dim disabled:text-on-background',
    error: 'bg-error text-on-error hover:bg-error disabled:bg-surface-dim disabled:text-on-background',
    tertiary: 'bg-transparent border-2 border-primary text-primary hover:bg-primary-fixed disabled:border-surface-dim disabled:text-on-background'
  }

  const sizes = {
    sm: 'px-4 py-2 text-sm',
    md: 'px-6 py-3 text-base',
    lg: 'px-8 py-4 text-lg'
  }

  const widthClass = fullWidth ? 'w-full' : ''

  return (
    <button
      type={type}
      onClick={onClick}
      disabled={disabled}
      className={`${baseStyles} ${variants[variant]} ${sizes[size]} ${widthClass} ${className}`}
      {...props}
    >
      {children}
    </button>
  )
}
