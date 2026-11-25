<script setup lang="ts">
import { computed, onBeforeUnmount, ref, useId } from 'vue'
import { toast } from 'vue-sonner'
import { type SiteImageSlot, siteImageAccept, validateSiteImageFile } from '@/content/siteImages'
import { useSiteImageStore } from '@/stores/siteImageStore'
import { useAuthStore } from '@/stores/authStore'

const props = defineProps<{ group: string; slotDefinition: SiteImageSlot }>()
const store = useSiteImageStore()
const auth = useAuthStore()
const id = useId()
const file = ref<File | null>(null)
const preview = ref('')
const alt = ref('')
const error = ref('')
const input = ref<HTMLInputElement | null>(null)
const resetRequested = ref(false)
const current = computed(() => store.value(props.group, props.slotDefinition.id))
const busy = computed(() => !!store.saving[props.group])
const validAlt = computed(
  () =>
    !!alt.value.trim() &&
    alt.value.trim().length <= 300 &&
    !/[\u0000-\u001f\u007f-\u009f\u2028\u2029]/.test(alt.value),
)

function clear(resetInput = true) {
  if (preview.value) URL.revokeObjectURL(preview.value)
  preview.value = ''
  file.value = null
  resetRequested.value = false
  if (resetInput && input.value) input.value.value = ''
}

function select(event: Event) {
  const selected = (event.target as HTMLInputElement).files?.[0]
  clear(false)
  error.value = ''
  if (!selected) return
  const validation = validateSiteImageFile(selected)
  if (validation) {
    error.value = validation
    return
  }
  file.value = selected
  alt.value = current.value.alt
  preview.value = URL.createObjectURL(selected)
}

async function save(reset = false) {
  if (!auth.isAdmin || busy.value || (!reset && (!file.value || !validAlt.value))) return
  error.value = ''
  try {
    await store.change(
      props.group,
      props.slotDefinition.id,
      reset ? undefined : file.value!,
      alt.value,
    )
    clear()
    toast.success(reset ? 'Standardbilledet er gendannet.' : 'Billedet er gemt og offentliggjort.')
  } catch (failure) {
    const response = (failure as { response?: { status?: number; data?: { title?: string } } })
      ?.response
    error.value =
      response?.status === 401 || response?.status === 403
        ? 'Du har ikke adgang til at gemme. Log ind igen som administrator. Dit valgte billede er bevaret.'
        : (response?.data?.title ??
          'Billedet kunne ikke gemmes. Dit valgte billede er bevaret. Prøv igen.')
  }
}

onBeforeUnmount(clear)
</script>

<template>
  <section class="border-t border-theme-border pt-4">
    <h4 class="text-lg text-theme-heading pb-3">{{ slotDefinition.label }}</h4>
    <div class="flex justify-center bg-theme-soft rounded-xl p-2 mb-3 max-w-96">
      <img
        :src="preview || current.url"
        :alt="preview ? alt : current.alt"
        :class="slotDefinition.previewClass"
        :style="{
          aspectRatio: slotDefinition.aspectRatio,
          objectFit: slotDefinition.aspectRatio ? 'cover' : undefined,
        }"
      />
    </div>
    <p v-if="preview" class="text-theme-accent pb-2">Forhåndsvisning – endnu ikke gemt</p>
    <label :for="`${id}-file`" class="block pb-1">Skift billede – {{ slotDefinition.label }}</label>
    <input
      :id="`${id}-file`"
      ref="input"
      type="file"
      :accept="siteImageAccept"
      :disabled="busy || !auth.isAdmin"
      class="block w-full min-w-0 cursor-pointer bg-theme-soft border border-theme-border rounded-xl p-2 mb-3 text-sm text-theme-text hover:bg-theme-mute focus:outline-none focus:ring-2 focus:ring-theme-accent file:mr-3 file:cursor-pointer file:rounded-lg file:border-0 file:bg-theme-mute file:px-3 file:py-2 file:font-semibold file:text-theme-heading disabled:cursor-not-allowed disabled:opacity-50 disabled:file:cursor-not-allowed"
      @change="select"
    />
    <form v-if="file" class="space-y-3" @submit.prevent="save()">
      <label :for="`${id}-alt`" class="block">Alternativ billedtekst</label>
      <input
        :id="`${id}-alt`"
        v-model="alt"
        type="text"
        required
        maxlength="300"
        :disabled="busy || !auth.isAdmin"
        class="w-full rounded border border-theme-border bg-theme-soft p-2 text-theme-text"
        :aria-describedby="`${id}-help`"
      />
      <p :id="`${id}-help`" class="text-sm text-theme-text">
        Beskriv billedet kort på dansk til besøgende, der ikke kan se det.
      </p>
      <div class="flex flex-wrap gap-3">
        <button
          type="submit"
          :disabled="busy || !auth.isAdmin || !validAlt"
          class="cursor-pointer rounded border border-theme-border px-3 py-2 disabled:cursor-not-allowed disabled:opacity-50"
        >
          Gem billede
        </button>
        <button
          type="button"
          :disabled="busy"
          class="cursor-pointer underline disabled:cursor-not-allowed disabled:opacity-50"
          @click="clear()"
        >
          Annuller valg
        </button>
      </div>
    </form>
    <div v-if="store.overrides[group]?.[slotDefinition.id]" class="pt-3">
      <button
        v-if="!resetRequested"
        type="button"
        :disabled="busy || !auth.isAdmin"
        class="cursor-pointer underline disabled:cursor-not-allowed disabled:opacity-50"
        @click="resetRequested = true"
      >
        Gendan standardbillede
      </button>
      <div v-else class="space-y-2">
        <p>Vil du erstatte det offentliggjorte billede med standardbilledet?</p>
        <div class="flex flex-wrap gap-3">
          <button
            type="button"
            :disabled="busy || !auth.isAdmin"
            class="cursor-pointer underline disabled:cursor-not-allowed disabled:opacity-50"
            @click="save(true)"
          >
            Ja, gendan standardbillede
          </button>
          <button
            type="button"
            :disabled="busy"
            class="cursor-pointer underline disabled:cursor-not-allowed disabled:opacity-50"
            @click="resetRequested = false"
          >
            Annuller
          </button>
        </div>
      </div>
    </div>
    <p v-if="error" role="alert" class="pt-3 text-theme-text">{{ error }}</p>
  </section>
</template>
