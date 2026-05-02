import { Link } from 'react-router-dom'

const helpCards = [
  {
    title: 'Order Help',
    description: 'Report missing items, wrong orders, or late deliveries.',
    action: 'Get Order Help',
    to: '/orders',
    icon: 'receipt_long',
  },
  {
    title: 'Track an Order',
    description: 'See live updates for your active delivery.',
    action: 'Track Order',
    to: '/orders',
    icon: 'location_on',
  },
  {
    title: 'Refunds & Credits',
    description: 'Understand refund timelines and credit policies.',
    action: 'Learn More',
    to: '/help#refunds',
    icon: 'currency_exchange',
  },
]

const faqItems = [
  {
    question: 'How do I apply a promo code?',
    answer: 'Add your code at checkout. If it is valid, the discount will apply instantly.',
  },
  {
    question: 'Can I change my delivery address after ordering?',
    answer: 'If the order is not yet in preparation, you can update the address from Orders.',
  },
  {
    question: 'What if my order arrives late?',
    answer: 'Open Order Help to report the delay and we will review it for credits.',
  },
]

export const HelpSupportPage = () => {
  return (
    <div className="mx-auto max-w-6xl px-4 py-10">
      <div className="mb-8">
        <p className="text-sm font-semibold uppercase tracking-wider text-primary">Help & Support</p>
        <h1 className="text-3xl md:text-4xl font-bold text-slate-900 mt-2">How can we help?</h1>
        <p className="text-base text-slate-600 mt-3 max-w-2xl">
          Get quick answers, track your delivery, or contact support if you need help right away.
        </p>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        {helpCards.map((card) => (
          <div
            key={card.title}
            className="rounded-2xl border border-slate-200 bg-white/80 p-6 shadow-sm hover:shadow-md transition-shadow"
          >
            <div className="flex items-center gap-3 text-primary">
              <span className="material-symbols-outlined text-2xl">{card.icon}</span>
              <h2 className="text-lg font-semibold text-slate-900">{card.title}</h2>
            </div>
            <p className="text-sm text-slate-600 mt-3">{card.description}</p>
            <Link
              to={card.to}
              className="inline-flex items-center gap-2 mt-5 text-sm font-semibold text-primary hover:text-indigo-600"
            >
              {card.action}
              <span className="material-symbols-outlined text-base">arrow_forward</span>
            </Link>
          </div>
        ))}
      </div>

      <div id="refunds" className="mt-12 grid gap-8 md:grid-cols-[1.2fr_1fr]">
        <div className="rounded-2xl border border-slate-200 bg-white/80 p-6">
          <h3 className="text-xl font-semibold text-slate-900">Contact Support</h3>
          <p className="text-sm text-slate-600 mt-2">
            Need a real person? Reach out and we will respond as fast as possible.
          </p>
          <div className="mt-5 space-y-3 text-sm text-slate-700">
            <div className="flex items-center gap-2">
              <span className="material-symbols-outlined text-base text-primary">mail</span>
              <span>surkuntinuthanreddy@gmail.com</span>
            </div>
            <div className="flex items-center gap-2">
              <span className="material-symbols-outlined text-base text-primary">call</span>
              <span>+91 9010256545</span>
            </div>
            <div className="flex items-center gap-2">
              <span className="material-symbols-outlined text-base text-primary">schedule</span>
              <span>Available 8am - 11pm, daily</span>
            </div>
          </div>
        </div>

        <div className="rounded-2xl border border-slate-200 bg-white/80 p-6">
          <h3 className="text-xl font-semibold text-slate-900">Refunds & Credits</h3>
          <p className="text-sm text-slate-600 mt-2">
            If your order is missing items or arrives late, report it within 24 hours to be considered for credits.
          </p>
          <Link
            to="/orders"
            className="inline-flex items-center gap-2 mt-4 text-sm font-semibold text-primary hover:text-indigo-600"
          >
            Go to Orders
            <span className="material-symbols-outlined text-base">arrow_forward</span>
          </Link>
        </div>
      </div>

      <div className="mt-12">
        <h3 className="text-xl font-semibold text-slate-900">Common Questions</h3>
        <div className="mt-4 space-y-4">
          {faqItems.map((faq) => (
            <div key={faq.question} className="rounded-2xl border border-slate-200 bg-white/80 p-5">
              <p className="text-sm font-semibold text-slate-900">{faq.question}</p>
              <p className="text-sm text-slate-600 mt-2">{faq.answer}</p>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}

export default HelpSupportPage
