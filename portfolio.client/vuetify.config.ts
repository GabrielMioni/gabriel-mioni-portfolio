// vuetify.config.ts
import { defineVuetifyConfiguration } from 'vuetify-nuxt-module/custom-configuration'

export default defineVuetifyConfiguration({
  theme: {
    themes: {
      light: {
        colors: {
          header: '#80CBC4'
        }
      },
      dark: {
        colors: {
          header: '#1e293b'
        }
      }
    }
  }
})
