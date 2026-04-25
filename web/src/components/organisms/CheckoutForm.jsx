import { useMemo, useState } from 'react'
import { Button } from '../atoms/Button'
import { AddressField } from '../molecules/AddressField'
import { StepperIndicator } from '../molecules/StepperIndicator'
import { TimeSlotSelector } from '../molecules/TimeSlotSelector'

const steps = ['Address', 'Time Slot', 'Review']

export const CheckoutForm = ({ timeSlots = [], onSubmit }) => {
  const [currentStep, setCurrentStep] = useState(0)
  const [address, setAddress] = useState('')
  const [selectedSlot, setSelectedSlot] = useState(null)

  const canProceed = useMemo(() => {
    if (currentStep === 0) return address.trim().length > 5
    if (currentStep === 1) return !!selectedSlot
    return true
  }, [currentStep, address, selectedSlot])

  const next = () => setCurrentStep((s) => Math.min(s + 1, steps.length - 1))
  const back = () => setCurrentStep((s) => Math.max(s - 1, 0))

  const handleSubmit = () => {
    onSubmit?.({ address, slot: selectedSlot })
  }

  return (
    <div className="space-y-4 rounded-2xl border border-outline p-4">
      <StepperIndicator steps={steps} currentStep={currentStep} />

      {currentStep === 0 ? (
        <AddressField value={address} onChange={(e) => setAddress(e.target.value)} />
      ) : null}

      {currentStep === 1 ? (
        <TimeSlotSelector slots={timeSlots} selected={selectedSlot} onSelect={setSelectedSlot} />
      ) : null}

      {currentStep === 2 ? (
        <div className="rounded-xl bg-surface-dim p-3 text-sm">
          <p><span className="font-semibold">Address:</span> {address}</p>
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
