// Bu dosya, Supabase ve JWT'den gelen verileri modellemek için kullanılır.

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface SetPasswordPayload {
  newPassword: string;
}

/**
 * Supabase tarafından üretilen JWT'nin içindeki temel alanları temsil eder.
 * Gerçek token daha fazla alan içerebilir.
 */
export interface DecodedToken {
  sub: string; // User ID
  email: string;
  role: string; // Supabase'de bu genellikle tek bir string'dir.
  app_metadata: {
    roles?: string[]; // Rollerimizi bu alanda saklayacağız.
  };
  user_metadata: {
    full_name?: string;
  };
  exp: number;
  iss: string;
  aud: string;
}
