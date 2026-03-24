import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../store/auth'

const routes = [
  {
    path: '/',
    name: 'PublicLookup',
    component: () => import('../pages/PublicLookupPage.vue'),
    meta: { public: true }
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../pages/LoginPage.vue'),
    meta: { public: true }
  },
  {
    path: '/admin',
    component: () => import('../layouts/AdminLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'Dashboard',
        component: () => import('../pages/DashboardPage.vue'),
        meta: { roles: ['Owner'] }
      },
      {
        path: 'customers',
        name: 'Customers',
        component: () => import('../pages/CustomersPage.vue'),
        meta: { roles: ['Owner'] }
      },
      {
        path: 'assets',
        name: 'Assets',
        component: () => import('../pages/AssetsPage.vue'),
        meta: { roles: ['Owner'] }
      },
      {
        path: 'contracts',
        name: 'Contracts',
        component: () => import('../pages/ContractsPage.vue'),
        meta: { roles: ['Owner'] }
      },
      {
        path: 'extension',
        name: 'Extension',
        component: () => import('../pages/ExtensionPage.vue'),
        meta: { roles: ['Owner'] }
      },
      {
        path: 'redemption',
        name: 'Redemption',
        component: () => import('../pages/RedemptionPage.vue'),
        meta: { roles: ['Owner'] }
      },
      {
        path: 'liquidation',
        name: 'Liquidation',
        component: () => import('../pages/LiquidationPage.vue'),
        meta: { roles: ['Owner'] }
      },
      {
        path: 'reports',
        name: 'Reports',
        component: () => import('../pages/ReportsPage.vue'),
        meta: { roles: ['Owner'] }
      },
      {
        path: 'account-security',
        name: 'AccountSecurity',
        component: () => import('../pages/AccountSecurityPage.vue')
      },
      {
        path: 'lookup',
        name: 'Lookup',
        component: () => import('../pages/LookupPage.vue'),
        meta: { roles: ['Customer'] }
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  if (to.meta.public) {
    next()
    return
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
    return
  }

  if (to.meta.roles && !to.meta.roles.includes(authStore.userRole)) {
    if (authStore.userRole === 'Customer') {
      next('/admin/lookup')
    } else {
      next('/admin')
    }
    return
  }

  next()
})

export default router
