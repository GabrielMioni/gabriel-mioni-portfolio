import { expect, test, type APIRequestContext } from '@playwright/test'
import { signInAsAdmin } from './helpers/authentication.js'

const adminGraphQlUrl = 'http://127.0.0.1:3100/graphql/admin'

const createProjectMutation = `
  mutation CreateProject($input: CreateProjectInput!) {
    createProject(input: $input) {
      project { id }
      userErrors { code message field }
    }
  }
`

const prepareUploadsMutation = `
  mutation PrepareProjectImageUploads($input: PrepareProjectImageUploadsInput!) {
    prepareProjectImageUploads(input: $input) {
      items {
        full { uploadUrl }
      }
      userErrors { code message field }
    }
  }
`

const deleteProjectMutation = `
  mutation DeleteProject($input: DeleteProjectInput!) {
    deleteProject(input: $input) {
      deletedProjectId
      userErrors { code message field }
    }
  }
`

type GraphQlResponse<T> = {
  data: T
  errors?: unknown[]
}

const sendGraphQl = async <T>(
  request: APIRequestContext,
  query: string,
  variables: Record<string, unknown>
) => {
  const response = await request.post(adminGraphQlUrl, {
    data: { query, variables }
  })

  expect(response.ok()).toBe(true)

  const body = await response.json() as GraphQlResponse<T>
  expect(body.errors).toBeUndefined()

  return body.data
}

test('a presigned image URL rejects a body with a different byte length', async ({
  page
}) => {
  const expectedSize = 16
  let projectId: string | undefined

  await signInAsAdmin(page)

  try {
    const createData = await sendGraphQl<{
      createProject: {
        project: { id: string }
        userErrors: unknown[]
      }
    }>(
      page.request,
      createProjectMutation,
      {
        input: {
          title: `E2E upload security ${crypto.randomUUID().slice(0, 8)}`,
          summary: null,
          body: null,
          status: 'DRAFT'
        }
      }
    )

    expect(createData.createProject.userErrors).toEqual([])
    projectId = createData.createProject.project.id

    const prepareData = await sendGraphQl<{
      prepareProjectImageUploads: {
        items: Array<{ full: { uploadUrl: string } }>
        userErrors: unknown[]
      }
    }>(
      page.request,
      prepareUploadsMutation,
      {
        input: {
          projectId,
          items: [
            {
              clientId: crypto.randomUUID(),
              altText: 'Security test image',
              fullContentType: 'image/png',
              fullSizeBytes: expectedSize,
              thumbContentType: 'image/png',
              thumbSizeBytes: expectedSize,
              height: 1,
              width: 1
            }
          ]
        }
      }
    )

    expect(prepareData.prepareProjectImageUploads.userErrors).toEqual([])

    const rejectedUpload = await page.request.put(
      prepareData.prepareProjectImageUploads.items[0]!.full.uploadUrl,
      {
        data: Buffer.alloc(expectedSize + 1),
        headers: {
          'Content-Type': 'image/png'
        }
      }
    )

    expect(rejectedUpload.status()).toBe(403)
  } finally {
    if (projectId) {
      const deleteData = await sendGraphQl<{
        deleteProject: {
          deletedProjectId: string
          userErrors: unknown[]
        }
      }>(
        page.request,
        deleteProjectMutation,
        { input: { projectId } }
      )

      expect(deleteData.deleteProject.userErrors).toEqual([])
      expect(deleteData.deleteProject.deletedProjectId).toBe(projectId)
    }
  }
})
