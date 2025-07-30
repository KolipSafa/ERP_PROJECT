// src/composables/useNotifier.ts
import { useToast } from 'vue-toastify';

export function useNotifier() {
  const toast = useToast();

  const success = (message: string) => {
    toast.success(message);
  };

  const error = (message: string) => {
    toast.error(message);
  };

  const info = (message: string) => {
    toast.info(message);
  };

  const warn = (message: string) => {
    toast.warn(message);
  };

  return { success, error, info, warn };
}
