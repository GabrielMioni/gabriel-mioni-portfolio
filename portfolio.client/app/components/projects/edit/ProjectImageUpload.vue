<script setup lang="ts">

</script>

<template>
  <v-container
    fluid
    class="pa-0 project-image-upload">
    <v-row>
      <v-col>
        images
      </v-col>
    </v-row>
    <v-divider class="my-6" />
    <v-row>
      <v-col>
        <div class="drag-drop-area d-flex align-center justify-center hover-surface">
          Drag and drop your images here, or click to select files
        </div>
      </v-col>
    </v-row>
  </v-container>
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
}
</style>
