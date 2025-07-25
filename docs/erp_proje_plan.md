# **ERP Projesi Geliştirme Planı**

Bu belge, Envanter, Müşteri Cari ve Teklif modüllerini içeren ERP projesinin geliştirme yol haritasını ve mimari prensiplerini tanımlar.

---

### **1. Mimari ve Teknolojik Prensipler**

*   **Backend:** .NET 9, Clean Architecture, CQRS (MediatR), Entity Framework Core, FluentValidation, AutoMapper.
*   **Veri Erişimi:** **Unit of Work Deseni**, Repository'ler aracılığıyla veri bütünlüğünü (transactional operations) ve merkezi veri erişimini garanti eder.
*   **Frontend:** Vue.js 3 (Vite), TypeScript, Vuetify, Pinia, Vue Router, Axios.
*   **Genel İlkeler:** Modüler, temiz kod, güvenlik odaklı, DTO tabanlı iletişim, tam kapsamlı validasyon.

---

### **2. Proje Aşamaları**

#### **Aşama 1: Envanter (Ürün) Modülü** `✅ Tamamlandı ve Sağlamlaştırıldı`

Bu aşamada, ürün yönetimiyle ilgili tüm temel backend ve frontend işlevleri tamamlanmıştır. Ayrıca, Müşteri modülü geliştirilirken tespit edilen mimari eksiklikler ve güvenlik açıkları bu modüle de geri uygulanarak proje genelinde tutarlılık sağlanmıştır.

**2.1. Backend Geliştirmeleri (.NET API)** `✅ Tamamlandı`
*   **Mimari:** Clean Architecture prensiplerine uygun proje yapısı kuruldu.
*   **Veritabanı:** `Product` entity'si ve `ApplicationDbContext` oluşturuldu.
*   **Repository ve Unit of Work Katmanı:** Veritabanı işlemleri, `IUnitOfWork` arayüzü arkasında soyutlanmıştır. Repository'ler (`IProductRepository` vb.) artık tekil kaydetme işlemleri yapmaz; bunun yerine tüm değişiklikler `UnitOfWork` tarafından tek bir transaction içinde yönetilir. Bu, veri tutarlılığını garanti altına alır.
*   **CQRS:** `MediatR` ile `Query` ve `Command`'ler oluşturuldu. `CommandHandler`'lar artık doğrudan Repository'leri değil, `IUnitOfWork`'ü kullanarak işlemleri yönetir.
*   **API Controller:** `ProductsController` ile aşağıdaki endpoint'ler oluşturuldu ve tamamen işlevsel hale getirildi:
    *   `GET /api/products`: Gelişmiş arama, filtreleme ve sıralama özellikleriyle.
    *   `GET /api/products/{id}`
    *   `POST /api/products`: Otomatik SKU oluşturma mantığıyla.
    *   `PATCH /api/products/{id}`: Sadece gönderilen alanları güncelleyen güvenli "Akıllı PATCH" mantığıyla.
    *   `DELETE /api/products/{id}`: Ürünü arşive gönderen "Soft Delete".
    *   `DELETE /api/products/hard/{id}`: Ürünü kalıcı olarak silen "Hard Delete".
*   **Validasyon:** `FluentValidation` ile tüm komutlar için sunucu taraflı, detaylı validasyon kuralları eklendi. Kod tekrarını önlemek için `ProductValidatorBase` oluşturuldu.
*   **DTO ve Mapping:** `ProductDto` kullanılarak API'nin dış dünyaya açtığı veri modeli güvenli hale getirildi. `AutoMapper` ile entity-DTO dönüşümleri otomatikleştirildi.
*   **Veri Bütünlüğü:** `IsActive` alanı ile "Soft Delete" mantığı ve SKU benzersizlik kontrolleri validasyon katmanına taşındı.

