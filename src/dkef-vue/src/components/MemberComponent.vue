<script setup lang="ts">
import { type Contact } from '@/types/members'
import { computed, ref } from 'vue'
import BaseModal from '@/components/BaseModal.vue';
import LoadingButton from '@/components/LoadingButton.vue'

// Modal state
const isOpen = ref(false);
const isLoading = ref(false);

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
let attInvoice = props.contact?.attInvoice; // ATT Faktura
let companyAddress = props.contact?.companyAddress; // Firma vejnavn og nr.
let companyCity = props.contact?.companyCity; // Firma by
let companyZIP = props.contact?.companyZIP; // Firma postnummer
let cvrNumber = props.contact?.cvrNumber; // CVR nr.
let companyPhone = props.contact?.companyPhone; // Firma mobil
let companyEmail = props.contact?.companyEmail; // Firma e-mail

async function editMember() {
  console.info('Form submitted!');
}

async function authorize() {
  isLoading.value = true;

  try {
    // Simulate
    await new Promise(resolve => setTimeout(resolve, 2000));
    edit.value = true;
  } catch (error) {

  } finally {
    isLoading.value = false;
  }
}

const edit = ref(false);
const hasAccess = ref(true);

</script>

<template>
  <div class="border-x-2 border-gray-900 w-full justify-between flex hover:bg-gray-200 hover:text-gray-900 cursor-pointer"
    @click="openModal"
    :class="{ 'bg-black': index % 2 == 0, 'bg-gray-800': index % 2 == 1 }"
  >
    <div v-for="(field, i) in fields" :key="i"
      class="border-2 border-gray-900 h-10 py-1 px-1 flex-1 min-w-0 flex justify-center"
    >
      <span class="truncate">{{ field }}</span>
    </div>
  </div>
  <BaseModal
    :is-open="isOpen"
    :title="'Medlem'"
    :is-loading="isLoading"
    @close="closeModal"
  >
    <div class="pb-4" v-if="hasAccess">
      <div v-if="!edit">
        <LoadingButton @loading-button-click="authorize" default-text="Autorisér" loading-text="Autoriserer..." :is-loading="isLoading"/>
      </div>
      <div v-else class="text-lg text-green-600">
        <span>Autoriseret</span>
      </div>
    </div>
    <form @submit.prevent="editMember">
      <div class="flex justify-between">
        <div>
          <label for="companyAddress_input">Firma vejnavn og nr.</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="companyAddress_input" type="text" v-model="companyAddress" :disabled="isLoading || !edit">
        </div>
        <div>
          <label for="companyCity_input">Firma by</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="companyCity_input" type="text" v-model="companyCity" :disabled="isLoading || !edit">
        </div>
        <div>
          <label for="companyZIP_input">Firma postnummer</label>
          <input :class="{ 'cursor-not-allowed': !edit }" class="inputField" id="companyZIP_input" type="text" v-model="companyZIP" :disabled="isLoading || !edit">
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
