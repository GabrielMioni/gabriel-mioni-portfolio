<script setup>
import { computed, ref, watch } from 'vue'
import { makeRequiredRule } from '@/rules/index.js'

import BaseButton from '@/components/BaseButton.vue'
import BaseForm from '@/components/inputs/BaseForm.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'
import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexRow from '@/components/flex/FlexRow.vue'
import TextField from '@/components/inputs/TextField/TextField.vue'
import TextArea from '@/components/inputs/TextArea.vue'

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

const formIsValid = ref(false)

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

</script>

<template>
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
      <base-form v-model="formIsValid">
        <flex-container
          fluid
          class="px-0">
          <flex-row>
            <flex-column class="px-0">
              <text-field
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
          </flex-row>
        </flex-container>
      </base-form>
    </div>
    <template #footer>
      <base-button
        :filled="false"
        @click="showValue = false">
        Cancel
      </base-button>
      <base-button :disabled="!(formIsValid && projectHasBeenEdited)">
        Save
      </base-button>
    </template>
  </Dialog>
</template>

<script>
export default {
  name: 'ProjectEditModal'
}
</script>

<style lang="scss" scoped>

</style>
