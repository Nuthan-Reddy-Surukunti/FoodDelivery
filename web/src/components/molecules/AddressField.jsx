import { FormField } from './FormField'

export const AddressField = ({ value, onChange, error }) => {
  return (
    <FormField
      label="Delivery Address"
      name="address"
      value={value}
      onChange={onChange}
      error={error}
      required
      placeholder="House no, street, landmark"
    />
  )
}

export default AddressField
