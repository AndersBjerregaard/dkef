<script setup lang="ts">
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import { type ColumnSortState, Sort } from '@/types/members'
import MemberComponent from './MemberComponent.vue'
import MemberHeaderComponent from './MemberHeaderComponent.vue'
import { onMounted, reactive, ref } from 'vue'

const items = ref([])

function fetchItems() {
  apiservice
    .get(urlservice.getContacts())
    .then(function (response) {
      // Make each item reactive
      items.value = response.data.collection.map((item: any) => reactive(item))
    })
    .catch(function (error) {
      console.error(error)
    })
}

onMounted(fetchItems)

const columnSortStates = ref<ColumnSortState>({
  name: Sort.None,
  email: Sort.None,
  phone: Sort.None,
  section: Sort.None,
  address: Sort.None,
})

// Function to handle update from MemberHeaderComponent
const handleSortUpdate = (headerKey: string, newSortDirection: Sort) => {
  // Reset all other headers to Sort.None
  for (const key in columnSortStates.value) {
    if (key !== headerKey) {
      columnSortStates.value[key] = Sort.None
    }
  }

  // Set the new sort direction for the clicked header
  columnSortStates.value[headerKey] = newSortDirection

  // TODO: Sort
  console.info('items before: ', items.value);
  items.value[0].firstName = 'John';
  console.info('items after: ', items.value);
}
</script>

<template>
  <div class="pb-20 justify-center items-center text-center members-list">
    <div>
      <div class="py-8">
        <h1 class="text-4xl py-8">Members List</h1>
      </div>
      <div class="py-4">
        <div class="flex w-full justify-between border-2 border-gray-800">
          <MemberHeaderComponent :header="'Navn'" :currentSort="columnSortStates.name"
            @update:sort="(newValue: Sort) => handleSortUpdate('name', newValue)" />
          <MemberHeaderComponent :header="'Email'" :currentSort="columnSortStates.email"
            @update:sort="(newValue: Sort) => handleSortUpdate('email', newValue)" />
          <MemberHeaderComponent :header="'Telefon Nr.'" :currentSort="columnSortStates.phone"
            @update:sort="(newValue: Sort) => handleSortUpdate('phone', newValue)" />
          <MemberHeaderComponent :header="'Primær Sektion'" :currentSort="columnSortStates.section"
            @update:sort="(newValue: Sort) => handleSortUpdate('section', newValue)" />
          <MemberHeaderComponent :header="'Addresse'" :currentSort="columnSortStates.address"
            @update:sort="(newValue: Sort) => handleSortUpdate('address', newValue)" />
        </div>
        <div class="w-full justify-between border-2 border-gray-800">
          <MemberComponent v-for="(item, index) in items" :key="index" :contact="item" :index="index" />
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
