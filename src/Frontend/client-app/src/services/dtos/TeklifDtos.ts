// D:\yazilim_projelerim\ERP_PROJECT\src\Frontend\client-app\src\services\dtos\TeklifDtos.ts

/**
 * Teklif satırını temsil eden DTO.
 * Backend'deki `TeklifSatiriDto.cs` ile eşleşir.
 */
export interface TeklifSatiriDto {
  id: string; // Guid
  urunId: number;
  urunAdi: string;
  aciklama: string;
  miktar: number;
  birimFiyat: number;
  toplam: number;
}

/**
 * Ana teklif nesnesini temsil eden DTO.
 * Backend'deki `TeklifDto.cs` ile eşleşir.
 */
export interface TeklifDto {
  id: string; // Guid
  teklifNumarasi: string;
  musteriId: string; // Guid
  musteriAdi: string;
  teklifTarihi: string; // ISO 8601 formatında tarih (örn: "2025-07-29T10:00:00Z")
  gecerlilikTarihi: string; // ISO 8601 formatında tarih
  toplamTutar: number;
  currencyId: number;
  currencyCode?: string;
  durum: string; // Enum'ın string hali (örn: "Hazırlanıyor")
  isActive: boolean;
  teklifSatirlari: TeklifSatiriDto[];
}

// --- Payload Tipleri ---

/**
 * Yeni bir teklif oluştururken gönderilecek veri yapısı.
 */
export interface CreateTeklifPayload {
  musteriId: string;
  teklifTarihi: string;
  gecerlilikTarihi: string;
  currencyId: number;
  teklifSatirlari: {
    urunId: number;
    aciklama?: string;
    miktar: number;
    birimFiyat: number;
  }[];
}

/**
 * Mevcut bir teklifi güncellerken gönderilecek veri yapısı.
 * Artık backend tüm satırları bu tek komutla yönetiyor.
 */
export interface UpdateTeklifPayload {
  musteriId: string;
  teklifTarihi: string;
  gecerlilikTarihi: string;
  currencyId: number;
  durum: number;
  isActive: boolean;
  teklifSatirlari: {
    id?: string; // Mevcut satırlar için ID gönderilir, yeniler için gönderilmez.
    urunId: number;
    aciklama: string;
    miktar: number;
    birimFiyat: number;
  }[];
}
