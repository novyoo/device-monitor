import { readFileSync } from 'node:fs'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    https: {
      cert: readFileSync('certs/localhost.pem'),
      key: readFileSync('certs/localhost.key'),
    },
  },
})
