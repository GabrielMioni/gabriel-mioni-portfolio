const isEndToEnd = process.env.NUXT_END_TO_END === 'true'
const isHostedBuild = !isEndToEnd
  && (process.env.CI === 'true' || process.env.CF_PAGES === '1')

function readOrigin (name: string, localFallback: string) {
  const value = process.env[name] || (isHostedBuild ? undefined : localFallback)

  if (!value) {
    throw new Error(`${name} must be configured for hosted builds.`)
  }

  let url: URL

  try {
    url = new URL(value)
  }
  catch {
    throw new Error(`${name} must be a valid absolute URL.`)
  }

  if (!['http:', 'https:'].includes(url.protocol)
    || url.username
    || url.password
    || url.pathname !== '/'
    || url.search
    || url.hash) {
    throw new Error(`${name} must be an HTTP(S) origin without a path, query, or fragment.`)
  }

  if (isHostedBuild && url.protocol !== 'https:') {
    throw new Error(`${name} must use HTTPS for hosted builds.`)
  }

  return url
}

function readStorageBase () {
  const value = process.env.NUXT_PUBLIC_STORAGE_BASE

  if (!value) {
    if (isHostedBuild) {
      throw new Error('NUXT_PUBLIC_STORAGE_BASE must be configured for hosted builds.')
    }

    return ''
  }

  let url: URL

  try {
    url = new URL(value)
  }
  catch {
    throw new Error('NUXT_PUBLIC_STORAGE_BASE must be a valid absolute URL.')
  }

  if (!['http:', 'https:'].includes(url.protocol)) {
    throw new Error('NUXT_PUBLIC_STORAGE_BASE must use HTTP or HTTPS.')
  }

  if (isHostedBuild && url.protocol !== 'https:') {
    throw new Error('NUXT_PUBLIC_STORAGE_BASE must use HTTPS for hosted builds.')
  }

  return value.replace(/\/$/, '')
}

const apiOrigin = readOrigin('NUXT_API_ORIGIN', 'http://localhost:5217')
const adminOrigin = readOrigin('NUXT_ADMIN_ORIGIN', 'http://localhost:3000')
const storageBase = readStorageBase()
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
      storageBase
    }
  },
  routeRules: {
    '/api/**': {
      proxy: {
        to: `${apiOrigin.origin}/api/**`,
        headers: adminProxyHeaders,
        fetchOptions: {
          redirect: 'manual'
        }
      }
    },
    '/graphql/admin': {
      proxy: {
        to: `${apiOrigin.origin}/graphql/admin`,
        headers: adminProxyHeaders
      }
    },
    '/graphql': {
      proxy: {
        to: `${apiOrigin.origin}/graphql`,
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
