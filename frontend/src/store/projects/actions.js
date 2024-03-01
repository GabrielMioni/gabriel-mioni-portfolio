import { fetchProjects } from '@/services/projectsService.js'

export const actions = {
  addProject (project) {
    this.PROJECTS.push(project)
  },
  async loadProjects ({ skip, take }) {
    const { projects, count } = await fetchProjects({ skip, take }).catch(e => console.error(e))
    if (projects.length <= 0) {
      console.warn('No projectsStore loaded')
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
  updateProject ({ id, description, git, name }) {
    const index = this.PROJECTS.findIndex(item => item.id === id)
    if (index < 0) {
      console.warn(`Unable to update project using id ${id}`)
      return
    }
    const updatedProject = { ...this.PROJECTS[index], description, git, name }
    const projects = [...this.PROJECTS]
    projects[index] = updatedProject
    this.$patch({
      PROJECTS: projects
    })
  },
  setProjectsLoading (value) {
    this.PROJECTS_LOADING = value
  }
}
