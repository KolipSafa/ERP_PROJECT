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

#### **Aşama 3: Teklif Modülü** `✅ Tamamlandı`

Bu aşamada, müşterilere ürünler içeren tekliflerin oluşturulacağı ve yönetileceği modül, hem backend hem de frontend olarak uçtan uca tamamlanmıştır.

**3.1. Backend Geliştirme (.NET API)** `✅ Tamamlandı`
*   **Entity'ler ve İlişkiler:** `Teklif`, `TeklifSatiri` ve `QuoteStatus` (Türkçe karakter desteğiyle) `Core.Domain` katmanında oluşturuldu.
*   **Repository ve Unit of Work:** `ITeklifRepository` ve implementasyonu, `IUnitOfWork` çatısı altında oluşturuldu.
*   **CQRS & DTO:** Teklif ve teklif satırı işlemleri için `Query`, `Command` ve DTO'lar oluşturuldu.
*   **Akıllı Güncelleme Mimarisi:** Frontend'in işini basitleştirmek için, `UpdateTeklifCommand` komutu, bir teklifin tüm satırlarını (yeni, güncellenmiş, silinmiş) tek bir istekte akıllıca işleyecek şekilde yeniden tasarlandı. Bu, frontend'deki karmaşık değişiklik izleme mantığını ortadan kaldırır.
*   **Controller:** `TekliflerController`'a, ana CRUD işlemlerinin yanı sıra, teklif satırlarını yönetmek için gereken tüm endpoint'ler eklendi ve daha sonra bu yapı basitleştirilerek tek bir `PUT` endpoint'ine dönüştürüldü.
*   **Validasyon:** Teklif ve satırları için `FluentValidation` kuralları yazıldı. Özellikle tarihler arasındaki tutarlılık (`GreaterThan`) kontrolü eklendi.
*   **Veritabanı:** Tüm bu değişiklikleri içeren `AddTeklifModule` adında bir Entity Framework migration'ı oluşturuldu.

**3.2. Frontend Geliştirme (Vue.js)** `✅ Tamamlandı`
*   **API Servisi:** `TeklifService.ts` ve bu servisin kullandığı tüm DTO ve Payload tiplerini içeren `dtos/TeklifDtos.ts` dosyaları oluşturuldu. Servis, backend'in yeni "akıllı güncelleme" mimarisine uyumlu hale getirildi.
*   **Listeleme Sayfası (`QuotesView.vue`):**
    *   `v-data-table` ile teklifler, formatlanmış tarih ve para birimiyle listelendi.
    *   Teklif durumları, anlama yardımcı olan renkli `v-chip`'lerle gösterildi.
    *   Her satır için, teklif durumunu doğrudan listeden değiştirmeye olanak tanıyan bir "Eylemler" menüsü eklendi.
*   **Form Sayfası (`QuoteFormView.vue`):**
    *   Hem "Yeni Teklif" hem de "Teklif Düzenle" modlarında çalışan, tam fonksiyonel bir form oluşturuldu.
    *   **Müşteri ve Ürün Seçimi:** `v-autocomplete` bileşenleri, backend servislerini kullanarak dinamik arama ve başlangıç listesi gösterme özellikleriyle donatıldı.
    *   **Dinamik Satır Yönetimi:** Kullanıcıların teklife kolayca ürün ekleyip çıkarabildiği, miktarları ve fiyatları anında düzenleyebildiği bir `v-data-table` entegre edildi.
    *   **Anlık Hesaplama:** Teklifin toplam tutarı, satırlardaki herhangi bir değişiklikte anında yeniden hesaplanacak şekilde `computed` bir property ile yönetildi.
    *   **Durum Yönetimi:** Düzenleme modunda, teklifin mevcut durumunu (`Hazırlanıyor`, `Sunuldu` vb.) değiştirmek için bir `v-select` bileşeni eklendi.
*   **Hata Ayıklama ve Sağlamlaştırma:**
    *   Backend'in `400 Bad Request` hatası vermesine neden olan tarih doğrulama (`GreaterThan`) sorunu, frontend'de geçerlilik tarihinin gün sonuna ayarlanmasıyla çözüldü.
    *   Geliştirme sırasında karşılaşılan çok sayıda TypeScript tip hatası giderilerek kodun güvenilirliği artırıldı.

---

#### **Aşama 4: Ayarlar Modülü** `✅ Tamamlandı`

