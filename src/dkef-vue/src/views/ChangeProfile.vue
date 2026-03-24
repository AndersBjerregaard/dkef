<script setup lang="ts">
import { ref, computed, onMounted, type Ref } from 'vue'
import { useRouter } from 'vue-router'
import { Toaster, toast } from 'vue-sonner'
import 'vue-sonner/style.css'
import api from '@/services/apiservice'
import { useAuthStore } from '@/stores/authStore'
import type { Contact, UpdateProfileDto, Section } from '@/types/members'
import { Section as SectionEnum, SECTION_DISPLAY_MAP } from '@/types/members'

const router = useRouter()

const authStore = useAuthStore()

// Tab state
const activeTab: Ref<number> = ref(0)

// Loading and error states
const isLoading: Ref<boolean> = ref(false)
const submitError: Ref<string | null> = ref(null)
const passwordChangeError: Ref<string | null> = ref(null)

// Personal Information tab fields
const name: Ref<string> = ref('')
const title: Ref<string> = ref('')
const employmentStatus: Ref<string> = ref('')
const address: Ref<string> = ref('')
const zip: Ref<string> = ref('')
const city: Ref<string> = ref('')
const privatePhoneNumber: Ref<string> = ref('')
const primarySection: Ref<Section | null> = ref(null)
const secondarySection: Ref<Section | null> = ref(null)

// Company Information tab fields
const companyName: Ref<string> = ref('')
const companyAddress: Ref<string> = ref('')
const companyZIP: Ref<string> = ref('')
const companyCity: Ref<string> = ref('')
const cvrNumber: Ref<string> = ref('')
const companyPhone: Ref<string> = ref('')
const magazineDelivery: Ref<string> = ref('')
const eanNumber: Ref<string> = ref('')

// Security tab - password change fields
const currentPassword: Ref<string> = ref('')
const newPassword: Ref<string> = ref('')
const confirmPassword: Ref<string> = ref('')
const passwordChangeLoading: Ref<boolean> = ref(false)

// Security tab - email change fields
const currentEmail: Ref<string> = ref('')
const newEmail: Ref<string> = ref('')
const emailChangeLoading: Ref<boolean> = ref(false)
const emailChangeError: Ref<string | null> = ref(null)

// Enum options
const sectionOptions = computed(() =>
  Object.entries(SECTION_DISPLAY_MAP).map(([key, value]) => ({
    value: Number(key) as Section,
    label: value,
  })),
)

// Computed validation
const personalInfoValid = computed(() => {
  return (
    name.value.trim() !== '' &&
    employmentStatus.value.trim() !== '' &&
    primarySection.value !== null
  )
})

const passwordChangeValid = computed(() => {
  return (
    currentPassword.value.trim() !== '' &&
    newPassword.value.trim() !== '' &&
    confirmPassword.value.trim() !== '' &&
    newPassword.value === confirmPassword.value &&
    newPassword.value.length >= 8
  )
})

const emailChangeValid = computed(() => {
  return newEmail.value.trim() !== '' && newEmail.value.includes('@')
})

// Lifecycle
onMounted(async () => {
  await fetchProfile()
})

// API calls
async function fetchProfile() {
  isLoading.value = true
  try {
    const response = await api.get<Contact>('/profile')
    populateFields(response.data)
  } catch (error: unknown) {
    const axiosError = error as {
      response?: { data?: { message?: string } }
      message?: string
    }
    console.error(error)
    submitError.value =
      axiosError.response?.data?.message ||
      axiosError.message ||
      'Fejl ved hentning af profil. Prøv igen.'
  } finally {
    isLoading.value = false
  }
}

function populateFields(contact: Contact) {
  // Personal info
  name.value = contact.name
  title.value = contact.title
  employmentStatus.value = contact.employmentStatus
  address.value = contact.address
  zip.value = contact.zip
  city.value = contact.city
  privatePhoneNumber.value = contact.privatePhoneNumber
  primarySection.value = contact.primarySection
  secondarySection.value = contact.secondarySection

  // Company info
  companyName.value = contact.companyName
  companyAddress.value = contact.companyAddress
  companyZIP.value = contact.companyZIP
  companyCity.value = contact.companyCity
  cvrNumber.value = contact.cvrNumber
  companyPhone.value = contact.companyPhone
  magazineDelivery.value = contact.magazineDelivery
  eanNumber.value = contact.eanNumber

  // Email for security tab
  currentEmail.value = contact.email
}

