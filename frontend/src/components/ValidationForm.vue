<script setup>
import { ref } from 'vue'
import TextFieldV2 from '@/components/TextFieldV2.vue'
import BaseFormV2 from '@/components/inputs/BaseFormV2.vue'
import FlexContainer from '@/components/flex/FlexContainer.vue'
import FlexRow from '@/components/flex/FlexRow.vue'
import FlexColumn from '@/components/flex/FlexColumn.vue'
import BaseButton from '@/components/BaseButton.vue'

const validateEmail = (email) => {
  const regex = /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$/i
  if (!regex.test(email)) {
    return 'This field must be a valid email'
  }
  return true
}

const required = (value) => {
  return (value ? value.trim().length > 0 : false) || 'This field is required'
}

const email = ref('')
const name = ref('')

</script>

<template>
  <base-form-v2 v-slot="{ isValid }">
    <flex-container fluid>
      <flex-row>
        <flex-column>
          <text-field-v2
            v-model="email"
            field-name="email"
            label="Email"
            float-label
            small
            :rules="[validateEmail]" />
        </flex-column>
      </flex-row>
      <flex-row>
        <flex-column>
          <text-field-v2
            v-model="name"
            field-name="name"
            help-text="Name"
            label="Name"
            float-label
            small
            :rules="[required]" />
        </flex-column>
      </flex-row>
      <flex-column>
        <flex-row>
          <base-button :disabled="!isValid">
            Sign Up
          </base-button>
        </flex-row>
      </flex-column>
    </flex-container>
  </base-form-v2>
</template>

<style>

</style>
