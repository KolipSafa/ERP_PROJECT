<template>
  <v-row>
    <v-col cols="12" md="2">
      <v-card rounded="lg">
        <v-list nav dense>
          <v-list-item
            v-for="(item, i) in items"
            :key="i"
            :value="item.component"
            @click="selectedItem = item.component"
            :active="selectedItem === item.component"
            color="primary"
          >
            <template v-slot:prepend>
              <v-icon :icon="item.icon"></v-icon>
            </template>
            <v-list-item-title v-text="item.text"></v-list-item-title>
          </v-list-item>
        </v-list>
      </v-card>
    </v-col>

    <v-col cols="12" md="10">
      <v-card rounded="lg">
        <v-card-text>
          <component :is="selectedComponent" />
        </v-card-text>
      </v-card>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import CompanySettings from '@/components/settings/CompanySettings.vue';
import CurrencySettings from '@/components/settings/CurrencySettings.vue';

const selectedItem = ref('CompanySettings');

const items = [
  { text: 'Firmalar', icon: 'mdi-domain', component: 'CompanySettings' },
  { text: 'Para Birimleri', icon: 'mdi-cash-multiple', component: 'CurrencySettings' },
];

const components: { [key: string]: any } = {
  CompanySettings,
  CurrencySettings,
};

const selectedComponent = computed(() => components[selectedItem.value]);
</script>
