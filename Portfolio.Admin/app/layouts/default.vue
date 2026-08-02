<script setup lang="ts">
import { getFetchErrorStatus } from '~/utils/http'

const drawer = ref(true)
const isSigningOut = ref(false)
const snackbarStore = useSnackbarStore()
const { apiFetch } = useApiFetch()

const signOut = async () => {
  isSigningOut.value = true

  try {
    await apiFetch('/auth/logout', { method: 'POST' })
    await navigateTo('/login', { replace: true })
  } catch (error) {
    if (getFetchErrorStatus(error) === 401) {
      await navigateTo('/login', { replace: true })
      return
    }

    snackbarStore.showSnackbar('Sign out failed. Please try again.', 'error')
  } finally {
    isSigningOut.value = false
  }
}
</script>

<template>
  <v-layout>
    <LeftNavDrawer v-model="drawer" />
    <v-app-bar
      flat
      border="b">
      <v-app-bar-nav-icon @click="drawer = !drawer" />
      <v-spacer />
      <v-btn
        :loading="isSigningOut"
        prepend-icon="mdi-logout"
        @click="signOut">
        Sign out
      </v-btn>
    </v-app-bar>
    <v-main>
      <slot />
      <GlobalSnackbar />
    </v-main>
  </v-layout>
</template>
