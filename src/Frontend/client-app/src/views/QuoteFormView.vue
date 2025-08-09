<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import TeklifService from '@/services/TeklifService';
import CustomerService from '@/services/CustomerService';
import ProductService from '@/services/ProductService';
import SettingsService from '@/services/SettingsService';
import type { 
  TeklifDto, 
  TeklifSatiriDto, 
  CreateTeklifPayload,
  UpdateTeklifPayload
} from '@/services/dtos/TeklifDtos';
import type { CustomerDto } from '@/services/CustomerService';
import type { ProductDto } from '@/services/ProductService';
import type { CurrencyDto } from '@/services/dtos/CurrencyDto';
import debounce from 'lodash.debounce';
import { useNotifier } from '@/composables/useNotifier';
import TeklifApi from '@/services/TeklifService';

const route = useRoute();
const router = useRouter();
const notifier = useNotifier();

// --- Mod ve ID Yönetimi ---
const quoteId = computed(() => route.params.id as string | undefined);
const isEditMode = computed(() => !!quoteId.value);
const pageTitle = computed(() => (isEditMode.value ? 'Teklif Düzenle' : 'Yeni Teklif Oluştur'));

// --- Form Veri Modeli ---
const teklif = ref<Partial<TeklifDto>>({
  musteriId: undefined,
  teklifTarihi: new Date().toISOString().split('T')[0],
  gecerlilikTarihi: new Date(new Date().setDate(new Date().getDate() + 15)).toISOString().split('T')[0],
  currencyId: undefined,
  durum: 'Hazırlanıyor',
  teklifSatirlari: [],
});

// --- Müşteri, Ürün, Para Birimi ve Durum Verileri ---
const customers = ref<CustomerDto[]>([]);
const products = ref<ProductDto[]>([]);
const currencies = ref<CurrencyDto[]>([]);
const customerSearch = ref('');
const productSearch = ref('');
const selectedProduct = ref<ProductDto | null>(null);
// Düzenleme modunda, orijinal teklif satır miktarlarını ürün bazında tutar (çifte düşümü önlemek için)
const originalProductQtyMap = ref<Map<number, number>>(new Map());

// Backend enum sırası: Sunuldu=0, Onaylandı=1, Reddedildi=2, ChangeRequested=3
const statusStringMap: { [key: string]: number } = {
  'Sunuldu': 0,
  'Onaylandı': 1,
  'Reddedildi': 2,
  'ChangeRequested': 3,
};

const statusOptions = [
  { title: 'Sunuldu', value: 0 },
  { title: 'Onaylandı', value: 1 },
  { title: 'Reddedildi', value: 2 },
  { title: 'Değişiklik Talep Edildi', value: 3 },
];

const loading = ref(false);
const errorMessage = ref('');

// Para formatlama fonksiyonu
const formatCurrency = (value: number, currencyCode: string = 'TRY') => {
  if (typeof value !== 'number') return '';
  return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: currencyCode }).format(value);
};

const teklifDurum = computed({
  get: () => Number(teklif.value.durum),
  set: (val) => {
    teklif.value.durum = val;
  }
});

// --- Veri Yükleme ---
onMounted(async () => {
  loading.value = true;
  try {
    const [customerResponse, productResponse, currencyResponse] = await Promise.all([
      CustomerService.getAll({ includeInactive: false }),
      ProductService.getAll({ includeInactive: false }),
      SettingsService.getCurrencies()
    ]);
    customers.value = customerResponse.data;
    products.value = productResponse.data;
    currencies.value = currencyResponse.data;

    if (isEditMode.value) {
      const response = await TeklifService.getById(quoteId.value!);
      response.data.teklifTarihi = response.data.teklifTarihi.split('T')[0];
      response.data.gecerlilikTarihi = response.data.gecerlilikTarihi.split('T')[0];
      
      // Durumu string'den sayısal değere çevir
      const statusString = response.data.durum as keyof typeof statusStringMap;
      const statusValue = statusStringMap[statusString];

      teklif.value = {
        ...response.data,
        durum: statusValue,
      };

      // Orijinal satır miktarlarını ürün bazında haritalandır
      const m = new Map<number, number>();
      (teklif.value.teklifSatirlari || []).forEach(s => {
        const pid = s.urunId as unknown as number;
        const q = Number(s.miktar) || 0;
        m.set(pid, (m.get(pid) || 0) + q);
      });
      originalProductQtyMap.value = m;
    }
  } catch (error) {
    notifier.error('Veriler yüklenirken bir hata oluştu.');
    console.error(error);
  } finally {
    loading.value = false;
  }
});

