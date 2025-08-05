# Staj Raporu

Bu belge, ERP projesi üzerinde yapılan günlük çalışmaları özetlemektedir.

---

## 5 Ağustos 2025 Salı

Bugün, bir önceki gün tespit edilen ve müşteri aktivasyon akışını tamamen engelleyen kritik backend hatalarını çözmek için yoğun bir hata ayıklama ve yeniden yapılandırma (refactoring) çalışması yapıldı.

### Supabase Admin Entegrasyonunun Yeniden İnşası
- **Kök Neden Tespiti:** `supabase-csharp` kütüphanesinin `0.16.2` versiyonunda, yönetici (admin) fonksiyonlarına erişimle ilgili ciddi tutarsızlıklar ve eksik dokümantasyon olduğu tespit edildi. `AdminClient`, `UserAttributes` ve `_client.Auth.Admin` gibi nesnelerle yapılan sayısız deneme, kütüphanenin mevcut sürümünün kararlı bir yönetici arayüzü sunmadığını kanıtladı.
- **Stratejik Değişiklik (API Odaklı Yaklaşım):** Kütüphane bağımlılığının yarattığı risk ve zaman kaybını ortadan kaldırmak için radikal bir karar alındı. `SupabaseAuthAdminService`, kütüphaneyi kullanmak yerine, doğrudan Supabase'in sunduğu **stabil ve iyi belgelenmiş REST API**'sine `HttpClient` ile istek atacak şekilde sıfırdan yeniden yazıldı.
- **Güvenli ve Sağlam Implementasyon:**
  - `Program.cs`, artık `supabase-csharp` client'ı yerine, bu yeni servis için bir `HttpClient` fabrikası kaydettirecek şekilde güncellendi.
  - Yeni servis, `appsettings.json`'dan okuduğu `ServiceRoleKey`'i `Bearer` token olarak kullanarak Supabase'e güvenli ve yetkili istekler yapacak şekilde yapılandırıldı.
  - Bu yeni mimari, projenin Supabase yönetici işlemlerini kütüphane versiyonlarından tamamen bağımsız hale getirerek geleceğe dönük büyük bir kararlılık sağladı.

### Hata Ayıklama ve Son Düzeltmeler
- **Yapılandırma Hatasının Giderilmesi:** Yeni `HttpClient` tabanlı servisin, `appsettings.json`'daki Supabase anahtarını yanlış bir isimle (`ServiceKey` yerine `ServiceRoleKey`) okumaya çalıştığı tespit edildi ve bu kritik yapılandırma hatası düzeltildi.
- **Derleme Hatalarının Temizlenmesi:** `ISupabaseAuthAdminService` arayüzü ve bu arayüzü kullanan `CreateCustomerCommandHandler` gibi sınıflar, yeni `HttpClient` tabanlı servisin `async Task<User?>` gibi modern C# desenlerini kullanan metot imzalarıyla tam uyumlu hale getirildi. Bu süreçte eksik `using` ifadeleri ve yanlış tür bildirimlerinden kaynaklanan tüm derleme hataları giderildi.

Günün sonunda, müşteri aktivasyon ve silme işlemlerinin altyapısı, kütüphane sorunlarından arındırılmış, test edilebilir ve son derece sağlam bir yapıya kavuşturulmuştur. Projenin backend'i şu anda bilinen hiçbir derleme hatası veya bug olmadan çalışır durumdadır.

---

## 4 Ağustos 2025 Pazartesi

Bugün, müşteri yaşam döngüsünün kritik bir parçası olan **Müşteri Silme** işlevselliği üzerine odaklanıldı ve bu süreçte hem frontend hem de backend'de önemli geliştirmeler ve hata ayıklamaları yapıldı.

### Müşteri Silme Akışının Geliştirilmesi
- **Bütünsel Yaklaşım:** Bir müşterinin silinmesinin, sadece bizim veritabanımızdaki (`Customers` tablosu) kaydın pasif hale getirilmesi değil, aynı zamanda **Supabase Authentication**'daki ilgili kullanıcının da kalıcı olarak silinmesi gerektiği belirlendi. Bu, sistemde artık bir karşılığı olmayan "hayalet" kullanıcıların kalmasını önler.
- **Backend Servisi Oluşturma (`ISupabaseAuthAdminService`):**
  - Supabase üzerinde yönetici yetkileriyle (kullanıcı silme, rol atama vb.) işlem yapacak olan servis için `ISupabaseAuthAdminService` arayüzü oluşturuldu.
  - Bu servisin ilk implementasyonu, `supabase-csharp` NuGet paketi kullanılarak `SupabaseAuthAdminService.cs` sınıfında gerçekleştirildi.
