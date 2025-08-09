# **ERP Projesi Geliştirme Planı ve Günlüğü**

**Belge Amacı:** Bu belge, ERP projesinin başlangıcından itibaren tüm geliştirme aşamalarını, alınan mimari kararları, tamamlanan işleri ve gelecek yol haritasını içeren canlı bir proje günlüğüdür. Projeye dahil olan her geliştiricinin, geçmişi ve geleceği tek bir yerden takip edebilmesini hedefler.

---

### **1. Mevcut Durum ve Stratejik Değişiklik**

Proje, başlangıçta .NET 9 Clean Architecture ve Vue.js 3 temelinde, SQL Server ve OpenIddict kullanılarak geliştirilmiştir. Ancak, `subject claim` hatasıyla ortaya çıkan kimlik doğrulama sorunları ve yerel geliştirme ortamının (localhost) getirdiği kısıtlamalar, projenin canlıya alınmasını engellemektedir.

Bu engelleri aşmak ve projeyi daha modern, yönetilebilir ve ölçeklenebilir bir yapıya kavuşturmak amacıyla **hibrit bulut mimarisine geçiş** kararı alınmıştır.

---

### **2. Hedef Hibrit Mimari**

*   **Backend (.NET API):** Mevcut iş mantığı korunacak, **Render** üzerinde barındırılacak.
*   **Frontend (Vue.js):** Mevcut arayüz korunacak, **Vercel** üzerinde barındırılacak.
*   **Veritabanı (DBaaS):** SQL Server yerine **Supabase Postgres** kullanılacak.
*   **Kimlik Doğrulama (Auth BaaS):** OpenIddict yerine **Supabase Auth** kullanılacak.

#### **2.1. Rol Tabanlı Yetkilendirme Modeli (RBAC)**

Proje, iki temel kullanıcı rolü üzerine inşa edilecektir:
*   **Yönetici (Admin):** Sistemin tüm modüllerine (Envanter, Müşteri Yönetimi, Teklifler, Ayarlar) tam erişim sahibi olan şirket içi kullanıcılardır. Bu kullanıcılar davetle değil, kontrollü bir şekilde (örn: Supabase arayüzü veya özel bir script ile) oluşturulur.
*   **Müşteri (Customer):** Sadece kendileriyle ilgili teklifleri görüntüleyebilen, onaylayabilen veya değişiklik talep edebilen dış kullanıcılardır. Müşteriler, sadece yöneticiler tarafından panel üzerinden sisteme davet edilebilir.