async function updateProfile() {
  submitError.value = null

  if (!personalInfoValid.value) {
    submitError.value =
      'Udfyld venligst alle obligatoriske felter: Navn, Beskæftigelse og Primær sektion.'
    return
  }

  isLoading.value = true
  try {
    const updateDto: UpdateProfileDto = {
      name: name.value,
      title: title.value,
      employmentStatus: employmentStatus.value,
      address: address.value,
      zip: zip.value,
      city: city.value,
      companyName: companyName.value,
      companyAddress: companyAddress.value,
      companyZIP: companyZIP.value,
      companyCity: companyCity.value,
      cvrNumber: cvrNumber.value,
      companyPhone: companyPhone.value,
      magazineDelivery: magazineDelivery.value,
      eanNumber: eanNumber.value,
      primarySection: primarySection.value ?? SectionEnum.Jutland,
      secondarySection: secondarySection.value,
    }

    await api.put('/profile', updateDto)
    toast.success('Din profil er opdateret!')
  } catch (error: unknown) {
    const axiosError = error as {
      response?: { data?: { message?: string } }
      message?: string
    }
    console.error(error)
    submitError.value =
      axiosError.response?.data?.message ||
      axiosError.message ||
      'Fejl ved gemming af profil. Prøv igen.'
  } finally {
    isLoading.value = false
  }
}

async function changePassword() {
  passwordChangeError.value = null

  if (!passwordChangeValid.value) {
    if (newPassword.value.length < 8) {
      passwordChangeError.value = 'Den nye adgangskode skal være mindst 8 tegn lang.'
    } else if (newPassword.value !== confirmPassword.value) {
      passwordChangeError.value = 'Adgangskodeordene er ikke ens.'
    } else {
      passwordChangeError.value = 'Udfyld alle felter for at ændre adgangskode.'
    }
    return
  }

  passwordChangeLoading.value = true
  try {
    await api.post('/profile/change-password', {
      currentPassword: currentPassword.value,
      newPassword: newPassword.value,
    })
    toast.success('Adgangskoden er ændret! Du bliver logget ud for sikkerhed.')
    // Clear password fields
    currentPassword.value = ''
    newPassword.value = ''
    confirmPassword.value = ''
    // Logout user after password change for security
    setTimeout(async () => {
      await authStore.logout()
      await router.push({ name: 'home' })
      window.scrollTo({ top: 0, behavior: 'smooth' })
    }, 2000)
  } catch (error: unknown) {
    const axiosError = error as {
      response?: { data?: { message?: string; errors?: string[] } }
      message?: string
    }
    console.error(error)
    if (axiosError.response?.data?.errors && Array.isArray(axiosError.response.data.errors)) {
      passwordChangeError.value = axiosError.response.data.errors.join(', ')
    } else {
      passwordChangeError.value =
        axiosError.response?.data?.message ||
        axiosError.message ||
        'Fejl ved ændring af adgangskode. Prøv igen.'
    }
  } finally {
    passwordChangeLoading.value = false
  }
}

async function changeEmail() {
  emailChangeError.value = null

  if (!emailChangeValid.value) {
    emailChangeError.value = 'Indtast venligst en gyldig e-mail adresse.'
    return
  }

  emailChangeLoading.value = true
  try {
    await api.post('/profile/change-email', {
      newEmail: newEmail.value,
    })
    toast.success('En bekræftelses-email er sendt til din nye e-mail adresse. Tjek din indbakke.')
    newEmail.value = ''
  } catch (error: unknown) {
    const axiosError = error as {
      response?: { data?: { message?: string } }
      message?: string
    }
    console.error(error)
    emailChangeError.value =
      axiosError.response?.data?.message ||
      axiosError.message ||
      'Fejl ved ændring af e-mail. Prøv igen.'
  } finally {
    emailChangeLoading.value = false
  }
}
</script>

