import type { CodegenConfig } from '@graphql-codegen/cli'

const schema =
  process.env.ADMIN_GRAPHQL_SCHEMA ??
  'http://localhost:5217/graphql/admin'

const config: CodegenConfig = {
  schema,
  documents: ['app/graphql/**/*.gql'],
  generates: {
    './app/generated/': {
      preset: 'client',
      config: {
        useTypeImports: true,
        scalars: {
          UUID: 'string',
          DateTime: 'string',
          DateTimeOffset: 'string'
        }
      }
    },
    './app/generated/schema.graphql': {
      plugins: ['schema-ast']
    }
  }
}

export default config
