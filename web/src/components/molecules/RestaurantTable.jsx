import PropTypes from 'prop-types'
import { Link } from 'react-router-dom'

export const RestaurantTable = ({ 
  restaurants = [], 
  loading = false, 
  onDelete = () => {},
  onEdit = () => {},
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
          <p className="text-on-surface-variant">Loading restaurants...</p>
        </div>
      </div>
    )
  }

  if (restaurants.length === 0) {
    return (
      <div className="bg-surface-container-lowest rounded-xl border border-surface-variant shadow-sm p-8 flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <span className="text-6xl mb-4 block">🏪</span>
          <p className="text-on-surface-variant">No restaurants found</p>
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
              <th className="px-6 py-4 text-left text-sm font-semibold text-on-surface">Name</th>
              <th className="px-6 py-4 text-left text-sm font-semibold text-on-surface">Category</th>
              <th className="px-6 py-4 text-left text-sm font-semibold text-on-surface">Location</th>
              <th className="px-6 py-4 text-left text-sm font-semibold text-on-surface">Rating</th>
              <th className="px-6 py-4 text-left text-sm font-semibold text-on-surface">Status</th>
              <th className="px-6 py-4 text-center text-sm font-semibold text-on-surface">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-surface-variant">
            {restaurants.map((restaurant) => {
              const statusColor = {
                Active: 'bg-emerald-100 text-emerald-800',
                PendingApproval: 'bg-amber-100 text-amber-800',
                Pending: 'bg-amber-100 text-amber-800',
                Rejected: 'bg-red-100 text-red-800',
                Inactive: 'bg-slate-100 text-slate-600',
              }[restaurant.status] || 'bg-slate-100 text-slate-600'

              return (
                <tr key={restaurant.id} className="hover:bg-surface-container-low transition-colors">
                  <td className="px-6 py-4 text-sm font-medium text-on-surface">{restaurant.name}</td>
                  <td className="px-6 py-4 text-sm text-on-surface-variant">{restaurant.cuisine || '—'}</td>
                  <td className="px-6 py-4 text-sm text-on-surface-variant">{restaurant.city || '—'}</td>
                  <td className="px-6 py-4 text-sm">
                    <div className="flex items-center gap-1">
                      {restaurant.rating ? '⭐ ' + restaurant.rating : '—'}
                    </div>
                  </td>
                  <td className="px-6 py-4 text-sm">
                    <span className={`inline-block px-3 py-1 rounded-full text-xs font-medium ${statusColor}`}>
                      {restaurant.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-sm text-center">
                    <div className="flex items-center justify-center gap-2">
                      <button
                        onClick={() => onEdit(restaurant)}
                        className="p-2 hover:bg-blue-50 text-blue-600 rounded-lg transition-colors"
                        aria-label="Edit"
                      >
                        <span className="material-symbols-outlined text-[20px]">edit</span>
                      </button>
                      <button
                        onClick={() => onDelete(restaurant.id, restaurant.name)}
                        className="p-2 hover:bg-red-50 text-red-600 rounded-lg transition-colors"
                        aria-label="Delete"
                      >
                        <span className="material-symbols-outlined text-[20px]">delete</span>
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
            Showing {(currentPage - 1) * pageSize + 1} to {Math.min(currentPage * pageSize, totalItems)} of {totalItems} entries
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

RestaurantTable.propTypes = {
  restaurants: PropTypes.arrayOf(PropTypes.object),
  loading: PropTypes.bool,
  onDelete: PropTypes.func,
  onEdit: PropTypes.func,
  currentPage: PropTypes.number,
  pageSize: PropTypes.number,
  totalItems: PropTypes.number,
  onPageChange: PropTypes.func,
}