// Ürün arama
watch(productSearch, debounce(async (val: string) => {
  if (val && val.length > 1) {
    const response = await ProductService.getAll({ search: val, includeInactive: false });
    products.value = response.data;
  } else if (!val) {
    const response = await ProductService.getAll({ includeInactive: false });
    products.value = response.data;
  }
}, 300));


// --- Teklif Satırı Mantığı ---
const satirHeaders: any[] = [
  { title: 'Ürün Adı', key: 'urunAdi', width: '30%' },
  { title: 'Açıklama', key: 'aciklama', width: '30%' },
  { title: 'Miktar', key: 'miktar', width: 120, align: 'center' },
  { title: 'Birim Fiyat', key: 'birimFiyat', width: 180 },
  { title: 'Toplam', key: 'toplam', width: 160, align: 'end' },
  { title: 'Eylemler', key: 'actions', sortable: false, align: 'center' },
];

function addProductToQuote() {
  if (!selectedProduct.value) return;
  // Para birimi: İlk üründe teklife otomatik set et; farklı para birimini engelle
  // Teklif para birimini ürün para birimine otomatik eşitle (kullanıcı isterse üstten değiştirir)
  if (!teklif.value.currencyId || teklif.value.currencyId !== (selectedProduct.value.currencyId as unknown as number)) {
    teklif.value.currencyId = selectedProduct.value.currencyId as unknown as number;
  }

  const yeniSatir: TeklifSatiriDto = {
    id: `new_${Date.now()}`,
    urunId: selectedProduct.value.id,
    urunAdi: selectedProduct.value.name,
    aciklama: selectedProduct.value.name,
    miktar: 1,
    birimFiyat: selectedProduct.value.price,
    toplam: selectedProduct.value.price,
  };

  teklif.value.teklifSatirlari?.push(yeniSatir);
  selectedProduct.value = null;
  productSearch.value = '';
}

function removeSatir(satir: TeklifSatiriDto) {
  teklif.value.teklifSatirlari = teklif.value.teklifSatirlari?.filter(s => s.id !== satir.id);
}

const toplamTutar = computed(() => {
  return teklif.value.teklifSatirlari?.reduce((acc, satir) => {
    const toplam = (satir.miktar || 0) * (satir.birimFiyat || 0);
    satir.toplam = toplam;
    return acc + toplam;
  }, 0) || 0;
});
// Kullanılabilir stok hesaplama (ürün bazında, teklifteki geçici miktarlarla birlikte)
const productAvailabilityMap = computed(() => {
  const map = new Map<number, number>();
  // 1) Başlangıç: backend available = stock - reserved (reserved mevcut teklifin orijinal satırlarını da içerir)
  products.value.forEach(p => map.set(p.id as number, (p as any).availableQuantity ?? (p.stockQuantity - p.reservedQuantity)));

  // 2) Düzenleme modunda, orijinal satırları nötrle: available'ı orijinal miktar kadar artır
  originalProductQtyMap.value.forEach((origQty, pid) => {
    map.set(pid, (map.get(pid) || 0) + (Number(origQty) || 0));
  });

  // 3) Mevcut formdaki toplamları ürün bazında topla ve onlardan düş
  const currentTotals = new Map<number, number>();
  (teklif.value.teklifSatirlari || []).forEach(s => {
    const pid = s.urunId as unknown as number;
    currentTotals.set(pid, (currentTotals.get(pid) || 0) + (Number(s.miktar) || 0));
  });
  currentTotals.forEach((qty, pid) => {
    map.set(pid, (map.get(pid) || 0) - qty);
  });

  return map;
});

function getAvailableForProduct(productId: number) {
  return productAvailabilityMap.value.get(productId) ?? 0;
}


const selectedCurrencyCode = computed(() => {
  const currency = currencies.value.find(c => c.id === teklif.value.currencyId);
  return currency ? currency.code : 'TRY';
});

// Ürün seçimi değiştiğinde, seçili para birimini UI’da anında güncelle
watch(selectedProduct, (p) => {
  if (!p) return;
  if (!teklif.value.currencyId) {
    teklif.value.currencyId = p.currencyId as unknown as number;
  }
});


