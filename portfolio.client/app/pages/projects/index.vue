<script setup lang="ts">
import { useQuery } from '@urql/vue'
import { GetProjectByIdDocument } from '~/generated/graphql'

const { data, fetching, error } = useQuery({
  query: GetProjectByIdDocument,
  variables: {
    id: '3b61d270-70f3-4a1f-421c-08de6781bdd2'
  }
})
</script>

<template>
  <div v-if="fetching">
    Loading...
  </div>
  <div v-else-if="error">
    Error: {{ error }}
  </div>
  <div v-else>
    <pre>{{ data }}</pre>
  </div>
</template>

<style scoped>

</style>
