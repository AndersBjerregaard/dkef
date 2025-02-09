<script setup lang="ts">
import { ref } from 'vue';
import EventComponent from './EventComponent.vue';
const items = ref([1, 2, 3]);
</script>

<template>
  <div class="py-24">
    <div class="flex justify-center items-center">
      <h1 class="text-4xl">Arrangementer og Nyheder</h1>
    </div>
    <div class="flex justify-center items-center py-12 gap-x-8">
      <button
        class="flex justify-center rounded bg-gray-600 h-10 sm:h-12 py-2 w-24 sm:w-32 cursor-pointer hover:bg-gray-800 sm:text-lg">Alle</button>
      <button
        class="flex justify-center rounded bg-gray-600 h-10 sm:h-12 py-2 w-24 sm:w-32 cursor-pointer hover:bg-gray-800 sm:text-lg">Nyheder</button>
      <button
        class="flex justify-center rounded bg-gray-600 h-10 sm:h-12 py-2 w-24 sm:w-36 cursor-pointer hover:bg-gray-800 sm:text-lg">Arrangementer</button>
      <button
        class="flex justify-center rounded bg-gray-600 h-10 sm:h-12 py-2 w-24 sm:w-48 cursor-pointer hover:bg-gray-800 sm:text-lg">Generalforsamlinger</button>
    </div>
    <div class="flex justify-center items-center gap-x-8">
      <div v-for="x in items" :key="x">
        <EventComponent />
      </div>
    </div>
  </div>
</template>
