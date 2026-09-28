import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import basicSsl from '@vitejs/plugin-basic-ssl'

export default defineConfig({
  // The backend issues the session as a Secure cookie. A browser only stores/sends
  // a Secure cookie over an actual HTTPS connection (confirmed empirically - a plain
  // http://localhost:5173 page proxying to an https backend does NOT count, the
  // browser only looks at its own connection scheme), so the dev server itself must
  // be HTTPS too, not just the proxy target.
  plugins: [react(), tailwindcss(), basicSsl()],
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:7240',
        changeOrigin: true,
        secure: false, // accept the backend's self-signed local dev certificate
      },
    },
  },
})
