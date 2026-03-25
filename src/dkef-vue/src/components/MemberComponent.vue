<script setup lang="ts">
import {
  type Contact,
  type ContactDto,
  Section,
  SECTION_DISPLAY_MAP,
  MemberType,
  MEMBER_TYPE_DISPLAY_MAP,
} from '@/types/members'
import { computed, ref } from 'vue'
import BaseModal from '@/components/BaseModal.vue'
import LoadingButton from '@/components/LoadingButton.vue'
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import DeleteMemberModal from '@/components/DeleteMemberModal.vue'

// Modal state
const isOpen = ref(false)
const isLoading = ref(false)
const editState = ref(0) // -1 for error, 0 for none, 1 for update
const isDeleteOpen = ref(false)

const localDateTime = computed(() => {
  const now = new Date()
  return now.toLocaleString(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
  })
})

function openModal() {
  isOpen.value = true
}

function closeModal() {
  resetFields()
  isOpen.value = false
}

function resetFields() {
  editState.value = 0
  edit.value = false
}

const props = defineProps<{ contact: Contact; index: number }>()
const emit = defineEmits<{
  (e: 'contact-updated', contact: Contact): void
  (e: 'contact-deleted', id: string): void
}>()

// Shown fields
const fields = computed(() => [
  props.contact?.name,
  props.contact?.email,
  props.contact?.privatePhoneNumber,
  props.contact?.primarySection !== null ? SECTION_DISPLAY_MAP[props.contact.primarySection] : '',
  props.contact?.address,
])

// Editable fields (local reactive copies — never mutate props directly)
const email = ref(props.contact?.email ?? '') // Email
const name = ref(props.contact?.name ?? '') // Navn
const address = ref(props.contact?.address ?? '') // Vejnavn og nr.
const zip = ref(props.contact?.zip ?? '') // Postnummer
const city = ref(props.contact?.city ?? '') // By
const countryCode = ref(props.contact?.countryCode ?? '') // Landekode
const cvrNumber = ref(props.contact?.cvrNumber ?? '') // CVR nr.
const eanNumber = ref(props.contact?.eanNumber ?? '') // EAN nr.
const privatePhoneNumber = ref(props.contact?.privatePhoneNumber ?? '') // Mobil
const attPerson = ref(props.contact?.attPerson ?? '') // Att Person
const enrollmentDate = ref(props.contact?.enrollmentDate ?? '') // Indmeldingsdato
const subscription = ref(props.contact?.subscription ?? '') // Kontingent
const invoiceName2 = ref(props.contact?.invoiceName2 ?? '') // Faktura navn 2
const companyName = ref(props.contact?.companyName ?? '') // Firma navn
const companyAddress = ref(props.contact?.companyAddress ?? '') // Firma vejnavn og nr.
const companyZIP = ref(props.contact?.companyZIP ?? '') // Firma postnummer
const companyCity = ref(props.contact?.companyCity ?? '') // Firma by
const companyPhone = ref(props.contact?.companyPhone ?? '') // Firma mobil
const employmentStatus = ref(props.contact?.employmentStatus ?? '') // Beskæftigelse
const primarySection = ref(props.contact?.primarySection ?? (Section.Jutland as Section | null)) // Primær sektion
const secondarySection = ref(props.contact?.secondarySection ?? (null as Section | null)) // Sekundær sektion
const magazineDelivery = ref(props.contact?.magazineDelivery ?? '') // Magasin levering
const title = ref(props.contact?.title ?? '') // Titel
const memberType = ref(props.contact?.memberType ?? (MemberType.Member as MemberType | null))

// Enum options
const sectionOptions = computed(() =>
  Object.entries(SECTION_DISPLAY_MAP).map(([key, value]) => ({
    value: Number(key) as Section,
    label: value,
  })),
)

const memberTypeOptions = computed(() =>
  Object.entries(MEMBER_TYPE_DISPLAY_MAP).map(([key, value]) => ({
    value: Number(key) as MemberType,
    label: value,
  })),
)

