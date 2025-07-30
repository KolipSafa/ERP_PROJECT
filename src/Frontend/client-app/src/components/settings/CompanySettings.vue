<template>
  <div>
    <v-toolbar flat>
      <v-toolbar-title>Firma Yönetimi</v-toolbar-title>
      <v-divider class="mx-4" inset vertical></v-divider>
      <v-spacer></v-spacer>
      <v-btn color="primary" dark @click="dialog = true">Yeni Firma</v-btn>
    </v-toolbar>

    <v-data-table
      :headers="headers"
      :items="companies"
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
                <v-text-field v-model="editedItem.name" label="Firma Adı"></v-text-field>
              </v-col>
              <v-col cols="12">
                <v-text-field v-model="editedItem.taxNumber" label="Vergi Numarası"></v-text-field>
              </v-col>
              <v-col cols="12">
                <v-textarea v-model="editedItem.address" label="Adres"></v-textarea>
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
        <v-card-title class="text-h5 bg-error">Firmayı Sil</v-card-title>
        <v-card-text class="py-4">
          <strong>UYARI:</strong> <strong>{{ editedItem.name }}</strong> isimli firmayı sileceksiniz. Bu işlem firmanın durumunu "Pasif" olarak ayarlayacaktır.
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
import type { CompanyDto } from '@/services/dtos/CompanyDto';
import { useNotifier } from '@/composables/useNotifier';

const notifier = useNotifier();

const companies = ref<CompanyDto[]>([]);
const dialog = ref(false);
const dialogDelete = ref(false);
const editedIndex = ref(-1);

const editedItem = ref<Partial<CompanyDto>>({
  id: '',
  name: '',
  taxNumber: '',
  address: '',
  isActive: true,
});

const defaultItem = {
  id: '',
  name: '',
  taxNumber: '',
  address: '',
  isActive: true,
};

const headers = [
  { title: 'Firma Adı', value: 'name' },
  { title: 'Vergi Numarası', value: 'taxNumber' },
  { title: 'Adres', value: 'address' },
  { title: 'Eylemler', value: 'actions', sortable: false },
];

const formTitle = computed(() => (editedIndex.value === -1 ? 'Yeni Firma' : 'Firma Düzenle'));

onMounted(async () => {
  await fetchCompanies();
});

async function fetchCompanies() {
  try {
    const response = await SettingsService.getCompanies();
    companies.value = response.data;
  } catch (error) {
    notifier.error('Firmalar getirilirken bir hata oluştu.');
    console.error(error);
  }
}

function editItem(item: CompanyDto) {
  editedIndex.value = companies.value.indexOf(item);
  editedItem.value = { ...item };
  dialog.value = true;
}

function deleteItem(item: CompanyDto) {
  editedIndex.value = companies.value.indexOf(item);
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
      await SettingsService.updateCompany(editedItem.value.id!, editedItem.value);
      notifier.success('Firma başarıyla güncellendi.');
    } else {
      // Create
      const payload = {
        name: editedItem.value.name!,
        taxNumber: editedItem.value.taxNumber,
        address: editedItem.value.address,
      };
      await SettingsService.createCompany(payload);
      notifier.success('Firma başarıyla oluşturuldu.');
    }
    await fetchCompanies();
  } catch (error) {
    notifier.error('İşlem sırasında bir hata oluştu.');
    console.error(error);
  } finally {
    close();
  }
}

async function deleteItemConfirm() {
  try {
    await SettingsService.deleteCompany(editedItem.value.id!);
    notifier.success('Firma başarıyla silindi.');
    await fetchCompanies();
  } catch (error) {
    notifier.error('Silme işlemi sırasında bir hata oluştu.');
    console.error(error);
  } finally {
    closeDelete();
  }
}
</script>
