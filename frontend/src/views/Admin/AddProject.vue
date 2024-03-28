<script setup>
import { ref } from 'vue'
import { makeRequiredRule, urlRule } from '@/rules/index.js'
import { addNewProject } from '@/services/projectsService.js'
import BaseForm from '@/components/inputs/BaseForm.vue'
import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'
import TextField from '@/components/inputs/TextField/TextField.vue'
import TextArea from '@/components/inputs/TextArea.vue'
import FlexRow from '@/components/flex/FlexRow.vue'
import BaseButton from '@/components/BaseButton.vue'

const isValid = ref(false)
const name = ref('')
const description = ref('')
const git = ref('')

const nameRule = makeRequiredRule('Name')
const descriptionRule = makeRequiredRule('Description')
import eventBus from '@/services/EventBus.js'

const handleSubmit = async () => {
  try {
    await addNewProject({ name: name.value, description: description.value, git: git.value })
    eventBus.$emit('toast', { severity: 'success', summary: 'Success', message: 'Project added successfully' })
  } catch (error) {
    console.error('Error adding new project:', error)
    eventBus.$emit('toast', { severity: 'error', summary: 'Error', message: 'Error adding new project' })
  }
}

</script>

<template>
  <card>
    <template #content>
      <base-form v-model="isValid">
        <flex-container fluid>
          <flex-row>
            <flex-column>
              <text-field
                v-model="name"
                label="Name"
                field-name="name"
                float-label
                small
                :rules="[nameRule]" />
            </flex-column>
          </flex-row>
          <flex-row>
            <flex-column
              cols="12"
              class="pb-1">
              <text-field
                v-model="git"
                label="Repo URL"
                field-name="email"
                float-label
                small
                :rules="[urlRule]" />
            </flex-column>
          </flex-row>
          <flex-row>
            <flex-column
              cols="12"
              class="pb-1">
              <text-area
                v-model="description"
                label="Description"
                :rules="[descriptionRule]" />
            </flex-column>
          </flex-row>
          <flex-row>
            <flex-column>
              <base-button>
                Cancel
              </base-button>
              <base-button
                :disabled="!isValid"
                @click="handleSubmit">
                Submit
              </base-button>
            </flex-column>
          </flex-row>
        </flex-container>
      </base-form>
    </template>
  </card>
</template>

<script>
export default {
  name: 'AddProject'
}
</script>

<style scoped>

</style>
