<script setup lang="ts">
import { ref, onMounted } from 'vue'
import TeklifService from '@/services/TeklifService'
import type { TeklifDto } from '@/services/dtos/TeklifDtos'
import { useAuthStore } from '@/stores/auth.store'
import { useNotifier } from '@/composables/useNotifier'

// --- Composables ---
const authStore = useAuthStore()
const notifier = useNotifier()

// --- Component State ---
const teklifler = ref<TeklifDto[]>([])
const loading = ref(true)

// --- Dialog State ---
const dialog = ref(false)
const dialogTitle = ref('')
const dialogMessage = ref('')
const dialogColor = ref('primary')
const selectedTeklif = ref<TeklifDto | null>(null)
let onConfirmAction: () => void = () => {}

const revisionDialog = ref(false)
const revisionNotes = ref('')

// --- Data Table Headers ---
const headers: any[] = [
  { title: 'Teklif No', key: 'teklifNumarasi' },
  { title: 'Tarih', key: 'teklifTarihi' },
  { title: 'Geçerlilik', key: 'gecerlilikTarihi' },
  { title: 'Kalemler', key: 'items' },
  { title: 'Tutar', key: 'toplamTutar', align: 'end' },
  { title: 'Durum', key: 'durum', align: 'center' },
  { title: 'Eylemler', key: 'actions', sortable: false, align: 'center' },
]

// --- Lifecycle Hooks ---
onMounted(async () => {
  // Auth state'in hazır olduğundan emin ol
  await authStore.fetchUser()
  await fetchTeklifler()
})

// --- Data Fetching ---
const fetchTeklifler = async () => {
  loading.value = true
  try {
    const response = await TeklifService.getAll({ applicationUserId: authStore.user?.id })
    teklifler.value = response.data
  } catch (error) {
    console.error('Teklifler alınırken hata oluştu:', error)
    notifier.error('Teklifler yüklenemedi.')
  } finally {
    loading.value = false
  }
}

// --- Dialog Management ---
function openConfirmationDialog(config: {
  title: string
  message: string
  color?: string
  teklif: TeklifDto
  onConfirm: () => void
}) {
  dialogTitle.value = config.title
  dialogMessage.value = config.message
  dialogColor.value = config.color || 'primary'
  selectedTeklif.value = config.teklif
  onConfirmAction = config.onConfirm
  dialog.value = true
}

function confirmDialog() {
  onConfirmAction()
  dialog.value = false
}

function openRevisionDialog(teklif: TeklifDto) {
  selectedTeklif.value = teklif
  revisionNotes.value = ''
  revisionDialog.value = true
}

// --- Actions ---
const approveTeklif = async () => {
  if (!selectedTeklif.value) return
  try {
    await TeklifService.approve(selectedTeklif.value.id)
    notifier.success('Teklif başarıyla onaylandı ve faturanız oluşturuldu.')
    fetchTeklifler()
  } catch (error) {
    notifier.error('Teklif onaylanırken bir hata oluştu.')
  }
}

const rejectTeklif = async () => {
  if (!selectedTeklif.value) return
  try {
    await TeklifService.reject(selectedTeklif.value.id)
    notifier.info('Teklif reddedildi.')
    fetchTeklifler()
  } catch (error) {
    notifier.error('Teklif reddedilirken bir hata oluştu.')
  }
}

const requestRevision = async () => {
  if (!selectedTeklif.value || !revisionNotes.value) return
  try {
    await TeklifService.requestChange(selectedTeklif.value.id, { notes: revisionNotes.value })
    notifier.success('Revizyon talebiniz yöneticiye iletildi.')
    revisionDialog.value = false
    fetchTeklifler()
  } catch (error) {
    notifier.error('Revizyon talebi gönderilirken bir hata oluştu.')
  }
}

// --- Formatting and Display ---
const formatDate = (dateString: string) => new Date(dateString).toLocaleDateString('tr-TR')
const formatCurrency = (value: number, currencyCode = 'TRY') =>
  new Intl.NumberFormat('tr-TR', { style: 'currency', currency: currencyCode }).format(value)

const getStatusText = (status: string | number) => {
  const statusMap: { [key: string]: string } = {
    Sunuldu: 'Sunuldu',
    Onaylandı: 'Onaylandı',
    Reddedildi: 'Reddedildi',
    ChangeRequested: 'Değişiklik Talep Edildi',
  }
  return statusMap[status] || 'Bilinmeyen Durum'
}

