# Plan 2: Gelişmiş ERP İşlevleri Entegrasyon Planı

**Amaç:** Projeyi, temel CRUD operasyonlarının ötesine taşıyarak, envanter yönetimi, müşteri etkileşimi ve faturalandırma gibi temel ERP iş akışlarını destekleyen bir platform haline getirmek.

---

### **Genel Strateji**

Müşteriyi iş akışının merkezine alan, teklif sürecini dinamik hale getiren ve envanter takibini otomatikleştiren bir yapı kurulacaktır. Her adım, hem backend iş mantığı hem de frontend kullanıcı arayüzü geliştirmelerini içerecektir. Rol tabanlı yetkilendirme (Admin/Customer) bu fazın temelini oluşturacaktır.

---

### **Faz 1: Gelişmiş Envanter Yönetimi** `📝 Planlandı`

*   **Adım 1.1: Veritabanı ve Domain Modelini Genişletme**
    1.  **Entity Güncellemesi (`Core.Domain/Entities/Product.cs`):**
        *   `Product` sınıfına, onay bekleyen tekliflerdeki ürün miktarını tutmak için `ReservedQuantity` (Rezerve Miktar) adında bir `int` property ekle. Varsayılan değeri `0` olmalı.
    2.  **DTO Güncellemesi (`Application/DTOs/ProductDto.cs`):**
        *   `ProductDto` sınıfına `ReservedQuantity` property'sini ekle.
        *   Anlık olarak teklif edilebilir stoğu göstermek için, veritabanına yansıtılmayacak (`[NotMapped]`) bir `AvailableQuantity` (Kullanılabilir Miktar) property'si ekle. Bu property, `StockQuantity - ReservedQuantity` değerini döndürecek.
    3.  **Veritabanı Migration'ı:**
        *   `Infrastructure` projesi dizinindeyken terminalde yeni migration oluştur: `dotnet ef migrations add AddReservedQuantityToProducts`
        *   Migration'ı veritabanına uygula: `dotnet ef database update`

*   **Adım 1.2: Backend İş Mantığını Uygulama**
    *   **`CreateTeklifCommand`:** Teklif oluşturulduğunda, teklifteki her bir ürünün `ReservedQuantity` değeri, satırdaki `Miktar` kadar **artırılacak**.
    *   **`UpdateTeklifCommand`:** Teklif güncellendiğinde, önce teklifin eski satırlarındaki ürünlerin `ReservedQuantity` değeri **azaltılacak**, sonra yeni satırlardaki ürünlerin `ReservedQuantity` değeri **artırılacak**. Bu, miktarın doğru yeniden hesaplanmasını sağlar.
    *   **`DeleteTeklifCommand` (Arşivleme):** Teklif arşivelendiğinde, içindeki ürünlerin `ReservedQuantity` değeri **azaltılacak**.
    *   **Durum Değişikliği:** Bir teklifin durumu `Onaylandı` veya `Reddedildi` olarak güncellendiğinde, içindeki ürünlerin `ReservedQuantity` değeri **azaltılacak**.

*   **Adım 1.3: Frontend Arayüzünü Geliştirme**
    *   **`services/ProductService.ts`:** `ProductDto` interface'ini `reservedQuantity` ve `availableQuantity` alanlarını içerecek şekilde güncelle.
    *   **`views/QuoteFormView.vue`:**
        *   Ürün arama ve ekleme bölümünde, seçilen ürünün `availableQuantity` değeri bir `v-chip` ile gösterilecek.
        *   Eğer `availableQuantity <= 0` ise, `v-chip` rengi `error` (kırmızı) olacak ve kullanıcıya stoktan fazla teklif verildiği uyarısı yapılacak.

---

### **Faz 2: Müşteri Etkileşimi ve Fatura Akışı**

