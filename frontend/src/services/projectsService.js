import apolloClient from '@/apollo/apolloClient.js'
import GetAllProjects from '@/graphql/queries/getAllProjects.gql'

export const fetchAllProjects = async () => {
  const { data, errors } = await apolloClient.query({
    query: GetAllProjects
  })

  if (errors) {
    throw new Error('Error fetching projects')
  }

  return data.projects
}
