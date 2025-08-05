import axios from 'axios';
import { useAuthStore } from '@/stores/auth.store';

// Axios için varsayılan bir instance oluştur
const apiClient = axios.create({
  baseURL: `${import.meta.env.VITE_API_BASE_URL}/api`, // API'nin temel adresi .env dosyasından alınıp sonuna /api ekleniyor
});

// Request Interceptor
// Bu, her istek gönderilmeden önce araya girer.
apiClient.interceptors.request.use(
  (config) => {
    const authStore = useAuthStore();
    const token = authStore.accessToken;

    if (token) {
      // Eğer token varsa, Authorization başlığını ekle
      config.headers['Authorization'] = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response Interceptor
// Bu, her cevap alındıktan sonra araya girer.
apiClient.interceptors.response.use(
  (response) => {
    // Başarılı cevapları doğrudan geri döndür
    return response;
  },
  async (error) => {
    // Eğer 401 (Unauthorized) hatası alınırsa, bu genellikle token'ın süresinin dolduğu anlamına gelir.
    // Kullanıcıyı güvenli bir şekilde logout yapıp login sayfasına yönlendiriyoruz.
    if (error.response && error.response.status === 401) {
      const authStore = useAuthStore();
      authStore.logout();
    }
    
    // Diğer tüm hataları olduğu gibi geri döndür.
    return Promise.reject(error);
  }
);

export default apiClient;
