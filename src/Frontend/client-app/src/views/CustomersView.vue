<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import CustomerService, { type CustomerDto, type CustomerFilterParams } from '@/services/CustomerService';
import { useNotifier } from '@/composables/useNotifier';
import debounce from 'lodash.debounce';

const router = useRouter();
const notifier = useNotifier();
const customers = ref<CustomerDto[]>([]);
const loading = ref(true);

// --- Onay Penceresi (Dialog) Mantığı ---
// Bu bölüm, ProductsView'den birebir alınmıştır. Kod tekrarını önlemek için
// gelecekte bu bir "composable" veya component haline getirilebilir.
const dialog = ref(false);
const dialogTitle = ref('');
const dialogMessage = ref('');
const dialogColor = ref('primary');
let onConfirmAction: () => void = () => {};
const selectedCustomer = ref<CustomerDto | null>(null);

function openDialog({
  title,
  message,
  color = 'primary',
  customer,
  onConfirm,
}: {
  title: string;
  message: string;
  color?: string;
  customer: CustomerDto;
  onConfirm: () => void;
}) {
  dialogTitle.value = title;
  dialogMessage.value = message;
  dialogColor.value = color;
  selectedCustomer.value = customer;
  onConfirmAction = onConfirm;
  dialog.value = true;
}

function confirmDialog() {
  onConfirmAction();
  dialog.value = false;
}
// -----------------------------------------

const filters = ref<CustomerFilterParams>({
  searchTerm: '',
  includeInactive: false,
  sortBy: 'date',
  isDescending: true,
});

// Sıralama seçenekleri artık 'value' olarak basit bir string tutuyor.
// Bu, state yönetimini daha sağlam hale getirir.
const sortOptions = [
  { title: 'Tarihe Göre (Yeni)', value: 'date_desc' },
  { title: 'Tarihe Göre (Eski)', value: 'date_asc' },
  { title: 'Ada Göre (A-Z)', value: 'name_asc' },
  { title: 'Ada Göre (Z-A)', value: 'name_desc' },
  { title: 'Firmaya Göre (A-Z)', value: 'company_asc' },
  { title: 'Firmaya Göre (Z-A)', value: 'company_desc' },
];
// v-select'in modeli artık sadece bir string.
const selectedSort = ref(sortOptions[0].value);

const fetchCustomers = async () => {
  loading.value = true;
  try {
    const response = await CustomerService.getAll(filters.value);
    customers.value = response.data;
  } catch (error) {
    console.error('Müşteriler getirilirken bir hata oluştu:', error);
    notifier.error('Müşteriler getirilirken bir hata oluştu.', { autoClose: 4000 });
  } finally {
    loading.value = false;
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
  
  fetchCustomers();
});

watch(filters, debounce(fetchCustomers, 300), { deep: true });

// selectedSort (string) değiştiğinde, bu string'i parçalayıp
// asıl 'filters' objemizi güncelliyoruz.
watch(selectedSort, (newSortValue) => {
  const [sortBy, sortOrder] = newSortValue.split('_');
  filters.value.sortBy = sortBy;
  filters.value.isDescending = sortOrder === 'desc';
});

// --- Eylem Fonksiyonları ---
const archiveCustomer = async () => {
  if (!selectedCustomer.value) return;
  try {
    await CustomerService.archive(selectedCustomer.value.id);
    notifier.success(`'${selectedCustomer.value.fullName}' başarıyla arşivlendi.`, { autoClose: 4000 });
    fetchCustomers(); // Listeyi yenile
  } catch (error) {
    notifier.error('Müşteri arşivlenirken bir hata oluştu.', { autoClose: 4000 });
    console.error(error);
  }
};

const restoreCustomer = async () => {
  if (!selectedCustomer.value) return;
  try {
    await CustomerService.restore(selectedCustomer.value.id);
    notifier.success(`'${selectedCustomer.value.fullName}' başarıyla geri yüklendi.`, { autoClose: 4000 });
    fetchCustomers(); // Listeyi yenile
  } catch (error) {
    notifier.error('Müşteri geri yüklenirken bir hata oluştu.', { autoClose: 4000 });
    console.error(error);
  }
};
// -------------------------

