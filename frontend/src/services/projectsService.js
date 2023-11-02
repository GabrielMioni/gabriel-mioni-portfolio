import apolloClient from '@/apollo/apolloClient.js'
import GetProjects from '@/graphql/queries/getProjects.gql'
import AddNewProject from '@/graphql/mutations/addNewProject.gql'

export const fetchProjects = async ({ skip, take }) => {
  const { data: { projects } } = await apolloClient.query({
    query: GetProjects,
    variables: {
      skip,
      take
    }
  })
  const { errors, nodes, count } = projects

  if (errors && errors.length) {
    throw new Error('Error fetching projects')
  }

  return { projects: nodes, count }
}

export const addNewProject = async ({ name, description, git }) => {
  const data = await apolloClient.mutate({
    mutation: AddNewProject,
    variables: {
      name,
      description,
      git
    }
  }).catch(error => console.log(error))
  console.log(data)
}
