<script setup>
import { ref } from 'vue'
import TextFieldV2 from '@/components/TextFieldV2.vue'
import BaseFormV2 from '@/components/inputs/BaseFormV2.vue'

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
    <text-field-v2
      v-model="email"
      field-name="email"
      :rules="[validateEmail]" />
    <text-field-v2
      v-model="name"
      field-name="name"
      :rules="[required]" />
    <pre>
      {{ `isValid: ${isValid}` }}
    </pre>
    <button :disabled="!isValid">
      Sign Up!
    </button>
  </base-form-v2>
</template>

<style>

</style>
