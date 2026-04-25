import { useState, useEffect } from 'react'
import { Button } from '../components/atoms/Button'
import { AddressForm } from '../components/organisms/AddressForm'
import { AddressList } from '../components/organisms/AddressList'
import orderApi from '../services/orderApi'
import { useNotification } from '../hooks/useNotification'

export const AddressManagementPage = () => {
  const [addresses, setAddresses] = useState([])
  const [showForm, setShowForm] = useState(false)
  const [editingAddress, setEditingAddress] = useState(null)
  const [loading, setLoading] = useState(false)
  const [fetching, setFetching] = useState(true)
  const { showSuccess, showError } = useNotification()

  useEffect(() => {
    loadAddresses()
  }, [])

  const loadAddresses = async () => {
    try {
      setFetching(true)
      const data = await orderApi.getAddresses()
      setAddresses(data || [])
    } catch (error) {
      showError(error.message || 'Failed to load addresses')
    } finally {
      setFetching(false)
    }
  }

  const handleAddAddress = () => {
    setEditingAddress(null)
    setShowForm(true)
  }

  const handleEditAddress = (address) => {
    setEditingAddress(address)
    setShowForm(true)
  }

  const handleDeleteAddress = async (addressId) => {
    if (window.confirm('Are you sure you want to delete this address?')) {
      try {
        await orderApi.deleteAddress(addressId)
        showSuccess('Address deleted successfully')
        loadAddresses()
      } catch (error) {
        showError(error.message || 'Failed to delete address')
      }
    }
  }

  const handleFormSubmit = async (formData) => {
    try {
      setLoading(true)
      if (editingAddress) {
        await orderApi.updateAddress(editingAddress.id, formData)
        showSuccess('Address updated successfully')
      } else {
        await orderApi.createAddress(formData)
        showSuccess('Address created successfully')
      }
      setShowForm(false)
      setEditingAddress(null)
      loadAddresses()
    } catch (error) {
      showError(error.message || 'Failed to save address')
    } finally {
      setLoading(false)
    }
  }

  const handleCancel = () => {
    setShowForm(false)
    setEditingAddress(null)
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-8">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-3xl font-bold">Manage Addresses</h1>
        {!showForm && (
          <Button onClick={handleAddAddress}>+ Add Address</Button>
        )}
      </div>

      {showForm ? (
        <div className="mb-6">
          <AddressForm
            address={editingAddress}
            onSubmit={handleFormSubmit}
            onCancel={handleCancel}
            loading={loading}
          />
        </div>
      ) : null}

      {fetching ? (
        <div className="text-center text-on-surface-variant">Loading addresses...</div>
      ) : (
        <AddressList
          addresses={addresses}
          onEdit={handleEditAddress}
          onDelete={handleDeleteAddress}
        />
      )}
    </div>
  )
}

export default AddressManagementPage
