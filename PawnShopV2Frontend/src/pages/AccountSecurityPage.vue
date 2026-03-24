<script setup>
import { ref } from 'vue'
import BaseCard from '../components/BaseCard.vue'
import BaseInput from '../components/BaseInput.vue'
import { authService } from '../services'
import { useUiStore } from '../store/ui'

const uiStore = useUiStore()
const form = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})
const loading = ref(false)

const handleSubmit = async () => {
  if (form.value.newPassword !== form.value.confirmPassword) {
    uiStore.showToast('Mật khẩu mới không khớp', 'error')
    return
  }

  if (form.value.newPassword.length < 6) {
    uiStore.showToast('Mật khẩu mới phải có ít nhất 6 ký tự', 'error')
    return
  }

  loading.value = true
  try {
    await authService.changePassword({
      currentPassword: form.value.currentPassword,
      newPassword: form.value.newPassword
    })
    uiStore.showToast('Đổi mật khẩu thành công')
    form.value = { currentPassword: '', newPassword: '', confirmPassword: '' }
  } catch (error) {
    uiStore.showToast(error.response?.data?.message || 'Đổi mật khẩu thất bại', 'error')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div>
    <h1 class="text-2xl font-bold text-white mb-6">Bảo Mật Tài Khoản</h1>

    <BaseCard title="Đổi mật khẩu" class="max-w-md">
      <form @submit.prevent="handleSubmit" class="space-y-4">
        <BaseInput
          v-model="form.currentPassword"
          type="password"
          label="Mật khẩu hiện tại"
          required
        />
        <BaseInput
          v-model="form.newPassword"
          type="password"
          label="Mật khẩu mới"
          required
        />
        <BaseInput
          v-model="form.confirmPassword"
          type="password"
          label="Xác nhận mật khẩu mới"
          required
        />
        <button type="submit" :disabled="loading" class="btn btn-primary w-full">
          {{ loading ? 'Đang xử lý...' : 'Đổi mật khẩu' }}
        </button>
      </form>
    </BaseCard>
  </div>
</template>
