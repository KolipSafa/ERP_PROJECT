<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import CustomerService, { 
  type CustomerDto, 
  type InviteCustomerPayload, 
  type UpdateCustomerPayload 
} from '@/services/CustomerService';
import SettingsService from '@/services/SettingsService';
import type { CompanyDto } from '@/services/dtos/CompanyDto';

import { useNotifier } from '@/composables/useNotifier';

const route = useRoute();
const router = useRouter();
const notifier = useNotifier();

const customerId = computed(() => route.params.id as string | undefined);
const isEditMode = computed(() => !!customerId.value);
const pageTitle = computed(() => isEditMode.value ? 'Müşteri Düzenle' : 'Yeni Müşteri Davet Et');

const isFormValid = ref(false);

const customer = ref<Partial<CustomerDto>>({
  firstName: '',
  lastName: '',
  companyId: undefined,
  phoneNumber: '',
  email: '',
  isActive: true,
});

const rules = {
  required: (value: any) => !!value || 'Bu alan zorunludur.',
  email: (value: string) => {
    const pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return pattern.test(value) || 'Geçerli bir e-posta adresi giriniz.';
  },
  phone: (value: string) => {
    if (!value) return true;
    const digits = value.replace(/\D/g, '');
    return digits.length >= 10 || 'Geçerli bir telefon numarası giriniz.';
  }
};

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
  if (!isFormValid.value) {
    notifier.error('Lütfen formdaki tüm zorunlu alanları doğru bir şekilde doldurun.');
    return;
  }

  loading.value = true;
  errorMessage.value = '';
  let success = false;
  let successMessage = '';

  const cleanedPhoneNumber = customer.value.phoneNumber?.replace(/\D/g, '');

  try {
    if (isEditMode.value) {
      const payload: UpdateCustomerPayload = {
        firstName: customer.value.firstName,
        lastName: customer.value.lastName,
        companyId: customer.value.companyId,
        phoneNumber: cleanedPhoneNumber,
        email: customer.value.email,
        isActive: customer.value.isActive,
      };
      await CustomerService.update(customerId.value!, payload);
      successMessage = 'Müşteri başarıyla güncellendi.';
    } else {
      const payload: InviteCustomerPayload = {
        email: customer.value.email!,
        data: {
          first_name: customer.value.firstName!,
          last_name: customer.value.lastName!,
          company_id: customer.value.companyId,
          phone_number: cleanedPhoneNumber,
        }
      };
      await CustomerService.inviteCustomer(payload);
      successMessage = 'Müşteri başarıyla davet edildi. Kullanıcıya şifre belirlemesi için bir e-posta gönderildi.';
    }
    success = true;
  } catch (error: any) {
    errorMessage.value = error.response?.data?.errors?.[0]?.ErrorMessage || error.message || 'Bir hata oluştu.';
    notifier.error(errorMessage.value, { autoClose: 4000 });
  } finally {
    loading.value = false;
    if (success) {
      router.push({ 
        name: 'customers', 
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

        <v-form v-model="isFormValid" @submit.prevent="saveCustomer">
          <v-row>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="customer.firstName"
                label="Ad"
                :rules="[rules.required]"
                required
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="customer.lastName"
                label="Soyad"
                :rules="[rules.required]"
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
                :rules="[rules.required]"
                required
                clearable
              ></v-autocomplete>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="customer.phoneNumber"
                label="Telefon Numarası"
                :rules="[rules.phone]"
                mask="(###) ###-####"
                placeholder="(555) 123-4567"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field
                v-model="customer.email"
                label="E-posta Adresi"
                type="email"
                :rules="[rules.required, rules.email]"
                required
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
              :disabled="!isFormValid"
            >
              {{ isEditMode ? 'Kaydet' : 'Davet Gönder' }}
            </v-btn>
          </div>
        </v-form>
      </v-card-text>
    </v-card>
  </v-container>
</template>
