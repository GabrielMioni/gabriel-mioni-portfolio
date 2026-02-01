// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  modules: ['@nuxt/eslint'],
  runtimeConfig: {
    public: {
      apiBase: '/api'
    }
  },
  nitro: {
    routeRules: {
      '/api/**': {
        proxy: 'http://127.0.0.1:5217/api/**'
      }
    }
  }
})