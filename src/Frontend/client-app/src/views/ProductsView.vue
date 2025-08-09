<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import ProductService, { type ProductDto, type ProductFilterParams } from '@/services/ProductService';
import { useNotifier } from '@/composables/useNotifier';
import debounce from 'lodash.debounce';
import type { VDataTable } from 'vuetify/components';

const router = useRouter();
const notifier = useNotifier();
const products = ref<ProductDto[]>([]);
const loading = ref(true);

// --- Onay Penceresi (Dialog) Mantığı ---
const dialog = ref(false);
const dialogTitle = ref('');
const dialogMessage = ref('');
const dialogColor = ref('primary');
let onConfirmAction: () => void = () => {};
const selectedProduct = ref<ProductDto | null>(null);

function openDialog({
  title,
  message,
  color = 'primary',
  product,
  onConfirm,
}: {
  title: string;
  message: string;
  color?: string;
  product: ProductDto;
  onConfirm: () => void;
}) {
  dialogTitle.value = title;
  dialogMessage.value = message;
  dialogColor.value = color;
  selectedProduct.value = product;
  onConfirmAction = onConfirm;
  dialog.value = true;
}

function confirmDialog() {
  onConfirmAction();
  dialog.value = false;
}
// -----------------------------------------

const filters = ref<ProductFilterParams>({
  search: '',
  minPrice: undefined,
  maxPrice: undefined,
  includeInactive: false,
  sortBy: 'date',
  sortOrder: 'desc',
});

const sortOptions = [
  { title: 'Tarihe Göre (Yeni)', value: { sortBy: 'date', sortOrder: 'desc' as const } },
  { title: 'Tarihe Göre (Eski)', value: { sortBy: 'date', sortOrder: 'asc' as const } },
  { title: 'Fiyata Göre (Artan)', value: { sortBy: 'price', sortOrder: 'asc' as const } },
  { title: 'Fiyata Göre (Azalan)', value: { sortBy: 'price', sortOrder: 'desc' as const } },
  { title: 'İsme Göre (A-Z)', value: { sortBy: 'name', sortOrder: 'asc' as const } },
  { title: 'İsme Göre (Z-A)', value: { sortBy: 'name', sortOrder: 'desc' as const } },
];
const selectedSort = ref(sortOptions[0].value);

const fetchProducts = async () => {
  loading.value = true;
  try {
    const response = await ProductService.getAll(filters.value);
    products.value = response.data;
  } catch (error) {
    console.error('Ürünler getirilirken bir hata oluştu:', error);
    notifier.error('Ürünler getirilirken bir hata oluştu.', { autoClose: 4000 });
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
  fetchProducts();
});

watch(filters, debounce(fetchProducts, 300), { deep: true });
watch(selectedSort, (newSortValue) => {
  filters.value.sortBy = newSortValue.sortBy;
  filters.value.sortOrder = newSortValue.sortOrder;
});

// --- Eylem Fonksiyonları ---
const archiveProduct = async () => {
  if (!selectedProduct.value) return;
  try {
    await ProductService.archive(selectedProduct.value.id);
    notifier.success(`'${selectedProduct.value.name}' başarıyla arşivlendi.`, { autoClose: 4000 });
    fetchProducts(); // Listeyi yenile
  } catch (error) {
    notifier.error('Ürün arşivlenirken bir hata oluştu.', { autoClose: 4000 });
    console.error(error);
  }
};

const restoreProduct = async () => {
  if (!selectedProduct.value) return;
  try {
    await ProductService.restore(selectedProduct.value.id);
    notifier.success(`'${selectedProduct.value.name}' başarıyla geri yüklendi.`, { autoClose: 4000 });
    fetchProducts(); // Listeyi yenile
  } catch (error) {
    notifier.error('Ürün geri yüklenirken bir hata oluştu.', { autoClose: 4000 });
    console.error(error);
  }
};

const hardDeleteProduct = async () => {
  if (!selectedProduct.value) return;
  try {
    await ProductService.hardDelete(selectedProduct.value.id);
    notifier.warn(`'${selectedProduct.value.name}' kalıcı olarak silindi.`, { autoClose: 4000 });
    fetchProducts(); // Listeyi yenile
  } catch (error) {
    notifier.error('Ürün silinirken bir hata oluştu.', { autoClose: 4000 });
    console.error(error);
  }
};
// -------------------------

