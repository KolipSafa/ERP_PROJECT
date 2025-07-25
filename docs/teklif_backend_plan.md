# **Teklif Modülü Backend Geliştirme Kılavuzu (Müşteri Modülü Referansıyla)**

Bu belge, `Teklif` modülünün backend'ini geliştirirken izlenecek adımları, mimari prensipleri ve en iyi pratikleri tanımlar. Bu kılavuz, `Müşteri` modülünün geliştirilmesi ve refactoring'i sırasında karşılaşılan zorluklardan çıkarılan dersleri temel alarak, tutarlı, güvenli ve yüksek kaliteli bir geliştirme süreci sağlamayı hedefler.

---

### **Temel Mimari ve Güvenlik Prensipleri**

Geliştirilecek her kod parçası, aşağıdaki prensiplere harfiyen uymalıdır:

1.  **Clean Architecture:** Katmanların sorumlulukları net bir şekilde ayrılmalıdır (`Core.Domain`, `Application`, `Infrastructure`, `API.Web`).
2.  **Unit of Work Deseni:** Tüm veritabanı yazma işlemleri (`Create`, `Update`, `Delete`) tek bir transaction içinde, `IUnitOfWork.SaveChangesAsync()` ile yönetilmelidir. Repository'lerdeki `Add`/`Update` metotları **senkron (`void`)** olmalıdır.
3.  **CQRS (Command Query Responsibility Segregation):** Veri okuma (`Query`) ve veri yazma (`Command`) işlemleri birbirinden tamamen ayrılmalıdır.
4.  **DTO (Data Transfer Objects):** API, dış dünyaya asla veritabanı `Entity`'lerini açmamalıdır. Tüm iletişim `DTO`'lar üzerinden yapılmalıdır.
5.  **Katmanlı Validasyon:** Gelen her `Command`, iş mantığına ulaşmadan önce `FluentValidation` ile doğrulanmalıdır. Bu validasyon, hem format/zorunluluk kontrollerini hem de veritabanı kontrollerini (örn: benzersizlik) içermelidir.
6.  **"Akıllı" Kısmi Güncelleme (`PATCH`):** `Update` işlemleri, `HTTP PATCH` metodu ile yapılmalı ve sadece gönderilen alanları güncelleyen, veriyi bozmayan güvenli bir mantık izlemelidir.

---

### **Geliştirme Adımları**

#### **Adım 1: `Core.Domain` Katmanı (Temeller)**

*   **Entity'ler:** Teklifler, satırlardan oluştuğu için iki entity'ye ihtiyacımız olacak.
    *   **`Teklif.cs`:** `Id (Guid)`, `TeklifNumarasi (string)`, `MusteriId (Guid)`, `TeklifTarihi (DateTime)`, `GecerlilikTarihi (DateTime)`, `ToplamTutar (decimal)`, `Durum (enum: Hazırlanıyor, Sunuldu, Onaylandı, Reddedildi)`, `IsActive (bool)`.
    *   **`TeklifSatiri.cs`:** `Id (Guid)`, `TeklifId (Guid)`, `UrunId (int)`, `Aciklama (string)`, `Miktar (decimal)`, `BirimFiyat (decimal)`, `Toplam (decimal)`.
*   **İlişkiler:** `Teklif` ile `Musteri` arasında ve `TeklifSatiri` ile `Urun` arasında ilişkiler kurulacak.
*   **Arayüzler (`Interfaces`):**
    *   `ITeklifRepository.cs` oluşturulacak.
    *   `IUnitOfWork.cs`'e `ITeklifRepository TeklifRepository { get; }` eklenecek.

#### **Adım 2: `Infrastructure` Katmanı (Uygulama)**

*   **`ApplicationDbContext`:** `DbSet<Teklif>` ve `DbSet<TeklifSatiri>` eklenecek. İlişkiler `OnModelCreating` içinde yapılandırılacak.
*   **`TeklifRepository.cs`:** `ITeklifRepository` arayüzü, `IQueryable` kullanılarak gelişmiş filtreleme ve sıralama yetenekleriyle birlikte implemente edilecek.
*   **`UnitOfWork.cs`:** Yeni `TeklifRepository`'yi yönetecek şekilde güncellenecek.

#### **Adım 3: `Application` Katmanı (İş Mantığı)**

