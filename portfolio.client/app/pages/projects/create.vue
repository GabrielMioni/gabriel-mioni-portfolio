<script setup lang="ts">
import { ProjectStatus } from '~/generated/graphql'

const form = reactive({
  title: '',
  summary: '',
  body: '',
  status: ProjectStatus.Draft
})
const isValid = ref(false)
</script>

<template>
  <v-container>
    <h3>Create Page</h3>
    <ProjectForm
      v-model:form="form"
      v-model:is-valid="isValid" />
  </v-container>
</template>

<style scoped>

</style>
