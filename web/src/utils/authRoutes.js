const ROLE_HOME_PATHS = {
  admin: '/admin',
  restaurantpartner: '/partner/dashboard',
  deliveryagent: '/agent/active',
  customer: '/',
}

export const normalizeRole = (role) => {
  if (!role) {
    return 'customer'
  }

  return String(role).replace(/\s+/g, '').toLowerCase()
}

export const getRoleHomePath = (role) => {
  const normalizedRole = normalizeRole(role)
  return ROLE_HOME_PATHS[normalizedRole] || '/'
}
