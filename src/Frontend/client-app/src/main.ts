import './assets/main.css'

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

// Vue Toastify
import VueToast from 'vue-toastify';
import 'vue-toastify/index.css';

import App from './App.vue'
import router from './router'

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

app.use(createPinia())
app.use(router)
app.use(vuetify)
app.use(VueToast, {
    position: 'top-right',
    duration: 4000,
    theme: 'dark',
    // Diğer varsayılan ayarlar
});

app.mount('#app')
