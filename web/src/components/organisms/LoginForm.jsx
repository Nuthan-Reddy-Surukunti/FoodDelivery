import { Button } from '../atoms/Button'
import { FormField } from '../molecules/FormField'

export const LoginForm = ({ values, errors = {}, onChange, onSubmit, loading = false }) => {
  return (
    <form onSubmit={onSubmit} className="space-y-4 rounded-2xl border border-outline bg-surface p-4">
      <FormField label="Email" name="email" type="email" value={values.email || ''} onChange={onChange} error={errors.email} required />
      <FormField label="Password" name="password" type="password" value={values.password || ''} onChange={onChange} error={errors.password} required />
      <Button type="submit" fullWidth disabled={loading}>{loading ? 'Signing in...' : 'Sign In'}</Button>
    </form>
  )
}

export default LoginForm
