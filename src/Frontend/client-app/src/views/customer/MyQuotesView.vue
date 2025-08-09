<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
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
  { title: '', key: 'data-table-expand' },
  { title: 'Teklif No', key: 'teklifNumarasi' },
  { title: 'Tarih', key: 'teklifTarihi' },
  { title: 'Geçerlilik', key: 'gecerlilikTarihi' },
  { title: 'Tutar', key: 'toplamTutar', align: 'end' },
  { title: 'Durum', key: 'durum', align: 'center' },
  { title: 'Eylemler', key: 'actions', sortable: false, align: 'center' },
]
const expanded = ref<string[]>([])
const detailsMap = ref(new Map<string, TeklifDto>())

// --- Lifecycle Hooks ---
onMounted(async () => {
  // Auth state'in hazır olduğundan emin ol
  await authStore.fetchUser()
  await fetchTeklifler()
})

watch(expanded, async (newVal, oldVal) => {
  const prev = Array.isArray(oldVal) ? oldVal : []
  const opened = newVal.find(id => !prev.includes(id))
  if (opened && !detailsMap.value.has(opened)) {
    try {
      const res = await TeklifService.getById(opened)
      detailsMap.value.set(opened, res.data)
    } catch (e) {
      console.error('Teklif detay alınamadı', e)
    }
  }
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
          show-expand
          v-model:expanded="expanded"
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

          <template v-slot:expanded-row="{ item, columns }">
            <tr>
              <td :colspan="columns.length">
                <v-card flat class="pa-3">
                  <div class="d-flex align-center mb-2">
                    <v-chip :color="getStatusColor(item.durum)" size="small" class="mr-2">
                      {{ getStatusText(item.durum) }}
                    </v-chip>
                  </div>
                  <div v-if="(detailsMap.get(item.id)?.changeRequestNotes || item.changeRequestNotes)" class="mb-3">
                    <v-alert type="info" variant="tonal" density="compact" border="start" border-color="info">
                      {{ detailsMap.get(item.id)?.changeRequestNotes || item.changeRequestNotes }}
                    </v-alert>
                  </div>
                  <div class="text-subtitle-1 mb-2">Kalemler</div>
                  <v-table density="compact">
                    <thead>
                      <tr>
                        <th class="text-left">Ürün</th>
                        <th class="text-right">Miktar</th>
                        <th class="text-right">Birim Fiyat</th>
                        <th class="text-right">Toplam</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="line in (detailsMap.get(item.id)?.teklifSatirlari || item.teklifSatirlari || [])" :key="line.id">
                        <td>{{ line.urunAdi }}</td>
                        <td class="text-right">{{ line.miktar }}</td>
                        <td class="text-right">{{ line.birimFiyat?.toLocaleString('tr-TR') }}</td>
                        <td class="text-right">{{ line.toplam?.toLocaleString('tr-TR') }}</td>
                      </tr>
                    </tbody>
                  </v-table>
                  <div class="d-flex justify-end mt-2">
                    <strong>Genel Toplam:&nbsp;</strong>
                    <span>{{ formatCurrency(item.toplamTutar, item.currencyCode) }}</span>
                  </div>
                </v-card>
              </td>
            </tr>
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