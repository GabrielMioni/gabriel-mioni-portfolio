// https://nuxt.com/docs/api/configuration/nuxt-config
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
      apiBase: '/api',
      graphQlBase: '/graphql'
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
  }
})
