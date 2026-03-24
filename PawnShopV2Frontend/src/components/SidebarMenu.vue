<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '../store/auth'

const authStore = useAuthStore()
const route = useRoute()

const ownerMenuItems = [
  { path: '/admin', label: 'Bảng điều khiển', icon: '📊' },
  { path: '/admin/customers', label: 'Khách hàng', icon: '👥' },
  { path: '/admin/assets', label: 'Tài sản', icon: '💎' },
  { path: '/admin/contracts', label: 'Hợp đồng', icon: '📄' },
  { path: '/admin/extension', label: 'Gia hạn', icon: '🔄' },
  { path: '/admin/redemption', label: 'Chuộc hàng', icon: '✅' },
  { path: '/admin/liquidation', label: 'Thanh lý', icon: '⚠️' },
  { path: '/admin/reports', label: 'Báo cáo', icon: '📈' }
]

const customerMenuItems = [
  { path: '/admin/lookup', label: 'Tra cứu', icon: '🔍' }
]

const menuItems = computed(() =>
  authStore.isOwner ? ownerMenuItems : customerMenuItems
)

const isActive = (path) => {
  if (path === '/admin') return route.path === '/admin'
  return route.path.startsWith(path)
}
</script>

<template>
  <aside class="w-64 bg-slate-800 border-r border-slate-700 hidden lg:block">
    <div class="p-6">
      <h1 class="text-xl font-bold text-white">Quản Lý Cầm Đồ</h1>
      <p class="text-sm text-slate-400 mt-1">PawnShop v2</p>
    </div>

    <nav class="px-4 pb-6">
      <router-link
        v-for="item in menuItems"
        :key="item.path"
        :to="item.path"
        :class="[
          'flex items-center gap-3 px-4 py-3 rounded-lg mb-1 transition-colors',
          isActive(item.path)
            ? 'bg-blue-600 text-white'
            : 'text-slate-300 hover:bg-slate-700'
        ]"
      >
        <span>{{ item.icon }}</span>
        <span>{{ item.label }}</span>
      </router-link>

      <div class="border-t border-slate-700 my-4"></div>

      <router-link
        to="/admin/account-security"
        :class="[
          'flex items-center gap-3 px-4 py-3 rounded-lg transition-colors',
          isActive('/admin/account-security')
            ? 'bg-blue-600 text-white'
            : 'text-slate-300 hover:bg-slate-700'
        ]"
      >
        <span>🔒</span>
        <span>Bảo mật tài khoản</span>
      </router-link>
    </nav>
  </aside>
</template>
