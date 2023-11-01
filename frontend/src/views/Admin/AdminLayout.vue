<template>
  <div>
    <main>
      <router-view v-slot="{ Component }">
        <v-container fluid>
          <v-row>
            <v-col :cols="12">
              <transition name="fade">
                <component :is="Component" />
              </transition>
            </v-col>
          </v-row>
        </v-container>
      </router-view>
    </main>
  </div>
</template>
