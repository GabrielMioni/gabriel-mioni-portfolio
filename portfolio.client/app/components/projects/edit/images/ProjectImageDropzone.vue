<script setup lang="ts">
const filesList = defineModel<File[]>({ required: true })

const input = ref<HTMLInputElement | null>(null)
const isDragging = ref(false)

const onDragOver = () => {
  isDragging.value = true
}

const onDragLeave = () => {
  isDragging.value = false
}

const onDrop = (event: DragEvent) => {
  isDragging.value = false
  const files = event.dataTransfer?.files
  if (files) {
    handleFiles(files)
  }
}

const openFileDialog = () => {
  input.value?.click()
}

const handleFiles = (files: FileList | File[]) => {
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
      :class="{ 'is-dragging': isDragging }"
      @click="openFileDialog"
      @dragenter.prevent="isDragging = true"
      @dragover.prevent="onDragOver"
      @dragleave="onDragLeave"
      @drop.prevent="onDrop">
      Drag and drop your images here, or click to select files
    </v-sheet>
    <input
      ref="input"
      type="file"
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
}
</style>
