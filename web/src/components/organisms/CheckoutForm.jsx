import { useState, useEffect } from 'react'
import { AddressForm } from './AddressForm'
import orderApi from '../../services/orderApi'
import { useNotification } from '../../hooks/useNotification'

export const CheckoutForm = ({ onCheckoutDataChange }) => {
  const [addresses, setAddresses] = useState([])
  const [selectedAddressId, setSelectedAddressId] = useState(null)
  const [selectedDeliveryOption, setSelectedDeliveryOption] = useState('standard')
  const [loadingAddresses, setLoadingAddresses] = useState(true)
  const [showAddressForm, setShowAddressForm] = useState(false)
  const [isSavingAddress, setIsSavingAddress] = useState(false)
  const { showSuccess, showError } = useNotification()

  useEffect(() => {
    loadAddresses()
  }, [])

  useEffect(() => {
    onCheckoutDataChange?.({
      addressId: selectedAddressId,
      deliveryOption: selectedDeliveryOption,
    })
  }, [selectedAddressId, selectedDeliveryOption, onCheckoutDataChange])

  const loadAddresses = async () => {
    try {
      setLoadingAddresses(true)
      const data = await orderApi.getAddresses()
      setAddresses(data || [])
      if (data && data.length > 0) {
        const defaultAddress = data.find(a => a.isDefault)
        setSelectedAddressId(defaultAddress?.id || data[0].id)
      } else {
        setShowAddressForm(true)
      }
    } catch (error) {
      console.error('Failed to load addresses:', error)
      showError('Failed to load addresses')
    } finally {
      setLoadingAddresses(false)
    }
  }

  const handleAddressSubmit = async (formData) => {
    try {
      setIsSavingAddress(true)
      const newAddress = await orderApi.createAddress(formData)
      showSuccess('Address created successfully')
      
      const data = await orderApi.getAddresses()
      setAddresses(data || [])
      
      const selectedId = newAddress?.id || (data && data.length > 0 ? data[data.length - 1].id : null)
      if (selectedId) {
        setSelectedAddressId(selectedId)
      }
      
      setShowAddressForm(false)
    } catch (error) {
      showError(error.message || 'Failed to save address')
    } finally {
      setIsSavingAddress(false)
    }
  }

  const selectedAddressObj = addresses.find(a => a.id === selectedAddressId)

  const addressIcon = (label) => {
    const lower = (label || '').toLowerCase()
    if (lower.includes('work') || lower.includes('office')) return 'work'
    return 'home'
  }

  return (
    <div className="space-y-6">
      <section className="bg-surface-container-lowest rounded-xl p-6 shadow-sm border border-slate-100">
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-title-lg font-semibold text-on-surface">Delivery Address</h2>
          {addresses.length > 0 && !showAddressForm && (
            <button
              type="button"
              onClick={() => setShowAddressForm(true)}
              className="inline-flex items-center gap-1.5 text-primary text-sm font-semibold hover:opacity-80 transition-opacity"
            >
              <span className="material-symbols-outlined text-base">add</span>
              Add New
            </button>
          )}
        </div>

        {loadingAddresses && (
          <div className="text-sm text-on-surface-variant">Loading addresses...</div>
        )}

        {!loadingAddresses && showAddressForm && (
          <AddressForm
            onSubmit={handleAddressSubmit}
            onCancel={addresses.length > 0 ? () => setShowAddressForm(false) : undefined}
            loading={isSavingAddress}
          />
        )}

        {!loadingAddresses && !showAddressForm && addresses.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {addresses.map((address) => {
              const isSelected = selectedAddressId === address.id
              return (
                <button
                  key={address.id}
                  type="button"
                  onClick={() => setSelectedAddressId(address.id)}
                  className={`relative text-left p-4 rounded-xl transition-colors border ${isSelected ? 'border-2 border-primary bg-surface-bright' : 'border border-outline-variant bg-surface-container-lowest hover:bg-surface-container-low'}`}
                >
                  <div className="absolute top-4 right-4">
                    <span className="material-symbols-outlined text-primary" style={{ fontVariationSettings: isSelected ? "'FILL' 1" : "'FILL' 0" }}>
                      {isSelected ? 'radio_button_checked' : 'radio_button_unchecked'}
                    </span>
                  </div>

                  <div className="flex items-center gap-2 mb-2">
                    <span className="material-symbols-outlined text-on-surface-variant text-[20px]">{addressIcon(address.label)}</span>
                    <span className="font-semibold text-sm text-on-surface">{address.label || 'Address'}</span>
                    {address.isDefault && (
                      <span className="px-2 py-0.5 rounded-full bg-primary/10 text-primary text-xs font-medium">Default</span>
                    )}
                  </div>

                  <p className="text-sm text-on-surface-variant leading-6">
                    {address.street}, {address.city}, {address.state} {address.pinCode}
                  </p>
                </button>
              )
            })}
          </div>
        )}

        {!loadingAddresses && !showAddressForm && addresses.length === 0 && (
          <div className="rounded-xl border border-outline-variant bg-surface-container-low p-4">
            <p className="text-sm text-on-surface-variant mb-3">No saved addresses. Add one to continue.</p>
            <button
              type="button"
              onClick={() => setShowAddressForm(true)}
              className="inline-flex items-center gap-1.5 text-primary text-sm font-semibold hover:opacity-80 transition-opacity"
            >
              <span className="material-symbols-outlined text-base">add</span>
              Add Address
            </button>
          </div>
        )}
      </section>

      <section className="bg-surface-container-lowest rounded-xl p-6 shadow-sm border border-slate-100">
        <h2 className="text-title-lg font-semibold text-on-surface mb-6">Delivery Options</h2>
        <div className="space-y-4">
          <label className="flex items-start gap-4 p-4 rounded-xl border border-outline-variant bg-surface-container-lowest cursor-pointer hover:bg-surface-container-low transition-colors">
            <input
              className="mt-1 text-primary focus:ring-primary border-outline"
              name="delivery_option"
              type="radio"
              checked={selectedDeliveryOption === 'standard'}
              onChange={() => setSelectedDeliveryOption('standard')}
            />
            <div className="flex-1">
              <div className="flex justify-between items-center mb-1">
                <span className="font-semibold text-sm text-on-surface">Standard Delivery</span>
                <span className="font-semibold text-sm text-on-surface">Free</span>
              </div>
              <p className="text-sm text-on-surface-variant">Estimated arrival: 30-45 mins</p>
            </div>
          </label>

          <label className="flex items-start gap-4 p-4 rounded-xl border border-outline-variant bg-surface-container-lowest cursor-pointer hover:bg-surface-container-low transition-colors">
            <input
              className="mt-1 text-primary focus:ring-primary border-outline"
              name="delivery_option"
              type="radio"
              checked={selectedDeliveryOption === 'priority'}
              onChange={() => setSelectedDeliveryOption('priority')}
            />
            <div className="flex-1">
              <div className="flex justify-between items-center mb-1">
                <span className="font-semibold text-sm text-on-surface">Priority Delivery</span>
                <span className="font-semibold text-sm text-on-surface">+₹49</span>
              </div>
              <p className="text-sm text-on-surface-variant">Delivered directly to you. Est: 20-30 mins</p>
            </div>
          </label>
        </div>
      </section>
    </div>
  )
}

export default CheckoutForm