- **İş Mantığı Entegrasyonu (`DeleteCustomerCommand`):**
  - Müşteri silme komutu olan `DeleteCustomerCommandHandler`, artık `IUnitOfWork`'e ek olarak `ISupabaseAuthAdminService`'i de enjekte alacak şekilde güncellendi.
  - İş akışı, önce Supabase'den kullanıcıyı silmeye çalışacak, bu işlem başarılı olsun veya olmasın, ardından yerel veritabanındaki müşteri kaydını pasif hale getirecek şekilde yeniden düzenlendi.
- **Frontend Entegrasyonu (`CustomersView.vue`):**
  - Müşteri listesi ekranına, "Arşivle" (`mdi-archive-arrow-down`) ve "Geri Yükle" (`mdi-restore`) ikon butonları eklendi.
  - Bu butonlar, ilgili backend endpoint'lerini çağıran `CustomerService.ts` metotlarını tetikleyecek şekilde bağlandı.

### Kritik Hata Ayıklama: `appsettings.json`
- **Sorun Tespiti:** Geliştirme sırasında, backend'in Supabase'e bağlanamamasına neden olan `SocketException: Bilinen böyle bir ana bilgisayar yok` hatası alındı.
- **Kök Neden:** Hataya, `appsettings.Development.json` dosyasının, hassas bilgileri (connection string, service role key) içermediği için `.gitignore`'da olmasının ve bu dosyanın yerel makinede yanlış veya eksik olmasının neden olduğu anlaşıldı.
- **Çözüm ve Standardizasyon:**
  - Projeye, doğru `appsettings.Development.json` yapısını gösteren bir `appsettings.Development.json.example` dosyası eklendi.
  - Geliştirme ortamı için gerekli olan `ConnectionString`, `Supabase:Url` ve `Supabase:ServiceRoleKey` gibi tüm kritik yapılandırma anahtarları bu dosyada standartlaştırıldı ve doğru değerlerle dolduruldu. Bu, gelecekteki "benim makinemde çalışmıyor" sorunlarını önleyecektir.

---

## 1 Ağustos 2025 Cuma

Bugün, hafta boyunca geliştirilen ve projenin en karmaşık iş akışlarından biri olan **güvenli müşteri davet ve aktivasyon** sürecinin son adımları tamamlandı ve uçtan uca test edildi.

### Davet Akışının Tamamlanması
- **Şifre Belirleme Sayfası (`SetPasswordView.vue`):**
  - Davet linkiyle gelen kullanıcıların ilk şifrelerini oluşturacakları arayüz geliştirildi.
  - Forma, girilen iki şifrenin eşleşmesi ve minimum uzunlukta olması gibi temel **doğrulama kuralları (validation rules)** eklendi.
- **Hesap Aktivasyon Mantığı:**
  - "Kaydet" butonuna basıldığında tetiklenen `savePassword` metodu, birden fazla adımdan oluşan bir zinciri yönetecek şekilde implemente edildi:
    1.  Önce `AuthService.updateUserPassword` ile kullanıcının şifresi Supabase'de güncellenir.
    2.  Ardından, `CustomerService.create` ile backend API'sine istek atılarak, Supabase'den alınan kullanıcı bilgileriyle (`user_id`, `email`, `isim` vb.) `Customers` tablosuna yeni bir kayıt oluşturulur.
    3.  Son olarak, `AuthService.updateUserMetadata` ile Supabase'deki kullanıcının `app_metadata.status` alanı `'invited'`'dan `'active'`'e çevrilir.
- **Yönlendirme Mantığı (`router/index.ts`):**
  - Uygulamanın `router guard`'ı (`beforeEach`), `status`'u `'active'` olan bir kullanıcının artık `/set-password` sayfasına erişememesini ve doğrudan kendi paneline yönlendirilmesini sağlayacak şekilde güncellendi.

### Uçtan Uca Test ve Hata Ayıklama
- **Zamanlama (Race Condition) Sorunları:** Testler sırasında, `status` güncellendikten hemen sonra yönlendirmenin çalışmadığı tespit edildi. Sorunun, `onAuthStateChange` dinleyicisinin, reaktif state güncellemelerindeki zamanlama farkları nedeniyle doğru anda tetiklenmemesinden kaynaklandığı anlaşıldı. Bu sorun, yönlendirme mantığının daha sağlam koşullarla yeniden yazılmasıyla çözüldü.
- **Veri Bütünlüğü:** Davet akışı sırasında `company_id` gibi kritik verilerin `user_metadata` aracılığıyla `SetPasswordView`'e kadar doğru bir şekilde taşındığı ve backend'e eksiksiz olarak iletildiği doğrulandı.

---

## 31 Temmuz 2025 Perşembe

Bugün, müşteri davet akışının temelini oluşturan **rol ve durum yönetimi** üzerine odaklanıldı. Amaç, davet edilen bir kullanıcıyı, şifresini belirleyene kadar sistemde kısıtlı bir durumda tutmaktı.

