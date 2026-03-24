<script setup>
import { computed } from 'vue';

const props = defineProps({
  status: [Number, String],
  statusMap: {
    type: Object,
    required: true
  }
})

const statusInfo = computed(() => props.statusMap[props.status] || { label: 'Không xác định', color: 'bg-gray-500' })
</script>

<template>
  <span :class="['px-2 py-1 rounded-full text-xs font-medium text-white', statusInfo.color]">
    {{ statusInfo.label }}
  </span>
</template>
