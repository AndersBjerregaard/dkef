<script setup lang="ts">
import { ref, type Ref } from 'vue'
import { useRouter } from 'vue-router'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import BaseModal from '@/components/BaseModal.vue'

const props = defineProps<{
  isOpen: boolean
  id: string
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'deleted', id: string): void
}>()

const router = useRouter()

const isLoading: Ref<boolean> = ref(false)
const deleteError: Ref<string | null> = ref(null)

async function deleteMember() {
  isLoading.value = true
  deleteError.value = null
  try {
    await apiservice.delete(urlservice.deleteContact(props.id))
    emit('deleted', props.id)
    await router.push({ name: 'members' })
    emit('close')
  } catch (err: unknown) {
    const errorMessage = err instanceof Error ? err.message : 'Kunne ikke slette medlem'
    deleteError.value = errorMessage
    console.error(err)
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <BaseModal :is-open="isOpen" title="Slet Medlem" :is-loading="isLoading" @close="emit('close')">
    <p>Er du sikker på at du vil slette dette medlem? Denne handling kan ikke fortrydes.</p>
    <div class="pt-4 mt-4 flex gap-3">
      <button
        type="button"
        class="cursor-pointer inline-flex justify-center rounded-md border border-transparent bg-red-400 px-4 py-2 text-md font-semibold hover:bg-red-900 focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent focus-visible:ring-offset-2 shadow-lg shadow-amber-600/20 disabled:opacity-50 hover:text-theme-accent transition-colors text-theme-heading"
        :disabled="isLoading"
        @click="deleteMember"
      >
        Bekræft
      </button>
      <button
        type="button"
        class="cursor-pointer inline-flex justify-center rounded-md border border-transparent bg-theme-mute px-4 py-2 text-md font-medium hover:bg-theme-border hover:text-theme-accent focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent text-theme-heading focus-visible:ring-offset-2 disabled:opacity-50"
        :disabled="isLoading"
        @click="emit('close')"
      >
        Annuller
      </button>
    </div>
    <div class="pt-4 text-theme-accent" v-if="deleteError">
      <p>Der skete en fejl under sletningen.</p>
    </div>
  </BaseModal>
</template>
