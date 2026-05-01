import { useCallback, useContext } from 'react'
import { LogoutConfirmationContext } from '../context/LogoutConfirmationContext'

export const useLogoutConfirmation = () => {
  const context = useContext(LogoutConfirmationContext)

  if (!context) {
    throw new Error('useLogoutConfirmation must be used within LogoutConfirmationProvider')
  }

  const { openConfirmation } = context

  const confirmLogout = useCallback((onLogoutConfirmed) => {
    openConfirmation(onLogoutConfirmed)
  }, [openConfirmation])

  return { confirmLogout }
}

export default useLogoutConfirmation
