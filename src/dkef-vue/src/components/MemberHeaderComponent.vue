<script setup lang="ts">
import { ref, computed } from 'vue';
import { Sort } from '@/types/members';

const props = defineProps<{ header: string, sort: Sort }>()
const emit = defineEmits(['update:sort']);

const currentSortDirection = ref(props.sort);

const sortArrow = computed(() => {
  if (props.sort === Sort.None) {
    return '';
  }
  return currentSortDirection.value === Sort.Asc ? '&#9650;' : '&#9660;'; // Up arrow or down arrow
});

const handleClick = () => {
  let newDirection = Sort.Asc;
  if (props.sort === Sort.None) {
    // Toggle direction if already sorting by this header
    newDirection = currentSortDirection.value === Sort.Asc ? Sort.Desc : Sort.Asc;
  }
  currentSortDirection.value = newDirection; // Update internal state

  // Emit event to parent with new information
  emit('update:sort', { key: props.header, sort: newDirection });
}
</script>

<template>
  <div class="border-x-2 border-gray-800 w-full py-1 cursor-pointer" @click="handleClick">
    <h2 class="text-lg overflow-ellipsis flex items-center justify-between">
      <span>{{ props.header }}</span>
      <span v-html="sortArrow" class="ml-2"></span>
    </h2>
  </div>
</template>

<style lang="css" scoped></style>
