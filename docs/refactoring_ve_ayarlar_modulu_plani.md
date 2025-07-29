# Refactoring ve Ayarlar Modülü Geliştirme Planı

Bu belge, projeye yeni bir "Ayarlar" modülü eklemek, mevcut modülleri bu yeni yapıya entegre etmek ve proje genelinde çeşitli iyileştirmeler yapmak için izlenecek yol haritasını tanımlar.

---

### **Genel Hedefler**

1.  **Merkezi Ayarlar Yönetimi:** Para birimi ve müşteri firmaları gibi sistem genelindeki verileri yönetmek için yeni bir "Ayarlar" modülü oluşturmak.
2.  **Veri Bütünlüğünü Artırma:** Müşterileri, metin tabanlı firma isimleri yerine, önceden tanımlanmış firma kayıtlarına bağlamak.
3.  **Esneklik:** Sisteme yeni para birimleri ekleyebilme ve fiyatlandırmayı bu para birimlerine bağlama yeteneği kazandırmak.
4.  **Kullanıcı Deneyimini İyileştirme:**
    *   Tüm CRUD işlemleri sonucunda kullanıcıya anlık geri bildirim (toast notification) sağlamak.
    *   Yeni ürün eklerken, sistem tarafından önerilen benzersiz bir SKU'yu arayüzde göstermek.
5.  **Veri Kaybını Önleme:** Mevcut veritabanındaki verileri, yeni şema yapısına **veri kaybı olmadan** güvenli bir şekilde taşımak (Data Migration).

---

### **Aşama 1: Backend Altyapısını Hazırlama ve Veri Taşıma**

**1.1. Domain Katmanını Genişletme (`Core.Domain`)**
*   **Yeni Entity'ler:** `✅ Tamamlandı`
    *   `Currency.cs`: Para birimlerini yönetmek için (`Id`, `Name`, `Code`, `Symbol`, `IsActive`).
    *   `Company.cs`: Müşteri firmalarını yönetmek için (`Id`, `Name`, `TaxNumber`, `Address`, `IsActive`).
*   **Mevcut Entity'leri Güncelleme:** `✅ Tamamlandı`
    *   `Customer.cs`: `string CompanyName` alanı kaldırıldı, yerine `Guid CompanyId` ve `Company Company` navigasyon özelliği eklendi.
    *   `Product.cs` ve `Teklif.cs`: Fiyatlandırma için para birimi bağlamı eklemek amacıyla `int CurrencyId` ve `Currency Currency` navigasyon özelliği eklendi.

**1.2. Altyapı ve Veritabanı (`Infrastructure`)**
*   **DbContext Güncellemesi:** `ApplicationDbContext`'e yeni `DbSet<Currency>` ve `DbSet<Company>` eklendi. `✅ Tamamlandı`
*   **Yeni Repository'ler:** `ICurrencyRepository`, `ICompanyRepository` ve implementasyonları oluşturuldu. `✅ Tamamlandı`
*   **Mimari Tutarlılık Refactoring'i:** Projedeki tüm `Delete` metotları, `soft delete` (`IsActive=false`) mantığını içerecek şekilde standartlaştırıldı. Bu mantık Repository katmanına taşındı ve `Application` katmanındaki Command Handler'lar temizlendi. `✅ Tamamlandı`
*   **Veritabanı Migration'ı ve Veri Taşıma:**
    1.  `dotnet ef migrations add AddSettingsModuleAndRefactor` komutu ile yeni bir migration dosyası oluşturulacak.
    2.  Bu dosya **henüz veritabanına uygulanmadan önce** manuel olarak düzenlenecek:
        *   **Para Birimi Taşıma:**
            *   `migrationBuilder.InsertData()` kullanılarak `Currencies` tablosuna varsayılan para birimleri (TRY, USD, EUR) eklenecek (seeding).
            *   `Products` ve `Teklifler` tablolarına `CurrencyId` kolonu eklendikten sonra, tüm mevcut kayıtların bu kolonu varsayılan olarak `1` (TRY) olacak şekilde güncellenecek.
        *   **Firma Verisi Taşıma:**
            *   `migrationBuilder.Sql()` kullanılarak, `Customers` tablosundaki benzersiz `CompanyName` değerleri okunup yeni `Companies` tablosuna `INSERT` edilecek.
            *   İkinci bir `migrationBuilder.Sql()` ile, her müşterinin yeni `CompanyId` alanı, eski `CompanyName` metnine karşılık gelen ID ile güncellenecek.
            *   Son olarak, eski `CompanyName` kolonu `Customers` tablosundan düşürülecek.
    3.  Tüm bu adımlardan sonra `dotnet ef database update` komutu çalıştırılarak şema ve veri geçişi tek seferde güvenle uygulanacak.

**1.3. Uygulama ve API Katmanları (`Application`, `API.Web`)**
*   **Yeni "Ayarlar" Modülü API'si:**
    *   `SettingsController.cs` adında yeni bir controller oluşturulacak.
    *   Bu controller içinde para birimleri ve firmalar için tam CRUD (Oluştur, Oku, Güncelle, Sil) endpoint'leri hazırlanacak.