**2.2. Frontend Geliştirmeleri (Vue.js)** `✅ Tamamlandı`
*   **Proje Altyapısı:** Modern Vite altyapısı ile proje kuruldu. Vuetify, Vue Router, Axios entegre edildi.
*   **API Servisi:** `ProductService.ts` ile tüm backend iletişimi merkezileştirildi ve `PATCH` metodunu kullanacak şekilde güncellendi.
*   **Ana Yerleşim (Layout):** `App.vue` içinde, tüm uygulama için tutarlı bir navigasyon menüsü ve başlık çubuğu oluşturuldu.
*   **Ürün Listeleme Sayfası (`ProductsView.vue`):**
    *   `v-data-table` ile ürünler listelendi.
    *   Arama, fiyat filtreleme, durum filtreleme ve sıralama için kullanıcı dostu kontroller eklendi.
    *   **Yeni:** Ürün açıklamalarını göstermek için genişletilebilir satır (`show-expand`) özelliği eklendi.
*   **Ürün Form Sayfası (`ProductFormView.vue`):**
    *   Hem "Yeni Ürün Ekleme" hem de "Ürün Düzenleme" modlarında çalışabilen, yeniden kullanılabilir bir form bileşeni oluşturuldu.
    *   SKU alanı, "Yeni Ekleme" modunda gizlendi ve "Düzenleme" modunda sadece okunabilir hale getirildi.
*   **CRUD ve Arşivleme İşlevleri:**
    *   Yeni ürün ekleme (Açıklama alanı artık zorunlu).
    *   Mevcut ürünü düzenleme.
    *   Aktif bir ürünü arşivleme (Soft Delete).
    *   Pasif bir ürünü geri yükleme (Restore).
    *   Pasif bir ürünü kalıcı olarak silme (Hard Delete).
    *   Tüm tehlikeli işlemler için kullanıcı onayı alan diyalog pencereleri eklendi.
*   **Kod Kalitesi:** Proje genelinde isimlendirme tutarlılığı sağlandı ve tüm TypeScript hataları giderildi.

---

#### **Aşama 2: Müşteri Cari Modülü** `✅ Tamamlandı`

Bu aşamada, müşteri bilgilerinin yönetileceği modül geliştirilmiştir.

**2.1. Backend Geliştirme (.NET API)** `✅ Tamamlandı`
*   **Entity:** `Customer` adında yeni bir veritabanı varlığı (`Guid` Id ile) oluşturuldu.
*   **Repository:** `ICustomerRepository` ve `CustomerRepository`, `IUnitOfWork` çatısı altında, gelişmiş filtreleme yetenekleriyle oluşturuldu.
*   **CQRS & DTO:** Müşteri işlemleri için `Query`, `Command` ve `CustomerDto`'lar oluşturuldu.
*   **Controller:** `CustomersController` ile temel CRUD, arşivleme ve `PATCH` endpoint'leri eklendi.
*   **Validasyon:** Müşteri bilgileri için (`Email` benzersizlik kontrolü dahil) `FluentValidation` kuralları, `CustomerValidatorBase` kullanılarak yazıldı.

**2.2. Frontend Geliştirme (Vue.js)** `✅ Tamamlandı`
*   **API Servisi:** `CustomerService.ts` oluşturuldu.
*   **Listeleme Sayfası (`CustomersView.vue`):**
    *   Ürünler sayfasındakine benzer, `v-data-table` ile müşteri listesi oluşturuldu.
    *   Müşteri adı, firma adı veya e-postaya göre arama yapma ve sıralama işlevleri eklendi.
    *   Aktif/pasif müşterileri filtreleme seçeneği sunuldu.
    *   Detaylı bilgileri (adres, vergi no) göstermek için genişletilebilir satır özelliği eklendi.
*   **Form Sayfası (`CustomerFormView.vue`):**
    *   Yeni müşteri ekleme ve mevcut müşteriyi düzenleme için yeniden kullanılabilir bir form oluşturuldu.
*   **İşlevsellik:** Müşteri ekleme, düzenleme, arşivleme ve geri yükleme işlevleri, ürün modülündeki gibi onay pencereleriyle birlikte tamamlandı.
*   **Hata Ayıklama:** Geliştirme sırasında karşılaşılan yönlendirme ve filtreleme hataları giderildi.

---

#### **Aşama 3: Teklif Modülü** `❌ Başlanmadı`

Bu aşamada, müşterilere ürünler içeren tekliflerin oluşturulacağı modül geliştirilecektir. Bu, diğer iki modülle ilişkili olacağı için daha karmaşıktır.

... (Bu bölüm daha sonra detaylandırılacaktır) ...