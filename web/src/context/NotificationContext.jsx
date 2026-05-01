import React, { createContext, useCallback, useMemo, useState } from 'react'

export const NotificationContext = createContext(null)

let nextNotificationId = 1

export const NotificationProvider = ({ children }) => {
  const [notifications, setNotifications] = useState([])
  const [inboxItems, setInboxItems] = useState([])
  const [isCenterOpen, setIsCenterOpen] = useState(false)

  const dismiss = useCallback((id) => {
    setNotifications((prev) => prev.filter((item) => item.id !== id))
  }, [])

  const show = useCallback((type, message, duration = 3000) => {
    const id = nextNotificationId++
    const notification = { id, type, message }

    setNotifications((prev) => [...prev, notification])

    if (duration > 0) {
      window.setTimeout(() => {
        dismiss(id)
      }, duration)
    }

    return id
  }, [dismiss])

  const showSuccess = useCallback((message, duration) => show('success', message, duration), [show])
  const showError = useCallback((message, duration) => show('error', message, duration), [show])
  const showInfo = useCallback((message, duration) => show('info', message, duration), [show])
  const showWarning = useCallback((message, duration) => show('warning', message, duration), [show])

  const refreshInbox = useCallback(() => {
    setInboxItems([])
  }, [])

  const markAllRead = useCallback(() => {
    setInboxItems((prev) => prev.map((item) => ({ ...item, isRead: true })))
  }, [])

  const markRead = useCallback((id) => {
    setInboxItems((prev) => prev.map((item) => (item.id === id ? { ...item, isRead: true } : item)))
  }, [])

  const openCenter = useCallback(() => {
    setIsCenterOpen(true)
  }, [])

  const closeCenter = useCallback(() => {
    setIsCenterOpen(false)
  }, [])

  const toggleCenter = useCallback(() => {
    setIsCenterOpen((prev) => !prev)
  }, [])

  const unreadCount = useMemo(
    () => inboxItems.filter((item) => !item.isRead).length,
    [inboxItems]
  )

  const value = useMemo(() => ({
    notifications,
    inboxItems,
    isCenterOpen,
    unreadCount,
    show,
    showSuccess,
    showError,
    showInfo,
    showWarning,
    dismiss,
    refreshInbox,
    markAllRead,
    markRead,
    openCenter,
    closeCenter,
    toggleCenter,
  }), [
    notifications,
    inboxItems,
    isCenterOpen,
    unreadCount,
    show,
    showSuccess,
    showError,
    showInfo,
    showWarning,
    dismiss,
    refreshInbox,
    markAllRead,
    markRead,
    openCenter,
    closeCenter,
    toggleCenter,
  ])

  return (
    <NotificationContext.Provider value={value}>
      {children}
      <NotificationViewport notifications={notifications} onDismiss={dismiss} />
    </NotificationContext.Provider>
  )
}

const NotificationViewport = ({ notifications, onDismiss }) => {
  if (!notifications.length) return null

  return (
    <div className="fixed top-4 right-4 z-50 flex w-full max-w-sm flex-col gap-2 px-4">
      {notifications.map((item) => (
        <NotificationToast key={item.id} notification={item} onDismiss={onDismiss} />
      ))}
    </div>
  )
}

const NotificationToast = ({ notification, onDismiss }) => {
  const toneClass = {
    success: 'border-green-200 bg-green-50 text-green-800',
    error: 'border-red-200 bg-red-50 text-red-800',
    warning: 'border-amber-200 bg-amber-50 text-amber-800',
    info: 'border-blue-200 bg-blue-50 text-blue-800',
  }[notification.type] || 'border-gray-200 bg-white text-gray-800'

  return (
    <div className={`rounded-xl border px-4 py-3 shadow-md ${toneClass}`} role="status" aria-live="polite">
      <div className="flex items-start justify-between gap-2">
        <p className="text-sm font-medium">{notification.message}</p>
        <button
          type="button"
          onClick={() => onDismiss(notification.id)}
          className="text-xs font-semibold opacity-70 transition hover:opacity-100"
          aria-label="Dismiss notification"
        >
          Close
        </button>
      </div>
    </div>
  )
}

export const useNotificationContext = () => {
  const context = React.useContext(NotificationContext)
  if (!context) {
    throw new Error('useNotificationContext must be used within NotificationProvider')
  }
  return context
}
