<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import TeklifService, { type TeklifFilterParams } from '@/services/TeklifService';
import CustomerService, { type CustomerDto } from '@/services/CustomerService';
import type { TeklifDto, UpdateTeklifPayload } from '@/services/dtos/TeklifDtos';
import { useNotifier } from '@/composables/useNotifier';
import debounce from 'lodash.debounce';

const router = useRouter();
const notifier = useNotifier();
const teklifler = ref<TeklifDto[]>([]);
const customers = ref<CustomerDto[]>([]);
const loading = ref(true);

// --- Filtreleme ve Sıralama State ---
const filters = ref<TeklifFilterParams>({
  musteriId: undefined,
  baslangicTarihi: undefined,
  bitisTarihi: undefined,
  durum: undefined,
  includeInactive: false,
  sortBy: 'date',
  sortOrder: 'desc',
});

const sortOptions = [
  { title: 'Tarihe Göre (Yeni)', value: { sortBy: 'date', sortOrder: 'desc' as const } },
  { title: 'Tarihe Göre (Eski)', value: { sortBy: 'date', sortOrder: 'asc' as const } },
  { title: 'Tutara Göre (Artan)', value: { sortBy: 'amount', sortOrder: 'asc' as const } },
  { title: 'Tutara Göre (Azalan)', value: { sortBy: 'amount', sortOrder: 'desc' as const } },
];
const selectedSort = ref(sortOptions[0].value);

// --- Onay Diyalogu için State ---
const dialog = ref(false);
const dialogTitle = ref('');
const dialogMessage = ref('');
const dialogColor = ref('primary');
let onConfirmAction: () => void = () => {};
const selectedTeklif = ref<TeklifDto | null>(null);

function openDialog({ title, message, color = 'primary', teklif, onConfirm }: { title: string; message: string; color?: string; teklif: TeklifDto; onConfirm: () => void; }) {
  dialogTitle.value = title;
  dialogMessage.value = message;
  dialogColor.value = color;
  selectedTeklif.value = teklif;
  onConfirmAction = onConfirm;
  dialog.value = true;
}

function confirmDialog() {
  onConfirmAction();
  dialog.value = false;
}

// --- Durum Yönetimi ---
const statusOptions = [
  { title: 'Hazırlanıyor', value: 0 },
  { title: 'Sunuldu', value: 1 },
  { title: 'Onaylandı', value: 2 },
  { title: 'Reddedildi', value: 3 },
];

const headers: any[] = [
  { title: 'Teklif Numarası', key: 'teklifNumarasi' },
  { title: 'Müşteri', key: 'musteriAdi' },
  { title: 'Teklif Tarihi', key: 'teklifTarihi' },
  { title: 'Tutar', key: 'toplamTutar', align: 'end' },
  { title: 'Durum', key: 'durum', align: 'center' },
  { title: 'Aktif', key: 'isActive', align: 'center' },
  { title: 'Eylemler', key: 'actions', sortable: false, align: 'end' },
];

const fetchTeklifler = async () => {
  loading.value = true;
  try {
    const response = await TeklifService.getAll(filters.value);
    teklifler.value = response.data;
  } catch (error) {
    console.error('Teklifler getirilirken bir hata oluştu:', error);
    notifier.error('Teklifler getirilirken bir hata oluştu.');
  } finally {
    loading.value = false;
  }
};

const fetchCustomers = async () => {
  try {
    const response = await CustomerService.getAll({ includeInactive: true });
    customers.value = response.data;
  } catch (error) {
    console.error('Müşteriler getirilirken hata:', error);
    notifier.error('Müşteriler getirilirken bir hata oluştu.', { autoClose: 4000 });
  }
};

onMounted(() => {
  const notification = history.state.notification as { type: 'success' | 'error', message: string, duration?: number } | null;
  if (notification) {
    if (notification.type === 'success') {
      notifier.success(notification.message, { autoClose: notification.duration || 4000 });
    } else if (notification.type === 'error') {
      notifier.error(notification.message, { autoClose: notification.duration || 4000 });
    }
    history.replaceState({ ...history.state, notification: null }, '');
  }
  fetchTeklifler();
});

