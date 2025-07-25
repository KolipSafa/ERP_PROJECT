import { createRouter, createWebHistory } from 'vue-router'
import DashboardView from '../views/DashboardView.vue'
import ProductsView from '../views/ProductsView.vue'
import ProductFormView from '../views/ProductFormView.vue'
import CustomersView from '../views/CustomersView.vue'
import CustomerFormView from '../views/CustomerFormView.vue'
import QuotesView from '../views/QuotesView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'dashboard',
      component: DashboardView,
    },
    {
      path: '/products',
      name: 'products',
      component: ProductsView,
    },
    {
      path: '/products/new',
      name: 'product-create',
      component: ProductFormView,
    },
    {
      path: '/products/edit/:id',
      name: 'product-edit',
      component: ProductFormView,
      props: true,
    },
    {
      path: '/customers',
      name: 'customers',
      component: CustomersView,
    },
    {
      path: '/customers/new',
      name: 'customer-create',
      component: CustomerFormView,
    },
    {
      path: '/customers/edit/:id',
      name: 'customer-edit',
      component: CustomerFormView,
      props: true,
    },
    {
      path: '/quotes',
      name: 'quotes',
      component: QuotesView,
    },
    {
      path: '/about',
      name: 'about',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/AboutView.vue'),
    },
  ],
})

export default router