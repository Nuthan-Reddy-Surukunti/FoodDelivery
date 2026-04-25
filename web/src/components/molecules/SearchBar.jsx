import { Input } from '../atoms/Input'
import { Button } from '../atoms/Button'

export const SearchBar = ({ value, onChange, onSearch, placeholder = 'Search restaurants or food...' }) => {
  return (
    <div className="flex gap-2">
      <Input value={value} onChange={onChange} placeholder={placeholder} className="flex-1" />
      <Button onClick={onSearch} variant="primary">Search</Button>
    </div>
  )
}

export default SearchBar
