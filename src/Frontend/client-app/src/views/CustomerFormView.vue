<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import CustomerService, { 
  type CustomerDto, 
  type CreateCustomerPayload, 
  type UpdateCustomerPayload 
} from '@/services/CustomerService';

const route = useRoute();
const router = useRouter();

// --- Mod ve ID Yönetimi ---
// `computed` property'ler, bağımlı oldukları kaynak (route.params.id) değiştiğinde
// otomatik olarak yeniden hesaplanan, performanslı ve reaktif değişkenlerdir.
const customerId = computed(() => route.params.id as string | undefined);
const isEditMode = computed(() => !!customerId.value);
const pageTitle = computed(() => isEditMode.value ? 'Müşteri Düzenle' : 'Yeni Müşteri Ekle');

// --- Form Veri Modeli ---
// `Partial<CustomerDto>` kullanıyoruz çünkü yeni müşteri eklerken form başlangıçta boştur
// ve tüm alanlar hemen dolu olmayabilir. Bu, TypeScript'in "property 'x' is missing"
// hatası vermesini engeller.
const customer = ref<Partial<CustomerDto>>({
  firstName: '',
  lastName: '',
  companyName: '',
  taxNumber: '',
  address: '',
  phoneNumber: '',
  email: '',
  isActive: true, // Yeni müşteriler varsayılan olarak aktif başlar.
});

const loading = ref(false);
const errorMessage = ref('');

// --- Veri Yükleme (Sadece Düzenleme Modunda) ---
// `onMounted`, component DOM'a eklendiğinde bir kereliğine çalışan bir "lifecycle hook"tur.
// Düzenleme modundaysak, müşterinin mevcut verilerini backend'den çekmek için en doğru yerdir.
onMounted(async () => {
  if (isEditMode.value) {
    loading.value = true;
    try {
      const response = await CustomerService.getById(customerId.value!);
      customer.value = response.data;
    } catch (error) {
      console.error('Müşteri getirilirken hata:', error);
      errorMessage.value = 'Müşteri bilgileri yüklenemedi.';
    } finally {
      loading.value = false;
    }
  }
});

// --- Kaydetme Mantığı ---
const saveCustomer = async () => {
  loading.value = true;
  errorMessage.value = '';
  try {
    if (isEditMode.value) {
      // Düzenleme modunda, `UpdateCustomerPayload` tipine uygun bir nesne oluştururuz.
      // Bu nesne sadece backend'in güncellemesine izin verilen alanları içerir.
      const payload: UpdateCustomerPayload = {
        firstName: customer.value.firstName,
        lastName: customer.value.lastName,
        companyName: customer.value.companyName,
        taxNumber: customer.value.taxNumber,
        address: customer.value.address,
        phoneNumber: customer.value.phoneNumber,
        email: customer.value.email,
        isActive: customer.value.isActive,
        // `balance` gibi alanları buradan gönderemeyiz, çünkü payload tipimiz buna izin vermez.
      };
      await CustomerService.update(customerId.value!, payload);
    } else {
      // Yeni ekleme modunda, `CreateCustomerPayload` tipine uygun bir nesne oluştururuz.
      const payload: CreateCustomerPayload = {
        firstName: customer.value.firstName!,
        lastName: customer.value.lastName!,
        companyName: customer.value.companyName,
        taxNumber: customer.value.taxNumber,
        address: customer.value.address,
        phoneNumber: customer.value.phoneNumber,
        email: customer.value.email,
      };
      await CustomerService.create(payload);
    }
    router.push('/customers'); // Başarılı işlem sonrası listeleme sayfasına yönlendir.
  } catch (error: any) {
    console.error('Müşteri kaydedilirken hata:', error);
    // Backend'den gelen FluentValidation hatalarını göstermek için ideal bir yapı:
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

        <v-form @submit.prevent="saveCustomer">
          <v-row>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="customer.firstName"
                label="Ad"
                :rules="[v => !!v || 'Ad zorunludur']"
                required
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="customer.lastName"
                label="Soyad"
                :rules="[v => !!v || 'Soyad zorunludur']"
                required
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="customer.companyName"
                label="Firma Adı"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="customer.taxNumber"
                label="Vergi Numarası"
              ></v-text-field>
            </v-col>
            <v-col cols="12">
              <v-text-field
                v-model="customer.address"
                label="Adres"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="customer.phoneNumber"
                label="Telefon Numarası"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="customer.email"
                label="E-posta Adresi"
                type="email"
              ></v-text-field>
            </v-col>
            
            <!-- Bakiye alanı sadece düzenleme modunda ve sadece okunabilir olarak gösterilir.
                 Çünkü bakiye, faturalar ve ödemelerle otomatik değişen bir alandır, manuel değiştirilmemelidir. -->
            <v-col v-if="isEditMode" cols="12" md="6">
              <v-text-field
                :model-value="new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(customer.balance || 0)"
                label="Bakiye"
                readonly
                disabled
                hint="Bakiye otomatik olarak hesaplanır"
                persistent-hint
              ></v-text-field>
            </v-col>

            <v-col v-if="isEditMode" cols="12" md="6">
              <v-switch
                v-model="customer.isActive"
                label="Müşteri Aktif"
                color="primary"
              ></v-switch>
            </v-col>
          </v-row>

          <v-divider class="my-4"></v-divider>

          <div class="d-flex">
            <v-spacer></v-spacer>
            <v-btn @click="router.push('/customers')">İptal</v-btn>
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
