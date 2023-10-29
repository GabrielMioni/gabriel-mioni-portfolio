import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '@/views/Public/HomeView.vue'
import AboutView from '@/views/Public/AboutView.vue'
import PublicLayout from '@/views/Public/PublicLayout.vue'
import AdminLayout from '@/views/Admin/AdminLayout.vue'

const routes = [
  {
    path: '/',
    component: PublicLayout,
    children: [
      {
        path: '',
        component: HomeView
      },
      {
        path: '/about',
        component: AboutView
      }
    ]
  },
  {
    path: '/admin',
    component: AdminLayout
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
