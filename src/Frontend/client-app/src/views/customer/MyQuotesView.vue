<template>
  <v-container fluid>
    <v-card>
      <v-toolbar flat>
        <v-toolbar-title>Tekliflerim</v-toolbar-title>
      </v-toolbar>
      <v-card-text>
        <v-data-table
          :headers="headers"
          :items="teklifler"
          :loading="loading"
          no-data-text="Henüz size özel bir teklif oluşturulmamış."
        >
          <template #item.toplamTutar="{ item }">
            <span>{{ formatCurrency(item.toplamTutar, item.currencyCode) }}</span>
          </template>
          <template #item.durum="{ item }">
            <v-chip :color="getStatusColor(String(item.durum))" size="small">{{ item.durum }}</v-chip>
          </template>
          <template #item.actions="{ item }">
            <v-btn icon="mdi-eye" variant="text" size="small" @click="viewQuoteDetails(item.id)"></v-btn>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import TeklifService from '@/services/TeklifService';
import type { TeklifDto } from '@/services/dtos/TeklifDtos';
import { useNotifier } from '@/composables/useNotifier';
import { useAuthStore } from '@/stores/auth.store';
import type { VDataTable } from 'vuetify/components';

type ReadonlyHeaders = VDataTable['$props']['headers'];

const router = useRouter();
const notifier = useNotifier();
const authStore = useAuthStore();
const teklifler = ref<TeklifDto[]>([]);
const loading = ref(true);

const headers: ReadonlyHeaders = [
  { title: 'Teklif Numarası', key: 'teklifNumarasi' },
  { title: 'Teklif Tarihi', key: 'teklifTarihi' },
  { title: 'Geçerlilik Tarihi', key: 'gecerlilikTarihi' },
  { title: 'Tutar', key: 'toplamTutar', align: 'end' },
  { title: 'Durum', key: 'durum', align: 'center' },
  { title: 'Detaylar', key: 'actions', sortable: false, align: 'center' },
];

const fetchTeklifler = async () => {
  if (!authStore.user?.id) return;
  loading.value = true;
  try {
    const response = await TeklifService.getAll({ musteriId: authStore.user.id });
    teklifler.value = response.data;
  } catch {
    notifier.error('Teklifleriniz getirilirken bir hata oluştu.');
  } finally {
    loading.value = false;
  }
};

onMounted(fetchTeklifler);

const viewQuoteDetails = (id: string) => {
  // Müşteriler için teklif detay sayfası ileride oluşturulacak.
  // Şimdilik adminin kullandığı düzenleme sayfasına yönlendirelim.
  router.push(`/quotes/edit/${id}`);
};

const formatCurrency = (value: number, currencyCode: string = 'TRY') => {
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: currencyCode }).format(value);
};

const getStatusColor = (status: string) => {
  switch (status) {
    case 'Onaylandı': return 'success';
    case 'Reddedildi': return 'error';
    case 'Sunuldu': return 'info';
    default: return 'grey';
  }
};
</script>
