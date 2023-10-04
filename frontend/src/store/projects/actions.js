import { fetchProjects } from '@/services/projectsService.js'

export const actions = {
  addProject (project) {
    this.PROJECTS.push(project)
  },
  async loadProjects ({ skip, take }) {
    const projects = await fetchProjects({ skip, take }).catch(e => console.error(e))
    if (projects.length <= 0) {
      console.warn('No projects loaded')
    }
    this.PROJECTS = projects
  },
  setProjectsLoading (value) {
    this.PROJECTS_LOADING = value
  }
}
