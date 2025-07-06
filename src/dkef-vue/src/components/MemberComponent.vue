<script setup lang="ts">
import { type Contact, type ContactDto } from '@/types/members'
import { computed, ref } from 'vue'
import BaseModal from '@/components/BaseModal.vue';
import LoadingButton from '@/components/LoadingButton.vue'
import apiservice from '@/services/apiservice';
import urlservice from '@/services/urlservice';

// Modal state
const isOpen = ref(false);
const isLoading = ref(false);
const editState = ref(0); // -1 for error, 0 for none, 1 for update

const localDateTime = computed(() => {
  const now = new Date();
  return now.toLocaleString(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
  });
});

function openModal() {
  isOpen.value = true;
}

function closeModal() {
  resetFields();
  isOpen.value = false;
}

function resetFields() {
  edit.value = false;
}

const props = defineProps<{ contact: Contact, index: number }>();

// Shown fields
const fields = computed(() => [
  props.contact?.firstName,
  props.contact?.email,
  props.contact?.privatePhone,
  props.contact?.primarySection,
  props.contact?.privateAddress
]);

// Editable fields
let companyName = props.contact?.companyName; // Firma navn
let companyAddress = props.contact?.companyAddress; // Firma vejnavn og nr.
let companyCity = props.contact?.companyCity; // Firma by
let companyZIP = props.contact?.companyZIP; // Firma postnummer
let cvrNumber = props.contact?.cvrNumber; // CVR nr.
let companyPhone = props.contact?.companyPhone; // Firma mobil
let companyEmail = props.contact?.companyEmail; // Firma e-mail
let title = props.contact?.title; // Titel
let occupation = props.contact?.occupation; // Beskæftigelse
let workTasks = props.contact?.workTasks; // Arbejds Opgaver
let email = props.contact?.email; // Email
let firstName = props.contact?.firstName; // Fornavn
let lastName = props.contact?.lastName; // Efternavn
let primarySection = props.contact?.primarySection; // Primær sektion
let secondarySection = props.contact?.secondarySection; // Sekundær sektion
let privateAddress = props.contact?.privateAddress; // Privat vejnavn og nr.
let privateCity = props.contact?.privateCity; // Privat by
let privateZIP = props.contact?.privateZIP; // Privat postnummer
let privatePhone = props.contact?.privatePhone; // Privat mobil
let elTeknikDelivery = props.contact?.elTeknikDelivery; // El-teknik levering
let eanNumber = props.contact?.eanNumber; // EAN nr.
let invoice = props.contact?.invoice; // Fakturering
let invoiceEmail = props.contact?.invoiceEmail; // Faktura e-mail
let attInvoice = props.contact?.attInvoice; // ATT Faktura
let oldMemberNumber = props.contact?.oldMemberNumber; // Gammelt Medlemsnummer
let helpToStudents = props.contact?.helpToStudents; // Hjælp til studerende
let mentor = props.contact?.mentor; // Mentor
let expectedEndDateOfBeingStudent = props.contact?.expectedEndDateOfBeingStudent; // Hvornår forventer du at være færdig some studerende?

// Computed fields
const memberCreatedAt = computed(() => {
  const date = new Date(props.contact?.createdAt);

  return new Intl.DateTimeFormat(undefined, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric',
  }).format(date);
});

