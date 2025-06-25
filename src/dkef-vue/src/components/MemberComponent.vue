<script setup lang="ts">
import { type Contact } from '@/types/members'

const props = defineProps<{ contact: Contact, index: number }>();

const fields = [
  props.contact?.firstName,
  props.contact?.email,
  props.contact?.privatePhone,
  props.contact?.primarySection,
  props.contact?.privateAddress
];

const handleFieldClick = (fieldValue: string | undefined, fieldIndex: number) => {
  console.info('Field clicked: ', fieldValue);
  console.info('Index clicked: ', fieldIndex);
};

</script>

<template>
  <div class="border-x-2 border-gray-900 w-full justify-between flex"
    :class="{ 'bg-black': index % 2 == 0, 'bg-gray-800': index % 2 == 1 }">
    <div v-for="(field, i) in fields"
      :key="i"
      class="border-2 border-gray-900 h-10 py-1 px-1 flex-1 min-w-0 flex justify-center"
      @click="handleFieldClick(field, i)">
      <span class="truncate">{{ field }}</span>
    </div>
  </div>
</template>

<style lang="css" scoped></style>
