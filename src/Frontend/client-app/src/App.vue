<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useAuthStore } from '@/stores/auth.store'

const drawer = ref(true)
const route = useRoute()
const authStore = useAuthStore()

// Uygulama ilk yüklendiğinde, davet linki gibi özel durumları ele al
onMounted(() => {
  // Adres çubuğunda bir Supabase token'ı var mı diye kontrol et (magic link / davet)
  if (window.location.hash.includes('access_token')) {
    // Eğer varsa, bu yeni bir oturum anlamına gelir.
    // Önce mevcut olabilecek eski oturumu (örn: admin) LOKAL olarak temizle.
    // Bu, Supabase'e logout isteği göndermez, sadece tarayıcının kafasının karışmasını önler.
    authStore.clearSession();
  }
  
  // Temiz bir başlangıçla, oturum dinleyicisini başlat.
  // Bu, URL'deki yeni token'ı veya mevcut localStorage token'ını işleyecektir.
  authStore.initialize();
})

const { isLoggedIn, user } = storeToRefs(authStore)

const showLayout = computed(() => {
  return isLoggedIn.value && route.name !== 'set-password'
})

const pageTitle = computed(() => {
  const name = route.name?.toString()
  if (!name) {
    return 'ERP Projesi'
  }
  return name
    .split('-')
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ')
})

const adminNavItems = [
  { title: 'Dashboard', icon: 'mdi-view-dashboard', to: '/' },
  { title: 'Ürünler', icon: 'mdi-package-variant-closed', to: '/products' },
  { title: 'Müşteriler', icon: 'mdi-account-group', to: '/customers' },
  { title: 'Teklifler', icon: 'mdi-file-document-outline', to: '/quotes' },
  { title: 'Ayarlar', icon: 'mdi-cog', to: '/settings' },
];

const customerNavItems = [
  { title: 'Tekliflerim', icon: 'mdi-file-document-outline', to: '/my-quotes' },
];

const navItems = computed(() => {
  if (authStore.isAdmin) {
    return adminNavItems;
  }
  if (authStore.isCustomer) {
    return customerNavItems;
  }
  return [];
});

const handleLogout = () => {
  authStore.logout()
}
</script>

<template>
  <v-app id="inspire">
    <v-app-bar v-if="showLayout" flat>
      <v-app-bar-nav-icon @click="drawer = !drawer"></v-app-bar-nav-icon>
      <v-toolbar-title>{{ pageTitle }}</v-toolbar-title>
      <v-spacer></v-spacer>

      <div>
        <v-menu offset-y>
          <template v-slot:activator="{ props }">
            <v-btn v-bind="props" icon>
              <v-icon>mdi-account-circle</v-icon>
            </v-btn>
          </template>
          <v-list>
            <v-list-item>
              <v-list-item-title>{{ user?.name }}</v-list-item-title>
              <v-list-item-subtitle>{{ user?.email }}</v-list-item-subtitle>
            </v-list-item>
            <v-divider></v-divider>
            <v-list-item @click="handleLogout" link>
              <v-list-item-title>Çıkış Yap</v-list-item-title>
              <template v-slot:prepend>
                <v-icon>mdi-logout</v-icon>
              </template>
            </v-list-item>
          </v-list>
        </v-menu>
      </div>
    </v-app-bar>

    <v-navigation-drawer v-if="showLayout" v-model="drawer">
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
