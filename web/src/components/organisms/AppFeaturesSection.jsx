import React from 'react';

const FloatingCard = ({ iconSrc, title, positionClasses, delay }) => (
  <div className={`absolute z-20 ${positionClasses}`}>
    <div 
      className="bg-white rounded-3xl shadow-xl border border-slate-100 p-5 flex flex-col items-center justify-center gap-3 transform transition-transform hover:scale-105 cursor-pointer"
      style={{ animation: `float 6s ease-in-out infinite ${delay}s` }}
    >
      <img src={iconSrc} alt={title} className="w-20 h-20 object-contain drop-shadow-md" />
      <span className="text-sm font-bold text-slate-700 whitespace-nowrap">{title}</span>
    </div>
  </div>
);

const featureCards = [
  {
    iconSrc: '/images/features/icon_veg_filter_new.png',
    title: 'Veg Filter',
    label: 'Vegetarian-only browsing',
    positionClasses: 'left-0 top-6',
    delay: 0,
  },
  {
    iconSrc: '/images/features/icon_search_new.png',
    title: 'Search',
    label: 'Find restaurants and dishes',
    positionClasses: '-left-16 top-1/2 -translate-y-1/2',
    delay: 1.5,
  },
  {
    iconSrc: '/images/features/icon_coupons_new.png',
    title: 'Coupons',
    label: 'Apply promo codes in cart',
    positionClasses: 'left-0 bottom-6',
    delay: 0.8,
  },
  {
    iconSrc: '/images/features/icon_cart_new.png',
    title: 'Cart',
    label: 'Review items before checkout',
    positionClasses: 'right-0 top-6',
    delay: 1,
  },
  {
    iconSrc: '/images/features/icon_tracking_new.png',
    title: 'Tracking',
    label: 'Follow live order status',
    positionClasses: '-right-16 top-1/2 -translate-y-1/2',
    delay: 0.5,
  },
  {
    iconSrc: '/images/features/icon_browse_new.png',
    title: 'Browse',
    label: 'Explore cuisines and restaurants',
    positionClasses: 'right-0 bottom-6',
    delay: 2,
  },
];

export const AppFeaturesSection = () => {
  return (
    <section className="relative w-full overflow-hidden bg-white py-20 pb-32">
      {/* Background Gradient */}
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-rose-50 via-white to-white pointer-events-none z-0" />
      
      <div className="relative z-10 max-w-7xl mx-auto px-6">
        {/* Header */}
        <div className="text-center mb-16 md:mb-24">
          <h2 className="text-4xl md:text-5xl font-extrabold text-rose-500 mb-4 tracking-tight">
            Everything you need to order on QuickBite
          </h2>
          <p className="text-lg md:text-xl text-slate-500 font-medium max-w-2xl mx-auto">
            Search, browse, customize, checkout, and track your food with the features already live in the app.
          </p>
        </div>

        {/* Centerpiece & Floating Cards Container */}
        <div className="relative max-w-4xl mx-auto h-[450px] md:h-[550px] flex items-center justify-center">
          
          {/* Floating Cards (Desktop Absolute Layout) */}
          <div className="hidden md:block absolute inset-0">
            {featureCards.map((card) => (
              <FloatingCard
                key={card.title}
                iconSrc={card.iconSrc}
                title={card.title}
                positionClasses={card.positionClasses}
                delay={card.delay}
              />
            ))}
          </div>

          {/* Center Food Asset */}
          <div className="relative z-10 animate-float-slow">
            <div className="absolute inset-0 bg-rose-500/10 blur-[100px] rounded-full pointer-events-none" />
            <img 
              src="/images/features/center_food.png" 
              alt="QuickBite food spotlight" 
              className="w-80 h-80 md:w-[500px] md:h-[500px] object-contain drop-shadow-2xl relative z-10"
            />
          </div>

        </div>

        {/* Mobile Fallback Grid (Visible only on small screens) */}
        <div className="md:hidden mt-12 grid grid-cols-2 sm:grid-cols-3 gap-4">
           {featureCards.map(item => (
             <div key={item.title} className="bg-white rounded-2xl shadow-md border border-slate-100 p-5 flex flex-col items-center justify-center gap-3 text-center">
                <img src={item.iconSrc} alt={item.title} className="w-16 h-16 object-contain" />
                <span className="text-xs font-bold text-slate-700 text-center">{item.title}</span>
             </div>
           ))}
        </div>

      </div>
      
      <style dangerouslySetInnerHTML={{__html: `
        @keyframes float {
          0% { transform: translateY(0px); }
          50% { transform: translateY(-15px); }
          100% { transform: translateY(0px); }
        }
        .animate-float-slow {
          animation: float 8s ease-in-out infinite;
        }
      `}} />
    </section>
  );
};