### `status: 'invited'` Mimarisi
- **Strateji:** Bir önceki gün karşılaşılan `aud` (audience) claim sorununu aşmak için, Supabase'in `app_metadata` özelliği kullanılarak daha esnek ve güvenilir bir çözüm geliştirildi.
- **Edge Function Güncellemesi (`invite-user/index.ts`):**
  - Müşteri davet eden Supabase Edge Function, artık davet edilen kullanıcının `app_metadata`'sına `{ "roles": ["Customer"], "status": "invited" }` verisini ekleyecek şekilde güncellendi. Bu `status` alanı, kullanıcının davet akışını henüz tamamlamadığını gösteren bir bayrak görevi görür.
- **State Management Güncellemesi (`auth.store.ts`):**
  - Pinia tabanlı `authStore`, JWT'yi parse ederken `app_metadata` içindeki bu yeni `status` alanını okuyacak ve state'e kaydedecek şekilde genişletildi.
  - Store'a, o anki kullanıcının davetli mi yoksa aktif mi olduğunu kolayca kontrol edebilmek için yeni `getter`'lar eklendi.
- **Güvenli Yönlendirme (`router/index.ts`):**
  - Uygulamanın ana yönlendiricisine (`router guard`), `authStore`'daki kullanıcı durumunu kontrol eden bir mantık eklendi.
  - Bu mantık, `status`'u `'invited'` olan bir kullanıcının, şifre belirleme sayfası (`/set-password`) dışında hiçbir sayfaya erişememesini garanti altına alır. Kullanıcı başka bir sayfaya gitmeye çalışırsa, otomatik olarak `/set-password`'e yönlendirilir.

Bu çalışmalarla, davet edilen kullanıcıların sisteme kontrollü bir şekilde dahil edilmesini sağlayan güvenli ve sağlam bir temel oluşturulmuştur.

---

## 30 Temmuz 2025 Çarşamba

Bugün, projenin en önemli özelliklerinden biri olan **Yönetici tarafından Müşteri Davet Etme** mekanizmasının altyapısı, modern ve sunucusuz (serverless) bir yaklaşımla, **Supabase Edge Functions** kullanılarak kuruldu.

### Supabase Edge Function Geliştirmesi
- **Supabase CLI Entegrasyonu:**
  - Geliştirme ortamına `supabase` CLI kuruldu ve `npx supabase init` ile proje yapısı oluşturuldu.
  - `npx supabase link` komutu kullanılarak yerel proje, buluttaki Supabase projesine başarıyla bağlandı.
- **`invite-user` Fonksiyonu:**
  - `npx supabase functions new invite-user` komutuyla, Deno (TypeScript) tabanlı yeni bir sunucusuz fonksiyon oluşturuldu.
  - Fonksiyonun ana mantığı, Supabase'in admin yetkilerine sahip bir istemci (`service_role_key` kullanarak) oluşturup, bu istemci aracılığıyla `inviteUserByEmail` metodunu çağırmak üzerine kuruldu.
- **Güvenlik ve Rol Atama:**
  - Fonksiyonun, yetkisiz kişiler tarafından çağrılmasını engellemek için, isteği yapan kullanıcının JWT'sini doğrulayan ve içinde "Admin" rolünün olup olmadığını kontrol eden bir güvenlik katmanı eklendi.
  - Davet edilen kullanıcıya, `app_metadata` üzerinden otomatik olarak "Customer" rolünün atanması sağlandı. Bu, davet edilen her kullanıcının sisteme doğru yetkilerle başlamasını garanti eder.
- **Frontend Entegrasyonu:**
  - `CustomerService.ts` içine, bu yeni Edge Function'ı çağıran `inviteCustomer` adında yeni bir metot eklendi.
  - `CustomerFormView.vue`, "Yeni Müşteri Ekle" modundayken artık doğrudan backend'e `POST` isteği atmak yerine, bu yeni `inviteCustomer` servisini çağıracak şekilde güncellendi.

Bu geliştirme ile birlikte, müşteri oluşturma süreci, güvenli, ölçeklenebilir ve modern bir sunucusuz mimariye taşınmıştır.

---

## 29 Temmuz 2025 Salı

Bugünün ana odağı, projeye yeni bir **Ayarlar Modülü** eklemek, bu modülün ilk işlevselliği olan para birimi yönetimini (Currency Management) hayata geçirmek ve bu süreçte proje genelinde tespit edilen çeşitli hataları ve tutarsızlıkları gidererek kod kalitesini artırmaktı.

