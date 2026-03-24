<script setup>
const props = defineProps({
  show: Boolean,
  title: String,
  size: {
    type: String,
    default: 'md'
  }
})

const emit = defineEmits(['close'])

const sizeClasses = {
  sm: 'max-w-md',
  md: 'max-w-2xl',
  lg: 'max-w-4xl',
  xl: 'max-w-6xl'
}
</script>

<template>
  <Teleport to="body">
    <div v-if="show" class="modal-backdrop" @click.self="emit('close')">
      <div :class="['modal-content w-full', sizeClasses[size]]">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-semibold">{{ title }}</h3>
          <button
            @click="emit('close')"
            class="text-slate-400 hover:text-white text-2xl leading-none"
          >
            &times;
          </button>
        </div>
        <slot />
      </div>
    </div>
  </Teleport>
</template>
