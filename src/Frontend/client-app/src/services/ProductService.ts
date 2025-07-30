import axios from 'axios';

const API_URL = 'https://localhost:7277/api/products'; // Backend API adresimiz

// Backend'deki ProductDto'ya karşılık gelen bir interface tanımlayalım
export interface ProductDto {
  id: number;
  name: string;
  description?: string;
  price: number;
  currencyId: number;
  currencyCode?: string;
  stockQuantity: number;
  sku: string;
  isActive: boolean;
}

// Gelişmiş filtreleme için parametreleri tanımlayan bir interface
export interface ProductFilterParams {
  search?: string;
  minPrice?: number;
  maxPrice?: number;
  includeInactive?: boolean;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

// Create işlemi için gönderilecek veri tipi.
export interface CreateProductPayload {
  name: string;
  description?: string;
  price: number;
  stockQuantity: number;
  currencyId: number;
}

// Update işlemi için gönderilecek veri tipi. ID hariç tüm alanlar opsiyonel.
export type UpdateProductPayload = Partial<Omit<ProductDto, 'id' | 'sku' | 'currencyCode'>>;


class ProductService {
  getAll(params: ProductFilterParams = {}) {
    return axios.get<ProductDto[]>(API_URL, { params });
  }

  getById(id: number) {
    return axios.get<ProductDto>(`${API_URL}/${id}`);
  }

  create(product: CreateProductPayload) {
    return axios.post<ProductDto>(API_URL, product);
  }

  update(id: number, product: UpdateProductPayload) {
    // PUT'u PATCH olarak güncelliyoruz.
    return axios.patch<ProductDto>(`${API_URL}/${id}`, product);
  }

  // Bir ürünü arşivleyen (soft delete) metot
  archive(id: number) {
    return axios.delete(`${API_URL}/${id}`);
  }

  // Bir ürünü geri yükleyen (tekrar aktif yapan) metot
  restore(id: number) {
    // Update endpoint'ini kullanarak sadece isActive durumunu güncelliyoruz
    return axios.patch<ProductDto>(`${API_URL}/${id}`, { isActive: true });
  }

  // Bir ürünü kalıcı olarak silen (hard delete) metot
  hardDelete(id: number) {
    return axios.delete(`${API_URL}/hard/${id}`);
  }
}

export default new ProductService();
