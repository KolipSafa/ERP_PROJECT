<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import CustomerService, { 
  type CustomerDto, 
  type CreateCustomerPayload, 
  type UpdateCustomerPayload 
} from '@/services/CustomerService';
import SettingsService from '@/services/SettingsService';
import type { CompanyDto } from '@/services/dtos/CompanyDto';
import NotificationService from '@/services/NotificationService';

import { useNotifier } from '@/composables/useNotifier';

const route = useRoute();
const router = useRouter();
const notifier = useNotifier();

const customerId = computed(() => route.params.id as string | undefined);
const isEditMode = computed(() => !!customerId.value);
const pageTitle = computed(() => isEditMode.value ? 'Müşteri Düzenle' : 'Yeni Müşteri Ekle');

const customer = ref<Partial<CustomerDto>>({
  firstName: '',
  lastName: '',
  companyId: undefined,
  taxNumber: '',
  address: '',
  phoneNumber: '',
  email: '',
  isActive: true,
});

const companies = ref<CompanyDto[]>([]);
const loading = ref(false);
const errorMessage = ref('');

onMounted(async () => {
  loading.value = true;
  try {
    const companiesResponse = await SettingsService.getCompanies();
    companies.value = companiesResponse.data;

    if (isEditMode.value) {
      const customerResponse = await CustomerService.getById(customerId.value!);
      customer.value = customerResponse.data;
    }
  } catch (error) {
    notifier.error('Veriler yüklenirken bir hata oluştu.');
    console.error(error);
  } finally {
    loading.value = false;
  }
});

const saveCustomer = async () => {
  loading.value = true;
  errorMessage.value = '';
  try {
    if (isEditMode.value) {
      const payload: UpdateCustomerPayload = {
        firstName: customer.value.firstName,
        lastName: customer.value.lastName,
        companyId: customer.value.companyId,
        taxNumber: customer.value.taxNumber,
        address: customer.value.address,
        phoneNumber: customer.value.phoneNumber,
        email: customer.value.email,
        isActive: customer.value.isActive,
      };
      await CustomerService.update(customerId.value!, payload);
      notifier.success('Müşteri başarıyla güncellendi.');
    } else {
      const payload: CreateCustomerPayload = {
        firstName: customer.value.firstName!,
        lastName: customer.value.lastName!,
        companyId: customer.value.companyId,
        taxNumber: customer.value.taxNumber,
        address: customer.value.address,
        phoneNumber: customer.value.phoneNumber,
        email: customer.value.email,
      };
      await CustomerService.create(payload);
      notifier.success('Müşteri başarıyla oluşturuldu.');
    }
    router.push('/customers');
  } catch (error: any) {
    errorMessage.value = error.response?.data?.errors?.[0]?.ErrorMessage || 'Bir hata oluştu.';
    notifier.error(errorMessage.value);
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
              <v-autocomplete
                v-model="customer.companyId"
                :items="companies"
                item-title="name"
                item-value="id"
                label="Firma"
                clearable
              ></v-autocomplete>
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
