# Proje Geneli Test Stratejisi ve Planı

**Amaç:** Projenin tüm kritik işlevlerinin beklendiği gibi çalıştığını otomatize testlerle garanti altına almak, gelecekteki değişikliklerin mevcut sistemi bozmamasını (regression'ı önlemek) sağlamak ve yapılan son mimari değişikliklerin herhangi bir yan etkiye neden olup olmadığını doğrulamak.

---

### **Bölüm 1: Backend Test Stratejisi**

#### **1.1. Unit Testler (Birim Testleri) - `Application.UnitTests`**
*   **Hedef:** `Application` katmanındaki `CommandHandler` ve `QueryHandler`'ların iç iş mantığını, dış bağımlılıklardan (veritabanı, servisler) izole bir şekilde test etmek.
*   **Araçlar:** xUnit, Moq, FluentAssertions.
*   **Test Edilecek Öncelikli Alanlar:**
    *   **`SetPasswordForCustomerCommandHandler`:** Zaten test edildi. `✅`
    *   **`CreateCustomerCommandHandler`:**
        *   `UserManager.CreateAsync` ve `AddToRoleAsync` metotlarının doğru parametrelerle çağrıldığını,
        *   `IBackgroundJobService.Enqueue`'nun doğru metotla tetiklendiğini,
        *   `IUnitOfWork.SaveChangesAsync`'in en az bir kez çağrıldığını test et.
    *   **`UpdateProductCommandHandler`:**
        *   Sadece gönderilen alanların güncellendiğini, gönderilmeyenlerin aynı kaldığını ("akıllı güncelleme") test et.
        *   SKU değiştirildiğinde benzersizlik kontrolünün yapıldığını test et.
    *   **`CreateTeklifCommandHandler`:**
        *   Toplam tutarın doğru hesaplandığını,
        *   Teklif numarasının üretildiğini,
        *   Tüm satırların doğru şekilde eklendiğini test et.
    *   **Tüm `ValidationBehavior`'lar:** Her komut için yazılmış olan `FluentValidation` kurallarının, geçersiz veri gönderildiğinde `ValidationException` fırlattığını test et.

#### **1.2. Integration Testler (Entegrasyon Testleri) - `API.IntegrationTests`**
*   **Hedef:** API endpoint'lerini, bir HTTP isteğinden başlayarak veritabanı işlemlerine kadar tüm akışıyla, uçtan uca test etmek. Bu testler, mimari refactoring sonrası katmanların hala doğru konuştuğunu doğrulamanın en iyi yoludur.
*   **Araçlar:** xUnit, `WebApplicationFactory`, Test veritabanı (örn: LocalDB veya in-memory).
*   **Test Edilecek Öncelikli Akışlar:**
    *   **`AuthController`:**
        *   `POST /api/auth/set-password`: Geçerli bir kullanıcı ve token ile şifrenin gerçekten veritabanında güncellendiğini test et. Geçersiz token ile `400 Bad Request` döndüğünü test et.
    *   **`CustomersController`:**
        *   `POST /api/customers`: Yeni bir müşteri oluşturulduğunda hem `Customers` hem de `AspNetUsers` tablolarına kayıt atıldığını ve `201 Created` döndüğünü doğrula.
        *   `GET /api/customers`: Filtreleme ve sıralama parametrelerinin (`searchTerm`, `sortBy` vb.) beklendiği gibi çalıştığını ve `IsAccountActive` alanının doğru geldiğini doğrula.
    *   **`ProductsController` & `TekliflerController`:**
        *   Tüm temel CRUD (GET, POST, PATCH/PUT, DELETE) endpoint'lerinin başarılı durum kodları (`200 OK`, `201 Created`, `204 No Content`) döndüğünü ve veritabanını doğru şekilde etkilediğini doğrula.
    *   **Yetkilendirme:**
        *   Tüm yönetimsel endpoint'lere (`/api/products`, `/api/customers` vb.) `[Authorize]` attribute'u eklendikten sonra, **token olmadan** istek yapıldığında `401 Unauthorized` hatası alındığını test et.

---

### **Bölüm 2: Frontend Test Stratejisi**

#### **2.1. Unit Testler (Birim Testleri) - `vitest`**
*   **Hedef:** `stores` ve `composables` gibi yeniden kullanılabilir mantık içeren birimlerin izole bir şekilde test edilmesi.
*   **Araçlar:** Vitest, `@vue/test-utils`.
*   **Test Edilecek Öncelikli Alanlar:**
    *   **`auth.store.ts`:**
        *   `login` action'ı başarılı olduğunda `accessToken` ve `user` state'lerinin doğru ayarlandığını, `localStorage`'a yazıldığını test et.
        *   `login` başarısız olduğunda state'lerin temizlendiğini test et.
        *   `logout` action'ının tüm state'i ve `localStorage`'ı temizlediğini test et.
        *   `isAdmin` ve `isCustomer` getter'larının, `user` rolüne göre doğru boolean değeri döndürdüğünü test et.
    *   **`useNotifier.ts`:** `success`, `error` gibi fonksiyonların, `vue3-toastify`'ın ilgili metodunu doğru mesajla çağırdığını test et (mocking ile).

#### **2.2. End-to-End (E2E) Testler - `Cypress` veya `Playwright`**
*   **Hedef:** Kullanıcının tarayıcıda yapacağı kritik yolculukları baştan sona simüle ederek tüm sistemin (frontend + backend) bir bütün olarak doğru çalıştığını doğrulamak.
*   **Araçlar:** Cypress veya Playwright.
*   **Test Edilecek Öncelikli Senaryolar:**
    1.  **Login Akışı:**
        *   Kullanıcı `login` sayfasına gider.
        *   Geçerli admin bilgileriyle giriş yapar.
        *   Başarılı bir şekilde `dashboard` sayfasına yönlendirilir.
        *   Sayfayı yenilediğinde hala giriş yapmış durumda kalır.
        *   Logout butonuna tıkladığında `login` sayfasına geri döner.
    2.  **Yeni Müşteri Oluşturma ve Aktivasyon Akışı:**
        *   Admin, `customers` sayfasına gider, "Yeni Müşteri" butonuna tıklar.
        *   Formu doldurur ve kaydeder.
        *   Müşterinin listede "Hesap Durumu: Aktivasyon Bekliyor" olarak göründüğünü doğrular.
        *   (Testin bu kısmı için backend log'larını veya sahte bir e-posta servisini kontrol ederek) Aktivasyon linkinin üretildiğini doğrular.
    3.  **Ürün CRUD Akışı:**
        *   Admin, yeni bir ürün oluşturur, listede görür, düzenler ve son olarak arşivler. Her adımda bildirimlerin doğru çıktığını doğrular.
