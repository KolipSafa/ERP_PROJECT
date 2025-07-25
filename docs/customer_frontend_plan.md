# **Müşteri Modülü Frontend Geliştirme Planı**

Bu belge, Müşteri Cari modülünün frontend geliştirmesi için izlenecek adımları, kullanılacak component'leri ve `Product` modülünden alınacak referans desenleri detaylandırmaktadır. Amaç, uygulama genelinde tam bir görsel ve işlevsel tutarlılık sağlamaktır.

---

### **1. Dosya Yapısı ve Yeni Component'ler**

Aşağıdaki yeni dosyalar oluşturulacaktır:

*   `src/services/CustomerService.ts`: Backend ile tüm müşteri API iletişimini yönetecek olan servis.
*   `src/views/CustomersView.vue`: Müşterilerin listeleneceği, filtreleneceği ve yönetileceği ana sayfa.
*   `src/views/CustomerFormView.vue`: Yeni müşteri ekleme ve mevcut müşteriyi düzenleme işlemlerinin yapılacağı form sayfası.

---

### **2. Adım: API Servisi (`CustomerService.ts`)**

Bu servis, `ProductService.ts`'in yapısını birebir takip edecektir.

*   **`CustomerDto` Interface'i:** Backend'deki `CustomerDto` ile eşleşen bir TypeScript `interface`'i tanımlanacak.
    ```typescript
    export interface CustomerDto {
      id: string; // Guid, frontend'de string olarak ele alınır
      firstName: string;
      lastName: string;
      fullName: string;
      companyName?: string;
      email?: string;
      phoneNumber?: string;
      balance: number;
      isActive: boolean;
    }
    ```
*   **Filtre Parametreleri:** Müşteri listesini filtrelemek için `CustomerFilterParams` adında bir `interface` oluşturulacak.
*   **Payload Tipleri:** `Create` ve `Update` işlemleri için `CreateCustomerPayload` ve `UpdateCustomerPayload` adında, `Omit` ve `Partial` kullanılarak güvenli tipler oluşturulacak.
*   **Metotlar:** Aşağıdaki `axios` metotları implemente edilecek:
    *   `getAll(params: CustomerFilterParams)`
    *   `getById(id: string)`
    *   `create(customer: CreateCustomerPayload)`
    *   `update(id: string, customer: UpdateCustomerPayload)` (Backend ile tutarlılık için `axios.patch` kullanacak)
    *   `archive(id: string)` (Soft delete için `DELETE /api/customers/{id}`)
    *   `restore(id: string)` (`update` endpoint'ini kullanarak `isActive: true` gönderen bir `patch` isteği)

---

### **3. Adım: Müşteri Listeleme Sayfası (`CustomersView.vue`)**

Bu sayfa, `ProductsView.vue`'nin görsel ve yapısal ikizi olacaktır.

*   **Ana Yapı:** `v-card` içinde `v-toolbar` ve `v-data-table` yerleşimi kullanılacak.
*   **Toolbar:** "Müşteri Listesi" başlığı ve `/customers/new` adresine yönlendiren "Yeni Müşteri" butonu bulunacak.
*   **Filtreleme (`v-expansion-panels`):**
    *   **Arama:** Ad, soyad, firma adı veya e-postaya göre arama yapabilen bir `v-text-field`.
    *   **Durum:** Aktif/pasif müşterileri göstermek için bir `v-switch`.
    *   **Sıralama:** Ada, firmaya ve oluşturulma tarihine göre sıralama seçenekleri sunan bir `v-select`.
*   **Veri Tablosu (`v-data-table`):**
    *   **`headers`:** Sütunlar şu şekilde olacak: `Ad Soyad`, `Firma Adı`, `E-posta`, `Telefon`, `Bakiye`, `Durum` ve `Eylemler`.
    *   **`v-slot:item.isActive`:** `Product` modülündeki gibi, `v-chip` kullanılarak "Aktif" (yeşil) ve "Pasif" (kırmızı) etiketleri gösterilecek.
    *   **`v-slot:item.actions`:** Müşterinin `isActive` durumuna göre farklı eylem butonları (`v-btn` ve `v-menu`) gösterilecek:
        *   **Aktif Müşteri İçin:** Düzenle (`mdi-pencil`), Arşivle (`mdi-archive-arrow-down`).
        *   **Pasif Müşteri İçin:** Geri Yükle (`mdi-restore`). (Müşteriler için kalıcı silme işlemi olmayacak, bu daha güvenli bir yaklaşımdır).
*   **Onay Penceresi (`v-dialog`):** Arşivleme ve geri yükleme işlemleri için `ProductsView.vue`'deki `openDialog` ve `v-dialog` mantığı aynen kopyalanacak.

---

### **4. Adım: Müşteri Form Sayfası (`CustomerFormView.vue`)**

Bu sayfa, `ProductFormView.vue`'nin yeniden kullanılabilir ve koşullu yapısını temel alacaktır.

*   **Modlar:** URL'de `:id` parametresinin varlığına göre "Yeni Müşteri Ekle" veya "Müşteri Düzenle" modunda çalışacak.
*   **Form Yapısı (`v-form`):**
    *   `v-text-field`: Ad, Soyad, Firma Adı, Vergi No, Adres, Telefon, E-posta.
    *   `v-text-field type="number"`: Bakiye.
    *   `v-switch`: `isActive` durumu (sadece düzenleme modunda görünecek).
*   **Validasyon:** Frontend tarafında temel zorunluluk (`v => !!v`) ve format (`email`, `telefon`) kontrolleri eklenecek.
*   **Kaydetme Mantığı (`saveProduct`):** `isEditMode` durumuna göre `CustomerService.create` veya `CustomerService.update` metotlarını çağıracak.

---

### **5. Adım: Yönlendirme (Routing)**

`src/router/index.ts` dosyasına aşağıdaki yeni yollar (route) eklenecek:

*   `{ path: '/customers', name: 'Müşteriler', component: CustomersView }`
*   `{ path: '/customers/new', name: 'Yeni Müşteri', component: CustomerFormView }`
*   `{ path: '/customers/edit/:id', name: 'Müşteri Düzenle', component: CustomerFormView, props: true }`

Bu plan, yarınki geliştirmeyi son derece verimli ve tutarlı bir şekilde yapmamızı sağlayacaktır.
