import { useState, useRef, useEffect } from 'react'

export const UserTable = ({ 
  users = [], 
  loading = false,
  currentPage = 1,
  pageSize = 10,
  totalItems = 0,
  onPageChange = () => {},
  onView = () => {},
  onToggleStatus = () => {},
  onDelete = () => {},
}) => {
  const totalPages = Math.ceil(totalItems / pageSize)
  const [activeDropdown, setActiveDropdown] = useState(null)
  const dropdownRef = useRef(null)

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setActiveDropdown(null)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  if (loading) {
    return (
      <div className="p-8 flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="w-10 h-10 mx-auto mb-4 border-4 border-primary/20 border-t-primary rounded-full animate-spin" />
          <p className="text-slate-500">Loading users...</p>
        </div>
      </div>
    )
  }

  if (users.length === 0) {
    return (
      <div className="p-8 flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <span className="text-6xl mb-4 block opacity-20">group</span>
          <p className="text-slate-500 font-medium">No users found</p>
        </div>
      </div>
    )
  }

  return (
    <div className="flex flex-col w-full">
      <div className="overflow-x-auto">
        <table className="w-full border-collapse">
          <thead>
            <tr className="border-b border-slate-100 bg-white">
              <th className="px-6 py-4 text-left text-[13px] font-semibold text-slate-600 uppercase tracking-wider">Customer</th>
              <th className="px-6 py-4 text-left text-[13px] font-semibold text-slate-600 uppercase tracking-wider">Contact</th>
              <th className="px-6 py-4 text-left text-[13px] font-semibold text-slate-600 uppercase tracking-wider">Joined Date</th>
              <th className="px-6 py-4 text-left text-[13px] font-semibold text-slate-600 uppercase tracking-wider">Status</th>
              <th className="px-6 py-4 text-right text-[13px] font-semibold text-slate-600 uppercase tracking-wider pr-10">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-50">
            {users.map((user) => {
              const isActive = user.status === 'Active'
              
              const statusClasses = isActive 
                ? 'bg-slate-50 text-slate-700' 
                : 'bg-red-50 text-red-700'
              
              const dotColor = isActive ? 'bg-primary' : 'bg-red-600'

              const nameInitial = (user.name || user.email || 'U')[0].toUpperCase()

              return (
                <tr key={user.id} className="hover:bg-slate-50/50 transition-colors group">
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-3">
                      <div className="w-9 h-9 rounded-full bg-rose-50 text-primary flex items-center justify-center text-xs font-bold shadow-sm border border-rose-100">
                        {nameInitial}
                      </div>
                      <span className="font-bold text-slate-900 text-sm tracking-tight">{user.name || '—'}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="flex flex-col">
                      <span className="text-sm text-slate-700 font-medium">{user.email || '—'}</span>
                      {user.phone && <span className="text-xs text-slate-400 mt-0.5">{user.phone}</span>}
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <span className="text-sm text-slate-600">
                      {user.joinedDate 
                        ? new Date(user.joinedDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
                        : '—'
                      }
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <div className={`inline-flex items-center gap-2 px-3 py-1 rounded-full text-xs font-bold ${statusClasses} border border-slate-100 shadow-sm`}>
                      <span className={`w-2 h-2 rounded-full ${dotColor}`} />
                      {user.status}
                    </div>
                  </td>
                  <td className="px-6 py-4 text-right pr-10 relative">
                    <div className="flex items-center justify-end gap-2">
                      <button
                        onClick={() => onView(user)}
                        className="p-1.5 hover:bg-rose-50 text-primary rounded-lg transition-colors"
                        title="View Details"
                      >
                        <span className="material-symbols-outlined text-[20px]">visibility</span>
                      </button>
                      
                      <div className="relative">
                        <button
                          onClick={(e) => {
                            e.stopPropagation()
                            setActiveDropdown(activeDropdown === user.id ? null : user.id)
                          }}
                          className={`p-1.5 rounded-lg transition-colors ${
                            activeDropdown === user.id
                              ? 'bg-slate-100 text-slate-900 shadow-inner'
                              : 'hover:bg-slate-100 text-slate-400 group-hover:text-slate-600'
                          }`}
                        >
                          <span className="material-symbols-outlined text-[20px]">more_vert</span>
                        </button>

                        {activeDropdown === user.id && (
                          <div 
                            ref={dropdownRef}
                            className="absolute right-0 top-full mt-2 w-52 bg-white rounded-2xl shadow-xl border border-slate-100 overflow-hidden z-20 animate-in fade-in slide-in-from-top-2 duration-200"
                          >
                            <button
                              onClick={() => {
                                onToggleStatus(user)
                                setActiveDropdown(null)
                              }}
                              className="w-full text-left px-4 py-3.5 text-[13px] font-semibold hover:bg-slate-50 transition-colors flex items-center gap-3 text-slate-700"
                            >
                              <span className="material-symbols-outlined text-[20px] text-slate-400">
                                {user.status === 'Active' ? 'block' : 'check_circle'}
                              </span>
                              {user.status === 'Active' ? 'Suspend Account' : 'Activate Account'}
                            </button>
                            <button
                              onClick={() => {
                                onDelete(user)
                                setActiveDropdown(null)
                              }}
                              className="w-full text-left px-4 py-3.5 text-[13px] font-semibold hover:bg-red-50 text-red-600 transition-colors flex items-center gap-3 border-t border-slate-50"
                            >
                              <span className="material-symbols-outlined text-[20px] text-red-400">delete</span>
                              Delete Permanently
                            </button>
                          </div>
                        )}
                      </div>
                    </div>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>

      {/* Pagination matching screenshot */}
      <div className="px-6 py-5 border-t border-slate-100 flex items-center justify-between bg-white">
        <div className="text-[13px] font-medium text-slate-500">
          Showing <span className="text-slate-900 font-bold">{(currentPage - 1) * pageSize + 1}</span> to <span className="text-slate-900 font-bold">{Math.min(currentPage * pageSize, totalItems)}</span> of <span className="text-slate-900 font-bold">{totalItems}</span> results
        </div>
        
        {totalPages > 1 && (
          <div className="flex items-center gap-1">
            <button
              onClick={() => onPageChange(currentPage - 1)}
              disabled={currentPage === 1}
              className="p-2 rounded-xl text-slate-400 hover:bg-slate-50 disabled:opacity-30 transition-all"
            >
              <span className="material-symbols-outlined text-[20px]">chevron_left</span>
            </button>
            
            {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
              <button
                key={page}
                onClick={() => onPageChange(page)}
                className={`w-9 h-9 rounded-lg text-sm font-bold transition-all ${
                  currentPage === page
                    ? 'bg-primary text-white shadow-md shadow-primary/20'
                    : 'text-slate-600 hover:bg-slate-50'
                }`}
              >
                {page}
              </button>
            ))}
            
            <button
              onClick={() => onPageChange(currentPage + 1)}
              disabled={currentPage === totalPages}
              className="p-2 rounded-xl text-slate-400 hover:bg-slate-50 disabled:opacity-30 transition-all"
            >
              <span className="material-symbols-outlined text-[20px]">chevron_right</span>
            </button>
          </div>
        )}
      </div>
    </div>
  )
}

