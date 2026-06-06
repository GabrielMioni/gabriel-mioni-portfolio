// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/eslint',
    '@nuxt/ui'
  ],
  devtools: {
    enabled: true
  },
  css: ['~/assets/css/main.css'],
  runtimeConfig: {
    public: {
      graphQlBase: process.env.NUXT_PUBLIC_GRAPHQL_BASE || '/graphql'
    }
  },
  routeRules: {
    '/': { prerender: true }
  },
  devServer: {
    port: 3001
  },
  compatibilityDate: '2025-01-15',
  nitro: {
    routeRules: {
      '/graphql': {
        proxy: 'http://localhost:5217/graphql'
      }
    }
  },
  eslint: {
    config: {
      stylistic: {
        commaDangle: 'never',
        braceStyle: '1tbs'
      }
    }
  }
})
