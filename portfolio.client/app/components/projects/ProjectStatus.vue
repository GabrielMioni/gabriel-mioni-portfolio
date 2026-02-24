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

const createdDate = computed(() => props.project.createdAt ? formatDate(props.project.createdAt) : 'Unknown')
const publishedDate = computed(() => props.project.publishedAt ? formatDate(props.project.publishedAt) : 'Not published')
const updatedDate = computed(() => props.project.updatedAt ? formatDate(props.project.updatedAt) : 'Unknown')

const updatedDifferent = computed(() => {
  return createdDate.value !== updatedDate.value
})

</script>

<template>
  <v-tooltip>
    <template #activator="{ props: activatorProps }">
      <v-icon
        v-bind="activatorProps"
        :color="meta.color"
        :icon="meta.icon" />
    </template>
    <div>
      <div v-text="meta.label" />
      <div
        v-if="project.publishedAt"
        v-text="publishedDate" />
      <div
        v-if="updatedDifferent"
        v-text="updatedDate" />
    </div>
  </v-tooltip>
</template>

<style scoped>

</style>
