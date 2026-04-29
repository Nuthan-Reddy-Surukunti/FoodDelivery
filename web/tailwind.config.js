/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  darkMode: 'class',
  theme: {
    fontFamily: {
      inter: ['"Inter"', 'sans-serif'],
      sans: ['"Inter"', 'sans-serif'],
    },
    extend: {
      colors: {
        // Horizon UI Color Palette
        primary: '#e11d48', // rose-600 (Zomato-inspired red)
        'primary-container': '#be123c', // rose-700
        'on-primary': '#ffffff',
        'on-primary-container': '#ffffff',
        'inverse-primary': '#fda4af', // rose-300
        'primary-fixed': '#ffe4e6', // rose-100
        'primary-fixed-dim': '#fecdd3', // rose-200
        'on-primary-fixed': '#881337', // rose-900
        'on-primary-fixed-variant': '#9f1239', // rose-800

        secondary: '#475569',
        'secondary-container': '#e2e8f0',
        'on-secondary': '#ffffff',
        'on-secondary-container': '#0f172a',
        'secondary-fixed': '#cbd5e1',
        'secondary-fixed-dim': '#94a3b8',
        'on-secondary-fixed': '#1e293b',
        'on-secondary-fixed-variant': '#334155',

        tertiary: '#64748b',
        'tertiary-container': '#f1f5f9',
        'on-tertiary': '#ffffff',
        'on-tertiary-container': '#0f172a',
        'tertiary-fixed': '#e2e8f0',
        'tertiary-fixed-dim': '#cbd5e1',
        'on-tertiary-fixed': '#0f172a',
        'on-tertiary-fixed-variant': '#334155',

        error: '#ba1a1a',
        'on-error': '#ffffff',
        'error-container': '#ffdad6',
        'on-error-container': '#93000a',

        background: '#f8fafc',
        'on-background': '#0f172a',
        surface: '#ffffff',
        'surface-dim': '#e2e8f0',
        'surface-bright': '#ffffff',
        'surface-container-lowest': '#ffffff',
        'surface-container-low': '#f8fafc',
        'surface-container': '#f8fafc',
        'surface-container-high': '#e2e8f0',
        'surface-container-highest': '#cbd5e1',
        'on-surface': '#0f172a',
        'on-surface-variant': '#475569',
        'inverse-surface': '#1e293b',
        'inverse-on-surface': '#f1f5f9',
        outline: '#94a3b8',
        'outline-variant': '#cbd5e1',
        'surface-tint': '#e11d48', // rose-600
        'surface-variant': '#f1f5f9',
      },
      borderRadius: {
        sm: '0.25rem',
        DEFAULT: '0.75rem',
        md: '0.75rem',
        lg: '1rem',
        xl: '1.5rem',
        '2xl': '1.125rem',
        full: '9999px',
      },
      spacing: {
        'unit': '4px',
        'stack-sm': '8px',
        'stack-md': '16px',
        'stack-lg': '32px',
        'gutter': '16px',
        'container-padding': '24px',
      },
      fontSize: {
        'caption-sm': ['12px', { lineHeight: '1.4', fontWeight: '400' }],
        'label-md': ['14px', { lineHeight: '1.43', fontWeight: '500' }],
        'body-md': ['14px', { lineHeight: '1.43', fontWeight: '400' }],
        'body-lg': ['16px', { lineHeight: '1.5', fontWeight: '400' }],
        'title-lg': ['18px', { lineHeight: '1.33', fontWeight: '600' }],
        'price-lg': ['20px', { lineHeight: '1.0', fontWeight: '700' }],
        'headline-md': ['20px', { lineHeight: '1.25', fontWeight: '600' }],
        'headline-lg': ['24px', { lineHeight: '1.33', fontWeight: '700' }],
        'display-xl': ['34px', { lineHeight: '1.24', letterSpacing: '-0.02em', fontWeight: '700' }],
      },
      boxShadow: {
        'ambient': '0 4px 16px rgba(25, 120, 229, 0.08)',
        'soft-xl': '0 20px 40px -15px rgba(0,0,0,0.05)',
        'glow': '0 0 20px rgba(25, 120, 229, 0.4)',
        'glow-sm': '0 0 10px rgba(25, 120, 229, 0.2)',
      },
      animation: {
        'fade-in-up': 'fadeInUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards',
        'fade-in': 'fadeIn 0.4s ease-out forwards',
        'float': 'float 6s ease-in-out infinite',
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'shimmer': 'shimmer 2s linear infinite',
      },
      keyframes: {
        fadeInUp: {
          '0%': { opacity: '0', transform: 'translateY(20px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        float: {
          '0%, 100%': { transform: 'translateY(0)' },
          '50%': { transform: 'translateY(-10px)' },
        },
        shimmer: {
          '0%': { backgroundPosition: '-1000px 0' },
          '100%': { backgroundPosition: '1000px 0' },
        }
      },
      backgroundImage: {
        'gradient-radial': 'radial-gradient(var(--tw-gradient-stops))',
        'gradient-mesh': 'radial-gradient(at 40% 20%, hsla(213,100%,82%,1) 0px, transparent 50%), radial-gradient(at 80% 0%, hsla(189,100%,76%,1) 0px, transparent 50%), radial-gradient(at 0% 50%, hsla(240,100%,86%,1) 0px, transparent 50%)',
      }
    },
  },
  plugins: [],
}
