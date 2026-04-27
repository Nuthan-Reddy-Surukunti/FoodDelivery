import { useMemo, useState, useEffect } from 'react'
import { Button } from '../atoms/Button'
import { AddressForm } from './AddressForm'
import { AddressList } from './AddressList'
import { StepperIndicator } from '../molecules/StepperIndicator'
import orderApi from '../../services/orderApi'
import { useNotification } from '../../hooks/useNotification'

const steps = ['Address', 'Review']

export const CheckoutForm = ({ timeSlots = [], onSubmit, loading = false }) => {
  const [currentStep, setCurrentStep] = useState(0)
  const [addresses, setAddresses] = useState([])
  const [selectedAddressId, setSelectedAddressId] = useState(null)
  const [selectedSlot, setSelectedSlot] = useState(null)
  const [loadingAddresses, setLoadingAddresses] = useState(true)
  const [showAddressForm, setShowAddressForm] = useState(false)
  const [isSavingAddress, setIsSavingAddress] = useState(false)
  const { showSuccess, showError } = useNotification()

  useEffect(() => {
    loadAddresses()
  }, [])

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
  const finalAddress = selectedAddressObj
    ? `${selectedAddressObj.street}, ${selectedAddressObj.city}, ${selectedAddressObj.state} - ${selectedAddressObj.pinCode}`
    : ''

  const canProceed = useMemo(() => {
    if (currentStep === 0) return !!selectedAddressId && !showAddressForm
    return true
  }, [currentStep, selectedAddressId, showAddressForm])

  const next = () => setCurrentStep((s) => Math.min(s + 1, steps.length - 1))
  const back = () => setCurrentStep((s) => Math.max(s - 1, 0))

  const handleSubmit = () => {
    if (loading) return
    onSubmit?.({ 
      addressId: selectedAddressId,
      address: finalAddress, 
      slot: selectedSlot 
    })
  }

  return (
    <div className="space-y-4 rounded-2xl border border-outline p-4">
      <StepperIndicator steps={steps} currentStep={currentStep} />

      {currentStep === 0 ? (
        <div className="space-y-4">
          {!loadingAddresses && addresses.length > 0 && !showAddressForm && (
            <div>
              <div className="flex items-center justify-between mb-2">
                <h3 className="font-semibold">Select a Saved Address</h3>
                <Button variant="secondary" onClick={() => setShowAddressForm(true)}>
                  + Add New
                </Button>
              </div>
              <AddressList
                addresses={addresses}
                selectable
                selectedId={selectedAddressId}
                onSelect={(addr) => setSelectedAddressId(addr.id)}
              />
            </div>
          )}

          {showAddressForm && !loadingAddresses && (
            <div className="mb-4">
              <AddressForm 
                onSubmit={handleAddressSubmit} 
                onCancel={addresses.length > 0 ? () => setShowAddressForm(false) : undefined}
                loading={isSavingAddress} 
              />
            </div>
          )}

          {loadingAddresses && (
            <div className="text-center text-on-surface-variant">Loading addresses...</div>
          )}
        </div>
      ) : null}

      {currentStep === 1 ? (
        <div className="rounded-xl bg-surface-dim p-3 text-sm">
          <p><span className="font-semibold">Address:</span> {finalAddress}</p>
        </div>
      ) : null}

      <div className="flex justify-between">
        <Button variant="secondary" onClick={back} disabled={currentStep === 0 || loading}>Back</Button>
        {currentStep < steps.length - 1 ? (
          <Button onClick={next} disabled={!canProceed}>Next</Button>
        ) : (
          <Button onClick={handleSubmit} disabled={loading}>
            {loading ? 'Placing Order...' : 'Place Order'}
          </Button>
        )}
      </div>
    </div>
  )
}

export default CheckoutForm
