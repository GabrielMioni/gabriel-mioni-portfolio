// https://nuxt.com/docs/api/configuration/nuxt-config
const apiOrigin = process.env.NUXT_API_ORIGIN || 'http://localhost:5217'
const isEndToEnd = process.env.NUXT_END_TO_END === 'true'

export default defineNuxtConfig({
  buildDir: process.env.NUXT_BUILD_DIR || '.nuxt',
  modules: [
    '@nuxt/eslint',
    '@nuxt/ui'
  ],
  components: {
    dirs: [{ path: '~/components', pathPrefix: false }]
  },
  devtools: {
    enabled: !isEndToEnd
  },
  css: ['~/assets/css/main.css'],
  colorMode: {
    preference: 'dark'
  },
  ui: {
    theme: {
      colors: ['primary', 'secondary', 'info', 'success', 'warning', 'error', 'neutral']
    }
  },
  runtimeConfig: {
    public: {
      graphQlBase: process.env.NUXT_PUBLIC_GRAPHQL_BASE || '/graphql',
      storageBase: process.env.NUXT_PUBLIC_STORAGE_BASE || ''
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
        proxy: `${apiOrigin}/graphql`
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
