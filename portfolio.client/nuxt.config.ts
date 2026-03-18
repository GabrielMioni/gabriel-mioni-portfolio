export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  css: ['~/assets/scss/main.scss'],
  devtools: { enabled: true },
  devServer: {
    port: 3000
  },
  components: [
    { path: '~/components', pathPrefix: false }
  ],
  modules: [
    '@nuxt/eslint',
    '@vueuse/nuxt',
    'vuetify-nuxt-module'
  ],
  runtimeConfig: {
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE || '/api',
      graphQlBase: process.env.NUXT_PUBLIC_GRAPHQL_BASE || '/graphql',
      storageBase: process.env.NUXT_PUBLIC_STORAGE_BASE || ''
    }
  },
  nitro: {
    routeRules: {
      '/api/**': {
        proxy: 'http://localhost:5217/api/**'
      },
      '/graphql': {
        proxy: 'http://localhost:5217/graphql'
      }
    }
  },
  vuetify: {
    vuetifyOptions: './vuetify.config.ts'
  }
})
