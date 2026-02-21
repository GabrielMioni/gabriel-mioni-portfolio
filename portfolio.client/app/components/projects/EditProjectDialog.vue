<script setup lang="ts">
import type { Project } from '~/generated/graphql'
const dialog = defineModel<boolean>()

const props = defineProps<{
  project: Project
}>()

const projectState = ref<Project>(props.project)

watch(
  () => props.project,
  (val) => {
    projectState.value = val
  }
)

const submit = () => {
  console.log('submit', projectState.value)
}

</script>

<template>
  <BaseDialog
    v-model="dialog"
    divider
    title="Edit Project">
    <v-form>
      <v-container
        class="pa-0"
        fluid>
        <v-row>
          <v-col>
            <v-text-field
              v-model="projectState.title"
              variant="filled"
              label="Title" />
          </v-col>
        </v-row>
      </v-container>
    </v-form>
    <template #actions>
      <v-spacer />
      <v-btn
        variant="flat"
        @click="dialog = false">
        Cancel
      </v-btn>
      <v-btn
        variant="flat"
        color="primary"
        @click="submit">
        Save
      </v-btn>
    </template>
  </BaseDialog>
</template>

<style scoped>

</style>
