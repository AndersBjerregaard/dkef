<script setup lang="ts">
import { type Contact, type ContactDto } from '@/types/members'
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
  props.contact?.firstName,
  props.contact?.email,
  props.contact?.privatePhone,
  props.contact?.primarySection,
  props.contact?.privateAddress,
])

// Editable fields (local reactive copies — never mutate props directly)
const companyName = ref(props.contact?.companyName ?? '') // Firma navn
const companyAddress = ref(props.contact?.companyAddress ?? '') // Firma vejnavn og nr.
const companyCity = ref(props.contact?.companyCity ?? '') // Firma by
const companyZIP = ref(props.contact?.companyZIP ?? '') // Firma postnummer
const cvrNumber = ref(props.contact?.cvrNumber ?? '') // CVR nr.
const companyPhone = ref(props.contact?.companyPhone ?? '') // Firma mobil
const companyEmail = ref(props.contact?.companyEmail ?? '') // Firma e-mail
const title = ref(props.contact?.title ?? '') // Titel
const occupation = ref(props.contact?.occupation ?? '') // Beskæftigelse
const workTasks = ref(props.contact?.workTasks ?? '') // Arbejds Opgaver
const email = ref(props.contact?.email ?? '') // Email
const firstName = ref(props.contact?.firstName ?? '') // Fornavn
const lastName = ref(props.contact?.lastName ?? '') // Efternavn
const primarySection = ref(props.contact?.primarySection ?? '') // Primær sektion
const secondarySection = ref(props.contact?.secondarySection ?? '') // Sekundær sektion
const privateAddress = ref(props.contact?.privateAddress ?? '') // Privat vejnavn og nr.
const privateCity = ref(props.contact?.privateCity ?? '') // Privat by
const privateZIP = ref(props.contact?.privateZIP ?? '') // Privat postnummer
const privatePhone = ref(props.contact?.privatePhone ?? '') // Privat mobil
const elTeknikDelivery = ref(props.contact?.elTeknikDelivery ?? '') // El-teknik levering
const eanNumber = ref(props.contact?.eanNumber ?? '') // EAN nr.
const invoice = ref(props.contact?.invoice ?? '') // Fakturering
const invoiceEmail = ref(props.contact?.invoiceEmail ?? '') // Faktura e-mail
const attInvoice = ref(props.contact?.attInvoice ?? '') // ATT Faktura
const oldMemberNumber = ref(props.contact?.oldMemberNumber ?? '') // Gammelt Medlemsnummer
const helpToStudents = ref(props.contact?.helpToStudents ?? '') // Hjælp til studerende
const mentor = ref(props.contact?.mentor ?? '') // Mentor
const expectedEndDateOfBeingStudent = ref(props.contact?.expectedEndDateOfBeingStudent ?? '') // Hvornår forventer du at være færdig some studerende?

// Computed fields
const memberCreatedAt = computed(() => {
  const date = new Date(props.contact?.createdAt)

  return new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
  }).format(date)
})

