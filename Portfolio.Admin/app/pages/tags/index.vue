<script setup lang="ts">
const onEdit = (tagId: string) => {
  // TODO: open edit dialog
}

const onViewProjects = (tagId: string) => {
  // TODO: navigate or open project list dialog
}

const onDelete = (tagId: string) => {
  // TODO: open delete confirmation dialog
}
</script>

<template>
  <v-container>
    <TagsTable
      @edit="onEdit"
      @view-projects="onViewProjects"
      @delete="onDelete" />
  </v-container>
</template>
