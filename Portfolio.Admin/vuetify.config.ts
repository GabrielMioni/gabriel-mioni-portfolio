// vuetify.config.ts
import { defineVuetifyConfiguration } from 'vuetify-nuxt-module/custom-configuration'

const lightThemeColors = {
  background: '#E8E2D2',
  surface: '#F3EFE3',
  'surface-muted': '#DDD7C6',
  'surface-bright': '#FBF8ED',
  'on-background': '#22231F',
  'on-surface': '#22231F',
  primary: '#0D718B',
  'on-primary': '#FFFFFF',
  secondary: '#814236',
  'on-secondary': '#FFFFFF',
  success: '#2F755B',
  warning: '#FFBA00',
  'on-warning': '#22231F',
  error: '#9C3F35',
  info: '#168AA8',
  header: '#D8D1BE',
  grey: '#6F6B5E',
  border: '#AAA38E',
  paper: '#E8E2D2',
  'paper-raised': '#F3EFE3',
  ink: '#22231F',
  muted: '#6F6B5E',
  rule: '#AAA38E',
  amber: '#FFBA00',
  coral: '#D98F87',
  cyan: '#168AA8',
  rust: '#814236',
  // Public's project-card hover: 22% amber mixed with raised paper.
  'nav-surface': '#F6E3B1',
  'nav-active': '#814236',
  'nav-active-surface': '#F8D77F',
  'nav-ink': '#22231F',
  'nav-muted': '#6F6B5E',
  'nav-rule': '#AAA38E'
}

const darkThemeColors = {
  background: '#151A17',
  surface: '#1D231F',
  'surface-muted': '#29312B',
  'surface-bright': '#252C27',
  'on-background': '#E3DDC9',
  'on-surface': '#E3DDC9',
  primary: '#70B5BD',
  'on-primary': '#151A17',
  secondary: '#CF8278',
  'on-secondary': '#151A17',
  success: '#70AD8C',
  warning: '#FFB000',
  'on-warning': '#151A17',
  error: '#E08C82',
  'on-error': '#151A17',
  info: '#70B5BD',
  header: '#29312B',
  grey: '#9B9C8B',
  border: '#465047',
  paper: '#151A17',
  'paper-raised': '#1D231F',
  ink: '#E3DDC9',
  muted: '#9B9C8B',
  rule: '#465047',
  amber: '#FFB000',
  coral: '#CF8278',
  cyan: '#70B5BD',
  rust: '#6E4035',
  'nav-surface': '#151A17',
  'nav-active': '#FFB000',
  'nav-active-surface': '#303630',
  'nav-ink': '#E3DDC9',
  'nav-muted': '#9B9C8B',
  'nav-rule': '#465047'
}

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
        dark: false,
        colors: lightThemeColors
      },
      dark: {
        dark: true,
        colors: darkThemeColors
      }
    }
  }
})
