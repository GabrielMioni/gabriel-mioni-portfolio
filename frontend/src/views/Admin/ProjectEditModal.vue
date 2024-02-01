<script setup>
import { computed, ref } from 'vue'
import BaseForm from '@/components/inputs/BaseForm.vue'
import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'
import TextField from '@/components/inputs/TextField/TextField.vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    required: true
  }
})

const emit = defineEmits(['update:modelValue'])

const showValue = computed({
  get: () => props.modelValue,
  set: (val) => {
    emit('update:modelValue', val)
  }
})

const name = ref('')
const formIsValid = ref(false)

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
          class="py-0 px-0">
          <flex-row>
            <flex-column class="px-0">
              <text-field
                v-model="name"
                label="Name"
                field-name="name"
                float-label
                small />
            </flex-column>
          </flex-row>
        </flex-container>
      </base-form>
    </div>
  </Dialog>
</template>

<script>
export default {
  name: 'ProjectEditModal'
}
</script>

<style scoped>

</style>
