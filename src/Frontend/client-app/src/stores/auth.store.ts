import { defineStore } from 'pinia';
import { jwtDecode } from 'jwt-decode';
import router from '@/router';
import AuthService from '@/services/AuthService';
import type { LoginCredentials } from '@/services/dtos/AuthDtos';
import { supabase } from '@/lib/supabaseClient';

// Kullanıcı bilgilerini ve rollerini içeren arayüz
interface User {
  id: string;
  name: string;
  email: string;
  roles: string[];
  aud: string;
  status?: 'invited' | 'active'; // Olası durumları belirtelim
  // user_metadata'yı da içerebilecek esnek bir yapı
  [key: string]: any;
}

// Store'un state'inin yapısını tanımlayan arayüz
export interface AuthState {
  user: User | null;
  accessToken: string | null;
  returnUrl: string | null;
  isListenerInitialized: boolean;
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    user: JSON.parse(localStorage.getItem('user') || 'null'),
    accessToken: localStorage.getItem('access_token'),
    returnUrl: null,
    isListenerInitialized: false,
  }),
  getters: {
    isLoggedIn: (state) => !!state.accessToken,
    isAdmin: (state) => state.user?.roles.includes('Admin') ?? false,
    isCustomer: (state) => state.user?.roles.includes('Customer') ?? false,
  },
  actions: {
    async login(credentials: LoginCredentials) {
      const { error } = await AuthService.login(credentials);
      if (error) {
        console.error('Login failed:', error.message);
        throw error;
      }
    },
    async logout() {
      try {
        await AuthService.logout();
      } catch (error) {
        console.error("Logout failed on server, clearing session locally.", error);
      } finally {
        this.clearSession();
        router.push('/login');
      }
    },
    setSession(session: any) {
      if (session && session.access_token) {
        if (this.accessToken === session.access_token) return;

        const token = session.access_token;
        this.accessToken = token;
        localStorage.setItem('access_token', token);

        const decodedToken: any = jwtDecode(token);
        const roles = decodedToken.app_metadata?.roles || [];
        const status = decodedToken.app_metadata?.status;
        const meta = decodedToken.user_metadata;
        const name = meta?.full_name || (meta?.first_name && meta?.last_name ? `${meta.first_name} ${meta.last_name}` : decodedToken.email);

        this.user = {
          id: decodedToken.sub,
          email: decodedToken.email,
          name: name,
          roles: roles,
          status: status,
          aud: decodedToken.aud,
          ...meta
        };
        localStorage.setItem('user', JSON.stringify(this.user));
      } else {
        this.clearSession();
      }
    },
    clearSession() {
      this.user = null;
      this.accessToken = null;
      this.returnUrl = null;
      localStorage.removeItem('user');
      localStorage.removeItem('access_token');
    },
    async fetchUser() {
      const { data: { session } } = await supabase.auth.getSession();
      this.setSession(session);
      return session?.user;
    },
    async forceRefreshSession() {
      const { data, error } = await supabase.auth.refreshSession();
      if (error) {
        console.error('Failed to refresh session:', error);
        this.logout();
      } else if (data.session) {
        this.setSession(data.session);
      }
      return data.session;
    },
    initialize() {
      if (this.isListenerInitialized) return;

      AuthService.onAuthStateChange(async (_event, session) => {
        this.setSession(session);

        const currentRouteName = router.currentRoute.value.name;

        if (session) {
          const isInvitation = this.user?.status === 'invited';
          
          if (isInvitation) {
            // Davetli kullanıcıyı her zaman şifre belirleme sayfasına yönlendir.
            if (currentRouteName !== 'set-password') {
              router.push({ name: 'set-password' });
            }
          } else if (this.user) {
            // Giriş yapmış ve davet akışında olmayan bir kullanıcı
            // Eğer login sayfasındaysa, onu ana paneline yönlendir.
            if (currentRouteName === 'login') {
              router.push(this.returnUrl || (this.isAdmin ? '/' : '/my-quotes'));
              this.returnUrl = null;
            }
          }
        } else if (_event === 'SIGNED_OUT') {
          this.clearSession();
          router.push('/login');
        }
      });

      this.isListenerInitialized = true;
    }
  },
});