### Ayarlar Modülü Geliştirmesi ve Hibrit Yaklaşım
- **Hibrit Para Birimi Yönetimi:** Kullanıcıya esneklik sunmak ile veri bütünlüğünü korumak arasında bir denge kuran **hibrit bir yaklaşım** benimsendi.
- **Veritabanı Seeding:** Bu yaklaşıma uygun olarak, Entity Framework Core'un "Data Seeding" özelliği kullanılarak sisteme en sık kullanılan para birimleri (TRY, USD, EUR) başlangıç verisi olarak eklendi. Bu değişiklikleri uygulamak için `SeedCurrencies` adında yeni bir veritabanı migration'ı oluşturuldu ve başarıyla uygulandı.
- **Para Birimi CRUD İşlemleri (Backend):**
  - `SettingsController` adında yeni bir API kontrolcüsü oluşturuldu.
  - **Listeleme (`GET`):** Tüm para birimlerini getiren `GetCurrenciesQuery` ve işleyicisi (handler) implemente edildi.
  - **Oluşturma (`POST`):** Sisteme yeni bir para birimi eklemeyi sağlayan `CreateCurrencyCommand`, bu komutu işleyen `CreateCurrencyCommandHandler` ve gelen veriyi doğrulayan `CreateCurrencyCommandValidator` (FluentValidation ile) uçtan uca geliştirildi.
  - Controller, bu yeni CQRS bileşenlerini `MediatR` aracılığıyla kullanacak şekilde güncellendi.

### Proje Geneli Sağlamlaştırma ve Hata Ayıklama
- **Mimari Tutarlılık ve Hata Giderme:**
  - Geliştirme sırasında, `Company` modülündeki komut işleyicilerinin `IUnitOfWork` arayüzünü yanlış kullandığı (`_unitOfWork.Companies` yerine `_unitOfWork.CompanyRepository` kullanılmalıydı) tespit edildi. Bu durumdan kaynaklanan **çok sayıda derleme hatası** proje genelinde düzeltildi.
  - `TeklifMappings` ve `CustomerRepository` gibi dosyalarda, `null` referanslara yol açabilecek ve derleyici tarafından uyarı olarak işaretlenen kod bölümleri, gerekli kontroller eklenerek güvenli hale getirildi.
  - `CreateCurrencyCommandValidator` içinde, `Application` katmanının `Infrastructure` katmanına referans vermesine neden olan (mimariyi bozan) hatalı bir `using` ifadesi kaldırılarak katmanların ayrılığı prensibi korundu.

Bugünkü çalışmalar sonucunda, Ayarlar modülünün ilk temel taşı olan para birimi yönetimi altyapısı tamamlanmış, proje genelindeki gizli kalmış hatalar temizlenmiş ve backend projesi **hatasız ve uyarısız** bir şekilde derlenir hale getirilmiştir.
---

## 28 Temmuz 2025 Pazartesi

Bugün, projenin üçüncü ve en karmaşık modülü olan **Teklif Yönetimi**'nin backend altyapısı, projenin mevcut Clean Architecture ve CQRS prensiplerine sadık kalınarak uçtan uca geliştirildi. Bu geliştirme, `Product` ve `Customer` modüllerinden gelen verileri bir araya getiren, ilişkisel bir yapıya sahiptir.

### Teklif Modülü Backend Geliştirmesi
- **Veritabanı ve Domain Katmanı (`Core.Domain`):**
  - `Teklif` ve `TeklifSatiri` adında iki yeni entity oluşturuldu. `Teklif`, ana teklif bilgilerini (müşteri, tarih, durum vb.) tutarken, `TeklifSatiri` ise o teklife eklenen ürünleri ve fiyatlarını barındırır.
  - Tekliflerin durumunu (Taslak, Onaylandı, Reddedildi vb.) yönetmek için `QuoteStatus` adında bir `enum` tanımlandı.
  - Veri erişimini soyutlamak için `ITeklifRepository` arayüzü oluşturuldu ve `IUnitOfWork`'e entegre edildi.
- **Veri Erişimi ve Altyapı (`Infrastructure`):**
  - `ApplicationDbContext`'e yeni `DbSet`'ler (`Teklifler`, `TeklifSatirlari`) eklendi.
  - `TeklifRepository`, `ITeklifRepository` arayüzünü implemente edecek şekilde, verimli sorgulama mantıklarıyla birlikte yazıldı.
  - Entity Framework Core için `AddTeklifModule` adında yeni bir `migration` oluşturularak veritabanı şeması güncellendi.
- **İş Mantığı Katmanı (`Application`):**
  - API'nin güvenli ve temiz bir veri modeli sunması için `TeklifDto` ve `TeklifSatiriDto` oluşturuldu.
  - Entity-DTO dönüşümleri için `TeklifMappings` profili `AutoMapper` kullanılarak yazıldı.
  - Tüm CRUD işlemleri için CQRS `Query` ve `Command`'leri (`GetAllTekliflerQuery`, `CreateTeklifCommand` vb.) ve bu komutları işleyen `Handler`'lar oluşturuldu.
  - `FluentValidation` kullanılarak, yeni teklif oluşturma ve güncelleme komutları için detaylı doğrulama kuralları eklendi.
- **API Katmanı (`API.Web`):**
  - Dış dünyanın teklif verilerine erişebilmesi için `/api/teklifler` endpoint'lerini yöneten `TekliflerController` oluşturuldu.

Bugünkü çalışmalarla birlikte, projenin en önemli işlevsel modüllerinden birinin backend altyapısı tamamlanmış ve frontend geliştirmesi için hazır hale getirilmiştir.