async function editMember() {
  isLoading.value = true;
  try {
    const editMemberDto: ContactDto = {
      email: email,
      firstName: firstName,
      lastName: lastName,
      title: title,
      occupation: occupation,
      workTasks: workTasks,
      privateAddress: privateAddress,
      privateZIP: privateZIP,
      privateCity: privateCity,
      privatePhone: privatePhone,
      companyName: companyName,
      companyAddress: companyAddress,
      companyZIP: companyZIP,
      companyCity: companyCity,
      cvrNumber: cvrNumber,
      companyPhone: companyPhone,
      companyEmail: companyEmail,
      elTeknikDelivery: elTeknikDelivery,
      eanNumber: eanNumber,
      invoice: invoice,
      helpToStudents: helpToStudents,
      mentor: mentor,
      primarySection: primarySection,
      secondarySection: secondarySection,
      invoiceEmail: invoiceEmail,
      oldMemberNumber: oldMemberNumber,
      attInvoice: attInvoice,
      expectedEndDateOfBeingStudent: expectedEndDateOfBeingStudent
    };
    const response = await apiservice.put<Contact>(urlservice.updateContact(props.contact?.id), editMemberDto);
    if (response.status >= 200 && response.status < 300) {
      editState.value = 1; // Indicate edit success
      // Update properties to reflect the member list
      const updatedContact = response.data;
      props.contact.email = updatedContact.email;
      props.contact.firstName = updatedContact.firstName;
      props.contact.lastName = updatedContact.lastName;
      props.contact.title = updatedContact.title;
      props.contact.occupation = updatedContact.occupation;
      props.contact.workTasks = updatedContact.workTasks;
      props.contact.privateAddress = updatedContact.privateAddress;
      props.contact.privateZIP = updatedContact.privateZIP;
      props.contact.privateCity = updatedContact.privateCity;
      props.contact.privatePhone = updatedContact.privatePhone;
      props.contact.companyName = updatedContact.companyName;
      props.contact.companyAddress = updatedContact.companyAddress;
      props.contact.companyZIP = updatedContact.companyZIP;
      props.contact.cvrNumber = updatedContact.cvrNumber;
      props.contact.companyPhone = updatedContact.companyPhone;
      props.contact.companyEmail = updatedContact.companyEmail;
      props.contact.elTeknikDelivery = updatedContact.elTeknikDelivery;
      props.contact.eanNumber = updatedContact.eanNumber;
      props.contact.invoice = updatedContact.invoice;
      props.contact.helpToStudents = updatedContact.helpToStudents;
      props.contact.mentor = updatedContact.mentor;
      props.contact.primarySection = updatedContact.primarySection;
      props.contact.secondarySection = updatedContact.secondarySection;
      props.contact.invoiceEmail = updatedContact.invoiceEmail;
      props.contact.oldMemberNumber = updatedContact.oldMemberNumber;
      props.contact.attInvoice = updatedContact.attInvoice;
      props.contact.expectedEndDateOfBeingStudent = updatedContact.expectedEndDateOfBeingStudent;
    } else {
      throw `Unexpected error attempting to update contact information ${response}`;
    }
  } catch (error) {
    console.error(error);
  } finally {
    isLoading.value = false;
  }
}

const authorizing = ref(false);

async function authorize() {
  authorizing.value = true;

  try {
    const response = await apiservice.get(urlservice.getContactAuthorize(props.contact?.id));
    if (response.status >= 200 && response.status < 300) {
      edit.value = true;
    } else {
      throw 'Received non-success status from authorization of contact'
    }
  } catch (error) {
    console.error(error);
  } finally {
    authorizing.value = false;
  }
}

const edit = ref(false);
const hasAccess = ref(true);

</script>

