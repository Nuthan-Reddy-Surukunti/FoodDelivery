import { useState } from 'react'
import { Button } from '../components/atoms/Button'
import { Card } from '../components/atoms/Card'

const initialUsers = [
  { id: 'U-11', name: 'Alex Roy', role: 'Customer', status: 'Active' },
  { id: 'U-22', name: 'Meera Singh', role: 'RestaurantPartner', status: 'Active' },
  { id: 'U-30', name: 'Liam G', role: 'DeliveryAgent', status: 'Suspended' },
]

export const AdminUsersPage = () => {
  const [users, setUsers] = useState(initialUsers)

  const toggleStatus = (id) => {
    setUsers((prev) => prev.map((user) => (
      user.id === id ? { ...user, status: user.status === 'Active' ? 'Suspended' : 'Active' } : user
    )))
  }

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-5 text-2xl font-bold">User Management</h1>
      <div className="space-y-3">
        {users.map((user) => (
          <Card key={user.id} className="flex items-center justify-between">
            <div>
              <p className="font-semibold">{user.name}</p>
              <p className="text-sm text-on-background/70">{user.role} • {user.status}</p>
            </div>
            <Button size="sm" variant="secondary" onClick={() => toggleStatus(user.id)}>
              {user.status === 'Active' ? 'Suspend' : 'Activate'}
            </Button>
          </Card>
        ))}
      </div>
    </div>
  )
}
