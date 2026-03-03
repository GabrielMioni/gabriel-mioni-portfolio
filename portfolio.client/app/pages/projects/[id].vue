<script setup lang="ts">
import { ProjectStatus } from '~/generated/graphql'


const isValid = ref(false)
const form = reactive({
  title: '',
  summary: '',
  body: '',
  status: ProjectStatus.Draft
})

</script>

<template>
  <v-container>
    <v-col>
      <ProjectForm
        v-model:form="form"
        v-model:is-valid="isValid" />
    </v-col>
  </v-container>
</template>

<style scoped>

</style>
