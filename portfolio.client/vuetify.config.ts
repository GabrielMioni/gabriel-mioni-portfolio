// vuetify.config.ts
import { defineVuetifyConfiguration } from 'vuetify-nuxt-module/custom-configuration'

export default defineVuetifyConfiguration({
  theme: {
    defaultTheme: 'light',
    themes: {
      light: {
        colors: {
          primary: '#1E88E5',
          secondary: '#546E7A',
          success: '#2E7D32',
          warning: '#ED6C02',
          error: '#D32F2F',
          header: '#80CBC4',
          grey: '#6B7280',
          'surface-muted': '#F5F5F5',
          border: '#E0E0E0'
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