<template>
  <div
    class="border-x-2 border-gray-900 w-full justify-between flex hover:bg-gray-200 hover:text-gray-900 cursor-pointer"
    @click="openModal" :class="{ 'bg-black': index % 2 == 0, 'bg-gray-800': index % 2 == 1 }">
    <div v-for="(field, i) in fields" :key="i"
      class="border-2 border-gray-900 h-10 py-1 px-1 flex-1 min-w-0 flex justify-center">
      <span class="truncate">{{ field }}</span>
    </div>
  </div>
  <BaseModal :is-open="isOpen" :title="'Medlem Information'" :is-loading="isLoading" @close="closeModal">
    <div class="pb-4 h-14 flex justify-between" v-if="hasAccess">
      <div>
        <div v-if="!edit">
          <LoadingButton @loading-button-click="authorize" default-text="Autorisér" loading-text="Autoriserer..."
            :is-loading="authorizing" />
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
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="email_input" type="text"
            v-model="email" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="firstName_input">Fornavn</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="firstName_input" type="text"
            v-model="firstName" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="lastName_input">Efternavn</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="lastName_input" type="text"
            v-model="lastName" :disabled="isLoading || !edit">
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="primarySection_input">Primær Sektion</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="primarySection_input" type="text"
            v-model="primarySection" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="secondarySection_input">Sekundær Sektion</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="secondarySection_input" type="text"
            v-model="secondarySection" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="privateAddress_input">Privat vejnavn og nr.</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="privateAddress_input" type="text"
            v-model="privateAddress" :disabled="isLoading || !edit">
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="privateCity_input">Privat by</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="privateCity_input" type="text"
            v-model="privateCity" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="privateZIP_input">Privat postnummer</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="privateZIP_input" type="text"
            v-model="privateZIP" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="privatePhone_input">Privat mobil</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="privatePhone_input" type="text"
            v-model="privatePhone" :disabled="isLoading || !edit">
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="companyName_input">Firma navn</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="companyName_input" type="text"
            v-model="companyName" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="companyAddress_input">Firma vejnavn og nr.</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="companyAddress_input" type="text"
            v-model="companyAddress" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="companyCity_input">Firma by</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="companyCity_input" type="text"
            v-model="companyCity" :disabled="isLoading || !edit">
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="companyZIP_input">Firma postnummer</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="companyZIP_input" type="text"
            v-model="companyZIP" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="cvrNumber_input">CVR nr.</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="cvrNumber_input" type="text"
            v-model="cvrNumber" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="companyPhone_input">Firma mobil</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="companyPhone_input" type="text"
            v-model="companyPhone" :disabled="isLoading || !edit">
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="companyEmail_input">Firma e-mail</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="companyEmail_input" type="text"
            v-model="companyEmail" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="title_input">Titel</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="title_input" type="text"
            v-model="title" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="occupation_input">Beskæftigelse</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="occupation_input" type="text"
            v-model="occupation" :disabled="isLoading || !edit">
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="workTasks_input">Arbejdsopgaver</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="workTasks_input" type="text"
            v-model="workTasks" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="elTeknikDelivery_input">El-teknik levering</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="elTeknikDelivery_input" type="text"
            v-model="elTeknikDelivery" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="eanNumber_input">EAN nr.</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="eanNumber_input" type="text"
            v-model="eanNumber" :disabled="isLoading || !edit">
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="invoice_input">Fakturering</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="invoice_input" type="text"
            v-model="invoice" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="invoiceEmail_input">Faktura E-mail</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="invoiceEmail_input" type="text"
            v-model="invoiceEmail" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="attInvoice_input">ATT Fakutra</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="attInvoice_input" type="text"
            v-model="attInvoice" :disabled="isLoading || !edit">
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="oldMemberNumber_input">Gammelt Medlemsnummer</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="oldMemberNumber_input" type="text"
            v-model="oldMemberNumber" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="helpToStudents_input">Hjælp til studerende</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="helpToStudents_input" type="text"
            v-model="helpToStudents" :disabled="isLoading || !edit">
        </div>
        <div class="w-[30%]">
          <label for="mentor_input">Mentor</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="mentor_input" type="text"
            v-model="mentor" :disabled="isLoading || !edit">
        </div>
      </div>

      <div class="flex justify-between pb-4">
        <div class="w-[30%]">
          <label for="expectedEndDateOfBeingStudent_input">Sidste dato som studerende</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="expectedEndDateOfBeingStudent_input"
            type="text" v-model="expectedEndDateOfBeingStudent" :disabled="isLoading || !edit">
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
              :class="{ 'cursor-pointer': !isLoading, 'cursor-not-allowed': isLoading, 'hover:bg-gray-400': !isLoading }"
              class="inline-flex justify-center rounded-md border border-transparent bg-gray-300 px-4 py-2 font-medium focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 text-gray-900 focus-visible:ring-offset-2"
              @click="closeModal"
            >
              <span>Fortryd</span>
            </button>
          </div>
          <div class="w-80 pt-2">
            <span class="text-green-600">Opdateret: {{ localDateTime }}</span>
          </div>
        </div>
        <div class="pt-4 w-full flex justify-end">
          <button
            type="button"
            :disabled="isLoading"
            :class="{ 'cursor-pointer': !isLoading, 'cursor-not-allowed': isLoading, 'hover:bg-gray-400': !isLoading }"
            class="inline-flex justify-center rounded-md border border-transparent bg-gray-300 px-4 py-2 font-medium focus:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 text-gray-900 focus-visible:ring-offset-2"
            @click="closeModal"
          >
            <span>Luk</span>
          </button>
        </div>
      </div>

    </form>
  </BaseModal>
</template>

<style lang="css" scoped>
.inputField {
  width: 100%;
  background-color: var(--color-gray-800);
  border-style: var(--tw-border-style);
  border-width: 0px;
  border-radius: var(--radius-xl);
  padding: calc(var(--spacing) * 2);
}
@property --tw-border-style {
  syntax: "*";
  inherits: false;
  initial-value: solid;
}
</style>
