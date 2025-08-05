import { supabase } from '@/lib/supabaseClient';
import type { LoginCredentials, SetPasswordPayload } from '@/services/dtos/AuthDtos';

class AuthService {
  /**
   * Kullanıcıyı e-posta ve şifre ile Supabase'e giriş yaptırır.
   * @param credentials E-posta ve şifre içeren obje.
   * @returns Supabase'in session nesnesini içeren bir Promise.
   */
  async login(credentials: LoginCredentials) {
    return await supabase.auth.signInWithPassword(credentials);
  }

  /**
   * Mevcut kullanıcının oturumunu kapatır.
   */
  async logout() {
    return await supabase.auth.signOut();
  }

  /**
   * Kullanıcının şifresini günceller. Bu genellikle şifre sıfırlama veya
   * davet linkinden sonra ilk şifreyi belirleme akışında kullanılır.
   * @param payload Yeni şifreyi içeren obje.
   */
  async updateUserPassword(newPassword: string) {
    return await supabase.auth.updateUser({ password: newPassword });
  }

  /**
   * Supabase'in kimlik doğrulama durumundaki değişiklikleri dinler.
   * (örn: token yenilendi, kullanıcı giriş/çıkış yaptı).
   * @param callback Durum değişikliğinde çalıştırılacak fonksiyon.
   */
  onAuthStateChange(callback: (event: string, session: any) => void) {
    return supabase.auth.onAuthStateChange(callback);
  }

  /**
   * Mevcut aktif oturumu Supabase'den alır.
   */
  async getSession() {
    return await supabase.auth.getSession();
  }

  /**
   * Oturumu açık olan kullanıcının meta verilerini günceller.
   * Özellikle davet akışını tamamlamak için kullanılır.
   * @param metadata Güncellenecek meta veri objesi.
   */
  async updateUserMetadata(metadata: object) {
    return await supabase.auth.updateUser({ data: metadata });
  }
}

export default new AuthService();