<script setup lang="ts">
import apiservice from '@/services/apiservice';
import { Sort } from '@/types/members'
import MemberComponent from './MemberComponent.vue';
import MemberHeaderComponent from './MemberHeaderComponent.vue';
import { onMounted, ref } from 'vue';

const items = ref([]);

function fetchItems() {
  apiservice.get('/contacts')
    .then(function (response) {
      items.value = response.data.collection;
      console.info(items);
    })
    .catch(function (error) {
      console.error(error);
    })
}

onMounted(
  fetchItems
);

// TODO: Hook into sort clicked event from MemberHeaderComponent

</script>

<template>
  <div class="pb-20 justify-center items-center text-center members-list">
    <div >
      <div class="py-8">
        <h1 class="text-4xl py-8">Members List</h1>
      </div>
      <div class="py-4">
        <div class="flex w-full justify-between border-2 border-gray-800">
          <MemberHeaderComponent :header="'Navn'" :sort="Sort.None" />
          <MemberHeaderComponent :header="'Email'" :sort="Sort.None" />
          <MemberHeaderComponent :header="'Telefon Nr.'" :sort="Sort.None" />
          <MemberHeaderComponent :header="'Primær Sektion'" :sort="Sort.None" />
          <MemberHeaderComponent :header="'Addresse'" :sort="Sort.None" />
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
