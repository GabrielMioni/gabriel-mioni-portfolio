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

import '@/assets/theme.css'
import 'primevue/resources/primevue.min.css'
import 'primeflex/primeflex.css'
import 'primeicons/primeicons.css'

import '@/assets/global.scss'

import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Image from 'primevue/image'
import InputText from 'primevue/inputtext'
import Paginator from 'primevue/paginator'
import Toolbar from 'primevue/toolbar'

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

const primeComponents = [
  Button,
  Card,
  Column,
  DataTable,
  Image,
  InputText,
  Paginator,
  Toolbar,
]

primeComponents.forEach(pComponent => {
  app.component(pComponent.name, pComponent)
})

app.mount('#app')
