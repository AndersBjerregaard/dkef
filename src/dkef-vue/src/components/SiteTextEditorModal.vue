<script setup lang="ts">
import { computed, onBeforeUnmount, ref, useId, watch } from 'vue'
import { toast } from 'vue-sonner'
import BaseModal from '@/components/BaseModal.vue'
import { getSiteTextFields, getSiteTextGroup } from '@/content/siteTexts'
import { validateSiteTextValue } from '@/content/siteTextRules'
import { useAuthStore } from '@/stores/authStore'
import { useSiteTextStore } from '@/stores/siteTextStore'

const props = defineProps<{ isOpen: boolean; group: string }>()
const emit = defineEmits<{ close: [] }>()
const auth = useAuthStore()
const store = useSiteTextStore()
const id = useId()
const definition = computed(() => getSiteTextGroup(props.group))
const fields = computed(() => (definition.value ? getSiteTextFields(definition.value) : []))
const baseline = ref<Record<string, string> | null>(null)
const draft = ref<Record<string, string>>({})
const loading = ref(false)
const saving = ref(false)
const loadError = ref('')
const saveError = ref('')
const serverErrors = ref<Record<string, string>>({})
const sessionBlocked = ref(false)
let generation = 0

const errors = computed(() =>
  Object.fromEntries(
    fields.value.map((field) => [
      field.key,
      validateSiteTextValue(field, draft.value[field.key] ?? '') || serverErrors.value[field.key],
    ]),
  ),
)
const dirty = computed(
  () =>
    baseline.value &&
    fields.value.some((field) => draft.value[field.key] !== baseline.value![field.key]),
)
const canSave = computed(
  () =>
    props.isOpen &&
    auth.isAdmin &&
    !sessionBlocked.value &&
    baseline.value !== null &&
    store.groups[props.group]?.editorReady &&
    !loading.value &&
    !saving.value &&
    store.groups[props.group]?.saveStatus !== 'saving' &&
    dirty.value &&
    !Object.values(errors.value).some(Boolean),
)

function reset() {
  generation++
  baseline.value = null
  draft.value = {}
  loading.value = false
  saving.value = false
  loadError.value = ''
  saveError.value = ''
  serverErrors.value = {}
  sessionBlocked.value = false
}

async function load() {
  reset()
  if (!definition.value) {
    loadError.value = 'Tekstgruppen findes ikke.'
    return
  }
  const current = generation
  loading.value = true
  try {
    const values = await store.refreshForEditor(props.group)
    if (current !== generation) return
    baseline.value = { ...values }
    draft.value = { ...values }
  } catch {
    if (current === generation)
      loadError.value = 'Teksterne kunne ikke hentes. Prøv igen, før du redigerer.'
  } finally {
    if (current === generation) loading.value = false
  }
}

function close() {
  if (saving.value) return
  reset()
  emit('close')
}

function edit(key: string, event: Event) {
  draft.value[key] = (event.target as HTMLInputElement | HTMLTextAreaElement).value
  delete serverErrors.value[key]
}

async function save() {
  if (!canSave.value || !baseline.value) return
  const current = generation
  saving.value = true
  saveError.value = ''
  serverErrors.value = {}
  try {
    await store.saveGroup(props.group, { ...draft.value }, { ...baseline.value })
    if (current !== generation) return
    // The store publishes the authoritative response, including server sanitization.
    toast.success('Teksterne er gemt og offentliggjort.')
    saving.value = false
    close()
  } catch (error) {
    if (current !== generation) return
    const response = (error as { response?: { status?: number; data?: { errors?: unknown } } })
      ?.response
    if (response?.status === 401 || response?.status === 403) {
      sessionBlocked.value = true
      saveError.value =
        'Du har ikke adgang til at gemme. Log ind igen som administrator. Dine ændringer er bevaret.'
    } else {
      saveError.value = 'Teksterne kunne ikke gemmes. Dine ændringer er bevaret. Prøv igen.'
      const problem = response?.data?.errors
      if (problem && typeof problem === 'object') {
        for (const field of fields.value) {
          const messages = (problem as Record<string, unknown>)[`changes.${field.key}`]
          if (Array.isArray(messages) && messages.every((message) => typeof message === 'string'))
            serverErrors.value[field.key] = messages.join(' ')
        }
      }
    }
  } finally {
    if (current === generation) saving.value = false
  }
}

