<script setup lang="ts">
import { ref } from 'vue';

const items = ref([{ name: 'Nam' }, { name: 'Num' }])
</script>

<template>
  <div class="flex justify-center items-center">
    <div class="py-8">
      <h1 class="text-4xl py-8">Members List</h1>
    </div>
    <ul>
      <li v-for="(item, index) in items" v-bind:key="index">

      </li>
    </ul>
  </div>
</template>

<style lang="css" scoped></style>
