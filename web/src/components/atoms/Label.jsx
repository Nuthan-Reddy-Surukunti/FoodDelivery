export const Label = ({ htmlFor, children, required = false, className = '' }) => {
  return (
    <label htmlFor={htmlFor} className={`mb-1 block text-sm font-medium text-on-background ${className}`}>
      {children}
      {required ? <span className="ml-1 text-error">*</span> : null}
    </label>
  )
}

export default Label
