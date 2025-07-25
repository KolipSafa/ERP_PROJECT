<script setup lang="ts">
import { ref } from 'vue'
import { RouterView, useRoute } from 'vue-router'

const drawer = ref(true)
const route = useRoute()

const navItems = [
  { title: 'Dashboard', icon: 'mdi-view-dashboard', to: '/' },
  { title: 'Ürünler', icon: 'mdi-package-variant-closed', to: '/products' },
  { title: 'Müşteriler', icon: 'mdi-account-group', to: '/customers' },
  { title: 'Teklifler', icon: 'mdi-file-document-outline', to: '/quotes' },
]
</script>

<template>
  <v-app id="inspire">
    <v-app-bar flat>
      <v-app-bar-nav-icon @click="drawer = !drawer"></v-app-bar-nav-icon>
      <v-toolbar-title>{{ route.name?.toString().charAt(0).toUpperCase() + route.name?.toString().slice(1) || 'ERP Projesi' }}</v-toolbar-title>
    </v-app-bar>

    <v-navigation-drawer v-model="drawer">
      <v-list>
        <v-list-item
          v-for="item in navItems"
          :key="item.title"
          :prepend-icon="item.icon"
          :title="item.title"
          :to="item.to"
          link
        ></v-list-item>
      </v-list>
    </v-navigation-drawer>

    <v-main>
      <v-container>
        <RouterView />
      </v-container>
    </v-main>
  </v-app>
</template>