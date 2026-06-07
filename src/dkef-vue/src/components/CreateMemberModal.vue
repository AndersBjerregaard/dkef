<script setup lang="ts">
import { ref, type Ref } from 'vue'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import BaseModal from '@/components/BaseModal.vue'
import {
  Section,
  SECTION_DISPLAY_MAP,
  MemberType,
  MEMBER_TYPE_DISPLAY_MAP,
  type CreateMemberDto,
  type Contact,
} from '@/types/members'
import { toast } from 'vue-sonner'
import { AxiosError } from 'axios'

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
const secondarySection = ref<Section | null>(null)
const title = ref('')
const employmentStatus = ref('')
const memberType = ref<MemberType>(MemberType.Member)
const address = ref('')
const city = ref('')
const zip = ref('')
const countryCode = ref('')
const cvrNumber = ref('')
const eanNumber = ref('')
const privatePhoneNumber = ref('')
const attPerson = ref('')
const subscription = ref('')
const companyName = ref('')
const companyAddress = ref('')
const companyZip = ref('')
const companyCity = ref('')
const companyPhone = ref('')
const magazineDelivery = ref('')

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
  title.value = ''
  primarySection.value = Section.Jutland
  secondarySection.value = null
  employmentStatus.value = ''
  address.value = ''
  city.value = ''
  zip.value = ''
  privatePhoneNumber.value = ''
  companyName.value = ''
  companyAddress.value = ''
  companyCity.value = ''
  companyZip.value = ''
  cvrNumber.value = ''
  companyPhone.value = ''
  magazineDelivery.value = ''
  eanNumber.value = ''
  countryCode.value = ''
  attPerson.value = ''
  subscription.value = ''
  memberType.value = MemberType.Member
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
      title: title.value,
      primarySection: primarySection.value,
      secondarySection: secondarySection.value,
      employmentStatus: employmentStatus.value,
      address: address.value,
      city: city.value,
      zip: zip.value,
      privatePhoneNumber: privatePhoneNumber.value,
      companyName: companyName.value,
      companyAddress: companyAddress.value,
      companyCity: companyCity.value,
      companyZip: companyZip.value,
      cvrNumber: cvrNumber.value,
      companyPhone: companyPhone.value,
      magazineDelivery: magazineDelivery.value,
      eanNumber: eanNumber.value,
      countryCode: countryCode.value,
      attPerson: attPerson.value,
      subscription: subscription.value,
      memberType: memberType.value,
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
    if (error instanceof AxiosError && typeof error.response?.data === 'string') {
      const msg = error.response.data.toLowerCase()
      if (msg.includes('email already exists')) {
        errorMessage.value = 'En bruger med denne email eksisterer allerede'
      } else {
        errorMessage.value = msg
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

const memberTypeOptions = Object.entries(MEMBER_TYPE_DISPLAY_MAP).map(([key, value]) => ({
  value: Number(key) as MemberType,
  label: value,
}))
</script>

<template>
  <BaseModal
    :is-open="isOpen"
    title="Opret Nyt Medlem"
    :is-loading="isLoading"
    max-width="max-w-5xl"
    @close="closeModal"
  >
    <form @submit.prevent="createMember">
      <div class="space-y-4">
        <!-- Email, Navn, Titel -->
        <div class="flex gap-4">
          <div class="w-[30%]">
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

          <div class="w-[30%]">
            <label for="name_input" class="block text-sm font-medium">Navn</label>
            <input
              id="name_input"
              type="text"
              v-model="name"
              class="inputField"
              :disabled="isLoading"
            />
          </div>

          <div class="w-[30%]">
            <label for="title_input" class="block text-sm font-medium">Titel</label>
            <input
              id="title_input"
              type="text"
              v-model="title"
              class="inputField"
              :disabled="isLoading"
            />
          </div>
        </div>

        <!-- Primær Sektion, Sekundær Sektion, Beskæftigelse -->
        <div class="flex gap-4">
          <div class="w-[30%]">
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

          <div class="w-[30%]">
            <label for="secondarySection_input" class="block text-sm font-medium">
              Sekundær Sektion
            </label>
            <select
              id="secondarySection_input"
              v-model.number="secondarySection"
              class="inputField"
              :disabled="isLoading"
            >
              <option :value="null">Ingen</option>
              <option v-for="opt in sectionOptions" :key="opt.value" :value="opt.value">
                {{ opt.label }}
              </option>
            </select>
          </div>

          <div class="w-[30%]">
            <label for="employmentStatus_input" class="block text-sm font-medium"
              >Beskæftigelse</label
            >
            <input
              id="employmentStatus_input"
              type="text"
              v-model="employmentStatus"
              class="inputField"
              :disabled="isLoading"
            />
          </div>
        </div>

        <!-- Vejnavn og nr., By, Postnummer -->
        <div class="flex gap-4">
          <div class="w-[30%]">
            <label for="address_input" class="block text-sm font-medium">Vejnavn og nr.</label>
            <input
              id="address_input"
              type="text"
              v-model="address"
              class="inputField"
              :disabled="isLoading"
            />
          </div>

          <div class="w-[30%]">
            <label for="city_input" class="block text-sm font-medium">By</label>
            <input
              id="city_input"
              type="text"
              v-model="city"
              class="inputField"
              :disabled="isLoading"
            />
          </div>

          <div class="w-[30%]">
            <label for="zip_input" class="block text-sm font-medium">Postnummer</label>
            <input
              id="zip_input"
              type="text"
              v-model="zip"
              class="inputField"
              :disabled="isLoading"
            />
          </div>
        </div>

        <!-- Mobil, Firma navn, Firma vejnavn og nr. -->
        <div class="flex gap-4">
          <div class="w-[30%]">
            <label for="privatePhoneNumber_input" class="block text-sm font-medium">Mobil</label>
            <input
              id="privatePhoneNumber_input"
              type="text"
              v-model="privatePhoneNumber"
              class="inputField"
              :disabled="isLoading"
            />
          </div>

          <div class="w-[30%]">
            <label for="companyName_input" class="block text-sm font-medium">Firma navn</label>
            <input
              id="companyName_input"
              type="text"
              v-model="companyName"
              class="inputField"
              :disabled="isLoading"
            />
          </div>

          <div class="w-[30%]">
            <label for="companyAddress_input" class="block text-sm font-medium"
              >Firma vejnavn og nr.</label
            >
            <input
              id="companyAddress_input"
              type="text"
              v-model="companyAddress"
              class="inputField"
              :disabled="isLoading"
            />
          </div>
        </div>

        <!-- Firma by, Firma postnummer, CVR nr. -->
        <div class="flex gap-4">
          <div class="w-[30%]">
            <label for="companyCity_input" class="block text-sm font-medium">Firma by</label>
            <input
              id="companyCity_input"
              type="text"
              v-model="companyCity"
              class="inputField"
              :disabled="isLoading"
            />
          </div>

          <div class="w-[30%]">
            <label for="companyZip_input" class="block text-sm font-medium">Firma postnummer</label>
            <input
              id="companyZip_input"
              type="text"
              v-model="companyZip"
              class="inputField"
              :disabled="isLoading"
            />
          </div>

          <div class="w-[30%]">
            <label for="cvrNumber_input" class="block text-sm font-medium">CVR nr.</label>
            <input
              id="cvrNumber_input"
              type="text"
              v-model="cvrNumber"
              class="inputField"
              :disabled="isLoading"
            />
          </div>
        </div>

        <!-- Firma mobil, Magasin levering, EAN nr. -->
        <div class="flex gap-4">
          <div class="w-[30%]">
            <label for="companyPhone_input" class="block text-sm font-medium">Firma mobil</label>
            <input
              id="companyPhone_input"
              type="text"
              v-model="companyPhone"
              class="inputField"
              :disabled="isLoading"
            />
          </div>

          <div class="w-[30%]">
            <label for="magazineDelivery_input" class="block text-sm font-medium"
              >Magasin levering</label
            >
            <input
              id="magazineDelivery_input"
              type="text"
              v-model="magazineDelivery"
              class="inputField"
              :disabled="isLoading"
            />
          </div>

          <div class="w-[30%]">
            <label for="eanNumber_input" class="block text-sm font-medium">EAN nr.</label>
            <input
              id="eanNumber_input"
              type="text"
              v-model="eanNumber"
              class="inputField"
              :disabled="isLoading"
            />
          </div>
        </div>

        <!-- Landekode, Att. Person, Kontingent -->
        <div class="flex gap-4">
          <div class="w-[30%]">
            <label for="countryCode_input" class="block text-sm font-medium">Landekode</label>
            <input
              id="countryCode_input"
              type="text"
              v-model="countryCode"
              class="inputField"
              :disabled="isLoading"
            />
          </div>

          <div class="w-[30%]">
            <label for="attPerson_input" class="block text-sm font-medium">Att. Person</label>
            <input
              id="attPerson_input"
              type="text"
              v-model="attPerson"
              class="inputField"
              :disabled="isLoading"
            />
          </div>

          <div class="w-[30%]">
            <label for="subscription_input" class="block text-sm font-medium">Kontingent</label>
            <input
              id="subscription_input"
              type="text"
              v-model="subscription"
              class="inputField"
              :disabled="isLoading"
            />
          </div>
        </div>

        <!-- Medlemstype -->
        <div class="flex gap-4">
          <div class="w-[30%]">
            <label for="memberType_input" class="block text-sm font-medium">Medlemstype</label>
            <select
              id="memberType_input"
              v-model.number="memberType"
              class="inputField"
              :disabled="isLoading"
            >
              <option v-for="opt in memberTypeOptions" :key="opt.value" :value="opt.value">
                {{ opt.label }}
              </option>
            </select>
          </div>
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
