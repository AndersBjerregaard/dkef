<script setup lang="ts">
import { ref, type Ref } from 'vue'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import BaseModal from '@/components/BaseModal.vue'
import { Section, SECTION_DISPLAY_MAP, type CreateMemberDto, type Contact } from '@/types/members'
import { toast } from 'vue-sonner'

defineProps<{
  isOpen: boolean
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'member-created', contact: Contact): void
}>()

const isLoading: Ref<boolean> = ref(false)
const errorMessage: Ref<string | null> = ref(null)

const email = ref('')
const name = ref('')
const primarySection = ref<Section>(Section.Jutland)

const emailError = ref('')

function validateEmail(value: string): boolean {
  if (!value) {
    emailError.value = 'Email er påkrævet'
    return false
  }
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
    emailError.value = 'Ugyldig email-adresse'
    return false
  }
  emailError.value = ''
  return true
}

function resetForm() {
  email.value = ''
  name.value = ''
  primarySection.value = Section.Jutland
  errorMessage.value = null
  emailError.value = ''
}

function closeModal() {
  resetForm()
  emit('close')
}

async function createMember() {
  errorMessage.value = null

  if (!name.value) {
    errorMessage.value = 'Navn er påkrævet'
    return
  }

  if (!validateEmail(email.value)) return

  isLoading.value = true

  try {
    const dto: CreateMemberDto = {
      email: email.value,
      name: name.value,
      primarySection: primarySection.value,
    }

    const response = await apiservice.post<Contact>(urlservice.postContact(), dto)

    if (response.status >= 200 && response.status < 300) {
      toast.success('Medlem oprettet!')
      emit('member-created', response.data)
      closeModal()
    } else {
      throw 'Unexpected response'
    }
  } catch (error: unknown) {
    if (error instanceof Error) {
      const msg = error.message.toLowerCase()
      if (msg.includes('email already exists') || msg.includes('already exists')) {
        errorMessage.value = 'En bruger med denne email eksisterer allerede'
      } else {
        errorMessage.value = 'Kunne ikke oprette medlem'
      }
    } else {
      errorMessage.value = 'Kunne ikke oprette medlem'
    }
    console.error(error)
  } finally {
    isLoading.value = false
  }
}

const sectionOptions = Object.entries(SECTION_DISPLAY_MAP).map(([key, value]) => ({
  value: Number(key) as Section,
  label: value,
}))
</script>

<template>
  <BaseModal :is-open="isOpen" title="Opret Nyt Medlem" :is-loading="isLoading" @close="closeModal">
    <form @submit.prevent="createMember">
      <div class="space-y-4">
        <div>
          <label for="email_input" class="block text-sm font-medium">Email</label>
          <input
            id="email_input"
            type="email"
            v-model="email"
            class="inputField"
            :disabled="isLoading"
            @input="validateEmail(email)"
          />
          <p v-if="emailError" class="text-red-500 text-sm mt-1">{{ emailError }}</p>
        </div>

        <div>
          <label for="name_input" class="block text-sm font-medium">Navn</label>
          <input
            id="name_input"
            type="text"
            v-model="name"
            class="inputField"
            :disabled="isLoading"
          />
        </div>

        <div>
          <label for="primarySection_input" class="block text-sm font-medium">
            Primær Sektion
          </label>
          <select
            id="primarySection_input"
            v-model.number="primarySection"
            class="inputField"
            :disabled="isLoading"
          >
            <option v-for="opt in sectionOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
        </div>

        <p v-if="errorMessage" class="text-red-500 text-sm">{{ errorMessage }}</p>
      </div>

      <div class="pt-6 flex gap-3">
        <button
          type="submit"
          class="cursor-pointer inline-flex justify-center rounded-md border border-transparent bg-amber-600 px-4 py-2 text-md font-semibold hover:bg-amber-500 focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent focus-visible:ring-offset-2 shadow-lg shadow-amber-600/20 disabled:opacity-50 hover:text-white transition-colors text-navy-950"
          :disabled="isLoading"
        >
          Opret medlem
        </button>
        <button
          type="button"
          class="cursor-pointer inline-flex justify-center rounded-md border border-transparent bg-theme-mute px-4 py-2 text-md font-medium hover:bg-theme-border hover:text-theme-accent focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent text-theme-heading focus-visible:ring-offset-2 disabled:opacity-50"
          :disabled="isLoading"
          @click="closeModal"
        >
          Annuller
        </button>
      </div>
    </form>
  </BaseModal>
</template>

<style lang="css" scoped>
.inputField {
  width: 100%;
  background-color: var(--color-theme-soft);
  border: 1px solid var(--color-theme-border);
  border-radius: 0.75rem;
  padding: 0.5rem;
}
.inputField:focus {
  outline: none;
  box-shadow: 0 0 0 2px var(--color-accent);
  border-color: var(--color-accent);
}
</style>
