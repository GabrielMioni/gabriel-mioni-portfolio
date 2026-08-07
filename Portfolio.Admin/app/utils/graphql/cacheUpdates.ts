import type {
  Cache,
  DataFields,
  UpdatesConfig
} from '@urql/exchange-graphcache'

const stringField = (result: DataFields, fieldName: string) => {
  const value = result[fieldName]
  return typeof value === 'string' ? value : null
}

const stringListField = (result: DataFields, fieldName: string) => {
  const value = result[fieldName]

  if (!Array.isArray(value)) return []

  return value.filter((item): item is string => typeof item === 'string')
}

export const invalidateQueryFields = (
  cache: Cache,
  fieldNames: readonly string[]
) => {
  const names = new Set(fieldNames)

  cache.inspectFields('Query')
    .filter(field => names.has(field.fieldName))
    .forEach((field) => {
      cache.invalidate('Query', field.fieldName, field.arguments)
    })
}

export const adminCacheUpdates: UpdatesConfig = {
  Mutation: {
    createProject: (_result, _args, cache) => {
      invalidateQueryFields(cache, ['projects'])
    },
    editProject: (_result, _args, cache) => {
      invalidateQueryFields(cache, ['projects'])
    },
    deleteProject: (result, _args, cache) => {
      const projectId = stringField(result, 'deletedProjectId')

      if (projectId) {
        cache.invalidate({ __typename: 'Project', id: projectId })
      }

      invalidateQueryFields(cache, ['projects'])
    },
    createProjectTags: (_result, _args, cache) => {
      invalidateQueryFields(cache, ['tags', 'tagSummaries'])
    },
    renameProjectTag: (_result, _args, cache) => {
      invalidateQueryFields(cache, ['tagSummaries'])
    },
    deleteProjectTag: (result, _args, cache) => {
      const tagId = stringField(result, 'deletedTagId')

      if (tagId) {
        cache.invalidate({ __typename: 'ProjectTag', id: tagId })
      }

      invalidateQueryFields(cache, ['tags', 'tagSummaries'])
    },
    removeTagFromProjects: (result, _args, cache) => {
      stringListField(result, 'projectIds').forEach((projectId) => {
        cache.invalidate({ __typename: 'Project', id: projectId }, 'tags')
      })

      invalidateQueryFields(cache, ['projectsByTagId', 'tagSummaries'])
    }
  }
}
