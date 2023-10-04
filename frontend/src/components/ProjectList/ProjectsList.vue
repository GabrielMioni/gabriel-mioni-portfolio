<script setup>
import { computed, onMounted } from 'vue'
import { useProjectsStore } from '@/store/projects/index.js'

const projectStore = useProjectsStore()
const projects = computed(() => projectStore.projectsFormatted)

onMounted(() => {
  projectStore.loadProjects({ skip: 0, take: 10 })
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
              <a
                v-if="project.git"
                :href="project.git"
                class="github-link text-decoration-none"
                target="_blank">
                <v-icon icon="mdi-github" />
                <span class="font-italic font-weight-bold">
                  See it on Github
                </span>
              </a>
            </div>
          </v-col>
        </v-row>
      </v-container>
    </v-card-text>
  </v-card>
</template>

<style lang="scss" scoped>
.github-link {
  color: #007bff;
}
</style>
