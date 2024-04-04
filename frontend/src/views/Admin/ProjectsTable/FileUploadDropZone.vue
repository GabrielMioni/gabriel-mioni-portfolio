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
  const toastPayload = { message: '', type: messageTypes.error }
  if (files.length <= 0) {
    toastPayload.message = 'You may only upload a single file'
    sendToast({ ...toastPayload })
    return
  }
  const droppedFile = files?.[0]
  if (!droppedFile || !droppedFile.type.startsWith('image/')) {
    toastPayload.message = 'You may only upload an image file'
    sendToast({ ...toastPayload })
    return
  }
  file.value = droppedFile
}

const removeFile = () => {
  file.value = null
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
        <template v-if="fileUrl">
          <img
            v-if="fileUrl"
            :src="fileUrl"
            alt="file">
          <button
            class="remove-button"
            @click.stop="removeFile">
            <i class="pi pi-trash" />
          </button>
        </template>
        <template v-else>
          <p v-if="isDragActive">
            Drop the files here ...
          </p>
          <p v-else>
            Drag 'n' drop a file here, or click to select one
          </p>
        </template>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
$remove-button-size: 2rem;
$dropZoneColor: #c0c0c0;

.file-upload-drop-zone {
  height: 230px;
  &__area {
    border: 2px dashed $dropZoneColor;
    height: 100%;
    border-radius: 4px;
    padding: 20px;
    text-align: center;
    cursor: pointer;
    font-size: 1.2em;
    color: $dropZoneColor;
    &__content {
      width: 100%;
      height: 100%;
      display: flex;
      justify-content: center;
      align-items: center;
      position: relative;
      .remove-button {
        background: white;
        border: none;
        position: absolute;
        right: 0;
        top: 0;
        cursor: pointer;
        border-radius: 50%;
        height: $remove-button-size;
        width: $remove-button-size;
        opacity: .5;
        transition: opacity .3s;
        &:hover,
        &:focus {
          opacity: 1;
        }
      }
      img {
        max-width: 100%;
        max-height: 100%;
        object-fit: contain;
      }
    }
  }
}
</style>
