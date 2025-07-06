<script setup lang="ts">
import apiservice from '@/services/apiservice'
import urlservice from '@/services/urlservice'
import { type ColumnSortState, type Contact, type ContactCollection, Sort } from '@/types/members'
import MemberComponent from './MemberComponent.vue'
import MemberHeaderComponent from './MemberHeaderComponent.vue'
import { computed, onMounted, onUnmounted, reactive, ref, type ComputedRef, type Ref } from 'vue'
import type { AxiosResponse } from 'axios'

const items: Ref<Contact[]> = ref([]);
const fetchedCount: Ref<number> = ref(0);
const totalCount: Ref<number> = ref(0);
const filterString: Ref<String> = ref('');

const loadingProgress = computed<number>(() => {
  if (totalCount.value === 0) {
    return 0;
  }
  return (fetchedCount.value / totalCount.value) * 100;
});

const sortKey: Ref<string> = ref('none'); // Default sort key
const sortOrder: Ref<number> = ref(1); // 1 for ascending, -1 for descending, 0 for none

const sortedItems: ComputedRef<Contact[]> = computed(() => {

  const sortKeyValue = sortKey.value;
  const sortOrderValue = sortOrder.value;

  if (sortKeyValue === 'none' || sortOrderValue === 0) {
    return items.value;
  }

  const sorted = [...items.value]; // Create shallow copy

  sorted.sort((a: Contact, b: Contact): number => {
    const aValue = matchContactProperty(a, sortKeyValue);
    const bValue = matchContactProperty(b, sortKeyValue);
    // All of the property types of the Contact type are strings.
    // Should this at some point not be true,
    // more data types need to be accounted for in the sorting implementation.
    return aValue.localeCompare(bValue) * sortOrderValue;
  });

  return sorted;
});

const filteredItems = computed(() => {
  const fetchedSortedItems = sortedItems;

  if (!filterString.value || filterString.value.length < 3) {
    return fetchedSortedItems.value;
  }

  const items = [...fetchedSortedItems.value]; // Create shallow copy

  const lowerCaseQuery = filterString.value.toLowerCase();

  const filteredContacts = items.filter(contact => {
    // Iterate over each property of the contact object
    for (const key in contact) {
      // Exclude 'id' property
      if (key === 'id') {
        continue;
      }
      const value = contact[key as keyof Contact]; // Type assertion for key
      // Ensure the value is a string before checking for inclusion
      if (typeof value === 'string') {
        if (value.toLowerCase().includes(lowerCaseQuery)) {
          return true; // Match found in this property
        }
      }
      // Maybe handle different type differently here
    }
    return false; // No match found in any property for this contact
  });

  return filteredContacts
});

/**
 * @param contact The contact instance to find the property of
 * @param propertyName The property to find on the contact instance
 * @returns The property value of the contact instance matched by the propertyName input argument
 * @throws If the input argument value of propertyName cannot be found as a property on the contact type
 */
function matchContactProperty(contact: Contact, propertyName: string): string {
  switch (propertyName) {
    case 'name':
      return contact.firstName;
    case 'email':
      return contact.email;
    case 'phone':
      return contact.privatePhone;
    case 'section':
      return contact.primarySection;
    case 'address':
      return contact.privateAddress;
    default:
      throw `Property with the name of ${propertyName} does not exist on type 'Contact'`;
  }
}

const takeAmount = 25;
let initialFetch = true;
let total = Number.MAX_SAFE_INTEGER;

const abortController: AbortController = new AbortController();

async function fetchItems(skip?: number): Promise<void> {
  if (items.value.length >= total) {
    return;
  }

  if (skip === undefined) {
    skip = 0;
  }

  try {
    const response: AxiosResponse<ContactCollection> = await apiservice
      .get<ContactCollection>(urlservice.getContacts(), {
        params: {
          take: takeAmount,
          skip: skip
        },
        signal: abortController.signal
      })

    // Retrieve response body
    const body = response.data;

    // Only set total at initial fetch, in case the total changes while requesting
    if (initialFetch) {
      total = body.total;
      totalCount.value = total;
      initialFetch = false;
    }

    const collection = body.collection;

    // Return early if no items were fetched
    if (collection.length === 0) {
      return;
    }

    // Make each item reactive
    collection.forEach((item: Contact) => {
      items.value.push(reactive(item));
    });

    fetchedCount.value = items.value.length;

    // Only recursively call if the component is still mounted and total hasn't been reached
    if (items.value.length < total) {
      fetchItems(skip + collection.length);
    }

  } catch (error) {
    console.error(error);
  }
};

onUnmounted(() => {
  abortController.abort();
  console.info('Fetching aborted due to component umount.');
});

onMounted(() => {
  fetchItems();
});

// Describes the sort state of the MemberHeaderComponents rendered altogether
const columnSortStates = ref<ColumnSortState>({
  name: Sort.None,
  email: Sort.None,
  phone: Sort.None,
  section: Sort.None,
  address: Sort.None,
});

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

  sort(headerKey, newSortDirection);
};

function sort(by: string, order: Sort): void {
  switch (order) {
    case Sort.Asc:
      sortOrder.value = 1;
      break;
    case Sort.Desc:
      sortOrder.value = -1;
      break;
    default:
      sortOrder.value = 0;
      break;
  }
  sortKey.value = by;
}
</script>

<template>
  <div class="pb-20 justify-center items-center text-center members-list">
    <div>
      <div class="py-8">
        <h1 class="text-4xl py-8">Alle medlemmer</h1>
      </div>
      <div v-if="loadingProgress < 100" class="pb-4">
        <div class="w-full bg-gray-200 rounded-full h-4 dark:bg-gray-700 mb-4">
          <div class="bg-blue-600 h-4 rounded-full" :style="{ width: loadingProgress + '%' }"></div>
          <p class="text-sm text-gray-500 mt-1">{{ fetchedCount }} / {{ totalCount }} Medlemmer hentet</p>
        </div>
      </div>
      <div class="py-4 flex gap-4">
        <label class="p-1">Søg: </label>
        <input
          type="text"
          class="bg-gray-700 rounded p-1 w-[50%]"
          placeholder="Minimum 3 bogstaver..."
          v-model="filterString"
        >
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
          <MemberComponent v-for="(item, index) in filteredItems" :key="item.id" :contact="item" :index="index" />
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
