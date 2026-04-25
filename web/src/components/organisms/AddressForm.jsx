import { useState, useEffect } from 'react'
import { Button } from '../atoms/Button'
import { FormField } from '../molecules/FormField'
import { Card } from '../atoms/Card'

export const AddressForm = ({ address = null, onSubmit, onCancel, loading = false }) => {
  const [formData, setFormData] = useState({
    label: '',
    street: '',
    city: '',
    state: '',
    pinCode: '',
    isDefault: false,
  })

  const [errors, setErrors] = useState({})

  useEffect(() => {
    if (address) {
      setFormData(address)
    }
  }, [address])

  const validateForm = () => {
    const newErrors = {}
    if (!formData.label.trim()) newErrors.label = 'Label is required'
    if (!formData.street.trim()) newErrors.street = 'Street is required'
    if (!formData.city.trim()) newErrors.city = 'City is required'
    if (!formData.state.trim()) newErrors.state = 'State is required'
    if (!formData.pinCode.trim()) newErrors.pinCode = 'PIN code is required'
    if (!/^\d{6}$/.test(formData.pinCode)) newErrors.pinCode = 'PIN code must be 6 digits'
    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target
    setFormData({
      ...formData,
      [name]: type === 'checkbox' ? checked : value,
    })
  }

  const handleSubmit = (e) => {
    e.preventDefault()
    if (validateForm()) {
      onSubmit(formData)
    }
  }

  return (
    <Card className="p-6">
      <h2 className="mb-4 text-xl font-bold">
        {address ? 'Edit Address' : 'Add New Address'}
      </h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <FormField
          label="Address Label"
          name="label"
          placeholder="e.g., Home, Office"
          value={formData.label}
          onChange={handleChange}
          error={errors.label}
          required
        />

        <FormField
          label="Street Address"
          name="street"
          placeholder="House no, street, landmark"
          value={formData.street}
          onChange={handleChange}
          error={errors.street}
          required
        />

        <div className="grid grid-cols-2 gap-4">
          <FormField
            label="City"
            name="city"
            placeholder="City"
            value={formData.city}
            onChange={handleChange}
            error={errors.city}
            required
          />

          <FormField
            label="State"
            name="state"
            placeholder="State"
            value={formData.state}
            onChange={handleChange}
            error={errors.state}
            required
          />
        </div>

        <FormField
          label="PIN Code"
          name="pinCode"
          placeholder="6 digit PIN code"
          value={formData.pinCode}
          onChange={handleChange}
          error={errors.pinCode}
          required
        />

        <label className="flex cursor-pointer items-center gap-2">
          <input
            type="checkbox"
            name="isDefault"
            checked={formData.isDefault}
            onChange={handleChange}
            className="rounded border-outline"
          />
          <span className="text-sm font-medium">Set as default address</span>
        </label>

        <div className="flex gap-3 pt-4">
          <Button variant="secondary" onClick={onCancel} disabled={loading}>
            Cancel
          </Button>
          <Button type="submit" disabled={loading}>
            {loading ? 'Saving...' : address ? 'Update Address' : 'Add Address'}
          </Button>
        </div>
      </form>
    </Card>
  )
}

export default AddressForm
