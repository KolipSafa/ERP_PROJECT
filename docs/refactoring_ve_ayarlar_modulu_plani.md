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
*   `✅` **Refactoring ve Altyapı:** Proje genelindeki `Delete` metotları standartlaştırıldı, `Domain` ve `Infrastructure` katmanları yeni `Company` ve `Currency` entity'lerini ve repository'lerini içerecek şekilde tamamen güncellendi.
*   `✅` **Proje Geneli Sağlamlaştırma:** Geliştirme sırasında `Company` modülündeki `IUnitOfWork` yanlış kullanımlarından kaynaklanan tüm derleme hataları giderildi. `TeklifMappings` ve `CustomerRepository` dosyalarındaki potansiyel `null` referans uyarıları temizlendi.
*   `✅` **Hibrit Para Birimi Yaklaşımı:**
    *   **Veritabanı Seeding:** Temel para birimlerini (TRY, USD, EUR) veritabanına otomatik olarak eklemek için `ApplicationDbContext`'e seeding verisi eklendi.
    *   `SeedCurrencies` adında yeni bir migration oluşturuldu ve veritabanına başarıyla uygulandı.
*   `✅` **Ayarlar Modülü API (`Currency`):**
    *   `SettingsController` oluşturuldu.
    *   `GetCurrenciesQuery` implemente edilerek para birimlerinin listelenmesi sağlandı.
    *   `CreateCurrencyCommand` ve `Validator`'ı implemente edilerek yeni para birimi ekleme özelliği tamamlandı.
    *   Controller, bu yeni özellikleri sunacak şekilde güncellendi.
*   `✅` **Mimari Tutarlılık:** `Application` katmanının `Infrastructure` katmanına yanlışlıkla referans vermesine neden olan bir `using` ifadesi `CreateCurrencyCommandValidator`'dan kaldırılarak mimari bütünlük korundu.

**KALINAN YER VE BİR SONRAKİ ADIM:**

**Mevcut Durum:** Backend projesi hatasız ve uyarısız bir şekilde derleniyor. Ayarlar modülünün para birimi yönetimi için listeleme (`GET`) ve oluşturma (`POST`) işlevleri tamamlandı.

**➡️ Bir Sonraki Adım:** **Ayarlar modülüne devam etmek.** Para birimleri için `Update` ve `Delete` işlemlerini implemente etmek veya Firmaları (`Company`) yönetmek için CRUD operasyonlarını geliştirmeye başlamak mantıklı bir sonraki adım olacaktır.
