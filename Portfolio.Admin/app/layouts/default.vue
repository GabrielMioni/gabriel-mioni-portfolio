<script setup lang="ts">
import { getFetchErrorStatus } from '~/utils/http'

const drawer = ref(true)
const isSigningOut = ref(false)
const route = useRoute()
const snackbarStore = useSnackbarStore()
const { apiFetch } = useApiFetch()

const pageTitle = computed(() => {
  if (route.path.startsWith('/tags')) return 'Tags'
  if (route.path.startsWith('/projects')) return 'Projects'

  return 'Admin'
})

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
      class="admin-app-bar"
      color="surface"
      flat
      border="b">
      <v-app-bar-nav-icon
        aria-label="Toggle navigation"
        @click="drawer = !drawer" />
      <v-app-bar-title class="admin-page-title">
        {{ pageTitle }}
      </v-app-bar-title>
      <v-spacer />
      <v-btn
        :loading="isSigningOut"
        color="secondary"
        prepend-icon="mdi-logout"
        variant="text"
        @click="signOut">
        Sign out
      </v-btn>
    </v-app-bar>
    <v-main class="admin-main bg-background">
      <slot />
      <GlobalSnackbar />
    </v-main>
  </v-layout>
</template>

<style scoped>
.admin-app-bar {
  border-color: #d7d3c9 !important;
}

.admin-page-title {
  color: #25312e;
  font-size: 1.05rem;
  font-weight: 700;
  letter-spacing: -0.01em;
}

.admin-main {
  min-height: 100vh;
}
</style>
