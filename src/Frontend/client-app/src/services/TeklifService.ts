import apiClient from './axios';
import type { 
  TeklifDto, 
  CreateTeklifPayload, 
  UpdateTeklifPayload
} from './dtos/TeklifDtos';

const API_URL = '/teklifler';

/**
 * Teklifler için filtreleme ve sıralama parametreleri.
 * Backend'deki GetAllTekliflerQuery ile eşleşir.
 */
export interface TeklifFilterParams {
  musteriId?: string;
  baslangicTarihi?: string;
  bitisTarihi?: string;
  durum?: number;
  includeInactive?: boolean;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

/**
 * Teklifler ve teklif satırları ile ilgili tüm API iletişimini yöneten servis.
 */
class TeklifService {
  /**
   * Tüm teklifleri, belirtilen filtre ve sıralama seçenekleriyle getirir.
   */
  getAll(params?: TeklifFilterParams) {
    return apiClient.get<TeklifDto[]>(API_URL, { params });
  }

  /**
   * Belirtilen ID'ye sahip tek bir teklifi tüm detaylarıyla getirir.
   * @param id Teklif ID'si
   */
  getById(id: string) {
    return apiClient.get<TeklifDto>(`${API_URL}/${id}`);
  }

  /**
   * Yeni bir teklif oluşturur.
   * @param payload Yeni teklif verileri
   */
  create(payload: CreateTeklifPayload) {
    return apiClient.post<TeklifDto>(API_URL, payload);
  }

  /**
   * Mevcut bir teklifi tümüyle günceller.
   * @param id Güncellenecek teklifin ID'si
   * @param payload Teklifin son halini içeren veri
   */
  update(id: string, payload: UpdateTeklifPayload) {
    return apiClient.put<TeklifDto>(`${API_URL}/${id}`, payload);
  }

  /**
   * Bir teklifi arşivler (soft delete).
   * @param id Arşivlenecek teklifin ID'si
   */
  archive(id: string) {
    return apiClient.delete(`${API_URL}/${id}`);
  }

  /**
   * Arşivlenmiş bir teklifi geri yükler.
   * @param id Geri yüklenecek teklifin ID'si
   */
  restore(id: string) {
    return apiClient.post(`${API_URL}/${id}/restore`);
  }
}

export default new TeklifService();
