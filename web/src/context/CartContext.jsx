import React, { createContext, useState, useCallback, useEffect } from 'react'

export const CartContext = createContext()

export const CartProvider = ({ children }) => {
  const [items, setItems] = useState([])
  const [restaurantId, setRestaurantId] = useState(null)
  const [isInitialized, setIsInitialized] = useState(false)

  // Load cart from localStorage on mount
  useEffect(() => {
    const savedCart = localStorage.getItem('cart')
    const savedRestaurantId = localStorage.getItem('restaurantId')
    
    if (savedCart) {
      try {
        setItems(JSON.parse(savedCart))
      } catch (e) {
        console.error('Failed to parse cart', e)
      }
    }
    if (savedRestaurantId) {
      setRestaurantId(savedRestaurantId)
    }
    setIsInitialized(true)
  }, [])

  // Save to localStorage whenever items or restaurantId change
  useEffect(() => {
    if (!isInitialized) return

    if (items.length > 0) {
      localStorage.setItem('cart', JSON.stringify(items))
      if (restaurantId) {
        localStorage.setItem('restaurantId', restaurantId)
      }
    } else {
      localStorage.removeItem('cart')
      localStorage.removeItem('restaurantId')
      if (restaurantId !== null) {
        setRestaurantId(null)
      }
    }
  }, [items, restaurantId, isInitialized])

  const addItem = useCallback((item, newRestaurantId) => {
    // Only allow items from one restaurant at a time
    if (restaurantId && restaurantId !== newRestaurantId) {
      if (!window.confirm('Adding items from a different restaurant will clear your current cart. Do you want to continue?')) {
        return;
      }
      setItems([])
      setRestaurantId(newRestaurantId)
    } else {
      setRestaurantId(newRestaurantId)
    }

    setItems(prevItems => {
      const existingItem = prevItems.find(i => i.id === item.id)
      
      if (existingItem) {
        return prevItems.map(i =>
          i.id === item.id ? { ...i, quantity: i.quantity + 1 } : i
        )
      } else {
        return [...prevItems, { ...item, quantity: 1 }]
      }
    })
  }, [restaurantId])

  const removeItem = useCallback((itemId) => {
    setItems(prevItems => prevItems.filter(i => i.id !== itemId))
  }, [])

  const updateQuantity = useCallback((itemId, quantity) => {
    if (quantity <= 0) {
      removeItem(itemId)
    } else {
      setItems(prevItems =>
        prevItems.map(i =>
          i.id === itemId ? { ...i, quantity } : i
        )
      )
    }
  }, [removeItem])

  const clearCart = useCallback(() => {
    setItems([])
    setRestaurantId(null)
  }, [])

  const totalPrice = items.reduce((sum, item) => sum + (item.price * item.quantity), 0)
  const totalItems = items.reduce((sum, item) => sum + item.quantity, 0)

  const value = {
    items,
    restaurantId,
    totalPrice,
    totalItems,
    addItem,
    removeItem,
    updateQuantity,
    clearCart
  }

  return (
    <CartContext.Provider value={value}>
      {children}
    </CartContext.Provider>
  )
}

export const useCart = () => {
  const context = React.useContext(CartContext)
  if (!context) {
    throw new Error('useCart must be used within CartProvider')
  }
  return context
}
