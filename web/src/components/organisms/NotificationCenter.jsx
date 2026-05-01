import PropTypes from 'prop-types'

export const NotificationCenter = ({
  isOpen,
  items,
  unreadCount,
  onClose,
  onMarkAllRead,
  onRefresh,
  onMarkRead,
}) => {
  if (!isOpen) return null

  return (
    <div className="absolute right-0 top-12 z-50 w-80 rounded-2xl border border-slate-200 bg-white shadow-xl">
      <div className="flex items-center justify-between border-b border-slate-100 px-4 py-3">
        <div>
          <p className="text-sm font-semibold text-slate-900">Notifications</p>
          <p className="text-xs text-slate-500">{unreadCount} unread</p>
        </div>
        <div className="flex items-center gap-1">
          <button
            type="button"
            onClick={onRefresh}
            className="rounded-lg px-2 py-1 text-xs font-semibold text-slate-500 hover:text-primary hover:bg-slate-50"
          >
            Refresh
          </button>
          <button
            type="button"
            onClick={onMarkAllRead}
            className="rounded-lg px-2 py-1 text-xs font-semibold text-slate-500 hover:text-primary hover:bg-slate-50"
          >
            Mark all read
          </button>
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg px-2 py-1 text-xs font-semibold text-slate-400 hover:text-slate-700"
            aria-label="Close notifications"
          >
            Close
          </button>
        </div>
      </div>

      <div className="max-h-96 overflow-y-auto">
        {items.length === 0 ? (
          <div className="px-4 py-8 text-center text-sm text-slate-500">
            No notifications yet.
          </div>
        ) : (
          <div className="divide-y divide-slate-100">
            {items.map((item) => (
              <button
                key={item.id}
                type="button"
                onClick={() => onMarkRead(item.id)}
                className="w-full px-4 py-3 text-left hover:bg-slate-50 transition-colors"
              >
                <div className="flex items-start gap-3">
                  <div className={`mt-0.5 h-8 w-8 rounded-xl flex items-center justify-center ${
                    item.isRead ? 'bg-slate-100 text-slate-400' : 'bg-primary/10 text-primary'
                  }`}>
                    <span className="material-symbols-outlined text-lg">{item.icon}</span>
                  </div>
                  <div className="flex-1">
                    <div className="flex items-center justify-between gap-2">
                      <p className={`text-sm font-semibold ${item.isRead ? 'text-slate-600' : 'text-slate-900'}`}>
                        {item.title}
                      </p>
                      {!item.isRead && <span className="h-2 w-2 rounded-full bg-primary" />}
                    </div>
                    <p className="text-xs text-slate-500 mt-1">{item.message}</p>
                    <p className="text-[11px] text-slate-400 mt-2">{item.time}</p>
                  </div>
                </div>
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

NotificationCenter.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(
    PropTypes.shape({
      id: PropTypes.number.isRequired,
      title: PropTypes.string.isRequired,
      message: PropTypes.string.isRequired,
      time: PropTypes.string.isRequired,
      icon: PropTypes.string.isRequired,
      isRead: PropTypes.bool.isRequired,
    })
  ).isRequired,
  unreadCount: PropTypes.number.isRequired,
  onClose: PropTypes.func.isRequired,
  onMarkAllRead: PropTypes.func.isRequired,
  onRefresh: PropTypes.func.isRequired,
  onMarkRead: PropTypes.func.isRequired,
}

export default NotificationCenter
