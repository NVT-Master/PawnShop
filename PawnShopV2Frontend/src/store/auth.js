import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token') || null)
  const refreshToken = ref(localStorage.getItem('refreshToken') || null)
  const user = ref(JSON.parse(localStorage.getItem('user') || 'null'))

  const isAuthenticated = computed(() => !!token.value)
  const isOwner = computed(() => user.value?.role === 1)
  const isCustomer = computed(() => user.value?.role === 2)
  const userRole = computed(() => user.value?.role === 1 ? 'Owner' : 'Customer')
  const customerId = computed(() => user.value?.customerId)

  function setAuth(data) {
    token.value = data.accessToken
    refreshToken.value = data.refreshToken
    user.value = data.user

    localStorage.setItem('token', data.accessToken)
    localStorage.setItem('refreshToken', data.refreshToken)
    localStorage.setItem('user', JSON.stringify(data.user))
  }

  function logout() {
    token.value = null
    refreshToken.value = null
    user.value = null

    localStorage.removeItem('token')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('user')
  }

  return {
    token,
    refreshToken,
    user,
    isAuthenticated,
    isOwner,
    isCustomer,
    userRole,
    customerId,
    setAuth,
    logout
  }
})
