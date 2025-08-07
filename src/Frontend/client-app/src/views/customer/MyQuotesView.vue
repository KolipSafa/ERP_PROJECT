<template>
  <v-container>
    <v-row>
      <v-col>
        <h1 class="text-h4">Tekliflerim</h1>
        <p class="text-subtitle-1">Size özel olarak hazırlanan teklifleri burada bulabilirsiniz.</p>
      </v-col>
    </v-row>

    <v-row v-if="loading">
      <v-col cols="12">
        <v-progress-linear indeterminate></v-progress-linear>
      </v-col>
    </v-row>

    <v-row v-if="!loading && teklifler.length === 0">
      <v-col cols="12">
        <v-alert type="info" variant="tonal">
          Henüz size atanmış bir teklif bulunmuyor.
        </v-alert>
      </v-col>
    </v-row>

    <v-row v-if="!loading && teklifler.length > 0">
      <v-col v-for="teklif in teklifler" :key="teklif.id" cols="12" md="6" lg="4">
        <v-card class="mb-4" :color="getStatusColor(teklif.durum)">
          <v-card-title>{{ teklif.teklifNumarasi }}</v-card-title>
          <v-card-subtitle>
            Sunulma Tarihi: {{ new Date(teklif.teklifTarihi).toLocaleDateString() }}
          </v-card-subtitle>
          <v-card-text>
            <p class="text-h6 font-weight-bold">
              Toplam Tutar: {{ teklif.toplamTutar.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' }) }}
            </p>
            <v-chip size="small" class="mt-2">{{ getStatusText(teklif.durum) }}</v-chip>
          </v-card-text>

          <v-divider></v-divider>

          <v-card-actions v-if="teklif.durum === 0"> <!-- 0: Sunuldu -->
            <v-btn color="success" @click="onayla(teklif.id)">Onayla</v-btn>
            <v-btn color="error" @click="reddet(teklif.id)">Reddet</v-btn>
            <v-btn @click="degisiklikTalepEt(teklif.id)">Değişiklik Talep Et</v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>

    <!-- Snackbar for user feedback -->
    <v-snackbar v-model="snackbar.show" :color="snackbar.color" :timeout="3000">
      {{ snackbar.text }}
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import TeklifService from '@/services/TeklifService';
import type { TeklifDto } from '@/services/dtos/TeklifDtos';
import { useAuthStore } from '@/stores/auth.store';

// --- State ---
const teklifler = ref<TeklifDto[]>([]);
const loading = ref(true);
const authStore = useAuthStore();
const snackbar = ref({
  show: false,
  text: '',
  color: 'success'
});

// --- Lifecycle ---
onMounted(() => {
  fetchTeklifler();
});

// --- Methods ---
const fetchTeklifler = async () => {
  loading.value = true;
  try {
    // Sadece bu müşteriye ait teklifleri getirmek için filtreleme yapıyoruz.
    // Backend'in bu filtreyi desteklediğinden emin olmalıyız.
    const response = await TeklifService.getAll({ musteriId: authStore.user?.id });
    teklifler.value = response.data;
  } catch (error) {
    console.error("Teklifler alınırken hata oluştu:", error);
    showSnackbar('Teklifler yüklenemedi.', 'error');
  } finally {
    loading.value = false;
  }
};

const onayla = async (id: string) => {
  if (!confirm("Bu teklifi onaylamak istediğinizden emin misiniz? Bu işlem geri alınamaz.")) return;
  try {
    await TeklifService.approve(id);
    showSnackbar('Teklif başarıyla onaylandı ve faturanız oluşturuldu.', 'success');
    fetchTeklifler(); // Listeyi yenile
  } catch (error) {
    console.error("Onaylama sırasında hata:", error);
    showSnackbar('Teklif onaylanırken bir hata oluştu.', 'error');
  }
};

const reddet = async (id: string) => {
  if (!confirm("Bu teklifi reddetmek istediğinizden emin misiniz?")) return;
  try {
    await TeklifService.reject(id);
    showSnackbar('Teklif reddedildi.', 'info');
    fetchTeklifler();
  } catch (error) {
    console.error("Reddetme sırasında hata:", error);
    showSnackbar('Teklif reddedilirken bir hata oluştu.', 'error');
  }
};

const degisiklikTalepEt = (id: string) => {
  // TODO: Değişiklik talebi için bir dialog/modal açılıp kullanıcıdan notlar alınabilir.
  // Şimdilik basit bir prompt kullanıyoruz.
  const reason = prompt("Lütfen talep ettiğiniz değişiklikleri kısaca açıklayınız:");
  if (reason) {
    alert(`"'${reason}'" talebiniz yöneticiye iletildi.`);
    // Gerçek implementasyon:
    // TeklifService.requestChange(id, { notes: reason });
  }
};

const showSnackbar = (text: string, color: 'success' | 'error' | 'info' = 'success') => {
  snackbar.value.text = text;
  snackbar.value.color = color;
  snackbar.value.show = true;
};

const getStatusText = (status: number): string => {
  switch (status) {
    case 0: return 'Sunuldu';
    case 1: return 'Onaylandı';
    case 2: return 'Reddedildi';
    case 3: return 'Değişiklik Talep Edildi';
    default: return 'Bilinmeyen Durum';
  }
};

const getStatusColor = (status: number): string => {
  switch (status) {
    case 0: return 'blue-grey'; // Sunuldu
    case 1: return 'green';     // Onaylandı
    case 2: return 'red';       // Reddedildi
    case 3: return 'orange';    // Değişiklik Talep Edildi
    default: return 'grey';
  }
};
</script>
