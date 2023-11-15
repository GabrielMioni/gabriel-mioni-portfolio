import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { DefaultApolloClient } from '@vue/apollo-composable'
import apolloClient from '@/apollo/apolloClient.js'
import router from '@/router/index.js'
import App from './App.vue'

import 'vuetify/styles'
import '@mdi/font/css/materialdesignicons.css'
import { aliases, mdi } from 'vuetify/iconsets/mdi'
import { createVuetify } from 'vuetify'
import PrimeVue from 'primevue/config'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { VDataTableServer } from 'vuetify/labs/VDataTable'

import 'primevue/resources/themes/lara-light-indigo/theme.css'
import 'primevue/resources/primevue.min.css'
import 'primeicons/primeicons.css'

import DataTable from 'primevue/datatable'
import Column from 'primevue/column'

const vuetify = createVuetify({
  components: {
    ...components,
    VDataTableServer
  },
  directives,
  icons: {
    defaultSet: 'mdi',
    aliases,
    sets: {
      mdi
    }
  },
})

const app = createApp(App)

const pinia = createPinia()

app.provide(DefaultApolloClient, apolloClient)

app.use(pinia)
app.use(vuetify)
app.use(PrimeVue)
app.use(router)

app.component('DataTable', DataTable)
// eslint-disable-next-line vue/multi-word-component-names
app.component('Column', Column)

app.mount('#app')
