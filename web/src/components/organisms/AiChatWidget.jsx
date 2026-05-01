import { useState, useRef, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import catalogApi from '../../services/catalogApi'
import { RestaurantCard } from '../molecules/RestaurantCard'
import { useAuth } from '../../context/AuthContext'
import { useCart } from '../../context/CartContext'
import { useNotification } from '../../hooks/useNotification'
import biteBotAvatar from '../../assets/bitebot-avatar.png'

// ─── Status Badge ─────────────────────────────────────────────────────────────
const StatusBadge = ({ status }) => {
  const map = {
    Pending:    { color: 'bg-yellow-100 text-yellow-800', label: 'Pending' },
    Confirmed:  { color: 'bg-blue-100 text-blue-800',    label: 'Confirmed' },
    Preparing:  { color: 'bg-orange-100 text-orange-800', label: 'Preparing' },
    ReadyForPickup: { color: 'bg-purple-100 text-purple-800', label: 'Ready for Pickup' },
    OutForDelivery: { color: 'bg-indigo-100 text-indigo-800', label: 'Out for Delivery' },
    Delivered:  { color: 'bg-green-100 text-green-800',  label: 'Delivered' },
    Cancelled:  { color: 'bg-red-100 text-red-800',      label: 'Cancelled' },
  }
  const { color, label } = map[status] || { color: 'bg-slate-100 text-slate-700', label: status }
  return <span className={`text-[10px] font-bold px-2 py-0.5 rounded-full ${color}`}>{label}</span>
}

// ─── Order Status Card ────────────────────────────────────────────────────────
const OrderStatusCard = ({ orderStatus, onNavigate }) => (
  <div className="mt-3 w-full bg-white border border-slate-100 rounded-xl shadow-sm overflow-hidden">
    <div className="flex items-center justify-between px-3 py-2 bg-slate-50 border-b border-slate-100">
      <span className="text-[10px] font-bold text-slate-500 uppercase tracking-wider">Your Order</span>
      <StatusBadge status={orderStatus.status} />
    </div>
    <div className="p-3 space-y-1">
      <p className="text-xs font-semibold text-slate-800">{orderStatus.restaurantName}</p>
      {orderStatus.itemNames?.slice(0, 3).map((item, i) => (
        <p key={i} className="text-[11px] text-slate-500">· {item}</p>
      ))}
      {orderStatus.itemNames?.length > 3 && (
        <p className="text-[11px] text-slate-400">+{orderStatus.itemNames.length - 3} more</p>
      )}
      <div className="flex items-center justify-between pt-1">
        <span className="text-xs font-bold text-primary">₹{Number(orderStatus.total || 0).toFixed(2)}</span>
        <button
          onClick={onNavigate}
          className="text-[10px] text-primary font-semibold hover:underline"
        >
          View Details →
        </button>
      </div>
    </div>
  </div>
)

// ─── Quick Reply Chips ────────────────────────────────────────────────────────
const QuickChips = ({ chips, onChip }) => (
  <div className="flex flex-wrap gap-1.5 mt-2">
    {chips.map((chip, i) => (
      <button
        key={i}
        onClick={() => onChip(chip)}
        className="text-[11px] px-2.5 py-1 bg-primary/10 text-primary border border-primary/20 rounded-full hover:bg-primary/20 transition-colors font-medium"
      >
        {chip}
      </button>
    ))}
  </div>
)

// ─── Derive contextual quick chips from the last AI message ──────────────────
const deriveChips = (msg) => {
  if (!msg || msg.role !== 'model') return []
  const hasRestaurants = msg.recommendedRestaurants?.length > 0
  const hasMenuItems   = msg.recommendedMenuItems?.length > 0
  const hasOrderStatus = !!msg.orderStatus

  if (hasOrderStatus) return ['Reorder this', 'Track order', 'Cancel order']
  if (hasRestaurants) return ['Show menu', 'Best rated', 'Veg options only']
  if (hasMenuItems)   return ['Add top item to cart', 'Show veg only', 'Under ₹200']
  return ['Best Indian food', 'Spicy dishes', 'Veg under ₹300', 'Show my order']
}

// ─── Main Widget ──────────────────────────────────────────────────────────────
export const AiChatWidget = () => {
  const [isOpen, setIsOpen]       = useState(false)
  const [aiStatus, setAiStatus]   = useState('unknown') // 'unknown' | 'checking' | 'online' | 'offline'
  const [messages, setMessages]   = useState([{
    role: 'model',
    text: "Hi! I'm BiteBot, your personal food guide!\nWhat kind of delicious food are you looking for today?",
    recommendedRestaurants: [],
    recommendedMenuItems:   [],
    orderStatus: null,
  }])
  const [inputText, setInputText] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [isRecording, setIsRecording] = useState(false)
  const [isVoiceEnabled, setIsVoiceEnabled] = useState(true)
  const messagesEndRef = useRef(null)
  const mediaRecorderRef = useRef(null)
  const audioChunksRef = useRef([])
  const navigate       = useNavigate()
  const { user }       = useAuth()
  const { addItem }    = useCart()
  const { showSuccess, showError } = useNotification()

  // Check AI status whenever the widget is opened
  const checkStatus = async () => {
    setAiStatus('checking')
    const online = await catalogApi.checkAiStatus()
    setAiStatus(online ? 'online' : 'offline')
  }

  const handleToggle = () => {
    const opening = !isOpen
    setIsOpen(opening)
    if (opening && aiStatus === 'unknown') checkStatus()
  }

  useEffect(() => {
    if (isOpen) messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages, isOpen])

  const handleMicClick = async () => {
    if (isRecording) {
      mediaRecorderRef.current?.stop()
      setIsRecording(false)
      return
    }

    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true })
      const mediaRecorder = new MediaRecorder(stream)
      mediaRecorderRef.current = mediaRecorder
      audioChunksRef.current = []

      mediaRecorder.ondataavailable = (event) => {
        if (event.data.size > 0) audioChunksRef.current.push(event.data)
      }

      mediaRecorder.onstop = async () => {
        const audioBlob = new Blob(audioChunksRef.current, { type: 'audio/webm' })
        stream.getTracks().forEach(track => track.stop())
        
        setIsLoading(true)
        try {
          const formData = new FormData()
          formData.append('file', audioBlob, 'recording.webm')
          
          const res = await fetch('http://localhost:8000/transcribe', {
            method: 'POST',
            body: formData
          })
          if (!res.ok) throw new Error("STT server error")
          const data = await res.json()
          
          if (data.text) {
             await handleSend(data.text)
          } else {
             setIsLoading(false)
          }
        } catch (err) {
          console.error("STT Error:", err)
          showError("Failed to transcribe audio. Is the local Audio Engine running?")
          setIsLoading(false)
        }
      }

      mediaRecorder.start()
      setIsRecording(true)
    } catch (err) {
      console.error("Mic access denied:", err)
      showError("Microphone access denied.")
    }
  }

  const handleSend = async (text) => {
    const trimmed = (text ?? inputText).trim()
    if (!trimmed || isLoading) return

    const userMessage = { role: 'user', text: trimmed }
    const newMessages = [...messages, userMessage]
    setMessages(newMessages)
    setInputText('')
    setIsLoading(true)

    try {
      const payload = {
        messages: newMessages.map(m => ({ role: m.role, text: m.text }))
      }
      const response = await catalogApi.getAiRecommendation(payload)

      // Handle add_to_cart action
      if (response.action?.type === 'add_to_cart' && response.action.cartPayload) {
        const { itemId, itemName, price, isVeg, imageUrl, restaurantId } = response.action.cartPayload
        addItem(
          { id: itemId, name: itemName, price, isVeg, imageUrl },
          restaurantId
        )
        showSuccess(`"${itemName}" added to your cart! 🛒`)
      }

      setMessages(prev => [
        ...prev,
        {
          role:                   'model',
          text:                   response.text,
          recommendedRestaurants: response.recommendedRestaurants || [],
          recommendedMenuItems:   response.recommendedMenuItems   || [],
          orderStatus:            response.orderStatus ?? null,
        }
      ])

      // Trigger Local TTS
      if (isVoiceEnabled && response.text) {
        try {
          const formData = new FormData()
          formData.append('text', response.text)
          const audioRes = await fetch('http://localhost:8000/speak', {
            method: 'POST',
            body: formData
          })
          if (audioRes.ok) {
            const blob = await audioRes.blob()
            const url = URL.createObjectURL(blob)
            const audio = new Audio(url)
            audio.play()
          }
        } catch (err) {
          console.error("TTS Error:", err)
        }
      }
    } catch (err) {
      console.error('AI Chat error:', err)
      setMessages(prev => [
        ...prev,
        { role: 'model', text: "Sorry, I'm having trouble connecting right now. Please try again later." }
      ])
    } finally {
      setIsLoading(false)
    }
  }

  const handleAddItemToCart = (item) => {
    if (!item.restaurantId) {
      showError('Cannot add to cart — restaurant info is missing.')
      return
    }
    addItem(
      { id: item.id, name: item.name, price: item.price, isVeg: item.isVeg, imageUrl: item.imageUrl },
      item.restaurantId
    )
    showSuccess(`"${item.name}" added to your cart! 🛒`)
  }

  const lastMsg = messages[messages.length - 1]
  const quickChips = isLoading ? [] : deriveChips(lastMsg)

  return (
    <div className="fixed bottom-6 right-6 z-50 flex flex-col items-end">

      {/* ── Chat Window ── */}
      {isOpen && (
        <div
          className="mb-4 w-80 sm:w-96 bg-white rounded-2xl shadow-2xl border border-slate-100 flex flex-col overflow-hidden"
          style={{ height: '580px', maxHeight: 'calc(100vh - 120px)' }}
        >
          {/* Header */}
          <div className="bg-gradient-to-r from-primary to-primary/80 px-4 py-3 text-white flex justify-between items-center flex-shrink-0">
            <div className="flex items-center gap-3">
              <div className="relative">
                <div className="w-10 h-10 rounded-full border-2 border-white/30 overflow-hidden bg-white/10 shadow-inner group transition-transform hover:scale-110">
                  <img src={biteBotAvatar} alt="BiteBot" className="w-full h-full object-cover animate-pulse-slow" />
                </div>
                {aiStatus === 'online' && (
                  <span className="absolute bottom-0 right-0 w-3 h-3 bg-green-400 border-2 border-primary rounded-full" />
                )}
              </div>
              <div>
                <h3 className="font-bold text-sm tracking-tight">BiteBot</h3>
                <p className="text-[10px] opacity-75 flex items-center gap-1">
                  {aiStatus === 'checking' && (
                    <><span className="w-1.5 h-1.5 rounded-full bg-yellow-300 inline-block animate-pulse" />Connecting...</>
                  )}
                  {aiStatus === 'online' && (
                    <><span className="w-1.5 h-1.5 rounded-full bg-green-400 inline-block" />AI Online · Ready</>  
                  )}
                  {aiStatus === 'offline' && (
                    <><span className="w-1.5 h-1.5 rounded-full bg-red-400 inline-block" />AI Offline</>  
                  )}
                  {aiStatus === 'unknown' && (
                    <><span className="w-1.5 h-1.5 rounded-full bg-slate-300 inline-block" />Checking...</>  
                  )}
                </p>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <button 
                onClick={() => setIsVoiceEnabled(!isVoiceEnabled)} 
                className="text-[10px] bg-white/20 hover:bg-white/30 px-2 py-0.5 rounded-full transition-colors flex items-center gap-1"
                title={isVoiceEnabled ? "Mute Voice" : "Enable Voice"}
              >
                <span className="material-symbols-outlined text-[14px]">{isVoiceEnabled ? 'volume_up' : 'volume_off'}</span>
              </button>
              {aiStatus === 'offline' && (
                <button
                  onClick={checkStatus}
                  title="Retry connection"
                  className="text-[10px] bg-white/20 hover:bg-white/30 px-2 py-0.5 rounded-full transition-colors"
                >
                  🔄 Retry
                </button>
              )}
              <button onClick={() => setIsOpen(false)} className="hover:bg-black/10 p-1 rounded-full transition-colors">
                <span className="material-symbols-outlined text-lg">close</span>
              </button>
            </div>
          </div>

          {/* Messages Area */}
          <div className="flex-1 overflow-y-auto p-4 space-y-4 bg-slate-50">
            {messages.map((msg, idx) => (
              <div key={idx} className={`flex flex-col ${msg.role === 'user' ? 'items-end' : 'items-start'}`}>

                {/* Bubble */}
                <div 
                  className={`max-w-[90%] px-4 py-2.5 rounded-2xl text-sm whitespace-pre-wrap leading-relaxed ${
                    msg.role === 'user'
                      ? 'bg-primary text-white rounded-tr-none'
                      : 'bg-white text-slate-800 border border-slate-100 shadow-sm rounded-tl-none'
                  }`}
                  dangerouslySetInnerHTML={{
                    __html: msg.text
                      ? msg.text
                          .replace(/\*\*(.*?)\*\*/g, '<b class="font-bold">$1</b>')
                          .replace(/\*(.*?)\*/g, '<i class="italic">$1</i>')
                          .replace(/^- (.*)/gm, '• $1')
                          .replace(/^(\d+)\. (.*)/gm, '<b class="font-bold">$1.</b> $2')
                      : ''
                  }}
                />

                {/* Restaurant Cards */}
                {msg.recommendedRestaurants?.length > 0 && (
                  <div className="mt-3 w-full overflow-x-auto pb-2">
                    <div className="flex gap-3 pl-1">
                      {msg.recommendedRestaurants.map(restaurant => (
                        <div key={restaurant.id} className="min-w-[240px] w-60 flex-shrink-0">
                          <RestaurantCard
                            restaurant={restaurant}
                            onOpen={() => { setIsOpen(false); navigate(`/restaurant/${restaurant.id}`) }}
                          />
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {/* Menu Item Chips */}
                {msg.recommendedMenuItems?.length > 0 && (
                  <div className="mt-3 w-full space-y-2">
                    <p className="text-[10px] font-bold text-slate-400 uppercase tracking-wider ml-1">Suggested Dishes</p>
                    {msg.recommendedMenuItems.map(item => (
                      <div
                        key={item.id}
                        className="w-full flex items-center gap-3 p-2.5 bg-white border border-slate-100 rounded-xl shadow-sm"
                      >
                        {/* Thumbnail */}
                        <div className="w-12 h-12 rounded-lg bg-slate-100 overflow-hidden flex-shrink-0">
                          {item.imageUrl
                            ? <img src={item.imageUrl} alt={item.name} className="w-full h-full object-cover" />
                            : <div className="w-full h-full flex items-center justify-center text-xl">🍲</div>
                          }
                        </div>

                        {/* Info */}
                        <div
                          className="flex-1 min-w-0 cursor-pointer"
                          onClick={() => { setIsOpen(false); navigate(`/restaurant/${item.restaurantId}`) }}
                        >
                          <p className="text-xs font-bold text-slate-800 truncate hover:text-primary transition-colors">{item.name}</p>
                          <p className="text-[10px] text-slate-500 truncate">{item.restaurantName || 'View Restaurant'}</p>
                        </div>

                        {/* Price + Add button */}
                        <div className="flex items-center gap-2 flex-shrink-0">
                          <span className="text-xs font-bold text-primary">₹{item.price}</span>
                          <button
                            onClick={() => handleAddItemToCart(item)}
                            title="Add to cart"
                            className="w-7 h-7 rounded-full bg-primary text-white flex items-center justify-center hover:bg-primary/80 transition-colors active:scale-90"
                          >
                            <span className="material-symbols-outlined text-sm">add</span>
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}

                {/* Order Status Card */}
                {msg.orderStatus && (
                  <OrderStatusCard
                    orderStatus={msg.orderStatus}
                    onNavigate={() => { setIsOpen(false); navigate('/orders') }}
                  />
                )}

                {/* Quick Chips (only for last model message) */}
                {idx === messages.length - 1 && msg.role === 'model' && !isLoading && quickChips.length > 0 && (
                  <QuickChips chips={quickChips} onChip={handleSend} />
                )}
              </div>
            ))}

            {/* Offline banner */}
            {aiStatus === 'offline' && (
              <div className="mx-4 mt-3 p-3 bg-red-50 border border-red-100 rounded-xl text-center">
                <p className="text-xs font-semibold text-red-600">⚠️ AI service is currently unreachable</p>
                <p className="text-[10px] text-red-400 mt-0.5">Check your backend is running, then tap Retry</p>
              </div>
            )}

            {isLoading && (
              <div className="flex items-start">
                <div className="bg-white border border-slate-100 shadow-sm rounded-2xl rounded-tl-none px-4 py-3 flex items-center gap-1.5">
                  <div className="w-2 h-2 rounded-full bg-primary/60 animate-bounce" />
                  <div className="w-2 h-2 rounded-full bg-primary/60 animate-bounce" style={{ animationDelay: '0.15s' }} />
                  <div className="w-2 h-2 rounded-full bg-primary/60 animate-bounce" style={{ animationDelay: '0.3s' }} />
                  <span className="text-[10px] text-slate-400 ml-1">Searching...</span>
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          {/* Input Area */}
          <div className="p-3 bg-white border-t border-slate-100 flex-shrink-0">
            <form onSubmit={(e) => { e.preventDefault(); handleSend() }} className="flex gap-2">
              <button
                type="button"
                onClick={handleMicClick}
                disabled={isLoading || aiStatus === 'offline'}
                className={`w-10 h-10 shrink-0 rounded-full flex items-center justify-center transition-all ${
                  isRecording 
                    ? 'bg-red-500 text-white animate-pulse shadow-lg shadow-red-500/30' 
                    : 'bg-slate-100 text-slate-500 hover:bg-slate-200'
                }`}
                title="Hold to talk"
              >
                <span className="material-symbols-outlined text-[20px]">mic</span>
              </button>
              <input
                type="text"
                value={isRecording ? 'Listening...' : inputText}
                onChange={e => setInputText(e.target.value)}
                placeholder={aiStatus === 'offline' ? 'AI offline — tap Retry above' : 'Type or use voice...'}
                className="flex-1 px-4 py-2.5 bg-slate-100 rounded-full text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 transition disabled:opacity-50 min-w-0"
                disabled={isLoading || aiStatus === 'offline' || isRecording}
              />
              <button
                type="submit"
                disabled={!inputText.trim() || isLoading || aiStatus === 'offline' || isRecording}
                className="w-10 h-10 shrink-0 rounded-full bg-primary text-white flex items-center justify-center disabled:opacity-40 hover:bg-primary/90 active:scale-95 transition-all"
              >
                <span className="material-symbols-outlined text-sm">send</span>
              </button>
            </form>
            {!user && (
              <p className="text-center text-[10px] text-slate-400 mt-1.5">
                <button onClick={() => navigate('/login')} className="text-primary underline">Sign in</button> to track orders & reorder
              </p>
            )}
          </div>
        </div>
      )}

      {/* ── Floating AI Button ── */}
      <button
        onClick={handleToggle}
        className={`flex items-center gap-2 px-5 py-3 rounded-2xl shadow-xl transition-all duration-300 font-semibold text-sm ${
          isOpen
            ? 'bg-slate-800 text-white hover:bg-slate-700'
            : 'bg-gradient-to-r from-primary to-indigo-600 text-white hover:shadow-2xl hover:shadow-primary/30 hover:-translate-y-0.5'
        }`}
        aria-label="Chat with BiteBot"
      >
        {isOpen ? (
          <>
            <span className="material-symbols-outlined text-lg">close</span>
            <span>Close</span>
          </>
        ) : (
          <>
            <div className="w-8 h-8 rounded-full border border-white/30 overflow-hidden bg-white/10 shadow-sm">
              <img src={biteBotAvatar} alt="BiteBot" className="w-full h-full object-cover" />
            </div>
            <span>BiteBot</span>
            {/* Status dot */}
            <span className={`w-2 h-2 rounded-full ml-0.5 ${
              aiStatus === 'online'   ? 'bg-emerald-400' :
              aiStatus === 'offline'  ? 'bg-red-400' :
              aiStatus === 'checking' ? 'bg-yellow-400 animate-pulse' :
              'bg-white/50'
            }`} />
          </>
        )}
      </button>
    </div>
  )
}
