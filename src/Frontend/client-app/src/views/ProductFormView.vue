<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import ProductService, { 
  type ProductDto, 
  type CreateProductPayload, 
  type UpdateProductPayload 
} from '@/services/ProductService';

const route = useRoute();
const router = useRouter();

const productId = computed(() => route.params.id as string | undefined);
const isEditMode = computed(() => !!productId.value);

const product = ref<Partial<ProductDto>>({
  name: '',
  description: '',
  price: 0,
  stockQuantity: 0,
  sku: '',
  isActive: true,
});

const loading = ref(false);
const errorMessage = ref('');
const pageTitle = computed(() => isEditMode.value ? 'Ürün Düzenle' : 'Yeni Ürün Ekle');

onMounted(async () => {
  if (isEditMode.value) {
    loading.value = true;
    try {
      const response = await ProductService.getById(Number(productId.value));
      product.value = response.data;
    } catch (error) {
      console.error('Ürün getirilirken hata:', error);
      errorMessage.value = 'Ürün bilgileri yüklenemedi.';
    } finally {
      loading.value = false;
    }
  }
});

const saveProduct = async () => {
  loading.value = true;
  errorMessage.value = '';
  try {
    if (isEditMode.value) {
      const payload: UpdateProductPayload = {
        name: product.value.name,
        description: product.value.description,
        price: product.value.price,
        stockQuantity: product.value.stockQuantity,
        sku: product.value.sku,
        isActive: product.value.isActive,
      };
      await ProductService.update(Number(productId.value), payload);
    } else {
      const payload: CreateProductPayload = {
        name: product.value.name!,
        description: product.value.description,
        price: product.value.price!,
        stockQuantity: product.value.stockQuantity!,
      };
      await ProductService.create(payload);
    }
    router.push('/products');
  } catch (error: any) {
    console.error('Ürün kaydedilirken hata:', error);
    errorMessage.value = error.response?.data?.errors?.[0]?.ErrorMessage || 'Bir hata oluştu.';
  } finally {
    loading.value = false;
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
                :rules="[v => !!v || 'Ürün adı zorunludur']"
                required
              ></v-text-field>
            </v-col>
            
            <!-- SKU Alanı Artık Koşullu -->
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
            <v-col cols="12" md="6">
              <v-text-field
                v-model.number="product.price"
                label="Fiyat"
                type="number"
                prefix="₺"
                :rules="[v => v >= 0 || 'Fiyat negatif olamaz']"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field
                v-model.number="product.stockQuantity"
                label="Stok Miktarı"
                type="number"
                :rules="[v => v >= 0 || 'Stok negatif olamaz']"
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
