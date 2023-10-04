import apolloClient from '@/apollo/apolloClient.js'
import GetProjects from '@/graphql/queries/getProjects.gql'

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
