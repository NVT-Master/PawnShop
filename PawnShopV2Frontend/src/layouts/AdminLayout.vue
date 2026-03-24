<script setup>
import { useRouter } from 'vue-router'
import SidebarMenu from '../components/SidebarMenu.vue'
import TopNavbar from '../components/TopNavbar.vue'
import { useAuthStore } from '../store/auth'

const authStore = useAuthStore()
const router = useRouter()

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <div class="min-h-screen bg-slate-900 flex">
    <!-- Sidebar -->
    <SidebarMenu />

    <!-- Main content -->
    <div class="flex-1 flex flex-col">
      <TopNavbar @logout="handleLogout" />

      <main class="flex-1 p-6 overflow-auto">
        <router-view />
      </main>
    </div>
  </div>
</template>
