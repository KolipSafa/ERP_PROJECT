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
    // Pinia store'dan token almak yerine, Supabase'in localStorage'da sakladığı
    // oturum bilgisini doğrudan okumak daha güvenilirdir.
    // Bu, state'in henüz yüklenmediği "race condition" durumlarını engeller.
    const supabaseUrl = import.meta.env.VITE_SUPABASE_URL;
    if (!supabaseUrl) {
      console.error("Supabase URL (VITE_SUPABASE_URL) is not defined in .env file.");
      return config;
    }
    
    // URL'den Proje ID'sini çıkar (örn: https://<proje-id>.supabase.co)
    const projectId = supabaseUrl.split('.')[0].replace('https://', '');
    const supabaseSessionKey = `sb-${projectId}-auth-token`;
    const sessionDataString = localStorage.getItem(supabaseSessionKey);
    
    if (sessionDataString) {
      const sessionData = JSON.parse(sessionDataString);
      const token = sessionData.access_token;
      if (token) {
        config.headers['Authorization'] = `Bearer ${token}`;
      }
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
