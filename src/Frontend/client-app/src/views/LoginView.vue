<template>
  <v-container class="fill-height" fluid>
    <v-row align="center" justify="center">
      <v-col cols="12" sm="8" md="4">
        <v-card class="elevation-12">
          <v-toolbar color="primary" dark flat>
            <v-toolbar-title>Giriş Yap</v-toolbar-title>
          </v-toolbar>
          <v-card-text>
            <v-alert
              v-if="isDev"
              density="compact"
              type="info"
              variant="tonal"
              class="mb-4"
            >
              <strong>Geliştirici Notu:</strong><br />
              Admin E-posta: <strong>admin@example.com</strong><br />
              Şifre: <strong>Admin123!</strong>
            </v-alert>
            <v-form @submit.prevent="handleLogin">
              <v-text-field
                v-model="email"
                label="E-posta"
                name="email"
                prepend-icon="mdi-account"
                type="text"
                :rules="[rules.required, rules.email]"
              ></v-text-field>
              <v-text-field
                v-model="password"
                label="Şifre"
                name="password"
                prepend-icon="mdi-lock"
                type="password"
                :rules="[rules.required]"
              ></v-text-field>
              <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="primary" type="submit">Giriş Yap</v-btn>
              </v-card-actions>
            </v-form>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '@/stores/auth.store';

const email = ref('');
const password = ref('');
const authStore = useAuthStore();
const isDev = import.meta.env.DEV;

const rules = {
  required: (value: string) => !!value || 'Bu alan zorunludur.',
  email: (value: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value) || 'Geçerli bir e-posta adresi giriniz.',
};

const handleLogin = () => {
  if (email.value && password.value) {
    authStore.login({ email: email.value, password: password.value });
  }
};
</script>