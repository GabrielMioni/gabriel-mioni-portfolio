<script setup>
import { ref, computed } from 'vue'
import { useDropzone } from 'vue3-dropzone'

const file = ref(null)
const fileUrl = computed(() => {
  if (!file.value) {
    return ''
  }
  return URL.createObjectURL(file.value)
})

const onDrop = (files) => {
  file.value = files[0]
  console.log(files)
}

const { getRootProps, getInputProps, isDragActive } = useDropzone({ onDrop })

</script>

<template>
  <div class="file-upload-drop-zone">
    <div
      v-bind="getRootProps()"
      class="file-upload-drop-zone__area">
      <input v-bind="getInputProps()">
      <div class="file-upload-drop-zone__area__content">
        <img
          v-if="fileUrl"
          :src="fileUrl"
          alt="file">
        <template v-else>
          <p v-if="isDragActive">
            Drop the files here ...
          </p>
          <p v-else>
            Drag 'n' drop some files here, or click to select files
          </p>
        </template>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.file-upload-drop-zone {
  height: 230px;
  &__area {
    border: 2px dashed #c0c0c0;
    height: 100%;
    border-radius: 4px;
    padding: 20px;
    text-align: center;
    cursor: pointer;
    font-size: 1.2em;
    color: #c0c0c0;
    &__content {
      width: 100%;
      height: 100%;
      display: flex;
      justify-content: center;
      align-items: center;
      img {
        max-width: 100%;
        max-height: 100%;
        object-fit: contain;
      }
    }
  }
}
</style>