watch(filters, debounce(fetchTeklifler, 400), { deep: true });
watch(selectedSort, (newSortValue) => {
  filters.value.sortBy = newSortValue.sortBy;
  filters.value.sortOrder = newSortValue.sortOrder;
});

const updateStatus = async (teklifToUpdate: TeklifDto, yeniDurumValue: number) => {
  try {
    const response = await TeklifService.getById(teklifToUpdate.id);
    const guncelTeklif = response.data;

    const payload: UpdateTeklifPayload = {
      musteriId: guncelTeklif.musteriId,
      teklifTarihi: guncelTeklif.teklifTarihi,
      gecerlilikTarihi: guncelTeklif.gecerlilikTarihi,
      currencyId: guncelTeklif.currencyId,
      durum: yeniDurumValue,
      isActive: guncelTeklif.isActive,
      teklifSatirlari: guncelTeklif.teklifSatirlari.map(s => ({ id: s.id, urunId: s.urunId, aciklama: s.aciklama, miktar: s.miktar, birimFiyat: s.birimFiyat })),
    };

    await TeklifService.update(teklifToUpdate.id, payload);
    notifier.success(`'${teklifToUpdate.teklifNumarasi}' numaralı teklifin durumu güncellendi.`, { autoClose: 6000 });
    fetchTeklifler(); // Listeyi yenile
  } catch (error) {
    notifier.error('Durum güncellenirken bir hata oluştu.', { autoClose: 4000 });
    console.error('Durum güncellenirken hata:', error);
  }
};

const archiveTeklif = async () => {
  if (!selectedTeklif.value) return;
  try {
    await TeklifService.archive(selectedTeklif.value.id);
    notifier.success(`'${selectedTeklif.value.teklifNumarasi}' başarıyla arşivlendi.`, { autoClose: 4000 });
    fetchTeklifler(); // Listeyi yenile
  } catch (error) {
    notifier.error('Teklif arşivlenirken bir hata oluştu.', { autoClose: 4000 });
    console.error(error);
  }
};

const restoreTeklif = async () => {
  if (!selectedTeklif.value) return;
  try {
    await TeklifService.restore(selectedTeklif.value.id);
    notifier.success(`'${selectedTeklif.value.teklifNumarasi}' başarıyla geri yüklendi.`, { autoClose: 4000 });
    fetchTeklifler(); // Listeyi yenile
  } catch (error) {
    notifier.error('Teklif geri yüklenirken bir hata oluştu.', { autoClose: 4000 });
    console.error(error);
  }
};

const formatDate = (dateString: string) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toLocaleDateString('tr-TR');
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

