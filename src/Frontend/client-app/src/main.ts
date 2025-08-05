// Bizim özel stillerimiz
import './assets/main.css'

// Vue3 Toastify (Doğru kütüphanenin stilleri)
import 'vue3-toastify/dist/index.css';

import { createApp } from 'vue'
import { createPinia } from 'pinia'

// Vuetify
import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import '@mdi/font/css/materialdesignicons.css'

// Vuetify Dil Desteği
import { tr } from 'vuetify/locale'

// Vue3 Toastify Eklentisi
import Vue3Toastify, { type ToastContainerOptions } from 'vue3-toastify';

import App from './App.vue'
import router from './router'
import { useAuthStore } from './stores/auth.store';


const vuetify = createVuetify({
  components,
  directives,
  icons: {
    defaultSet: 'mdi',
  },
  locale: {
    locale: 'tr',
    fallback: 'en',
    messages: { tr },
  },
})

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(vuetify)

app.use(Vue3Toastify, {
  theme: 'dark',
  position: 'top-right',
} as ToastContainerOptions);

// Uygulamayı başlatmadan önce, ilk oturum dinleyicisini kur ve bekle
const authStore = useAuthStore();
authStore.initialize(); // Bu, onAuthStateChange dinleyicisini hemen kurar

// Router'ın tüm başlangıç yönlendirmelerini bitirmesini bekle, sonra uygulamayı ekrana çiz.
// Bu, zamanlama hatalarını (race conditions) önler.
router.isReady().then(() => {
  app.mount('#app')
})


// Uygulamayı başlatmadan önce, ilk oturum dinleyicisini kur