const headers: any[] = [
  { title: '', key: 'data-table-expand' },
  { title: 'ID', key: 'id', width: '80px' },
  { title: 'Ürün Adı', key: 'name' },
  { title: 'SKU', key: 'sku' },
  { title: 'Fiyat', key: 'price' },
  { title: 'Stok', key: 'stockQuantity' },
  { title: 'Rezerve', key: 'reservedQuantity' },
  { title: 'Kalan', key: 'available' },
  { title: 'Durum', key: 'isActive' },
  { title: 'Eylemler', key: 'actions', sortable: false, align: 'end' },
];
</script>

<template>
  <v-container fluid>
    <v-card>
      <v-toolbar flat>
        <v-toolbar-title>Ürün Listesi</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-btn color="primary" prepend-icon="mdi-plus" @click="router.push('/products/new')">Yeni Ürün</v-btn>
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
                  <v-text-field v-model="filters.search" label="Ara..." prepend-inner-icon="mdi-magnify" density="compact" hide-details></v-text-field>
                </v-col>
                <v-col cols="6" md="2">
                  <v-text-field v-model.number="filters.minPrice" label="Min Fiyat" type="number" density="compact" hide-details></v-text-field>
                </v-col>
                <v-col cols="6" md="2">
                  <v-text-field v-model.number="filters.maxPrice" label="Max Fiyat" type="number" density="compact" hide-details></v-text-field>
                </v-col>
                <v-col cols="12" md="3">
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
          :items="products"
          :loading="loading"
          item-value="id"
          fixed-header
          height="65vh"
          show-expand
        >
          <template v-slot:item.available="{ item }">
            <v-chip :color="item.availableQuantity < 0 ? 'red' : 'green'" size="small" variant="tonal">
              {{ item.availableQuantity }}
            </v-chip>
          </template>
          <template v-slot:item.isActive="{ item }">
            <v-chip :color="item.isActive ? 'green' : 'red'" size="small">
              {{ item.isActive ? 'Aktif' : 'Pasif' }}
            </v-chip>
          </template>
          
          <template v-slot:item.actions="{ item }">
            <!-- Aktif Ürünler İçin Eylemler -->
            <div v-if="item.isActive">
              <v-btn icon="mdi-pencil" variant="text" size="small" class="me-2" @click="router.push(`/products/edit/${item.id}`)"></v-btn>
              <v-btn icon="mdi-archive-arrow-down" variant="text" size="small" @click="openDialog({ title: 'Ürünü Arşivle', message: `<strong>${item.name}</strong> isimli ürünü arşive göndermek istediğinize emin misiniz? Bu ürün pasif hale getirilecektir.`, product: item, onConfirm: archiveProduct })"></v-btn>
            </div>
            <!-- Pasif Ürünler İçin Eylemler -->
            <div v-else>
              <v-btn icon="mdi-restore" variant="text" size="small" class="me-2" @click="openDialog({ title: 'Ürünü Geri Yükle', message: `<strong>${item.name}</strong> isimli ürünü tekrar aktif hale getirmek istediğinize emin misiniz?`, product: item, onConfirm: restoreProduct })"></v-btn>
              <v-menu offset-y>
                <template v-slot:activator="{ props }">
                  <v-btn icon="mdi-dots-vertical" variant="text" size="small" v-bind="props"></v-btn>
                </template>
                <v-list dense>
                  <v-list-item @click="openDialog({ title: 'KALICI OLARAK SİL', message: `<strong>UYARI:</strong> <strong>${item.name}</strong> isimli ürünü kalıcı olarak sileceksiniz. <strong>Bu işlem geri alınamaz!</strong>`, color: 'error', product: item, onConfirm: hardDeleteProduct })">
                    <template v-slot:prepend>
                      <v-icon color="error">mdi-delete-forever</v-icon>
                    </template>
                    <v-list-item-title class="text-error">Kalıcı Olarak Sil</v-list-item-title>
                  </v-list-item>
                </v-list>
              </v-menu>
            </div>
          </template>

          <template v-slot:expanded-row="{ columns, item }">
            <tr>
              <td :colspan="columns.length">
                <v-card-text>
                  <div class="font-weight-bold">Açıklama:</div>
                  <div>{{ item.description || 'Açıklama girilmemiş.' }}</div>
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
