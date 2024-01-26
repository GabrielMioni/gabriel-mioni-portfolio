<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useProjectsStore } from '@/store/projects/index.js'
import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexRow from '@/components/flex/FlexRow.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'

const projectStore = useProjectsStore()

// Data
const currentPage = ref(1)
const itemsPerPage = ref(10)

// Computed
const projects = computed(() => projectStore.projectsFormatted)
const projectCount = computed(() => projectStore.projectCount)

// Methods
const loadProjectsForCurrentPage = () => {
  const skip = (currentPage.value - 1) * itemsPerPage.value
  projectStore.loadProjects({ skip, take: itemsPerPage.value })
}


// Watchers
watch(currentPage, () => {
  loadProjectsForCurrentPage()
})

// Lifecycle Hooks
onMounted(() => {
  loadProjectsForCurrentPage()
})

</script>

<template>
  <flex-container
    fluid
    class="pa-0 project-view">
    <flex-row>
      <flex-column>
        <card
          v-for="project in projects"
          :key="project.id"
          class="mb-3">
          <template #title>
            <span class="text-lg">
              {{ project.name }}
            </span>
          </template>
          <template #content>
            <div class="grid">
              <div
                v-if="project.image"
                class="col-4">
                <image
                  :src="project.image"
                  alt="project image" />
              </div>
              <div class="col">
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
                  <i class="pi pi-github" />
                  <span class="font-italic font-weight-bold">
                    See it on Github
                  </span>
                </a>
              </div>
            </div>
          </template>
        </card>
      </flex-column>
    </flex-row>
    <flex-row>
      <flex-column>
        <div class="flex justify-content-center">
          <paginator
            v-model="currentPage"
            class="project-view__pagination"
            :rows="itemsPerPage"
            :rows-per-page-options="[10, 20, 30]"
            :total-records="projectCount" />
        </div>
      </flex-column>
    </flex-row>
  </flex-container>
</template>

<style lang="scss" scoped>
.github-link {
  color: #007bff;
}
</style>
