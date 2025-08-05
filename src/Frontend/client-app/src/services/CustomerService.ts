import apiClient from './axios';
import { supabase } from '@/lib/supabaseClient'; // Supabase client'ı import et

const API_URL = '/customers'; // Müşteri API adresi

/**
 * Backend'deki CustomerDto.cs ile %100 uyumlu, doğrulanmış TypeScript interface'i.
 * Bu, frontend ve backend arasındaki veri sözleşmesini tanımlar.
 */
export interface CustomerDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  companyId?: string; // Eklendi
  companyName?: string;
  taxNumber?: string;
  address?: string;
  phoneNumber?: string;
  email?: string;
  balance: number;
  isActive: boolean;
  isAccountActive: boolean; // Eklendi
}

/**
 * Müşteri listesini filtrelemek için kullanılacak parametreleri tanımlar.
 * Metotlara tek bir obje olarak geçilerek kod okunabilirliğini artırır.
 */
export interface CustomerFilterParams {
  searchTerm?: string;
  includeInactive?: boolean;
  sortBy?: string;
  isDescending?: boolean;
}

/**
 * Yeni bir müşteri oluştururken gönderilecek veri yapısı (payload).
 * TypeScript'in `Omit` özelliği kullanılarak, backend tarafından yönetilen
 * ('id', 'fullName', 'isActive', 'balance') alanlar bu tipten çıkarılmıştır.
 * Bu, frontend'in yanlışlıkla bu alanları göndermesini engelleyen bir güvenlik katmanıdır.
 */
/**
 * Yeni bir müşteri oluştururken gönderilecek veri yapısı (payload).
 * Backend'deki `CreateCustomerCommand` ile eşleşir.
 */
export interface CreateCustomerPayload {
  applicationUserId: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  companyId?: string;
  phoneNumber?: string;
}

/**
 * Bir müşteriyi güncellerken gönderilecek veri yapısı (payload).
 * `Partial` ile tüm alanlar opsiyonel hale getirilmiştir, çünkü PATCH işleminde
 * sadece değişen alanlar gönderilir.
 * `Omit` ile 'id' ve 'fullName' gibi asla güncellenmemesi gereken alanlar çıkarılmıştır.
 */
export type UpdateCustomerPayload = Partial<Omit<CustomerDto, 'id' | 'fullName' | 'companyName'>>;

/**
 * Bir müşteriyi sisteme davet ederken Edge Function'a gönderilecek veri yapısı.
 */
export interface InviteCustomerPayload {
  email: string;
  data: {
    first_name: string;
    last_name: string;
    company_id?: string;
  }
}

/**
 * Müşteri verileriyle ilgili tüm API iletişimini merkezi bir yerden yöneten servis sınıfı.
 * Bu, sorumlulukların ayrılması, kod tekrarının önlenmesi ve bakım kolaylığı sağlar.
 */
class CustomerService {

  /**
   * Bir müşteriyi sisteme davet etmek için 'invite-user' Edge Function'ını çağırır.
   * @param payload InviteCustomerPayload tipinde davet verisi
   * @returns Edge Function'dan dönen sonucu içeren bir Promise
   */
  async inviteCustomer(payload: InviteCustomerPayload) {
    const { data, error } = await supabase.functions.invoke('invite-user', {
      body: { email: payload.email, data: payload.data },
    });

    if (error) {
      throw new Error(error.message);
    }
    return data;
  }

  /**
   * Filtre parametrelerine göre müşteri listesini getirir.
   * @param params CustomerFilterParams objesi
   * @returns CustomerDto dizisi içeren bir Promise
   */
  getAll(params: CustomerFilterParams = {}) {
    return apiClient.get<CustomerDto[]>(API_URL, { params });
  }

  /**
   * Belirtilen ID'ye sahip tek bir müşteriyi getirir.
   * @param id Müşteri ID'si (string olarak)
   * @returns Tek bir CustomerDto içeren bir Promise
   */
  getById(id: string) {
    return apiClient.get<CustomerDto>(`${API_URL}/${id}`);
  }

  /**
   * Yeni bir müşteri oluşturur. (Bu metot artık sadece davet kabul edildikten sonra kullanılacak)
   * @param customer CreateCustomerPayload tipinde müşteri verisi
   * @returns Oluşturulan yeni CustomerDto'yu içeren bir Promise
   */
  create(customer: CreateCustomerPayload) {
    return apiClient.post<CustomerDto>(API_URL, customer);
  }

  /**
   * Mevcut bir müşterinin bilgilerini kısmi olarak günceller (PATCH).
   * @param id Güncellenecek müşteri ID'si
   * @param customer UpdateCustomerPayload tipinde güncellenecek alanları içeren veri
   * @returns Güncellenmiş CustomerDto'yu içeren bir Promise
   */
  update(id: string, customer: UpdateCustomerPayload) {
    return apiClient.patch<CustomerDto>(`${API_URL}/${id}`, customer);
  }

  /**
   * Bir müşteriyi arşivler (soft delete).
   * @param id Arşivlenecek müşteri ID'si
   * @returns Başarılı olursa 204 No Content döner.
   */
  archive(id: string) {
    return apiClient.delete(`${API_URL}/${id}`);
  }

  /**
   * Arşivlenmiş bir müşteriyi geri yükler (tekrar aktif hale getirir).
   * Bu, mevcut update metodunu akıllıca yeniden kullanır.
   * @param id Geri yüklenecek müşteri ID'si
   * @returns Güncellenmiş CustomerDto'yu içeren bir Promise
   */
  restore(id: string) {
    return this.update(id, { isActive: true });
  }
}

// Singleton pattern: Uygulama boyunca bu servisin tek bir örneğinin kullanılmasını sağlar.
export default new CustomerService();