const headers: any[] = [
  { title: 'Ad Soyad', key: 'fullName' },
  { title: 'Firma Adı', key: 'companyName' },
  { title: 'E-posta', key: 'email' },
  { title: 'Telefon', key: 'phoneNumber' },
  { title: 'Bakiye', key: 'balance', align: 'end' },
  { title: 'Cari Durumu', key: 'isActive', align: 'center' },
  { title: 'Hesap Durumu', key: 'isAccountActive', align: 'center' },
  { title: 'Eylemler', key: 'actions', sortable: false, align: 'end' },
];
</script>

<template>
  <v-container fluid>
    <v-card>
      <v-toolbar flat>
        <v-toolbar-title>Müşteri Listesi</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn color="primary" prepend-icon="mdi-plus" @click="router.push('/customers/new')">Yeni Müşteri</v-btn>
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
                <v-col cols="12" md="6">
                  <v-text-field v-model="filters.searchTerm" label="Ad, soyad, firma veya e-postaya göre ara..." prepend-inner-icon="mdi-magnify" density="compact" hide-details></v-text-field>
                </v-col>
                <v-col cols="12" md="4">
                  <v-select v-model="selectedSort" :items="sortOptions" label="Sırala" density="compact" hide-details></v-select>
                </v-col>
                <v-col cols="12" md="2">
                  <v-switch v-model="filters.includeInactive" label="Pasifler" color="primary" hide-details></v-switch>
                </v-col>
              </v-row>
            </v-expansion-panel-text>
          </v-expansion-panel>
        </v-expansion-panels>

        <v-data-table
          :headers="headers"
          :items="customers"
          :loading="loading"
          item-value="id"
          fixed-header
          height="65vh"
        >
          <template v-slot:item.balance="{ item }">
            <span>{{ new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(item.balance) }}</span>
          </template>

          <template v-slot:item.isActive="{ item }">
            <v-chip :color="item.isActive ? 'green' : 'red'" size="small">
              {{ item.isActive ? 'Aktif' : 'Pasif' }}
            </v-chip>
          </template>

          <template v-slot:item.isAccountActive="{ item }">
            <v-chip v-if="item.isAccountActive" color="blue" size="small">
              Aktif
            </v-chip>
            <v-chip v-else color="orange" size="small">
              Aktivasyon Bekliyor
            </v-chip>
          </template>
          
          <template v-slot:item.actions="{ item }">
            <!-- Aktif Müşteriler İçin Eylemler -->
            <div v-if="item.isActive">
              <v-btn icon="mdi-pencil" variant="text" size="small" class="me-2" @click="router.push(`/customers/edit/${item.id}`)"></v-btn>
              <v-btn icon="mdi-archive-arrow-down" variant="text" size="small" @click="openDialog({ title: 'Müşteriyi Arşivle', message: `<strong>${item.fullName}</strong> isimli müşteriyi arşive göndermek istediğinize emin misiniz? Bu müşteri pasif hale getirilecektir.`, customer: item, onConfirm: archiveCustomer })"></v-btn>
            </div>
            <!-- Pasif Müşteriler İçin Eylemler -->
            <div v-else>
              <v-btn icon="mdi-restore" variant="text" size="small" class="me-2" @click="openDialog({ title: 'Müşteriyi Geri Yükle', message: `<strong>${item.fullName}</strong> isimli müşteriyi tekrar aktif hale getirmek istediğinize emin misiniz?`, customer: item, onConfirm: restoreCustomer })"></v-btn>
              <!-- Müşteriler için kalıcı silme planlanmadığı için o menü burada yok. -->
            </div>
          </template>

        </v-data-table>
      </v-card-text>
    </v-card>

    <!-- Onay Penceresi -->
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