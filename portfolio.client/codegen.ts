import type { CodegenConfig } from '@graphql-codegen/cli'

const config: CodegenConfig = {
  schema: 'https://localhost:7022/graphql',
  documents: ['app/graphql/**/*.gql'],
  generates: {
    './app/generated/': {
      preset: 'client',
      config: {
        scalars: {
          UUID: 'string',
          DateTime: 'string',
          DateTimeOffset: 'string'
        }
      }
    }
  }
}

export default config