*   **Adım 2.1: Veritabanı ve Domain Modelini Genişletme** `📝 Planlandı`
    1.  **Yeni Entity'ler (`Core.Domain/Entities`):**
        *   `Invoice.cs`: `Id`, `InvoiceNumber`, `CustomerId`, `QuoteId`, `InvoiceDate`, `DueDate`, `TotalAmount`, `Status` (`InvoiceStatus` enum) gibi alanları içerecek.
        *   `InvoiceLine.cs`: `Id`, `InvoiceId`, `ProductId`, `Description`, `Quantity`, `UnitPrice`, `Total` alanlarını içerecek.
    2.  **Yeni Enum'lar (`Core.Domain/Enums`):**
        *   `InvoiceStatus.cs`: `Draft`, `Sent`, `Paid`, `Overdue` gibi durumları içerecek.
        *   `QuoteStatus.cs`: Mevcut durumlara `ChangeRequested` (Değişiklik Talep Edildi) durumu eklenecek.
    3.  **Veritabanı Migration'ı:**
        *   `Infrastructure` projesi dizinindeyken yeni migration oluştur: `dotnet ef migrations add AddInvoicingAndQuoteNegotiation`
        *   Migration'ı uygula: `dotnet ef database update`

*   **Adım 2.2: Backend İş Mantığını Uygulama** `📝 Planlandı`
    1.  **Yeni CQRS Komutları (`Application/Features`):**
        *   **`ApproveQuoteAndCreateInvoiceCommand`:**
            *   Bu komut, müşterinin ID'sini ve teklif ID'sini alacak.
            *   Tek bir transaction içinde:
                1.  Teklifin durumunu `Onaylandı` yapacak.
                2.  Teklifteki ürünlerin `ReservedQuantity` ve `StockQuantity` değerlerini düşecek.
                3.  Teklif bilgilerinden yeni bir `Invoice` ve `InvoiceLine` oluşturacak.
        *   **`RequestQuoteChangeCommand`:**
            *   Müşterinin ID'sini, teklif ID'sini ve güncellenmiş satır bilgilerini alacak.
            *   Teklifin satırlarını güncelleyecek ve durumunu `ChangeRequested` yapacak. Envanterde değişiklik **yapmayacak**.
    2.  **Yeni API Endpoint'leri (`API.Web/Controllers`):**
        *   `TekliflerController`'a bu yeni komutları tetikleyecek `[HttpPost("{id}/approve")]` ve `[HttpPost("{id}/request-change")]` gibi yeni endpoint'ler eklenecek. Bu endpoint'ler, sadece "Customer" rolüne sahip kullanıcılar tarafından erişilebilir olmalı (`[Authorize(Roles = "Customer")]`).

*   **Adım 2.3: Frontend Müşteri Panelini Geliştirme** `📝 Planlandı`
    *   **Yetki:** Bu paneldeki tüm sayfalar ve görünümler, sadece `Customer` rolüne sahip kullanıcılar tarafından erişilebilir olmalıdır.
    1.  **`views/customer/MyQuotesView.vue`:**
        *   Durumu "Sunuldu" olan teklifler için "Onayla" ve "Değişiklik Talep Et" butonları eklenecek.
        *   "Onayla" butonu, `/api/teklifler/{id}/approve` endpoint'ini çağıracak.
        *   "Değişiklik Talep Et" butonu, kullanıcıyı yeni oluşturulacak `RequestQuoteChangeView` sayfasına yönlendirecek.
    2.  **Yeni Sayfa: `views/customer/RequestQuoteChangeView.vue`:**
        *   `QuoteFormView`'un basitleştirilmiş bir kopyası olacak. Müşteri sadece miktar ve birim fiyatı düzenleyebilecek.
        *   "Talebi Gönder" butonu, `/api/teklifler/{id}/request-change` endpoint'ini çağıracak.
    3.  **Yeni Sayfa: `views/customer/MyInvoicesView.vue`:**
        *   Müşterinin onayladığı tekliflerden oluşan faturaları listeleyecek.

