import apiClient from './axios';

// TODO: Backend DTO'ları ile eşleşen arayüzleri tanımla
export interface InvoiceDto {
  id: string;
  invoiceNumber: string;
  customerName: string;
  invoiceDate: string;
  dueDate: string;
  totalAmount: number;
  status: number; // 0: Draft, 1: Sent, 2: Paid, 3: Overdue
}

export interface InvoiceFilterParams {
  customerId?: string;
  startDate?: string;
  endDate?: string;
  status?: number;
}

class InvoiceService {
  private API_URL = '/invoices';

  /**
   * Faturaları filtreleyerek getirir.
   * @param params Filtreleme parametreleri
   */
  getAll(params?: InvoiceFilterParams) {
    return apiClient.get<InvoiceDto[]>(this.API_URL, { params });
  }

  /**
   * Belirtilen ID'ye sahip tek bir faturayı getirir.
   * @param id Fatura ID'si
   */
  getById(id: string) {
    return apiClient.get<InvoiceDto>(`${this.API_URL}/${id}`);
  }
}

export default new InvoiceService();
