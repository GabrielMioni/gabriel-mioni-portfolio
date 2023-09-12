<script setup>
import { useProjectsStore } from '@/store/projects/index.js'

const projectStore = useProjectsStore()
const projects = projectStore.projects || []

projects.forEach(project => {
  project.formattedDescription = project.description ? project.description.split('\n\n') : []
})

</script>

<template>
  <v-card
    v-for="project in projects"
    :key="project.id"
    class="mb-4"
    elevation="1">
    <v-card-title>
      {{ project.name }}
    </v-card-title>
    <v-card-text>
      <v-container>
        <v-row>
          <v-col
            v-if="project.image"
            :cols="4">
            <v-img
              :src="project.image"
              max-width="100%" />
          </v-col>
          <v-col class="d-flex align-center">
            <div>
              <div
                v-for="(paragraph, i) in project.formattedDescription"
                :key="i"
                class="mb-3">
                {{ paragraph }}
              </div>
            </div>
          </v-col>
        </v-row>
      </v-container>
    </v-card-text>
  </v-card>
</template>