---

## 25 Temmuz 2025 Cuma

Bugünün ana odağı, projenin ikinci ana modülü olan **Müşteri Cari**'nin frontend (kullanıcı arayüzü) katmanını, mevcut Ürün modülünün yüksek standartlarını referans alarak uçtan uca geliştirmek ve bu süreçte karşılaşılan hataları ayıklamaktı.

### Müşteri Modülü Frontend Geliştirmesi
- **Planlı Geliştirme:** `customer_frontend_plan.md` belgesinde tanımlanan plana sadık kalınarak, Müşteri modülü için gerekli tüm Vue.js component'leri ve servisleri oluşturuldu.
- **API Servisi (`CustomerService.ts`):** Backend ile tüm müşteri API iletişimini yönetecek olan servis, `ProductService.ts`'in yapısı referans alınarak, tip güvenliği (TypeScript DTO'ları ve Payload'ları) ve merkezi yönetim ilkeleriyle oluşturuldu.
- **Müşteri Listeleme Ekranı (`CustomersView.vue`):**
  - `v-data-table` kullanılarak, müşterileri listeleyen, arama ve sıralama yapabilen dinamik bir tablo oluşturuldu.
  - Kullanıcı deneyimini iyileştirmek için, adres ve vergi numarası gibi detay bilgiler, `Product` modülündeki gibi genişletilebilir bir satır (`v-slot:expanded-row`) içinde gösterildi.
- **Müşteri Form Ekranı (`CustomerFormView.vue`):**
  - Hem "Yeni Müşteri Ekleme" hem de "Müşteri Düzenleme" modlarında çalışabilen, yeniden kullanılabilir tek bir form component'i geliştirildi.
  - URL'deki `:id` parametresine göre modlar arasında geçiş yapma ve ilgili verileri yükleme mantığı implemente edildi.
- **Yönlendirme (`router/index.ts`):** Müşteri modülünün tüm yeni sayfaları (`/customers`, `/customers/new`, `/customers/edit/:id`), uygulamanın yönlendirme sistemine eklendi.

### Hata Ayıklama ve Sağlamlaştırma
- **Yönlendirme (Routing) Hataları:** Geliştirme sırasında karşılaşılan "No match found for location" ve "duplicate declaration" gibi kritik yönlendirme hataları, `router/index.ts` dosyasındaki eksik ve yinelenen `import` ifadeleri düzeltilerek giderildi.
- **Filtreleme ve Sıralama Mantığı:**
  - İlk başta çalışmayan filtreleme ve sıralama özelliğinin kök nedeni, frontend ve backend arasındaki API parametre isimlerinin (`search` vs `searchTerm`) uyuşmaması olarak tespit edildi ve bu uyumsuzluk giderildi.
  - Sıralamanın (örn: Z-A) düzgün çalışmamasına neden olan reaktivite (state management) hatası, `v-select` component'inin state'i daha basit ve güvenilir bir string (`'name_desc'`) üzerinden yönetmesi sağlanarak çözüldü.
- **Veri Bütünlüğü ve Test Verisi:**
  - Düzenleme formunda hatalı verilerin görünmesi sorununun, koddan değil, veritabanındaki bozuk bir kayıttan kaynaklandığı tespit edildi.
  - Geliştirme ortamını zenginleştirmek için, Entity Framework Core'un "Data Seeding" mekanizması kullanılarak veritabanına 20 adet örnek müşteri verisi eklendi. Bu süreçte karşılaşılan `ConnectionString` ve `dotnet ef` komut hataları, `ApplicationDbContextFactory`'nin düzeltilmesi ve sabit tarih değerleri kullanılmasıyla aşıldı.

Bugünkü çalışmalar sonucunda, Müşteri modülü hem backend hem de frontend olarak tamamlanmış, test edilmiş ve proje planına uygun şekilde teslim edilmiştir.

---

## 24 Temmuz 2025 Perşembe

Bugün, projenin ikinci ana modülü olan **Müşteri Cari**'nin backend altyapısı, projenin mevcut yüksek standartları referans alınarak uçtan uca geliştirildi. Bu süreç, aynı zamanda `Product` modülünde tespit edilen mimari ve güvenlik eksikliklerinin giderildiği kapsamlı bir **refactoring ve hata ayıklama** çalışmasını da içerdi.

### Müşteri Modülü Backend Geliştirmesi
- **Uçtan Uca İnşa:** `Product` modülünden alınan derslerle, Müşteri modülü için `Core.Domain` (Entity, Interface), `Infrastructure` (Repository), `Application` (CQRS, DTO, Validator) ve `API.Web` (Controller) katmanları eksiksiz bir şekilde oluşturuldu.
- **Mimari Tutarlılık:** Geliştirme, en başından itibaren **Unit of Work** desenine, **senkron `Add/Update`** metotlarına ve **güvenli `PATCH`** işlemine uygun olarak yapıldı.
- **Veritabanı Entegrasyonu:** `Customer` varlığı için Entity Framework migration'ı oluşturuldu ve veritabanı şeması başarıyla güncellendi.

