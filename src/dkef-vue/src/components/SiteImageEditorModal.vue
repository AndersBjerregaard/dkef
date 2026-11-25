<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import BaseModal from '@/components/BaseModal.vue'
import SiteImageSlotEditor from '@/components/SiteImageSlotEditor.vue'
import { getSiteImageGroup } from '@/content/siteImages'
import { useSiteImageStore } from '@/stores/siteImageStore'
import { useAuthStore } from '@/stores/authStore'

const props = defineProps<{ isOpen: boolean; group: string }>()
const emit = defineEmits<{ close: [] }>()
const store = useSiteImageStore()
const auth = useAuthStore()
const definition = computed(() => getSiteImageGroup(props.group))
const loading = ref(false)
const error = ref('')
const ready = ref(false)
let generation = 0

async function load() {
  const current = ++generation
  ready.value = false
  loading.value = true
  error.value = ''
  try {
    await store.loadGroup(props.group, true)
    if (generation === current) ready.value = true
  } catch {
    if (generation === current)
      error.value = 'Billederne kunne ikke hentes. Prøv igen, før du redigerer.'
  } finally {
    if (generation === current) loading.value = false
  }
}

watch(
  () => [props.isOpen, props.group] as const,
  ([open]) => {
    if (open) void load()
    else {
      generation++
      ready.value = false
    }
  },
  { immediate: true },
)
</script>

<template>
  <BaseModal
    :is-open="isOpen"
    :title="`Rediger billeder – ${definition?.label ?? ''}`"
    :is-loading="store.saving[group]"
    @close="emit('close')"
  >
    <p class="text-theme-text pb-4">
      Gem ét billede ad gangen. Ændringer offentliggøres med det samme. JPEG, PNG eller WebP, højst
      5 MB og 16 megapixel. Animerede billeder understøttes ikke.
    </p>
    <p v-if="loading" role="status">Henter billeder…</p>
    <p v-if="store.saving[group]" role="status">Gemmer billede…</p>
    <div v-if="error" role="alert" class="pb-4">
      <p>{{ error }}</p>
      <button type="button" class="underline" @click="load">Prøv igen</button>
    </div>
    <p v-if="!auth.isAdmin" role="alert">
      Du skal være logget ind som administrator for at gemme. Dine valgte billeder er bevaret,
      indtil du lukker vinduet.
    </p>
    <div v-if="isOpen && ready" class="space-y-6">
      <SiteImageSlotEditor
        v-for="slot in definition?.slots"
        :key="`${group}/${slot.id}`"
        :group="group"
        :slot-definition="slot"
      />
    </div>
  </BaseModal>
</template>
