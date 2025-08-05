// src/composables/useNotifier.ts
import { toast, type ToastOptions } from 'vue3-toastify';

export function useNotifier() {
  // Bu fonksiyonlar artık ikinci bir argüman olarak ToastOptions alabilir.
  // Bu sayede her bildirim için özel ayarlar (örn: autoClose) yapabiliriz.
  const success = (message: string, options?: ToastOptions) => {
    toast.success(message, options);
  };

  const error = (message: string, options?: ToastOptions) => {
    toast.error(message, options);
  };

  const info = (message: string, options?: ToastOptions) => {
    toast.info(message, options);
  };

  const warn = (message: string, options?: ToastOptions) => {
    toast.warn(message, options);
  };

  return { success, error, info, warn };
}

