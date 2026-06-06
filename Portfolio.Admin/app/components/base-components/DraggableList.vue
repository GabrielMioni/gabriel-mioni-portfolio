<script setup lang="ts" generic="T">
import draggable from 'vuedraggable'

const props = withDefaults(
  defineProps<{
      itemKey?: string
      handleClass?: string
    }>(),
  {
    itemKey: 'clientId',
    handleClass: 'drag-handle'
  }
)

const model = defineModel<T[]>({ required: true })

const normalizedHandle = computed(() => {
  return props.handleClass.startsWith('.')
    ? props.handleClass
    : `.${props.handleClass}`
})
</script>

<template>
  <draggable
    v-model="model"
    :item-key="itemKey"
    :handle="normalizedHandle">
    <template #item="{ element, index }">
      <div>
        <slot
          :element="element"
          :index="index" />
        <v-divider
          v-if="index !== model.length - 1"
          class="my-3" />
      </div>
    </template>
  </draggable>
</template>
