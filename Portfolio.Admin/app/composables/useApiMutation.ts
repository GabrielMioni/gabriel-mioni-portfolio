import { useMutation as useUrqlMutation } from '@urql/vue'
import type { AnyVariables, TypedDocumentNode } from '@urql/core'
import { useFragment, type FragmentType } from '~/generated'
import {
  type MutationUserErrorFragment,
  MutationUserErrorFragmentDoc
} from '~/generated/graphql'

type MutationPayload = {
  userErrors: ReadonlyArray<FragmentType<typeof MutationUserErrorFragmentDoc>>
}

export class ApiMutationError extends Error {
  readonly userErrors: ReadonlyArray<MutationUserErrorFragment>
  readonly originalError?: unknown

  constructor (
    fallbackMessage: string,
    userErrors: ReadonlyArray<MutationUserErrorFragment> = [],
    originalError?: unknown
  ) {
    super(userErrors[0]?.message ?? fallbackMessage)
    this.name = 'ApiMutationError'
    this.userErrors = userErrors
    this.originalError = originalError
  }
}

export const useApiMutation = <
  Data,
  Variables extends AnyVariables,
  Payload extends MutationPayload
>(
    document: TypedDocumentNode<Data, Variables>,
    selectPayload: (data: Data) => Payload | null | undefined,
    fallbackMessage: string
  ) => {
  const {
    executeMutation: executeUrqlMutation,
    fetching
  } = useUrqlMutation<Data, Variables>(document)

  const executeMutation = async (variables: Variables): Promise<Payload> => {
    const response = await executeUrqlMutation(variables)

    if (response.error) {
      throw new ApiMutationError(fallbackMessage, [], response.error)
    }

    if (!response.data) {
      throw new ApiMutationError(fallbackMessage)
    }

    const payload = selectPayload(response.data)

    if (!payload) {
      throw new ApiMutationError(fallbackMessage)
    }

    const userErrors = useFragment(
      MutationUserErrorFragmentDoc,
      payload.userErrors
    )

    if (userErrors.length > 0) {
      throw new ApiMutationError(fallbackMessage, userErrors)
    }

    return payload
  }

  return {
    executeMutation,
    fetching
  }
}
