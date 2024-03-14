import { createRouter, createWebHistory } from 'vue-router'
import AboutView from '@/views/Public/AboutView.vue'
// import AddProject from '@/views/Admin/AddProject.vue'
import AdminLayout from '@/views/Admin/AdminLayout.vue'
import ProjectsView from '@/views/Public/ProjectsView.vue'
import PublicLayout from '@/views/Public/PublicLayout.vue'
import ProjectsTable from '@/views/Admin/ProjectsTable/ProjectsTable.vue'
import PrimeVue from '@/views/Public/PrimeVue.vue'
import ValidationForm from '@/components/ValidationForm.vue'

const routes = [
  {
    path: '/',
    component: PublicLayout,
    children: [
      {
        path: '',
        component: ProjectsView
      },
      {
        path: '/about',
        component: AboutView
      },
      {
        path: '/prime-vue',
        component: PrimeVue
      }
    ]
  },
  {
    path: '/admin',
    component: AdminLayout,
    children: [
      {
        path: '',
        component: ProjectsTable
      },
      {
        path: 'validation',
        component: ValidationForm
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
