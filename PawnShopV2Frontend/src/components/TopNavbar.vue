<script setup>
import { useAuthStore } from '../store/auth';

const authStore = useAuthStore()
const emit = defineEmits(['logout'])
</script>

<template>
  <header class="bg-slate-800 border-b border-slate-700 px-6 py-4">
    <div class="flex items-center justify-between">
      <div class="lg:hidden">
        <h1 class="text-lg font-bold text-white">PawnShop</h1>
      </div>

      <div class="flex items-center gap-4 ml-auto">
        <div class="text-right">
          <p class="text-sm font-medium text-white">{{ authStore.user?.fullName }}</p>
          <p class="text-xs text-slate-400">
            {{ authStore.isOwner ? 'Chủ tiệm' : 'Khách hàng' }}
          </p>
        </div>

        <button
          @click="emit('logout')"
          class="btn btn-secondary text-sm"
        >
          Đăng xuất
        </button>
      </div>
    </div>
  </header>
</template>
