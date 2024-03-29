import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { DefaultApolloClient } from '@vue/apollo-composable'
import apolloClient from '@/apollo/apolloClient.js'
import router from '@/router/index.js'
import App from './App.vue'
import PrimeVue from 'primevue/config'

import '@/assets/theme.css'
import 'primevue/resources/primevue.min.css'
import 'primeflex/primeflex.css'
import 'primeicons/primeicons.css'

import '@/assets/global.scss'

import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import ConfirmDialog from 'primevue/confirmdialog'
import ConfirmationService from 'primevue/confirmationservice'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import Image from 'primevue/image'
import InputSwitch from 'primevue/inputswitch'
import InputText from 'primevue/inputtext'
import Menu from 'primevue/menu'
import Paginator from 'primevue/paginator'
import Tag from 'primevue/tag'
import Textarea from 'primevue/textarea'
import Toast from 'primevue/toast'
import ToastService from 'primevue/toastservice'
import Toolbar from 'primevue/toolbar'

const app = createApp(App)

const pinia = createPinia()

app.provide(DefaultApolloClient, apolloClient)

app.use(pinia)
app.use(PrimeVue)
app.use(ConfirmationService)
app.use(ToastService)
app.use(router)

const primeComponents = [
  Button,
  Card,
  Column,
  ConfirmDialog,
  DataTable,
  Dialog,
  Image,
  InputSwitch,
  InputText,
  Menu,
  Paginator,
  Tag,
  Textarea,
  Toast,
  Toolbar,
]

primeComponents.forEach(pComponent => {
  app.component(pComponent.name, pComponent)
})

app.mount('#app')
