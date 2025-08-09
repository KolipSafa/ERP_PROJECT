<template>
  <v-container>
    <v-row>
      <v-col>
        <h1 class="text-h4">Faturalarım</h1>
        <p class="text-subtitle-1">Tüm faturalarınızı buradan takip edebilirsiniz.</p>
      </v-col>
    </v-row>

    <v-card>
      <v-card-text>
        <v-data-table
          :headers="headers"
          :items="invoices"
          :loading="loading"
          item-value="id"
        >
          <template v-slot:item.invoiceDate="{ item }">
            <span>{{ new Date(item.invoiceDate).toLocaleDateString() }}</span>
          </template>
          <template v-slot:item.dueDate="{ item }">
            <span>{{ new Date(item.dueDate).toLocaleDateString() }}</span>
          </template>
          <template v-slot:item.totalAmount="{ item }">
            <span>{{ item.totalAmount.toLocaleString('tr-TR', { style: 'currency', currency: 'TRY' }) }}</span>
          </template>
          <template v-slot:item.status="{ item }">
            <v-chip :color="getStatusColor(item.status)" size="small">
              {{ getStatusText(item.status) }}
            </v-chip>
          </template>
          <template v-slot:item.actions="{ item }">
            <v-btn icon="mdi-eye" variant="text" size="small" @click="viewInvoice(item.id)"></v-btn>
            <v-btn icon="mdi-download" variant="text" size="small" @click="downloadInvoice(item.id)"></v-btn>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import InvoiceService from '@/services/InvoiceService';
import type { InvoiceDto } from '@/services/InvoiceService';
import { useAuthStore } from '@/stores/auth.store';

const invoices = ref<InvoiceDto[]>([]);
const loading = ref(true);
const authStore = useAuthStore();

const headers = [
  { title: 'Fatura No', key: 'invoiceNumber' },
  { title: 'Fatura Tarihi', key: 'invoiceDate' },
  { title: 'Vade Tarihi', key: 'dueDate' },
  { title: 'Tutar', key: 'totalAmount', align: 'end' },
  { title: 'Durum', key: 'status', align: 'center' },
  { title: 'Eylemler', key: 'actions', sortable: false, align: 'end' },
];

onMounted(async () => {
  loading.value = true;
  try {
    // Customer için params gerekmez; backend kendi faturalarını döndürür.
    const response = await InvoiceService.getAll();
    invoices.value = response.data;
  } catch (error) {
    console.error("Faturalar alınamadı:", error);
  } finally {
    loading.value = false;
  }
});

const getStatusText = (status: number): string => {
  const statuses: { [key: number]: string } = {
    0: 'Taslak',
    1: 'Gönderildi',
    2: 'Ödendi',
    3: 'Vadesi Geçti'
  };
  return statuses[status] || 'Bilinmiyor';
};

const getStatusColor = (status: number): string => {
  const colors: { [key: number]: string } = {
    0: 'grey',
    1: 'blue',
    2: 'success',
    3: 'error'
  };
  return colors[status] || 'grey';
};

const viewInvoice = (id: string) => {
  // TODO: Fatura detay görünümü için bir route'a yönlendir.
  alert(`Fatura ${id} görüntülenecek.`);
};

const downloadInvoice = async (id: string) => {
  try {
    const res = await InvoiceService.downloadPdf(id);
    const blob = new Blob([res.data], { type: 'application/pdf' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `invoice-${id}.pdf`;
    document.body.appendChild(a);
    a.click();
    a.remove();
    window.URL.revokeObjectURL(url);
  } catch (e) {
    console.error('PDF indirilemedi', e);
  }
};
</script>
