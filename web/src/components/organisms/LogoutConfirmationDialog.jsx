import { useContext } from 'react'
import { Modal } from '../atoms/Modal'
import { LogoutConfirmationContext } from '../../context/LogoutConfirmationContext'

export const LogoutConfirmationDialog = () => {
  const { isOpen, closeConfirmation, handleConfirm } = useContext(LogoutConfirmationContext)

  const footer = (
    <div className="flex gap-3">
      <button
        type="button"
        onClick={closeConfirmation}
        className="flex-1 px-4 py-3 bg-surface border border-outline rounded-lg text-body-md font-semibold text-on-background hover:bg-surface-variant transition-colors"
      >
        Cancel
      </button>
      <button
        type="button"
        onClick={handleConfirm}
        className="flex-1 px-4 py-3 bg-error hover:bg-error/90 rounded-lg text-body-md font-semibold text-white transition-colors"
      >
        Logout
      </button>
    </div>
  )

  return (
    <Modal
      isOpen={isOpen}
      title="Confirm Logout"
      onClose={closeConfirmation}
      footer={footer}
    >
      <p className="text-body-md text-on-background/70">
        Are you sure you want to logout?
      </p>
    </Modal>
  )
}

export default LogoutConfirmationDialog