async function editMember() {
  isLoading.value = true
  try {
    const editMemberDto: ContactDto = {
      email: email.value,
      name: name.value,
      address: address.value,
      zip: zip.value,
      city: city.value,
      countryCode: countryCode.value,
      cvrNumber: cvrNumber.value,
      eanNumber: eanNumber.value,
      privatePhoneNumber: privatePhoneNumber.value,
      attPerson: attPerson.value,
      enrollmentDate: enrollmentDate.value,
      subscription: subscription.value,
      invoiceName2: invoiceName2.value,
      companyName: companyName.value,
      companyAddress: companyAddress.value,
      companyZIP: companyZIP.value,
      companyCity: companyCity.value,
      companyPhone: companyPhone.value,
      employmentStatus: employmentStatus.value,
      primarySection: primarySection.value ?? Section.Jutland,
      secondarySection: secondarySection.value,
      magazineDelivery: magazineDelivery.value,
      title: title.value,
      memberType: memberType.value,
    }
    const response = await apiservice.put<Contact>(
      urlservice.updateContact(props.contact?.id),
      editMemberDto,
    )
    if (response.status >= 200 && response.status < 300) {
      editState.value = 1 // Indicate edit success
      // Update local refs to reflect the saved values
      const updatedContact = response.data
      email.value = updatedContact.email
      name.value = updatedContact.name
      address.value = updatedContact.address
      zip.value = updatedContact.zip
      city.value = updatedContact.city
      countryCode.value = updatedContact.countryCode
      cvrNumber.value = updatedContact.cvrNumber
      eanNumber.value = updatedContact.eanNumber
      privatePhoneNumber.value = updatedContact.privatePhoneNumber
      attPerson.value = updatedContact.attPerson
      enrollmentDate.value = updatedContact.enrollmentDate
      subscription.value = updatedContact.subscription
      invoiceName2.value = updatedContact.invoiceName2
      companyName.value = updatedContact.companyName
      companyAddress.value = updatedContact.companyAddress
      companyZIP.value = updatedContact.companyZIP
      companyCity.value = updatedContact.companyCity
      companyPhone.value = updatedContact.companyPhone
      employmentStatus.value = updatedContact.employmentStatus
      primarySection.value = updatedContact.primarySection
      secondarySection.value = updatedContact.secondarySection
      magazineDelivery.value = updatedContact.magazineDelivery
      title.value = updatedContact.title
      memberType.value = updatedContact.memberType
      // Notify parent so the list row stays in sync
      emit('contact-updated', updatedContact)
    } else {
      throw `Unexpected error attempting to update contact information ${response}`
    }
  } catch (error) {
    console.error(error)
  } finally {
    isLoading.value = false
  }
}

const authorizing = ref(false)

async function authorize() {
  authorizing.value = true

  try {
    const response = await apiservice.get(urlservice.getContactAuthorize(props.contact?.id))
    if (response.status >= 200 && response.status < 300) {
      edit.value = true
    } else {
      throw 'Received non-success status from authorization of contact'
    }
  } catch (error) {
    console.error(error)
  } finally {
    authorizing.value = false
  }
}

const edit = ref(false)
const hasAccess = ref(true)
</script>

