import axios from 'axios';
import { useAuthStore } from '@/stores/auth.store';
import { supabase } from '@/lib/supabaseClient';

// Axios için varsayılan bir instance oluştur
const apiClient = axios.create({
  baseURL: `${import.meta.env.VITE_API_BASE_URL}/api`, // API'nin temel adresi .env dosyasından alınıp sonuna /api ekleniyor
});

// Request Interceptor
// Bu, her istek gönderilmeden önce araya girer.
apiClient.interceptors.request.use(
  (config) => {
    // Supabase'in localStorage oturum formatını ve olası varyantlarını kontrol et
    const supabaseUrl = import.meta.env.VITE_SUPABASE_URL;
    if (supabaseUrl) {
      const projectId = supabaseUrl.split('.')[0].replace('https://', '');
      const supabaseSessionKey = `sb-${projectId}-auth-token`;
      const sessionDataString = localStorage.getItem(supabaseSessionKey);
      if (sessionDataString) {
        try {
          const sessionData = JSON.parse(sessionDataString);
          const token = sessionData?.currentSession?.access_token
            || sessionData?.access_token
            || sessionData?.session?.access_token;
          if (token) {
            config.headers['Authorization'] = `Bearer ${token}`;
            return config;
          }
        } catch (_) {
          // JSON parse hatasını sessizce geç
        }
      }
    }
    // Son çare: Pinia store'un yazdığı fallback anahtar
    const fallbackToken = localStorage.getItem('access_token');
    if (fallbackToken) {
      config.headers['Authorization'] = `Bearer ${fallbackToken}`;
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
    const authStore = useAuthStore();
    const originalRequest = error.config;
    if (error.response && error.response.status === 401 && !originalRequest.__isRetry) {
      try {
        // Token yenilemeyi dene
        const { data, error: refreshErr } = await supabase.auth.refreshSession();
        if (refreshErr) throw refreshErr;
        if (data.session?.access_token) {
          authStore.setSession(data.session); // Pinia state ve localStorage güncellensin
          originalRequest.__isRetry = true;
          originalRequest.headers = originalRequest.headers || {};
          originalRequest.headers['Authorization'] = `Bearer ${data.session.access_token}`;
          return apiClient(originalRequest);
        }
      } catch (_) {
        // refresh başarısızsa normal akışla devam
      }
    }
    // 2. kez 401 veya refresh başarısız ise logout
    if (error.response && error.response.status === 401) {
      try { await authStore.logout(); } catch {}
    }
    return Promise.reject(error);
  }
);

export default apiClient;
