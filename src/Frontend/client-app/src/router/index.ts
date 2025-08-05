import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

// Views
import DashboardView from '../views/DashboardView.vue'
import ProductsView from '../views/ProductsView.vue'
import ProductFormView from '../views/ProductFormView.vue'
import CustomersView from '../views/CustomersView.vue'
import CustomerFormView from '../views/CustomerFormView.vue'
import QuotesView from '../views/QuotesView.vue'
import QuoteFormView from '../views/QuoteFormView.vue'
import SettingsView from '../views/SettingsView.vue'
import LoginView from '../views/LoginView.vue'
import SetPasswordView from '../views/SetPasswordView.vue'
import AuthCallback from '../views/AuthCallback.vue' // AuthCallback import edildi

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { requiresAuth: false }
    },
    {
      path: '/auth/callback',
      name: 'auth-callback',
      component: AuthCallback,
      meta: { requiresAuth: false }
    },
    {
      path: '/set-password',
      name: 'set-password',
      component: SetPasswordView,
      meta: { requiresAuth: true, isInvitation: true } // Davet akışı için özel sayfa
    },
    {
      path: '/',
      name: 'dashboard',
      component: DashboardView,
      meta: { requiresAuth: true, roles: ['Admin', 'Customer'] }
    },
    {
      path: '/products',
      name: 'products',
      component: ProductsView,
      meta: { requiresAuth: true, roles: ['Admin'] }
    },
    {
      path: '/products/new',
      name: 'product-create',
      component: ProductFormView,
      meta: { requiresAuth: true, roles: ['Admin'] }
    },
    {
      path: '/products/edit/:id',
      name: 'product-edit',
      component: ProductFormView,
      props: true,
      meta: { requiresAuth: true, roles: ['Admin'] }
    },
    {
      path: '/customers',
      name: 'customers',
      component: CustomersView,
      meta: { requiresAuth: true, roles: ['Admin'] }
    },
    {
      path: '/customers/new',
      name: 'customer-create',
      component: CustomerFormView,
      meta: { requiresAuth: true, roles: ['Admin'] }
    },
    {
      path: '/customers/edit/:id',
      name: 'customer-edit',
      component: CustomerFormView,
      props: true,
      meta: { requiresAuth: true, roles: ['Admin'] }
    },
    {
      path: '/quotes',
      name: 'quotes',
      component: QuotesView,
      meta: { requiresAuth: true, roles: ['Admin'] }
    },
    {
      path: '/quotes/new',
      name: 'quote-create',
      component: QuoteFormView,
      meta: { requiresAuth: true, roles: ['Admin'] }
    },
    {
      path: '/quotes/edit/:id',
      name: 'quote-edit',
      component: QuoteFormView,
      props: true,
      meta: { requiresAuth: true, roles: ['Admin'] }
    },
    {
      path: '/settings',
      name: 'settings',
      component: SettingsView,
      meta: { requiresAuth: true, roles: ['Admin'] }
    },
    {
      path: '/my-quotes',
      name: 'my-quotes',
      component: () => import('../views/customer/MyQuotesView.vue'),
      meta: { requiresAuth: true, roles: ['Customer'] }
    },
  ],
})

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore();
  // authStore'un başlatıldığından emin ol, bu onAuthStateChange dinleyicisini kurar.
  if (!authStore.isListenerInitialized) {
    authStore.initialize();
  }
  
  const isLoggedIn = authStore.isLoggedIn;
  // Davet durumunu yeni status alanına göre kontrol et
  const isInvitationSession = isLoggedIn && authStore.user?.status === 'invited';

  if (isInvitationSession && to.name !== 'set-password') {
    // Davet modundaki kullanıcıyı SADECE şifre belirleme sayfasına gitmeye zorla.
    return next({ name: 'set-password' });
  }

  if (to.meta.requiresAuth && !isLoggedIn) {
    // Eğer sayfa yetki gerektiriyorsa ve kullanıcı giriş yapmamışsa, login'e yolla.
    // AuthCallback gibi sayfalar bu kontrolden muaftır.
    if (to.name !== 'auth-callback') {
      authStore.returnUrl = to.fullPath;
      return next({ name: 'login' });
    }
  }

  if (isLoggedIn && !isInvitationSession && to.name === 'login') {
    // Normal giriş yapmış kullanıcı login sayfasına gidemez.
    return next({ name: authStore.isAdmin ? 'dashboard' : 'my-quotes' });
  }

  // Rol kontrolü
  const requiredRoles = to.meta.roles as string[] | undefined;
  if (isLoggedIn && !isInvitationSession && requiredRoles && requiredRoles.length > 0) {
    const userRoles = authStore.user?.roles || [];
    const hasRequiredRole = requiredRoles.some(role => userRoles.includes(role));
    if (!hasRequiredRole) {
      // Yetkisi yoksa, kendi ana sayfasına yönlendir.
      return next({ name: authStore.isAdmin ? 'dashboard' : 'my-quotes' });
    }
  }

  next();
});

export default router
