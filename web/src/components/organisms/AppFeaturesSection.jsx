import React from 'react';

const FloatingCard = ({ iconSrc, title, positionClasses, delay }) => (
  <div 
    className={`absolute bg-white rounded-3xl shadow-xl border border-slate-100 p-5 flex flex-col items-center justify-center gap-3 transform transition-transform hover:scale-105 cursor-pointer z-20 ${positionClasses}`}
    style={{ animation: `float 6s ease-in-out infinite ${delay}s` }}
  >
    <img src={iconSrc} alt={title} className="w-20 h-20 object-contain drop-shadow-md" />
    <span className="text-sm font-bold text-slate-700 whitespace-nowrap">{title}</span>
  </div>
);

export const AppFeaturesSection = () => {
  return (
    <section className="relative w-full overflow-hidden bg-white py-20 pb-32">
      {/* Background Gradient */}
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-rose-50 via-white to-white pointer-events-none z-0" />
      
      <div className="relative z-10 max-w-7xl mx-auto px-6">
        {/* Header */}
        <div className="text-center mb-16 md:mb-24">
          <h2 className="text-4xl md:text-5xl font-extrabold text-rose-500 mb-4 tracking-tight">
            What's waiting for you on QuickBite?
          </h2>
          <p className="text-lg md:text-xl text-slate-500 font-medium max-w-2xl mx-auto">
            Our app is packed with features that enable you to experience food delivery like never before
          </p>
        </div>

        {/* Centerpiece & Floating Cards Container */}
        <div className="relative max-w-4xl mx-auto h-[450px] md:h-[550px] flex items-center justify-center">
          
          {/* Floating Cards (Desktop Absolute Layout) */}
          <div className="hidden md:block absolute inset-0">
            {/* Left Side */}
            <FloatingCard 
              iconSrc="/images/features/icon_healthy.png" 
              title="Healthy" 
              positionClasses="-left-12 top-10" 
              delay={0}
            />
            <FloatingCard 
              iconSrc="/images/features/icon_veg.png" 
              title="Veg Mode" 
              positionClasses="left-8 top-1/2 -translate-y-1/2" 
              delay={1.5}
            />
            <FloatingCard 
              iconSrc="/images/features/icon_party.png" 
              title="Plan a Party" 
              positionClasses="-left-6 bottom-10" 
              delay={0.8}
            />

            {/* Right Side */}
            <FloatingCard 
              iconSrc="/images/features/icon_offers.png" 
              title="Offers" 
              positionClasses="-right-12 top-10" 
              delay={1}
            />
            <FloatingCard 
              iconSrc="/images/features/icon_gourmet.png" 
              title="Gourmet" 
              positionClasses="right-8 top-1/2 -translate-y-1/2" 
              delay={0.5}
            />
            <FloatingCard 
              iconSrc="/images/features/icon_schedule.png" 
              title="Schedule" 
              positionClasses="-right-6 bottom-10" 
              delay={2}
            />
          </div>

          {/* Center Food Asset */}
          <div className="relative z-10 animate-float-slow">
            <div className="absolute inset-0 bg-rose-500/10 blur-[100px] rounded-full pointer-events-none" />
            <img 
              src="/images/features/center_food.png" 
              alt="Gourmet Burger" 
              className="w-80 h-80 md:w-[500px] md:h-[500px] object-contain drop-shadow-2xl relative z-10"
            />
          </div>

        </div>

        {/* Mobile Fallback Grid (Visible only on small screens) */}
        <div className="md:hidden mt-12 grid grid-cols-2 sm:grid-cols-3 gap-4">
           {[
             {src: '/images/features/icon_healthy.png', title: 'Healthy'},
             {src: '/images/features/icon_veg.png', title: 'Veg Mode'},
             {src: '/images/features/icon_party.png', title: 'Plan a Party'},
             {src: '/images/features/icon_offers.png', title: 'Offers'},
             {src: '/images/features/icon_gourmet.png', title: 'Gourmet'},
             {src: '/images/features/icon_schedule.png', title: 'Schedule'}
           ].map(item => (
             <div key={item.title} className="bg-white rounded-2xl shadow-md border border-slate-100 p-5 flex flex-col items-center justify-center gap-3">
                <img src={item.src} alt={item.title} className="w-16 h-16 object-contain" />
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
