<script setup lang="ts">
import { ProjectLinkType } from '~/generated/graphql'
import ProjectLinkSelect from '~/components/projects/edit/links/ProjectLinkSelect.vue'

const isValid = ref<boolean>(false)

const linkType = ref<ProjectLinkType>(ProjectLinkType.Repository)
const linkUrl = ref<string>('')
const linkText = ref<string>('')

</script>

<template>
  <v-form v-model="isValid">
    <v-container
      fluid
      class="pa-0">
      <v-row>
        <v-col
          cols="auto"
          class="d-flex justify-center align-center">
          <LinkTypeIcon
            :link-type="linkType"
            size="x-large" />
        </v-col>
        <v-col class="d-flex justify-start align-center">
          <v-text-field
            v-model="linkUrl"
            label="Url"
            variant="filled"
            hide-details />
        </v-col>
        <v-col>
          <v-text-field
            v-model="linkText"
            label="Link Text"
            variant="filled"
            hide-details />
        </v-col>
        <v-col>
          <ProjectLinkSelect
            v-model="linkType"/>
        </v-col>
        <v-col
          cols="auto"
          class="d-flex justify-center align-center">
          <v-btn
            density="comfortable"
            icon="mdi-plus" />
        </v-col>
      </v-row>
    </v-container>
  </v-form>
</template>

<style scoped>

</style>
