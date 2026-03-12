/** @type {import('tailwindcss').Config} */
export default {
  content: ['./src/**/*.{html,js,ts,vue}'],
  theme: {
    extend: {
      colors: {
        navy: {
          950: '#0a0f1e',
          900: '#0f172a',
          800: '#1e2a45',
          700: '#2d3f6b',
          600: '#3d5499',
        },
      },
    },
  },
  plugins: [],
}
