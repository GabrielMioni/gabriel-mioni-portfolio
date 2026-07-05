<script setup lang="ts">
import type { ProjectTagSummary } from '~/generated/graphql'

const dialog = defineModel<boolean>()

defineProps<{
  tag: ProjectTagSummary | null
}>()
</script>

<template>
  <BaseDialog
    v-model="dialog"
    title="Edit Tag">
    Edit tag content.
    <template #actions>
      <v-btn
        color="primary"
        variant="text"
        @click="dialog = false">
        Close
      </v-btn>
    </template>
  </BaseDialog>
</template>
