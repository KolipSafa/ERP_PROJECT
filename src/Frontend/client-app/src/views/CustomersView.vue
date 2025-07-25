<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import CustomerService, { type CustomerDto, type CustomerFilterParams } from '@/services/CustomerService';
import debounce from 'lodash.debounce';

const router = useRouter();
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
  } finally {
    loading.value = false;
  }
};

onMounted(fetchCustomers);

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
  await CustomerService.archive(selectedCustomer.value.id);
  fetchCustomers(); // Listeyi yenile
};

const restoreCustomer = async () => {
  if (!selectedCustomer.value) return;
  await CustomerService.restore(selectedCustomer.value.id);
  fetchCustomers(); // Listeyi yenile
};
// -------------------------

const headers: any[] = [
  { title: '', key: 'data-table-expand' }, // Genişletme ikonu için boş başlık
  { title: 'Ad Soyad', key: 'fullName' },
  { title: 'Firma Adı', key: 'companyName' },
  { title: 'E-posta', key: 'email' },
  { title: 'Telefon', key: 'phoneNumber' },
  { title: 'Bakiye', key: 'balance', align: 'end' },
  { title: 'Durum', key: 'isActive', align: 'center' },
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
          show-expand
        >
          <template v-slot:item.balance="{ item }">
            <span>{{ new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(item.balance) }}</span>
          </template>

          <template v-slot:item.isActive="{ item }">
            <v-chip :color="item.isActive ? 'green' : 'red'" size="small">
              {{ item.isActive ? 'Aktif' : 'Pasif' }}
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

          <template v-slot:expanded-row="{ columns, item }">
            <tr>
              <td :colspan="columns.length">
                <v-card-text>
                  <v-row>
                    <v-col cols="12" md="6">
                      <div class="font-weight-bold">Adres:</div>
                      <div>{{ item.address || 'Girilmemiş' }}</div>
                    </v-col>
                    <v-col cols="12" md="6">
                      <div class="font-weight-bold">Vergi Numarası:</div>
                      <div>{{ item.taxNumber || 'Girilmemiş' }}</div>
                    </v-col>
                  </v-row>
                </v-card-text>
              </td>
            </tr>
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