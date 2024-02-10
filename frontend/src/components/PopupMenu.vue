<script setup>
import { ref } from 'vue'

const menu = ref()

defineProps({
  items: {
    type: Array,
    required: true
  }
})

const toggle = (event) => {
  menu.value.toggle(event)
}


</script>

<template>
  <div class="card flex justify-content-center">
    <Button
      type="button"
      icon="pi pi-ellipsis-v"
      aria-haspopup="true"
      aria-controls="overlay_menu"
      @click="toggle" />
    <Menu
      ref="menu"
      :model="items"
      :popup="true" />
  </div>
</template>

<script>
export default {
  name: 'PopupMenu'
}
</script>

<style scoped>

</style>