Bu roller, Supabase Auth JWT'lerinin `app_metadata` alanında saklanacak ve hem Backend (.NET API'de `[Authorize(Roles = "...")]`) hem de Frontend (Vue'da `v-if`) katmanlarında yetki denetimi için kullanılacaktır. Veri modelinin temizliği ve güvenliği için yönetici profilleri `Administrators`, müşteri profilleri ise `Customers` tablosunda ayrı olarak tutulacaktır.

---

### **3. Geliştirme Yol Haritası**

Proje, iki ana ve birbiriyle paralel ilerleyecek aşamada tamamlanacaktır. Her faz, kendi içinde detaylı adımlara ayrılmıştır ve bu adımlar ilgili plan dokümanlarında açıklanmıştır.

#### **Aşama 5 (Yeniden Tanımlandı): Mimari Dönüşüm ve Canlıya Hazırlık** `✅ Tamamlandı`

**Hedef:** Projenin teknik altyapısını yerel bağımlılıklardan kurtarıp tamamen bulut tabanlı hibrit mimariye taşımak, temel iş akışlarını (kullanıcı daveti vb.) tamamlamak ve canlıya çıkış için stabil bir temel oluşturmak.

*   **Detaylı Plan:** Bu aşamanın tüm teknik adımları, `docs/01_architectural_migration_plan.md` dosyasında belgelenmiştir.
*   **Öncelik:** **YÜKSEK**.
*   **Özet:** Bu aşama kapsamında backend API'si ve frontend uygulaması, Supabase (Postgres & Auth) ile entegre olacak şekilde başarıyla güncellendi. Backend'deki .NET paket uyumsuzlukları giderildi. Supabase yönetici işlemleri için `supabase-csharp` kütüphanesinin yarattığı kararsızlıklardan kaçınmak amacıyla, doğrudan Supabase Management API'sine istek atan `HttpClient` tabanlı bir servis yazılarak mimari sağlamlaştırıldı. Frontend'de, davet linkiyle gelen kullanıcıların şifre belirlemesi ve sisteme doğru bir şekilde kaydedilmesi için karmaşık bir kimlik doğrulama akışı (Supabase Edge Function, Vue Router Guards, Pinia State Management) implemente edildi. Müşteri kaydı ve Supabase'deki `status` güncellemesi artık tek bir güvenli backend işlemiyle hallediliyor.
*   **Mevcut Durum:** Müşteri davet, şifre belirleme ve aktivasyon akışı başarıyla tamamlanmıştır. Şifresini belirleyen kullanıcı, `status`'unun `active` olarak güncellenmesinin ardından başarılı bir şekilde müşteri paneline yönlendirilmektedir. Bu aşama tamamlanmıştır.

#### **Aşama 6 (Yeniden Tanımlandı): Gelişmiş ERP İşlevleri ve Müşteri Etkileşimi** `🎯 Devam Ediyor`

**Hedef:** Projeyi, rezerve envanter takibi yapabilen, müşterilerin teklifleri onaylayıp karşı teklif sunabildiği ve bu süreçlerin sonunda otomatik fatura oluşturan, iş akışları zengin bir ERP platformuna dönüştürmek.

*   **Detaylı Plan:** Bu aşamanın tüm iş mantığı ve arayüz geliştirme adımları, `docs/02_erp_feature_enhancement_plan.md` dosyasında belgelenmiştir.
*   **Öncelik:** **YÜKSEK**. Mimari dönüşüm tamamlandığı için sıradaki ana hedeftir.
*   **Sıralama ve Mevcut Durum:**
    1.  **Faz 2: Müşteri Etkileşimi ve Fatura Akışı** `✅ Tamamlandı`
    2.  Faz 1: Gelişmiş Envanter Yönetimi `📝 Planlandı`
*   **Teknik Not (Çözülemeyen Kritik Sorun):** Bu fazın geliştirilmesi sırasında, .NET API'sindeki `[Authorize]` attribute'ü ile korunan endpoint'lerin, geçerli bir JWT token gönderilmesine rağmen `System.UnauthorizedAccessException: User ID could not be found in the token` hatası verdiği tespit edilmiştir. Bu durum, `HttpContext.User` nesnesinin kimlik bilgileriyle (claims) doğru bir şekilde doldurulmadığını göstermektedir.
    *   **Denenen Başarısız Çözümler:**
        1.  **Claim Mapping:** `Program.cs` içinde, Supabase'in `sub` claim'ini .NET'in standart `ClaimTypes.NameIdentifier`'ına manuel olarak map etme denendi.
        2.  **Doğrudan Claim Okuma:** `TekliflerController`'daki `CurrentUserId` özelliği, `NameIdentifier` yerine doğrudan `sub` claim'ini okuyacak şekilde değiştirildi.
        3.  **`appsettings.json` Yapılandırması:** JWT doğrulaması için gerekli olan `Authority` ve `Audience` bilgileri `appsettings.json` dosyasına eklendi.
        4.  **Ortam Değişkeni Düzeltmesi:** Ayarların `appsettings.Development.json` tarafından ezildiği düşünülerek, `Jwt` ayarları bu dosyaya taşındı.
        5.  **Strongly-Typed Yapılandırma:** Ayarların okunmasını garanti altına almak için bir `JwtSettings` sınıfı oluşturuldu ve yapılandırma bu sınıf üzerinden `TokenValidationParameters`'a manuel olarak atandı.
        6.  **Middleware Sıralaması:** `app.UseAuthentication()` ve `app.UseAuthorization()`'ın `app.MapControllers()`'dan önce doğru sırada olduğu teyit edildi.
    *   **Mevcut Durum:** Yukarıdaki denemelerin hiçbiri sorunu çözememiştir. Bu, projenin müşteri tarafındaki aksiyonlarının (teklif onayı/reddi) çalışmasını engellemektedir. **Kullanıcının kararı doğrultusunda, bu konunun çözümü projedeki diğer tüm fonksiyonel geliştirmeler tamamlandıktan sonra, en son ve en öncelikli teknik borç olarak ele alınacaktır.** Geliştirmeye devam edebilmek için, `TekliflerController`'daki müşteri aksiyonu endpoint'leri geçici olarak `[AllowAnonymous]` ile işaretlenebilir veya frontend'den bu özellikler geçici olarak kaldırılabilir.

---

### **4. Tamamlanan Aşamalar**

#### **Aşama 1-4: Temel Modüllerin Geliştirilmesi** `✅ Tamamlandı`

*   **Özet:** Envanter, Müşteri, Teklif ve Ayarlar modüllerinin ilk versiyonları geliştirildi. Bu aşamada oluşturulan iş mantığı, yeni mimaride de korunacaktır.

---

### **5. Gelecek Vizyonu**

Bu iki ana aşama tamamlandıktan sonra değerlendirilecek potansiyel geliştirmeler:

*   **Aşama 7: Kargo Takip Modülü** `🧊 Gelecek Vizyonu`
*   **Aşama 8: Raporlama ve Analiz Paneli** `🧊 Gelecek Vizyonu`
*   **Aşama 9: Çoklu Dil Desteği** `🧊 Gelecek Vizyonu`
