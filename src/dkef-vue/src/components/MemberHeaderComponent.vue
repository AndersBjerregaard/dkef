<script setup lang="ts">
import { computed, ref } from 'vue';
import { Sort } from '@/types/members';

const props = defineProps<{ header: string }>()

const emit = defineEmits(['update:sort']);

// const currentSortDirection = ref(props.sort);

const currentSortDirection = ref(Sort.None);

const sortArrow = computed(() => {
  if (currentSortDirection.value === Sort.None) {
    return '';
  }
  return currentSortDirection.value === Sort.Asc ? '&#9650;' : '&#9660;'; // Up arrow or down arrow
});

const handleClick = () => {
  let newDirection: Sort;

  if (currentSortDirection.value === Sort.None) {
    // If not currently sorted, start with ascending
    newDirection = Sort.Asc
  } else if (currentSortDirection.value === Sort.Asc) {
    // If currently Ascending, toggle to Descending
    newDirection = Sort.Desc;
  } else {
    // If currently Descending, toggle back to None
    newDirection = Sort.None;
  }

  currentSortDirection.value = newDirection;
  // Emit the new sort direction for *this* header.
  // The parent will be responsible for updating the global stae.
  emit('update:sort', { key: props.header, sort: newDirection });
}
</script>

<template>
  <div class="border-x-2 border-gray-800 w-full py-1 cursor-pointer" @click="handleClick">
    <h2 class="text-lg overflow-ellipsis flex items-center justify-center">
      <span>{{ props.header }}</span>
      <span v-html="sortArrow" class="ml-2"></span>
    </h2>
  </div>
</template>

<style lang="css" scoped></style>