<template>
  <div
    class="border-x-2 border-theme-border w-full justify-between flex hover:bg-theme-border hover:text-theme-accent cursor-pointer transition-colors"
    @click="openModal"
    :class="{ 'bg-theme-base': index % 2 == 0, 'bg-theme-soft': index % 2 == 1 }"
  >
    <div
      v-for="(field, i) in fields"
      :key="i"
      class="border-2 border-theme-border h-10 py-1 px-1 flex-1 min-w-0 flex justify-center"
    >
      <span class="truncate">{{ field }}</span>
    </div>
  </div>
  <BaseModal
    :is-open="isOpen"
    :title="'Medlem Information'"
    :is-loading="isLoading"
    max-width="max-w-5xl"
    @close="closeModal"
  >
    <div class="pb-4 h-14 flex justify-between" v-if="hasAccess">
      <div>
        <div v-if="!edit">
          <LoadingButton
            @loading-button-click="authorize"
            default-text="Autorisér"
            loading-text="Autoriserer..."
            :is-loading="authorizing"
          />
        </div>
        <div v-else class="text-lg text-green-600">
          <span>Autoriseret</span>
        </div>
      </div>
    </div>
    <form @submit.prevent="editMember">
      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="email_input">Email</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="email_input"
            type="text"
            v-model="email"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="name_input">Navn</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="name_input"
            type="text"
            v-model="name"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="title_input">Titel</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="title_input"
            type="text"
            v-model="title"
            :disabled="isLoading || !edit"
          />
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="primarySection_input">Primær Sektion</label>
          <select
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="primarySection_input"
            v-model.number="primarySection"
            :disabled="isLoading || !edit"
          >
            <option v-for="section in sectionOptions" :key="section.value" :value="section.value">
              {{ section.label }}
            </option>
          </select>
        </div>
        <div class="w-[30%]">
          <label for="secondarySection_input">Sekundær Sektion</label>
          <select
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="secondarySection_input"
            v-model.number="secondarySection"
            :disabled="isLoading || !edit"
          >
            <option :value="null">Ingen</option>
            <option v-for="section in sectionOptions" :key="section.value" :value="section.value">
              {{ section.label }}
            </option>
          </select>
        </div>
        <div class="w-[30%]">
          <label for="employmentStatus_input">Beskæftigelse</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="employmentStatus_input"
            type="text"
            v-model="employmentStatus"
            :disabled="isLoading || !edit"
          />
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="address_input">Vejnavn og nr.</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="address_input"
            type="text"
            v-model="address"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="city_input">By</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="city_input"
            type="text"
            v-model="city"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="zip_input">Postnummer</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="zip_input"
            type="text"
            v-model="zip"
            :disabled="isLoading || !edit"
          />
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="privatePhoneNumber_input">Mobil</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="privatePhoneNumber_input"
            type="text"
            v-model="privatePhoneNumber"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="companyName_input">Firma navn</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="companyName_input"
            type="text"
            v-model="companyName"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="companyAddress_input">Firma vejnavn og nr.</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="companyAddress_input"
            type="text"
            v-model="companyAddress"
            :disabled="isLoading || !edit"
          />
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="companyCity_input">Firma by</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="companyCity_input"
            type="text"
            v-model="companyCity"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="companyZIP_input">Firma postnummer</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="companyZIP_input"
            type="text"
            v-model="companyZIP"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="cvrNumber_input">CVR nr.</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="cvrNumber_input"
            type="text"
            v-model="cvrNumber"
            :disabled="isLoading || !edit"
          />
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="companyPhone_input">Firma mobil</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="companyPhone_input"
            type="text"
            v-model="companyPhone"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="magazineDelivery_input">Magasin levering</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="magazineDelivery_input"
            type="text"
            v-model="magazineDelivery"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="eanNumber_input">EAN nr.</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="eanNumber_input"
            type="text"
            v-model="eanNumber"
            :disabled="isLoading || !edit"
          />
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="countryCode_input">Landekode</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="countryCode_input"
            type="text"
            v-model="countryCode"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="attPerson_input">Att. Person</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="attPerson_input"
            type="text"
            v-model="attPerson"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="subscription_input">Kontingent</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="subscription_input"
            type="text"
            v-model="subscription"
            :disabled="isLoading || !edit"
          />
        </div>
      </div>

      <div class="w-[30%]">
        <label for="memberType_input">Medlemstype</label>
        <select
          :class="{ 'cursor-not-allowed': !edit }"
          class="inputField"
          id="memberType_input"
          v-model.number="memberType"
          :disabled="isLoading || !edit"
        >
          <option v-for="type in memberTypeOptions" :key="type.value" :value="type.value">
            {{ type.label }}
          </option>
        </select>
      </div>

      <div class="flex">
        <div v-if="hasAccess && edit" class="pt-4 flex gap-8">
          <LoadingButton
            default-text="Opdater"
            loading-text="Opdaterer..."
            :is-loading="isLoading"
            @loading-button-click="editMember"
          />
          <div class="mt-4">
            <button
              type="button"
              :disabled="isLoading"
              :class="{
                'cursor-pointer': !isLoading,
                'cursor-not-allowed': isLoading,
                'hover:bg-gray-400': !isLoading,
              }"
              class="inline-flex justify-center rounded-md border border-transparent bg-theme-mute px-4 py-2 font-medium focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent text-theme-heading hover:bg-theme-border hover:text-theme-accent focus-visible:ring-offset-2 transition-colors"
              @click="closeModal"
            >
              <span>Fortryd</span>
            </button>
          </div>
          <div class="mt-4">
            <button
              type="button"
              :disabled="isLoading"
              :class="{
                'cursor-pointer': !isLoading,
                'cursor-not-allowed': isLoading,
                'hover:bg-gray-400': !isLoading,
              }"
              class="inline-flex justify-center rounded-md border border-transparent bg-red-400 px-4 py-2 font-medium focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent text-theme-heading hover:bg-red-900 hover:text-theme-accent focus-visible:ring-offset-2 transition-colors"
              @click="isDeleteOpen = true"
            >
              Slet
            </button>
          </div>
          <div v-if="editState != 0" class="w-80 pt-2">
            <span class="text-green-600">Opdateret: {{ localDateTime }}</span>
          </div>
        </div>
        <div class="pt-4 w-full flex justify-end">
          <button
            type="button"
            :disabled="isLoading"
            :class="{
              'cursor-pointer': !isLoading,
              'cursor-not-allowed': isLoading,
              'hover:bg-gray-400': !isLoading,
            }"
            class="inline-flex justify-center rounded-md border border-transparent bg-theme-mute px-4 py-2 font-medium focus:outline-none focus-visible:ring-2 focus-visible:ring-theme-accent text-theme-heading hover:bg-theme-border hover:text-theme-accent focus-visible:ring-offset-2 transition-colors"
            @click="closeModal"
          >
            <span>Luk</span>
          </button>
        </div>
      </div>
    </form>
  </BaseModal>
  <DeleteMemberModal
    :is-open="isDeleteOpen"
    :id="contact.id"
    @close="isDeleteOpen = false"
    @deleted="(id) => emit('contact-deleted', id)"
  />
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
