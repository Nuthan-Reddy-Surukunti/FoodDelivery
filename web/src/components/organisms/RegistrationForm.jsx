import { Button } from '../atoms/Button'
import { FormField } from '../molecules/FormField'

export const RegistrationForm = ({ values, errors = {}, onChange, onSubmit, loading = false }) => {
  return (
    <form onSubmit={onSubmit} className="space-y-4 rounded-2xl border border-outline bg-surface p-4">
      <FormField label="Full Name" name="fullName" value={values.fullName || ''} onChange={onChange} error={errors.fullName} required />
      <FormField label="Email" name="email" type="email" value={values.email || ''} onChange={onChange} error={errors.email} required />
      <FormField label="Phone" name="mobileNumber" value={values.mobileNumber || ''} onChange={onChange} error={errors.mobileNumber} required />
      <FormField label="Password" name="password" type="password" value={values.password || ''} onChange={onChange} error={errors.password} required />
      <Button type="submit" fullWidth disabled={loading}>{loading ? 'Creating account...' : 'Create Account'}</Button>
    </form>
  )
}

export default RegistrationForm
