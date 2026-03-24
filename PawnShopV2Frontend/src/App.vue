<script setup>
import ConfirmDialog from './components/ConfirmDialog.vue';
import ToastStack from './components/ToastStack.vue';
import { useUiStore } from './store/ui';

const uiStore = useUiStore()
</script>

<template>
  <router-view />
  <ToastStack :toasts="uiStore.toasts" />
  <ConfirmDialog
    v-if="uiStore.confirmDialog"
    :dialog="uiStore.confirmDialog"
    @close="uiStore.closeConfirm"
  />
</template>
