import { useCallback, useEffect, useState } from 'react'

export const useApi = (requestFn, options = {}) => {
  const { immediate = true, dependencies = [] } = options

  const [data, setData] = useState(null)
  const [loading, setLoading] = useState(immediate)
  const [error, setError] = useState(null)

  const execute = useCallback(async (...args) => {
    setLoading(true)
    setError(null)

    try {
      const response = await requestFn(...args)
      setData(response)
      return response
    } catch (err) {
      setError(err)
      throw err
    } finally {
      setLoading(false)
    }
  }, [requestFn])

  useEffect(() => {
    if (!immediate) return

    execute().catch(() => {
      // Error is already captured in state.
    })
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [immediate, execute, ...dependencies])

  return {
    data,
    loading,
    error,
    execute,
    refetch: execute,
    setData,
  }
}

export default useApi
