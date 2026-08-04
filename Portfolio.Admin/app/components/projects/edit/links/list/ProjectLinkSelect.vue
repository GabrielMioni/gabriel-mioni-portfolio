<script setup lang="ts">
import { ProjectLinkType } from '~/generated/graphql'
import LinkTypeIcon from '~/components/projects/edit/links/LinkTypeIcon.vue'

const linkType = defineModel<ProjectLinkType>({ required: true })

withDefaults(
  defineProps<{
    disabled?: boolean
  }>(),
  {
    disabled: false
  }
)

const formatTitle = (linkType: ProjectLinkType): string => {
  switch (linkType) {
  case ProjectLinkType.Demo:
    return 'Demo'
  case ProjectLinkType.Repository:
    return 'Repository'
  default:
    return 'External'
  }
}

const selectItems = computed(() => {
  return Object.values(ProjectLinkType).map((itemType) => ({
    value: itemType,
    title: formatTitle(itemType)
  }))
})
</script>

<template>
  <v-select
    v-model="linkType"
    :items="selectItems"
    :disabled="disabled"
    variant="filled"
    hide-details>
    <template #item="{ props, item }">
      <v-list-item
        v-bind="props"
        :title="undefined">
        <template #prepend>
          <LinkTypeIcon :link-type="item.value" />
        </template>
        <v-list-item-title>
          {{ item.title }}
        </v-list-item-title>
      </v-list-item>
    </template>
    <template #prepend-inner>
      <LinkTypeIcon
        :link-type="linkType"
        class="mr-3"/>
    </template>
  </v-select>
</template>

<style scoped>

</style>
