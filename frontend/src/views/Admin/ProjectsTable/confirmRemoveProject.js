import { useConfirm } from 'primevue/useconfirm'
import apolloClient from '@/apollo/apolloClient.js'
import RemoveProject from '@/views/Admin/ProjectsTable/graphql/RemoveProject.gql'

async function removeProjectAsync (id) {
  try {
    const { data: { removeProject: { succeeded } } } = await apolloClient.mutate({
      mutation: RemoveProject,
      variables: { id },
      errorPolicy: 'all'
    })

    return succeeded

  } catch (e) {
    console.error(`Unable to removeProject: ${e}`)
    return false
  }
}

export function useConfirmDialog () {
  const confirmService = useConfirm()

  const showConfirmDialog = async (id) => {
    return new Promise((resolve, reject) => {
      const options = {
        message: `You are about to remove project ${id}. Are you sure you want to proceed?`,
        header: 'Remove Project',
        icon: 'pi pi-exclamation-triangle',
        rejectClass: 'p-button-secondary p-button-outlined',
        rejectLabel: 'Cancel',
        acceptLabel: 'Remove',
        accept: async () => {
          try {
            const success = await removeProjectAsync(id)
            if (success) {
              resolve(true)
            } else {
              resolve(false)
            }
          } catch (e) {
            reject(e)
          }
        },
        reject: () => resolve(false)
      }

      confirmService.require(options)
    })
  }

  return { showConfirmDialog }
}
