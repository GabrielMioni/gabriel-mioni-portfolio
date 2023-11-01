<script setup>
import { ref } from 'vue'

const name = ref('')
const description = ref('')
const repositoryUrl = ref('')

</script>

<template>
  <v-card>
    <v-card-text>
      <v-form>
        <v-text-field
          v-model="name"
          label="Name"
          variant="solo-filled" />
        <v-text-field
          v-model="repositoryUrl"
          label="Repo URL"
          variant="solo-filled" />
        <v-textarea
          v-model="description"
          label="Description"
          variant="solo-filled" />
      </v-form>
      <v-card-actions>
        <v-spacer />
        <v-btn
          variant="flat">
          Cancel
        </v-btn>
        <v-btn
          color="primary"
          variant="flat">
          Submit
        </v-btn>
      </v-card-actions>
    </v-card-text>
  </v-card>
</template>

<script>
export default {
  name: 'AddProject'
}
</script>

<style scoped>

</style>
