<script setup>
import { computed, ref, watch } from 'vue'
import { useProjectsStore } from '@/store/projects/index.js'
import apolloClient from '@/apollo/apolloClient.js'
import useVModel from '@/services/useVModel.js'
import { sendToast, messageTypes } from '@/services/sendToast.js'
import { uploadProjectFile } from '@/api/index.js'

// GraphQL
import AddProject from '@/views/Admin/ProjectsTable/graphql/AddProject.gql'
import EditProject from '@/views/Admin/ProjectsTable/graphql/EditProject.graphql'

// Rules
import { makeRequiredRule } from '@/rules/index.js'

// Components
import BaseButton from '@/components/BaseButton.vue'
import BaseForm from '@/components/inputs/BaseForm.vue'
import TextArea from '@/components/TextArea.vue'
import TextField from '@/components/inputs/TextField/TextField.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'
import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexRow from '@/components/flex/FlexRow.vue'
import FileUploadDropZone from '@/views/Admin/ProjectsTable/FileUploadDropZone.vue'


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
const file = ref(null)

const nameRule = makeRequiredRule('Name')
const descriptionRule = makeRequiredRule('Description')

const emit = defineEmits(['update:modelValue'])

const { localValue: showValue } = useVModel(props, emit)

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
    file.value = null
    return
  }
  name.value = projectValue.name
  git.value = projectValue.git
  description.value = projectValue.description
})

const projectStore = useProjectsStore()

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
    sendToast({ message: 'Project updated successfully', type: messageTypes.success })
  } catch (e) {
    sendToast({ message: 'Unable to update project', type: messageTypes.error })
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
    if (!project) {
      sendToast({ message: 'Unable to save new project', type: messageTypes.error })
      return
    }
    const projectId = project.id
    if (file.value) {
      const uploadResponse = await uploadProjectFile(file.value, projectId)
      console.log(uploadResponse)
    }
    projectStore.addProject({ ...project, active: true })
    sendToast({ message: 'Project added successfully', type: messageTypes.success })
  } catch (e) {
    sendToast({ message: 'Unable to save new project', type: messageTypes.error })
    console.warn(`Unable to save project: ${e}`)
  }
}

const fileChanged = (newFile) => {
  console.log('fileChanged', newFile)
  file.value = newFile
}

</script>

<template>
  <base-form v-slot="{ isValid }">
    <Dialog
      v-model:visible="showValue"
      class="project-edit-modal"
      modal
      header="Edit Profile"
      :style="{ width: '50rem' }">
      <template #header>
        <span class="font-bold">Edit Item</span>
      </template>
      <div class="project-edit-modal__content">
        <flex-container
          fluid
          class="px-0">
          <flex-row>
            <flex-column>
              <flex-column class="px-0">
                <text-field
                  :key="`name-${project?.id}`"
                  v-model="name"
                  label="Name"
                  field-name="name"
                  float-label
                  small
                  :rules="[nameRule]" />
              </flex-column>
              <flex-column
                class="px-0">
                <text-field
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
                <text-area
                  v-model="description"
                  label="Description"
                  field-name="description"
                  float-label
                  :rules="[descriptionRule]" />
              </flex-column>
            </flex-column>
            <flex-column class="flex justify-content-around">
              <div
                v-if="project?.imageUrl"
                class="project-edit-modal__image-container">
                <Button
                  class="close"
                  icon="pi pi-trash"
                  severity="secondary"
                  text
                  rounded
                  aria-label="Cancel" />
                <img
                  :src="project?.imageUrl"
                  alt="image"
                  class="project-edit-modal__image">
              </div>
              <file-upload-drop-zone
                v-else
                @file-change="fileChanged" />
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
  </base-form>
</template>

<script>
export default {
  name: 'ProjectEditModal'
}
</script>

<style lang="scss" scoped>
.project-edit-modal {
  &__image-container {
    position: relative;
    width: 100%;
    height: 100%;
    display: flex;
    justify-content: center;
    align-items: center;

    &:hover {
      .close {
        opacity: 1;
      }

      &:before {
        background-color: rgba(0, 0, 0, 0.25);
      }
    }

    &:before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background-color: rgba(0, 0, 0, 0);
      transition: background-color 0.5s;
      z-index: 1;
    }

    .close {
      color: white !important;
      position: absolute;
      height: 1rem !important;
      width: 1rem !important;
      top: 1rem;
      right: 1rem;
      transition: opacity 0.5s;
      opacity: 0;
      z-index: 1;
    }
  }

  &__image {
    width: 100%;
    object-fit: contain;
  }
}
</style>
