/**
 * Input Component
 * Text input with support for labels, errors, and icons
 */
export const Input = ({
  type = 'text',
  placeholder = '',
  value,
  onChange,
  onBlur,
  label,
  error,
  disabled = false,
  required = false,
  className = '',
  ...props
}) => {
  return (
    <div className="w-full">
      {label && (
        <label className="block text-label-md text-on-background mb-2 font-medium">
          {label}
          {required && <span className="text-error ml-1">*</span>}
        </label>
      )}
      <input
        type={type}
        placeholder={placeholder}
        value={value}
        onChange={onChange}
        onBlur={onBlur}
        disabled={disabled}
        className={`
          w-full px-4 py-3 rounded-2xl border-2
          text-body-md text-on-background
          placeholder:text-outline
          border-outline
          bg-surface
          transition-all duration-200
          focus:outline-none focus:border-primary focus:ring-2 focus:ring-primary focus:ring-opacity-20
          disabled:bg-surface-dim disabled:text-on-background disabled:cursor-not-allowed
          ${error ? 'border-error focus:border-error focus:ring-error focus:ring-opacity-20' : ''}
          ${className}
        `}
        {...props}
      />
      {error && (
        <p className="text-error text-caption-sm mt-1 font-medium">{error}</p>
      )}
    </div>
  )
}