<template>
  <v-container fluid>
    <v-card>
      <v-toolbar flat>
        <v-toolbar-title>Teklif Listesi</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn color="primary" prepend-icon="mdi-plus" @click="router.push('/quotes/new')">Yeni Teklif</v-btn>
      </v-toolbar>

      <v-card-text class="pa-0">
        <v-expansion-panels class="mb-4">
          <v-expansion-panel>
            <v-expansion-panel-title>
              <v-icon start icon="mdi-filter-variant"></v-icon>
              Filtreler ve Sıralama
            </v-expansion-panel-title>
            <v-expansion-panel-text>
              <v-row align="center">
                <v-col cols="12" md="3">
                  <v-autocomplete v-model="filters.musteriId" :items="customers" item-title="fullName" item-value="id" label="Müşteriye Göre Filtrele" density="compact" hide-details clearable></v-autocomplete>
                </v-col>
                <v-col cols="6" md="2">
                  <v-text-field v-model="filters.baslangicTarihi" label="Başlangıç Tarihi" type="date" density="compact" hide-details clearable></v-text-field>
                </v-col>
                <v-col cols="6" md="2">
                  <v-text-field v-model="filters.bitisTarihi" label="Bitiş Tarihi" type="date" density="compact" hide-details clearable></v-text-field>
                </v-col>
                <v-col cols="12" md="2">
                  <v-select v-model="filters.durum" :items="statusOptions" item-title="title" item-value="value" label="Duruma Göre" density="compact" hide-details clearable></v-select>
                </v-col>
                <v-col cols="6" md="2">
                  <v-select v-model="selectedSort" :items="sortOptions" label="Sırala" density="compact" hide-details></v-select>
                </v-col>
                <v-col cols="6" md="1">
                  <v-switch v-model="filters.includeInactive" label="Pasifler" color="primary" hide-details></v-switch>
                </v-col>
              </v-row>
            </v-expansion-panel-text>
          </v-expansion-panel>
        </v-expansion-panels>

        <v-data-table
          :headers="headers"
          :items="teklifler"
          :loading="loading"
          item-value="id"
          fixed-header
          height="65vh"
        >
          <template v-slot:item.teklifTarihi="{ item }">
            <span>{{ formatDate(item.teklifTarihi) }}</span>
          </template>
          <template v-slot:item.toplamTutar="{ item }">
            <span>{{ formatCurrency(item.toplamTutar, item.currencyCode) }}</span>
          </template>
          <template v-slot:item.durum="{ item }">
            <v-chip :color="getStatusColor(String(item.durum))" size="small">{{ item.durum }}</v-chip>
          </template>
          <template v-slot:item.isActive="{ item }">
            <v-chip :color="item.isActive ? 'green' : 'red'" size="small">{{ item.isActive ? 'Aktif' : 'Pasif' }}</v-chip>
          </template>

          <template v-slot:item.actions="{ item }">
            <v-btn icon="mdi-pencil" variant="text" size="small" class="me-2" @click="router.push(`/quotes/edit/${item.id}`)"></v-btn>
            <v-menu offset-y>
              <template v-slot:activator="{ props }">
                <v-btn icon="mdi-dots-vertical" variant="text" size="small" v-bind="props"></v-btn>
              </template>
              <v-list dense>
                <v-list-subheader>Durumu Değiştir</v-list-subheader>
                <v-list-item v-for="status in statusOptions" :key="status.value" @click="updateStatus(item, status.value)" :disabled="item.durum === status.title">
                  <v-list-item-title>{{ status.title }}</v-list-item-title>
                </v-list-item>
                <v-divider class="my-2"></v-divider>
                <v-list-item v-if="item.isActive" @click="openDialog({ title: 'Teklifi Arşivle', message: `<strong>${item.teklifNumarasi}</strong> numaralı teklifi arşive göndermek istediğinize emin misiniz?`, color: 'warning', teklif: item, onConfirm: archiveTeklif })">
                  <template v-slot:prepend><v-icon color="warning">mdi-archive-arrow-down</v-icon></template>
                  <v-list-item-title>Arşivle</v-list-item-title>
                </v-list-item>
                <v-list-item v-else @click="openDialog({ title: 'Teklifi Geri Yükle', message: `<strong>${item.teklifNumarasi}</strong> numaralı teklifi tekrar aktif hale getirmek istediğinize emin misiniz?`, color: 'success', teklif: item, onConfirm: restoreTeklif })">
                  <template v-slot:prepend><v-icon color="success">mdi-restore</v-icon></template>
                  <v-list-item-title>Geri Yükle</v-list-item-title>
                </v-list-item>
              </v-list>
            </v-menu>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>

    <v-dialog v-model="dialog" max-width="500px">
      <v-card>
        <v-card-title class="text-h5" :class="`bg-${dialogColor}`">{{ dialogTitle }}</v-card-title>
        <v-card-text class="py-4" v-html="dialogMessage"></v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="grey-darken-1" text @click="dialog = false">İptal</v-btn>
          <v-btn :color="dialogColor" text @click="confirmDialog">Onayla</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<style scoped>
.v-data-table {
  border-top: 1px solid rgba(0,0,0,0.12);
}
</style>

