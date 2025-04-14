<script setup lang="ts">

interface Contact {
  attInvoice: string,
  companyAddress: string,
  companyCity: string,
  companyEmail: string,
  companyName: string,
  companyPhone: string,
  companyZIP: string,
  createdAt: string,
  cvrNumber: string,
  eanNumber: string,
  elTeknikDelivery: string,
  email: string,
  expectedEndDateOfBeingStudent: string,
  firstName: string,
  helpToStudents: string,
  id: string,
  invoice: string,
  invoiceEmail: string,
  lastName: string,
  mentor: string,
  occupation: string,
  oldMemberNumber: string,
  primarySection: string,
  privateAddress: string,
  privateCity: string,
  privatePhone: string,
  privateZIP: string,
  registrationDate: string,
  secondarySection: string,
  source: string,
  title: string,
  workTasks: string
}

const props = defineProps<{ contact?: Contact, index: number }>();



</script>

<template>
  <div class="border-x-2 border-gray-900 w-full justify-between flex"
    :class="{ 'bg-black': index % 2 == 0, 'bg-gray-800': index % 2 == 1 }">
    <div class="border-2 border-gray-900 h-10 py-1 px-1 flex-1 min-w-0 flex justify-center">
      <span class="truncate">{{ props.contact?.firstName }}</span>
    </div>
    <div class="border-2 border-gray-900 h-10 py-1 px-1 flex-1 min-w-0 flex justify-center">
      <span class="truncate">{{ props.contact?.email }}</span>
    </div>
    <div class="border-2 border-gray-900 h-10 py-1 px-1 flex-1 min-w-0 flex justify-center">
      <span class="truncate">{{ props.contact?.privatePhone }}</span>
    </div>
    <div class="border-2 border-gray-900 h-10 py-1 px-1 flex-1 min-w-0 flex justify-center">
      <span class="truncate">{{ props.contact?.primarySection }}</span>
    </div>
    <div class="border-2 border-gray-900 h-10 py-1 px-1 flex-1 min-w-0 flex justify-center">
      <span class="truncate">{{ props.contact?.privateAddress }}</span>
    </div>
  </div>
</template>

<style lang="css" scoped></style>
