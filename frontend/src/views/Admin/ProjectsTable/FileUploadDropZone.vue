<script setup>
import { useDropzone } from 'vue3-dropzone'

const onDrop = (files) => {
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
      <div class="file-upload-drop-zone__area__text">
        <p v-if="isDragActive">
          Drop the files here ...
        </p>
        <p v-else>
          Drag 'n' drop some files here, or click to select files
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.file-upload-drop-zone {
  height: 100%;
  &__area {
    border: 2px dashed #c0c0c0;
    height: 100%;
    border-radius: 4px;
    padding: 20px;
    text-align: center;
    cursor: pointer;
    font-size: 1.2em;
    color: #c0c0c0;
    display: flex;
    justify-content: center;
    align-items: center;
  }
}
</style>
