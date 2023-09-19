export const getters = {
  projects: (state) => state.PROJECTS,
  projectsLoading: (state) => state.PROJECTS_LOADING,
  projectsFormatted: (state) => {
    return state.PROJECTS.map(project => {
      const { description } = project
      const formattedDescription = description ? description.split('\n\n') : []
      return { ...project, formattedDescription }
    })
  }
}
