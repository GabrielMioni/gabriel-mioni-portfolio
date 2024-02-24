import { fetchProjects } from '@/services/projectsService.js'

export const actions = {
  addProject (project) {
    this.PROJECTS.push(project)
  },
  async loadProjects ({ skip, take }) {
    const { projects, count } = await fetchProjects({ skip, take }).catch(e => console.error(e))
    if (projects.length <= 0) {
      console.warn('No projects loaded')
    }
    this.PROJECTS = projects
    this.PROJECTS_COUNT = count
  },
  setProjectActive ({ id, setActive }) {
    const projectIndex = this.PROJECTS.findIndex(p => p.id === id)
    if (projectIndex < 0) {
      console.warn(`Unable to update active value using id of ${id}`)
      return
    }
    const updateProject = {
      ...this.PROJECTS[projectIndex],
      active: setActive
    }
    this.$patch({
      PROJECTS: [
        ...this.PROJECTS.slice(0, projectIndex),
        updateProject,
        ...this.PROJECTS.slice(projectIndex + 1)
      ]
    })
  },
  setProjectsLoading (value) {
    this.PROJECTS_LOADING = value
  }
}
