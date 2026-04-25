import { Label } from '../atoms/Label'
import { Input } from '../atoms/Input'

export const FormField = ({
  label,
  name,
  value,
  onChange,
  onBlur,
  error,
  required = false,
  type = 'text',
  placeholder,
}) => {
  return (
    <div>
      <Label htmlFor={name} required={required}>{label}</Label>
      <Input
        id={name}
        name={name}
        type={type}
        value={value}
        onChange={onChange}
        onBlur={onBlur}
        placeholder={placeholder}
        className={error ? 'border-error' : ''}
      />
      {error ? <p className="mt-1 text-xs text-error">{error}</p> : null}
    </div>
  )
}

export default FormField