*   **DTO'lar:** `TeklifDto.cs` ve `TeklifSatiriDto.cs` oluşturulacak.
*   **AutoMapper (`TeklifMappings.cs`):**
    *   `CreateMap<Teklif, TeklifDto>();` ve `CreateMap<TeklifSatiri, TeklifSatiriDto>();` tanımlanacak.
    *   **DİKKAT:** `UpdateTeklifCommand` için "akıllı" map'leme kuralı **eklenmeyecek**. Güncelleme manuel yapılacak.
*   **CQRS (`Features/Teklifler`):**
    *   **Queries:** `GetAllTekliflerQuery`, `GetTeklifByIdQuery`.
    *   **Commands:**
        *   `CreateTeklifCommand`: `MusteriId` ve `List<TeklifSatiriDto>` gibi verileri alacak.
        *   `UpdateTeklifCommand`: `PATCH` metoduna uygun olarak tüm alanları `nullable` olacak.
        *   `DeleteTeklifCommand`: Teklifi arşivlemek için `IsActive = false` yapacak.
*   **Validator'lar:**
    *   `TeklifValidatorBase` oluşturulacak.
    *   `CreateTeklifCommandValidator`: Müşterinin ve teklife eklenen ürünlerin veritabanında var olup olmadığını, miktarların pozitif olduğunu kontrol edecek.
    *   `UpdateTeklifCommandValidator`: Benzer kontrolleri yapacak.

#### **Adım 4: `API.Web` Katmanı (Arayüz)**

*   **`TekliflerController.cs`:**
    *   `[ApiController]` ve `[Route("api/[controller]")]` ile standart controller yapısı.
    *   `GET`, `GET /:id`, `POST`, `PATCH /:id`, `DELETE /:id` endpoint'leri oluşturulacak.
    *   **DİKKAT:** `Update` metodu `[HttpPatch]` kullanacak ve `command.Id`'yi URL'den gelen `id` ile güvenli bir şekilde atayacak.

#### **Adım 5: Veritabanı**

*   Tüm bu değişiklikleri içeren yeni bir migration oluşturulacak (`dotnet ef migrations add AddTeklifModule`).
*   Veritabanı güncellenecek (`dotnet ef database update`).

---

### **Öğrenilen Dersler ve Kaçınılması Gereken Hatalar**

Bu modülü geliştirirken, aşağıdaki dersler mutlaka göz önünde bulundurulmalıdır:

1.  **Hata Ayıklama için Loglama:** `ErrorHandlingMiddleware`'in, yakaladığı tüm beklenmedik hataları `ILogger` ile **mutlaka logladığından** emin ol. Bu, boş backend logları sorununu engeller.
2.  **`Update` Mantığı:** "Akıllı" AutoMapper kuralları yerine, `UpdateCommandHandler` içinde **manuel `??` operatörü ile atama** yöntemini kullan. Bu, `bool` ve `decimal` gibi tiplerin varsayılan değerlerle ezilmesi sorununu engeller.
3.  **Validator Mimarisi:** Kod tekrarını önlemek için mutlaka bir `TeklifValidatorBase` sınıfı oluştur. `IUnitOfWork` enjeksiyonu için `Program.cs`'de validator'ların **`Scoped`** olarak kaydedildiğinden emin ol.
4.  **`PATCH` ve ID Tutarlılığı:** Controller'daki `Update` metodu, `command.Id`'yi her zaman URL'den gelen `id` ile doldurmalıdır. Bu, `PATCH` isteğinde `Id` gönderilmediğinde oluşan `BadRequest` hatasını engeller.
5.  **`Add` Metodu Tutarlılığı:** Repository'deki `Add` metodu, Unit of Work desenine uygun olarak **`void` (senkron)** olmalıdır. `AddAsync` kullanılmamalıdır.
6.  **Entity ID Yapılandırması:** `int` tipindeki Primary Key alanlarının üzerine `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` attribute'ünü ekleyerek belirsizlikleri ortadan kaldır.
7.  **Geliştirme Ortamı:** `dotnet run` komutunu çalıştırırken, `HTTPS`'i aktif hale getirmek için `--launch-profile https` bayrağını kullan. Gerekirse, `dotnet dev-certs https --trust` komutunu **yönetici yetkileriyle** çalıştır.
