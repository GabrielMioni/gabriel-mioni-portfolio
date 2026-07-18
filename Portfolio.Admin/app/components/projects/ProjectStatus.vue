<script setup lang="ts">
import { type Project, ProjectStatus } from '~/generated/graphql'
import { formatDate } from '~/utils/formatters'

const statusMeta: Record<ProjectStatus, { label: string; icon: string; color: string }> = {
  [ProjectStatus.Draft]: { label: 'Draft', icon: 'mdi-pencil', color: 'grey' },
  [ProjectStatus.Published]: { label: 'Published', icon: 'mdi-check-circle-outline', color: 'success' },
  [ProjectStatus.Archived]: { label: 'Archived', icon: 'mdi-archive-outline', color: 'secondary' }
}

const props = defineProps<{
  project: Project
}>()

const meta = computed(() => statusMeta[props.project.status ?? ProjectStatus.Draft])

const view = computed(() => ({
  createdAt: props.project.createdAt ? formatDate(props.project.createdAt) : null,
  publishedAt: props.project.publishedAt ? formatDate(props.project.publishedAt) : null,
  updatedAt: props.project.updatedAt ? formatDate(props.project.updatedAt) : null
}))

const updatedDifferent = computed(() => {
  return view.value.createdAt !== view.value.updatedAt
})

const disableToolTip = computed(() => {
  return !view.value.publishedAt && !updatedDifferent.value
})

</script>

<template>
  <v-tooltip
    :disabled="disableToolTip">
    <template #activator="{ props: activatorProps }">
      <v-chip
        :color="meta.color"
        v-bind="activatorProps"
        :prepend-icon="meta.icon">
        {{ meta.label }}
      </v-chip>
    </template>
    <div class="fs-10">
      <div
        v-if="view.publishedAt"
        v-text="`Published: ${view.publishedAt}`" />
      <div
        v-if="updatedDifferent"
        v-text="`Updated: ${view.updatedAt}`" />
    </div>
  </v-tooltip>
</template>

<style scoped>

</style>
