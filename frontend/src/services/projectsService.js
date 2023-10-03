import apolloClient from '@/apollo/apolloClient.js'
import GetAllProjects from '@/graphql/queries/getAllProjects.gql'

export const fetchAllProjects = async () => {
  const { data: { projects } } = await apolloClient.query({
    query: GetAllProjects
  })
  const { errors, nodes } = projects

  if (errors && errors.length) {
    throw new Error('Error fetching projects')
  }

  return nodes
}
