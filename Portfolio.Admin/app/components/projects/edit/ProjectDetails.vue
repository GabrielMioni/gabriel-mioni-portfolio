<script setup lang="ts">
import { ProjectStatus } from '~/generated/graphql'
import type { ProjectBaseForm } from '~/types/ui/form'
import type { TagEditorItem } from '~/types/tags'
import { required } from '~/utils/rules'

const form = defineModel<ProjectBaseForm>('form', { required: true })
const isValid = defineModel<boolean>('is-valid', { default: false })
const assignedTags = defineModel<TagEditorItem[]>('assignedTags', { default: () => [] })

const statusOptions = [
  { label: 'Draft', value: ProjectStatus.Draft },
  { label: 'Archived', value: ProjectStatus.Archived },
  { label: 'Published', value: ProjectStatus.Published }
]

</script>

<template>
  <v-form v-model="isValid">
    <v-container
      class="pa-0"
      fluid>
      <v-row no-gutters>
        <v-col>
          <v-text-field
            v-model="form.title"
            variant="filled"
            label="Title"
            :rules="[required()]"/>
        </v-col>
      </v-row>
      <v-row
        no-gutters
        class="mb-3">
        <v-col>
          <v-text-field
            v-model="form.summary"
            variant="filled"
            label="Summary"
            hide-details />
        </v-col>
      </v-row>
      <v-row no-gutters>
        <v-col>
          <v-radio-group
            v-model="form.status"
            inline
            hide-details>
            <v-radio
              v-for="option in statusOptions"
              :key="option.value"
              :label="option.label"
              :value="option.value" />
          </v-radio-group>
        </v-col>
      </v-row>
      <v-row
        no-gutters
        class="mb-3">
        <v-col>
          <TagsCombobox v-model:assigned-tags="assignedTags" />
        </v-col>
      </v-row>
      <v-row no-gutters>
        <v-col>
          <v-textarea
            v-model="form.body"
            max-height="300"
            label="Body"
            auto-grow
            persistent-hint />
        </v-col>
      </v-row>
    </v-container>
  </v-form>
</template>

<style scoped>

</style>
