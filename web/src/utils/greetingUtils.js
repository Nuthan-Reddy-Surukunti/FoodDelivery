/**
 * Returns a time-based greeting (Good Morning, Afternoon, or Evening)
 */
export const getTimeGreeting = () => {
  const hour = new Date().getHours()
  if (hour < 12) return 'Good Morning'
  if (hour < 17) return 'Good Afternoon'
  return 'Good Evening'
}

/**
 * Returns a hybrid greeting object with a main message and a role-specific sub-message
 * @param {string} role - The user's role (e.g., 'Admin', 'RestaurantPartner', 'DeliveryAgent', 'Customer')
 * @param {string} name - The user's full name or email
 */
export const getHybridGreeting = (role, name) => {
  const timeGreeting = getTimeGreeting()
  const firstName = name?.split(' ')[0] || 'there'
  
  const main = `${timeGreeting}, ${firstName}!`
  
  // Normalize role string
  const roleKey = role?.toLowerCase()?.replace(/\s+/g, '') || 'customer'
  
  switch (roleKey) {
    case 'admin':
      return {
        main,
        sub: 'Here is your platform snapshot for today.'
      }
    case 'restaurantpartner':
    case 'partner':
      return {
        main,
        sub: 'Ready to serve some deliciousness today?'
      }
    case 'deliveryagent':
    case 'agent':
      return {
        main,
        sub: "You're looking sharp! Ready for deliveries?"
      }
    case 'customer':
    default:
      return {
        main,
        sub: 'What are you craving today?'
      }
  }
}
