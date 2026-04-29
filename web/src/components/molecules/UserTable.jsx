import PropTypes from 'prop-types'

export const UserTable = ({ 
  users = [], 
  loading = false,
  currentPage = 1,
  pageSize = 10,
  totalItems = 0,
  onPageChange = () => {},
}) => {
  const totalPages = Math.ceil(totalItems / pageSize)

  if (loading) {
    return (
      <div className="bg-surface-container-lowest rounded-xl border border-surface-variant shadow-sm p-8 flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="w-10 h-10 mx-auto mb-4 border-4 border-primary/20 border-t-primary rounded-full animate-spin" />
          <p className="text-on-surface-variant">Loading users...</p>
        </div>
      </div>
    )
  }

  if (users.length === 0) {
    return (
      <div className="bg-surface-container-lowest rounded-xl border border-surface-variant shadow-sm p-8 flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <span className="text-6xl mb-4 block">👥</span>
          <p className="text-on-surface-variant">No users found</p>
        </div>
      </div>
    )
  }

  return (
    <div className="bg-surface-container-lowest rounded-xl border border-surface-variant shadow-sm overflow-hidden flex flex-col">
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead className="bg-surface-container border-b border-surface-variant">
            <tr>
              <th className="px-6 py-4 text-left text-sm font-semibold text-on-surface">Customer</th>
              <th className="px-6 py-4 text-left text-sm font-semibold text-on-surface">Contact</th>
              <th className="px-6 py-4 text-left text-sm font-semibold text-on-surface">Joined Date</th>
              <th className="px-6 py-4 text-left text-sm font-semibold text-on-surface">Status</th>
              <th className="px-6 py-4 text-center text-sm font-semibold text-on-surface">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-surface-variant">
            {users.map((user) => {
              const statusColor = {
                Active: 'text-emerald-700',
                Suspended: 'text-red-700',
                Inactive: 'text-slate-600',
              }[user.status] || 'text-slate-600'
              
              const dotColor = {
                Active: 'bg-emerald-600',
                Suspended: 'bg-red-600',
                Inactive: 'bg-slate-500',
              }[user.status] || 'bg-slate-500'

              const nameInitial = (user.name || user.email || 'U')[0].toUpperCase()

              return (
                <tr key={user.id} className="hover:bg-surface-container-low transition-colors">
                  <td className="px-6 py-4 text-sm">
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center text-xs font-semibold">
                        {nameInitial}
                      </div>
                      <span className="font-medium text-on-surface">{user.name || '—'}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4 text-sm text-on-surface-variant">
                    <div className="space-y-0.5">
                      <div>{user.email || '—'}</div>
                      {user.phone && <div className="text-xs">{user.phone}</div>}
                    </div>
                  </td>
                  <td className="px-6 py-4 text-sm text-on-surface-variant">
                    {user.joinedDate 
                      ? new Date(user.joinedDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
                      : '—'
                    }
                  </td>
                  <td className="px-6 py-4 text-sm">
                    <div className={`inline-flex items-center gap-1.5 font-medium text-xs ${statusColor}`}>
                      <span className={`w-1.5 h-1.5 rounded-full ${dotColor}`} />
                      {user.status}
                    </div>
                  </td>
                  <td className="px-6 py-4 text-sm text-center">
                    <div className="flex items-center justify-center gap-2">
                      <button
                        className="p-2 hover:bg-blue-50 text-blue-600 rounded-lg transition-colors"
                        aria-label="View Details"
                      >
                        <span className="material-symbols-outlined text-[20px]">visibility</span>
                      </button>
                      <button
                        className="p-2 hover:bg-slate-100 text-slate-600 rounded-lg transition-colors"
                        aria-label="More Options"
                      >
                        <span className="material-symbols-outlined text-[20px]">more_vert</span>
                      </button>
                    </div>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="px-6 py-4 border-t border-surface-variant flex items-center justify-between">
          <div className="text-sm text-on-surface-variant">
            Showing {(currentPage - 1) * pageSize + 1} to {Math.min(currentPage * pageSize, totalItems)} of {totalItems} results
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={() => onPageChange(currentPage - 1)}
              disabled={currentPage === 1}
              className="px-3 py-2 rounded-lg border border-surface-variant text-on-surface disabled:opacity-50 hover:bg-surface-container transition-colors text-sm font-medium"
            >
              ← Prev
            </button>
            {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
              <button
                key={page}
                onClick={() => onPageChange(page)}
                className={`px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
                  currentPage === page
                    ? 'bg-primary text-on-primary'
                    : 'border border-surface-variant text-on-surface hover:bg-surface-container'
                }`}
              >
                {page}
              </button>
            ))}
            <button
              onClick={() => onPageChange(currentPage + 1)}
              disabled={currentPage === totalPages}
              className="px-3 py-2 rounded-lg border border-surface-variant text-on-surface disabled:opacity-50 hover:bg-surface-container transition-colors text-sm font-medium"
            >
              Next →
            </button>
          </div>
        </div>
      )}
    </div>
  )
}

UserTable.propTypes = {
  users: PropTypes.arrayOf(PropTypes.object),
  loading: PropTypes.bool,
  currentPage: PropTypes.number,
  pageSize: PropTypes.number,
  totalItems: PropTypes.number,
  onPageChange: PropTypes.func,
}
