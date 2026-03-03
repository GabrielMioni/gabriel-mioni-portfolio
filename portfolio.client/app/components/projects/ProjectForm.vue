<script setup lang="ts">
import type { ProjectBaseForm } from '~/types/ui/form'
import { required } from '~/utils/rules'

const form = defineModel<ProjectBaseForm>('form', { required: true })
const isValid = defineModel<boolean>('isValid', { required: true })

</script>

<template>
  <v-form v-model="isValid">
    <v-container
      class="pa-0"
      fluid>
      <v-row no-gutters>
        <v-col>
          <v-text-field
            v-model="form.title"
            variant="filled"
            label="Title"
            :rules="[required()]"/>
        </v-col>
      </v-row>
      <v-row no-gutters>
        <v-col>
          <v-text-field
            v-model="form.summary"
            variant="filled"
            label="Summary" />
        </v-col>
      </v-row>
      <v-row no-gutters>
        <v-textarea
          v-model="form.body"
          max-height="300"
          label="Body"
          auto-grow
          persistent-hint />
      </v-row>
    </v-container>
  </v-form>
</template>

<style scoped>

</style>
