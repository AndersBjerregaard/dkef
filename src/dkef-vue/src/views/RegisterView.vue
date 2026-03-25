<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import type { RegisterDto } from '@/types/auth'
import { Section, SECTION_DISPLAY_MAP } from '@/types/members'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const name = ref('')
const primarySection = ref<Section | ''>('')
const showAdvanced = ref(false)
const employmentStatus = ref('')
const magazineDelivery = ref('Privat adresse')
const subscription = ref('Alm.')
const companyName = ref('')
const companyAddress = ref('')
const companyZIP = ref('')
const companyCity = ref('')
const companyPhone = ref('')
const error = ref('')
const loading = ref(false)

const sections = Object.entries(SECTION_DISPLAY_MAP).map(([value, label]) => ({
  value: Number(value) as Section,
  label,
}))

const employmentOptions = [
  { value: 'Studerende', label: 'Studerende' },
  { value: 'Erhvervsaktiv', label: 'Erhvervsaktiv' },
  { value: 'Uden job', label: 'Uden job' },
  { value: 'Pensionist', label: 'Pensionist' },
]

async function handleRegister() {
  error.value = ''

  // Validation
  if (password.value !== confirmPassword.value) {
    error.value = 'Adgangskodeordene er ikke ens'
    return
  }

  if (password.value.length < 8) {
    error.value = 'Adgangskoden skal være mindst 8 tegn lang'
    return
  }

  if (primarySection.value === '') {
    error.value = 'Vælg venligst en primarsektion'
    return
  }

  loading.value = true

  try {
    const registrationData: RegisterDto = {
      email: email.value,
      password: password.value,
      confirmPassword: confirmPassword.value,
      name: name.value,
      primarySection: primarySection.value as Section,
      employmentStatus: employmentStatus.value,
      magazineDelivery: magazineDelivery.value,
      subscription: subscription.value,
      companyName: companyName.value,
      companyAddress: companyAddress.value,
      companyZIP: companyZIP.value,
      companyCity: companyCity.value,
      companyPhone: companyPhone.value,
    }

    await authStore.register(registrationData)
    await router.push({ name: view })
    window.scrollTo({ top: 0, behavior: 'smooth' })
  } catch (err) {
    const axiosError = err as { response?: { data?: { message?: string } }; message?: string }
    error.value =
      axiosError.response?.data?.message ||
      axiosError.message ||
      'Registrering mislykkedes. Prøv igen.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-6">
      <div class="pb-4">
        <h2 class="mt-6 text-center text-3xl font-bold">Opret din konto</h2>
      </div>
      <form class="mt-8 space-y-4 flex-col gap-8" @submit.prevent="handleRegister">
        <div v-if="error" class="rounded-xl bg-red-900 bg-opacity-50 p-4 border border-red-700">
          <p class="text-sm text-red-200">{{ error }}</p>
        </div>

        <!-- Required fields -->
        <div class="space-y-4 flex flex-col gap-4">
          <div>
            <label for="name" class="block text-sm font-medium mb-1"> Navn </label>
            <input
              id="name"
              v-model="name"
              name="name"
              type="text"
              required
              class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
              placeholder="Dit fulde navn"
            />
          </div>

          <div>
            <label for="email-address" class="block text-sm font-medium mb-1"> Email </label>
            <input
              id="email-address"
              v-model="email"
              name="email"
              type="email"
              autocomplete="email"
              required
              class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
              placeholder="Email"
            />
          </div>

          <div>
            <label for="password" class="block text-sm font-medium mb-1">Adgangskode</label>
            <input
              id="password"
              v-model="password"
              name="password"
              type="password"
              autocomplete="new-password"
              required
              class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
              placeholder="Adgangskode (min 8 tegn)"
            />
          </div>

          <div>
            <label for="confirm-password" class="block text-sm font-medium mb-1">
              Bekræft adgangskode
            </label>
            <input
              id="confirm-password"
              v-model="confirmPassword"
              name="confirm-password"
              type="password"
              autocomplete="new-password"
              required
              class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
              placeholder="Bekræft adgangskode"
            />
          </div>

          <div>
            <label for="primary-section" class="block text-sm font-medium mb-1">
              Primær sektion *
            </label>
            <select
              id="primary-section"
              v-model.number="primarySection"
              name="primary-section"
              required
              class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent cursor-pointer"
            >
              <option :value="''" disabled>Vælg en sektion</option>
              <option v-for="section in sections" :key="section.value" :value="section.value">
                {{ section.label }}
              </option>
            </select>
          </div>
        </div>

        <!-- Advanced options toggle -->
        <div class="pt-4 border-t border-theme-border">
          <button
            type="button"
            @click="showAdvanced = !showAdvanced"
            class="flex items-center gap-2 text-sm font-medium text-theme-accent hover:text-theme-accent/80 transition-colors cursor-pointer"
          >
            <span>{{ showAdvanced ? '▼' : '▶' }}</span>
            Angiv valgfri oplysninger
          </button>
        </div>

        <!-- Advanced fields -->
        <div v-if="showAdvanced" class="space-y-4 flex flex-col gap-4 pt-4">
          <div>
            <label for="employment-status" class="block text-sm font-medium mb-1">
              Ansættelsestatus
            </label>
            <select
              id="employment-status"
              v-model="employmentStatus"
              name="employment-status"
              class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent cursor-pointer"
            >
              <option value="">Vælg en status</option>
              <option v-for="option in employmentOptions" :key="option.value" :value="option.value">
                {{ option.label }}
              </option>
            </select>
          </div>

          <div>
            <label for="magazine-delivery" class="block text-sm font-medium mb-1">
              Magasinlevering
            </label>
            <select
              id="magazine-delivery"
              v-model="magazineDelivery"
              name="magazine-delivery"
              class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent cursor-pointer"
            >
              <option value="Privat adresse">Privat adresse</option>
              <option value="Firma adresse">Firma adresse</option>
            </select>
          </div>

          <div>
            <label for="subscription" class="block text-sm font-medium mb-1"> Abonnement </label>
            <select
              id="subscription"
              v-model="subscription"
              name="subscription"
              class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent cursor-pointer"
            >
              <option value="Alm.">Alm.</option>
              <option value="Firmaopkr.">Firmaopkr.</option>
            </select>
          </div>

          <!-- Company fields - only show if employment status is "Erhvervsaktiv" -->
          <div
            v-if="employmentStatus === 'Erhvervsaktiv'"
            class="space-y-4 flex flex-col gap-4 pt-2 border-t border-theme-border/50"
          >
            <p class="text-sm text-theme-muted italic">Virksomhedsoplysninger</p>

            <div>
              <label for="company-name" class="block text-sm font-medium mb-1">
                Virksomhedsnavn
              </label>
              <input
                id="company-name"
                v-model="companyName"
                name="company-name"
                type="text"
                class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
                placeholder="Virksomhedsnavn"
              />
            </div>

            <div>
              <label for="company-address" class="block text-sm font-medium mb-1"> Adresse </label>
              <input
                id="company-address"
                v-model="companyAddress"
                name="company-address"
                type="text"
                class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
                placeholder="Gade og husnummer"
              />
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label for="company-zip" class="block text-sm font-medium mb-1"> Postnummer </label>
                <input
                  id="company-zip"
                  v-model="companyZIP"
                  name="company-zip"
                  type="text"
                  class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
                  placeholder="Postnummer"
                />
              </div>
              <div>
                <label for="company-city" class="block text-sm font-medium mb-1"> By </label>
                <input
                  id="company-city"
                  v-model="companyCity"
                  name="company-city"
                  type="text"
                  class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
                  placeholder="By"
                />
              </div>
            </div>

            <div>
              <label for="company-phone" class="block text-sm font-medium mb-1">
                Telefonnummer
              </label>
              <input
                id="company-phone"
                v-model="companyPhone"
                name="company-phone"
                type="tel"
                class="w-full bg-theme-soft border border-theme-border rounded-xl p-2 text-theme-heading placeholder-theme-muted focus:outline-none focus:ring-2 focus:ring-theme-accent focus:border-theme-accent"
                placeholder="Telefonnummer"
              />
            </div>
          </div>
        </div>

        <div class="pt-4">
          <button
            type="submit"
            :disabled="loading"
            class="w-full flex justify-center items-center rounded-xl bg-amber-600 h-12 px-8 cursor-pointer hover:bg-amber-500 text-navy-950 font-semibold text-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors shadow-lg shadow-amber-600/20"
          >
            {{ loading ? 'Opretter konto...' : 'Opret konto' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
