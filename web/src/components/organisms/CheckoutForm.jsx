import { useMemo, useState, useEffect } from 'react'
import { Button } from '../atoms/Button'
import { AddressField } from '../molecules/AddressField'
import { AddressList } from './AddressList'
import { StepperIndicator } from '../molecules/StepperIndicator'
import { TimeSlotSelector } from '../molecules/TimeSlotSelector'
import orderApi from '../../services/orderApi'

const steps = ['Address', 'Time Slot', 'Review']

export const CheckoutForm = ({ timeSlots = [], onSubmit }) => {
  const [currentStep, setCurrentStep] = useState(0)
  const [addresses, setAddresses] = useState([])
  const [selectedAddressId, setSelectedAddressId] = useState(null)
  const [address, setAddress] = useState('')
  const [selectedSlot, setSelectedSlot] = useState(null)
  const [loadingAddresses, setLoadingAddresses] = useState(true)
  const [useCustomAddress, setUseCustomAddress] = useState(false)

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
      }
    } catch (error) {
      console.error('Failed to load addresses:', error)
    } finally {
      setLoadingAddresses(false)
    }
  }

  const selectedAddressObj = addresses.find(a => a.id === selectedAddressId)
  const selectedAddressText = selectedAddressObj
    ? `${selectedAddressObj.street}, ${selectedAddressObj.city}, ${selectedAddressObj.state} - ${selectedAddressObj.pinCode}`
    : ''

  const finalAddress = useCustomAddress ? address : selectedAddressText

  const canProceed = useMemo(() => {
    if (currentStep === 0) {
      if (useCustomAddress) {
        return address.trim().length > 5
      } else {
        return !!selectedAddressId
      }
    }
    if (currentStep === 1) return !!selectedSlot
    return true
  }, [currentStep, address, selectedSlot, selectedAddressId, useCustomAddress])

  const next = () => setCurrentStep((s) => Math.min(s + 1, steps.length - 1))
  const back = () => setCurrentStep((s) => Math.max(s - 1, 0))

  const handleSubmit = () => {
    onSubmit?.({ 
      addressId: useCustomAddress ? null : selectedAddressId,
      address: finalAddress, 
      slot: selectedSlot 
    })
  }

  return (
    <div className="space-y-4 rounded-2xl border border-outline p-4">
      <StepperIndicator steps={steps} currentStep={currentStep} />

      {currentStep === 0 ? (
        <div className="space-y-4">
          {!loadingAddresses && addresses.length > 0 && !useCustomAddress && (
            <div>
              <h3 className="mb-2 font-semibold">Select a Saved Address</h3>
              <AddressList
                addresses={addresses}
                selectable
                selectedId={selectedAddressId}
                onSelect={(addr) => setSelectedAddressId(addr.id)}
              />
            </div>
          )}

          {addresses.length > 0 && (
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="customAddress"
                checked={useCustomAddress}
                onChange={(e) => setUseCustomAddress(e.target.checked)}
                className="rounded border-outline"
              />
              <label htmlFor="customAddress" className="text-sm">Use different address</label>
            </div>
          )}

          {(useCustomAddress || addresses.length === 0) && (
            <AddressField 
              value={address} 
              onChange={(e) => setAddress(e.target.value)} 
            />
          )}

          {loadingAddresses && (
            <div className="text-center text-on-surface-variant">Loading addresses...</div>
          )}
        </div>
      ) : null}

      {currentStep === 1 ? (
        <TimeSlotSelector slots={timeSlots} selected={selectedSlot} onSelect={setSelectedSlot} />
      ) : null}

      {currentStep === 2 ? (
        <div className="rounded-xl bg-surface-dim p-3 text-sm">
          <p><span className="font-semibold">Address:</span> {finalAddress}</p>
          <p className="mt-1"><span className="font-semibold">Slot:</span> {selectedSlot?.label || 'Not selected'}</p>
          <p className="mt-2">Payment method: Cash on Delivery</p>
        </div>
      ) : null}

      <div className="flex justify-between">
        <Button variant="secondary" onClick={back} disabled={currentStep === 0}>Back</Button>
        {currentStep < steps.length - 1 ? (
          <Button onClick={next} disabled={!canProceed}>Next</Button>
        ) : (
          <Button onClick={handleSubmit}>Place Order</Button>
        )}
      </div>
    </div>
  )
}

export default CheckoutForm
