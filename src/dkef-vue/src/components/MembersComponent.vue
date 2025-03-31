<script setup lang="ts">
import apiservice from '@/services/apiservice';
import MemberComponent from './MemberComponent.vue';
import MemberHeaderComponent from './MemberHeaderComponent.vue';

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

let items: Contact[] = [];

apiservice.get('/contacts')
  .then(function (response) {
    items = response.data.collection;
    console.info(items);
  })
  .catch(function (error) {
    console.error(error);
  })

</script>

<template>
  <div class="justify-center items-center text-center members-list">
    <div >
      <div class="py-8">
        <h1 class="text-4xl py-8">Members List</h1>
      </div>
      <div class="py-4">
        <div class="flex w-full justify-between border-2 border-gray-800">
          <MemberHeaderComponent :header="'Navn'" />
          <MemberHeaderComponent :header="'Email'" />
          <MemberHeaderComponent :header="'Telefon Nr.'" />
          <MemberHeaderComponent :header="'Primær Sektion'" />
          <MemberHeaderComponent :header="'Addresse'" />
        </div>
        <div class="w-full justify-between border-2 border-gray-800">
          <MemberComponent v-for="(item, index) in items" :key="index" :contact="item" />
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="css" scoped>
.members-list {
  width: 90vw;
}
</style>
