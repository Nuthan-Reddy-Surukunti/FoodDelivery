import { useState, useCallback } from 'react'

/**
 * Form validation hook
 * Manages form state and validation rules
 */
export const useFormValidation = (initialValues, onSubmit) => {
  const [values, setValues] = useState(initialValues)
  const [errors, setErrors] = useState({})
  const [touched, setTouched] = useState({})
  const [isSubmitting, setIsSubmitting] = useState(false)

  // Validation rules
  const validators = {
    email: (value) => {
      if (!value) return 'Email is required'
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
      if (!emailRegex.test(value)) return 'Please enter a valid email address'
      return null
    },
    password: (value) => {
      if (!value) return 'Password is required'
      if (value.length < 8) return 'Password must be at least 8 characters'
      return null
    },
    fullName: (value) => {
      if (!value) return 'Full name is required'
      if (value.trim().length < 2) return 'Full name must be at least 2 characters'
      return null
    },
    mobileNumber: (value) => {
      if (!value) return 'Phone number is required'
      const phoneRegex = /^[\d\s\-\+\(\)]{10,}$/
      if (!phoneRegex.test(value)) return 'Please enter a valid phone number'
      return null
    },
    terms: (value) => {
      if (!value) return 'You must agree to the terms and conditions'
      return null
    },
  }

  const validate = useCallback(() => {
    const newErrors = {}
    Object.keys(values).forEach((key) => {
      if (validators[key]) {
        const error = validators[key](values[key])
        if (error) newErrors[key] = error
      }
    })
    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }, [values])

  const handleChange = useCallback((e) => {
    const { name, value, type, checked } = e.target
    setValues((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }))

    // Clear error when user starts typing
    if (errors[name]) {
      setErrors((prev) => ({
        ...prev,
        [name]: null,
      }))
    }
  }, [errors])

  const handleBlur = useCallback((e) => {
    const { name } = e.target
    setTouched((prev) => ({
      ...prev,
      [name]: true,
    }))

    // Validate field on blur
    if (validators[name]) {
      const error = validators[name](values[name])
      setErrors((prev) => ({
        ...prev,
        [name]: error,
      }))
    }
  }, [values])

  const handleSubmit = useCallback(
    async (e) => {
      e.preventDefault()
      if (!validate()) return

      setIsSubmitting(true)
      try {
        await onSubmit(values)
      } catch (error) {
        console.error('Form submission error:', error)
      } finally {
        setIsSubmitting(false)
      }
    },
    [validate, onSubmit, values]
  )

  return {
    values,
    errors,
    touched,
    isSubmitting,
    handleChange,
    handleBlur,
    handleSubmit,
    setFieldValue: (name, value) => {
      setValues((prev) => ({ ...prev, [name]: value }))
    },
    setFieldError: (name, error) => {
      setErrors((prev) => ({ ...prev, [name]: error }))
    },
  }
}

export default useFormValidation
