<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import BaseInput from '../components/BaseInput.vue'
import { authService } from '../services'
import { useAuthStore } from '../store/auth'
import { useUiStore } from '../store/ui'

const router = useRouter()
const authStore = useAuthStore()
const uiStore = useUiStore()

const form = ref({
  username: '',
  password: ''
})
const loading = ref(false)

const handleLogin = async () => {
  if (!form.value.username || !form.value.password) {
    uiStore.showToast('Vui lòng nhập đầy đủ thông tin', 'error')
    return
  }

  loading.value = true
  try {
    const { data } = await authService.login(form.value)
    if (data.success) {
      authStore.setAuth(data.data)
      uiStore.showToast('Đăng nhập thành công')

      if (authStore.isOwner) {
        router.push('/admin')
      } else {
        router.push('/admin/lookup')
      }
    } else {
      uiStore.showToast(data.message || 'Đăng nhập thất bại', 'error')
    }
  } catch (error) {
    uiStore.showToast(error.response?.data?.message || 'Đăng nhập thất bại', 'error')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-slate-900 flex items-center justify-center px-4">
    <div class="w-full max-w-md">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-white">Quản Lý Cầm Đồ</h1>
        <p class="text-slate-400 mt-2">Đăng nhập để tiếp tục</p>
      </div>

      <div class="card">
        <form @submit.prevent="handleLogin" class="space-y-4">
          <BaseInput
            v-model="form.username"
            label="Tên đăng nhập"
            placeholder="Nhập tên đăng nhập"
            required
          />

          <BaseInput
            v-model="form.password"
            type="password"
            label="Mật khẩu"
            placeholder="Nhập mật khẩu"
            required
          />

          <button
            type="submit"
            :disabled="loading"
            class="btn btn-primary w-full"
          >
            {{ loading ? 'Đang đăng nhập...' : 'Đăng nhập' }}
          </button>
        </form>

        <div class="mt-6 text-center">
          <router-link to="/" class="text-blue-400 hover:text-blue-300 text-sm">
            Tra cứu hợp đồng công khai
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>
