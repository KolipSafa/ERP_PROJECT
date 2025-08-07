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

### **Faz 2: Müşteri Etkileşimi ve Fatura Akışı** `✅ Tamamlandı`

**Ana Hedef:** Yöneticinin oluşturduğu teklifin anında müşterinin panelinde görünmesini sağlamak ve müşteriye bu teklif üzerinde "Onayla", "Reddet" ve "Değişiklik Talep Et" aksiyonlarını sunmak. Onaylanan her teklif için otomatik olarak bir fatura oluşturulacaktır.

*   **Adım 2.1: Domain Katmanını Güncelleme (`Core.Domain`)** `✅`
*   **Adım 2.2: Veritabanı Geçişi (Migration)** `✅`
*   **Adım 2.3: İş Mantığını (CQRS) Uygulama (`Application`)** `✅`
*   **Adım 2.4: API Endpoint'lerini Oluşturma (`API.Web`)** `✅`
*   **Adım 2.5: Frontend Arayüzünü Geliştirme** `✅`

---

### **Güncel Durum Notu (07.08.2025)**

**Tamamlanan İş:** Müşteri davet ve aktivasyon akışı başarıyla tamamlandı. "Mimari Dönüşüm" aşaması bitti. Müşteri etkileşim ve fatura akışının ilk versiyonu tamamlandı. Müşteriler artık kendilerine atanan teklifleri görüp onay/red işlemleri yapabiliyor ve oluşan faturaları listeleyebiliyor.

**Sıradaki Adım:** Gelişmiş ERP İşlevleri'nin ikinci fazı olan **Faz 1 (Gelişmiş Envanter Yönetimi)**'ne başlanacak.

**Teknik Not:** Geliştirme sırasında .NET API'sindeki `[Authorize(Roles="...")]` attribute'larının beklenmedik şekilde `401 Unauthorized` hatalarına yol açtığı tespit edildi. Sorunun kök nedeni tam anlaşılamadığı için, geliştirmeyi yavaşlatmamak adına geçici bir çözüm uygulandı: Tüm Controller'lardaki rol tabanlı yetkilendirme etiketleri kaldırıldı. Bu, projenin şu anki halinde bir güvenlik açığı oluşturmaktadır. **Kullanıcının kararı doğrultusunda, bu konunun çözümü projedeki diğer tüm fonksiyonel geliştirmeler tamamlandıktan sonra, en son adım olarak ele alınacaktır.**