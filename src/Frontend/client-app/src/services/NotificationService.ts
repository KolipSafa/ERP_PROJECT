import { toast } from 'vue-toastify';

class NotificationService {
  success(message: string) {
    toast.success(message);
  }

  error(message: string) {
    toast.error(message);
  }

  info(message: string) {
    toast.info(message);
  }

  warn(message: string) {
    toast.warn(message);
  }
}

export default new NotificationService();
