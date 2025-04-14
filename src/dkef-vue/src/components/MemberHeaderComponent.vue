<script setup lang="ts">
import { Sort } from '@/types/members';

const props = defineProps<{ header: string, sort: Sort }>()

// TODO: Render header different depending on sort, and create sort click event to emit to parent component

</script>

<template>
  <div class="border-x-2 border-gray-800 w-full py-1">
    <h2 class="text-lg overflow-ellipsis">{{ props.header }}</h2>
  </div>
</template>

<style lang="css" scoped></style>
