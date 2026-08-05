// vuetify.config.ts
import { defineVuetifyConfiguration } from 'vuetify-nuxt-module/custom-configuration'

export default defineVuetifyConfiguration({
  defaults: {
    VBtn: {
      style: 'text-transform: none;'
    }
  },
  theme: {
    defaultTheme: 'light',
    themes: {
      light: {
        colors: {
          background: '#F3F1EA',
          surface: '#FFFEFB',
          primary: '#087A65',
          secondary: '#52635E',
          success: '#278553',
          warning: '#C7771A',
          error: '#BC3C35',
          header: '#DCEBE5',
          grey: '#68716E',
          'surface-muted': '#EAE7DF',
          border: '#D7D3C9',
          'nav-surface': '#1C1E1E',
          'nav-active': '#C8F0E1'
        }
      },
      dark: {
        colors: {
          primary: '#90CAF9',
          secondary: '#B0BEC5',
          success: '#66BB6A',
          warning: '#FFB74D',
          error: '#EF5350',
          header: '#1e293b',
          grey: '#9CA3AF',
          'surface-muted': '#1F2933',
          border: '#374151'
        }
      }
    }
  }
})
