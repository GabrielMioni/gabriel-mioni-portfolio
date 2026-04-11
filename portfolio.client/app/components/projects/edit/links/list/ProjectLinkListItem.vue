<script setup lang="ts">
import type { LinkEditorItem } from '~/types/links/LinkEditorItem'
import { ProjectLinkType } from '~/generated/graphql'

const item = defineModel<LinkEditorItem>('item', { required: true })

const icon = computed(() => {
  switch (item.value.type) {
  case ProjectLinkType.Demo:
    return 'mdi-monitor-dashboard'
  case ProjectLinkType.Repository:
    return 'mdi-git'
  default:
    return 'mdi-link-variant'
  }
})

</script>

<template>
  <v-container
    class="pa-0 project-link-list-item hover-surface">
    fluid>
    <v-row align="center">
      <v-col
        cols="auto"
        class="d-flex align-center justify-center order-1">
        <v-icon
          class="mr-2"
          color="primary">
          {{ icon}}
        </v-icon>
      </v-col>
      <v-col
        cols="auto"
        class="d-flex align-center justify-start order-2">
        <div>
          <v-text-field
            v-model="item.url"
            label="Url"
            variant="filled"
            hide-details
            density="compact" />
          <v-text-field
            v-model="item.text"
            label="Url"
            variant="filled"
            hide-details
            density="compact" />
        </div>
      </v-col>
    </v-row>

  </v-container>
</template>

<style scoped>

</style>
