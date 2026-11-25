<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import SiteImageEditorModal from '@/components/SiteImageEditorModal.vue'

defineProps<{ group: string }>()
const auth = useAuthStore()
const isOpen = ref(false)
</script>

<template>
  <button
    v-if="auth.isAdmin"
    type="button"
    class="cursor-pointer rounded-lg border border-theme-border bg-theme-soft px-4 py-2 text-theme-heading hover:text-theme-accent focus-visible:ring-2 focus-visible:ring-theme-accent"
    @click="isOpen = true"
  >
    Rediger billeder
  </button>
  <SiteImageEditorModal :is-open="isOpen" :group="group" @close="isOpen = false" />
</template>
