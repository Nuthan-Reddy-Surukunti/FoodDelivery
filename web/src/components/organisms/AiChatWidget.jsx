import { useState, useRef, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import catalogApi from '../../services/catalogApi'
import { RestaurantCard } from '../molecules/RestaurantCard'

export const AiChatWidget = () => {
  const [isOpen, setIsOpen] = useState(false)
  const [messages, setMessages] = useState([
    { role: 'model', text: "Hi! I'm QuickBite's AI Assistant. What kind of food are you looking for today?", recommendedRestaurants: [] }
  ])
  const [inputText, setInputText] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const messagesEndRef = useRef(null)
  const navigate = useNavigate()

  useEffect(() => {
    if (isOpen) {
      messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
    }
  }, [messages, isOpen])

  const handleSend = async (e) => {
    e?.preventDefault()
    if (!inputText.trim() || isLoading) return

    const userMessage = { role: 'user', text: inputText.trim() }
    const newMessages = [...messages, userMessage]
    setMessages(newMessages)
    setInputText('')
    setIsLoading(true)

    try {
      // Send only role and text to backend
      const payload = {
        messages: newMessages.map(m => ({ role: m.role, text: m.text }))
      }
      
      const response = await catalogApi.getAiRecommendation(payload)
      
      setMessages(prev => [
        ...prev, 
        { 
          role: 'model', 
          text: response.text, 
          recommendedRestaurants: response.recommendedRestaurants || [] 
        }
      ])
    } catch (err) {
      console.error('AI Chat error:', err)
      setMessages(prev => [
        ...prev, 
        { role: 'model', text: "Sorry, I'm having trouble connecting right now. Please try again later.", recommendedRestaurants: [] }
      ])
    } finally {
      setIsLoading(false)
    }
  }

  const handleRestaurantClick = (restaurant) => {
    setIsOpen(false)
    navigate(`/restaurant/${restaurant.id}`)
  }

  return (
    <div className="fixed bottom-6 right-6 z-50 flex flex-col items-end">
      {/* Chat Window */}
      {isOpen && (
        <div className="mb-4 w-80 sm:w-96 bg-white rounded-2xl shadow-xl border border-slate-100 flex flex-col overflow-hidden animate-fade-in-up origin-bottom-right" style={{ height: '500px', maxHeight: 'calc(100vh - 120px)' }}>
          {/* Header */}
          <div className="bg-primary px-4 py-3 text-white flex justify-between items-center">
            <div className="flex items-center gap-2">
              <span className="text-2xl">✨</span>
              <div>
                <h3 className="font-semibold text-sm">QuickBite Assistant</h3>
                <p className="text-xs text-primary-container opacity-80">Powered by AI</p>
              </div>
            </div>
            <button onClick={() => setIsOpen(false)} className="hover:bg-black/10 p-1 rounded-full transition-colors">
              <span className="material-symbols-outlined text-lg">close</span>
            </button>
          </div>

          {/* Messages Area */}
          <div className="flex-1 overflow-y-auto p-4 space-y-4 bg-slate-50">
            {messages.map((msg, idx) => (
              <div key={idx} className={`flex flex-col ${msg.role === 'user' ? 'items-end' : 'items-start'}`}>
                <div 
                  className={`max-w-[85%] px-4 py-2 rounded-2xl ${
                    msg.role === 'user' 
                      ? 'bg-primary text-white rounded-tr-none' 
                      : 'bg-white text-slate-800 border border-slate-100 shadow-sm rounded-tl-none'
                  }`}
                >
                  <p className="text-sm">{msg.text}</p>
                </div>
                
                {/* Render recommendations if any */}
                {msg.recommendedRestaurants && msg.recommendedRestaurants.length > 0 && (
                  <div className="mt-3 w-full space-y-3 pl-2 border-l-2 border-primary/30">
                    {msg.recommendedRestaurants.map(restaurant => (
                      <div key={restaurant.id} className="w-full sm:w-64">
                         <RestaurantCard restaurant={restaurant} onOpen={handleRestaurantClick} />
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ))}
            
            {isLoading && (
              <div className="flex items-start">
                 <div className="bg-white border border-slate-100 shadow-sm rounded-2xl rounded-tl-none px-4 py-3 flex gap-1">
                   <div className="w-2 h-2 rounded-full bg-slate-300 animate-bounce" />
                   <div className="w-2 h-2 rounded-full bg-slate-300 animate-bounce" style={{ animationDelay: '0.2s' }} />
                   <div className="w-2 h-2 rounded-full bg-slate-300 animate-bounce" style={{ animationDelay: '0.4s' }} />
                 </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          {/* Input Area */}
          <div className="p-3 bg-white border-t border-slate-100">
            <form onSubmit={handleSend} className="flex gap-2">
              <input
                type="text"
                value={inputText}
                onChange={e => setInputText(e.target.value)}
                placeholder="Ask for recommendations..."
                className="flex-1 px-4 py-2 bg-slate-100 rounded-full text-sm focus:outline-none focus:ring-2 focus:ring-primary/50"
                disabled={isLoading}
              />
              <button 
                type="submit" 
                disabled={!inputText.trim() || isLoading}
                className="w-10 h-10 rounded-full bg-primary text-white flex items-center justify-center disabled:opacity-50 hover:bg-primary/90 transition-colors"
              >
                <span className="material-symbols-outlined text-sm">send</span>
              </button>
            </form>
          </div>
        </div>
      )}

      {/* Floating Button */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="w-14 h-14 rounded-full bg-primary text-white shadow-lg flex items-center justify-center hover:scale-105 active:scale-95 transition-all"
        aria-label="AI Assistant"
      >
        {isOpen ? (
          <span className="material-symbols-outlined text-2xl">close</span>
        ) : (
          <span className="text-3xl">✨</span>
        )}
      </button>
    </div>
  )
}