// --- Kaydetme Mantığı ---
const saveQuote = async () => {
  console.log('Saving quote. isEditMode:', isEditMode.value); // Hata ayıklama için eklendi
  loading.value = true;
  errorMessage.value = '';
  let success = false;
  let successMessage = '';

  try {
    if (isEditMode.value) {
      await handleUpdate();
      successMessage = 'Teklif başarıyla güncellendi.';
    } else {
      await handleCreate();
      successMessage = 'Teklif başarıyla oluşturuldu.';
    }
    success = true;
  } catch (error: any) {
    errorMessage.value = error.response?.data?.errors?.[0]?.ErrorMessage || 'Bir hata oluştu.';
    notifier.error(errorMessage.value, { autoClose: 4000 });
  } finally {
    loading.value = false;
    if (success) {
      router.push({ 
        name: 'quotes', 
        state: { notification: { type: 'success', message: successMessage, duration: 6000 } } 
      });
    }
  }
};

async function handleCreate() {
  const gecerlilikTarihi = new Date(teklif.value.gecerlilikTarihi!);
  gecerlilikTarihi.setHours(23, 59, 59, 999);

  const payload: CreateTeklifPayload = {
    musteriId: teklif.value.musteriId!,
    teklifTarihi: new Date(teklif.value.teklifTarihi!).toISOString(),
    gecerlilikTarihi: gecerlilikTarihi.toISOString(),
    currencyId: teklif.value.currencyId!,
    teklifSatirlari: teklif.value.teklifSatirlari!.map(s => ({
      urunId: s.urunId,
      aciklama: s.aciklama,
      miktar: s.miktar,
      birimFiyat: s.birimFiyat,
    })),
  };
  await TeklifService.create(payload);
  // notifier.success('Teklif başarıyla oluşturuldu.', { autoClose: 6000 }); // Bu satır yukarı taşındı.
}

async function handleUpdate() {
  const gecerlilikTarihi = new Date(teklif.value.gecerlilikTarihi!);
  gecerlilikTarihi.setHours(23, 59, 59, 999);

  const payload: UpdateTeklifPayload = {
    musteriId: teklif.value.musteriId!,
    teklifTarihi: new Date(teklif.value.teklifTarihi!).toISOString(),
    gecerlilikTarihi: gecerlilikTarihi.toISOString(),
    currencyId: teklif.value.currencyId!,
    durum: Number(teklif.value.durum),
    isActive: teklif.value.isActive!,
    teklifSatirlari: teklif.value.teklifSatirlari!.map(s => ({
      id: s.id.startsWith('new_') ? undefined : s.id,
      urunId: s.urunId,
      aciklama: s.aciklama,
      miktar: parseFloat(String(s.miktar)) || 1,
      birimFiyat: parseFloat(String(s.birimFiyat)) || 0,
    })),
  };
  
  await TeklifService.update(quoteId.value!, payload);
  // notifier.success('Teklif başarıyla güncellendi.', { autoClose: 6000 }); // Bu satır yukarı taşındı.
}

async function resendQuote() {
  if (!quoteId.value) return;
  try {
    // Önce formdaki değişiklikleri kaydet
    await handleUpdate();
    // Ardından durumu Sunuldu yap
    await TeklifApi.resend(quoteId.value);
    notifier.success('Teklif tekrar sunuldu.', { autoClose: 3000 });
    router.push({ name: 'quotes' });
  } catch (e) {
    notifier.error('Tekrar sunma sırasında hata oluştu.', { autoClose: 4000 });
    console.error(e);
  }
}

</script>