<template>
  <Toaster />
  <main class="py-24 px-4 flex justify-center">
    <div class="w-[50%] mx-auto">
      <!-- Header -->
      <div class="flex justify-center items-center text-center mb-12">
        <h1 class="text-4xl font-bold text-theme-heading">Ret Profil</h1>
      </div>

      <!-- Error message at top -->
      <div
        v-if="submitError"
        class="mb-6 rounded-xl bg-red-900 bg-opacity-50 p-4 border border-red-700"
      >
        <p class="text-sm text-red-200">{{ submitError }}</p>
      </div>

      <!-- Tab Navigation -->
      <div class="flex gap-2 mb-8 border-b border-theme-border">
        <button
          @click="activeTab = 0"
          :class="[
            'px-4 py-3 font-medium transition-colors cursor-pointer',
            activeTab === 0
              ? 'text-theme-accent border-b-2 border-theme-accent'
              : 'text-theme-muted hover:text-theme-heading',
          ]"
        >
          Personlige Oplysninger
        </button>
        <button
          @click="activeTab = 1"
          :class="[
            'px-4 py-3 font-medium transition-colors cursor-pointer',
            activeTab === 1
              ? 'text-theme-accent border-b-2 border-theme-accent'
              : 'text-theme-muted hover:text-theme-heading',
          ]"
        >
          Firma Oplysninger
        </button>
        <button
          @click="activeTab = 2"
          :class="[
            'px-4 py-3 font-medium transition-colors cursor-pointer',
            activeTab === 2
              ? 'text-theme-accent border-b-2 border-theme-accent'
              : 'text-theme-muted hover:text-theme-heading',
          ]"
        >
          Sikkerhed
        </button>
      </div>

      <!-- Tab Content -->
      <form @submit.prevent="updateProfile" class="space-y-8">
        <!-- Tab 0: Personal Information -->
        <div v-if="activeTab === 0" class="space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
            <!-- Name -->
            <div>
              <label for="name" class="block text-sm font-medium text-theme-heading mb-2">
                Navn <span class="text-red-500">*</span>
              </label>
              <input
                id="name"
                v-model="name"
                type="text"
                class="inputField w-full"
                required
                :disabled="isLoading"
              />
            </div>

            <!-- Title -->
            <div>
              <label for="title" class="block text-sm font-medium text-theme-heading mb-2">
                Titel
              </label>
              <input
                id="title"
                v-model="title"
                type="text"
                class="inputField w-full"
                :disabled="isLoading"
              />
            </div>

            <!-- EmploymentStatus -->
            <div>
              <label
                for="employmentStatus"
                class="block text-sm font-medium text-theme-heading mb-2"
              >
                Beskæftigelse <span class="text-red-500">*</span>
              </label>
              <input
                id="employmentStatus"
                v-model="employmentStatus"
                type="text"
                class="inputField w-full"
                required
                :disabled="isLoading"
              />
            </div>
          </div>

          <!-- Address -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div>
              <label for="address" class="block text-sm font-medium text-theme-heading mb-2">
                Vejnavn og nr.
              </label>
              <input
                id="address"
                v-model="address"
                type="text"
                class="inputField w-full"
                :disabled="isLoading"
              />
            </div>

            <div class="grid grid-cols-2 gap-6">
              <div>
                <label for="zip" class="block text-sm font-medium text-theme-heading mb-2">
                  Postnummer
                </label>
                <input
                  id="zip"
                  v-model="zip"
                  type="text"
                  class="inputField w-full"
                  :disabled="isLoading"
                />
              </div>
              <div>
                <label for="city" class="block text-sm font-medium text-theme-heading mb-2">
                  By
                </label>
                <input
                  id="city"
                  v-model="city"
                  type="text"
                  class="inputField w-full"
                  :disabled="isLoading"
                />
              </div>
            </div>
          </div>

          <!-- Phone Number -->
          <div>
            <label
              for="privatePhoneNumber"
              class="block text-sm font-medium text-theme-heading mb-2"
            >
              Mobil
            </label>
            <input
              id="privatePhoneNumber"
              v-model="privatePhoneNumber"
              type="tel"
              class="inputField w-full"
              :disabled="isLoading"
            />
          </div>

          <!-- Primary & Secondary Sections -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-8 pb-4">
            <div>
              <label for="primarySection" class="block text-sm font-medium text-theme-heading mb-2">
                Primær sektion <span class="text-red-500">*</span>
              </label>
              <select
                id="primarySection"
                v-model.number="primarySection"
                class="inputField w-full"
                required
                :disabled="isLoading"
              >
                <option :value="null">Vælg sektion</option>
                <option
                  v-for="section in sectionOptions"
                  :key="section.value"
                  :value="section.value"
                >
                  {{ section.label }}
                </option>
              </select>
            </div>

            <div>
              <label
                for="secondarySection"
                class="block text-sm font-medium text-theme-heading mb-2"
              >
                Sekundær sektion
              </label>
              <select
                id="secondarySection"
                v-model.number="secondarySection"
                class="inputField w-full"
                :disabled="isLoading"
              >
                <option :value="null">Ingen</option>
                <option
                  v-for="section in sectionOptions"
                  :key="section.value"
                  :value="section.value"
                >
                  {{ section.label }}
                </option>
              </select>
            </div>
          </div>
        </div>

        <!-- Tab 1: Company Information -->
        <div v-if="activeTab === 1" class="space-y-8">
          <!-- Company Name -->
          <div>
            <label for="companyName" class="block text-sm font-medium text-theme-heading mb-2">
              Firma navn
            </label>
            <input
              id="companyName"
              v-model="companyName"
              type="text"
              class="inputField w-full"
              :disabled="isLoading"
            />
          </div>

          <!-- Company Address -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div>
              <label for="companyAddress" class="block text-sm font-medium text-theme-heading mb-2">
                Firma vejnavn og nr.
              </label>
              <input
                id="companyAddress"
                v-model="companyAddress"
                type="text"
                class="inputField w-full"
                :disabled="isLoading"
              />
            </div>

            <div class="grid grid-cols-2 gap-6">
              <div>
                <label for="companyZIP" class="block text-sm font-medium text-theme-heading mb-2">
                  Postnummer
                </label>
                <input
                  id="companyZIP"
                  v-model="companyZIP"
                  type="text"
                  class="inputField w-full"
                  :disabled="isLoading"
                />
              </div>
              <div>
                <label for="companyCity" class="block text-sm font-medium text-theme-heading mb-2">
                  By
                </label>
                <input
                  id="companyCity"
                  v-model="companyCity"
                  type="text"
                  class="inputField w-full"
                  :disabled="isLoading"
                />
              </div>
            </div>
          </div>

          <!-- CVR & Company Phone -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div>
              <label for="cvrNumber" class="block text-sm font-medium text-theme-heading mb-2">
                CVR nr.
              </label>
              <input
                id="cvrNumber"
                v-model="cvrNumber"
                type="text"
                class="inputField w-full"
                :disabled="isLoading"
              />
            </div>

            <div>
              <label for="companyPhone" class="block text-sm font-medium text-theme-heading mb-2">
                Firma mobil
              </label>
              <input
                id="companyPhone"
                v-model="companyPhone"
                type="tel"
                class="inputField w-full"
                :disabled="isLoading"
              />
            </div>
          </div>

          <!-- Magazine Delivery & EAN -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div>
              <label
                for="magazineDelivery"
                class="block text-sm font-medium text-theme-heading mb-2"
              >
                Magasin levering
              </label>
              <input
                id="magazineDelivery"
                v-model="magazineDelivery"
                type="text"
                class="inputField w-full"
                :disabled="isLoading"
              />
            </div>

            <div>
              <label for="eanNumber" class="block text-sm font-medium text-theme-heading mb-2">
                EAN nr.
              </label>
              <input
                id="eanNumber"
                v-model="eanNumber"
                type="text"
                class="inputField w-full"
                :disabled="isLoading"
              />
            </div>
          </div>
        </div>

        <!-- Tab 2: Security -->
        <div v-if="activeTab === 2" class="space-y-8">
          <!-- Change Password Section -->
          <div class="border-b border-theme-border">
            <h2 class="text-2xl font-bold text-theme-heading mb-6">Ændre Adgangskode</h2>

            <div
              v-if="passwordChangeError"
              class="mb-4 rounded-xl bg-red-900 bg-opacity-50 p-4 border border-red-700"
            >
              <p class="text-sm text-red-200">{{ passwordChangeError }}</p>
            </div>

            <div class="space-y-4">
              <div>
                <label
                  for="currentPassword"
                  class="block text-sm font-medium text-theme-heading mb-2"
                >
                  Nuværende adgangskode <span class="text-red-500">*</span>
                </label>
                <input
                  id="currentPassword"
                  v-model="currentPassword"
                  type="password"
                  class="inputField w-full"
                  required
                  :disabled="passwordChangeLoading"
                />
              </div>

              <div>
                <label for="newPassword" class="block text-sm font-medium text-theme-heading mb-2">
                  Ny adgangskode <span class="text-red-500">*</span>
                </label>
                <input
                  id="newPassword"
                  v-model="newPassword"
                  type="password"
                  class="inputField w-full"
                  required
                  :disabled="passwordChangeLoading"
                  placeholder="Minimum 8 tegn"
                />
              </div>

              <div>
                <label
                  for="confirmPassword"
                  class="block text-sm font-medium text-theme-heading mb-2"
                >
                  Bekræft ny adgangskode <span class="text-red-500">*</span>
                </label>
                <input
                  id="confirmPassword"
                  v-model="confirmPassword"
                  type="password"
                  class="inputField w-full"
                  required
                  :disabled="passwordChangeLoading"
                />
              </div>

              <div class="mb-4">
                <button
                  @click="changePassword"
                  type="button"
                  :disabled="passwordChangeLoading || !passwordChangeValid"
                  class="w-full md:w-auto rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <span v-if="!passwordChangeLoading">Gem Ny Adgangskode</span>
                  <span v-else>Gemmer...</span>
                </button>
              </div>
            </div>
          </div>

          <!-- Change Email Section -->
          <div>
            <h2 class="text-2xl font-bold text-theme-heading mb-6">Ændre E-mail Adresse</h2>

            <div
              v-if="emailChangeError"
              class="mb-4 rounded-xl bg-red-900 bg-opacity-50 p-4 border border-red-700"
            >
              <p class="text-sm text-red-200">{{ emailChangeError }}</p>
            </div>

            <div class="space-y-4">
              <div class="p-4 rounded-xl bg-theme-soft border border-theme-border">
                <p class="text-sm text-theme-muted mb-2">Nuværende e-mail adresse</p>
                <p class="text-lg text-theme-heading font-medium">{{ currentEmail }}</p>
              </div>

              <div>
                <label for="newEmail" class="block text-sm font-medium text-theme-heading mb-2">
                  Ny e-mail adresse <span class="text-red-500">*</span>
                </label>
                <input
                  id="newEmail"
                  v-model="newEmail"
                  type="email"
                  class="inputField w-full"
                  required
                  :disabled="emailChangeLoading"
                  placeholder="din-nye@email.com"
                />
              </div>

              <div class="p-4 rounded-xl bg-blue-900 bg-opacity-50 border border-blue-700">
                <p class="text-sm text-blue-200">
                  <strong>Bemærk:</strong> En bekræftelses-email vil blive sendt til din nye e-mail
                  adresse. Du skal bekræfte ændringen ved at klikke på linket i emailen.
                </p>
              </div>

              <button
                @click="changeEmail"
                type="button"
                :disabled="emailChangeLoading || !emailChangeValid"
                class="w-full md:w-auto rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <span v-if="!emailChangeLoading">Send Bekræftelses-Email</span>
                <span v-else>Sender...</span>
              </button>
            </div>
          </div>
        </div>

        <!-- Submit button - only shown for tabs 0 and 1 -->
        <div v-if="activeTab !== 2" class="pt-6 border-t border-theme-border flex gap-4">
          <button
            type="submit"
            :disabled="isLoading || !personalInfoValid"
            class="rounded-lg bg-amber-600 h-12 py-2 px-6 cursor-pointer hover:bg-amber-500 active:bg-amber-700 text-navy-950 font-semibold text-base shadow-lg shadow-amber-600/20 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <span v-if="!isLoading">Gem Ændringer</span>
            <span v-else>Gemmer...</span>
          </button>
        </div>
      </form>
    </div>
  </main>
</template>

<style scoped>
.inputField {
  width: 100%;
  background-color: var(--color-theme-soft);
  border: 1.5px solid var(--color-theme-border);
  border-radius: 0.75rem;
  padding: 0.75rem 1rem;
  font-size: 1rem;
  line-height: 1.5;
  transition: all 0.2s ease;
  color: var(--color-text);
}

.inputField:focus {
  outline: none;
  border-color: var(--color-accent);
  box-shadow:
    0 0 0 3px var(--color-accent-muted),
    0 0 0 1.5px var(--color-accent);
  background-color: var(--color-background);
}

.inputField:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  background-color: var(--color-background-mute);
}

.inputField::placeholder {
  color: var(--color-muted);
  opacity: 0.7;
}
</style>
