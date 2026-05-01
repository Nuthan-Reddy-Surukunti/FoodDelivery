import React, { createContext, useCallback, useMemo, useState } from 'react'

export const LogoutConfirmationContext = createContext(null)

export const LogoutConfirmationProvider = ({ children }) => {
  const [isOpen, setIsOpen] = useState(false)
  const [onConfirm, setOnConfirm] = useState(null)

  const openConfirmation = useCallback((confirmCallback) => {
    setOnConfirm(() => confirmCallback)
    setIsOpen(true)
  }, [])

  const closeConfirmation = useCallback(() => {
    setIsOpen(false)
    setOnConfirm(null)
  }, [])

  const handleConfirm = useCallback(async () => {
    if (onConfirm) {
      await onConfirm()
    }
    closeConfirmation()
  }, [onConfirm, closeConfirmation])

  const value = useMemo(() => ({
    isOpen,
    openConfirmation,
    closeConfirmation,
    handleConfirm,
  }), [isOpen, openConfirmation, closeConfirmation, handleConfirm])

  return (
    <LogoutConfirmationContext.Provider value={value}>
      {children}
    </LogoutConfirmationContext.Provider>
  )
}

export default LogoutConfirmationContext
