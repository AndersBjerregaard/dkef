<script setup lang="ts">
import { computed, ref, watch } from 'vue'; // Import watch
import { Sort } from '@/types/members';

const props = defineProps<{
  header: string;
  currentSort: Sort; // New prop to receive the sort state from parent
}>();

const emit = defineEmits(['update:sort']);

// Use a local ref to manage the state internally for click logic,
// but keep it synchronized with the prop.
const internalSortDirection = ref(props.currentSort);

// Watch for changes in the currentSort prop and update the internal ref
watch(() => props.currentSort, (newVal) => {
  internalSortDirection.value = newVal;
});

const sortArrow = computed(() => {
  if (internalSortDirection.value === Sort.None) {
    return '';
  }
  return internalSortDirection.value === Sort.Asc ? '&#9650;' : '&#9660;'; // Up arrow or down arrow
});

const handleClick = () => {
  let newDirection: Sort;

  if (internalSortDirection.value === Sort.None) {
    newDirection = Sort.Asc;
  } else if (internalSortDirection.value === Sort.Asc) {
    newDirection = Sort.Desc;
  } else {
    newDirection = Sort.None;
  }

  // Do not update internalSortDirection directly here based on newDirection.
  // Instead, emit the event, and let the parent update the prop, which
  // will then flow back down and update internalSortDirection via the watcher.
  emit('update:sort', newDirection); // No need for an object if you only pass the new direction
};
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
