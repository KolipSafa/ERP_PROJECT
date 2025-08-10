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
// Basit header tipi (Vuetify align literal'ları)
type TableHeader = {
  title: string;
  key: string;
  align?: 'start' | 'center' | 'end';
  sortable?: boolean;
};
const teklifler = ref<TeklifDto[]>([]);
const expanded = ref<string[]>([]);
const detailsMap = ref(new Map<string, TeklifDto>());
const customers = ref<CustomerDto[]>([]);
const loading = ref(true);

// --- Filtreleme ve Sıralama State ---
// API kontratı camelCase kullanıyor
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
  { title: 'Tarihe Göre (Yeni)', value: { sortBy: 'date' as const, sortOrder: 'desc' as const } },
  { title: 'Tarihe Göre (Eski)', value: { sortBy: 'date' as const, sortOrder: 'asc' as const } },
  { title: 'Tutara Göre (Artan)', value: { sortBy: 'amount' as const, sortOrder: 'asc' as const } },
  { title: 'Tutara Göre (Azalan)', value: { sortBy: 'amount' as const, sortOrder: 'desc' as const } },
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
  { title: 'Sunuldu', value: 0 },
  { title: 'Onaylandı', value: 1 },
  { title: 'Reddedildi', value: 2 },
  { title: 'Değişiklik Talep Edildi', value: 3 },
];

const statusTextByString: Record<string, string> = {
  Sunuldu: 'Sunuldu',
  Onaylandı: 'Onaylandı',
  Reddedildi: 'Reddedildi',
  ChangeRequested: 'Değişiklik Talep Edildi'
};

const statusColorByString: Record<string, string> = {
  Sunuldu: 'info',
  Onaylandı: 'success',
  Reddedildi: 'error',
  ChangeRequested: 'orange'
};

const getStatusText = (statusValue: string | number) => {
  if (typeof statusValue === 'string') {
    return statusTextByString[statusValue] ?? 'Bilinmeyen';
  }
  const status = statusOptions.find(s => s.value === statusValue);
  return status ? status.title : 'Bilinmeyen';
};

const getStatusColor = (statusValue: string | number) => {
  if (typeof statusValue === 'string') {
    return statusColorByString[statusValue] ?? 'grey';
  }
  switch (statusValue) {
    case 0: return 'info';
    case 1: return 'success';
    case 2: return 'error';
    case 3: return 'orange';
    default: return 'grey';
  }
};

