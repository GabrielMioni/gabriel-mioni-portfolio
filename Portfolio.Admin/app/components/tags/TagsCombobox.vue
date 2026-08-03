<script setup lang="ts">
import type { TagEditorItem } from '~/types/tags'
import { generateTagValue } from '~/utils/tags'
import {
  MAX_PROJECT_TAGS,
  takeItemsWithinCapacity
} from '~/utils/projects/limits'

const assignedTags = defineModel<TagEditorItem[]>('assignedTags', { default: () => [] })

const props = withDefaults(
  defineProps<{
      disableExisting?: boolean
      maxItems?: number
    }>(),
  {
    disableExisting: false,
    maxItems: undefined
  }
)

const { allTags, fetchingTags } = useTagQueries()
const { showSnackbar } = useSnackbarStore()

const search = ref('')
const tagLimitReached = computed(() =>
  props.maxItems !== undefined &&
  assignedTags.value.length >= props.maxItems
)
const tagLabel = computed(() =>
  props.maxItems === undefined
    ? 'Tags'
    : `Tags (${assignedTags.value.length}/${props.maxItems})`
)
const tagHint = computed(() =>
  tagLimitReached.value
    ? `Projects can have up to ${props.maxItems} tags.`
    : undefined
)

const tagItems = computed<TagEditorItem[]>(() => {
  const all = allTags.value.map(t => ({ id: t.id, name: t.name, value: t.value }))
  if (props.disableExisting && !search.value.trim()) return []
  return all
})

const removeTag = (index: number) => {
  assignedTags.value = assignedTags.value.filter((_, i) => i !== index)
}

const resolveTag = (v: string): TagEditorItem => {
  const value = generateTagValue(v)
  const existing = allTags.value.find(t => t.value === value)
  return existing ?? { id: null, name: v, value }
}

const onUpdateModelValue = (values: (TagEditorItem | string)[]) => {
  if (props.disableExisting) {
    const duplicate = values.find(
      v => typeof v === 'string' &&
      allTags.value.some(t => t.value === generateTagValue(v as string))
    )
    if (duplicate) {
      showSnackbar(`"${duplicate}" already exists`, 'warning')
      return
    }
  }
  const normalized = values.map(v =>
    typeof v === 'string' ? resolveTag(v) : v
  )

  const seen = new Set<string>()
  const uniqueValues = normalized.filter(v => {
    if (!v.value || seen.has(v.value)) return false
    seen.add(v.value)
    return true
  })

  const acceptedValues = props.maxItems === undefined
    ? uniqueValues
    : takeItemsWithinCapacity(uniqueValues, 0, props.maxItems)

  if (acceptedValues.length < uniqueValues.length) {
    showSnackbar(
      `Projects can have up to ${props.maxItems ?? MAX_PROJECT_TAGS} tags.`,
      'warning'
    )
  }

  assignedTags.value = acceptedValues
}
</script>

<template>
  <div class="pt-3">
    <v-combobox
      v-model:search="search"
      :model-value="assignedTags"
      :items="tagItems"
      :hide-no-data="false"
      :loading="fetchingTags"
      item-title="name"
      return-object
      :label="tagLabel"
      :hint="tagHint"
      :persistent-hint="tagLimitReached"
      chips
      multiple
      variant="filled"
      :hide-details="props.maxItems === undefined"
      @update:model-value="onUpdateModelValue">
      <template #item="{ item, props: itemProps }">
        <v-list-item
          v-bind="itemProps"
          :disabled="
            (disableExisting && !!item.raw.id) ||
            (tagLimitReached && !assignedTags.some(tag => tag.value === item.raw.value))
          "
          :subtitle="disableExisting && item.raw.id ? 'Already exists' : undefined" />
      </template>

      <template #no-data>
        <v-list-item>
          <template #prepend>
            <v-icon
              icon="mdi-plus"
              class="mr-2" />
          </template>
          <v-list-item-title>
            {{ tagLimitReached ? 'Tag limit reached' : 'Press enter to create tag' }}
          </v-list-item-title>
        </v-list-item>
      </template>

      <template #chip="{ item, index }">
        <v-tooltip
          :disabled="!!item.raw?.id"
          class="fs-10"
          content-class="font-italic"
          text="Pending"
          location="top">
          <template #activator="{ props }">
            <v-chip
              v-bind="props"
              :key="`tag-chip-${index}`"
              :value="item.raw.value"
              :color="item.raw?.id ? 'primary' : 'warning'"
              :variant="item.raw?.id ? 'flat' : 'outlined'"
              :prepend-icon="item.raw?.id ? undefined : 'mdi-plus'"
              class="ma-1"
              closable
              label
              small
              @mousedown.stop
              @click:close="removeTag(index)">
              {{ item.raw.name }}
            </v-chip>
          </template>
        </v-tooltip>
      </template>
    </v-combobox>
  </div>
</template>
