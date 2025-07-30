<template>
  <div>
    <v-toolbar flat>
      <v-toolbar-title>Para Birimi Yönetimi</v-toolbar-title>
      <v-divider class="mx-4" inset vertical></v-divider>
      <v-spacer></v-spacer>
      <v-btn color="primary" dark @click="dialog = true">Yeni Para Birimi</v-btn>
    </v-toolbar>

    <v-data-table
      :headers="headers"
      :items="currencies"
      class="elevation-1"
    >
      <template v-slot:[`item.actions`]="{ item }">
        <v-icon small class="mr-2" @click="editItem(item)">mdi-pencil</v-icon>
        <v-icon small @click="deleteItem(item)">mdi-delete</v-icon>
      </template>
    </v-data-table>

    <v-dialog v-model="dialog" max-width="500px">
      <v-card>
        <v-card-title>
          <span class="text-h5">{{ formTitle }}</span>
        </v-card-title>
        <v-card-text>
          <v-container>
            <v-row>
              <v-col cols="12">
                <v-text-field v-model="editedItem.name" label="Para Birimi Adı"></v-text-field>
              </v-col>
              <v-col cols="12" sm="6">
                <v-text-field v-model="editedItem.code" label="Kod"></v-text-field>
              </v-col>
              <v-col cols="12" sm="6">
                <v-text-field v-model="editedItem.symbol" label="Sembol"></v-text-field>
              </v-col>
            </v-row>
          </v-container>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="blue darken-1" text @click="close">İptal</v-btn>
          <v-btn color="blue darken-1" text @click="save">Kaydet</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-dialog v-model="dialogDelete" max-width="500px">
      <v-card>
        <v-card-title class="text-h5 bg-error">Para Birimini Sil</v-card-title>
        <v-card-text class="py-4">
          <strong>UYARI:</strong> <strong>{{ editedItem.name }} ({{ editedItem.code }})</strong> isimli para birimini sileceksiniz. Bu işlem para biriminin durumunu "Pasif" olarak ayarlayacaktır.
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="grey-darken-1" text @click="closeDelete">İptal</v-btn>
          <v-btn color="error" text @click="deleteItemConfirm">Onayla</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import SettingsService from '@/services/SettingsService';
import type { CurrencyDto } from '@/services/dtos/CurrencyDto';
import { useNotifier } from '@/composables/useNotifier';

const notifier = useNotifier();

const currencies = ref<CurrencyDto[]>([]);
const dialog = ref(false);
const dialogDelete = ref(false);
const editedIndex = ref(-1);

const editedItem = ref<Partial<CurrencyDto>>({
  id: 0,
  name: '',
  code: '',
  symbol: '',
  isActive: true,
});

const defaultItem = {
  id: 0,
  name: '',
  code: '',
  symbol: '',
  isActive: true,
};

const headers = [
  { title: 'Para Birimi Adı', value: 'name' },
  { title: 'Kod', value: 'code' },
  { title: 'Sembol', value: 'symbol' },
  { title: 'Eylemler', value: 'actions', sortable: false },
];

const formTitle = computed(() => (editedIndex.value === -1 ? 'Yeni Para Birimi' : 'Para Birimi Düzenle'));

onMounted(async () => {
  await fetchCurrencies();
});

async function fetchCurrencies() {
  try {
    const response = await SettingsService.getCurrencies();
    currencies.value = response.data;
  } catch (error) {
    notifier.error('Para birimleri getirilirken bir hata oluştu.');
    console.error(error);
  }
}

function editItem(item: CurrencyDto) {
  editedIndex.value = currencies.value.indexOf(item);
  editedItem.value = { ...item };
  dialog.value = true;
}

function deleteItem(item: CurrencyDto) {
  editedIndex.value = currencies.value.indexOf(item);
  editedItem.value = { ...item };
  dialogDelete.value = true;
}

function close() {
  dialog.value = false;
  editedItem.value = { ...defaultItem };
  editedIndex.value = -1;
}

function closeDelete() {
  dialogDelete.value = false;
  editedItem.value = { ...defaultItem };
  editedIndex.value = -1;
}

async function save() {
  try {
    if (editedIndex.value > -1) {
      // Update
      await SettingsService.updateCurrency(editedItem.value.id!, editedItem.value);
      notifier.success('Para birimi başarıyla güncellendi.');
    } else {
      // Create
      const payload = {
        name: editedItem.value.name!,
        code: editedItem.value.code!,
        symbol: editedItem.value.symbol!,
      };
      await SettingsService.createCurrency(payload);
      notifier.success('Para birimi başarıyla oluşturuldu.');
    }
    await fetchCurrencies();
  } catch (error: any) {
    const errorMsg = error.response?.data?.errors?.[0]?.ErrorMessage || 'İşlem sırasında bir hata oluştu.';
    notifier.error(errorMsg);
    console.error(error);
  } finally {
    close();
  }
}

async function deleteItemConfirm() {
  try {
    await SettingsService.deleteCurrency(editedItem.value.id!);
    notifier.success('Para birimi başarıyla silindi.');
    await fetchCurrencies();
  } catch (error) {
    notifier.error('Silme işlemi sırasında bir hata oluştu.');
    console.error(error);
  } finally {
    closeDelete();
  }
}
</script>
