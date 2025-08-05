<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import ProductService, { 
  type ProductDto, 
  type CreateProductPayload, 
  type UpdateProductPayload 
} from '@/services/ProductService';
import SettingsService from '@/services/SettingsService';
import type { CurrencyDto } from '@/services/dtos/CurrencyDto';
import { useNotifier } from '@/composables/useNotifier';

const route = useRoute();
const router = useRouter();
const notifier = useNotifier();

const productId = computed(() => route.params.id as string | undefined);
const isEditMode = computed(() => !!productId.value);

const product = ref<Partial<ProductDto>>({
  name: '',
  description: '',
  price: 0,
  currencyId: undefined,
  stockQuantity: 0,
  sku: '',
  isActive: true,
});

const currencies = ref<CurrencyDto[]>([]);
const loading = ref(false);
const errorMessage = ref('');
const pageTitle = computed(() => isEditMode.value ? 'Ürün Düzenle' : 'Yeni Ürün Ekle');

onMounted(async () => {
  loading.value = true;
  try {
    const currencyResponse = await SettingsService.getCurrencies();
    currencies.value = currencyResponse.data;

    if (isEditMode.value) {
      const productResponse = await ProductService.getById(Number(productId.value));
      product.value = productResponse.data;
    } else {
      // Yeni ürün eklerken varsayılan para birimini ata (örn. listedeki ilk para birimi)
      if (currencies.value.length > 0) {
        product.value.currencyId = currencies.value[0].id;
      }
    }
  } catch (error) {
    notifier.error('Veriler yüklenirken bir hata oluştu.');
    console.error(error);
  } finally {
    loading.value = false;
  }
});

const saveProduct = async () => {
  console.log('Saving product. isEditMode:', isEditMode.value); // Hata ayıklama için eklendi
  loading.value = true;
  errorMessage.value = '';
  let success = false;
  let successMessage = '';

  try {
    if (isEditMode.value) {
      const payload: UpdateProductPayload = {
        name: product.value.name,
        description: product.value.description,
        price: product.value.price,
        currencyId: product.value.currencyId,
        stockQuantity: product.value.stockQuantity,
        sku: product.value.sku,
        isActive: product.value.isActive,
      };
      await ProductService.update(Number(productId.value), payload);
      successMessage = 'Ürün başarıyla güncellendi.';
    } else {
      const payload: CreateProductPayload = {
        name: product.value.name!,
        description: product.value.description,
        price: product.value.price!,
        currencyId: product.value.currencyId!,
        stockQuantity: product.value.stockQuantity!,
      };
      await ProductService.create(payload);
      successMessage = 'Ürün başarıyla oluşturuldu.';
    }
    success = true; // API isteği başarılı oldu.
  } catch (error: any) {
    errorMessage.value = error.response?.data?.errors?.[0]?.ErrorMessage || 'Bir hata oluştu.';
    notifier.error(errorMessage.value, { autoClose: 4000 });
  } finally {
    loading.value = false;
    // Yönlendirme sadece API isteği başarılı olduysa yapılır.
    if (success) {
      router.push({ 
        name: 'products', 
        state: { notification: { type: 'success', message: successMessage, duration: 6000 } } 
      });
    }
  }
};
</script>

<template>
  <v-container fluid>
    <v-card>
      <v-card-title>{{ pageTitle }}</v-card-title>
      <v-divider></v-divider>
      <v-card-text>
        <v-alert v-if="errorMessage" type="error" dense class="mb-4">
          {{ errorMessage }}
        </v-alert>

        <v-form @submit.prevent="saveProduct">
          <v-row>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="product.name"
                label="Ürün Adı"
                :rules="[(v: string) => !!v || 'Ürün adı zorunludur']"
                required
              ></v-text-field>
            </v-col>
            
            <v-col v-if="isEditMode" cols="12" md="6">
              <v-text-field
                v-model="product.sku"
                label="SKU (Stok Kodu)"
                readonly
                disabled
                hint="SKU değiştirilemez"
                persistent-hint
              ></v-text-field>
            </v-col>

            <v-col cols="12">
              <v-textarea
                v-model="product.description"
                label="Açıklama"
                rows="3"
              ></v-textarea>
            </v-col>
            <v-col cols="12" md="4">
              <v-text-field
                v-model.number="product.price"
                label="Fiyat"
                type="number"
                :rules="[(v: number) => v >= 0 || 'Fiyat negatif olamaz']"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="2">
              <v-select
                v-model="product.currencyId"
                :items="currencies"
                item-title="code"
                item-value="id"
                label="Para Birimi"
              ></v-select>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field
                v-model.number="product.stockQuantity"
                label="Stok Miktarı"
                type="number"
                :rules="[(v: number) => v >= 0 || 'Stok negatif olamaz']"
              ></v-text-field>
            </v-col>
            <v-col v-if="isEditMode" cols="12">
              <v-switch
                v-model="product.isActive"
                label="Ürün Aktif"
                color="primary"
              ></v-switch>
            </v-col>
          </v-row>

          <v-divider class="my-4"></v-divider>

          <div class="d-flex">
            <v-spacer></v-spacer>
            <v-btn @click="router.push('/products')">İptal</v-btn>
            <v-btn
              type="submit"
              color="primary"
              class="ml-2"
              :loading="loading"
            >
              Kaydet
            </v-btn>
          </div>
        </v-form>
      </v-card-text>
    </v-card>
  </v-container>
</template>