watch(
  () => [props.isOpen, props.group] as const,
  ([open]) => {
    if (open) void load()
    else reset()
  },
  { immediate: true },
)
watch(
  () => [auth.isAdmin, auth.accessToken],
  () => {
    if (auth.isAdmin) sessionBlocked.value = false
  },
)
onBeforeUnmount(() => {
  generation++
})

const fieldId = (key: string) => `${id}-${key}`
</script>

<template>
  <BaseModal
    :is-open="isOpen"
    :title="`Rediger tekster – ${definition?.label ?? 'Ukendt tekstgruppe'}`"
    :is-loading="saving"
    @close="close"
  >
    <form
      novalidate
      :aria-busy="loading || saving"
      class="space-y-5 text-theme-text"
      @submit.prevent="save"
    >
      <p v-if="loading" role="status">Henter tekster…</p>
      <div v-if="loadError" role="alert" class="space-y-2">
        <p>{{ loadError }}</p>
        <button
          v-if="definition"
          type="button"
          class="underline focus-visible:ring-2 focus-visible:ring-theme-accent"
          @click="load"
        >
          Prøv igen
        </button>
      </div>
      <p v-if="!auth.isAdmin || sessionBlocked" role="alert">
        Du skal være logget ind som administrator for at gemme. Dine eventuelle ændringer er
        bevaret, indtil du lukker vinduet.
      </p>
      <template v-if="baseline && definition">
        <p>Ændringer offentliggøres, når du gemmer. Felter markeret med * skal udfyldes.</p>
        <fieldset
          v-for="section in definition.sections"
          :key="section.id"
          :disabled="saving"
          class="space-y-4 min-w-0"
        >
          <legend class="text-theme-heading font-semibold mb-2">{{ section.label }}</legend>
          <div v-for="field in section.fields" :key="field.key" class="space-y-1">
            <label :for="fieldId(field.key)" class="block font-medium text-theme-heading">
              {{ field.label }}<span v-if="field.required" aria-hidden="true"> *</span>
            </label>
            <component
              :is="field.input === 'multiline' ? 'textarea' : 'input'"
              :id="fieldId(field.key)"
              :type="
                field.input === 'multiline'
                  ? undefined
                  : field.format === 'email'
                    ? 'email'
                    : field.format === 'phone'
                      ? 'tel'
                      : 'text'
              "
              :rows="field.input === 'multiline' ? 5 : undefined"
              :value="draft[field.key]"
              :required="field.required"
              :maxlength="field.maxLength"
              :aria-invalid="Boolean(errors[field.key])"
              :aria-describedby="`${fieldId(field.key)}-hint${errors[field.key] ? ` ${fieldId(field.key)}-error` : ''}`"
              class="w-full min-w-0 bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading focus:outline-none focus:ring-2 focus:ring-theme-accent disabled:opacity-50"
              @input="edit(field.key, $event)"
            />
            <p :id="`${fieldId(field.key)}-hint`" class="text-sm">
              {{ draft[field.key]?.length ?? 0 }} / {{ field.maxLength }} tegn.
              <template v-if="field.format === 'email'"
                >Indtast én e-mailadresse uden navn eller mellemrum.</template
              >
              <template v-else-if="field.format === 'phone'"
                >Brug cifre, mellemrum, +, -, ( og ).</template
              >
            </p>
            <p
              v-if="errors[field.key]"
              :id="`${fieldId(field.key)}-error`"
              role="alert"
              class="font-medium"
            >
              {{ errors[field.key] }}
            </p>
          </div>
        </fieldset>
      </template>
      <p v-if="saveError" role="alert">{{ saveError }}</p>
      <p v-if="saving" role="status">Gemmer og offentliggør tekster…</p>
      <div class="flex flex-wrap gap-3">
        <button
          type="submit"
          :disabled="!canSave"
          class="rounded-lg border border-theme-border bg-theme-soft px-4 py-2 font-semibold text-theme-heading hover:text-theme-accent focus-visible:ring-2 focus-visible:ring-theme-accent disabled:opacity-50"
        >
          Gem ændringer
        </button>
        <button
          type="button"
          :disabled="saving"
          class="rounded-lg border border-theme-border px-4 py-2 text-theme-heading hover:text-theme-accent focus-visible:ring-2 focus-visible:ring-theme-accent disabled:opacity-50"
          @click="close"
        >
          Annuller
        </button>
      </div>
    </form>
  </BaseModal>
</template>
