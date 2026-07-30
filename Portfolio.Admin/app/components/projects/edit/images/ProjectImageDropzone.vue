<script setup lang="ts">
const filesList = defineModel<File[]>({ required: true })

const props = withDefaults(
  defineProps<{
    disabled?: boolean
    maximumCount?: number
    remainingCapacity?: number
    removedImagesPending?: boolean
  }>(),
  {
    disabled: false,
    maximumCount: 0,
    remainingCapacity: 0,
    removedImagesPending: false
  }
)

const input = ref<HTMLInputElement | null>(null)
const isDragging = ref(false)

const instructionText = computed(() => {
  if (props.disabled) {
    const saveRemovalsMessage = props.removedImagesPending
      ? ' Save removed images to free space.'
      : ''

    return `Image limit reached (${props.maximumCount}).${saveRemovalsMessage}`
  }

  return `Drag and drop images here, or click to select files ` +
    `(${props.remainingCapacity} remaining)`
})

const onDragOver = () => {
  if (props.disabled) return
  isDragging.value = true
}

const onDragLeave = () => {
  isDragging.value = false
}

const onDrop = (event: DragEvent) => {
  isDragging.value = false
  if (props.disabled) return

  const files = event.dataTransfer?.files
  if (files) {
    handleFiles(files)
  }
}

const openFileDialog = () => {
  if (props.disabled) return
  input.value?.click()
}

const handleFiles = (files: FileList | File[]) => {
  if (props.disabled) return

  const list = Array.from(files)

  const images = list.filter(file => file.type.startsWith('image/'))

  filesList.value.push(...images)
}

const onFileInputChange = (event: Event) => {
  const target = event.target as HTMLInputElement
  const files = target.files

  if (files) {
    handleFiles(files)
  }

  target.value = ''
}

</script>

<template>
  <div>
    <v-sheet
      class="drag-drop-area d-flex align-center justify-center hover-surface"
      :class="{
        'is-dragging': isDragging,
        'is-disabled': disabled
      }"
      :aria-disabled="disabled"
      @click="openFileDialog"
      @dragenter.prevent="onDragOver"
      @dragover.prevent="onDragOver"
      @dragleave="onDragLeave"
      @drop.prevent="onDrop">
      {{ instructionText }}
    </v-sheet>
    <input
      ref="input"
      type="file"
      :disabled="disabled"
      multiple
      accept="image/*"
      style="display: none"
      @change="onFileInputChange">
  </div>
</template>

<style lang="scss" scoped>
.drag-drop-area {
  border: 2px dashed rgba(var(--v-theme-grey), .3);
  border-radius: 4px;
  height: 150px;
  padding: 20px;
  text-align: center;
  color: rgb(var(--v-theme-grey));
  cursor: pointer;
  transition:
      border-color .15s ease,
      background-color .15s ease,
      color .15s ease;

  &.is-dragging {
    cursor: copy;
    border-color: rgba(var(--v-theme-primary), .8);
    background-color: rgba(var(--v-theme-primary), .1);
    color: rgb(var(--v-theme-primary));
  }

  &.is-disabled {
    cursor: not-allowed;
    opacity: .65;
  }
}
</style>
