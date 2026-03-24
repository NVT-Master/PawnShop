import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useUiStore = defineStore('ui', () => {
  const toasts = ref([])
  const confirmDialog = ref(null)
  let toastId = 0

  function showToast(message, type = 'success') {
    const id = ++toastId
    toasts.value.push({ id, message, type })

    setTimeout(() => {
      toasts.value = toasts.value.filter(t => t.id !== id)
    }, 2800)
  }

  function showConfirm(options) {
    return new Promise((resolve) => {
      confirmDialog.value = {
        ...options,
        resolve
      }
    })
  }

  function closeConfirm(result) {
    if (confirmDialog.value) {
      confirmDialog.value.resolve(result)
      confirmDialog.value = null
    }
  }

  return {
    toasts,
    confirmDialog,
    showToast,
    showConfirm,
    closeConfirm
  }
})