const getStatusColor = (status: string | number) => {
  const colorMap: { [key: string]: string } = {
    Sunuldu: 'info',
    Onaylandı: 'success',
    Reddedildi: 'error',
    ChangeRequested: 'warning',
  }
  return colorMap[status] || 'grey'
}
</script>

<template>
  <v-container fluid>
    <v-card>
      <v-toolbar flat>
        <v-toolbar-title>Tekliflerim</v-toolbar-title>
      </v-toolbar>

      <v-card-text>
        <v-data-table
          :headers="headers"
          :items="teklifler"
          :loading="loading"
          item-value="id"
          no-data-text="Henüz size atanmış bir teklif bulunmuyor."
        >
          <template v-slot:item.teklifTarihi="{ item }">
            <span>{{ formatDate(item.teklifTarihi) }}</span>
          </template>
          <template v-slot:item.gecerlilikTarihi="{ item }">
            <span>{{ formatDate(item.gecerlilikTarihi) }}</span>
          </template>
          <template v-slot:item.toplamTutar="{ item }">
            <span>{{ formatCurrency(item.toplamTutar, item.currencyCode) }}</span>
          </template>
          <template v-slot:item.durum="{ item }">
            <v-chip :color="getStatusColor(item.durum)" size="small">
              {{ getStatusText(item.durum) }}
            </v-chip>
          </template>

          <template v-slot:item.items="{ item }">
            <div>
              <div v-for="line in (item.teklifSatirlari || []).slice(0,2)" :key="line.id" class="text-caption">
                - {{ line.urunAdi }} x {{ line.miktar }}
              </div>
              <div v-if="(item.teklifSatirlari || []).length > 2" class="text-caption">…</div>
            </div>
          </template>

          <template v-slot:item.actions="{ item }">
            <div v-if="item.durum === 'Sunuldu'">
              <v-btn
                color="success"
                size="small"
                class="mr-2"
                @click="
                  openConfirmationDialog({
                    title: 'Teklifi Onayla',
                    message: `<strong>${item.teklifNumarasi}</strong> numaralı teklifi onaylamak istediğinize emin misiniz? Bu işlem sonrası faturanız oluşturulacaktır.`,
                    color: 'success',
                    teklif: item,
                    onConfirm: approveTeklif,
                  })
                "
              >
                Onayla
              </v-btn>
              <v-btn
                color="error"
                size="small"
                class="mr-2"
                @click="
                  openConfirmationDialog({
                    title: 'Teklifi Reddet',
                    message: `<strong>${item.teklifNumarasi}</strong> numaralı teklifi reddetmek istediğinize emin misiniz?`,
                    color: 'error',
                    teklif: item,
                    onConfirm: rejectTeklif,
                  })
                "
              >
                Reddet
              </v-btn>
              <v-btn size="small" @click="openRevisionDialog(item)"> Revize Talep Et </v-btn>
            </div>
            <div v-else>-</div>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>

    <!-- Onay Diyalogu -->
    <v-dialog v-model="dialog" max-width="500px">
      <v-card>
        <v-card-title class="text-h5" :class="`bg-${dialogColor}`">{{ dialogTitle }}</v-card-title>
        <v-card-text class="py-4" v-html="dialogMessage"></v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="grey-darken-1" text @click="dialog = false">İptal</v-btn>
          <v-btn :color="dialogColor" text @click="confirmDialog">Onayla</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Revizyon Talebi Diyalogu -->
    <v-dialog v-model="revisionDialog" max-width="600px">
      <v-card>
        <v-card-title class="text-h5 bg-primary">Revizyon Talebi</v-card-title>
        <v-card-text class="py-4">
          <p class="mb-4">
            Lütfen <strong>{{ selectedTeklif?.teklifNumarasi }}</strong> numaralı teklif için
            talep ettiğiniz değişiklikleri detaylı olarak açıklayınız.
          </p>
          <v-textarea
            v-model="revisionNotes"
            label="Değişiklik talepleriniz"
            rows="5"
            auto-grow
            variant="outlined"
            required
          ></v-textarea>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="grey-darken-1" text @click="revisionDialog = false">İptal</v-btn>
          <v-btn color="primary" text @click="requestRevision" :disabled="!revisionNotes.trim()">
            Talebi Gönder
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>