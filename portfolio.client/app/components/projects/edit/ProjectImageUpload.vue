<script setup lang="ts">
import ProjectImageDropzone from '~/components/projects/edit/ProjectImageDropzone.vue'

const filesList = ref<File[]>([])

</script>

<template>
  <v-container
    fluid
    class="pa-0 project-image-upload">
    <v-row>
      <v-col>
        <ProjectImageDropzone v-model="filesList"/>
      </v-col>
    </v-row>
    <v-divider class="my-6" />
    <v-row>
      <v-col>
        images
      </v-col>
    </v-row>
  </v-container>
</template>

<style lang="scss" scoped>

</style>
