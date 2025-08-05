# Plan 1: Mimari Dönüşüm ve Supabase Entegrasyonu

**Amaç:** Projenin veritabanı ve kimlik doğrulama katmanlarını, yönetilen bir servis olan Supabase'e taşıyarak altyapı karmaşıklığını azaltmak, geliştirme sürecini hızlandırmak ve canlıya çıkışı kolaylaştırmak.

---

### **Genel Strateji**

Mevcut .NET iş mantığını koruyarak, veritabanını **Supabase Postgres**'e ve kimlik doğrulamasını **Supabase Auth**'a devredeceğiz. Frontend (Vue), kimlik doğrulama işlemleri için doğrudan Supabase ile konuşacak; .NET API ise sadece Supabase tarafından üretilen JWT'leri doğrulayacak ve içindeki rollere göre yetkilendirme yapacak.

---

### **Faz 1: Hazırlık ve Kurulum** `✅ Tamamlandı`

*   **Adım 1.1: Supabase Projesi Oluşturma** `✅`
    *   [Supabase.io](https://supabase.io) adresinde yeni bir proje oluşturuldu.
    *   Gerekli `Project URL`, `API Keys` ve `Connection string` bilgileri alındı.
*   **Adım 1.2: Geliştirme Platformu Hesapları** `✅`
    *   Backend'i dağıtmak için [Render.com](https://render.com) üzerinde hesap oluşturuldu.
    *   Frontend'i dağıtmak için [Vercel.com](https://vercel.com) üzerinde hesap oluşturuldu.

---

### **Faz 2: Backend Refaktoring (.NET API)** `✅ Tamamlandı`

*   **Adım 2.1: Veritabanı Sağlayıcısını Değiştirme** `✅`
    1.  **NuGet Paket Yönetimi:** `✅` `SqlServer` ve `OpenIddict` paketleri kaldırıldı, `Npgsql` paketi eklendi.
    2.  **`Program.cs` Yapılandırması:** `✅` `UseNpgsql` kullanacak şekilde güncellendi.
    3.  **`appsettings.json` Güncellemesi:** `✅` Geliştirme ortamı için Supabase `session pooler` bağlantı bilgisi ayarlandı.
    4.  **Veritabanı Migration'ı:** `✅` Eski migration'lar temizlendi, `InitialSupabaseMigration` oluşturuldu ve `dotnet ef database update` komutuyla başarıyla çalıştırıldı.

*   **Adım 2.2: Kimlik Doğrulama Mantığını Değiştirme** `✅`
    1.  **Kod Temizliği:** `✅` `Program.cs` içerisinden `OpenIddict` yapılandırması ve `AuthController`'dan gereksiz endpoint'ler temizlendi.
    2.  **JWT Doğrulama Yapılandırması (`Program.cs`):** `✅` Supabase JWT'lerini doğrulayacak yapılandırma eklendi.
    3.  **Müşteri Oluşturma İşlemini Güncelleme (`CreateCustomerCommand.cs`):** `✅` Bu komutun sorumluluğu değiştirildi. Artık backend kullanıcı davet etmiyor. Bunun yerine, frontend tarafından yönetilen davet akışı sonrası, Supabase'de oluşan `UserId` ile birlikte ERP veritabanına müşteri kaydını oluşturuyor.
    4.  **Supabase Admin Entegrasyonunu Sağlamlaştırma:** `✅` `supabase-csharp` kütüphanesinin neden olduğu derleme ve kararlılık sorunları nedeniyle, Supabase'e yönetici (admin) yetkileriyle yapılacak tüm istekler (kullanıcı silme, `app_metadata` güncelleme vb.) için kütüphaneden vazgeçilmiştir. Bunun yerine, doğrudan Supabase'in REST API'sine istek atan, `HttpClient` tabanlı bir `SupabaseAuthAdminService` yazılarak daha sağlam ve sürdürülebilir bir mimari benimsenmiştir.

---

### **Faz 3: Frontend Refaktoring (Vue Client)** `✅ Tamamlandı`

*   **Adım 3.1: Supabase JS Client Kurulumu** `✅`
    1.  `@supabase/supabase-js` paketi projeye eklendi.
    2.  `src/lib/supabaseClient.ts` dosyası oluşturuldu ve Supabase client başlatıldı.
    3.  `.env.development` dosyasına `VITE_SUPABASE_URL` ve `VITE_SUPABASE_ANON_KEY` değişkenleri eklendi.

*   **Adım 3.2: Kimlik Doğrulama Akışlarını Güncelleme** `✅`
    1.  **`stores/auth.store.ts`:** `✅` Store, Supabase ile etkileşim kuran bir `AuthService` kullanacak şekilde düzenlendi. `login`, `logout` ve `onAuthStateChange` fonksiyonları Supabase'e bağlandı.
    2.  **Diğer Component'ler:** `✅` `LoginView`, `SetPasswordView` gibi görünümlerin bu yeni store ve servis yapısıyla uyumlu çalışması sağlandı.

---

### **Faz 4: Rol, Varlık ve Davet Mekanizması** `✅ Tamamlandı`

*   **Adım 4.1: Varlıkların Ayrıştırılması** `✅`
    *   Yönetici ve Müşteri sorumluluklarını ayırmak için `Core.Domain` katmanına `Administrator.cs` entity'si eklendi ve ilgili veritabanı migration'ı uygulandı.

*   **Adım 4.2: Müşteri Davet Mekanizmasının Kurulması** `✅`
    *   **Supabase CLI Kurulumu:** `✅` `npm install supabase` komutuyla CLI kuruldu, `npx supabase init` ile proje başlatıldı ve `npx supabase link` ile uzak projeye başarıyla bağlandı.
    *   **Edge Function Oluşturma:** `✅` `npx supabase functions new invite-user` komutuyla davet fonksiyonu oluşturuldu.
    *   **Güvenli Kod Yazımı:** `✅` Fonksiyonun içine, sadece "Admin" rolüne sahip kullanıcıların istek yapabilmesini sağlayan yetkilendirme kontrolü eklendi. Davet edilen kullanıcıya otomatik olarak "Customer" rolü atayan mantık dahil edildi.
    *   **Paylaşılan Kod Yapısı:** `✅` Fonksiyonun ihtiyaç duyduğu `_shared/cors.ts` dosyası oluşturuldu.
    *   **Dağıtım:** `✅` Fonksiyon, `npx supabase functions deploy invite-user` komutuyla başarıyla Supabase sunucularına dağıtıldı.

*   **Adım 4.3: Rollerin Tanımlanması ve Uygulanması** `✅ Tamamlandı`
    *   **Yönetici (Admin) Rolü:** Yöneticiler, Supabase arayüzünden manuel olarak oluşturulur, profilleri `Administrators` tablosuna kaydedilir ve `app_metadata`'larına `{ "roles": ["Admin"] }` eklenir.
    *   **Müşteri (Customer) Rolü:** Müşteriler, Edge Function ile davet edilir. Bu fonksiyon, `app_metadata`'ya `{ "roles": ["Customer"] }` rolünü otomatik olarak ekler.

*   **Adım 4.4: Yetki Denetiminin Koda Eklenmesi** `✅ Tamamlandı`
    *   **.NET API:** Kritik endpoint'lerin başına `[Authorize(Roles = "Admin")]` veya `[Authorize(Roles = "Customer")]` gibi attribute'lar eklenerek sunucu taraflı koruma sağlandı.
    *   **Vue Frontend:** `auth.store.ts` içindeki `user` nesnesi rolleri içerecek şekilde güncellendi. `v-if="authStore.isAdmin"` gibi direktiflerle menüler, butonlar ve sayfalar dinamik olarak gizlenip gösteriliyor.

---

### **Faz 5: Dağıtım (Deployment)** `📝 Planlandı`

*   **Adım 5.1: Backend (.NET API -> Render)**
    *   Projeyi bir Git repository'sine (GitHub, GitLab) push'la.
    *   Render.com'da yeni bir "Web Service" oluştur ve Git repository'sini bağla.
    *   **Build Command:** `dotnet publish -c Release -o out`
    *   **Start Command:** `dotnet out/API.Web.dll`
    *   **Environment Variables:**
        *   `ASPNETCORE_ENVIRONMENT`: `Production`
        *   `ConnectionStrings__DefaultConnection`: Supabase **Session Pooler** bağlantı bilgisi.
        *   `Supabase__Url`: Supabase Proje URL'niz.
        *   `Supabase__ServiceRoleKey`: Supabase `service_role` anahtarınız.
        *   `Jwt__Authority`: `https://<PROJE-REF>.supabase.co/auth/v1`
        *   `Jwt__Audience`: `authenticated`

*   **Adım 5.2: Frontend (Vue -> Vercel)**
    *   Projeyi bir Git repository'sine push'la.
    *   Vercel.com'da yeni bir proje oluştur ve Git repository'sini bağla.
    *   **Framework Preset:** `Vite` olarak seç.
    *   **Environment Variables:**
        *   `VITE_API_BASE_URL`: Render'da çalışan .NET API'nin adresi.
        *   `VITE_SUPABASE_URL`: Supabase Proje URL'niz.
        *   `VITE_SUPABASE_ANON_KEY`: Supabase `anon (public)` anahtarınız.