async function editMember() {
  isLoading.value = true
  try {
    const editMemberDto: ContactDto = {
      email: email.value,
      firstName: firstName.value,
      lastName: lastName.value,
      title: title.value,
      occupation: occupation.value,
      workTasks: workTasks.value,
      privateAddress: privateAddress.value,
      privateZIP: privateZIP.value,
      privateCity: privateCity.value,
      privatePhone: privatePhone.value,
      companyName: companyName.value,
      companyAddress: companyAddress.value,
      companyZIP: companyZIP.value,
      companyCity: companyCity.value,
      cvrNumber: cvrNumber.value,
      companyPhone: companyPhone.value,
      companyEmail: companyEmail.value,
      elTeknikDelivery: elTeknikDelivery.value,
      eanNumber: eanNumber.value,
      invoice: invoice.value,
      helpToStudents: helpToStudents.value,
      mentor: mentor.value,
      primarySection: primarySection.value,
      secondarySection: secondarySection.value,
      invoiceEmail: invoiceEmail.value,
      oldMemberNumber: oldMemberNumber.value,
      attInvoice: attInvoice.value,
      expectedEndDateOfBeingStudent: expectedEndDateOfBeingStudent.value,
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
      firstName.value = updatedContact.firstName
      lastName.value = updatedContact.lastName
      title.value = updatedContact.title
      occupation.value = updatedContact.occupation
      workTasks.value = updatedContact.workTasks
      privateAddress.value = updatedContact.privateAddress
      privateZIP.value = updatedContact.privateZIP
      privateCity.value = updatedContact.privateCity
      privatePhone.value = updatedContact.privatePhone
      companyName.value = updatedContact.companyName
      companyAddress.value = updatedContact.companyAddress
      companyZIP.value = updatedContact.companyZIP
      companyCity.value = updatedContact.companyCity
      cvrNumber.value = updatedContact.cvrNumber
      companyPhone.value = updatedContact.companyPhone
      companyEmail.value = updatedContact.companyEmail
      elTeknikDelivery.value = updatedContact.elTeknikDelivery
      eanNumber.value = updatedContact.eanNumber
      invoice.value = updatedContact.invoice
      helpToStudents.value = updatedContact.helpToStudents
      mentor.value = updatedContact.mentor
      primarySection.value = updatedContact.primarySection
      secondarySection.value = updatedContact.secondarySection
      invoiceEmail.value = updatedContact.invoiceEmail
      oldMemberNumber.value = updatedContact.oldMemberNumber
      attInvoice.value = updatedContact.attInvoice
      expectedEndDateOfBeingStudent.value = updatedContact.expectedEndDateOfBeingStudent
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
      <div>
        <span>Oprettet den: {{ memberCreatedAt }}</span>
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
          <label for="firstName_input">Fornavn</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="firstName_input"
            type="text"
            v-model="firstName"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="lastName_input">Efternavn</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="lastName_input"
            type="text"
            v-model="lastName"
            :disabled="isLoading || !edit"
          />
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="primarySection_input">Primær Sektion</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="primarySection_input"
            type="text"
            v-model="primarySection"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="secondarySection_input">Sekundær Sektion</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="secondarySection_input"
            type="text"
            v-model="secondarySection"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="privateAddress_input">Privat vejnavn og nr.</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="privateAddress_input"
            type="text"
            v-model="privateAddress"
            :disabled="isLoading || !edit"
          />
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="privateCity_input">Privat by</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="privateCity_input"
            type="text"
            v-model="privateCity"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="privateZIP_input">Privat postnummer</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="privateZIP_input"
            type="text"
            v-model="privateZIP"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="privatePhone_input">Privat mobil</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="privatePhone_input"
            type="text"
            v-model="privatePhone"
            :disabled="isLoading || !edit"
          />
        </div>
      </div>

      <div class="flex justify-between pb-4">
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
      </div>

      <div class="flex justify-between pb-4">
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
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="companyEmail_input">Firma e-mail</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="companyEmail_input"
            type="text"
            v-model="companyEmail"
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
        <div class="w-[30%]">
          <label for="occupation_input">Beskæftigelse</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="occupation_input"
            type="text"
            v-model="occupation"
            :disabled="isLoading || !edit"
          />
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="workTasks_input">Arbejdsopgaver</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="workTasks_input"
            type="text"
            v-model="workTasks"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="elTeknikDelivery_input">El-teknik levering</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="elTeknikDelivery_input"
            type="text"
            v-model="elTeknikDelivery"
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
          <label for="invoice_input">Fakturering</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="invoice_input"
            type="text"
            v-model="invoice"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="invoiceEmail_input">Faktura E-mail</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="invoiceEmail_input"
            type="text"
            v-model="invoiceEmail"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="attInvoice_input">ATT Fakutra</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="attInvoice_input"
            type="text"
            v-model="attInvoice"
            :disabled="isLoading || !edit"
          />
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="oldMemberNumber_input">Gammelt Medlemsnummer</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="oldMemberNumber_input"
            type="text"
            v-model="oldMemberNumber"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="helpToStudents_input">Hjælp til studerende</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="helpToStudents_input"
            type="text"
            v-model="helpToStudents"
            :disabled="isLoading || !edit"
          />
        </div>
        <div class="w-[30%]">
          <label for="mentor_input">Mentor</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="mentor_input"
            type="text"
            v-model="mentor"
            :disabled="isLoading || !edit"
          />
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="expectedEndDateOfBeingStudent_input">Sidste dato som studerende</label>
          <input
            :class="{ 'cursor-not-allowed': !edit }"
            class="inputField"
            id="expectedEndDateOfBeingStudent_input"
            type="text"
            v-model="expectedEndDateOfBeingStudent"
            :disabled="isLoading || !edit"
          />
        </div>
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
