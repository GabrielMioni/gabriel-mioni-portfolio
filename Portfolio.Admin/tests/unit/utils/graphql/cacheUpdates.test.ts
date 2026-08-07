import type {
  Cache,
  FieldInfo,
  ResolveInfo,
  UpdateResolver
} from '@urql/exchange-graphcache'
import { describe, expect, it, vi } from 'vitest'
import {
  adminCacheUpdates,
  invalidateQueryFields
} from '~/utils/graphql/cacheUpdates'

const createCache = (fields: FieldInfo[] = []) => {
  const inspectFields = vi.fn(() => fields)
  const invalidate = vi.fn()

  return {
    cache: { inspectFields, invalidate } as unknown as Cache,
    inspectFields,
    invalidate
  }
}

const getMutationUpdate = (fieldName: string): UpdateResolver => {
  const update = adminCacheUpdates.Mutation?.[fieldName]

  if (!update) {
    throw new Error(`Missing cache update for ${fieldName}.`)
  }

  return update
}

const resolveInfo = {} as ResolveInfo

describe('invalidateQueryFields', () => {
  it('invalidates every cached argument variant of the requested fields', () => {
    const projectArguments = { skip: 0, take: 10 }
    const filteredProjectArguments = { skip: 0, take: 10, where: { title: 'folio' } }
    const fields: FieldInfo[] = [
      { fieldKey: 'projects-a', fieldName: 'projects', arguments: projectArguments },
      { fieldKey: 'projects-b', fieldName: 'projects', arguments: filteredProjectArguments },
      { fieldKey: 'tags', fieldName: 'tags', arguments: null }
    ]
    const { cache, invalidate } = createCache(fields)

    invalidateQueryFields(cache, ['projects'])

    expect(invalidate).toHaveBeenCalledTimes(2)
    expect(invalidate).toHaveBeenNthCalledWith(1, 'Query', 'projects', projectArguments)
    expect(invalidate).toHaveBeenNthCalledWith(2, 'Query', 'projects', filteredProjectArguments)
  })
})

describe('adminCacheUpdates', () => {
  it('removes a deleted project and invalidates project collections', () => {
    const projectsArguments = { skip: 0, take: 10 }
    const { cache, invalidate } = createCache([
      { fieldKey: 'projects', fieldName: 'projects', arguments: projectsArguments }
    ])

    getMutationUpdate('deleteProject')(
      { deletedProjectId: 'project-id' },
      {},
      cache,
      resolveInfo
    )

    expect(invalidate).toHaveBeenCalledWith({
      __typename: 'Project',
      id: 'project-id'
    })
    expect(invalidate).toHaveBeenCalledWith(
      'Query',
      'projects',
      projectsArguments
    )
  })

  it('invalidates tag relationships for projects changed by tag removal', () => {
    const tagArguments = { tagId: 'tag-id' }
    const { cache, invalidate } = createCache([
      {
        fieldKey: 'projectsByTagId',
        fieldName: 'projectsByTagId',
        arguments: tagArguments
      }
    ])

    getMutationUpdate('removeTagFromProjects')(
      { projectIds: ['project-one', 'project-two'] },
      {},
      cache,
      resolveInfo
    )

    expect(invalidate).toHaveBeenCalledWith(
      { __typename: 'Project', id: 'project-one' },
      'tags'
    )
    expect(invalidate).toHaveBeenCalledWith(
      { __typename: 'Project', id: 'project-two' },
      'tags'
    )
    expect(invalidate).toHaveBeenCalledWith(
      'Query',
      'projectsByTagId',
      tagArguments
    )
  })
})
