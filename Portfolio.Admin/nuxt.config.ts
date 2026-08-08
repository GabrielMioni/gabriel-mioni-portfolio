const apiOrigin = process.env.NUXT_API_ORIGIN || 'http://localhost:5217'
const adminOrigin = new URL(
  process.env.NUXT_ADMIN_ORIGIN || 'http://localhost:3000'
)
const isEndToEnd = process.env.NUXT_END_TO_END === 'true'
const adminProxyHeaders = {
  'x-forwarded-host': adminOrigin.host,
  'x-forwarded-proto': adminOrigin.protocol.replace(':', '')
}

export default defineNuxtConfig({
  buildDir: process.env.NUXT_BUILD_DIR || '.nuxt',
  compatibilityDate: '2025-07-15',
  ssr: false,
  css: ['~/assets/scss/main.scss'],
  devtools: { enabled: !isEndToEnd },
  devServer: {
    port: 3000
  },
  components: [
    { path: '~/components', pathPrefix: false }
  ],
  modules: ['@nuxt/eslint', '@vueuse/nuxt', 'vuetify-nuxt-module', '@pinia/nuxt'],
  runtimeConfig: {
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE || '/api',
      authBase: process.env.NUXT_PUBLIC_AUTH_BASE || '/api',
      graphQlBase: process.env.NUXT_PUBLIC_GRAPHQL_BASE || '/graphql/admin',
      storageBase: process.env.NUXT_PUBLIC_STORAGE_BASE || ''
    }
  },
  routeRules: {
    '/api/**': {
      proxy: {
        to: `${apiOrigin}/api/**`,
        headers: adminProxyHeaders,
        fetchOptions: {
          redirect: 'manual'
        }
      }
    },
    '/graphql/admin': {
      proxy: {
        to: `${apiOrigin}/graphql/admin`,
        headers: adminProxyHeaders
      }
    },
    '/graphql': {
      proxy: {
        to: `${apiOrigin}/graphql`,
        headers: adminProxyHeaders
      }
    }
  },
  vite: {
    optimizeDeps: {
      include: [
        '@urql/exchange-graphcache',
        '@urql/vue',
        'date-fns',
        'pica',
        'vuedraggable'
      ]
    }
  },
  vuetify: {
    vuetifyOptions: './vuetify.config.ts'
  }
})
