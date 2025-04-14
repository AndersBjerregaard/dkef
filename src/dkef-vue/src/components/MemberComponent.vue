<script setup lang="ts">
import { type Contact } from '@/types/members'

const props = defineProps<{ contact: Contact, index: number }>();

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