*   **SKU Üretim Mekanizmasını Güncelleme:**
    *   `ProductsController.cs`'e, frontend'in yeni bir ürün oluşturmadan önce çağıracağı bir `GET /api/products/next-sku` endpoint'i eklenecek.
    *   `GetNextSkuQuery` ve işleyicisi oluşturularak benzersiz SKU üretme mantığı buraya taşınacak.
    *   `CreateProductCommand` güncellenecek: Artık SKU'yu kendisi üretmeyecek, frontend'den gelen hazır SKU'yu alacak. `CreateProductCommandValidator` ise bu gelen SKU'nun benzersizliğini kontrol edecek.

---

### **Aşama 2: Frontend Geliştirmeleri**

**2.1. Global Bildirim (Toast) Sistemini Kurma**
*   **Kütüphane Kurulumu:** `npm install vue-toastify@next` komutu ile kütüphane projeye eklenecek.
*   **Global Konfigürasyon:** `main.ts` içinde `vue-toastify`'ı ve CSS dosyalarını içe aktararak tüm uygulama için kullanılabilir hale getirilecek.
*   **Wrapper Servisi Oluşturma:** `NotificationService.ts` adında bir servis oluşturulacak. Bu servis, `vue-toastify`'ı sarmalayarak `NotificationService.success('İşlem başarılı!')` gibi basit ve merkezi bir kullanım sunacak.

**2.2. Yeni "Ayarlar" Arayüzünü Oluşturma**
*   `SettingsView.vue` adında ana bir ayarlar sayfası oluşturulacak.
*   Bu sayfa içinde, para birimlerini ve firmaları yönetmek için iki ayrı component (`CurrencySettings.vue`, `CompanySettings.vue`) kullanılacak. Bu component'ler, `v-data-table` ile listeleme ve diyalog pencereleri ile ekleme/düzenleme işlevlerini barındıracak.

**2.3. Mevcut Modülleri Yeni Sisteme Entegre Etme**
*   **Müşteri Modülü (`CustomerFormView.vue`):**
    *   Firma adı için kullanılan metin kutusu (`v-text-field`), `SettingsController`'dan firmaları çeken bir `v-autocomplete` ile değiştirilecek.
*   **Ürün ve Teklif Modülleri (`ProductFormView.vue`, `QuoteFormView.vue`):**
    *   Fiyat alanlarının yanına, `SettingsController`'dan para birimlerini çeken bir `v-select` eklenecek.
*   **SKU Entegrasyonu (`ProductFormView.vue`):**
    *   "Yeni Ürün Ekle" modunda, sayfa yüklendiğinde `productService.getNextSku()` çağrılacak.
    *   Dönen SKU, `readonly` (sadece okunabilir) bir `v-text-field` içinde gösterilecek.
*   **Toast Bildirimlerini Ekleme:**
    *   Tüm modüllerdeki CRUD işlemlerinin (`archive`, `restore`, `update` vb.) `try...catch` bloklarına, işlem sonucuna göre `NotificationService.success(...)` veya `NotificationService.error(...)` çağrıları eklenecek.

---

### **Geliştirme Günlüğü ve Mevcut Durum**

**Tarih:** 29 Temmuz 2025

**Tamamlanan Adımlar:**
*   `✅` **Refactoring:** Projedeki tüm `Delete` metotları, `soft delete` (`IsActive=false`) mantığını içerecek şekilde standartlaştırıldı. Bu mantık Repository katmanına taşındı ve `Application` katmanındaki Command Handler'lar temizlendi.
*   `✅` **Domain Katmanı:** `Company` ve `Currency` entity'leri oluşturuldu. `Customer`, `Product` ve `Teklif` entity'leri yeni ilişkilere (`CompanyId`, `CurrencyId`) göre güncellendi.
*   `✅` **Altyapı Katmanı:** `ICompanyRepository`, `ICurrencyRepository` ve implementasyonları oluşturuldu. `IUnitOfWork` ve `UnitOfWork.cs` yeni repository'leri içerecek şekilde güncellendi. `ApplicationDbContext`'e yeni `DbSet`'ler ve ilişkiler eklendi.
*   `✅` **Derleme Hataları:** Entity'lerde yapılan değişiklikler sonrası `Application` ve `Infrastructure` katmanlarında ortaya çıkan tüm derleme hataları giderildi.
*   `✅` **Migration Sorun Giderme:** Migration oluşturma sırasında karşılaşılan `UNIQUE INDEX` sorunu, `ApplicationDbContext`'te `Email` alanına `HasMaxLength(450)` kuralı eklenerek ve yapılandırma sırası düzeltilerek çözüldü.

**KALINAN YER VE BİR SONRAKİ ADIM:**

**Mevcut Durum:** En son migration (`AddSettingsModuleAndRefactor`) kaldırıldı ve `DbContext`'teki `Email` index sorunu düzeltildi. Backend projesi şu anda temiz bir şekilde derleniyor.

**➡️ Bir Sonraki Adım:** **Yeni ve düzeltilmiş migration'ı oluşturmak.** Bunun için aşağıdaki komut çalıştırılacak:
`dotnet ef migrations add AddSettingsModuleAndRefactor --startup-project ../API.Web`
Bu komut çalıştırıldıktan sonra, oluşturulan migration dosyası, planın **1.2** adımında belirtilen **veri taşıma SQL betikleri** ile manuel olarak düzenlenecek.
