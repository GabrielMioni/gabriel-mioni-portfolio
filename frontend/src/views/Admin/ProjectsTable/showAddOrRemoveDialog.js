import { useConfirm } from 'primevue/useconfirm'
import apolloClient from '@/apollo/apolloClient.js'
import SetProjectActive from '@/views/Admin/ProjectsTable/graphql/SetProjectActive.gql'

async function setProjectActive (id, setActive) {
  try {
    const { data: { setProjectActive: { succeeded } } } = await apolloClient.mutate({
      mutation: SetProjectActive,
      variables: {
        id,
        setActive
      },
      errorPolicy: 'all'
    })

    return succeeded

  } catch (e) {
    console.error(`Unable to removeProject: ${e}`)
    return false
  }
}

const upperCaseFirst = (stringValue) => {
  const firstChar = stringValue.charAt(0).toUpperCase()
  return firstChar + stringValue.slice(1)
}

export function useAddOrRemoveDialog () {
  const confirmService = useConfirm()

  const showAddOrRemoveDialog = async (id, setActive) => {
    const action = setActive ? 'add' : 'remove'
    const actionUc = upperCaseFirst(action)

    return new Promise((resolve, reject) => {
      const options = {
        message: `You are about to ${action} project ${id}. Are you sure you want to proceed?`,
        header: `${actionUc} Project`,
        icon: 'pi pi-exclamation-triangle',
        rejectClass: 'p-button-secondary p-button-outlined',
        rejectLabel: 'Cancel',
        acceptLabel: actionUc,
        accept: async () => {
          try {
            const success = await setProjectActive(id, setActive)
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

  return { showAddOrRemoveDialog }
}
