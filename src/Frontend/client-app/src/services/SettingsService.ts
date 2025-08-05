import apiClient from './axios';
import type { CompanyDto } from './dtos/CompanyDto';
import type { CurrencyDto } from './dtos/CurrencyDto';

const API_URL = '/Settings';

class SettingsService {
  // Company methods
  getCompanies() {
    return apiClient.get<CompanyDto[]>(`${API_URL}/companies`);
  }

  createCompany(company: Omit<CompanyDto, 'id' | 'isActive'>) {
    return apiClient.post<CompanyDto>(`${API_URL}/companies`, company);
  }

  updateCompany(id: string, company: Partial<CompanyDto>) {
    return apiClient.put<CompanyDto>(`${API_URL}/companies/${id}`, company);
  }

  deleteCompany(id: string) {
    return apiClient.delete(`${API_URL}/companies/${id}`);
  }

  // Currency methods
  getCurrencies() {
    return apiClient.get<CurrencyDto[]>(`${API_URL}/currencies`);
  }

  createCurrency(currency: Omit<CurrencyDto, 'id' | 'isActive'>) {
    return apiClient.post<CurrencyDto>(`${API_URL}/currencies`, currency);
  }

  updateCurrency(id: number, currency: Partial<CurrencyDto>) {
    return apiClient.put<CurrencyDto>(`${API_URL}/currencies/${id}`, currency);
  }

  deleteCurrency(id: number) {
    return apiClient.delete(`${API_URL}/currencies/${id}`);
  }
}

export default new SettingsService();
