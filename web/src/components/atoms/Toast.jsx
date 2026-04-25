import { Icon } from './Icon'

export const Toast = ({ type = 'info', message, onClose }) => {
  const style = {
    success: 'border-green-200 bg-green-50 text-green-800',
    error: 'border-red-200 bg-red-50 text-red-800',
    warning: 'border-amber-200 bg-amber-50 text-amber-800',
    info: 'border-blue-200 bg-blue-50 text-blue-800',
  }[type] || 'border-gray-200 bg-white text-gray-800'

  return (
    <div className={`rounded-xl border px-4 py-3 shadow-md ${style}`} role="status">
      <div className="flex items-start justify-between gap-3">
        <p className="text-sm font-medium">{message}</p>
        <button type="button" onClick={onClose} className="opacity-70 hover:opacity-100" aria-label="Close toast">
          <Icon name="close" size={16} />
        </button>
      </div>
    </div>
  )
}

export default Toast
