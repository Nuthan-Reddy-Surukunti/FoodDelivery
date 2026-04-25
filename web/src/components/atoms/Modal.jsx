import { Icon } from './Icon'

export const Modal = ({ isOpen, title, children, onClose, footer = null }) => {
  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4" role="dialog" aria-modal="true">
      <div className="w-full max-w-lg rounded-2xl bg-surface shadow-xl">
        <div className="flex items-center justify-between border-b border-outline px-5 py-4">
          <h3 className="text-lg font-semibold text-on-background">{title}</h3>
          <button type="button" onClick={onClose} aria-label="Close modal">
            <Icon name="close" size={20} />
          </button>
        </div>
        <div className="px-5 py-4">{children}</div>
        {footer ? <div className="border-t border-outline px-5 py-4">{footer}</div> : null}
      </div>
    </div>
  )
}

export default Modal