### Proje Geneli Refactoring ve Sağlamlaştırma
- **Validator Mimarisi:** Hem `Product` hem de `Customer` modülleri için, kod tekrarını önleyen ve `IUnitOfWork` enjekte edilmiş **temel validator sınıfları (`ProductValidatorBase`, `CustomerValidatorBase`)** oluşturuldu. Bu sayede, `SKU` ve `Email` gibi alanların veritabanından **benzersizlik (uniqueness) kontrolü** artık validasyon katmanında yapılıyor.
- **"Akıllı Güncelleme" Mantığının Güvenliği:** `Customer` modülünde `Update` işlemi sırasında `AutoMapper`'ın `bool` ve `decimal` gibi tiplerin varsayılan değerlerini atayarak veri bozulmasına yol açtığı tespit edildi. Bu kritik hata, "akıllı" AutoMapper kuralı kaldırılarak ve yerine daha güvenli olan **manuel `??` operatörü ile atama** yöntemine geri dönülerek düzeltildi.
- **API Tutarlılığı:** Hem `ProductsController` hem de `CustomersController`'daki `Update` endpoint'leri, RESTful standartlarına uygun olarak `[HttpPut]`'tan **`[HttpPatch]`**'e çevrildi. Controller'lar, `PATCH` isteğinde `Id` olmasa bile komutu doğru `Id` ile dolduracak şekilde daha sağlam hale getirildi.

### Hata Ayıklama ve Ortam Kurulumu
- **HTTPS Sorunlarının Giderilmesi:** Geliştirme ortamında backend'in `HTTPS` üzerinden başlamaması sorunu, `.NET geliştirme sertifikasının` **yönetici yetkileriyle** yeniden oluşturulması ve `dotnet run` komutuna `--launch-profile https` bayrağının eklenmesiyle kalıcı olarak çözüldü.
- **Boş Backend Logları Sorununun Çözümü:** Testler sırasında karşılaşılan "500 Internal Server Error" hatalarının backend konsolunda görünmemesi sorunu, **`ErrorHandlingMiddleware`**'e `ILogger` enjekte edilerek ve yakalanan hataların konsola yazdırılması sağlanarak giderildi. Bu, gelecekteki hata ayıklama süreçlerini büyük ölçüde hızlandıracaktır.
- **İş Mantığı Hatalarının Düzeltilmesi:** Loglama sayesinde, `GenerateUniqueSku` metodundaki bir `Substring` hatası ve `Product` repository'sindeki `AddAsync` tutarsızlığı gibi birçok gizli kalmış hata tespit edilip düzeltildi.

### Frontend İyileştirmeleri
- **Kullanıcı Geri Bildirimine Yanıt:** Kullanıcı testleri sonucunda, "Ürün Açıklaması" alanının yeni ürün eklerken zorunlu olması sağlandı. Ayrıca, `ProductsView`'deki `v-data-table`'a, ürün açıklamalarını şık bir şekilde gösteren **genişletilebilir satır (`show-expand`)** özelliği eklendi.

Bugünkü çalışmalar sonucunda, sadece yeni bir modülün backend'i tamamlanmakla kalmadı, aynı zamanda projenin genel kod kalitesi, güvenliği ve tutarlılığı önemli ölçüde artırıldı.

---

## 23 Temmuz 2025 Çarşamba


Bugün, Müşteri modülüne başlamadan önce projenin backend mimarisini temelden güçlendirmek ve geleceğe hazırlamak için kritik bir yapısal değişikliğe odaklanıldı. Mevcut veri erişim katmanı, **Unit of Work** deseni kullanılarak yeniden yapılandırıldı.

### Mimari Yeniden Yapılandırma (Refactoring)
- **Unit of Work Deseni Entegrasyonu:** Projenin en önemli mimari değişikliği yapıldı. Daha önce her Repository metodu kendi veritabanı kaydını (`SaveChangesAsync`) kendisi yapıyordu. Bu durum, gelecekte birbiriyle ilişkili birden fazla veritabanı işlemi (örn: stok düşürme ve bakiye güncelleme) gerektiğinde veri tutarlılığı riski oluşturuyordu.
  - Bu riski ortadan kaldırmak için `IUnitOfWork` arayüzü oluşturuldu.
  - Artık Repository'ler sadece değişiklikleri Entity Framework'ün "Change Tracker" mekanizmasında işaretliyor.
  - Tüm değişikliklerin tek bir transaction (işlem birimi) altında veritabanına kaydedilmesi sorumluluğu, merkezi olarak `UnitOfWork` sınıfına devredildi. Bu, "Ya Hep Ya Hiç" prensibini garanti altına alarak veri bütünlüğünü sağlar.

