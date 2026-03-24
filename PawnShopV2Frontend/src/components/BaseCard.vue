<script setup>
defineProps({
  title: String
})
</script>

<template>
  <div class="card">
    <h2 v-if="title" class="text-lg font-semibold mb-4">{{ title }}</h2>
    <slot />
  </div>
</template>
