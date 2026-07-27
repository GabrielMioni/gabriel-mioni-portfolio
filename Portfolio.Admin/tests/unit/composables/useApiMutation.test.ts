import { afterEach, describe, expect, it, vi } from 'vitest'
import type { TypedDocumentNode } from '@urql/core'
import {
  ApiMutationError,
  useApiMutation
} from '~/composables/useApiMutation'
import { makeFragmentData } from '~/generated'
import {
  MutationUserErrorFragmentDoc,
  UserErrorCode
} from '~/generated/graphql'

const urqlMocks = vi.hoisted(() => {
  const executeMutation = vi.fn()
  const fetching = { value: false }

  return {
    executeMutation,
    fetching,
    useMutation: vi.fn(() => ({
      executeMutation,
      fetching
    }))
  }
})

vi.mock('@urql/vue', () => ({
  useMutation: urqlMocks.useMutation
}))

type TestVariables = {
  input: {
    id: string
  }
}

const createUserError = (
  message: string,
  field: string
) => makeFragmentData(
  {
    code: UserErrorCode.Validation,
    message,
    field: [field]
  },
  MutationUserErrorFragmentDoc
)

type TestPayload = {
  message: string
  userErrors: ReadonlyArray<ReturnType<typeof createUserError>>
}

type TestData = {
  testMutation: TestPayload | null
}

const document = {} as TypedDocumentNode<TestData, TestVariables>
const selectPayload = (data: TestData) => data.testMutation
const fallbackMessage = 'The test mutation failed.'
const variables: TestVariables = {
  input: {
    id: 'project-id'
  }
}

const captureApiMutationError = async (
  execution: Promise<unknown>
): Promise<ApiMutationError> => {
  try {
    await execution
  } catch (error) {
    expect(error).toBeInstanceOf(ApiMutationError)
    return error as ApiMutationError
  }

  throw new Error('Expected the mutation to throw ApiMutationError.')
}

afterEach(() => {
  urqlMocks.executeMutation.mockReset()
  urqlMocks.useMutation.mockClear()
  urqlMocks.fetching.value = false
})

describe('useApiMutation', () => {
  it('returns the selected payload from a successful mutation', async () => {
    const payload = {
      message: 'Project saved.',
      userErrors: []
    } satisfies TestPayload
    urqlMocks.executeMutation.mockResolvedValue({
      data: {
        testMutation: payload
      }
    })
    const { executeMutation } = useApiMutation(
      document,
      selectPayload,
      fallbackMessage
    )

    const result = await executeMutation(variables)

    expect(urqlMocks.executeMutation).toHaveBeenCalledWith(variables)
    expect(result).toBe(payload)
  })

  it('exposes the urql fetching state', () => {
    urqlMocks.fetching.value = true

    const { fetching } = useApiMutation(
      document,
      selectPayload,
      fallbackMessage
    )

    expect(fetching).toBe(urqlMocks.fetching)
    expect(fetching.value).toBe(true)
  })

  it('throws all user errors using the first message', async () => {
    const userErrors = [
      createUserError('Title is required.', 'title'),
      createUserError('Summary is too long.', 'summary')
    ]
    urqlMocks.executeMutation.mockResolvedValue({
      data: {
        testMutation: {
          message: '',
          userErrors
        }
      }
    })
    const { executeMutation } = useApiMutation(
      document,
      selectPayload,
      fallbackMessage
    )

    const error = await captureApiMutationError(
      executeMutation(variables)
    )

    expect(error.message).toBe('Title is required.')
    expect(error.userErrors).toEqual([
      {
        code: UserErrorCode.Validation,
        message: 'Title is required.',
        field: ['title']
      },
      {
        code: UserErrorCode.Validation,
        message: 'Summary is too long.',
        field: ['summary']
      }
    ])
    expect(error.originalError).toBeUndefined()
  })

  it('preserves the original transport error', async () => {
    const originalError = new Error('Network connection failed.')
    urqlMocks.executeMutation.mockResolvedValue({
      error: originalError
    })
    const { executeMutation } = useApiMutation(
      document,
      selectPayload,
      fallbackMessage
    )

    const error = await captureApiMutationError(
      executeMutation(variables)
    )

    expect(error.message).toBe(fallbackMessage)
    expect(error.userErrors).toEqual([])
    expect(error.originalError).toBe(originalError)
  })

  it.each([
    {
      missingValue: 'response data',
      response: {}
    },
    {
      missingValue: 'selected payload',
      response: {
        data: {
          testMutation: null
        }
      }
    }
  ])(
    'throws the fallback error when $missingValue is missing',
    async ({ response }) => {
      urqlMocks.executeMutation.mockResolvedValue(response)
      const { executeMutation } = useApiMutation(
        document,
        selectPayload,
        fallbackMessage
      )

      const error = await captureApiMutationError(
        executeMutation(variables)
      )

      expect(error.message).toBe(fallbackMessage)
      expect(error.userErrors).toEqual([])
      expect(error.originalError).toBeUndefined()
    }
  )
})
