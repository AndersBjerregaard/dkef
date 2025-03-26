<script setup lang="ts">
import apiservice from '@/services/apiservice';
import MemberComponent from './MemberComponent.vue';
import { ref } from 'vue';

const items = ref([{ Name: 'John' }, { Name: 'Anders' }, { Name: 'Henrik' }])

apiservice.get('/contacts')
  .then(function (response) {
    console.info(response);
  })
  .catch(function (error) {
    console.error(error);
  })

</script>

<template>
  <div class="flex justify-center items-center">
    <div class="py-8">
      <h1 class="text-4xl py-8">Members List</h1>
    </div>
    <ul>
      <MemberComponent v-for="(item, index) in items" :key="index" :contact="item" />
    </ul>
  </div>
</template>

<style lang="css" scoped></style>