### Kod Kalitesi ve Hata Ayıklama
- **Proje Geneli Namespace Tutarlılığı:** Refactoring sırasında, proje katmanları (`Core.Domain`, `Application`, `Infrastructure`, `API.Web`) arasındaki `namespace` bildirimlerinde ve `using` ifadelerinde ciddi tutarsızlıklar olduğu tespit edildi.
  - Tüm `.cs` dosyaları (Entity Framework tarafından otomatik oluşturulan migration dosyaları dahil) sistematik olarak tarandı ve `namespace`'ler projenin yeni, temiz yapısına uygun hale getirildi.
  - Bu kapsamlı temizlik operasyonu, sayısız derleme hatasını çözdü ve projenin bakımını kolaylaştırarak kod kalitesini artırdı.

### Test ve Güvenlik
- **HTTPS Yapılandırması:** Geliştirme ortamında güvenlik standartlarını en başından uygulamak amacıyla, backend sunucusunun `HTTP` yerine **`HTTPS`** üzerinden hizmet vermesi sağlandı. Bunun için `launchSettings.json` dosyası yapılandırıldı ve `dotnet run` komutu `https` profilini kullanacak şekilde güncellendi.
- **Uçtan Uca Test:** Yapılan tüm bu köklü değişikliklerin ardından, Ürün modülünün tüm fonksiyonları (Listeleme, Ekleme, Güncelleme, Arşivleme ve Kalıcı Silme) frontend üzerinden uçtan uca test edildi. Tüm testler başarıyla tamamlandı ve mimarinin doğru çalıştığı doğrulandı.

Bugünkü çalışmalar sonucunda, projenin backend altyapısı artık endüstri standartlarına uygun, veri bütünlüğünü garanti eden ve gelecekteki modülleri eklemeye hazır, sağlam bir temele kavuşmuştur.

---

## 22 Temmuz 2025 Salı

Bugün frontend geliştirmesine odaklanıldı ve kullanıcı arayüzünün temelleri atıldı. Ayrıca backend tarafında test verileri eklendi.

### Frontend Kurulumu ve Yapılandırması
- **Proje Oluşturma:** Eski ve hatalı `vue-cli` tabanlı proje yapısı temizlendi. Yerine, modern ve hızlı **Vite** altyapısını kullanan yeni bir Vue.js projesi (`client-app`) sıfırdan oluşturuldu.
- **Kütüphane Entegrasyonu:** Projeye aşağıdaki temel kütüphaneler eklendi ve yapılandırıldı:
  - **Vuetify:** Modern ve şık bir görünüm için UI bileşen kütüphanesi.
  - **Axios:** Backend API'si ile iletişim kurmak için HTTP istemcisi.
  - **@mdi/font:** Vuetify ikonlarının doğru görüntülenmesi için ikon kütüphanesi.
  - **lodash.debounce:** Kullanıcı girdilerinde gereksiz API isteklerini önlemek için yardımcı kütüphane.
- **API Servisi:** Backend ile tüm `Product` endpoint'i iletişimini tek bir merkezden yönetmek için `ProductService.ts` dosyası oluşturuldu.

### Ana Arayüz (Layout) Geliştirmesi
- **Profesyonel Yerleşim:** Uygulamanın ana iskeleti (`App.vue`), solda modüller arası geçişi sağlayan bir navigasyon menüsü (`v-navigation-drawer`) ve üstte sayfa başlığını gösteren bir başlık çubuğu (`v-app-bar`) içerecek şekilde yeniden tasarlandı.
- **Görsel Hataların Giderilmesi:** Menünün açılıp kapanırken sayfa içeriğini kaydırması, gölge oluşturması ve butonu arkada bırakması gibi tüm görsel ve fonksiyonel hatalar düzeltildi.
- **Router Yapılandırması:** Vue Router (`router/index.ts`), `/` (Dashboard) ve `/products` (Ürünler) yollarını mantıksal olarak ayıracak şekilde güncellendi.

### Ürün Listeleme Sayfası (`HomeView.vue`)
- **Dinamik Tablo:** `v-data-table` bileşeni kullanılarak, backend'den gelen ürünleri listeleyen dinamik bir tablo oluşturuldu.
- **CORS Sorunu Çözümü:** Frontend (`localhost:5173`) ve backend (`localhost:7277`) arasındaki iletişimi engelleyen **CORS (Cross-Origin Resource Sharing)** hatası, backend `Program.cs` dosyasına gerekli politikalar eklenerek çözüldü.
- **Gelişmiş Kontroller:** Kullanıcının verilerle etkileşime geçebilmesi için aşağıdaki kontroller eklendi:
  - Arama çubuğu
  - Fiyat aralığına göre filtreleme
  - Aktif/pasif durumuna göre filtreleme
  - Fiyata, isme ve tarihe göre sıralama
