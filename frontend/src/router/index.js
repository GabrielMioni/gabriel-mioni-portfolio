import { createRouter, createWebHistory } from 'vue-router'
import AboutView from '@/views/Public/AboutView.vue'
import AdminLayout from '@/views/Admin/AdminLayout.vue'
import ProjectsList from '@/views/Public/ProjectsList.vue'
import PublicLayout from '@/views/Public/PublicLayout.vue'

const routes = [
  {
    path: '/',
    component: PublicLayout,
    children: [
      {
        path: '',
        component: ProjectsList
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
