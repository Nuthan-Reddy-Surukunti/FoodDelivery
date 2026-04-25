import { useEffect, useMemo, useState } from 'react'
import { useParams } from 'react-router-dom'
import { MenuSection } from '../components/organisms/MenuSection'
import { useCart } from '../context/CartContext'
import { useNotification } from '../hooks/useNotification'
import catalogApi from '../services/catalogApi'

const normalizeMenuItems = (payload) => {
  const raw = Array.isArray(payload) ? payload : payload?.items || payload?.data || []
  return raw.map((item) => ({
    id: item.id,
    name: item.name,
    description: item.description || 'No description',
    price: Number(item.price || 0),
  }))
}

export const RestaurantDetailsPage = () => {
  const { id } = useParams()
  const { addItem } = useCart()
  const { showSuccess } = useNotification()
  const [menuData, setMenuData] = useState([])
  const [restaurantName, setRestaurantName] = useState('Restaurant Menu')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true

    const loadDetails = async () => {
      setLoading(true)
      setError('')
      try {
        const [restaurantResponse, menuResponse] = await Promise.all([
          catalogApi.getRestaurantById(id),
          catalogApi.getRestaurantMenu(id),
        ])

        if (!active) return

        setRestaurantName(restaurantResponse?.name || 'Restaurant Menu')
        setMenuData(normalizeMenuItems(menuResponse))
      } catch (err) {
        if (!active) return
        setError(err.response?.data?.message || err.message || 'Failed to load menu')
      } finally {
        if (active) {
          setLoading(false)
        }
      }
    }

    if (id) {
      loadDetails()
    }

    return () => {
      active = false
    }
  }, [id])

  const menuItems = useMemo(() => menuData, [menuData])

  const handleAddToCart = (item) => {
    addItem({ ...item, restaurantId: id }, id)
    showSuccess(`${item.name} added to cart`)
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <section className="mb-6 rounded-3xl border border-outline bg-surface p-6">
        <h1 className="text-2xl font-bold">{restaurantName}</h1>
        <p className="mt-2 text-sm text-on-background/70">Pick your favorites and add them to cart.</p>
      </section>

      {loading ? <p className="text-sm text-on-background/70">Loading menu...</p> : null}
      {error ? <p className="text-sm text-error">{error}</p> : null}
      {!loading && !error ? <MenuSection title="Menu Items" items={menuItems} onAddToCart={handleAddToCart} /> : null}
    </div>
  )
}