- **Yeni Ürün Ekleme:** "Yeni Ürün" butonuna basıldığında, kullanıcıyı ürün ekleme formunun bulunduğu `/products/new` sayfasına yönlendirme işlevi eklendi. Bunun için `ProductFormView.vue` adında yeni bir sayfa oluşturuldu.

### Veritabanı İşlemleri
- **Dummy Veri (Data Seeding):** Uygulamanın daha "dolu" görünmesi ve paginasyon gibi özelliklerin test edilebilmesi için veritabanına Entity Framework Core'un "Data Seeding" özelliği kullanılarak **15 adet örnek ürün** eklendi.
- **Migration Sorunları:** Bu süreçte karşılaşılan `DbContextFactory` hatası ve `PRIMARY KEY` çakışması sorunları, fabrika sınıfı eklenerek ve veritabanı sıfırlanarak çözüldü.

### Kod Kalitesi ve Refactoring
- `HomeView.vue` dosyasında, Vuetify bileşenlerinin tipleriyle ilgili ortaya çıkan TypeScript hataları giderilerek kodun tip güvenliği artırıldı.
- Proje genelinde isimlendirme tutarlılığını artırmak ve kodun okunabilirliğini iyileştirmek için, ürünler sayfasını temsil eden `HomeView.vue` dosyası, yaptığı işi daha net yansıtan `ProductsView.vue` olarak yeniden adlandırıldı. Router konfigürasyonu da bu değişikliğe uygun olarak güncellendi.

---

## 21 Temmuz 2025 Pazartesi

Bugün tamamen backend API'sinin `Product` modülü üzerindeki işlevselliği ve mimari kalitesini artırmaya odaklanıldı.

### Backend API Geliştirmeleri (`Product` Modülü)
- **Akıllı PUT (Kısmi Güncelleme):** `Update` işlemleri, frontend'den sadece değişen verinin gönderilmesine olanak tanıyan "Akıllı PUT" mantığıyla yeniden yazıldı. Bu, çok kullanıcılı sistemlerde veri kaybını önleyen kritik bir özelliktir.
- **Şartlı Validasyon:** `UpdateProductCommandValidator`, bu yeni esnek güncelleme yapısıyla uyumlu çalışması için, alanların sadece `null` olmadığında doğrulama yapmasını sağlayan şartlı validasyon kuralları (`When()`) ile güncellendi.
- **DTO Entegrasyonu:** API'nin dış dünyaya doğrudan veritabanı nesnelerini (Entity) açmasını engellemek için `ProductDto` (Data Transfer Object) katmanı oluşturuldu. Bu, güvenlik ve esneklik açısından en iyi pratiktir.
- **Mapping Altyapısı:** Veritabanı nesnelerini DTO'lara dönüştürme işlemini merkezileştirmek ve kod tekrarını önlemek için `ProductMappings.cs` dosyası oluşturuldu.
- **API Tutarlılığı:** `Create`, `Update`, `GetAll` ve `GetById` dahil olmak üzere tüm `Product` endpoint'lerinin, frontend'e her zaman tutarlı ve güvenli olan `ProductDto` nesnesini dönmesi sağlandı.
- **Gelişmiş Filtreleme ve Sıralama:** `GET /api/products` endpoint'i, `search`, `minPrice`, `maxPrice`, `sortBy`, `sortOrder` gibi birden fazla parametreyi aynı anda alıp, buna uygun dinamik ve optimize SQL sorguları üretebilecek şekilde `IProductRepository` katmanında `IQueryable` kullanılarak geliştirildi.
- **Proje Planı:** Yapılan tüm bu backend geliştirmeleri ve yeni hedefler, `docs/erp_proje_plan.md` dosyasına işlenerek proje dokümantasyonu güncel tutuldu.

**Kod Örneği: "Akıllı" Kısmi Güncelleme Mantığı (`UpdateProductCommandHandler.cs`)**
```csharp
// Sadece frontend'den gelen (null olmayan) veriler güncellenir.
// Bu, "Akıllı Güncelleme" mantığıdır ve veri kaybını önler.
public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
{
    var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.Id);
    // ...
    product.Name = request.Name ?? product.Name;
    product.Description = request.Description ?? product.Description;
    product.Price = request.Price ?? product.Price;
    // ...
    _unitOfWork.ProductRepository.Update(product);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
}
```

**Kod Örneği: IQueryable ile Dinamik Filtreleme (`ProductRepository.cs`)**
```csharp
public async Task<IEnumerable<Product>> GetAllAsync(/* filtre parametreleri */)
{
    // Önce bir sorgu taslağı oluşturulur.
    var query = _context.Products.AsQueryable();

    // Gelen her filtre parametresine göre, bu taslak üzerine yeni koşullar eklenir.
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        query = query.Where(p => p.Name.Contains(searchTerm));
    }
    if (minPrice.HasValue)
    {
        query = query.Where(p => p.Price >= minPrice.Value);
    }
    // ...

    // Sorgu, sadece en sonda veritabanına gönderilir.
    return await query.ToListAsync();
}
```