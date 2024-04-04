<script setup>
import { ref, computed } from 'vue'
import { useDropzone } from 'vue3-dropzone'
import { sendToast, messageTypes } from '@/services/sendToast.js'

const file = ref(null)
const fileUrl = computed(() => {
  if (!file.value) {
    return ''
  }
  return URL.createObjectURL(file.value)
})

const onDrop = (files) => {
  if (files.length <= 0) {
    const toastPayload = { message: 'You may only upload a single file', type: messageTypes.error }
    sendToast({ ...toastPayload })
    return
  }
  const droppedFile = files?.[0]
  if (!droppedFile || !droppedFile.type.startsWith('image/')) {
    const toastPayload = { message: 'You may only upload an image file', type: messageTypes.error }
    sendToast({ ...toastPayload })
    return
  }
  file.value = droppedFile
}

const { getRootProps, getInputProps, isDragActive } = useDropzone({ onDrop, multiple: false })

</script>

<template>
  <div class="file-upload-drop-zone">
    <div
      v-bind="getRootProps()"
      class="file-upload-drop-zone__area">
      <input
        v-bind="getInputProps()"
        type="file"
        accept="image/*">
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
