<script setup>
const props = defineProps({
  dialog: {
    type: Object,
    required: true
  }
})

const emit = defineEmits(['close'])

const confirm = () => emit('close', true)
const cancel = () => emit('close', false)
</script>

<template>
  <div class="modal-backdrop" @click.self="cancel">
    <div class="modal-content w-full max-w-md">
      <h3 class="text-lg font-semibold mb-4">{{ dialog.title || 'Xác nhận' }}</h3>
      <p class="text-slate-300 mb-6">{{ dialog.message }}</p>
      <div class="flex justify-end gap-3">
        <button class="btn btn-secondary" @click="cancel">Hủy</button>
        <button
          :class="['btn', dialog.danger ? 'btn-danger' : 'btn-primary']"
          @click="confirm"
        >
          {{ dialog.confirmText || 'Xác nhận' }}
        </button>
      </div>
    </div>
  </div>
</template>
