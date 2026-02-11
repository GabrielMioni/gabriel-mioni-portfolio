// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  devServer: {
    port: 3000
  },
  modules: [
    '@nuxt/eslint',
    'vuetify-nuxt-module'
  ],
  runtimeConfig: {
    public: {
      apiBase: '/api'
    }
  },
  nitro: {
    routeRules: {
      '/api/**': {
        proxy: 'http://localhost:5217/api/**'
      }
    }
  }
})
