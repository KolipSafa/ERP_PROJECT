import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
// Fallback: Rollup wasm build available if native fails (env controlled on Vercel)
import vue from '@vitejs/plugin-vue'
// import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig(({ command }) => ({
  plugins: [
    vue(),
    // Devtools eklentisi build'te peer hatası verdiği için dev dışı kapalı
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
}))
