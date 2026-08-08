<script setup lang="ts">
import { ProjectStatus } from '~/generated/graphql'
import type { ProjectBaseForm } from '~/types/ui/form'
import type { TagEditorItem } from '~/types/tags'
import { maxLength, required } from '~/utils/rules'
import {
  MAX_PROJECT_BODY_LENGTH,
  MAX_PROJECT_SUMMARY_LENGTH,
  MAX_PROJECT_TAGS,
  MAX_PROJECT_TITLE_LENGTH
} from '~/utils/projects/limits'

const form = defineModel<ProjectBaseForm>('form', { required: true })
const isValid = defineModel<boolean>('is-valid', { default: false })
const assignedTags = defineModel<TagEditorItem[]>('assignedTags', { default: () => [] })

const props = defineProps<{ showTags?: boolean }>()
const showTags = computed(() => props.showTags ?? true)

const statusOptions = [
  { label: 'Draft', value: ProjectStatus.Draft },
  { label: 'Archived', value: ProjectStatus.Archived },
  { label: 'Published', value: ProjectStatus.Published }
]

</script>

<template>
  <v-form
    v-model="isValid"
    class="project-details">
    <div class="project-details__grid">
      <section class="project-details__section">
        <header class="project-details__section-header">
          <span>Content</span>
          <h2>Project details</h2>
        </header>
        <div class="project-details__section-body">
          <v-text-field
            v-model="form.title"
            variant="filled"
            label="Title"
            :maxlength="MAX_PROJECT_TITLE_LENGTH"
            :counter="MAX_PROJECT_TITLE_LENGTH"
            persistent-counter
            :rules="[
              required(),
              maxLength(MAX_PROJECT_TITLE_LENGTH, 'Title')
            ]" />
          <v-text-field
            v-model="form.summary"
            variant="filled"
            label="Summary"
            :maxlength="MAX_PROJECT_SUMMARY_LENGTH"
            :counter="MAX_PROJECT_SUMMARY_LENGTH"
            persistent-counter
            :rules="[
              maxLength(MAX_PROJECT_SUMMARY_LENGTH, 'Summary')
            ]" />
          <v-textarea
            v-model="form.body"
            max-height="420"
            label="Body"
            :maxlength="MAX_PROJECT_BODY_LENGTH"
            :counter="MAX_PROJECT_BODY_LENGTH"
            persistent-counter
            :rules="[
              maxLength(MAX_PROJECT_BODY_LENGTH, 'Body')
            ]"
            auto-grow
            persistent-hint />
        </div>
      </section>

      <section class="project-details__section">
        <header class="project-details__section-header">
          <span>Publication</span>
          <h2>Visibility and tags</h2>
        </header>
        <div class="project-details__section-body">
          <label class="project-details__field-label">Status</label>
          <v-radio-group
            v-model="form.status"
            class="mt-1"
            hide-details>
            <v-radio
              v-for="option in statusOptions"
              :key="option.value"
              :label="option.label"
              :value="option.value" />
          </v-radio-group>
          <TagsCombobox
            v-if="showTags"
            v-model:assigned-tags="assignedTags"
            :max-items="MAX_PROJECT_TAGS" />
        </div>
      </section>
    </div>
  </v-form>
</template>

<style scoped>
.project-details__grid {
  display: grid;
  gap: 1rem;
  grid-template-columns: minmax(0, 2fr) minmax(17rem, 1fr);
}

.project-details__section {
  align-self: start;
  background: rgb(var(--v-theme-paper));
  border: 1px solid rgb(var(--v-theme-rule));
}

.project-details__section-header {
  border-bottom: 1px solid rgb(var(--v-theme-rule));
  padding: 0.75rem 1rem;
}

.project-details__section-header span,
.project-details__field-label {
  color: rgb(var(--v-theme-rust));
  font-family: var(--admin-font-mono);
  font-size: 0.68rem;
  font-weight: 700;
  letter-spacing: 0.09em;
  text-transform: uppercase;
}

.project-details__section-header h2 {
  font-family: var(--admin-font-display);
  font-size: 1.4rem;
  font-weight: 850;
  line-height: 1.1;
  margin-top: 0.2rem;
}

.project-details__section-body {
  padding: 1rem;
}

.project-details__section-body > :last-child {
  margin-bottom: 0;
}

@media (max-width: 959px) {
  .project-details__grid {
    grid-template-columns: 1fr;
  }
}

</style>
