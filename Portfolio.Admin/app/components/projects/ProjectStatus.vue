<script setup lang="ts">
import { type Project, ProjectStatus } from '~/generated/graphql'
import { formatDate } from '~/utils/formatters'

type UiStatus = 'DRAFT' | 'UNPUBLISHED' | 'PUBLISHED' | 'ARCHIVED'

const statusMeta: Record<UiStatus, { label: string; icon: string; color: string; }> = {
  DRAFT: { label: 'Draft', icon: 'mdi-pencil', color: 'grey' },
  UNPUBLISHED: { label: 'Unpublished', icon: 'mdi-eye-off-outline', color: 'warning' },
  PUBLISHED: { label: 'Published', icon: 'mdi-check-circle-outline', color: 'success' },
  ARCHIVED: { label: 'Archived', icon: 'mdi-archive-outline', color: 'secondary' }
}

const getUiStatus = (p: Project): UiStatus => {
  if (!p.status) return 'UNPUBLISHED'
  if (p.status === ProjectStatus.Archived) return 'ARCHIVED'
  if (p.publishedAt) return 'PUBLISHED'
  if (p.status === ProjectStatus.Draft) return 'DRAFT'
  return 'UNPUBLISHED'
}


const props = defineProps<{
  project: Project
}>()

const meta = computed(() => statusMeta[getUiStatus(props.project)])

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
