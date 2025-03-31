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

const props = defineProps<{ contact?: Contact }>();

console.info(props.contact);

</script>

<template>
  <div class="border-x-2 border-gray-800 w-full py-1">
    <p>{{ props.contact?.firstName }}</p>
  </div>
</template>

<style lang="css" scoped></style>
