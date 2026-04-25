import { Button } from '../atoms/Button'
import { Card } from '../atoms/Card'

export const AddressList = ({ addresses = [], onEdit, onDelete, onSelect, selectable = false, selectedId = null }) => {
  const formatAddress = (address) => {
    const { street, city, state, pinCode } = address
    return `${street}, ${city}, ${state} - ${pinCode}`
  }

  if (addresses.length === 0) {
    return (
      <div className="rounded-xl border border-outline bg-surface-dim p-4 text-center text-on-surface-variant">
        <p>No saved addresses. Create one to get started.</p>
      </div>
    )
  }

  return (
    <div className="space-y-3">
      {addresses.map((address) => (
        <Card key={address.id} className="p-4">
          <div className="flex items-start justify-between">
            <div className="flex-1">
              <div className="flex items-center gap-2">
                <h3 className="font-semibold text-on-surface">{address.label}</h3>
                {address.isDefault && (
                  <span className="rounded-full bg-primary-container px-2 py-1 text-xs font-medium text-on-primary-container">
                    Default
                  </span>
                )}
              </div>
              <p className="mt-1 text-sm text-on-surface-variant">{formatAddress(address)}</p>
            </div>
            <div className="ml-4 flex gap-2">
              {selectable && (
                <Button
                  variant={selectedId === address.id ? 'primary' : 'secondary'}
                  size="sm"
                  onClick={() => onSelect?.(address)}
                >
                  {selectedId === address.id ? 'Selected' : 'Select'}
                </Button>
              )}
              <Button variant="secondary" size="sm" onClick={() => onEdit?.(address)}>
                Edit
              </Button>
              <Button variant="error" size="sm" onClick={() => onDelete?.(address.id)}>
                Delete
              </Button>
            </div>
          </div>
        </Card>
      ))}
    </div>
  )
}

export default AddressList
