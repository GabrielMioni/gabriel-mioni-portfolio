<script setup>
import profilePic from '@/assets/saturn-eating-his-son.png'
import ContactForm from '@/views/Public/components/ContactForm.vue'

</script>

<template>
  <v-footer color="#036870">
    <v-container>
      <v-row align="center">
        <v-col
          :cols="3"
          class="d-flex justify-center align-center">
          <v-avatar size="200px">
            <v-img
              :src="profilePic"
              cover />
          </v-avatar>
        </v-col>
        <v-col>
          <contact-form />
        </v-col>
      </v-row>
    </v-container>
  </v-footer>
</template>
