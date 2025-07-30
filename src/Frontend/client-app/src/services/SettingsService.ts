import axios from 'axios';
import type { CompanyDto } from './dtos/CompanyDto';
import type { CurrencyDto } from './dtos/CurrencyDto';

const API_URL = 'https://localhost:7277/api/Settings';

class SettingsService {
  // Company methods
  getCompanies() {
    return axios.get<CompanyDto[]>(`${API_URL}/companies`);
  }

  createCompany(company: Omit<CompanyDto, 'id' | 'isActive'>) {
    return axios.post<CompanyDto>(`${API_URL}/companies`, company);
  }

  updateCompany(id: string, company: Partial<CompanyDto>) {
    return axios.put<CompanyDto>(`${API_URL}/companies/${id}`, company);
  }

  deleteCompany(id: string) {
    return axios.delete(`${API_URL}/companies/${id}`);
  }

  // Currency methods
  getCurrencies() {
    return axios.get<CurrencyDto[]>(`${API_URL}/currencies`);
  }

  createCurrency(currency: Omit<CurrencyDto, 'id' | 'isActive'>) {
    return axios.post<CurrencyDto>(`${API_URL}/currencies`, currency);
  }

  updateCurrency(id: number, currency: Partial<CurrencyDto>) {
    return axios.put<CurrencyDto>(`${API_URL}/currencies/${id}`, currency);
  }

  deleteCurrency(id: number) {
    return axios.delete(`${API_URL}/currencies/${id}`);
  }
}

export default new SettingsService();
