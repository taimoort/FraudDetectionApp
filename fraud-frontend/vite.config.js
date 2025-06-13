import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // Any request that starts with /api goes to the .NET service
      '/api': 'http://localhost:5000'   // ← use whatever port FraudDetectorService is on
    }
  }
});