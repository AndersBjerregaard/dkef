<script setup lang="ts">
import { ref } from 'vue'
import SiteTextEditorModal from '@/components/SiteTextEditorModal.vue'
import { getSiteTextGroup } from '@/content/siteTexts'
import { useAuthStore } from '@/stores/authStore'

defineProps<{ group: string; label?: string; description?: string }>()
const auth = useAuthStore()
const isOpen = ref(false)
</script>

<template>
  <button
    v-if="auth.isAdmin && getSiteTextGroup(group)"
    type="button"
    class="rounded-lg border border-theme-border bg-theme-soft px-4 py-2 text-theme-heading hover:text-theme-accent focus-visible:ring-2 focus-visible:ring-theme-accent"
    @click="isOpen = true"
  >
    <template v-if="label">{{ label }}</template>
    <template v-else>
      Rediger tekster<span class="sr-only"> – {{ getSiteTextGroup(group)?.label }}</span>
    </template>
  </button>
  <!-- Keep an open draft mounted on logout/role loss; only the action is gated. -->
  <SiteTextEditorModal
    :is-open="isOpen"
    :group="group"
    :description="description"
    @close="isOpen = false"
  />
</template>
