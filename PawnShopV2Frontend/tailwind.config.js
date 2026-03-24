/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        psms: {
          bg: '#0f172a',
          panel: '#111827',
          card: '#1f2937',
          muted: '#94a3b8'
        }
      }
    },
  },
  plugins: [],
}
