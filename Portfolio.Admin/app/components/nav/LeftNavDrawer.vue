<script setup lang="ts">
const drawer = defineModel<boolean>({ default: true })
const route = useRoute()

const navItems = [
  { title: 'Projects', icon: 'mdi-folder-multiple-outline', to: '/projects' },
  { title: 'Tags', icon: 'mdi-tag-multiple-outline', to: '/tags' }
]
</script>

<template>
  <v-navigation-drawer
    v-model="drawer"
    class="admin-drawer"
    color="nav-surface">
    <div class="admin-brand">
      <div class="admin-brand__mark">
        <span
          aria-hidden="true"
          class="admin-brand__owl" />
      </div>
      <div>
        <div class="admin-brand__name">
          Portfolio
        </div>
        <div class="admin-brand__label">
          Admin studio
        </div>
      </div>
    </div>

    <v-divider class="admin-drawer__divider" />

    <v-list
      class="admin-nav"
      nav>
      <v-list-item
        v-for="item in navItems"
        :key="item.to"
        :active="route.path.startsWith(item.to)"
        color="nav-active"
        class="admin-nav__item"
        :prepend-icon="item.icon"
        rounded="lg"
        :title="item.title"
        :to="item.to" />
    </v-list>
  </v-navigation-drawer>
</template>

<style scoped>
.admin-drawer {
  /*noinspection CssUnresolvedCustomProperty*/
  background: rgb(var(--v-theme-nav-surface)) !important;
}

.admin-brand {
  align-items: center;
  display: flex;
  gap: 0.75rem;
  min-height: 80px;
  padding: 0 1rem;
}

.admin-brand__mark {
  align-items: center;
  display: flex;
  height: 44px;
  justify-content: center;
  width: 44px;
}

.admin-brand__owl {
  /*noinspection CssUnresolvedCustomProperty*/
  background: rgb(var(--v-theme-ink));
  height: 30px;
  mask: url('/owl-icon.svg') center / contain no-repeat;
  width: 30px;
  -webkit-mask: url('/owl-icon.svg') center / contain no-repeat;
}

.admin-brand__name {
  /*noinspection CssUnresolvedCustomProperty*/
  color: rgb(var(--v-theme-nav-ink));
  font-family: var(--admin-font-display);
  font-size: 1rem;
  font-weight: 750;
  letter-spacing: -0.01em;
  line-height: 1.2;
}

.admin-brand__label {
  /*noinspection CssUnresolvedCustomProperty*/
  color: rgb(var(--v-theme-nav-muted));
  font-family: var(--admin-font-mono);
  font-size: 0.75rem;
  letter-spacing: 0.08em;
  margin-top: 0.2rem;
  text-transform: uppercase;
}

.admin-drawer__divider {
  /*noinspection CssUnresolvedCustomProperty*/
  border-color: rgb(var(--v-theme-nav-rule));
  opacity: 1;
}

.admin-nav {
  padding: 0.75rem;
}

.admin-nav__item {
  /*noinspection CssUnresolvedCustomProperty*/
  color: rgb(var(--v-theme-nav-ink));
  margin-bottom: 0.25rem;
}

.admin-nav__item.v-list-item--active {
  /*noinspection CssUnresolvedCustomProperty*/
  background: rgb(var(--v-theme-nav-active-surface));
  font-weight: 650;
}
</style>
