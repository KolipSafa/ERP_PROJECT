# Frontend Mimarisi İyileştirme Planı

**Tarih:** 25 Temmuz 2025

**Mevcut Durum:**
Frontend projesinde, veri yapılarını tanımlayan TypeScript `interface`'leri (DTO'lar, Payload'lar vb.) bu tipleri kullanan servis dosyalarının (`ProductService.ts`, `CustomerService.ts`) içinde tanımlanmıştır. Bu, küçük modüller için hızlı bir başlangıç sağlasa da, projenin ölçeği büyüdükçe aşağıdaki sorunlara yol açabilir:
- **Tek Sorumluluk Prensibi (SRP) İhlali:** Servis dosyaları hem API iletişiminden hem de veri yapısı tanımından sorumlu hale gelir.
- **Tekrarlanan Tanımlar ve Bağımlılıklar:** Aynı veri tipine birden fazla component veya store'un ihtiyaç duyması durumunda, anlamsal olarak yanlış bağımlılıklar (`component` -> `service` sadece tip için) oluşabilir.
- **Bakım Zorluğu:** Tiplerin proje geneline dağılması, onları bulmayı ve yönetmeyi zorlaştırır.

**Hedef:**
Backend'deki Clean Architecture disiplinini frontend'e de yansıtarak, daha ölçeklenebilir, bakımı kolay ve profesyonel bir yapı kurmak.

**Aksiyon Planı:**
1.  `src/` altında `types` adında yeni bir ana klasör oluşturulacak.
2.  Bu klasör içinde `DTOs`, `Payloads`, `Enums` gibi alt klasörler oluşturularak tipler kategorize edilecek.
3.  Mevcut `ProductService.ts` ve geliştirilecek `CustomerService.ts` içindeki tüm `interface` ve `type` tanımları, bu yeni klasör yapısındaki ilgili dosyalara taşınacak.
    - Örnek: `ProductDto` interface'i `src/types/DTOs/ProductDto.ts` dosyasına taşınacak.
4.  Servisler, component'ler ve diğer dosyalar, tipleri artık bu merkezi `types` klasöründen `import` edecekler.
    - Örnek: `import { type ProductDto } from '@/types/DTOs/ProductDto';`

**Zamanlama:**
Bu refactoring işlemi, Müşteri modülünün ilk fonksiyonel versiyonu tamamlandıktan sonra, hem Ürün hem de Müşteri modüllerini kapsayacak şekilde tek seferde yapılacaktır. Bu, mevcut acil iş akışını bölmeden teknik borcun planlı bir şekilde ödenmesini sağlar.