<template>
  <v-container fluid>
    <v-card :loading="loading">
  <v-card-title class="d-flex align-center">
        <span>{{ pageTitle }}</span>
        <v-spacer></v-spacer>
    <v-btn v-if="isEditMode && (Number(teklifDurum) === 3)" color="info" @click="resendQuote">
          Tekrar Sun
        </v-btn>
      </v-card-title>
      <v-divider></v-divider>
      <v-card-text>
        <v-alert v-if="errorMessage" type="error" dense class="mb-4" closable @click:close="errorMessage = ''">
          {{ errorMessage }}
        </v-alert>

        <v-form @submit.prevent="saveQuote">
          <!-- Ana Teklif Bilgileri -->
          <v-row>
            <v-col cols="12" md="4">
              <v-autocomplete
                v-model="teklif.musteriId"
                :items="customers"
                item-title="fullName"
                item-value="id"
                label="Müşteri Seçin"
                :rules="[(v: any) => !!v || 'Müşteri seçimi zorunludur']"
                required
              ></v-autocomplete>
            </v-col>
            <v-col cols="12" md="3">
              <v-text-field
                v-model="teklif.teklifTarihi"
                label="Teklif Tarihi"
                type="date"
                required
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="3">
              <v-text-field
                v-model="teklif.gecerlilikTarihi"
                label="Geçerlilik Tarihi"
                type="date"
                required
              ></v-text-field>
            </v-col>
             <v-col cols="12" md="2">
              <v-select
                v-model="teklif.currencyId"
                :items="currencies"
                item-title="code"
                item-value="id"
                label="Para Birimi"
              ></v-select>
            </v-col>
            <v-col v-if="isEditMode" cols="12" md="2">
              <v-select
                v-model="teklifDurum"
                :items="statusOptions"
                item-title="title"
                item-value="value"
                label="Teklif Durumu"
              ></v-select>
            </v-col>
          </v-row>

          <v-divider class="my-6"></v-divider>

          <!-- Teklif Satırları -->
          <h3 class="mb-4">Teklif Satırları</h3>
          <v-row align="center">
            <v-col cols="12" md="8">
              <v-autocomplete
                v-model="selectedProduct"
                v-model:search="productSearch"
                :items="products"
                item-title="name"
                item-value="id"
                label="Ürün Ara ve Ekle"
                return-object
                hide-details
                no-filter
              >
                <template v-slot:item="{ props, item }">
                  <v-list-item v-bind="props" :title="item.raw.name">
                    <template v-slot:subtitle>
                      Stok: {{ item.raw.stockQuantity }} | Rezerve: {{ item.raw.reservedQuantity }}
                    </template>
                  </v-list-item>
                </template>
              </v-autocomplete>
            </v-col>
            <v-col cols="12" md="2">
              <v-chip v-if="selectedProduct"
                     :color="getAvailableForProduct(selectedProduct.id) < 0 ? 'red' : 'green'"
                     variant="tonal">
                Kalan: {{ getAvailableForProduct(selectedProduct.id) }}
              </v-chip>
            </v-col>
            <v-col cols="12" md="2">
              <v-btn color="primary" @click="addProductToQuote" :disabled="!selectedProduct" block height="56">
                Ekle
              </v-btn>
            </v-col>
          </v-row>

          <v-data-table
            :headers="satirHeaders"
            :items="teklif.teklifSatirlari"
            class="mt-4"
            hide-default-footer
            no-data-text="Teklife ürün ekleyin."
          >
            <template v-slot:item.aciklama="{ item }">
              <v-text-field v-model="item.aciklama" dense hide-details></v-text-field>
            </template>
            <template v-slot:item.miktar="{ item }">
              <div class="qty-cell">
                <v-text-field
                  v-model.number="item.miktar"
                  type="number"
                  min="1"
                  :color="getAvailableForProduct(item.urunId) < 0 ? 'error' : 'success'"
                  :error="getAvailableForProduct(item.urunId) < 0"
                  :messages="[`Kalan: ${getAvailableForProduct(item.urunId)}`]"
                  density="compact"
                  hide-details="auto"
                  class="qty-input"
                ></v-text-field>
              </div>
            </template>
            <template v-slot:item.birimFiyat="{ item }">
              <div class="price-cell">
                <v-text-field v-model.number="item.birimFiyat" type="number" min="0" density="compact" hide-details :prefix="selectedCurrencyCode" class="price-input"></v-text-field>
              </div>
            </template>
            <template v-slot:item.toplam="{ item }">
              <span>{{ formatCurrency(item.toplam, selectedCurrencyCode) }}</span>
            </template>
            <template v-slot:item.actions="{ item }">
              <v-btn icon="mdi-delete" variant="text" color="error" size="small" @click="removeSatir(item)"></v-btn>
            </template>
          </v-data-table>

          <!-- Toplam Tutar -->
          <v-row class="mt-4">
            <v-col class="d-flex justify-end">
              <div class="text-h5">Toplam Tutar: {{ formatCurrency(toplamTutar, selectedCurrencyCode) }}</div>
            </v-col>
          </v-row>

          <v-divider class="my-4"></v-divider>

          <!-- Eylemler -->
          <div class="d-flex">
            <v-spacer></v-spacer>
            <v-btn @click="router.push('/quotes')">İptal</v-btn>
            <v-btn v-if="!(isEditMode && teklifDurum === 3)" type="submit" color="primary" class="ml-2" :loading="loading">
              Kaydet
            </v-btn>
          </div>
        </v-form>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<style scoped>
.qty-cell, .price-cell {
  display: flex;
  align-items: center;
  height: 56px; /* v-data-table default row height */
}
.qty-input, .price-input {
  max-width: 120px;
}
.price-input {
  max-width: 160px;
}
.v-data-table .v-data-table__td {
  vertical-align: middle;
}
</style>