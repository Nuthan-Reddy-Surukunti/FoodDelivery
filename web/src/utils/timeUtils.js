/**
 * Safely parses an ISO string and ensures it is treated as UTC if no TZ is present
 */
export const parseIsoToUtc = (date) => {
  if (!date) return null
  const dateStr = typeof date === 'string' && !date.endsWith('Z') && !date.includes('+') 
    ? `${date}Z` 
    : date
  return new Date(dateStr)
}

/**
 * Formats a date as a relative time string (e.g., "5 minutes ago")
 */
export const formatTimeAgo = (date) => {
  const parsedDate = parseIsoToUtc(date)
  if (!parsedDate) return '—'
  
  const now = new Date()
  const diffInSeconds = Math.floor((now - parsedDate) / 1000)

  // Handle future dates due to small clock skews
  if (diffInSeconds < 0) return 'Just now'
  if (diffInSeconds < 60) return 'Just now'
  
  const diffInMinutes = Math.floor(diffInSeconds / 60)
  if (diffInMinutes < 60) return `${diffInMinutes}m ago`
  
  const diffInHours = Math.floor(diffInMinutes / 60)
  if (diffInHours < 24) return `${diffInHours}h ago`
  
  const diffInDays = Math.floor(diffInHours / 24)
  if (diffInDays === 1) return 'Yesterday'
  if (diffInDays < 7) return `${diffInDays}d ago`
  
  return parsedDate.toLocaleDateString('en-IN', {
    day: 'numeric',
    month: 'short',
  })
}

/**
 * Returns true if the date was within the last 5 minutes
 */
export const isVeryRecent = (date) => {
  const parsedDate = parseIsoToUtc(date)
  if (!parsedDate) return false

  const diffInMinutes = Math.floor((new Date() - parsedDate) / 60000)
  return diffInMinutes >= 0 && diffInMinutes < 5
}
