<script setup>
import { Form  } from 'vee-validate'
</script>

<template>
  <Form v-slot="{ meta }">
    <slot
      :is-valid="meta?.valid || false" />
  </Form>
</template>

<script>
export default {
  name: 'BaseForm'
}
</script>


<style scoped lang="scss">

</style>
