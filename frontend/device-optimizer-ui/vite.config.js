import { existsSync, readFileSync } from 'node:fs'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const config = {
  plugins: [react()],
  server: {
    port: 5173,
  },
}

if (existsSync('certs/localhost.pem')) {
  config.server.https = {
    cert: readFileSync('certs/localhost.pem'),
    key: readFileSync('certs/localhost.key'),
  }
}

export default defineConfig(config)
