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
    color="nav-surface"
    theme="dark">
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
        active-color="nav-active"
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
  background: #1c1e1e !important;
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
  background: #ffba00;
  border-radius: 12px;
  display: flex;
  height: 44px;
  justify-content: center;
  width: 44px;
}

.admin-brand__owl {
  background: #132421;
  height: 30px;
  mask: url('/owl-icon.svg') center / contain no-repeat;
  width: 30px;
  -webkit-mask: url('/owl-icon.svg') center / contain no-repeat;
}

.admin-brand__name {
  color: #f7faf8;
  font-size: 1rem;
  font-weight: 750;
  letter-spacing: -0.01em;
  line-height: 1.2;
}

.admin-brand__label {
  color: #9eb5ae;
  font-size: 0.75rem;
  letter-spacing: 0.08em;
  margin-top: 0.2rem;
  text-transform: uppercase;
}

.admin-drawer__divider {
  border-color: #343838;
  opacity: 1;
}

.admin-nav {
  padding: 0.75rem;
}

.admin-nav__item {
  color: #c7d3cf;
  margin-bottom: 0.25rem;
}

.admin-nav__item.v-list-item--active {
  background: #303635;
  font-weight: 650;
}
</style>
