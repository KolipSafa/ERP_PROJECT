<template>
  <v-container class="fill-height" fluid>
    <v-row align="center" justify="center">
      <v-col cols="12" sm="8" md="4" class="text-center">
        <v-progress-circular
          indeterminate
          color="primary"
          size="64"
        ></v-progress-circular>
        <p class="mt-4">Yönlendiriliyorsunuz, lütfen bekleyin...</p>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
// Bu sayfanın tek görevi, auth.store.ts içindeki onAuthStateChange
// dinleyicisinin tetiklenmesini beklemektir. O dinleyici, Supabase'in
// adres çubuğundaki token'ı işlemesinden sonra doğru yönlendirmeyi yapacaktır.
// Bu sayede tüm zamanlama (race condition) sorunları ortadan kalkar.
</script>