*   **Adım 2.4: Frontend Admin Panelini Güncelleme** `📝 Planlandı`
    *   **Yetki:** Bu paneldeki tüm sayfalar ve görünümler, sadece `Admin` rolüne sahip kullanıcılar tarafından erişilebilir olmalıdır.
    1.  **`views/QuotesView.vue`:**
        *   Durumu `ChangeRequested` olan teklifler, adminin dikkatini çekmesi için farklı bir renkte (`v-chip color="orange"`) gösterilecek.
        *   Admin, bu teklifi düzenleme ekranında müşterinin taleplerini görüp, teklifi yeniden düzenleyip durumunu tekrar "Sunuldu"ya çekebilecek.
    2.  **Yeni Modül: `views/InvoicesView.vue`:**
        *   Sistemdeki tüm faturaların listelendiği, yönetildiği ve görüntülendiği yeni bir sayfa oluşturulacak.

*   **Adım 2.5: Güvenli Müşteri Davet ve Kayıt Akışının Uygulanması** `🎯 Güncel Hedef`
    *   **Genel Strateji:** Müşteri davet ve kayıt süreci, Supabase Edge Function'a eklenecek özel bir meta veri (`status: 'invited'`) ile yönetilecektir. Bu, davetle gelen kullanıcıların durumunu net bir şekilde belirleyerek, onları şifre belirleme ve hesap aktivasyon akışına doğru bir şekilde yönlendirmemizi sağlayacaktır.
    *   **Akış Adımları:**
        1.  **Yönetici Davet Eder (Frontend):** `✅ Tamamlandı`
            *   Yönetici, müşteri bilgilerini forma girer ve "Davet Gönder" butonuna tıklar.
        2.  **Davet İşlenir (Supabase Edge Function):** `✅ Tamamlandı`
            *   Fonksiyon, isteği yapanın "Admin" rolünü doğrular.
            *   Supabase'e davet gönderirken, kullanıcının `app_metadata`'sına `{ "roles": ["Customer"], "status": "invited" }` verisini ekler.
        3.  **Müşteri Daveti Kabul Eder (E-posta -> Frontend):** `✅ Tamamlandı`
            *   Müşteri, e-postasındaki linke tıklar ve `/auth/callback` yoluna yönlendirilir.
        4.  **Davet Durumu Kontrolü (Frontend):** `✅ Tamamlandı`
            *   `auth.store.ts` ve `router/index.ts`, `status`'u `'invited'` olan kullanıcıyı `/set-password` sayfasına zorunlu olarak yönlendirir.
        5.  **Müşteri Şifre Belirler (Frontend - `SetPasswordView.vue`):** `✅ Tamamlandı`
            *   Müşteri, bu korumalı sayfada yeni şifresini oluşturur.
        6.  **Hesap Aktivasyonu ve Veritabanı Kaydı (Backend):** `✅ Tamamlandı`
            *   "Kaydet" butonuna tıklandığında, frontend önce Supabase'e şifreyi güncelletir.
            *   Ardından, backend API'sine **tek bir istek** atarak `CreateCustomerCommand`'ı çalıştırır.
            *   `CreateCustomerCommandHandler`, **tek bir atomik işlem içinde** hem müşteriyi ERP veritabanına kaydeder hem de **sunucu tarafında güvenli bir şekilde** Supabase kullanıcısının `app_metadata`'sındaki `status` alanını `'active'` olarak günceller.
        7.  **Panele Yönlendirme (Frontend):** `❗ Yapılacak`
            *   Backend'den başarılı yanıt alındıktan sonra, `SetPasswordView.vue` bileşeni, kullanıcıyı müşteri paneline (`/my-quotes`) yönlendirecektir. **(Mevcut hata burada yaşanmaktadır.)**

---
### **Güncel Durum Notu (05.08.2025)**

**Kaldığımız Yer:** Müşteri davet ve aktivasyon akışının backend tarafındaki tüm adımları tamamlandı ve sağlamlaştırıldı. `supabase-csharp` kütüphanesinden kaynaklanan tüm sorunlar, doğrudan `HttpClient` kullanan bir servis yazılarak aşıldı. Müşteri kaydı ve Supabase'deki `status` güncellemesi artık tek bir güvenli backend işlemiyle hallediliyor.

**Sıradaki Adım:** Akışın son adımı olan, `SetPasswordView.vue` sayfasının, başarılı bir aktivasyon sonrası kullanıcıyı müşteri paneline doğru bir şekilde **yönlendirmesi** sorununu çözmek.