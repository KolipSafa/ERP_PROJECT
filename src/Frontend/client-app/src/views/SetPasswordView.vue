<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import AuthService from '@/services/AuthService';
import CustomerService from '@/services/CustomerService';
import type { CreateCustomerPayload } from '@/services/CustomerService';
import { useNotifier } from '@/composables/useNotifier';
import { useAuthStore } from '@/stores/auth.store';

const router = useRouter();
const notifier = useNotifier();
const authStore = useAuthStore();

const isFormValid = ref(false);
const password = ref('');
const passwordConfirm = ref('');
const loading = ref(false);

const rules = {
  required: (value: string) => !!value || 'Bu alan zorunludur.',
  minLength: (value: string) => (value && value.length >= 6) || 'Şifre en az 6 karakter olmalıdır.',
  match: (value: string) => value === password.value || 'Şifreler eşleşmiyor.',
};

const savePassword = async () => {
  if (!isFormValid.value) {
    notifier.error('Lütfen formu doğru bir şekilde doldurun.');
    return;
  }

  loading.value = true;
  try {
    // 1. Supabase'de şifreyi güncelle
    await AuthService.updateUserPassword(password.value);

    // 2. Store'daki mevcut kullanıcı bilgilerini al
    const user = authStore.user;
    if (!user || !user.id || !user.email) {
      throw new Error('Oturum bilgileri alınamadı. Lütfen davet linkiyle tekrar deneyin.');
    }
    
    // 3. Backend'e müşteri kaydını oluştur ve AKTİVASYONU tamamla
    const payload: CreateCustomerPayload = {
      applicationUserId: user.id,
      email: user.email,
      firstName: user.first_name,
      lastName: user.last_name,
      companyId: user.company_id,
      phoneNumber: user.phone_number,
    };
    
    // Bu çağrı artık backend'den güncel Supabase User bilgisini de getirecek
    await CustomerService.create(payload);

    // 4. Store'u en güncel session bilgisiyle ZORLA tazele
    // Bu, backend'in yaptığı status değişikliğini anında yansıtacaktır.
    await authStore.forceRefreshSession();

    notifier.success('Şifreniz başarıyla oluşturuldu ve hesabınız aktive edildi. Panele yönlendiriliyorsunuz...');

    // 5. Yönlendirmeyi yap
    // Router guard (router/index.ts) artık güncel 'status' bilgisine sahip
    // olduğu için doğru yönlendirmeyi kendisi yapacaktır.
    router.push('/my-quotes');

  } catch (error: any) {
    const errorMessage = error.response?.data?.message || error.message || 'Şifre güncellenirken bir hata oluştu.';
    notifier.error(errorMessage);
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <v-container class="fill-height" fluid>
    <v-row align="center" justify="center">
      <v-col cols="12" sm="8" md="4">
        <v-card class="elevation-12">
          <v-toolbar color="primary" dark flat>
            <v-toolbar-title>Yeni Şifre Oluştur</v-toolbar-title>
          </v-toolbar>
          <v-card-text>
            <p class="mb-4">
              Hesabınızı güvence altına almak için lütfen yeni bir şifre belirleyin.
            </p>
            <v-form v-model="isFormValid" @submit.prevent="savePassword">
              <v-text-field
                v-model="password"
                label="Yeni Şifre"
                type="password"
                required
                :rules="[rules.required, rules.minLength]"
              ></v-text-field>
              <v-text-field
                v-model="passwordConfirm"
                label="Yeni Şifre (Tekrar)"
                type="password"
                required
                :rules="[rules.required, rules.match]"
              ></v-text-field>
              <v-btn
                type="submit"
                color="primary"
                :loading="loading"
                :disabled="!isFormValid"
                block
              >
                Şifreyi Kaydet
              </v-btn>
            </v-form>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>