Bu aşamada, para birimi ve firmalar gibi sistem genelindeki verilerin yönetileceği altyapı hem backend hem de frontend olarak tamamlanmıştır. Mevcut modüller bu yeni yapıya entegre edilmiştir.

**4.1. Backend Geliştirme (.NET API)** `✅ Tamamlandı`
*   **Entity ve İlişkiler:** `Currency` ve `Company` entity'leri, ilgili koleksiyonlarla (`ICollection`) zenginleştirildi. `Customer`, `Product`, `Teklif` entity'leri bu yeni yapıları kullanacak şekilde (`CurrencyId`, `CompanyId`) güncellendi.
*   **Veritabanı Yapılandırması:** Entity'ler arasındaki tüm ilişkiler (`Customer-Company`, `Product-Currency`, `Teklif-Currency` vb.) `ApplicationDbContext` içinde **Fluent API** kullanılarak açıkça ve hatasız bir şekilde tanımlandı. `shadow property` oluşumuna neden olan tüm belirsizlikler giderildi.
*   **Veritabanı Sıfırlama:** Projenin tutarlı bir duruma gelmesi için mevcut migration'lar temizlendi, veritabanı silindi ve en son, doğru modele göre sıfırdan yeniden oluşturuldu.
*   **Repository ve Unit of Work:** `ICurrencyRepository`, `ICompanyRepository` ve implementasyonları oluşturuldu. `GetAllAsync` metotları, `soft delete` mantığına uygun olarak sadece aktif kayıtları getirecek şekilde güncellendi.
*   **CQRS (Firma ve Para Birimi):** Hem `Company` hem de `Currency` için tam **CRUD** (Create, Read, Update, Delete) operasyonlarını yöneten `Query`, `Command`, `Handler` ve `Validator`'lar implemente edildi.
*   **API Controller:** `SettingsController`, hem para birimleri hem de firmalar için tam CRUD işlevselliği sunan `GET`, `POST`, `PUT`, `DELETE` endpoint'lerini içerecek şekilde tamamlandı.
*   **DI ve Hata Ayıklama:** `Program.cs` dosyasında eksik olan repository bağımlılıkları eklendi. Proje genelindeki `null` referans uyarıları ve hataları giderildi.

**4.2. Frontend Geliştirme (Vue.js)** `✅ Tamamlandı`
*   **Bildirim Sistemi:** Eski `NotificationService` kaldırılarak yerine modern, `composable` tabanlı `useNotifier` sistemi kuruldu. `vue-toastify` kütüphanesi entegre edildi ve tüm CRUD işlemlerinde kullanıcıya geri bildirim (başarı/hata) verecek şekilde kullanıldı.
*   **Ayarlar Arayüzü (`SettingsView.vue`):**
    *   Sayfa, solda dar bir navigasyon menüsü ve sağda geniş bir içerik alanı olacak şekilde modern bir "master-detail" görünümüne kavuşturuldu.
    *   `CompanySettings.vue` ve `CurrencySettings.vue` adında, kendi içlerinde tam CRUD işlevselliği barındıran iki bileşen oluşturuldu.
*   **API Servisi (`SettingsService.ts`):** Ayarlar modülünün tüm backend iletişimini yönetmek için `Company` ve `Currency` DTO'ları ile birlikte yeni bir servis oluşturuldu.
*   **Entegrasyon:**
    *   `CustomerFormView.vue`: Firma seçimi için metin kutusu, `v-autocomplete` ile değiştirilerek Ayarlar modülüne bağlandı.
    *   `ProductFormView.vue`: Fiyat alanının yanına para birimi seçimi için `v-select` eklendi.
    *   `QuoteFormView.vue`: Teklifin para birimini belirlemek için `v-select` eklendi ve toplam tutar formatlaması dinamik hale getirildi.
*   **Tasarım ve UX İyileştirmeleri:**
    *   Kullanıcı geri bildirimleri doğrultusunda Ayarlar sayfasının yerleşimi optimize edildi.
    *   Toast bildirimlerinin (opaklık, boyut, ikon) ve silme onayı diyaloglarının tasarımları projenin geneliyle tutarlı hale getirildi.
*   **Hata Ayıklama:** Oturum boyunca karşılaşılan çok sayıda frontend hatası (hatalı import yolları, tanımsız değişkenler, tekrar eden kodlar) giderildi.