const headers: TableHeader[] = [
  { title: 'Teklif No', key: 'teklifNumarasi' },
  { title: 'Müşteri', key: 'musteriAdi' },
  { title: 'Tarih', key: 'teklifTarihi' },
  { title: 'Kalemler', key: 'items' },
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

watch(expanded, async (newVal, oldVal) => {
  const prev = Array.isArray(oldVal) ? oldVal : [];
  const opened = newVal.find(id => !prev.includes(id));
  if (opened && !detailsMap.value.has(opened)) {
    try {
      const res = await TeklifService.getById(opened);
      detailsMap.value.set(opened, res.data);
    } catch (e) {
      console.error('Teklif detay alınamadı', e);
    }
  }
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

const hardDelete = async () => {
  if (!selectedTeklif.value) return;
  try {
    await TeklifService.hardDelete(selectedTeklif.value.id);
    notifier.warn(`'${selectedTeklif.value.teklifNumarasi}' KALICI olarak silindi.`, { autoClose: 4000 });
    fetchTeklifler();
  } catch (error) {
    notifier.error('Teklif kalıcı silinirken bir hata oluştu.', { autoClose: 4000 });
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
          show-expand
          v-model:expanded="expanded"
        >
          <template v-slot:item.teklifTarihi="{ item }">
            <span>{{ formatDate(item.teklifTarihi) }}</span>
          </template>
          <template v-slot:item.toplamTutar="{ item }">
            <span>{{ formatCurrency(item.toplamTutar, item.currencyCode) }}</span>
          </template>
          <template v-slot:item.durum="{ item }">
            <v-chip :color="getStatusColor(item.durum)" size="small">{{ getStatusText(item.durum) }}</v-chip>
          </template>
          <template v-slot:item.items="{ item }">
            <div>
              <div v-for="line in (item.teklifSatirlari || []).slice(0,3)" :key="line.id" class="text-caption">
                - {{ line.urunAdi }} x {{ line.miktar }}
              </div>
              <div v-if="(item.teklifSatirlari || []).length > 3" class="text-caption">…</div>
            </div>
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
                <v-list-subheader>İşlemler</v-list-subheader>
                <v-list-item v-if="item.isActive" @click="openDialog({ title: 'Teklifi Arşivle', message: `<strong>${item.teklifNumarasi}</strong> numaralı teklifi arşive göndermek istediğinize emin misiniz?`, color: 'warning', teklif: item, onConfirm: archiveTeklif })">
                  <template v-slot:prepend><v-icon color="warning">mdi-archive-arrow-down</v-icon></template>
                  <v-list-item-title>Arşivle</v-list-item-title>
                </v-list-item>
                <v-list-item v-else @click="openDialog({ title: 'Teklifi Geri Yükle', message: `<strong>${item.teklifNumarasi}</strong> numaralı teklifi tekrar aktif hale getirmek istediğinize emin misiniz?`, color: 'success', teklif: item, onConfirm: restoreTeklif })">
                  <template v-slot:prepend><v-icon color="success">mdi-restore</v-icon></template>
                  <v-list-item-title>Geri Yükle</v-list-item-title>
                </v-list-item>
                <v-list-item @click="openDialog({ title: 'KALICI SİL', message: `<strong>UYARI:</strong> <strong>${item.teklifNumarasi}</strong> numaralı teklifi kalıcı olarak sileceksiniz. <strong>Geri alınamaz!</strong>`, color: 'error', teklif: item, onConfirm: hardDelete })">
                  <template v-slot:prepend><v-icon color="error">mdi-delete-forever</v-icon></template>
                  <v-list-item-title class="text-error">Kalıcı Sil</v-list-item-title>
                </v-list-item>
              </v-list>
            </v-menu>
          </template>
          <template v-slot:expanded-row="{ item, columns }">
            <tr>
              <td :colspan="columns.length">
                <v-card flat class="pa-3">
                  <div class="d-flex align-center mb-2">
                    <v-chip :color="getStatusColor(item.durum)" size="small" class="mr-2">{{ getStatusText(item.durum) }}</v-chip>
                  </div>
                  <div v-if="(detailsMap.get(item.id)?.changeRequestNotes || item.changeRequestNotes)" class="mb-3">
                    <v-alert type="info" variant="tonal" density="compact" border="start" border-color="info">
                      {{ detailsMap.get(item.id)?.changeRequestNotes || item.changeRequestNotes }}
                    </v-alert>
                  </div>
                  <div class="text-subtitle-1 mb-2">Kalemler</div>
                  <v-table density="compact">
                    <thead>
                      <tr>
                        <th class="text-left">Ürün</th>
                        <th class="text-right">Miktar</th>
                        <th class="text-right">Birim Fiyat</th>
                        <th class="text-right">Toplam</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="line in (detailsMap.get(item.id)?.teklifSatirlari || item.teklifSatirlari || [])" :key="line.id">
                        <td>{{ line.urunAdi }}</td>
                        <td class="text-right">{{ line.miktar }}</td>
                        <td class="text-right">{{ line.birimFiyat?.toLocaleString('tr-TR') }}</td>
                        <td class="text-right">{{ line.toplam?.toLocaleString('tr-TR') }}</td>
                      </tr>
                    </tbody>
                  </v-table>
                  <div class="d-flex justify-end mt-2">
                    <strong>Genel Toplam:&nbsp;</strong>
                    <span>{{ formatCurrency(item.toplamTutar, item.currencyCode) }}</span>
                  </div>
                </v-card>
              </td>
            </tr>
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

