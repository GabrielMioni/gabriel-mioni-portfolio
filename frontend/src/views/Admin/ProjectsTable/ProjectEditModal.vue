<script setup>
import { computed, ref, watch } from 'vue'
import { useProjectsStore } from '@/store/projects/index.js'
import apolloClient from '@/apollo/apolloClient.js'

// GraphQL
import AddProject from '@/views/Admin/ProjectsTable/graphql/AddProject.gql'
import EditProject from '@/views/Admin/ProjectsTable/graphql/EditProject.graphql'

// Rules
import { makeRequiredRule } from '@/rules/index.js'

// Components
import BaseButton from '@/components/BaseButton.vue'
import BaseFormV2 from '@/components/inputs/BaseFormV2.vue'
import TextAreaV2 from '@/components/TextAreaV2.vue'
import TextFieldV2 from '@/components/TextFieldV2.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'
import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexRow from '@/components/flex/FlexRow.vue'
import eventBus from '@/services/EventBus.js'


const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  },
  project: {
    type: [Object, null],
    required: true
  }
})

const name = ref('')
const git = ref('')
const description = ref('')

const nameRule = makeRequiredRule('Name')
const descriptionRule = makeRequiredRule('Description')

const emit = defineEmits(['update:modelValue'])

const showValue = computed({
  get: () => props.modelValue,
  set: (val) => {
    emit('update:modelValue', val)
  }
})

const projectHasBeenEdited = computed(() => {
  if (!props.project) {
    return false
  }
  return name.value !== props.project.name || git.value !== props.project.git || description.value !== props.project.description
})

watch(() => props.project, (projectValue) => {
  if (projectValue === null) {
    name.value = ''
    git.value = ''
    description.value = ''
    return
  }
  name.value = projectValue.name
  git.value = projectValue.git
  description.value = projectValue.description
})

const projectStore = useProjectsStore()

// const sendProjectUpdateToast = (message) => {
//   eventBus.$emit('toast', { severity: 'success', summary: 'Success', message, life: 3000 })
// }

const sendProjectUpdateToast = (message, severity = 'success', summary = 'Success') => {
  eventBus.$emit('toast', { severity, summary, message, life: 3000 })
}

const saveProject = async () => {
  const id = props.project?.id
  if (id) {
    await saveEdit(id)
  } else {
    await saveNewProject()
  }
  showValue.value = false
}

const saveEdit = async (id) => {
  try {
    await apolloClient.mutate({
      mutation: EditProject,
      variables: {
        id,
        description: description.value,
        git: git.value,
        name: name.value
      }
    })

    projectStore.updateProject({ id, description: description.value, git: git.value, name: name.value })
    sendProjectUpdateToast('Project updated successfully')
  } catch (e) {
    sendProjectUpdateToast('Unable to update project', 'error', 'Error')
    console.warn(`Unable to save project: ${e}`)
  }
}

const saveNewProject = async () => {
  try {
    const { data: { addProject: project } } = await apolloClient.mutate({
      mutation: AddProject,
      variables: {
        description: description.value,
        git: git.value,
        name: name.value
      }
    })
    projectStore.addProject({ ...project, active: true })
    sendProjectUpdateToast('Project added successfully')
  } catch (e) {
    sendProjectUpdateToast('Unable to save new project', 'error', 'Error')
    console.warn(`Unable to save project: ${e}`)
  }
}

</script>

<template>
  <base-form-v2 v-slot="{ isValid }">
    <Dialog
      v-model:visible="showValue"
      class="project-edit-modal"
      modal
      header="Edit Profile"
      :style="{ width: '25rem' }">
      <template #header>
        <span class="font-bold">Edit Item</span>
      </template>
      <div class="project-edit-modal__content">
        <flex-container
          fluid
          class="px-0">
          <flex-row>
            <flex-column class="px-0">
              <text-field-v2
                :key="`name-${project?.id}`"
                v-model="name"
                hide-details
                label="Name"
                field-name="name"
                float-label
                small
                :rules="[nameRule]" />
            </flex-column>
            <flex-column
              class="px-0">
              <text-field-v2
                :key="`git-${project?.id}`"
                v-model="git"
                hide-details
                label="Git"
                field-name="git"
                float-label
                small />
            </flex-column>
            <flex-column
              class="px-0"
              :cols="12">
              <text-area-v2
                v-model="description"
                label="Description"
                field-name="description"
                float-label
                :rules="[descriptionRule]" />
            </flex-column>
          </flex-row>
        </flex-container>
      </div>
      <template #footer>
        <base-button
          :filled="false"
          @click="showValue = false">
          Cancel
        </base-button>
        <base-button
          :disabled="!(isValid && projectHasBeenEdited)"
          @click="saveProject">
          Save
        </base-button>
      </template>
    </Dialog>
  </base-form-v2>
</template>

<script>
export default {
  name: 'ProjectEditModal'
}
</script>

<style lang="scss" scoped>

</style>
